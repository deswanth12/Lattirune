# POST-PLAN DEVELOPMENT & RUNTIME VALIDATION BACKLOG

## Project Status Overview
- **Engine**: Unity 2022.3.62f1
- **Platform Target**: Android Mobile (Portrait  \times 1920$, $\ge 52\text{ dp}$ touch targets)
- **Target Device**: OnePlus CPH2599 (Android 16 / API 36)
- **Save Version**: 1 (Full backward compatibility maintained)
- **Encryption**: AES-256 Rijndael with deterministic recovery

---

## Complete Player Journey Roadmap & Validation

`
BOOT
  ↓
MAIN MENU (Continue / Start New Run / Campfire Meta-Hub / Settings / Exit)
  ↓
HERO SELECTION (Rune Knight / Elementalist / Shadow Rogue / Iron Juggernaut)
  ↓
DUNGEON MAP DAG (10 Floors, Branching Elite vs Shrine, Vaults, Merchant, Campfire)
  ↓
5x5 LATTICE GRID & INVENTORY (Place Weapon, Place Runes, Connect Conduits, Rotations)
  ↓
INTERACTIVE TUTORIAL OVERLAY (3-Step contextual guidance with skip)
  ↓
START BATTLE & COMBAT (1v1 Auto-Battle, Cooldown ticks, 1x/2x/3x Speed)
  ↓
COMBO SYSTEM & FLOATING TEXT (Live hit counter, Damage multipliers, Crit shakes)
  ↓
ELEMENTAL REACTIONS & EFFECTS (Steam, Plasma, Toxic Flame, Superconduct, Frostbite)
  ↓
VICTORY & REWARDS (Gold drops, Ember drops, Blueprint unlocks, 3-Card Item Draft)
  ↓
PROCEDURAL RUN EVENTS (6 Deterministic floor events & transactional choices)
  ↓
MERCHANT STALL (dynamic offers, 40g grid expansions, 10g rerolls)
  ↓
FLOOR 5 MID-BOSS (Armored Skeleton Champion)
  ↓
FLOOR 8 CAMPFIRE REST (Heal 40% HP / Attune +3 Rune Power / Cleanse Curse)
  ↓
FLOOR 10 THE LICH LORD BOSS (Multi-phase boss, ice row freeze, conduit inversion)
  ↓
OFFLINE MONETIZATION & REVIVE (Opt-in Rewarded Ads, 1-per-run 50% HP Revive)
  ↓
RUN VICTORY / DEFEAT
  ↓
CAMPFIRE META PROGRESSION (Blueprint Forge, Class Unlocks, Ember investment)
  ↓
PERSISTENT SAVE / LOAD (Zero data loss, automatic background saving)
`

---

## Priority Backlog & Audit Status

### [P0] Critical Systems & Crash Prevention
- [x] **P0-1: Startup Scene Verification**: Bootstrap.unity contains TouchController and GridInteractionBootstrap.
- [x] **P0-2: Save File Integrity**: AES Encrypted JSON serialization with safe defaults and SaveVersion = 1 preserved across all new data structures.
- [x] **P0-3: Softlock Elimination**: All battle, event, merchant, and rest screens provide explicit completion or back navigation transitions.

### [P1] Gameplay Integrations & Screen Navigation
- [x] **P1-1: Hero Selection Navigation**: ScreenState.HERO_SELECTION integrated with HeroClassSelectionUIController and CampfireHubController.
- [x] **P1-2: Arcane Codex & Bestiary Navigation**: ScreenState.CODEX integrated with CodexUIController and CampfireHubController.
- [x] **P1-3: Dungeon Map Topology**: DungeonMapScreenController renders 10-floor branching DAG with room preview and selection.
- [x] **P1-4: Floor 8 Campfire Rest**: CampfireRestUIController provides 40% HP heal, rune attunement, and curse cleansing.
- [x] **P1-5: Interactive Onboarding**: TutorialOverlayUIController guides first-time players through grid placement and conduit connection.
- [x] **P1-6: 1-Time Run Revive**: RunManager.RevivePlayer enables 50% HP recovery on defeat with opt-in ad integration.

### [P2] Audio, Juice, and Performance Polish
- [x] **P2-1: Procedural Waveform Audio**: ProceduralAudioSynthesizer generates 11 distinct real-time SFX clips.
- [x] **P2-2: Combat Juice Engine**: FloatingCombatTextPool (32 pooled floaties) and CombatCameraShakeController (Perlin trauma shake).
- [x] **P2-3: Monte Carlo Balance Verification**: MonteCarloBalanceSimulator runs 1,000 automated runs with balanced win curves.

### [P3] Long-Term Balance & Content Expansion
- [x] **P3-1: Additional Relic Blueprints**: Expansion of Forge blueprints to 19 canonical blueprints with unique effects (PotionHealBonus, VampirismBonus, BonusEmberReward, MapVision).
- [x] **P3-2: Endless Dungeon Mode**: Procedural infinite floor scaling beyond Floor 10 with escalating enemy HP/ATK and bonus Ember harvesting.

### [P4] Post-Launch Live-Ops & Future Expansions
- [ ] **P4-1: Daily Challenge Runs**: Seeded daily modifier runs with global asynchronous leaderboards.
- [ ] **P4-2: Alternate Dungeon Biomes**: Volcanic Caldera and Frozen Catacombs biomes.
- [ ] **P4-3: Custom Conduit Shader Particles**: GPU particle trails for laser conduit beam intersections.
