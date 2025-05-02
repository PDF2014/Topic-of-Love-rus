using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.romance;
public class BehKissTarget : BehaviourActionActor
{
    private bool _ignoreDist;
    public BehKissTarget(bool ignoreDist=false)
    {
            _ignoreDist = ignoreDist;
    }
    public override BehResult execute(Actor pActor)
    {
        if (pActor.beh_actor_target == null)
            return BehResult.Stop;
        if (pActor.distanceToActorTile(pActor.beh_actor_target.a) > 4f && !_ignoreDist)
        {
            pActor.beh_actor_target.a.clearWait();
            return BehResult.Stop;
        }
        TolUtil.Debug(pActor.getName() + " is kissing "+pActor.beh_actor_target.a.getName());

        pActor.makeWait(1.5f);
        pActor.beh_actor_target.a.makeWait(1.5f);

        TolUtil.ActorsInteracted(pActor, pActor.beh_actor_target.a);
        pActor.addStatusEffect("just_kissed");
        pActor.beh_actor_target.addStatusEffect("just_kissed");
            
        EffectsLibrary.spawnAt("fx_hearts", pActor.current_position, pActor.current_scale.y);
        EffectsLibrary.spawnAt("fx_hearts", pActor.beh_actor_target.current_position, pActor.beh_actor_target.current_scale.y);

        return BehResult.Continue;
    }
}