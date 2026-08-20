using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;

namespace Lattirune.Runes
{
    /// <summary>
    /// Development visualization for active directional rune conduits and Prism branches using Unity LineRenderers.
    /// [DEVELOPMENT / DEBUG ONLY]
    /// </summary>
    public class RuneConduitDebugView : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField] private Color defaultLaserColor = new Color(0.2f, 0.85f, 1f, 0.85f); // Electric Cyan
        [SerializeField] private Color targetHitColor = new Color(1f, 0.8f, 0.25f, 0.9f);   // Bright Gold
        [SerializeField] private Color prismRefractColor = new Color(0.9f, 0.5f, 1f, 0.85f); // Arcane Violet
        [SerializeField] private float laserLineWidth = 0.04f;

        private GridView _gridView;
        private readonly List<LineRenderer> _activeLines = new List<LineRenderer>();
        private Material _lineMaterial;

        public void Initialize(GridView view)
        {
            _gridView = view;
            CreateLineMaterial();
        }

        private void CreateLineMaterial()
        {
            if (_lineMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                _lineMaterial = new Material(shader != null ? shader : Shader.Find("Unlit/Color"));
            }
        }

        public void RenderBeamPaths(IReadOnlyList<ConduitBeamPath> beamPaths)
        {
            ClearConduitVisuals();

            if (beamPaths == null || _gridView == null) return;

            for (int i = 0; i < beamPaths.Count; i++)
            {
                ConduitBeamPath beam = beamPaths[i];
                if (beam.TraversalLength == 0) continue;

                LineRenderer line = GetOrCreateLineRenderer(i);
                line.gameObject.SetActive(true);

                Vector3 startWorldPos = GridCoordinateUtility.GridToWorldPosition(
                    beam.Origin.x, 
                    beam.Origin.y, 
                    _gridView.GridOrigin, 
                    _gridView.CellSize, 
                    _gridView.CellSpacing
                );

                Vector2Int endCoord = beam.TraversedCells[beam.TraversalLength - 1];
                Vector3 endWorldPos = GridCoordinateUtility.GridToWorldPosition(
                    endCoord.x, 
                    endCoord.y, 
                    _gridView.GridOrigin, 
                    _gridView.CellSize, 
                    _gridView.CellSpacing
                );

                line.positionCount = 2;
                line.SetPosition(0, startWorldPos);
                line.SetPosition(1, endWorldPos);

                Color color = beam.IsSplitBranch 
                    ? prismRefractColor 
                    : (beam.TargetCell.HasValue ? targetHitColor : defaultLaserColor);

                line.startColor = color;
                line.endColor = color;
            }
        }

        public void RenderConduits(IReadOnlyList<RuneConduitResult> conduitResults)
        {
            ClearConduitVisuals();

            if (conduitResults == null || _gridView == null) return;

            for (int i = 0; i < conduitResults.Count; i++)
            {
                RuneConduitResult result = conduitResults[i];
                if (result.TraversalLength == 0) continue;

                LineRenderer line = GetOrCreateLineRenderer(i);
                line.gameObject.SetActive(true);

                Vector3 startWorldPos = GridCoordinateUtility.GridToWorldPosition(
                    result.Origin.x, 
                    result.Origin.y, 
                    _gridView.GridOrigin, 
                    _gridView.CellSize, 
                    _gridView.CellSpacing
                );

                Vector2Int endCoord = result.TraversedCells[result.TraversalLength - 1];
                Vector3 endWorldPos = GridCoordinateUtility.GridToWorldPosition(
                    endCoord.x, 
                    endCoord.y, 
                    _gridView.GridOrigin, 
                    _gridView.CellSize, 
                    _gridView.CellSpacing
                );

                line.positionCount = 2;
                line.SetPosition(0, startWorldPos);
                line.SetPosition(1, endWorldPos);

                Color color = result.HasTarget ? targetHitColor : defaultLaserColor;
                line.startColor = color;
                line.endColor = color;
            }
        }

        public void ClearConduitVisuals()
        {
            foreach (var line in _activeLines)
            {
                if (line != null)
                {
                    line.gameObject.SetActive(false);
                }
            }
        }

        private LineRenderer GetOrCreateLineRenderer(int index)
        {
            while (_activeLines.Count <= index)
            {
                GameObject lineObj = new GameObject($"ConduitLine_{_activeLines.Count}");
                lineObj.transform.SetParent(transform);

                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.material = _lineMaterial;
                lr.startWidth = laserLineWidth;
                lr.endWidth = laserLineWidth;
                lr.sortingOrder = 15;
                lr.useWorldSpace = true;

                _activeLines.Add(lr);
            }

            return _activeLines[index];
        }
    }
}
