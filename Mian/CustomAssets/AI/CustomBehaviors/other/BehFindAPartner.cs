using System;
using ai.behaviours;
using Topic_of_Love.Mian.CustomAssets.Traits;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.other;

// includes lover and other non-lovers (can be considered cheating)
public class BehFindAPartner : BehaviourActionActor
{
    private readonly float _distance;
    private readonly bool _mustBeLover;
    private readonly bool _mustBeFriend;
    private readonly bool _mustMatchPreference;
    private readonly bool _mustBeReproduceable;
    private readonly string _sexReason;
    private readonly Func<Actor, Actor, bool> _customValidity;
    private readonly Func<Actor, bool> _customCheck;
    public BehFindAPartner(
        bool mustBeLover=true,
        bool mustBeFriend=false, // if mustBeLover is false and lover is not found
        bool mustMatchPreference=false, 
        bool mustBeReproduceable=false, 
        string sexReason=null, 
        float distance=20f,
        Func<Actor, bool> customCheck=null,
        Func<Actor, Actor, bool> customValidity=null)
    {
        _distance = distance;
        _mustBeLover = mustBeLover;
        _mustBeFriend = mustBeFriend;
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
        
        Util.Debug(pActor.getName() + " is attempting to locate lover for romance!");

        Actor target = null;

        if (pActor.hasLover() 
            && IsTargetValid(pActor, pActor.lover))
            target = pActor.lover;

        if (pActor.hasLover() && IsForReproduction() &&
            Util.CanReproduce(pActor, pActor.lover) && target != pActor.lover)
            return BehResult.Stop;
        
        if (target == null && _mustBeLover)
            return BehResult.Stop;
        
        if (!Util.WillDoIntimacy(pActor, _sexReason, target != null, true))
        {
            Util.Debug("They decided that they will not do it.");
            return BehResult.Stop;
        }
        
        if(pActor.hasBestFriend()
           && IsTargetValid(pActor, pActor.getBestFriend()))
            target = pActor.getBestFriend();
        
        if (target == null && _mustBeFriend)
            return BehResult.Stop;

        if (target == null)
        {
            target = GetClosestPossibleMatchingActor(pActor);
            if (target == null)
                return BehResult.Stop;
        }
        
        Util.Debug("Lover found!");
        
        pActor.beh_actor_target = target;
        target.makeWait(_distance / 2);

        if (_sexReason != null)
        {
            pActor.data.set("sex_reason", _sexReason);
            target.data.set("sex_reason", _sexReason);
        }
        
        return BehResult.Continue;
    }

    private bool IsForReproduction()
    {
        return _sexReason != null && _sexReason.Equals("reproduction");
    }
    private bool IsTargetValid(Actor pActor, Actor target)
    {
        if (_customValidity != null && !_customValidity(pActor, target))
            return false;
        
        if (!pActor.isOnSameIsland(target) || target.isLying() || pActor.distanceToActorTile(target) > _distance)
            return false;
        if (_mustBeReproduceable && (!Util.CanMakeBabies(target) || !Util.CanReproduce(pActor, target)))
            return false;
        var isSexual = _sexReason != null;
        if (isSexual)
        {
            if (target.last_decision_id == "sexual_reproduction_try")
                return false;
            if(IsForReproduction())
                return (pActor.isSameSubspecies(target.subspecies) 
                       || (target.isSapient() && pActor.isSapient() 
                                               && QueerTraits.PreferenceMatches(target, pActor, true)))
                       && Util.WillDoIntimacy(target, _sexReason, target.lover == pActor);

            return Util.WillDoIntimacy(target, _sexReason, target.lover == pActor) &&
                   ((_mustMatchPreference && QueerTraits.PreferenceMatches(pActor, target, true)) ||
                    !_mustMatchPreference);
        }
        
        if (_mustMatchPreference && !QueerTraits.PreferenceMatches(pActor, target, false))
        {
            return false;
        }

        return true;
    }
    private Actor GetClosestPossibleMatchingActor(Actor pActor)
    {
        var chunkRadius = IsForReproduction() ? 4 : 2;
        var isRandom = !IsForReproduction();
        
        using (ListPool<Actor> pCollection = new ListPool<Actor>(5))
        {
            foreach (var pTarget in Finder.getUnitsFromChunk(pActor.current_tile, chunkRadius, pRandom: isRandom))
            {
                if (pTarget != pActor && IsTargetValid(pActor, pTarget))
                {
                    pCollection.Add(pTarget);
                }
            }

            return Toolbox.getClosestActor(pCollection, pActor.current_tile);
        }
    }
}