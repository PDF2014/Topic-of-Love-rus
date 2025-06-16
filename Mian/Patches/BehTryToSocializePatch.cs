using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ai;
using ai.behaviours;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BehTryToSocialize))]
public class BehTryToSocializePatch
{

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BehTryToSocialize.execute))]
    static IEnumerable<CodeInstruction> SocializeTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codeMatcher = new CodeMatcher(instructions, generator);

        codeMatcher = codeMatcher.ThrowIfNotMatchForward("Could not find canFallInLoveWith call!", CodeMatch.Calls(() =>
            default(Actor).canFallInLoveWith(default)))
            .Advance(-2) // go back to first instruction
            .RemoveInstructions(7) // remove the whole thingy ma jig
            .ThrowIfNotMatchForward("Could not find TelepathicLink call!", CodeMatch.Calls(() => 
                default(Actor).hasTelepathicLink()))
            .Advance(-1); // moves backwards
        
        // we are inserting our code after the resetSocialize methods

        var labelElse = generator.DefineLabel();
        var labelContinue = generator.DefineLabel();
        
        codeMatcher.Insert(
            // if (TolUtil.IsOrientationSystemEnabledFor(pActor))
            new CodeInstruction(OpCodes.Ldarg_1), // loads actor from argument 1
            CodeInstruction.Call(typeof(TolUtil), nameof(TolUtil.IsOrientationSystemEnabledFor)), // calls TolUtil.IsOrientationSystemEnabledFor,
            new CodeInstruction(OpCodes.Brfalse_S, labelElse), // go to else block
            
            // if(TolUtil.SocializedLoveCheck(__instance, pActor, randomActorAround))
            new CodeInstruction(OpCodes.Ldarg_0), // loads instance from argument 0
            new CodeInstruction(OpCodes.Ldarg_1), // loads actor from argument 1
            new CodeInstruction(OpCodes.Ldloc_0), // gets stored randomActorAround
            CodeInstruction.Call(typeof(TolUtil), nameof(TolUtil.SocializedLoveCheck)), // calls SocializedLoveCheck
            
            new CodeInstruction(OpCodes.Brfalse_S, labelContinue), // skips the rest of the code and goes back onto the original code if SocializedLoveCheck is false
            
            // if true..
            // return BehResult.Continue;
            new CodeInstruction(OpCodes.Ldc_I4_0),
            new CodeInstruction(OpCodes.Ret),
            
            // continue
            new CodeInstruction(OpCodes.Nop).WithLabels(labelElse), // no operation, just making sure we branch
            new CodeInstruction(OpCodes.Ldarg_1), // load actor
            new CodeInstruction(OpCodes.Ldloc_0), // load randomActorAround
            CodeInstruction.Call(typeof(ActorTool), nameof(ActorTool.checkFallInLove)), // calls checkFallInLove
            
            // continue:
            new CodeInstruction(OpCodes.Nop).WithLabels(labelContinue) // moving on!
        );
        
        return codeMatcher.InstructionEnumeration();
    }
    
    // [HarmonyPrefix]
    // [HarmonyPatch(nameof(BehTryToSocialize.execute))]
    // public static bool SocializePatch(BehTryToSocialize __instance, Actor pActor, ref BehResult __result)
    // {
    //     pActor.resetSocialize();
    //     Actor randomActorAround = __instance.getRandomActorAround(pActor);
    //     if (randomActorAround == null)
    //     {
    //         __result = BehResult.Stop;
    //         return false;
    //     }
    //
    //     pActor.beh_actor_target = randomActorAround;
    //
    //     if (TolUtil.IsOrientationSystemEnabledFor(pActor))
    //     {
    //         if (TolUtil.SocializedLoveCheck(__instance, pActor, randomActorAround))
    //         {
    //             return false;
    //         }  
    //     }
    //     else
    //     {
    //         ActorTool.checkFallInLove(pActor, randomActorAround);
    //     }
    //     pActor.resetSocialize();
    //     randomActorAround.resetSocialize();
    //     
    //     __result = pActor.hasTelepathicLink() && randomActorAround.hasTelepathicLink() ? __instance.forceTask(pActor, "socialize_do_talk", false) : __instance.forceTask(pActor, "socialize_go_to_target", false);
    //     return false;
    // }
}