using UnityEngine;

namespace CustomUI.Utils 
{
    /// <summary>
    /// An enum handler for creating positional-bounding borders.
    /// </summary>
    public enum Constraint 
    { 
        None = default, 
        Screen, 
        Parent, 
        Other 
    }

    public interface IConstraintable { Vector2[] Bounds { get; } }

    /// <summary>
    /// Uses a switch to determine which bounds to choose from.
    /// </summary>
    /// <param name="constraint">Determines the type of bounds.</param>
    /// <param name="rect">If "Parent" constraint is selected, then this function will return this rect's parent positional bounds.</param>
    /// <param name="otherRect">if "Other" constraint is selected, then this function will return this rect's positional bounds.</param>
    /// <returns>
    /// Positional bounds depending on the constraint type.
    /// </returns>

    public class UI_Constraint 
    {
        public static Vector2[] GetConstraintBounds(Constraint constraint, RectTransform rect, RectTransform otherRect)
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
                    bounds[0] = UI_Utility.RectTransformCorners(rect.parent.GetComponent<RectTransform>())[0];
                    bounds[1] = UI_Utility.RectTransformCorners(rect.parent.GetComponent<RectTransform>())[1];
                    return bounds;
                case Constraint.Other:
                    if (otherRect)
                    {
                        bounds[0] = UI_Utility.RectTransformCorners(otherRect)[0];
                        bounds[1] = UI_Utility.RectTransformCorners(otherRect)[1];
                    }
                    else
                    {
                        bounds[0] = new Vector2(float.MinValue, float.MinValue);
                        bounds[1] = new Vector2(float.MaxValue, float.MaxValue);
                    }
                    return bounds;
            }
        }
    }
}