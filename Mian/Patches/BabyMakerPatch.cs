using System.Collections;
using System.Collections.Generic;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

public class BabyMakerPatch
{
    [HarmonyPatch(typeof(BabyMaker), nameof(BabyMaker.makeBabyFromPregnancy))]
    class MakeBabyFromPregnancyPatch
    {
        static bool Prefix(Actor pActor)
        {
            var mother = pActor;
            mother.data.get("otherParent", out long otherParentID);
            var otherParent = MapBox.instance.units.get(otherParentID);
            mother.data.removeLong("otherParent");

            mother.birthEvent();
            BabyHelper.countMakeChild(mother, otherParent);
            BabyMaker.makeBaby(mother, otherParent);
            var pVal = 0.5f;
            var stat = (int) mother.stats["birth_rate"];
            for (var index = 0; index < stat && Randy.randomChance(pVal); ++index)
            {
                BabyMaker.makeBaby(mother, otherParent);
                pVal *= 0.85f;
            }

            return false;
        }
    }
        // We just randomize the parent chosen for the subspecies here
    [HarmonyPatch(typeof(BabyMaker), nameof(BabyMaker.makeBaby))]
    class MakeBabyPatch
    {
        static bool Prefix(
            Actor pParent1,
            Actor pParent2,
            ActorSex pForcedSexType,
            bool pCloneTraits,
            int pMutationRate,
            WorldTile pTile,
            bool pAddToFamily,
            ref Actor __result
            )
        {
            List<Actor> parents = new List<Actor>{pParent1};
            if(pParent2 != null)
                parents.Add(pParent2);
            
            City pCity = pParent1.city ?? pParent2?.city;
            
            if (pCity != null)
                --pCity.status.housing_free;

            Actor dominantParent = TOLUtil.EnsurePopulationFromParent(parents);
            if (dominantParent == null) 
                // there seems to be a bug in the game that allows reproduction strategies that aren't sexual to PRODUCE beyond the harmony traits cap. probably because they dont check for populations there lol
                dominantParent = pParent1;
            
            Actor nonDominantParent = dominantParent != pParent1 ? pParent1 : pParent2;
            ActorAsset asset = dominantParent.asset;
            
            ActorData pData = new ActorData();
            pData.created_time = World.world.getCurWorldTime();
            pData.id = World.world.map_stats.getNextId("unit");
            pData.asset_id = asset.id;
            int generation = dominantParent.data.generation;
            if (nonDominantParent != null && nonDominantParent.data.generation > generation)
                generation = nonDominantParent.data.generation;
            pData.generation = generation + 1;
            using (ListPool<WorldTile> list = new ListPool<WorldTile>(4))
            {
                foreach (WorldTile worldTile in dominantParent.current_tile.neighboursAll)
                {
                    if (worldTile != dominantParent.current_tile &&
                        (nonDominantParent == null || worldTile != nonDominantParent.current_tile) &&
                        worldTile.Type.ground)
                        list.Add(worldTile);
                }

                WorldTile pTile1 = pTile == null
                    ? ((ICollection)list).Count != 0 ? list.GetRandom<WorldTile>() : dominantParent.current_tile
                    : pTile;
                Actor actorFromData = World.world.units.createActorFromData(pData, pTile1, pCity);

                pParent1.data.get("familyParentA", out var familyParentAid, 0L);
                pParent1.data.get("familyParentB", out var familyParentBid, 0L);
                var familyParentA = World.world.units.get(familyParentAid);
                var familyParentB = World.world.units.get(familyParentBid);

                if (familyParentA != null)
                {
                    actorFromData.setParent1(familyParentA);
                    familyParentA.changeHappiness("adopted_baby");
                }
                else
                    actorFromData.setParent1(dominantParent);

                if (familyParentB != null){
                    actorFromData.setParent2(familyParentB);
                    familyParentB.changeHappiness("adopted_baby");
                }else if(nonDominantParent != null)
                    actorFromData.setParent2(nonDominantParent);

                if (pAddToFamily && familyParentA != null)
                {
                    // if (familyParentA != null)
                    // {
                        if (!familyParentA.hasFamily())
                            World.world.families.newFamily(familyParentA, familyParentA.current_tile, familyParentB);
                        // actorFromData.setFamily(familyParentA.family);
                    // }
                    // else
                    // {
                    //     var randomParentToGoTo = dominantParent.hasFamily() 
                    //                              && (Randy.randomChance(0.5f) || nonDominantParent == null) ? dominantParent : nonDominantParent;
                    //     if(randomParentToGoTo != null && randomParentToGoTo.hasFamily())
                    //         actorFromData.setFamily(randomParentToGoTo.family);
                    // }
                }
                BabyHelper.applyParentsMeta(familyParentA != null ? familyParentA : dominantParent, familyParentB != null ? familyParentB : nonDominantParent, actorFromData);
                // the game seems to have some sort of code that chooses a baby's subspecies based on generation? not really sure how it works tbh
                actorFromData.setSubspecies(dominantParent.subspecies);

                if (pCloneTraits || dominantParent.hasSubspeciesTrait("genetic_mirror"))
                {
                    if (pCloneTraits)
                    {
                        BabyHelper.traitsClone(actorFromData, dominantParent);
                        if (nonDominantParent != null)
                        {
                            BabyHelper.traitsClone(actorFromData, nonDominantParent);
                        }
                    }
                    else
                    {
                        BabyHelper.traitsClone(actorFromData, dominantParent);

                        if (nonDominantParent != null)
                        {
                            BabyHelper.traitsInherit(actorFromData, nonDominantParent, null);   
                        }                        
                    }
                }
                else
                {
                    foreach (ActorTrait trait in (IEnumerable<ActorTrait>)actorFromData.subspecies.getActorBirthTraits()
                                 .getTraits())
                        actorFromData.addTrait(trait);
                    BabyHelper.traitsInherit(actorFromData, pParent1, nonDominantParent);
                }

                actorFromData.checkTraitMutationOnBirth();
                actorFromData.setNutrition(SimGlobals.m.nutrition_start_level_baby);
                if (pForcedSexType != ActorSex.None)
                {
                    actorFromData.data.sex = pForcedSexType;
                }
                else
                {
                    ActorSex actorSex = ActorSex.None;

                    // confirm it was sexual
                    if (pParent1.hasSubspeciesTrait("reproduction_same_sex") && pParent2 != null)
                    {
                        actorSex = pParent1.data.sex;
                    } else if (pParent2 != null && pParent2.hasSubspeciesTrait("reproduction_same_sex"))
                    {
                        actorSex = pParent2.data.sex;
                    }
                    else
                    {
                        if (Randy.randomBool())
                            actorSex = !dominantParent.hasCity()
                                ? (dominantParent.subspecies.cached_females <= dominantParent.subspecies.cached_males
                                    ? ActorSex.Female
                                    : ActorSex.Male)
                                : (dominantParent.city.status.females <= dominantParent.city.status.males
                                    ? ActorSex.Female
                                    : ActorSex.Male);   
                    }
                    if (actorSex != ActorSex.None)
                        actorFromData.data.sex = actorSex;
                    else
                        actorFromData.generateSex();
                }
                actorFromData.checkShouldBeEgg();
                actorFromData.makeStunned(10f);
                actorFromData.applyRandomForce();
                BabyHelper.countBirth(actorFromData);
                actorFromData.setStatsDirty();
                actorFromData.event_full_stats = true;
                __result = actorFromData;
                
                // LogService.LogInfo($"[{__result.getName()}]: Baby is of {__result.subspecies.name} and comes from {__result.asset.getTranslatedName()}");
                
                pParent1.data.removeLong("familyParentA");
                pParent1.data.removeLong("familyParentB");
                return false;
            }
        }
    }
}