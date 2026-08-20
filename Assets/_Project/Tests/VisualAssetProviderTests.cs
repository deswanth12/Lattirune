using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Progression;
using Lattirune.UI;

namespace Lattirune.Tests
{
    [TestFixture]
    public class VisualAssetProviderTests
    {
        [Test]
        public void Heroes_ResolveNonEmptyTextures()
        {
            Texture2D knight = VisualAssetProvider.GetHeroTexture(HeroClassType.RuneKnight);
            Texture2D mage = VisualAssetProvider.GetHeroTexture(HeroClassType.Elementalist);
            Texture2D rogue = VisualAssetProvider.GetHeroTexture(HeroClassType.ShadowRogue);
            Texture2D jugg = VisualAssetProvider.GetHeroTexture(HeroClassType.IronJuggernaut);

            Assert.IsNotNull(knight, "Rune Knight texture should not be null");
            Assert.IsNotNull(mage, "Elementalist texture should not be null");
            Assert.IsNotNull(rogue, "Shadow Rogue texture should not be null");
            Assert.IsNotNull(jugg, "Iron Juggernaut texture should not be null");
        }

        [Test]
        public void Enemies_ResolveNonEmptyTextures()
        {
            string[] enemies = { "Sewer Rat", "Goblin Thief", "Acid Slime", "Sewer Witch", "Armored Skeleton", "Drain Hydra", "Shadow Assassin", "Flesh Behemoth" };
            foreach (var name in enemies)
            {
                Texture2D tex = VisualAssetProvider.GetEnemyTexture(name, isBoss: false);
                Assert.IsNotNull(tex, $"Enemy texture for '{name}' should not be null");
            }
        }

        [Test]
        public void Bosses_ResolveAllPhases()
        {
            Texture2D goliathP1 = VisualAssetProvider.GetEnemyTexture("Grave Goliath", isBoss: true, phase: 1);
            Texture2D goliathP2 = VisualAssetProvider.GetEnemyTexture("Grave Goliath", isBoss: true, phase: 2);
            Texture2D lichP1 = VisualAssetProvider.GetEnemyTexture("The Lich Lord", isBoss: true, phase: 1);
            Texture2D lichP2 = VisualAssetProvider.GetEnemyTexture("The Lich Lord", isBoss: true, phase: 2);
            Texture2D lichP3 = VisualAssetProvider.GetEnemyTexture("The Lich Lord", isBoss: true, phase: 3);

            Assert.IsNotNull(goliathP1, "Grave Goliath P1 texture should not be null");
            Assert.IsNotNull(goliathP2, "Grave Goliath P2 texture should not be null");
            Assert.IsNotNull(lichP1, "Lich Lord P1 texture should not be null");
            Assert.IsNotNull(lichP2, "Lich Lord P2 texture should not be null");
            Assert.IsNotNull(lichP3, "Lich Lord P3 texture should not be null");
        }

        [Test]
        public void Runes_ResolveAllElements()
        {
            ElementType[] elements = {
                ElementType.Fire, ElementType.Ice, ElementType.Lightning,
                ElementType.Poison, ElementType.Earth, ElementType.Light,
                ElementType.Shadow, ElementType.Wind, ElementType.Force, ElementType.Physical
            };

            foreach (var elem in elements)
            {
                Texture2D tex = VisualAssetProvider.GetRuneTexture(elem);
                Assert.IsNotNull(tex, $"Rune texture for '{elem}' should not be null");
            }
        }

        [Test]
        public void Items_ResolveStandardCatalogue()
        {
            string[] items = {
                "item_training_sword", "item_iron_broadsword", "item_ember_blade",
                "item_guard_plate", "item_arcane_relic", "item_health_potion",
                "item_gold_coin", "item_soul_ember"
            };

            foreach (var item in items)
            {
                Texture2D tex = VisualAssetProvider.GetItemTexture(item);
                Assert.IsNotNull(tex, $"Item texture for '{item}' should not be null");
            }
        }

        [Test]
        public void UIIcons_ResolveAllCommonIcons()
        {
            string[] icons = {
                "ui_icon_hp", "ui_icon_armor", "ui_icon_attack", "ui_icon_gold",
                "ui_icon_embers", "ui_icon_floor", "ui_icon_battle", "ui_icon_elite",
                "ui_icon_merchant", "ui_icon_campfire", "ui_icon_event", "ui_icon_boss",
                "ui_icon_victory", "ui_icon_death", "ui_icon_settings", "ui_icon_inventory"
            };

            foreach (var icon in icons)
            {
                Texture2D tex = VisualAssetProvider.GetUIIcon(icon);
                Assert.IsNotNull(tex, $"UI Icon texture for '{icon}' should not be null");
            }
        }
    }
}
