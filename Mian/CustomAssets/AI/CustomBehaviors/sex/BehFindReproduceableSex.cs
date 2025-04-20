using ai.behaviours;
using Topic_of_Love.Mian.CustomAssets.Traits;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.sex;
public class BehFindReproduceableSex : BehaviourActionActor
    {
        public override BehResult execute(Actor pActor)
        {
            var lover = pActor.lover;
            Actor closestActor = null;
            var withLover = false;
            Util.Debug(pActor.getName() + " is requesting reproduction");

            if (lover != null)
            {
                if (lover.isSameIslandAs(pActor)
                    && Util.WillDoIntimacy(lover, "reproduction") 
                    && Util.CanReproduce(pActor, lover)
                    && Util.CanMakeBabies(lover))
                {
                    closestActor = lover;
                    withLover = true;
                }

                if (Util.CanReproduce(pActor, lover) && !withLover)
                    return BehResult.Stop;
            }

            if (!Util.WillDoIntimacy(pActor, "reproduction", withLover, isInit: true))
            {
                Util.Debug("They decided that they will not do it.");
                return BehResult.Stop;
            }
            
            // try to do it with lover first
            if (closestActor == null)
            {
                closestActor = GetClosestPossibleMatchingActor(pActor);
                if (closestActor == null)
                    return BehResult.Stop;   
            }
            Util.Debug(pActor.getName()+ " is going to reproduce with: "+closestActor.getName()+". They are lovers: "+(pActor.lover==closestActor));
            pActor.beh_actor_target = closestActor;
            
            pActor.data.set("sex_reason", "reproduction");
            closestActor.data.set("sex_reason", "reproduction");
            
            Util.Debug($"\nAble to make a baby for task?\n{pActor.getName()}: "+(Util.CanMakeBabies(pActor)+$"\n${closestActor.getName()}: "+(Util.CanMakeBabies(closestActor))));
            
            return BehResult.Continue;
        }
        
        private Actor GetClosestPossibleMatchingActor(Actor pActor)
        {
            using (ListPool<Actor> pCollection = new ListPool<Actor>(5))
            {
                foreach (var pTarget in Finder.getUnitsFromChunk(pActor.current_tile, 4))
                {
                    if (pTarget != pActor 
                        && Util.CanMakeBabies(pTarget)
                        && pActor.isSameIslandAs(pTarget) 
                        && Util.CanReproduce(pActor, pTarget) 
                        && pTarget.isAdult()
                        && Util.WillDoIntimacy(pTarget, "reproduction", pTarget.lover == pActor)
                        && (pActor.isSameSubspecies(pTarget.subspecies) 
                            || (pTarget.isSapient() && pActor.isSapient() 
                                                      && QueerTraits.PreferenceMatches(pTarget, pActor, true))))
                    {
                        pCollection.Add(pTarget);
                    }
                }

                return Toolbox.getClosestActor(pCollection, pActor.current_tile);
            }
        }
    }