using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.romance;
public class BehCheckForDate : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        TolUtil.Debug("Checking to start date: "+ pActor.getName());
        pActor.data.removeFloat("date_happiness");
        if (pActor.beh_actor_target == null)
        {
            TolUtil.Debug(pActor.getName()+": Cancelled from starting date because actor was null");
            return BehResult.Stop;
        }
        var target = pActor.beh_actor_target.a;
        if (pActor.distanceToActorTile(pActor.beh_actor_target.a) > 4f)
        {
            target.clearWait();
            return BehResult.Stop;
        }
        target.beh_actor_target = pActor;
        
        target.setTask("follow_action_date", false, pForceAction: true);
        return forceTask(pActor, "action_date", false, true);
    }
}