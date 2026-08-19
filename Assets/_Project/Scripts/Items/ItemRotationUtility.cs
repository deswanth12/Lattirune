using UnityEngine;

namespace Lattirune.Items
{
    /// <summary>
    /// Utility for calculating 90-degree stepped item rotations and transformed footprint dimensions.
    /// </summary>
    public static class ItemRotationUtility
    {
        public static int NormalizeRotation(int rotationDegrees)
        {
            int normalized = rotationDegrees % 360;
            if (normalized < 0) normalized += 360;
            // Snap to nearest 90-degree step
            return (normalized / 90) * 90;
        }

        public static int GetNextRotation(int currentRotationDegrees)
        {
            return (NormalizeRotation(currentRotationDegrees) + 90) % 360;
        }

        /// <summary>
        /// Calculates the effective footprint (width, height) for an item after applying rotation.
        /// 0° / 180°: (base.x, base.y)
        /// 90° / 270°: (base.y, base.x)
        /// </summary>
        public static Vector2Int GetRotatedDimensions(Vector2Int baseDimensions, int rotationDegrees)
        {
            int normalized = NormalizeRotation(rotationDegrees);
            if (normalized == 90 || normalized == 270)
            {
                return new Vector2Int(baseDimensions.y, baseDimensions.x);
            }

            return baseDimensions;
        }
    }
}
