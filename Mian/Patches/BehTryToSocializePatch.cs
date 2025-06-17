using System.Collections.Generic;
using System.Reflection.Emit;
using ai;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BehTryToSocialize))]
public class BehTryToSocializePatch
{

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BehTryToSocialize.execute))]
    static IEnumerable<CodeInstruction> SocializePatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codeMatcher = new CodeMatcher(instructions, generator);

        codeMatcher = codeMatcher
            .MatchStartForward(new CodeMatch(OpCodes.Callvirt,
                AccessTools.Method(typeof(Actor), nameof(Actor.canFallInLoveWith), new[] { typeof(Actor) })))
            .ThrowIfInvalid("Could not find canFallInLoveWith call!")
            .Advance(-2) // go back to first instruction
            .RemoveInstructions(7); // remove the whole thingy ma jig
        codeMatcher.Instruction.labels.Clear(); // remove the label here since we removed the branch
        
        codeMatcher = codeMatcher.MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Actor), nameof(Actor.hasTelepathicLink))))
            .ThrowIfInvalid("Could not find TelepathicLink call!")
            .Advance(-1); // moves backwards
        
        // we are inserting our code after the resetSocialize methods

        var labelElse = generator.DefineLabel();
        var labelContinue = generator.DefineLabel();
        
        codeMatcher = codeMatcher.InsertAndAdvance(
            // if (TolUtil.IsOrientationSystemEnabledFor(pActor))
            CodeInstruction.LoadArgument(1), // loads actor from argument 1
            CodeInstruction.Call(typeof(TolUtil), nameof(TolUtil.IsOrientationSystemEnabledFor)), // calls TolUtil.IsOrientationSystemEnabledFor,
            new CodeInstruction(OpCodes.Brfalse_S, labelElse), // go to else block
            
            // if(TolUtil.SocializedLoveCheck(__instance, pActor, randomActorAround))
            CodeInstruction.LoadArgument(0), // loads instance from argument 0
            CodeInstruction.LoadArgument(1), // loads actor from argument 1
            CodeInstruction.LoadLocal(0), // gets stored randomActorAround
            new CodeInstruction(OpCodes.Ldc_I4_0), // pushes 0 integer which is just false in this case (BINARY WHOA)
            CodeInstruction.Call(typeof(TolUtil), nameof(TolUtil.SocializedLoveCheck)), // calls SocializedLoveCheck
            
            new CodeInstruction(OpCodes.Brfalse_S, labelContinue), // skips the rest of the code and goes back onto the original code if SocializedLoveCheck is false
            
            // if true..
            // return BehResult.Continue;
            new CodeInstruction(OpCodes.Ldc_I4_0),
            new CodeInstruction(OpCodes.Ret),
            
            // continue
            CodeInstruction.LoadArgument(1).WithLabels(labelElse), // load actor (and is the destination)
            CodeInstruction.LoadLocal(0), // load randomActorAround
            CodeInstruction.Call(typeof(ActorTool), nameof(ActorTool.checkFallInLove)) // calls checkFallInLove
        );
        codeMatcher = codeMatcher.AddLabels(List.Of(labelContinue)); // destination
        
        return codeMatcher.InstructionEnumeration();
    }
}