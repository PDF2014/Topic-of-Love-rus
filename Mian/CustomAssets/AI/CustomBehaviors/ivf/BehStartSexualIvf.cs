using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.ivf;
public class BehStartSexualIvf : BehaviourActionActor
    {
        private Actor _target;
        private BehResult Cancel()
        {
            _target.cancelAllBeh();
            return BehResult.Stop;
        }
        public override BehResult execute(Actor pActor)
        {
            TolUtil.Debug("Actually starting sexual ivf for "+pActor.getName());
            if (pActor.beh_actor_target == null || pActor.beh_building_target == null)
            {
                TolUtil.Debug(pActor.getName()+": Cancelled from starting sexual ivf target because actor was null");
                return BehResult.Stop;
            }

            _target = pActor.beh_actor_target.a;

            Actor pregnantActor = null;
            
            if (TolUtil.NeedDifferentSexTypeForReproduction(pActor) && TolUtil.NeedDifferentSexTypeForReproduction(_target))
            {
                if (pActor.data.sex == _target.data.sex) return Cancel();
                
                if (pActor.isSexFemale())
                    pregnantActor = pActor;
                else if (_target.isSexFemale())
                    pregnantActor = _target;
            }
            else if(TolUtil.NeedSameSexTypeForReproduction(pActor) && TolUtil.NeedSameSexTypeForReproduction(_target))
            {
                if (pActor.data.sex != _target.data.sex) return Cancel();
                pregnantActor = !Randy.randomBool() ? _target : pActor;
            } else if (TolUtil.CanDoAnySexType(pActor) || TolUtil.CanDoAnySexType(_target))
            {
                if(TolUtil.CanDoAnySexType(pActor) && TolUtil.CanDoAnySexType(_target))
                    pregnantActor = !Randy.randomBool() ? _target : pActor;
                else if (TolUtil.CanDoAnySexType(pActor))
                {
                    pregnantActor = pActor;
                }
                else
                {
                    pregnantActor = _target;
                }
            }

            if (pregnantActor == null)
                return Cancel();
            
            var nonPregnantActor = pregnantActor == pActor ? _target : pActor;

            pregnantActor.data.set("familyParentA", pActor.getID());
            if (pActor.hasLover())
            {
                pregnantActor.data.set("familyParentB", pActor.lover.getID());
            }
            
            (new BehCheckForBabiesFromSexualReproduction()).checkFamily(pActor, pActor.lover);

            var reproductionStrategy = pregnantActor.subspecies.getReproductionStrategy();
            switch (reproductionStrategy)
            {
                case ReproductiveStrategy.Egg:
                case ReproductiveStrategy.SpawnUnitImmediate:
                    BabyMaker.makeBabiesViaSexual(pregnantActor, pregnantActor, _target);
                    pregnantActor.subspecies.counterReproduction();
                    break;
                case ReproductiveStrategy.Pregnancy:
                    var maturationTimeSeconds = pregnantActor.getMaturationTimeSeconds();
                    pregnantActor.data.set("otherParent", nonPregnantActor.getID());

                    BabyHelper.babyMakingStart(pregnantActor);
                    pregnantActor.addStatusEffect("pregnant", maturationTimeSeconds);
                    pregnantActor.subspecies.counterReproduction();
                    break;
            }   
            TolUtil.Debug("Sexual ivf successful for "+pActor.getName());

            return BehResult.Continue;
        }
    }
