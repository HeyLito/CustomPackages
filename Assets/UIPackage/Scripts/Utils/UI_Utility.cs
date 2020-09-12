using UnityEngine;

namespace CustomUI.Utils
{
    public class UI_Utility
    {
        /// <summary>
        /// Allows a position to stay constant between different resolutions.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>
        /// A position multiplied by current screen resolution width and height.
        /// </returns>
        public static Vector2 ConvertPositionToScreenConstant(Vector2 position)
        {
            Vector2 constantBounds = new Vector2(Screen.currentResolution.width / 180, Screen.currentResolution.height / 180);
            return new Vector2(position.x * constantBounds.x, position.y * constantBounds.y);
        }

        /// <summary>
        /// Allows a position to stay constant between different resolutions.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>
        /// A position multiplied by current screen resolution width and height.
        /// </returns>
        public static Vector3 ConvertPositionToScreenConstant(Vector3 position)
        {
            Vector2 constantBounds = new Vector2(Screen.currentResolution.width / 180, Screen.currentResolution.height / 180);
            return new Vector2(position.x * constantBounds.x, position.y * constantBounds.y);
        }

        /// <summary>
        /// Gets the corners of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The subject to get the corners from.</param>
        /// <returns>
        /// Four Vector2 corners in world space.
        /// </returns>
        public static Vector2[] RectTransformCorners(RectTransform rectTransform)
        {
            if (!rectTransform)
                return new Vector2[2];
            Vector3[] rectCorners = new Vector3[4];
            rectTransform.GetWorldCorners(rectCorners);
            return new[] { new Vector2(rectCorners[0].x, rectCorners[0].y), new Vector2(rectCorners[2].x, rectCorners[2].y) };
        }
    }
}