using System.Collections.Generic;

namespace Topic_of_Love.Mian.CustomAssets.Traits
{
    public class ActorTraits : BaseTraits<ActorTrait, ActorTraitLibrary>
    {
        public void Init()
        {
            Init("actor", false);
            
            Add(new ActorTrait
            {
                id = "unfluid",
                group_id = "mind",
                rate_birth = 8,
                type = TraitType.Other,
                unlocked_with_achievement = false,
                rarity = Rarity.R1_Rare,
                needs_to_be_explored = true
            });
            Add(new ActorTrait
            {
                id = "intimacy_averse",
                group_id = "mind",
                rate_birth = 2,
                rate_acquire_grow_up = 4,
                type = TraitType.Other,
                unlocked_with_achievement = false,
                rarity = Rarity.R1_Rare,
                needs_to_be_explored = true
            });
            
            Add(new ActorTrait
            {
                id = "faithful",
                group_id = "mind",
                rate_birth = 2,
                rate_acquire_grow_up = 4,
                type = TraitType.Positive,
                unlocked_with_achievement = false,
                rarity = Rarity.R1_Rare,
                needs_to_be_explored = true,
                opposite_traits = new HashSet<ActorTrait>()
            }).addOpposite("unfaithful");
            Add(new ActorTrait
            {
                id = "unfaithful",
                group_id = "mind",
                rate_birth = 2,
                rate_acquire_grow_up = 4,
                type = TraitType.Negative,
                unlocked_with_achievement = false,
                rarity = Rarity.R1_Rare,
                needs_to_be_explored = true,
            }).addOpposite("faithful");
            Finish();
        }

        protected override void Finish()
        {
            foreach (var trait in _assets)
            {
                for (int index = 0; index < trait.rate_birth; ++index)
                    AssetManager.traits.pot_traits_birth.Add(trait);
                for (int index = 0; index < trait.rate_acquire_grow_up; ++index)
                    AssetManager.traits.pot_traits_growup.Add(trait);
                if (trait.in_training_dummy_combat_pot)
                    AssetManager.traits.pot_traits_combat.Add(trait);   
            }
            base.Finish();
        }
    }
}