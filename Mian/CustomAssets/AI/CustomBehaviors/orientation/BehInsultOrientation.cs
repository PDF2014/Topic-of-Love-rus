using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.orientation;
public class BehInsultOrientation : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        var target = pActor.beh_actor_target?.a;
        if (target == null || Toolbox.DistTile(target.current_tile, pActor.current_tile) > 4.0)
            return BehResult.Stop;
        target.changeHappiness("insulted_for_orientation");

        if (Randy.randomChance(0.5f))
        {
            pActor.addAggro(target);
            pActor.startFightingWith(target);
        }
        else if (Randy.randomChance(0.6f))
        {
            target.addStatusEffect("crying");
        } else if (Randy.randomChance(0.8f))
        {
            target.addAggro(pActor);
            target.startFightingWith(pActor);
        }

        return BehResult.Stop;
    }
}