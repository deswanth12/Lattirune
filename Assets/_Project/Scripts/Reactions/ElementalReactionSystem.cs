using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Runes;

namespace Lattirune.Reactions
{
    /// <summary>
    /// Evaluates 2-beam cross-intersections on the LatticeGrid and activates elemental reactions.
    /// Supports Prism-split beam branches and standard straight conduits.
    /// </summary>
    public class ElementalReactionSystem : MonoBehaviour
    {
        [Header("Reaction Database")]
        [SerializeField] private ElementalReactionDatabaseSO reactionDatabase;

        private readonly Dictionary<string, ElementalReactionResult> _activeReactionsByKey = new Dictionary<string, ElementalReactionResult>();

        public event Action<ElementalReactionResult> OnReactionActivated;
        public event Action<ElementalReactionResult> OnReactionDeactivated;

        public ElementalReactionDatabaseSO Database => reactionDatabase;
        public IReadOnlyCollection<ElementalReactionResult> ActiveReactions => _activeReactionsByKey.Values;
        public int ActiveReactionCount => _activeReactionsByKey.Count;

        private void Awake()
        {
            EnsureDefaultDefinitions();
        }

        public void Initialize(ElementalReactionDatabaseSO database)
        {
            reactionDatabase = database;
            EnsureDefaultDefinitions();
        }

        public void EnsureDefaultDefinitions()
        {
            if (reactionDatabase == null)
            {
                reactionDatabase = ElementalReactionDatabaseSO.CreateDefaultDatabase();
            }
        }

        /// <summary>
        /// Recalculates all beam intersections and updates active elemental reactions from ConduitBeamPath instances.
        /// </summary>
        public void UpdateReactions(IReadOnlyList<ConduitBeamPath> activeBeams)
        {
            EnsureDefaultDefinitions();

            List<BeamIntersection> intersections = ElementalIntersectionEngine.FindIntersections(activeBeams);
            Dictionary<string, ElementalReactionResult> newlyActiveReactions = new Dictionary<string, ElementalReactionResult>();

            for (int i = 0; i < intersections.Count; i++)
            {
                BeamIntersection inter = intersections[i];
                ElementalReactionDefinitionSO match = reactionDatabase.FindReaction(inter.ElementA, inter.ElementB);

                if (match != null)
                {
                    string key = $"{inter.GridCoordinate.x}_{inter.GridCoordinate.y}_{inter.RuneAId}_{inter.RuneBId}";
                    ElementalReactionResult result = ElementalReactionResult.CreateActive(match, inter);
                    newlyActiveReactions[key] = result;
                }
            }

            // 1. Detect deactivations
            List<string> toDeactivate = new List<string>();
            foreach (var kvp in _activeReactionsByKey)
            {
                if (!newlyActiveReactions.ContainsKey(kvp.Key))
                {
                    toDeactivate.Add(kvp.Key);
                }
            }

            foreach (string key in toDeactivate)
            {
                ElementalReactionResult prev = _activeReactionsByKey[key];
                _activeReactionsByKey.Remove(key);
                OnReactionDeactivated?.Invoke(prev);
            }

            // 2. Detect activations
            foreach (var kvp in newlyActiveReactions)
            {
                string key = kvp.Key;
                ElementalReactionResult result = kvp.Value;

                bool wasAlreadyActive = _activeReactionsByKey.ContainsKey(key);
                _activeReactionsByKey[key] = result;

                if (!wasAlreadyActive)
                {
                    OnReactionActivated?.Invoke(result);
                }
            }
        }

        /// <summary>
        /// Legacy overload recalculating reactions from single-conduit results.
        /// </summary>
        public void UpdateReactions(IReadOnlyList<(RuneData rune, Vector2Int origin, RuneConduitResult conduit)> activeConduits)
        {
            List<ConduitBeamPath> beamPaths = new List<ConduitBeamPath>();
            if (activeConduits != null)
            {
                for (int i = 0; i < activeConduits.Count; i++)
                {
                    var (rune, origin, conduit) = activeConduits[i];
                    if (rune == null || !rune.IsActive || conduit == null) continue;

                    beamPaths.Add(new ConduitBeamPath(
                        beamId: $"beam_{rune.RuneId}_{i}",
                        sourceRuneId: rune.RuneId,
                        element: rune.Element,
                        origin: origin,
                        direction: rune.Direction,
                        requestedRange: conduit.RequestedRange,
                        traversedCells: new List<Vector2Int>(conduit.TraversedCells),
                        terminationReason: conduit.TerminationReason,
                        targetCell: conduit.TargetCell
                    ));
                }
            }

            UpdateReactions(beamPaths);
        }
    }

    /// <summary>
    /// Visual FX component that renders animated elemental starbursts and pulsing halos
    /// at 2-beam crossing coordinates whenever an elemental reaction is active.
    /// </summary>
    public class ElementalReactionVFXController : MonoBehaviour
    {
        [SerializeField] private ElementalReactionSystem reactionSystem;
        [SerializeField] private Grid.GridView gridView;
        [SerializeField] private UI.ScreenNavigationController navigation;

        public void Initialize(ElementalReactionSystem system, Grid.GridView view, UI.ScreenNavigationController nav = null)
        {
            reactionSystem = system;
            gridView = view;
            navigation = nav;
        }

        private void OnGUI()
        {
            if (reactionSystem == null || gridView == null || Camera.main == null) return;
            if (reactionSystem.ActiveReactionCount == 0) return;
            if (navigation == null || (navigation.CurrentScreen != UI.ScreenState.GRID_BUILD && navigation.CurrentScreen != UI.ScreenState.COMBAT)) return;

            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 18;

            float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * 6.0f);

            foreach (var rxn in reactionSystem.ActiveReactions)
            {
                Vector3 worldPos = Grid.GridCoordinateUtility.GridToWorldPosition(
                    rxn.GridCoordinate.x,
                    rxn.GridCoordinate.y,
                    gridView.GridOrigin,
                    gridView.CellSize,
                    gridView.CellSpacing
                );

                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                float guiX = (screenPos.x / scale) - 90f;
                float guiY = ((Screen.height - screenPos.y) / scale) - 18f;

                Color oldColor = GUI.color;
                GUI.color = new Color(rxn.ReactionColor.r, rxn.ReactionColor.g, rxn.ReactionColor.b, pulse);
                GUI.Box(new Rect(guiX - 10, guiY, 200f, 36f), GUIContent.none, UI.LattiruneUITheme.StyleBadge);
                labelStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(guiX - 10, guiY, 200f, 36f), $"** {rxn.ReactionName.ToUpper()}! **", labelStyle);
                GUI.color = oldColor;
            }

            GUI.matrix = oldMatrix;
        }
    }
}
