using Microsoft.Xna.Framework;

namespace Oudidon
{
    public static class MathUtils
    {
        public static float NormalizedParabolicPosition(float t)
        {
            return 4 * t * (1 - t);
        }

        public static bool OverlapsWith(Rectangle first, Rectangle second)
        {
            return !(first.Bottom < second.Top
                    || first.Right < second.Left
                    || first.Top > second.Bottom
                    || first.Left > second.Right);
        }
    }
}
