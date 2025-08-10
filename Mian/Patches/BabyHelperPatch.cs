using EpPathFinding.cs;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BabyHelper))]
public class BabyHelperPatch
{
    public static void TraitsInherit(Actor pActorTarget, params Actor[] parents)
    {
        using (var listPool = new ListPool<ActorTrait>(256 /*0x80*/))
        {
            var totalCount = 0;
            foreach (var parent in parents)
            {
                if (parent == null)
                    continue;
                BabyHelper.addTraitsFromParentToList(parent, listPool, out var counter);
                totalCount += counter;
            }
            for (var index = 0; index < totalCount; ++index)
            {
                var random = listPool.GetRandom<ActorTrait>();
                pActorTarget.addTrait(random.id);
            }
        }
    }
}