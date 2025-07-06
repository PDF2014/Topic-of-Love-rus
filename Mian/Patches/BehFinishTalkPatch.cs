using System.Collections.Generic;
using System.Reflection.Emit;
using ai;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BehFinishTalk))]
public class BehFinishTalkPatch
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BehFinishTalk.finishTalk))]
    static IEnumerable<CodeInstruction> BehFinishTalkTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codeMatcher = new CodeMatcher(instructions, generator);

        codeMatcher.MatchStartForward(new CodeMatch(OpCodes.Call,
            AccessTools.Method(typeof(ActorTool), nameof(ActorTool.checkFallInLove), new[]{typeof(Actor), typeof(Actor)})))
            .ThrowIfInvalid("Could not find checkFallInLove.. damn game update :(")
            .RemoveInstructionsInRange(codeMatcher.Pos - 4, codeMatcher.Pos);
        //
        // var goPastLove = (Label) codeMatcher.Instruction.operand;
        //
        // codeMatcher.Advance(1);
        // var higherChanceBranch = generator.DefineLabel();
        // var fallInLoveBranch = generator.DefineLabel();
        // var skipFriendLabel = generator.DefineLabel();
        //
        // codeMatcher.InsertAndAdvance(
        //     new CodeInstruction(OpCodes.Ldarg_0),
        //     new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Actor), nameof(Actor.data))),
        //     new CodeInstruction(OpCodes.Ldnull),
        //     new CodeInstruction(OpCodes.Ceq),
        //     new CodeInstruction(OpCodes.Brtrue, skipFriendLabel),
        //     new CodeInstruction(OpCodes.Ldarg_0),
        //     new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Actor), nameof(Actor.getBestFriend))),
        //     new CodeInstruction(OpCodes.Ldarg_1),
        //     new CodeInstruction(OpCodes.Ceq),
        //     new CodeInstruction(OpCodes.Brtrue, higherChanceBranch),
        //     new CodeInstruction(OpCodes.Ldc_R4, 0.25f).WithLabels(skipFriendLabel),
        //     new CodeInstruction(OpCodes.Br, fallInLoveBranch),
        //     new CodeInstruction(OpCodes.Ldc_R4, 0.75f).WithLabels(higherChanceBranch),
        //     
        //     new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Randy), nameof(Randy.randomChance)))
        //         .WithLabels(fallInLoveBranch),
        //     new CodeInstruction(OpCodes.Brfalse, goPastLove)
        // );
        //
        return codeMatcher.InstructionEnumeration();
    }
    
    [HarmonyPrefix]
    [HarmonyPriority(Priority.VeryLow)] // dugga doo dugga doo dugga dugag dugga doo
    [HarmonyPatch(nameof(BehFinishTalk.finishTalk))]
    static void FinishTalkPatch(Actor pActor, Actor pTarget)
    {
        if(Randy.randomChance(pActor.getBestFriend() == pTarget ? 0.2f : 0.05f))
            ActorTool.checkFallInLove(pActor, pTarget);
        
        pActor.changeIntimacyHappiness(45);
        pTarget.changeIntimacyHappiness(45);
    }
}