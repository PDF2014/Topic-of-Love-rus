using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.ivf;
public class BehTrySexualIvf : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        TolUtil.Debug("Trying sexual ivf for "+pActor.getName());

        if (!pActor.hasHouse() || pActor.hasStatus("pregnant"))
            return BehResult.Stop;
        var home = pActor.getHomeBuilding();
            
        if (pActor.beh_actor_target == null)
            return BehResult.Stop;
        var target = pActor.beh_actor_target.a;
        pActor.beh_building_target = home;
        target.beh_actor_target = pActor;
        target.beh_building_target = home;
            
        TolUtil.Debug("Starting sexual ivf tasks for "+pActor.getName()+" and "+target.getName());

        target.setTask("go_and_wait_sexual_ivf", pCleanJob: true, pClean:false, pForceAction:true);
        target.timer_action = 0.0f;
        return forceTask(pActor, "go_sexual_ivf", pClean: false);
    }
}
