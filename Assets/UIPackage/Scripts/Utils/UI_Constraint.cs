using UnityEngine;

namespace CustomUI.Utils 
{
    /// <summary>
    /// An enum handler for creating positional-bounding borders.
    /// </summary>
    public enum Constraint { None = default, Screen, Parent, Other }

    public interface IConstraintable { Vector2[] Bounds { get; } }

    public class UI_Constraint
    {
        public static Vector2[] GetBounds(Constraint constraint, RectTransform rect, RectTransform otherRect)
        {
            Vector2[] bounds = new Vector2[2];
            switch (constraint)
            {
                default:
                    bounds[0] = new Vector2(float.MinValue, float.MinValue);
                    bounds[1] = new Vector2(float.MaxValue, float.MaxValue);
                    return bounds;
                case Constraint.Screen:
                    bounds[0] = new Vector2();
                    bounds[1] = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
                    return bounds;
                case Constraint.Parent:
                    bounds[0] = RectTransformCorners(rect.parent.GetComponent<RectTransform>())[0];
                    bounds[1] = RectTransformCorners(rect.parent.GetComponent<RectTransform>())[1];
                    return bounds;
                case Constraint.Other:
                    if (otherRect)
                    {
                        bounds[0] = RectTransformCorners(otherRect)[0];
                        bounds[1] = RectTransformCorners(otherRect)[1];
                    }
                    else 
                    {
                        bounds[0] = new Vector2(float.MinValue, float.MinValue);
                        bounds[1] = new Vector2(float.MaxValue, float.MaxValue);
                    }
                    return bounds;
            }
        }

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