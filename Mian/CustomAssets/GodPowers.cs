
using EpPathFinding.cs;
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
          
                    TolUtil.ShowWhisperTipWithTime("love_successful", 18f);
                    
                    _selectedActorB = pActor;
                    _selectedActorA.RemoveLovers();
                    _selectedActorB.RemoveLovers();
                    _selectedActorB.data.set("force_lover", true);
                    _selectedActorA.data.set("force_lover", true);
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
                path_icon = "status/broke_up",
                can_drag_map = true,
                type = PowerActionType.PowerSpecial,
                select_button_action = _ => 
                {
                    WorldTip.showNow("lover_selected", pPosition: "top");
                    _selectedActorA = null;
                    _selectedActorB = null;
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
                    pActor.BreakUp();
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
          
                    TolUtil.ShowWhisperTipWithTime("sex_successful", 24f);
                    
                    _selectedActorB = pActor;
                    _selectedActorA.cancelAllBeh();
                    // _selectedActorA.stopMovement();
                    _selectedActorB.cancelAllBeh();
                    // _selectedActorB.stopMovement();
                    _selectedActorB.data.set("sex_reason", "casual");
                    _selectedActorA.data.set("sex_reason", "casual");
                    _selectedActorA.beh_actor_target = _selectedActorB;
                    new BehGetPossibleTileForSex(false).execute(_selectedActorA);
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
                    
                    TolUtil.ShowWhisperTipWithTime("kiss_successful", 24f);
                    _selectedActorA.cancelAllBeh();
                    // _selectedActorA.stopMovement();
                    _selectedActorB.cancelAllBeh();
                    // _selectedActorB.stopMovement();
                    
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

                    if (!_selectedActorA.HaveAppropriatePartsForReproduction(_selectedActorB))
                    {
                        ActionLibrary.showWhisperTip("sexualivf_incapable_reproduce");
                        _selectedActorA = null;
                        _selectedActorB = null;
                        return false;
                    }
                    
                    _selectedActorA.cancelAllBeh();
                    // _selectedActorA.stopMovement();
                    _selectedActorB.cancelAllBeh();
                    // _selectedActorB.stopMovement();
                    _selectedActorB.makeWait();

                    _selectedActorA.beh_actor_target = _selectedActorB;
                    _selectedActorA.setTask("try_sexual_ivf", pCleanJob: true, pClean: false, pForceAction: true);
                    
                    TolUtil.ShowWhisperTipWithTime("sexualivf_successful", 24f);
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
          
                    TolUtil.ShowWhisperTipWithTime("date_successful", 24f);

                    _selectedActorA.cancelAllBeh();
                    // _selectedActorA.stopMovement();
                    _selectedActorB.cancelAllBeh();
                    // _selectedActorB.stopMovement();
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