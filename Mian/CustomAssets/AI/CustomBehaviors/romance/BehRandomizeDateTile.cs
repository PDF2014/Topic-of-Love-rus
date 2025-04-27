using System.Collections.Generic;
using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.romance;

public class BehRandomizeDateTile : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        TOLUtil.Debug("Checking for date before continuing to randomize tile: "+ pActor.getName());
        if (pActor.beh_actor_target == null)
        {
            TOLUtil.Debug(pActor.getName()+": Cancelled from continuing date because actor was null");
            return BehResult.Stop;
        }
        var follower = pActor.beh_actor_target.a;

        if (!follower.isTask("follow_action_date"))
        {
            TOLUtil.Debug(pActor.getName()+"'s date has ended!");
            return BehResult.Stop;
        }
        
        var region = pActor.current_tile.region;
        if (Randy.randomChance(0.35f) && region.tiles.Count > 0)
        {
            pActor.beh_tile_target = region.tiles.GetRandom();
            return BehResult.Continue;
        }

        Building building1 = null;
        
        var buildings = new List<Building>();
        foreach (var building2 in Finder.getBuildingsFromChunk(pActor.current_tile, 4, pRandom: true))
        {
            if (building2.asset.city_building && building2.current_tile.isSameIsland(pActor.current_tile) && building2.isCiv())
            {
                buildings.Add(building2);
                break;
            }
        }

        if (buildings.Count > 0)
        {
            building1 = buildings.GetRandom();
        }
        if (building1 == null)
        {
            if (region.tiles.Count <= 0)
                return BehResult.Stop;
            pActor.beh_tile_target = region.tiles.GetRandom();
            return BehResult.Continue;
        }
        pActor.beh_tile_target = building1.current_tile.region.tiles.GetRandom().getTileAroundThisOnSameIsland(pActor.current_tile, true);
        if (pActor.beh_tile_target == null)
            pActor.beh_tile_target = building1.current_tile.region.tiles.GetRandom();
        return BehResult.Continue;        
    }
}