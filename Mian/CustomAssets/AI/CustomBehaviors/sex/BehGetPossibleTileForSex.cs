using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.sex;
public class BehGetPossibleTileForSex : BehaviourActionActor
    {
        private bool _mustBePrivate;

        public BehGetPossibleTileForSex(bool mustBePrivate=true)
        {
            _mustBePrivate = mustBePrivate;
        }
        private static bool IsPlacePrivateForBreeding(Actor actor, WorldTile tile)
        {
            var num1 = Toolbox.countUnitsInChunk(tile);
            if (!actor.hasCity())
                return actor.asset.animal_breeding_close_units_limit > num1;
            var num2 = actor.city.getPopulationMaximum() * 2 + 10;
            return actor.city.countUnits() < num2;
        }
        public override BehResult execute(Actor pActor)
        {
            if (pActor.beh_actor_target == null)
            {
                TolUtil.Debug(pActor.getName()+": Cancelled because actor was null");

                return BehResult.Stop;
            }

            var homeBuilding = GetHomeBuilding(pActor, pActor.beh_actor_target.a);

            pActor.beh_tile_target = homeBuilding != null ? homeBuilding.current_tile : pActor.beh_actor_target.current_tile;
            if (!IsPlacePrivateForBreeding(pActor, pActor.beh_tile_target) && _mustBePrivate)
            {
                TolUtil.Debug("Cancelled because of lack of privacy");
                return BehResult.Stop;
            }
            
            var sexActor = pActor.beh_actor_target.a;
            
            sexActor.clearBeh();
            sexActor.beh_actor_target = pActor;
            sexActor.beh_tile_target = pActor.beh_tile_target;
            if (homeBuilding != null)
            {
                sexActor.beh_building_target = homeBuilding;
                pActor.beh_building_target = homeBuilding;
            }
            sexActor.setTask("have_sex_go", pCleanJob: true, pClean: false, pForceAction: true);
            return forceTaskImmediate(pActor, "have_sex_go", pClean: false, pForceAction: true);
        }

        private static Building GetHomeBuilding(Actor pActor1, Actor pActor2)
        {
            if (pActor1.hasHouse())
                return pActor1.getHomeBuilding();
            return pActor2.hasHouse() ? pActor2.getHomeBuilding() : null;
        }
    }