using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.sex;
public class BehCheckForSexTarget : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        if (pActor.beh_actor_target == null)
        {
            TolUtil.Debug(pActor.getName()+": Cancelled from checking for sex target because actor was null");
            return BehResult.Stop;
        }
        TolUtil.Debug(pActor.getName() + " is checking for sex target!");

        var sexActor = pActor.beh_actor_target.a;
            
        if (sexActor.isTask("have_sex_go") && sexActor.ai.action_index > 3 && (sexActor.beh_building_target == null || pActor.beh_building_target == null))
        {
            TolUtil.Debug(pActor.getName() + " is initating outside sex!");
            var result = forceTask(pActor, "sexual_reproduction_outside", false, true);
            sexActor.cancelAllBeh();
            sexActor.makeWait(6f);
            return result;
        }  
            
        if (sexActor.isTask("have_sex_go") && sexActor.beh_building_target == pActor.beh_building_target && sexActor.ai.action_index > 3)
        {
            TolUtil.Debug(pActor.getName() + " is initiating inside sex!");
            pActor.stayInBuilding(pActor.beh_building_target);
            sexActor.stayInBuilding(sexActor.beh_building_target);
            sexActor.setTask("sexual_reproduction_civ_wait", false, pForceAction: true);
                
            return forceTask(pActor, "sexual_reproduction_civ_action", false, true);
        }
        return !sexActor.isTask("have_sex_go") ? BehResult.Stop : BehResult.Continue;
    }
}