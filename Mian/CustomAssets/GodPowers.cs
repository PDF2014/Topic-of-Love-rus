
using Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.sex;

namespace Topic_of_Love.Mian.CustomAssets
{
    public class GodPowers
    {
        private static Actor _selectedActorA;
        private static Actor _selectedActorB;
        public static void Init()
        { 
            Add(new GodPower
            {
                id = "forceLover",
                name = "ForceLover",
                rank = PowerRank.Rank0_free,
                show_close_actor = true,
                unselect_when_window = true,
                path_icon = "god_powers/force_lover",
                can_drag_map = true,
                type = PowerActionType.PowerSpecial,
                select_button_action = _ => 
                {
                    WorldTip.showNow("unit_selected", pPosition: "top");
                    _selectedActorA = null;
                    _selectedActorB = null;
                    return false;
                },
                click_special_action = (pTile, _) =>
                {
                    var pActor = pTile != null ? ActionLibrary.getActorFromTile(pTile) : World.world.getActorNearCursor();
                    if (pActor == null)
                        return false;

                    if (_selectedActorA == null)
                    {
                        _selectedActorA = pActor;
                        ActionLibrary.showWhisperTip("unit_selected_first");
                        return false;
                    } 
                    
                    if (_selectedActorB == null && pActor == _selectedActorA)
                    {
                        ActionLibrary.showWhisperTip("love_cancelled");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }

                    if (_selectedActorA.lover == pActor)
                    {
                        ActionLibrary.showWhisperTip("love_cancelled");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }
          
                    Util.ShowWhisperTipWithTime("love_successful", 18f);
                    
                    _selectedActorB = pActor;
                    _selectedActorB.data.set("force_lover", true);
                    _selectedActorA.data.set("force_lover", true);
                    Util.BreakUp(_selectedActorA);
                    Util.BreakUp(_selectedActorB);
                    _selectedActorA.becomeLoversWith(_selectedActorB);
                    _selectedActorA = null;
                    _selectedActorB = null;
                
                    return true;
                },
            });
            
            Add(new GodPower
            { 
                id = "forceBreakup",
                name = "ForceBreakup",
                rank = PowerRank.Rank0_free,
                show_close_actor = true,
                unselect_when_window = true,
                path_icon = "status/disliked_sex",
                can_drag_map = true,
                type = PowerActionType.PowerSpecial,
                select_button_action = _ => 
                {
                    WorldTip.showNow("lover_selected", pPosition: "top");
                    return false;
                },
                click_special_action = (pTile, _) =>
                {
                    var pActor = pTile != null ? ActionLibrary.getActorFromTile(pTile) : World.world.getActorNearCursor();
                    if (pActor == null)
                        return false;
                    
                    if (!pActor.hasLover())
                    {
                        ActionLibrary.showWhisperTip("no_lover");
                        return false;
                    }
          
                    ActionLibrary.showWhisperTip("breakup_successful");
                    Util.BreakUp(pActor);
                    return true;
                },
            });
            
            Add(new GodPower
            { 
                id = "forceSex",
                name = "ForceSex",
                rank = PowerRank.Rank0_free,
                show_close_actor = true,
                unselect_when_window = true,
                path_icon = "status/enjoyed_sex",
                can_drag_map = true,
                type = PowerActionType.PowerSpecial,
                select_button_action = _ => 
                {
                    WorldTip.showNow("unit_selected", pPosition: "top");
                    _selectedActorA = null;
                    _selectedActorB = null;
                    return false;
                },
                click_special_action = (pTile, _) =>
                {
                    var pActor = pTile != null ? ActionLibrary.getActorFromTile(pTile) : World.world.getActorNearCursor();
                    if (pActor == null)
                        return false;

                    if (_selectedActorA == null)
                    {
                        _selectedActorA = pActor;
                        ActionLibrary.showWhisperTip("unit_selected_first");
                        return false;
                    } 
                    
                    if (_selectedActorB == null && pActor == _selectedActorA)
                    {
                        ActionLibrary.showWhisperTip("sex_cancelled");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }
          
                    Util.ShowWhisperTipWithTime("sex_successful", 24f);
                    
                    _selectedActorB = pActor;
                    _selectedActorA.cancelAllBeh();
                    _selectedActorA.stopMovement();
                    _selectedActorB.cancelAllBeh();
                    _selectedActorB.stopMovement();
                    _selectedActorB.data.set("sex_reason", "casual");
                    _selectedActorA.data.set("sex_reason", "casual");
                    _selectedActorA.beh_actor_target = _selectedActorB;
                    new BehGetPossibleTileForSex().execute(_selectedActorA);
                    _selectedActorA = null;
                    _selectedActorB = null;
                
                    return true;
                },
            });
            
            Add(new GodPower
            { 
                id = "forceKiss",
                name = "ForceKiss",
                rank = PowerRank.Rank0_free,
                show_close_actor = true,
                unselect_when_window = true,
                path_icon = "status/just_kissed",
                can_drag_map = true,
                type = PowerActionType.PowerSpecial,
                select_button_action = _ => 
                {
                    WorldTip.showNow("unit_selected", pPosition: "top");
                    _selectedActorA = null;
                    _selectedActorB = null;
                    return false;
                },
                click_special_action = (pTile, _) =>
                {
                    var pActor = pTile != null ? ActionLibrary.getActorFromTile(pTile) : World.world.getActorNearCursor();
                    if (pActor == null)
                        return false;

                    if (_selectedActorA == null)
                    {
                        _selectedActorA = pActor;
                        ActionLibrary.showWhisperTip("unit_selected_first");
                        return false;
                    } 
                    
                    if (_selectedActorB == null && pActor == _selectedActorA)
                    {
                        ActionLibrary.showWhisperTip("kiss_cancelled");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }
                    
                    _selectedActorB = pActor;
                    
                    if (!_selectedActorB.isOnSameIsland(_selectedActorA))
                    {
                        ActionLibrary.showWhisperTip("unit_too_far");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }
                    
                    Util.ShowWhisperTipWithTime("kiss_successful", 24f);
                    _selectedActorA.cancelAllBeh();
                    _selectedActorA.stopMovement();
                    _selectedActorB.cancelAllBeh();
                    _selectedActorB.stopMovement();
                    
                    _selectedActorA.beh_actor_target = _selectedActorB;
                    _selectedActorA.setTask("try_kiss", pClean:false, pCleanJob:true, pForceAction:true);
                    _selectedActorA = null;
                    _selectedActorB = null;
                
                    return true;
                },
            });
            
            Add(new GodPower
            { 
                id = "forceSexualIVF",
                name = "ForceSexualIVF",
                rank = PowerRank.Rank0_free,
                show_close_actor = true,
                unselect_when_window = true,
                path_icon = "status/adopted_baby",
                can_drag_map = true,
                type = PowerActionType.PowerSpecial,
                select_button_action = _ => 
                {
                    WorldTip.showNow("lover_selected", pPosition: "top");
                    return false;
                },
                click_special_action = (pTile, _) =>
                {
                    var pActor = pTile != null ? ActionLibrary.getActorFromTile(pTile) : World.world.getActorNearCursor();
                    if (pActor == null)
                        return false;
                    
                    // if (!pActor.hasLover())
                    // {
                    //     ActionLibrary.showWhisperTip("no_lover");
                    //     return false;
                    // }
                    //
                    // if (!pActor.hasLover() && !pActor.hasBestFriend())
                    // {
                    //     ActionLibrary.showWhisperTip("sexualivf_invalid_unit");
                    //     return false;
                    // }
                    
                    if (_selectedActorA == null)
                    {
                        if (!pActor.hasHouse() || pActor.hasStatus("pregnant"))
                        {
                            ActionLibrary.showWhisperTip("sexualivf_invalid_unit");
                            return false;
                        }
                        
                        _selectedActorA = pActor;
                        ActionLibrary.showWhisperTip("unit_selected_first");
                        return false;
                    } 
                    
                    if (_selectedActorB == null && pActor == _selectedActorA)
                    {
                        ActionLibrary.showWhisperTip("sexualivf_cancelled");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }
                    
                    _selectedActorB = pActor;
                    
                    if (!_selectedActorB.isOnSameIsland(_selectedActorA))
                    {
                        ActionLibrary.showWhisperTip("unit_too_far");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }
                    
                    if (_selectedActorB.hasStatus("pregnant"))
                    {
                        ActionLibrary.showWhisperTip("sexualivf_invalid_unit");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }
                    
                    //
                    // var home = pActor.getHomeBuilding();
                    //
                    // if (pActor.hasLover() && (pActor.lover.hasStatus("pregnant") || !pActor.isSameIslandAs(pActor.lover) || !Util.CanReproduce(pActor, pActor.lover))
                    //     && pActor.hasBestFriend() && (pActor.getBestFriend().hasStatus("pregnant") || !pActor.isOnSameIsland(pActor.getBestFriend()) || !Util.CanReproduce(pActor, pActor.getBestFriend())))
                    // {
                    //     ActionLibrary.showWhisperTip("sexualivf_unavailable");
                    //     return false;
                    // }
                    //
                    // if (pActor.hasLover() && Util.CanReproduce(pActor, pActor.lover) 
                    //                       && pActor.isSameIslandAs(pActor.lover)
                    //                       && !pActor.lover.hasStatus("pregnant"))
                    //     pActor.beh_actor_target = pActor.lover;
                    // else
                    //     pActor.beh_actor_target = pActor.getBestFriend();
                    _selectedActorA.cancelAllBeh();
                    _selectedActorA.stopMovement();
                    _selectedActorB.cancelAllBeh();
                    _selectedActorB.stopMovement();
                    
                    _selectedActorA.beh_actor_target = _selectedActorB;
                    _selectedActorA.setTask("try_sexual_ivf", pCleanJob: true, pClean: false, pForceAction: true);
                    
                    // pActor.beh_building_target = home;
                    // target.beh_actor_target = pActor;
                    // target.beh_building_target = home;
                    //
                    // pActor.beh_actor_target.a.setTask("go_and_wait_sexual_ivf", pCleanJob: true, pClean:false, pForceAction:true);
                    // pActor.beh_actor_target.a.timer_action = 0.0f;
                    // pActor.setTask("go_sexual_ivf", pClean: false, pForceAction:true);
                    
                    Util.ShowWhisperTipWithTime("sexualivf_successful", 24f);
                    _selectedActorA = null;
                    _selectedActorB = null;
                    return true;
                },
            });
            
            Add(new GodPower
            { 
                id = "forceDate",
                name = "ForceDate",
                rank = PowerRank.Rank0_free,
                show_close_actor = true,
                unselect_when_window = true,
                path_icon = "status/went_on_date",
                can_drag_map = true,
                type = PowerActionType.PowerSpecial,
                select_button_action = _ => 
                {
                    WorldTip.showNow("lover_selected", pPosition: "top");
                    return false;
                },
                click_special_action = (pTile, _) =>
                {
                    var pActor = pTile != null ? ActionLibrary.getActorFromTile(pTile) : World.world.getActorNearCursor();
                    if (pActor == null)
                        return false;
                    
                    if (_selectedActorA == null)
                    {
                        _selectedActorA = pActor;
                        ActionLibrary.showWhisperTip("unit_selected_first");
                        return false;
                    } 
                    
                    if (_selectedActorB == null && pActor == _selectedActorA)
                    {
                        ActionLibrary.showWhisperTip("date_cancelled");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }
                    
                    _selectedActorB = pActor;
                    
                    if (!_selectedActorB.isOnSameIsland(_selectedActorA))
                    {
                        ActionLibrary.showWhisperTip("not_same_island");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }
          
                    Util.ShowWhisperTipWithTime("date_successful", 24f);

                    _selectedActorA.cancelAllBeh();
                    _selectedActorA.stopMovement();
                    _selectedActorB.cancelAllBeh();
                    _selectedActorB.stopMovement();
                    _selectedActorA.beh_actor_target = _selectedActorB;
                    _selectedActorB.makeWait();
                    _selectedActorA.setTask("try_date", pClean: false, pForceAction:true);
                    _selectedActorA = null;
                    _selectedActorB = null;
                    return true;
                },
            });
        }

        private static void Add(GodPower power)
        {
            AssetManager.powers.add(power);
        }
    }
}