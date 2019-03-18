namespace App.Util {
    public static class MathUtility {
        /// <summary>
        /// Converts a number in a range to an equivalent number in another range.
        /// </summary>
        /// <param name="x">Number to convert.</param>
        /// <param name="min">Minimum of the starting range.</param>
        /// <param name="max">Maximum of the starting range.</param>
        /// <param name="newMin">Minimum of the new range.</param>
        /// <param name="newMax">Maximum of the new range.</param>
        /// <returns>A number from newMin to newMax at x's relative position between min and max.</returns>
        public static float Map(float x, float min, float max, float newMin, float newMax) {
            return (x - min) / (max - min) * (newMax - newMin) + newMin;
        }
    }
}