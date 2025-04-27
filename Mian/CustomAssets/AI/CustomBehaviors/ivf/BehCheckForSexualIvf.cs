using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.ivf;
public class BehCheckForSexualIvf : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        TOLUtil.Debug("Checking sexual ivf for "+pActor.getName());

        if (pActor.beh_actor_target == null || pActor.beh_building_target == null)
        {
            TOLUtil.Debug(pActor.getName()+": Cancelled from checking for sexual ivf target because actor was null");
            return BehResult.Stop;
        }

        var sexActor = pActor.beh_actor_target.a;
        if (sexActor.isTask("go_and_wait_sexual_ivf") && sexActor.beh_building_target == pActor.beh_building_target && sexActor.ai.action_index > 3)
        {
            pActor.stayInBuilding(pActor.beh_building_target);
            sexActor.stayInBuilding(sexActor.beh_building_target);
            sexActor.setTask("wait_sexual_ivf", false, pForceAction: true);
                
            return forceTask(pActor, "action_sexual_ivf", false, true);
        }
        return !sexActor.isTask("go_and_wait_sexual_ivf") ? BehResult.Stop : BehResult.Continue;
    }
}