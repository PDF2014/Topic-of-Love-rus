using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.romance;

public class BehCheckIfDateHere : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        TolUtil.Debug("Checking if date arrived: "+ pActor.getName());
        if (pActor.beh_actor_target == null)
        {
            TolUtil.Debug(pActor.getName()+": Cancelled from checking because actor was null");
            return BehResult.Stop;
        }
        var follower = pActor.beh_actor_target.a;
        if (!follower.isTask("follow_action_date"))
        {
            TolUtil.Debug(pActor.getName()+"'s date has ended!");
            return BehResult.Stop;
        }
        
        if (follower.is_moving)
        {
            return BehResult.StepBack;
        }

        return BehResult.Continue;
    }
}