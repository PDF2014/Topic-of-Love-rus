using System;
using ai.behaviours;
using Topic_of_Love.Mian.CustomAssets.Custom;
using Topic_of_Love.Mian.CustomAssets.Traits;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.other;

// includes lover and other non-lovers (can be considered cheating)
//TODO: look into reworking distance and waits
public class BehFindAPartner : BehaviourActionActor
{
    private readonly float _distance;
    private readonly PartnerType _partnerType;
    private readonly bool _mustMatchPreference;
    private readonly bool _mustBeReproduceable;
    private readonly SexType _sexReason;
    private readonly Func<Actor, Actor, bool> _customValidity;
    private readonly Func<Actor, bool> _customCheck;
    public BehFindAPartner(
        PartnerType partnerType=PartnerType.Lover,
        bool mustMatchPreference=false, 
        bool mustBeReproduceable=false,
        SexType sexReason=SexType.None, 
        float distance=100f,
        Func<Actor, bool> customCheck=null,
        Func<Actor, Actor, bool> customValidity=null)
    {
        _distance = distance;
        _partnerType = partnerType;
        _mustMatchPreference = mustMatchPreference;
        _mustBeReproduceable = mustBeReproduceable;
        _sexReason = sexReason;
        _customValidity = customValidity;
        _customCheck = customCheck;
    }
    public override BehResult execute(Actor pActor)
    {
        if (_customCheck != null && !_customCheck(pActor))
            return BehResult.Stop;
        
        TolUtil.Debug(pActor.getName() + " is attempting to locate lover for romance!");

        Actor target = null;

        if (_partnerType.Equals(PartnerType.Lover) || _partnerType.Equals(PartnerType.BothLoverAndFriend))
        {
            if (pActor.hasLover() && IsTargetValid(pActor, pActor.lover))
                target = pActor.lover;
        } else if (_partnerType.Equals(PartnerType.Friend) || _partnerType.Equals(PartnerType.BothLoverAndFriend))
        {
            if(pActor.hasBestFriend()
               && IsTargetValid(pActor, pActor.getBestFriend()))
                target = pActor.getBestFriend();
        } else if (_partnerType.Equals(PartnerType.Any))
        {
            target = GetClosestPossibleMatchingActor(pActor);
        }

        if (target == null || !pActor.WillDoIntimacy(target, _sexReason, true))
            return BehResult.Stop;

        TolUtil.Debug("Lover found!");
        
        pActor.beh_actor_target = target;
        target.makeWait(_distance / 2);

        pActor.data.set("sex_reason", _sexReason.ToString());
        target.data.set("sex_reason", _sexReason.ToString());
        
        return BehResult.Continue;
    }

    private bool IsForReproduction()
    {
        return _sexReason.Equals(SexType.Reproduction);
    }
    private bool IsTargetValid(Actor pActor, Actor target)
    {
        if (_customValidity != null && !_customValidity(pActor, target))
            return false;
        
        if (!pActor.isOnSameIsland(target) || target.isLying() || pActor.distanceToActorTile(target) > _distance)
            return false;
        if (_mustBeReproduceable && (!BabyHelper.canMakeBabies(target) || !pActor.CanReproduce(target)))
            return false;
        var isSexual = _sexReason != SexType.None;
        if (isSexual)
        {
            if (target.last_decision_id == "sexual_reproduction_try")
                return false;
            if(IsForReproduction())
                return (pActor.isSameSubspecies(target.subspecies) 
                       || (target.isSapient() && pActor.isSapient() 
                                               && LikesManager.LikeMatches(target, pActor, true)))
                       && target.WillDoIntimacy(pActor, _sexReason);

            return target.WillDoIntimacy(pActor, _sexReason) &&
                   ((_mustMatchPreference && LikesManager.LikeMatches(pActor, target, true)) ||
                    !_mustMatchPreference);
        }
        
        if (_mustMatchPreference && !LikesManager.LikeMatches(pActor, target, false))
        {
            return false;
        }

        return true;
    }
    private Actor GetClosestPossibleMatchingActor(Actor pActor)
    {
        var chunkRadius = IsForReproduction() ? 2 : 1;
        var isRandom = !IsForReproduction();

        Actor toReturn = null;
        
        foreach (var pTarget in Finder.getUnitsFromChunk(pActor.current_tile, chunkRadius, pRandom: isRandom))
        {
            if (pTarget != pActor && IsTargetValid(pActor, pTarget))
            {
                toReturn = pActor;
                break;
            }
        }

        return toReturn;
        
    }
}