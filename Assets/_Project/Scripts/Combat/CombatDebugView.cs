using UnityEngine;

namespace Lattirune.Combat
{
    /// <summary>
    /// Development-only on-screen HUD rendering combatant health bars, battle status,
    /// attack logs, and a Start Battle / Reset trigger button.
    /// [DEVELOPMENT / PROTOTYPE ONLY]
    /// </summary>
    public class CombatDebugView : MonoBehaviour
    {
        [SerializeField] private CombatSystem combatSystem;

        private string _lastLog = "Arrange items on grid and tap 'Start Battle'.";

        public void Initialize(CombatSystem system)
        {
            combatSystem = system;

            if (combatSystem != null)
            {
                combatSystem.OnAttackExecuted += HandleAttackExecuted;
                combatSystem.OnVictory += () => _lastLog = ">>> VICTORY! Enemy defeated. <<<";
                combatSystem.OnDefeat += () => _lastLog = ">>> DEFEAT! Player perished. <<<";
            }
        }

        private void OnDestroy()
        {
            if (combatSystem != null)
            {
                combatSystem.OnAttackExecuted -= HandleAttackExecuted;
            }
        }

        private void HandleAttackExecuted(DamageResult damage)
        {
            string bonusText = damage.HasSynergyBonus ? $" (+{damage.RuneBonus} Flame Synergy)" : "";
            _lastLog = $"{damage.SourceName} strikes {damage.TargetName} for {damage.FinalDamage} DMG{bonusText}!";
        }

        private void OnGUI()
        {
            if (combatSystem == null || combatSystem.Player == null || combatSystem.Enemy == null) return;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.fontSize = 14;
            boxStyle.alignment = TextAnchor.UpperLeft;

            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;

            GUILayout.BeginArea(new Rect(20, 20, 360, 260), boxStyle);

            GUILayout.Label($"[COMBAT STATUS: {combatSystem.CurrentState.ToString().ToUpper()}]", headerStyle);
            GUILayout.Space(5);

            // Player Stats
            PlayerCombatant p = combatSystem.Player;
            string synergyNote = p.HasActiveSynergy ? " [FLAMEBOUND EDGE ACTIVE]" : "";
            GUILayout.Label($"Hero HP: {p.CurrentHp}/{p.MaxHp} | Armor: {p.Armor} | ATK: {p.BaseAttackDamage}+{p.ActiveRuneBonus}{synergyNote}");

            // Enemy Stats
            EnemyCombatant e = combatSystem.Enemy;
            GUILayout.Label($"{e.CombatantName} HP: {e.CurrentHp}/{e.MaxHp} | Armor: {e.Armor} | ATK: {e.BaseAttackDamage}");

            GUILayout.Space(8);
            GUILayout.Label($"Log: {_lastLog}");
            GUILayout.Space(8);

            // Battle Control Buttons
            if (combatSystem.CurrentState == CombatState.Preparing)
            {
                if (GUILayout.Button("START BATTLE", GUILayout.Height(35)))
                {
                    combatSystem.StartCombat();
                }
            }
            else if (combatSystem.CurrentState == CombatState.Victory || combatSystem.CurrentState == CombatState.Defeat)
            {
                if (GUILayout.Button("RESET BATTLE", GUILayout.Height(35)))
                {
                    combatSystem.ResetCombat();
                }
            }
            else if (combatSystem.CurrentState == CombatState.Fighting)
            {
                GUILayout.Label(">>> AUTO-BATTLING... <<<");
            }

            GUILayout.EndArea();
        }
    }
}
