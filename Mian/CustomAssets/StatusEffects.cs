using System;
using System.Collections.Generic;
using Topic_of_Love.Mian.CustomAssets.Custom;
using Topic_of_Love.Mian.CustomAssets.Traits;
using UnityEngine;

namespace Topic_of_Love.Mian.CustomAssets;

public class StatusEffects
{
    private static List<StatusAsset> _assets = new List<StatusAsset>();
    
    public static void Init()
    {
        Add(new StatusAsset
        {
            id="just_kissed",
            duration = 25f,
            action_on_receive = (actor, _) =>
            {
                actor.a.data.get("last_had_intimate_interaction_with", out long partnerID);
                
                var partner = MapBox.instance.units.get(partnerID);
                if (partner != null)
                {
                    var changeBy = 0f;

                    if (LikesManager.LikeMatches(actor.a, partner, false))
                        changeBy += 10f;
                    
                    if (LikesManager.LikeMatches(actor.a, partner, true))
                        changeBy += 5f;
                
                    if (partner == actor.a.lover)
                        changeBy += 10f;
                
                    actor.a.changeIntimacyHappiness(changeBy);
                    
                    if(!actor.a.CanHaveIntimacyWithoutRepercussions(SexType.None))
                        TolUtil.PotentiallyCheatedWith(actor.a, partner);
                }

                actor.a.changeHappiness("just_kissed");
                TolUtil.Debug(actor.a.getName() + " just kissed!");
                return true;
            }
        });
        Add(new StatusAsset
        {
            id="enjoyed_sex",
            duration = 35f,
            action_on_receive = (actor, _) =>
            {
                actor.a.data.get("intimacy_happiness", out float happiness);
                actor.a.data.get("last_had_intimate_interaction_with", out long partnerID);
                var changeBy = 20f;
                
                var sexPartner = MapBox.instance.units.get(partnerID);
                if (sexPartner != null && sexPartner == actor.a.lover)
                    changeBy += 15f;
                
                if (happiness <= 0)
                {
                    changeBy += Math.Abs(happiness / 3);
                }
                
                actor.a.changeIntimacyHappiness(changeBy);
                
                actor.a.changeHappiness("enjoyed_sex");
                actor.a.finishStatusEffect("disliked_sex");
                actor.a.finishStatusEffect("okay_sex");
                return true;
            }
        });
        Add(new StatusAsset
        {
            id="disliked_sex",
            duration = 35f,
            action_on_receive = (actor, _) => 
            {
                actor.a.data.get("intimacy_happiness", out float happiness);
                var changeBy = -35f;
                
                if (happiness <= 0)
                {
                    changeBy += happiness / 3; // become more deprived if the sex was bad
                }

                actor.a.changeIntimacyHappiness(changeBy);
                actor.a.changeHappiness("disliked_sex");
                actor.a.finishStatusEffect("enjoyed_sex");
                actor.a.finishStatusEffect("okay_sex");
                return true;
            }
        });
        Add(new StatusAsset
        {
            id="okay_sex",
            duration = 35f,
            action_on_receive = (actor, _) =>
            {
                actor.a.data.get("last_had_intimate_interaction_with", out long partnerID);
                var changeBy = 10f;
                
                var sexPartner = MapBox.instance.units.get(partnerID);
                if (sexPartner != null && sexPartner == actor.a.lover)
                    changeBy += 5f; // okay sex but add extra if with lover at least
                
                actor.a.changeIntimacyHappiness(changeBy);
                actor.a.changeHappiness("okay_sex");
                actor.a.finishStatusEffect("enjoyed_sex");
                actor.a.finishStatusEffect("disliked_sex");
                return true;
            }
        });
        Add(new StatusAsset
        {
            id="went_on_date",
            duration = 60f,
            action_on_receive = (actor, _) =>
            {
                actor.a.changeHappiness("went_on_date");
                return true;
            }
        });
        
        Add(new StatusAsset
        {
            id = "broke_up",
            duration = 60f,
            action_on_receive = (actor, _) =>
            {
                actor.a.changeHappiness("broke_up");
                
                // depression :(
                actor.a.changeIntimacyHappiness(-50);
                return true;
            }
        });
        
        Add(new StatusAsset
        {
            id="cheated_on",
            duration = 60f,
            action_on_receive = (cheatedActor, _) =>
            {
                if (!TolUtil.CanStopBeingLovers(cheatedActor.a))
                    return false;
                
                var lover = cheatedActor.a.lover;
                if (lover == null)
                    return false;
                TolUtil.RemoveLovers(cheatedActor.a);
                if (Randy.randomChance(0.5f) || !(cheatedActor.a.warfare > 10)) // only ppl with warfare above 10 will consider fighting
                {
                    cheatedActor.a.addStatusEffect("crying");
                } else
                {
                    cheatedActor.a.addAggro(lover);
                    lover.data.get("last_had_intimate_interaction_with", out long id);
                    var otherActorInvolved = MapBox.instance.units.get(id);
                    if (otherActorInvolved != null)
                    {
                        cheatedActor.a.addAggro(otherActorInvolved);
                        if(otherActorInvolved.isOnSameIsland(cheatedActor.a))
                            cheatedActor.a.startFightingWith(otherActorInvolved);
                    }
                    else
                    {
                        if(lover.isOnSameIsland(cheatedActor.a))
                            cheatedActor.a.startFightingWith(lover);
                    }
                }
                
                // that's a fucking mood killer
                cheatedActor.a.changeIntimacyHappiness(-100);
                cheatedActor.a.changeHappiness("cheated_on");
                cheatedActor.a.data.set("cheated_" +lover.getID(),true);
                
                // DateableManager.Manager.AddOrRemoveUndateable(cheatedActor.a, lover);
                TolUtil.AddOrRemoveUndateableActor(cheatedActor.a, lover);

                TolUtil.Debug(lover.getName() + " just cheated on "+cheatedActor.a.getName());
                return true;
            }
        });
        Finish();
    }

    private static void Finish()
    {
        foreach (var statusAsset in _assets)
        {
            if (statusAsset.get_override_sprite != null)
            {
                statusAsset.has_override_sprite = true;
                statusAsset.need_visual_render = true;
            }
            if (statusAsset.get_override_sprite_position != null)
                statusAsset.has_override_sprite_position = true;
            if (statusAsset.get_override_sprite_rotation_z != null)
                statusAsset.has_override_sprite_rotation_z = true;
            if (statusAsset.texture != null)
                statusAsset.need_visual_render = true;
            
            if (statusAsset.texture != null && statusAsset.sprite_list == null)
                statusAsset.sprite_list = Resources.LoadAll<Sprite>("effects/" + statusAsset.texture);
        }
    }
    
    private static void Add(StatusAsset asset)
    {
        AssetManager.status.add(asset);
        asset.locale_id = "status_title_" + asset.id;
        asset.locale_description = "status_description_" + asset.id;
        asset.path_icon = "ui/Icons/status/" + asset.id;
        _assets.Add(asset);
    }
}