using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;

namespace Lattirune.Runes
{
    /// <summary>
    /// Development visualization for active directional rune conduits using Unity LineRenderers.
    /// [DEVELOPMENT / DEBUG ONLY]
    /// </summary>
    public class RuneConduitDebugView : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField] private Color defaultLaserColor = new Color(0f, 0.9f, 1f, 0.9f); // Electric Cyan
        [SerializeField] private Color targetHitColor = new Color(1f, 0.85f, 0.2f, 1f);   // Bright Gold
        [SerializeField] private float laserLineWidth = 0.15f;

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
                lr.sortingOrder = 15; // In front of grid, behind dragged items
                lr.useWorldSpace = true;

                _activeLines.Add(lr);
            }

            return _activeLines[index];
        }
    }
}
