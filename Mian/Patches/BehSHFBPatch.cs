using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using ai.behaviours;
using HarmonyLib;
using NeoModLoader.services;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BehSpawnHeartsFromBuilding))]
public class BehSHFBPatch
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BehSpawnHeartsFromBuilding.execute))]
    static IEnumerable<CodeInstruction> AllowForTargets(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codeMatcher = new CodeMatcher(instructions, generator);
        
        codeMatcher.MatchStartForward(new CodeMatch(OpCodes.Callvirt,
            AccessTools.Method(typeof(Actor), nameof(Actor.hasLover))));
        var returnFalse = (Label)codeMatcher.Advance(1).Operand;
        codeMatcher.RemoveInstructionsInRange(codeMatcher.Pos - 2, codeMatcher.Pos);

        var goToLoverCheck = generator.DefineLabel();
        var outtaHere  = generator.DefineLabel();
        var sexTarget = generator.DeclareLocal(typeof(Actor));
        
        codeMatcher.Start().InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Actor), nameof(Actor.beh_actor_target))),
            new CodeInstruction(OpCodes.Brfalse, goToLoverCheck),
            
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Actor), nameof(Actor.beh_actor_target))),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BaseSimObject), nameof(BaseSimObject.a))),
            new CodeInstruction(OpCodes.Stloc, sexTarget),
            new CodeInstruction(OpCodes.Nop, outtaHere),
            
            new CodeInstruction(OpCodes.Ldarg_0).WithLabels(goToLoverCheck),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Actor), nameof(Actor.lover))),
            new CodeInstruction(OpCodes.Brfalse, returnFalse),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Actor), nameof(Actor.lover))),
            new CodeInstruction(OpCodes.Stloc, sexTarget),
            new CodeInstruction(OpCodes.Nop, outtaHere)
        );

        try
        {
            var afterglowPos = codeMatcher.Start().MatchStartForward(new CodeMatch(OpCodes.Callvirt,
                    AccessTools.Method(typeof(Actor), nameof(Actor.addAfterglowStatus))))
                .Pos;
            codeMatcher.RemoveInstructionsInRange(afterglowPos - 1, afterglowPos + 4);
        }catch (InvalidOperationException e)
        {
            TolUtil.LogInfo("Did someone remove addAfterGlowStatus..? It's not here :(\n"+e.Message);
        }
        
        codeMatcher.Start().SearchForward(instruction => instruction.labels.Contains(returnFalse)).Advance(2).AddLabels(new[]{outtaHere});
        
        return codeMatcher.InstructionEnumeration();
    }
}