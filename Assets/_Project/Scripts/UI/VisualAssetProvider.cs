using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Progression;

namespace Lattirune.UI
{
    /// <summary>
    /// Centralized high-performance visual asset provider and cache.
    /// Provides instant access to heroes, enemies, bosses, runes, items, UI icons, and backdrops.
    /// </summary>
    public static class VisualAssetProvider
    {
        private static readonly Dictionary<string, Texture2D> s_TextureCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        // =========================================================================
        // 1. HERO VISUALS
        // =========================================================================
        public static Texture2D GetHeroTexture(string heroId)
        {
            string cleanId = NormalizeHeroId(heroId);
            return LoadArtTexture("Heroes/" + cleanId, "Heroes/hero_rune_knight");
        }

        public static Texture2D GetHeroTexture(HeroClassType heroClass)
        {
            switch (heroClass)
            {
                case HeroClassType.RuneKnight: return GetHeroTexture("hero_rune_knight");
                case HeroClassType.Elementalist: return GetHeroTexture("hero_elementalist");
                case HeroClassType.ShadowRogue: return GetHeroTexture("hero_shadow_rogue");
                case HeroClassType.IronJuggernaut: return GetHeroTexture("hero_iron_juggernaut");
                default: return GetHeroTexture("hero_rune_knight");
            }
        }

        private static string NormalizeHeroId(string id)
        {
            if (string.IsNullOrEmpty(id)) return "hero_rune_knight";
            string lower = id.ToLowerInvariant();
            if (lower.Contains("elementalist")) return "hero_elementalist";
            if (lower.Contains("shadow") || lower.Contains("rogue") || lower.Contains("blade")) return "hero_shadow_rogue";
            if (lower.Contains("juggernaut") || lower.Contains("iron") || lower.Contains("valkyrie")) return "hero_iron_juggernaut";
            return "hero_rune_knight";
        }

        // =========================================================================
        // 2. ENEMY & BOSS VISUALS
        // =========================================================================
        public static Texture2D GetEnemyTexture(string enemyName, bool isBoss = false, int phase = 1)
        {
            if (string.IsNullOrEmpty(enemyName)) return LoadArtTexture("Enemies/enemy_sewer_rat", "Enemies/enemy_sewer_rat");
            string lower = enemyName.ToLowerInvariant();

            // Boss Check
            if (isBoss || lower.Contains("goliath") || lower.Contains("lich"))
            {
                if (lower.Contains("goliath"))
                {
                    return phase >= 2 
                        ? LoadArtTexture("Bosses/boss_grave_goliath_p2", "Bosses/boss_grave_goliath_p1")
                        : LoadArtTexture("Bosses/boss_grave_goliath_p1", "Bosses/boss_grave_goliath_p1");
                }
                if (lower.Contains("lich"))
                {
                    if (phase == 2) return LoadArtTexture("Bosses/boss_lich_lord_p2", "Bosses/boss_lich_lord_p1");
                    if (phase >= 3) return LoadArtTexture("Bosses/boss_lich_lord_p3", "Bosses/boss_lich_lord_p1");
                    return LoadArtTexture("Bosses/boss_lich_lord_p1", "Bosses/boss_lich_lord_p1");
                }
            }

            // Normal & Elite Enemies
            if (lower.Contains("rat")) return LoadArtTexture("Enemies/enemy_sewer_rat", "Enemies/enemy_sewer_rat");
            if (lower.Contains("goblin")) return LoadArtTexture("Enemies/enemy_goblin_thief", "Enemies/enemy_goblin_thief");
            if (lower.Contains("slime")) return LoadArtTexture("Enemies/enemy_acid_slime", "Enemies/enemy_acid_slime");
            if (lower.Contains("witch")) return LoadArtTexture("Enemies/enemy_sewer_witch", "Enemies/enemy_sewer_witch");
            if (lower.Contains("skeleton")) return LoadArtTexture("Enemies/enemy_armored_skeleton", "Enemies/enemy_armored_skeleton");
            if (lower.Contains("hydra")) return LoadArtTexture("Enemies/enemy_drain_hydra", "Enemies/enemy_drain_hydra");
            if (lower.Contains("assassin")) return LoadArtTexture("Enemies/enemy_shadow_assassin", "Enemies/enemy_shadow_assassin");
            if (lower.Contains("behemoth")) return LoadArtTexture("Enemies/enemy_flesh_behemoth", "Enemies/enemy_flesh_behemoth");

            return LoadArtTexture("Enemies/enemy_sewer_rat", "Enemies/enemy_sewer_rat");
        }

        // =========================================================================
        // 3. RUNE VISUALS
        // =========================================================================
                public static Texture2D GetRuneTexture(ElementType element)
        {
            switch (element)
            {
                case ElementType.Fire: return LoadArtTexture("Runes/rune_fire", "Runes/rune_fire");
                case ElementType.Ice: return LoadArtTexture("Runes/rune_frost", "Runes/rune_frost");
                case ElementType.Lightning: return LoadArtTexture("Runes/rune_lightning", "Runes/rune_lightning");
                case ElementType.Poison:
                case ElementType.Earth: return LoadArtTexture("Runes/rune_nature", "Runes/rune_nature");
                case ElementType.Light: return LoadArtTexture("Runes/rune_light", "Runes/rune_light");
                case ElementType.Shadow: return LoadArtTexture("Runes/rune_shadow", "Runes/rune_shadow");
                case ElementType.Wind:
                case ElementType.Force:
                case ElementType.Physical: return LoadArtTexture("Runes/rune_void", "Runes/rune_void");
                default: return LoadArtTexture("Runes/rune_fire", "Runes/rune_fire");
            }
        }

        public static Texture2D GetConduitArrowTexture(string direction)
        {
            string lower = (direction ?? "north").ToLowerInvariant();
            if (lower.Contains("east")) return LoadArtTexture("Runes/conduit_arrow_east", "Runes/conduit_arrow_north");
            if (lower.Contains("south")) return LoadArtTexture("Runes/conduit_arrow_south", "Runes/conduit_arrow_north");
            if (lower.Contains("west")) return LoadArtTexture("Runes/conduit_arrow_west", "Runes/conduit_arrow_north");
            return LoadArtTexture("Runes/conduit_arrow_north", "Runes/conduit_arrow_north");
        }

        // =========================================================================
        // 4. ITEM VISUALS
        // =========================================================================
        public static Texture2D GetItemTexture(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return LoadArtTexture("Items/item_iron_broadsword", "Items/item_iron_broadsword");
            string lower = itemId.ToLowerInvariant();

            if (lower.Contains("training")) return LoadArtTexture("Items/item_training_sword", "Items/item_iron_broadsword");
            if (lower.Contains("ember")) return LoadArtTexture("Items/item_ember_blade", "Items/item_iron_broadsword");
            if (lower.Contains("decay")) return LoadArtTexture("Items/item_decaying_blade", "Items/item_iron_broadsword");
            if (lower.Contains("scythe") || lower.Contains("frost")) return LoadArtTexture("Items/item_frost_scythe", "Items/item_iron_broadsword");
            if (lower.Contains("hammer") || lower.Contains("storm")) return LoadArtTexture("Items/item_storm_hammer", "Items/item_iron_broadsword");
            if (lower.Contains("dagger") || lower.Contains("void")) return LoadArtTexture("Items/item_void_dagger", "Items/item_iron_broadsword");
            if (lower.Contains("spear") || lower.Contains("sunfire")) return LoadArtTexture("Items/item_sunfire_spear", "Items/item_iron_broadsword");

            if (lower.Contains("buckler") || lower.Contains("wooden")) return LoadArtTexture("Items/item_wooden_buckler", "Items/item_guard_plate");
            if (lower.Contains("guard")) return LoadArtTexture("Items/item_guard_plate", "Items/item_guard_plate");
            if (lower.Contains("chainmail")) return LoadArtTexture("Items/item_chainmail_coat", "Items/item_guard_plate");
            if (lower.Contains("aegis")) return LoadArtTexture("Items/item_ice_ward_aegis", "Items/item_guard_plate");
            if (lower.Contains("robe")) return LoadArtTexture("Items/item_robe_of_arcane", "Items/item_guard_plate");
            if (lower.Contains("dread")) return LoadArtTexture("Items/item_dread_plate", "Items/item_guard_plate");

            if (lower.Contains("arcane")) return LoadArtTexture("Items/item_arcane_relic", "Items/item_arcane_relic");
            if (lower.Contains("clover")) return LoadArtTexture("Items/item_lucky_clover", "Items/item_arcane_relic");
            if (lower.Contains("dragon") || lower.Contains("heart")) return LoadArtTexture("Items/item_dragon_heart", "Items/item_arcane_relic");
            if (lower.Contains("fang")) return LoadArtTexture("Items/item_vampire_fang", "Items/item_arcane_relic");
            if (lower.Contains("prism")) return LoadArtTexture("Items/item_prism_lens", "Items/item_arcane_relic");
            if (lower.Contains("idol")) return LoadArtTexture("Items/item_ancient_idol", "Items/item_ancient_idol");

            if (lower.Contains("health") || lower.Contains("vital")) return LoadArtTexture("Items/item_health_potion", "Items/item_health_potion");
            if (lower.Contains("mana")) return LoadArtTexture("Items/item_mana_flask", "Items/item_health_potion");
            if (lower.Contains("fury") || lower.Contains("elixir")) return LoadArtTexture("Items/item_elixir_fury", "Items/item_health_potion");
            if (lower.Contains("stoneskin")) return LoadArtTexture("Items/item_stoneskin_draught", "Items/item_health_potion");

            if (lower.Contains("gold") || lower.Contains("coin")) return LoadArtTexture("Items/item_gold_coin", "Items/item_gold_coin");
            if (lower.Contains("ember") || lower.Contains("soul")) return LoadArtTexture("Items/item_soul_ember", "Items/item_soul_ember");

            return LoadArtTexture("Items/item_iron_broadsword", "Items/item_iron_broadsword");
        }

        // =========================================================================
        // 5. UI ICONS
        // =========================================================================
        public static Texture2D GetUIIcon(string iconId)
        {
            string lower = (iconId ?? "hp").ToLowerInvariant();
            if (!lower.StartsWith("ui_icon_")) lower = "ui_icon_" + lower;
            return LoadArtTexture("UI/" + lower, "UI/ui_icon_hp");
        }

        // =========================================================================
        // 6. BACKDROPS & ENVIRONMENTS
        // =========================================================================
        public static Texture2D GetBackdrop(string backdropId)
        {
            string lower = (backdropId ?? "mainmenu").ToLowerInvariant();
            if (!lower.StartsWith("bg_")) lower = "bg_" + lower;
            return LoadArtTexture("Environment/" + lower, "Environment/bg_mainmenu");
        }

        // =========================================================================
        // INTERNAL LOADER & TEXTURE CACHE
        // =========================================================================
        private static Texture2D LoadArtTexture(string resourcePath, string fallbackPath)
        {
            if (s_TextureCache.TryGetValue(resourcePath, out Texture2D cached) && cached != null)
            {
                return cached;
            }

            Texture2D tex = Resources.Load<Texture2D>("Art/" + resourcePath);
            if (tex == null)
            {
                Sprite sprite = Resources.Load<Sprite>("Art/" + resourcePath);
                if (sprite != null) tex = sprite.texture;
            }

            if (tex == null && !string.IsNullOrEmpty(fallbackPath) && fallbackPath != resourcePath)
            {
                tex = Resources.Load<Texture2D>("Art/" + fallbackPath);
            }

            if (tex == null)
            {
                tex = CreateSolidFallbackTexture();
            }

            s_TextureCache[resourcePath] = tex;
            return tex;
        }

        private static Texture2D CreateSolidFallbackTexture()
        {
            Texture2D fallback = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color gold = new Color(0.85f, 0.65f, 0.2f, 1f);
            Color[] cols = new Color[64 * 64];
            for (int i = 0; i < cols.Length; i++) cols[i] = gold;
            fallback.SetPixels(cols);
            fallback.Apply();
            return fallback;
        }
    }
}
