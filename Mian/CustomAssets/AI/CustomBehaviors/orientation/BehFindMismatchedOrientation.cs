using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ai.behaviours;
using Topic_of_Love.Mian.CustomAssets.Custom;
using Topic_of_Love.Mian.CustomAssets.Traits;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.orientation;
public class BehFindMismatchedOrientation : BehaviourActionActor
    {
        public override BehResult execute(Actor pActor)
        {
            Actor closestActorWithMismatchedOrientation = GetClosestActorMismatchOrientation(pActor);
            if (closestActorWithMismatchedOrientation == null)
                return BehResult.Stop;
            pActor.beh_actor_target = closestActorWithMismatchedOrientation;
            return BehResult.Continue;
        }
        private static Actor GetClosestActorMismatchOrientation(Actor pActor)
        {
            using (ListPool<Actor> pCollection = new ListPool<Actor>(4))
            {
                var unfitPreferences = Orientation.RegisteredOrientations.Values.Where(orientation =>
                {
                    if (pActor.hasCultureTrait("homophobic"))
                        return orientation.IsHomo;
                    if (pActor.hasCultureTrait("heterophobic"))
                        return orientation.IsHetero;
                    return false;
                });

                var sexualPreference = Orientation.GetOrientation(pActor, true);
                var romanticPreference = Orientation.GetOrientation(pActor, false);

                if (unfitPreferences.Contains(sexualPreference) ||
                    unfitPreferences.Contains(romanticPreference)) return null;
                // don't insult someone else
                
                var pRandom = Randy.randomBool();
                var pChunkRadius = Randy.randomInt(1, 2);
                var num = Randy.randomInt(1, 4);
                foreach (Actor pTarget in Finder.getUnitsFromChunk(pActor.current_tile, pChunkRadius, pRandom: pRandom))
                {
                    if (pTarget != pActor && pActor.isSameIslandAs(pTarget))
                    { 
                        sexualPreference = Orientation.GetOrientation(pTarget, true);
                        romanticPreference = Orientation.GetOrientation(pTarget, false);

                        if (unfitPreferences.Contains(romanticPreference) || unfitPreferences.Contains(sexualPreference))
                        {
                            pCollection.Add(pTarget);
                        }
                        if (((ICollection) pCollection).Count >= num)
                            break;
                    }
                }
                
                return Toolbox.getClosestActor(pCollection, pActor.current_tile);
            }
        }
    }