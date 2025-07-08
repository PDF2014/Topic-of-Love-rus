using System.Collections.Generic;
using Topic_of_Love.Mian.CustomAssets.Traits;
using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.CustomAssets.AI;

public class Decisions
{
    private static List<DecisionAsset> _decisionAssets = new List<DecisionAsset>();

    public static void Init()
    {
        Add(new DecisionAsset
        {
            id = "insult_orientation_try",
            task_id = "insult_orientation",
            priority = NeuroLayer.Layer_2_Moderate,
            path_icon = "ui/Icons/culture_traits/orientationless",
            cooldown = 30,
            action_check_launch = actor => (actor.hasCultureTrait("homophobic") || actor.hasCultureTrait("heterophobic")) && TolUtil.IsOrientationSystemEnabledFor(actor),
            weight = 0.5f,
            list_civ = true,
            only_safe = true
        });
        AssetManager.subspecies_traits.get("wernicke_area").addDecision("insult_orientation_try");

        Add(new DecisionAsset
        {
            id = "find_kiss",
            priority = NeuroLayer.Layer_2_Moderate,
            path_icon = "ui/Icons/status/just_kissed",
            cooldown = 15,
            action_check_launch = actor => TolUtil.CapableOfLove(actor)
                                           && (actor.hasLover() || actor.hasBestFriend())
                                           && !actor.isIntimacyHappinessEnough(100f)
                                           && TolUtil.IsOrientationSystemEnabledFor(actor)
                                           && !actor.hasStatus("just_kissed"),
            weight_calculate_custom = actor => actor.isIntimacyHappinessEnough(75f) ? 0.5f: 
                actor.isIntimacyHappinessEnough( 50f) ? 0.6f : actor.isIntimacyHappinessEnough( 0) ? .8f : 
                actor.isIntimacyHappinessEnough( -50) ? 1f : actor.isIntimacyHappinessEnough( -100f) ? 1.5f : 1.25f,
            only_safe = true,
            cooldown_on_launch_failure = true
        });
        
        Add(new DecisionAsset
        {
            id = "find_date",
            priority = NeuroLayer.Layer_2_Moderate,
            path_icon = "ui/Icons/status/went_on_date",
            cooldown = 30,
            action_check_launch = actor => TolUtil.CapableOfLove(actor)
                                           && !actor.isIntimacyHappinessEnough( 100f)
                                           && TolUtil.IsOrientationSystemEnabledFor(actor)
                                           && !actor.hasStatus("went_on_date"),
            weight_calculate_custom = actor => !actor.hasLover() ? 1.5f : actor.isIntimacyHappinessEnough( 75f) ? 0.5f: 
                actor.isIntimacyHappinessEnough( 50f) ? 0.6f : actor.isIntimacyHappinessEnough( 0) ? .8f : 
                actor.isIntimacyHappinessEnough( -50) ? 1f : actor.isIntimacyHappinessEnough( -100f) ? 1.5f : 1.25f,
            only_safe = true,
            cooldown_on_launch_failure = true
        });
        
        Add(new DecisionAsset
        {
            id = "reproduce_preservation",
            priority = NeuroLayer.Layer_4_Critical,
            path_icon = "ui/Icons/status/disliked_sex",
            cooldown = 15,
            action_check_launch = actor =>
            {
                if (TolUtil.IsDyingOut(actor)
                    && actor.hasSubspeciesTrait("preservation")
                    && TolUtil.IsOrientationSystemEnabledFor(actor))
                {
                    actor.subspecies.countReproductionNeuron();
                    return true;
                }

                return false;
            },
            weight = 2.5f,
            only_adult = true,
            only_safe = true
        });
        // will force all units to make babies regardless of orientation if they have preservation
        AssetManager.subspecies_traits.get("reproduction_sexual").addDecision("reproduce_preservation");
        AssetManager.subspecies_traits.get("reproduction_same_sex").addDecision("reproduce_preservation");
        AssetManager.subspecies_traits.get("reproduction_hermaphroditic").addDecision("reproduce_preservation");
        
        Add(new DecisionAsset
        {
            id = "invite_for_sex",
            priority = NeuroLayer.Layer_3_High,
            path_icon = "ui/Icons/status/enjoyed_sex",
            cooldown = 15,
            action_check_launch = actor => TolUtil.CapableOfLove(actor)
                                           && !LikeAssets.Dislikes(actor, true)
                                           && !actor.isIntimacyHappinessEnough( 100f)
                                           && TolUtil.IsOrientationSystemEnabledFor(actor),
            weight_calculate_custom = actor =>
            {
                var weight = actor.isIntimacyHappinessEnough( 75f) ? 0.75f :
                    actor.isIntimacyHappinessEnough( 50f) ? 1f :
                    actor.isIntimacyHappinessEnough( 0) ? 1.25f :
                    actor.isIntimacyHappinessEnough( -50) ? 1.5f :
                    actor.isIntimacyHappinessEnough( -100f) ? 2f : 1.75f;

                if (actor.hasLover() && actor.distanceToActorTile(actor) <= 50)
                {
                    weight += 0.5f;
                }
                
                return weight;
            },
            only_adult = true,
            only_safe = true
        });

        Add(new DecisionAsset
        {
            id = "find_sexual_ivf",
            priority = NeuroLayer.Layer_2_Moderate,
            path_icon = "ui/Icons/status/adopted_baby",
            cooldown = 10,
            action_check_launch = actor =>
            {
                if (!actor.isSapient() || !TolUtil.WantsBaby(actor, false))
                    return false;
                    
                var bestFriend = actor.getBestFriend();

                if (actor.hasLover())
                {
                    if (!TolUtil.WantsBaby(actor.lover, false))
                        return false;

                    if (TolUtil.CouldReproduce(actor, actor.lover) &&
                        !LikeAssets.BothActorsPreferenceMatch(actor, actor.lover, true))
                    {
                        return true;
                    }

                    if (TolUtil.CouldReproduce(actor, actor.lover) &&
                        LikeAssets.BothActorsPreferenceMatch(actor, actor.lover, true))
                        return false;
                }

                bool success = bestFriend != null && TolUtil.CouldReproduce(actor, bestFriend) &&
                               !bestFriend.hasStatus("pregnant");
                return success;
            },
            list_civ = true,
            weight = 1.5f,
            only_safe = true,
            only_adult = true
        });
        Finish();
    }
    
            private static void Finish()
        {
            for(int i = 0; i < _decisionAssets.Count; i++)
            {
                var decisionAsset = _decisionAssets[i];
                decisionAsset.priority_int_cached = (int) decisionAsset.priority;
                decisionAsset.has_weight_custom = decisionAsset.weight_calculate_custom != null;
                if (!decisionAsset.unique)
                {
                    if (decisionAsset.list_baby)
                        AssetManager.decisions_library.list_only_children = AssetManager.decisions_library.list_only_children.AddToArray(decisionAsset);
                    else if (decisionAsset.list_animal)
                        AssetManager.decisions_library.list_only_animal = AssetManager.decisions_library.list_only_animal.AddToArray(decisionAsset);
                    else if (decisionAsset.list_civ)
                        AssetManager.decisions_library.list_only_civ = AssetManager.decisions_library.list_only_civ.AddToArray(decisionAsset);
                    else
                        AssetManager.decisions_library.list_only_city = AssetManager.decisions_library.list_only_city.AddToArray(decisionAsset);
                }
            }
        }

    
    private static void Add(DecisionAsset asset)
    {
        AssetManager.decisions_library.add(asset);
        asset.decision_index = AssetManager.decisions_library.list.Count-1;
        _decisionAssets.Add(asset);
    }
}