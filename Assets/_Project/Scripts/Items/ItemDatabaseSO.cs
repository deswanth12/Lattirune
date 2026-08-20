using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Items
{
    /// <summary>
    /// Centralized ScriptableObject database containing the complete MVP 1.0 20-Item Catalogue.
    /// Strictly adheres to PLAN.md Section 6.1.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Lattirune/Data/Item Database")]
    public class ItemDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<ItemDataSO> items = new List<ItemDataSO>();

        private readonly Dictionary<string, ItemDataSO> _itemLookup = new Dictionary<string, ItemDataSO>();

        public IReadOnlyList<ItemDataSO> AllItems => items;
        public int TotalItemCount => items != null ? items.Count : 0;

        public void Initialize(List<ItemDataSO> itemList)
        {
            items = itemList ?? new List<ItemDataSO>();
            BuildLookupTable();
        }

        private void OnEnable()
        {
            BuildLookupTable();
        }

        public void BuildLookupTable()
        {
            _itemLookup.Clear();
            if (items == null) return;

            foreach (var item in items)
            {
                if (item != null && !string.IsNullOrEmpty(item.ItemId))
                {
                    if (!_itemLookup.ContainsKey(item.ItemId))
                    {
                        _itemLookup.Add(item.ItemId, item);
                    }
                }
            }
        }

        public void RegisterAlias(ItemDataSO alias)
        {
            if (alias != null && !string.IsNullOrEmpty(alias.ItemId))
            {
                _itemLookup[alias.ItemId] = alias;
            }
        }

        public ItemDataSO GetItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (_itemLookup.Count != (items != null ? items.Count : 0))
            {
                BuildLookupTable();
            }

            if (_itemLookup.TryGetValue(id, out var item))
            {
                return item;
            }

            // Fallback search in list
            if (items != null)
            {
                return items.Find(x => x != null && x.ItemId == id);
            }

            return null;
        }

        public bool HasItem(string id)
        {
            return GetItem(id) != null;
        }

        public bool IsValid(out string error)
        {
            if (items == null || items.Count == 0)
            {
                error = "Item database cannot be empty.";
                return false;
            }

            HashSet<string> seenIds = new HashSet<string>();
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null)
                {
                    error = $"Null item reference at index {i}.";
                    return false;
                }
                if (!it.IsValid(out string itemErr))
                {
                    error = $"Item '{it.ItemId}' at index {i} is invalid: {itemErr}";
                    return false;
                }
                if (seenIds.Contains(it.ItemId))
                {
                    error = $"Duplicate item ID detected: '{it.ItemId}'.";
                    return false;
                }
                seenIds.Add(it.ItemId);
            }

            error = null;
            return true;
        }

        public bool ValidateDatabase(out List<string> errors)
        {
            errors = new List<string>();
            if (!IsValid(out string err))
            {
                if (!string.IsNullOrEmpty(err)) errors.Add(err);
                return false;
            }
            return true;
        }

        public bool ValidateDatabase() => IsValid(out _);

        /// <summary>
        /// Creates the complete canonical MVP 1.0 20-Item Database specified in PLAN.md Section 6.1.
        /// </summary>
        public static ItemDatabaseSO CreateCanonicalDatabase()
        {
            ItemDatabaseSO db = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            List<ItemDataSO> list = new List<ItemDataSO>();

            // 1. Rusty Dagger (Weapon, 1x1, 4 Dmg, 0.8s)
            ItemDataSO dagger = ScriptableObject.CreateInstance<ItemDataSO>();
            dagger.Initialize("item_rusty_dagger", "Rusty Dagger", "4 Dmg | 0.8s cd. Synergy: +2 Dmg if placed in corner.",
                ItemCategory.Weapon, new Vector2Int(1, 1), canRotate: false, new Color(0.6f, 0.6f, 0.6f),
                damage: 4, cd: 0.8f);
            list.Add(dagger);

            // 2. Iron Broadsword (Weapon, 1x2, 10 Dmg, 2.0s)
            ItemDataSO broadsword = ScriptableObject.CreateInstance<ItemDataSO>();
            broadsword.Initialize("item_iron_broadsword", "Iron Broadsword", "10 Dmg | 2.0s cd. Synergy: +3 Dmg for each adjacent weapon.",
                ItemCategory.Weapon, new Vector2Int(1, 2), canRotate: true, new Color(0.85f, 0.85f, 0.9f),
                damage: 10, cd: 2.0f);
            list.Add(broadsword);

            // 3. Shortbow (Weapon, 2x1, 6 Dmg, 1.4s, 5 Armor Pierce)
            ItemDataSO shortbow = ScriptableObject.CreateInstance<ItemDataSO>();
            shortbow.Initialize("item_shortbow", "Shortbow", "6 Dmg | 1.4s cd. Synergy: Attacks pierce 5 armor.",
                ItemCategory.Weapon, new Vector2Int(2, 1), canRotate: true, new Color(0.7f, 0.45f, 0.2f),
                damage: 6, cd: 1.4f, pierce: 5);
            list.Add(shortbow);

            // 4. Apprentice Wand (Weapon, 1x2, 7 Dmg, 1.8s, +50% rune damage)
            ItemDataSO wand = ScriptableObject.CreateInstance<ItemDataSO>();
            wand.Initialize("item_apprentice_wand", "Apprentice Wand", "7 Dmg | 1.8s cd. Synergy: +50% elemental rune damage.",
                ItemCategory.Weapon, new Vector2Int(1, 2), canRotate: true, new Color(0.4f, 0.8f, 0.9f),
                damage: 7, cd: 1.8f, runeMod: 0.5f);
            list.Add(wand);

            // 5. Battleaxe (Weapon, L-Shape / 2x2, 18 Dmg, 3.0s)
            ItemDataSO battleaxe = ScriptableObject.CreateInstance<ItemDataSO>();
            battleaxe.Initialize("item_battleaxe", "Battleaxe", "18 Dmg | 3.0s cd. Deals 1.5x damage if shield is 0.",
                ItemCategory.Weapon, new Vector2Int(2, 2), canRotate: true, new Color(0.8f, 0.2f, 0.2f),
                damage: 18, cd: 3.0f, lShape: true);
            list.Add(battleaxe);

            // 6. Phalanx Spear (Weapon, 1x3, 12 Dmg, 1.8s)
            ItemDataSO spear = ScriptableObject.CreateInstance<ItemDataSO>();
            spear.Initialize("item_phalanx_spear", "Phalanx Spear", "12 Dmg | 1.8s cd. +4 Dmg for empty tiles behind shaft.",
                ItemCategory.Weapon, new Vector2Int(1, 3), canRotate: true, new Color(0.75f, 0.75f, 0.5f),
                damage: 12, cd: 1.8f);
            list.Add(spear);

            // 7. Wooden Buckler (Shield, 1x1, 8 Shield)
            ItemDataSO buckler = ScriptableObject.CreateInstance<ItemDataSO>();
            buckler.Initialize("item_wooden_buckler", "Wooden Buckler", "8 Shield at start of battle.",
                ItemCategory.Shield, new Vector2Int(1, 1), canRotate: false, new Color(0.55f, 0.35f, 0.15f),
                shield: 8);
            list.Add(buckler);

            // 8. Iron Tower Shield (Shield, 2x2, 25 Shield)
            ItemDataSO towerShield = ScriptableObject.CreateInstance<ItemDataSO>();
            towerShield.Initialize("item_iron_tower_shield", "Iron Tower Shield", "25 Shield at start of battle.",
                ItemCategory.Shield, new Vector2Int(2, 2), canRotate: true, new Color(0.3f, 0.4f, 0.6f),
                shield: 25);
            list.Add(towerShield);

            // 9. Spiked Buckler (Shield, 1x2, 12 Shield, 4 Thorns)
            ItemDataSO spikedBuckler = ScriptableObject.CreateInstance<ItemDataSO>();
            spikedBuckler.Initialize("item_spiked_buckler", "Spiked Buckler", "12 Shield; reflects 4 Thorns damage when struck.",
                ItemCategory.Shield, new Vector2Int(1, 2), canRotate: true, new Color(0.5f, 0.5f, 0.4f),
                shield: 12, thorns: 4);
            list.Add(spikedBuckler);

            // 10. Leather Tunic (Armor, 2x2, +25 Max HP)
            ItemDataSO leatherTunic = ScriptableObject.CreateInstance<ItemDataSO>();
            leatherTunic.Initialize("item_leather_tunic", "Leather Tunic", "+25 Max HP; +10 HP per adjacent potion.",
                ItemCategory.Armor, new Vector2Int(2, 2), canRotate: true, new Color(0.6f, 0.4f, 0.25f),
                hpBonus: 25);
            list.Add(leatherTunic);

            // 11. Chainmail Coat (Armor, 2x2, +15 Max HP, -2 damage taken)
            ItemDataSO chainmail = ScriptableObject.CreateInstance<ItemDataSO>();
            chainmail.Initialize("item_chainmail_coat", "Chainmail Coat", "+15 Max HP; reduces all incoming damage by 2 flat.",
                ItemCategory.Armor, new Vector2Int(2, 2), canRotate: true, new Color(0.7f, 0.7f, 0.75f),
                hpBonus: 15, dmgReduction: 2);
            list.Add(chainmail);

            // 12. Whetstone (Relic, 1x1, +3 flat Base Damage to adjacent blades)
            ItemDataSO whetstone = ScriptableObject.CreateInstance<ItemDataSO>();
            whetstone.Initialize("item_whetstone", "Whetstone", "All adjacent bladed weapons gain +3 flat Base Damage.",
                ItemCategory.Relic, new Vector2Int(1, 1), canRotate: false, new Color(0.45f, 0.45f, 0.45f),
                flatBonus: 3);
            list.Add(whetstone);

            // 13. Ruby Ring (Relic, 1x1, +25% burn duration to adjacent Fire Runes)
            ItemDataSO rubyRing = ScriptableObject.CreateInstance<ItemDataSO>();
            rubyRing.Initialize("item_ruby_ring", "Ruby Ring", "Adjacent Fire Runes gain +25% burn duration.",
                ItemCategory.Relic, new Vector2Int(1, 1), canRotate: false, new Color(0.9f, 0.1f, 0.2f));
            list.Add(rubyRing);

            // 14. Sapphire Ring (Relic, 1x1, +25% slow potency to adjacent Ice Runes)
            ItemDataSO sapphireRing = ScriptableObject.CreateInstance<ItemDataSO>();
            sapphireRing.Initialize("item_sapphire_ring", "Sapphire Ring", "Adjacent Ice Runes gain +25% slow potency.",
                ItemCategory.Relic, new Vector2Int(1, 1), canRotate: false, new Color(0.1f, 0.4f, 0.9f));
            list.Add(sapphireRing);

            // 15. Lucky Clover (Relic, 1x1, +10% Critical Strike Chance)
            ItemDataSO clover = ScriptableObject.CreateInstance<ItemDataSO>();
            clover.Initialize("item_lucky_clover", "Lucky Clover", "+10% Critical Strike Chance.",
                ItemCategory.Relic, new Vector2Int(1, 1), canRotate: false, new Color(0.15f, 0.8f, 0.25f),
                crit: 0.10f);
            list.Add(clover);

            // 16. Health Potion (Consumable, 1x1, +35 HP below 30%)
            ItemDataSO healthPotion = ScriptableObject.CreateInstance<ItemDataSO>();
            healthPotion.Initialize("item_health_potion", "Health Potion", "Auto-drinks at < 30% HP; restores 35 HP.",
                ItemCategory.Consumable, new Vector2Int(1, 1), canRotate: false, new Color(0.95f, 0.2f, 0.3f));
            list.Add(healthPotion);

            // 17. Stamina Flask (Consumable, 1x1, +40% speed for 4s)
            ItemDataSO staminaFlask = ScriptableObject.CreateInstance<ItemDataSO>();
            staminaFlask.Initialize("item_stamina_flask", "Stamina Flask", "Auto-drinks at battle start; +40% speed for 4 seconds.",
                ItemCategory.Consumable, new Vector2Int(1, 1), canRotate: false, new Color(0.95f, 0.75f, 0.1f));
            list.Add(staminaFlask);

            // 18. Poison Vial (Consumable, 1x1, inflicts 15 Poison)
            ItemDataSO poisonVial = ScriptableObject.CreateInstance<ItemDataSO>();
            poisonVial.Initialize("item_poison_vial", "Poison Vial", "Breaks on first hit taken; inflicts 15 Poison on attacker.",
                ItemCategory.Consumable, new Vector2Int(1, 1), canRotate: false, new Color(0.2f, 0.85f, 0.35f));
            list.Add(poisonVial);

            // 19. Decaying Blade (Cursed Weapon, 1x2, 22 Dmg, 1.2s)
            ItemDataSO decayingBlade = ScriptableObject.CreateInstance<ItemDataSO>();
            decayingBlade.Initialize("item_decaying_blade", "Decaying Blade", "22 Dmg | 1.2s cd. Cursed: Deals 2 dmg to adjacent items every 3s.",
                ItemCategory.Weapon, new Vector2Int(1, 2), canRotate: true, new Color(0.35f, 0.2f, 0.4f),
                damage: 22, cd: 1.2f, cursed: true);
            list.Add(decayingBlade);

            // 20. Blood Shield (Cursed Shield, 2x2, 45 Shield)
            ItemDataSO bloodShield = ScriptableObject.CreateInstance<ItemDataSO>();
            bloodShield.Initialize("item_blood_shield", "Blood Shield", "45 Shield. Cursed: Reduces all healing by 50%.",
                ItemCategory.Shield, new Vector2Int(2, 2), canRotate: true, new Color(0.65f, 0.1f, 0.15f),
                shield: 45, cursed: true);
            list.Add(bloodShield);

            // 21. Burning Core (Build-Defining Relic, 1x1, +35% Fire Reaction Damage & Ignite)
            ItemDataSO burningCore = ScriptableObject.CreateInstance<ItemDataSO>();
            burningCore.Initialize("item_burning_core", "Burning Core", "Fire reactions deal +35% DMG and ignite target for 3s.",
                ItemCategory.Relic, new Vector2Int(1, 1), canRotate: false, new Color(1.0f, 0.35f, 0.1f), flatBonus: 5);
            list.Add(burningCore);

            // 22. Unstable Prism (Build-Defining Relic, 1x1, +2 Chain Reaction Window)
            ItemDataSO unstablePrism = ScriptableObject.CreateInstance<ItemDataSO>();
            unstablePrism.Initialize("item_unstable_prism", "Unstable Prism", "Reaction chains gain +2s combo window and trigger cascading bursts.",
                ItemCategory.Relic, new Vector2Int(1, 1), canRotate: false, new Color(0.85f, 0.4f, 0.95f), flatBonus: 4);
            list.Add(unstablePrism);

            // 23. Blood Sigil (Build-Defining Relic, 1x1, Lifesteal on Reactions/Crits)
            ItemDataSO bloodSigil = ScriptableObject.CreateInstance<ItemDataSO>();
            bloodSigil.Initialize("item_blood_sigil", "Blood Sigil", "Restores 4 HP whenever a critical hit or elemental reaction triggers.",
                ItemCategory.Relic, new Vector2Int(1, 1), canRotate: false, new Color(0.8f, 0.15f, 0.2f), hpBonus: 10);
            list.Add(bloodSigil);

            // 24. Void Catalyst (Build-Defining Relic, 1x1, 5-Combo Void Collapse)
            ItemDataSO voidCatalyst = ScriptableObject.CreateInstance<ItemDataSO>();
            voidCatalyst.Initialize("item_void_catalyst", "Void Catalyst", "Every 5x combo triggers a Dark Void collapse dealing 30 flat AoE damage.",
                ItemCategory.Relic, new Vector2Int(1, 1), canRotate: false, new Color(0.4f, 0.1f, 0.7f), flatBonus: 6);
            list.Add(voidCatalyst);

            // 25. Glacial Matrix (Build-Defining Relic, 1x1, Water+Ice Barrier)
            ItemDataSO glacialMatrix = ScriptableObject.CreateInstance<ItemDataSO>();
            glacialMatrix.Initialize("item_glacial_matrix", "Glacial Matrix", "Water + Ice reactions grant +12 temporary Armor barrier.",
                ItemCategory.Relic, new Vector2Int(1, 1), canRotate: false, new Color(0.2f, 0.75f, 0.95f), shield: 12);
            list.Add(glacialMatrix);

            // 26. Thunderstruck Coil (Build-Defining Relic, 1x1, Lightning Double Strike & Crit)
            ItemDataSO thunderCoil = ScriptableObject.CreateInstance<ItemDataSO>();
            thunderCoil.Initialize("item_thunderstruck_coil", "Thunderstruck Coil", "Lightning reactions strike twice and grant +50% Crit Damage.",
                ItemCategory.Relic, new Vector2Int(1, 1), canRotate: false, new Color(0.95f, 0.85f, 0.2f), crit: 0.15f);
            list.Add(thunderCoil);

            db.Initialize(list);

            // Prototype aliases for backwards compatibility
            ItemDataSO protoSword = ScriptableObject.CreateInstance<ItemDataSO>();
            protoSword.Initialize("item_training_sword", "Training Sword", "A reliable iron training sword.",
                ItemCategory.Weapon, new Vector2Int(1, 2), true, new Color(0.9f, 0.5f, 0.1f), damage: 10, cd: 2.0f);
            db.RegisterAlias(protoSword);

            ItemDataSO protoEmber = ScriptableObject.CreateInstance<ItemDataSO>();
            protoEmber.Initialize("item_ember_blade", "Ember Blade", "A blade glowing with stored heat.",
                ItemCategory.Weapon, new Vector2Int(2, 1), true, new Color(0.91f, 0.3f, 0.24f), damage: 12, cd: 1.6f);
            db.RegisterAlias(protoEmber);

            ItemDataSO protoGuard = ScriptableObject.CreateInstance<ItemDataSO>();
            protoGuard.Initialize("item_guard_plate", "Guard Plate", "A reinforced defensive chestplate.",
                ItemCategory.Shield, new Vector2Int(2, 2), true, new Color(0.2f, 0.6f, 0.86f), shield: 20);
            db.RegisterAlias(protoGuard);

            ItemDataSO protoRelic = ScriptableObject.CreateInstance<ItemDataSO>();
            protoRelic.Initialize("item_arcane_relic", "Arcane Relic", "Ancient artifact vibrating with energy.",
                ItemCategory.Relic, new Vector2Int(1, 1), false, new Color(0.61f, 0.35f, 0.71f), flatBonus: 4);
            db.RegisterAlias(protoRelic);

            ItemDataSO protoFlask = ScriptableObject.CreateInstance<ItemDataSO>();
            protoFlask.Initialize("item_vital_flask", "Vital Flask", "Restorative dungeon potion.",
                ItemCategory.Consumable, new Vector2Int(1, 1), false, new Color(0.18f, 0.8f, 0.44f));
            db.RegisterAlias(protoFlask);

            return db;
        }
    }
}
