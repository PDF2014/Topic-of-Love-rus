using System.Collections;
using ai.behaviours;
using Topic_of_Love.Mian.CustomAssets.Traits;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.sex;
    public class BehFindMatchingPreference : BehaviourActionActor
    {
        public override BehResult execute(Actor pActor)
        {
            var lover = pActor.lover;
            var withLover = false;
            Actor closestActor = null;

            Util.Debug(pActor.getName() + " is requesting casual sex");
            
            if (lover != null)
            {
                if (lover.isSameIslandAs(pActor)
                    && ((QueerTraits.PreferenceMatches(pActor, lover, true) && QueerTraits.PreferenceMatches(lover, pActor, true)) 
                        || Randy.randomChance(0.5f))
                    && Util.WillDoIntimacy(lover, "casual"))
                {
                    closestActor = lover;
                    withLover = true;
                }
            }
            
            if (!Util.WillDoIntimacy(pActor, "casual", withLover, isInit: true))
            {
                Util.Debug("They decided that they will not do it.");
                return BehResult.Stop;
            }
            
            if (closestActor == null)
            {
                closestActor = GetClosestPossibleMatchingActor(pActor);
                if (closestActor == null)
                {
                    Util.Debug("No actor found");
                    return BehResult.Stop;   
                }
            }
            
            Util.Debug(pActor.getName() + " is going to do casual sex with: "+closestActor.getName() + ". They are lovers: "+(pActor.lover==closestActor));
            pActor.beh_actor_target = closestActor;
            
            pActor.data.set("sex_reason", "casual");
            closestActor.data.set("sex_reason", "casual");
            return BehResult.Continue;
        }
        
        private Actor GetClosestPossibleMatchingActor(Actor pActor)
        {
            var bestFriendIsValid = pActor.hasBestFriend() 
                                    && pActor.isSameIslandAs(pActor.getBestFriend()) 
                                    && pActor.distanceToActorTile(pActor.getBestFriend()) < 50f; 
            
            using (ListPool<Actor> pCollection = new ListPool<Actor>(4))
            {
                if (bestFriendIsValid && QueerTraits.PreferenceMatches(pActor, pActor.getBestFriend(), true) &&
                    QueerTraits.PreferenceMatches(pActor.getBestFriend(), pActor, true)
                    && Util.WillDoIntimacy(pActor.getBestFriend(), "casual", pActor.lover == pActor.getBestFriend()))
                    return pActor.getBestFriend();
                
                var pRandom = Randy.randomBool();
                var pChunkRadius = Randy.randomInt(2, 3);
                var num = Randy.randomInt(5, 10);
                foreach (Actor pTarget in Finder.getUnitsFromChunk(pActor.current_tile, pChunkRadius, pRandom: pRandom))
                {
                    if (pTarget != pActor 
                        && pActor.isSameIslandAs(pTarget) 
                        && QueerTraits.PreferenceMatches(pActor, pTarget, true) 
                        && QueerTraits.PreferenceMatches(pTarget, pActor, true)
                        && Util.WillDoIntimacy(pTarget, "casual", pTarget.lover == pActor)
                        && pTarget.last_decision_id != "sexual_reproduction_try")
                    {
                        pCollection.Add(pTarget);
                        if (((ICollection) pCollection).Count >= num)
                            break;
                    }
                }
                
                return Toolbox.getClosestActor(pCollection, pActor.current_tile);
            }
        }
    }