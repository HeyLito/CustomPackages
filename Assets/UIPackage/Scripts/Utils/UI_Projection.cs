using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUI.Utils
{
    /// <summary>
    /// Drawless and baseless instance of a RectTransform with constraints.
    /// </summary>
    public class UI_Projection
    {
        #region Variables
        private Vector2[] _bounds = new Vector2[2];

        /// <summary>
        /// The projection's corner positions within local-space.
        /// </summary>
        /// <returns>
        /// A Vector2 array with a length of 4.
        /// The order of the corners are as follows: BottomLeft -> TopLeft -> TopRight -> BottomRight.
        /// </returns>
        public Vector2[] LocalCorners { get; private set; } = new Vector2[4];
        /// <summary>
        /// The projection's corner positions within world-space.
        /// </summary>
        /// <returns>
        /// A Vector2 array with a length of 4.
        /// The order of the corners are as follows: BottomLeft -> TopLeft -> TopRight -> BottomRight.
        /// </returns>
        public Vector2[] WorldCorners { get; private set; } = new Vector2[4];
        /// <summary>
        /// The projection's bounded corner positions within world-space.
        /// </summary>
        /// <returns>
        /// A Vector2 array with a length of 4.
        /// The order of the corners are as follows: BottomLeft -> TopLeft -> TopRight -> BottomRight.
        /// If bounds aren't present, then this will return WorldCorners.
        /// </returns>
        public Vector2[] BoundedCorners { get; private set; } = new Vector2[4];
        /// <summary>
        /// The projection's current world position.
        /// </summary>
        public Vector2 WorldPosition { get; private set; } = new Vector2();
        /// <summary>
        /// Uses LocalCorners to get this projection's size.
        /// </summary>
        public Vector2 Size 
        {
            get { return new Vector2(Mathf.Abs(LocalCorners[0].x) + Mathf.Abs(LocalCorners[2].x), Mathf.Abs(LocalCorners[0].y) + Mathf.Abs(LocalCorners[2].y)); }
        }
        /// <summary>
        /// Uses LocalCorners to get this projection's pivot.
        /// </summary>
        public Vector2 Pivot 
        {
            get 
            {
                return new Vector2()
                {
                    x = LocalCorners[0].x - LocalCorners[2].x != 0 ? ((LocalCorners[0].x + LocalCorners[2].x) / -Mathf.Abs(LocalCorners[0].x - LocalCorners[2].x) * 0.5f) + 0.5f : 0.5f,
                    y = LocalCorners[0].y - LocalCorners[2].y != 0 ? ((LocalCorners[0].y + LocalCorners[2].y) / -Mathf.Abs(LocalCorners[0].y - LocalCorners[2].y) * 0.5f) + 0.5f : 0.5f
                };
            }
        }

        /// <summary>
        /// Calculates the out-of-bounds distances between the projection's edge and bounds.
        /// If an edge is within its proper bounds, then the distance returned will always be a zero.
        /// </summary>
        /// <returns>
        /// A float array with a length of 4.
        /// The order of edge distances are as follows: Left -> Right -> Top -> Bottom.
        /// </returns>
        public float[] OutOfBoundsDistances 
        { 
            get 
            {
                return new[]
{
                    CalculateOutOfBoundsDistance(WorldCorners[0].x, new[] {_bounds[0].x, _bounds[1].x }),
                    CalculateOutOfBoundsDistance(WorldCorners[2].x, new[] {_bounds[0].x, _bounds[1].x }),
                    CalculateOutOfBoundsDistance(WorldCorners[2].y, new[] {_bounds[0].y, _bounds[1].y }),
                    CalculateOutOfBoundsDistance(WorldCorners[0].y, new[] {_bounds[0].y, _bounds[1].y })
                }; 
            } 
        }
        /// <summary>
        /// Determines whether the projection's edges are exceeding or subceeding bounds. 
        /// Will only apply if bounds has been set either by SetBounds() or MoveBoundedProjection(). 
        /// Will not apply correctly if MoveBoundedProjection() is actively used.
        /// </summary>
        /// <returns>
        /// True if any corner position in WorldCorners is caught outside of bounds.
        /// </returns>
        public bool IsOutOfBounds 
        { 
            get 
            { 
                foreach (float distance in OutOfBoundsDistances) 
                    if (distance != 0) 
                        return true; 
                return false; 
            } 
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a drawless instance of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">RectTransform to create an instance of.</param>
        public UI_Projection(RectTransform rectTransform)
        {
            SetBounds(new[] { new Vector2(float.MinValue, float.MinValue), new Vector2(float.MaxValue, float.MaxValue) });
            SetCorners(rectTransform);
        }
        /// <summary>
        /// Creates a drawless instance of a RectTransform and moves it via a world position while ignoring bounds.
        /// </summary>
        /// <param name="rectTransform">RectTransform to create an instance of.</param>
        /// <param name="worldPosition">Position in world-space to move the projection to.</param>
        public UI_Projection(RectTransform rectTransform, Vector2 worldPosition)
        {
            SetBounds(new[] { new Vector2(float.MinValue, float.MinValue), new Vector2(float.MaxValue, float.MaxValue) });
            SetCorners(rectTransform);
            MoveProjection(worldPosition);
        }
        /// <summary>
        /// Creates a drawless instance of a RectTransform and moves it via a world position that will have its edges restricted from exceeding or subceeding its world-positional bounds.
        /// </summary>
        /// <param name="rectTransform">RectTransform to create an instance of.</param>
        /// <param name="worldPosition">Position in world-space to move the projection to.</param>
        /// <param name="bounds">Bounds in world-space that will restrict the projection's edges from exceeding or subceeding its position.</param>
        public UI_Projection(RectTransform rectTransform, Vector2 worldPosition, Vector2[] bounds)
        {
            SetBounds(new[] { new Vector2(float.MinValue, float.MinValue), new Vector2(float.MaxValue, float.MaxValue) });
            SetCorners(rectTransform);
            MoveBoundedProjection(worldPosition, bounds);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Set the projection's world-positional bounds that this projection's edges are allow to stay within.
        /// </summary>
        /// <param name="bounds">Bounds in world-space that will restrict the projection's edges from exceeding or subceeding its position.</param>
        private void SetBounds(Vector2[] bounds) 
        {
            _bounds[0] = bounds[0];
            _bounds[1] = bounds[1];
        }
        private void SetCorners(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            Vector3 zeroPos = rectTransform.TransformPoint(new Vector3());
            rectTransform.GetWorldCorners(corners);
            for (int i = 0; i < corners.Length; i++)
                LocalCorners[i] = corners[i] - zeroPos;
        }

        /// <summary>
        /// Move a projection unaffected by bounds via a world position.
        /// </summary>
        /// <param name="worldPosition">Position in world-space to move the projection to.</param>
        public void MoveProjection(Vector3 worldPosition)
        {
            for (int i = 0; i < LocalCorners.Length; i++)
                WorldCorners[i] = LocalCorners[i] + new Vector2(worldPosition.x, worldPosition.y);
            WorldPosition = worldPosition;
        }
        /// <summary>
        /// Move a projection via a world position that will have its edges restricted from exceeding or subceeding its world-positional bounds.
        /// </summary>
        /// <param name="worldPosition">Position in world-space to move the projection to.</param>
        /// <param name="bounds">Bounds in world-space that will restrict the projection's edges from exceeding or subceeding its position.</param>
        public void MoveBoundedProjection(Vector3 worldPosition, Vector2[] bounds)
        {
            MoveProjection(worldPosition);
            SetBounds(bounds);

            float[] distances = new[]
            {
                CalculateOutOfBoundsDistance(WorldCorners[0].x, _bounds[0].x, float.MinValue),
                CalculateOutOfBoundsDistance(WorldCorners[2].x, _bounds[1].x, float.MaxValue),
                CalculateOutOfBoundsDistance(WorldCorners[2].y, _bounds[1].y, float.MaxValue),
                CalculateOutOfBoundsDistance(WorldCorners[0].y, _bounds[0].y, float.MinValue)
            };
            Vector2 clampedPos = new Vector2(worldPosition.x - distances[0] - distances[1], worldPosition.y - distances[2] - distances[3]);
            for (int i = 0; i < LocalCorners.Length; i++)
                BoundedCorners[i] = LocalCorners[i] + clampedPos;
            WorldPosition = clampedPos;
        }

        private float CalculateOutOfBoundsDistance(float corner, float[] axisBounds)
        {
            float distance;

            distance = corner - axisBounds[0];
            if (distance < 0)
                return Mathf.Clamp(distance, distance, 0);

            distance = corner - axisBounds[1];
            if (distance > 0)
                return Mathf.Clamp(distance, 0, distance);

            return 0;
        }
        private float CalculateOutOfBoundsDistance(float corner, float boundary, float limit)
        {
            float distance = corner - boundary;
            if (limit > 0)
                return Mathf.Clamp(distance, 0, distance);
            else
                return Mathf.Clamp(distance, distance, 0);
        }

        //private float CalculateOutOfBoundsDistance(float corner, float minBounds, float maxBounds)
        //{
        //    float distance;

        //    distance = corner - minBounds;
        //    if (distance < 0)
        //        return Mathf.Clamp(distance, distance, 0);

        //    distance = corner - maxBounds;
        //    if (distance > 0)
        //        return Mathf.Clamp(distance, 0, distance);

        //    return 0;
        //}
        #endregion

        #region Static Helper Methods
        public static void DrawLineCorners(Vector2[] corners, Color color)
        {
            for (int i = 0; i < corners.Length; i++)
            {
                if (i > 0)
                    Debug.DrawLine(corners[i], corners[i - 1], color);
                else Debug.DrawLine(corners[i], corners[corners.Length - 1], color);
            }
        }
        public static void DrawGizmosCorners(Vector2[] corners, Color color)
        {
            Gizmos.color = color;
            for (int i = 0; i < corners.Length; i++)
            {
                if (i > 0)
                    Gizmos.DrawLine(corners[i], corners[i - 1]);
                else Gizmos.DrawLine(corners[i], corners[corners.Length - 1]);
            }
        }
        public static void DrawLineOutOfBounds(UI_Projection projection, Color color, Color otherColor)
        {
            Vector2[] outDistancePositions = new[]
            {
                new Vector2(projection.WorldCorners[0].x, projection.WorldPosition.y),
                new Vector2(projection.WorldCorners[2].x, projection.WorldPosition.y),
                new Vector2(projection.WorldPosition.x, projection.WorldCorners[2].y),
                new Vector2(projection.WorldPosition.x, projection.WorldCorners[0].y)
            };
            Debug.DrawLine(outDistancePositions[0], new Vector2(outDistancePositions[0].x - projection.OutOfBoundsDistances[0], outDistancePositions[0].y), color);
            Debug.DrawLine(outDistancePositions[1], new Vector2(outDistancePositions[1].x - projection.OutOfBoundsDistances[1], outDistancePositions[1].y), color);
            Debug.DrawLine(outDistancePositions[2], new Vector2(outDistancePositions[2].x, outDistancePositions[2].y - projection.OutOfBoundsDistances[2]), color);
            Debug.DrawLine(outDistancePositions[3], new Vector2(outDistancePositions[3].x, outDistancePositions[3].y - projection.OutOfBoundsDistances[3]), color);

            Debug.DrawLine(outDistancePositions[0], new Vector2(outDistancePositions[0].x, outDistancePositions[3].y), otherColor);
            Debug.DrawLine(outDistancePositions[1], new Vector2(outDistancePositions[1].x, outDistancePositions[2].y), otherColor);
            Debug.DrawLine(outDistancePositions[2], new Vector2(outDistancePositions[1].x, outDistancePositions[2].y), otherColor);
            Debug.DrawLine(outDistancePositions[3], new Vector2(outDistancePositions[0].x, outDistancePositions[3].y), otherColor);
        }
        public static void DrawGizmosOutOfBounds(UI_Projection projection, Color color, Color otherColor)
        {
            Gizmos.color = color;
            Vector2[] outDistancePositions = new[]
            {
                new Vector2(projection.WorldCorners[0].x, projection.WorldPosition.y),
                new Vector2(projection.WorldCorners[2].x, projection.WorldPosition.y),
                new Vector2(projection.WorldPosition.x, projection.WorldCorners[2].y),
                new Vector2(projection.WorldPosition.x, projection.WorldCorners[0].y)
            };
            Gizmos.DrawLine(outDistancePositions[0], new Vector2(outDistancePositions[0].x - projection.OutOfBoundsDistances[0], outDistancePositions[0].y));
            Gizmos.DrawLine(outDistancePositions[1], new Vector2(outDistancePositions[1].x - projection.OutOfBoundsDistances[1], outDistancePositions[1].y));
            Gizmos.DrawLine(outDistancePositions[2], new Vector2(outDistancePositions[2].x, outDistancePositions[2].y - projection.OutOfBoundsDistances[2]));
            Gizmos.DrawLine(outDistancePositions[3], new Vector2(outDistancePositions[3].x, outDistancePositions[3].y - projection.OutOfBoundsDistances[3]));

            Gizmos.color = otherColor;

            if (projection.OutOfBoundsDistances[0] == 0)
            {
                if (projection.OutOfBoundsDistances[2] < 0)
                    Gizmos.DrawLine(outDistancePositions[0] + new Vector2(0, projection.LocalCorners[0].y), new Vector2(outDistancePositions[0].x, outDistancePositions[3].y));
                else if (projection.OutOfBoundsDistances[3] > 0)
                    Gizmos.DrawLine(outDistancePositions[0] + new Vector2(0, projection.LocalCorners[2].y), new Vector2(outDistancePositions[0].x, outDistancePositions[3].y));
            }
            else Gizmos.DrawLine(outDistancePositions[0], new Vector2(outDistancePositions[0].x, outDistancePositions[3].y));

            if (projection.OutOfBoundsDistances[1] == 0)
            {
                if (projection.OutOfBoundsDistances[2] < 0)
                    Gizmos.DrawLine(outDistancePositions[1] + new Vector2(0, projection.LocalCorners[0].y), new Vector2(outDistancePositions[1].x, outDistancePositions[2].y));
                else if (projection.OutOfBoundsDistances[3] > 0)
                    Gizmos.DrawLine(outDistancePositions[1] + new Vector2(0, projection.LocalCorners[2].y), new Vector2(outDistancePositions[1].x, outDistancePositions[2].y));
            }
            else Gizmos.DrawLine(outDistancePositions[1], new Vector2(outDistancePositions[1].x, outDistancePositions[2].y));

            if (projection.OutOfBoundsDistances[2] == 0)
            {
                if (projection.OutOfBoundsDistances[0] < 0)
                    Gizmos.DrawLine(outDistancePositions[2] + new Vector2(projection.LocalCorners[0].x, 0), new Vector2(outDistancePositions[0].x, outDistancePositions[2].y));
                else if (projection.OutOfBoundsDistances[1] > 0)
                    Gizmos.DrawLine(outDistancePositions[2] + new Vector2(projection.LocalCorners[2].x, 0), new Vector2(outDistancePositions[0].x, outDistancePositions[2].y));
            }
            else Gizmos.DrawLine(outDistancePositions[2], new Vector2(outDistancePositions[0].x, outDistancePositions[2].y));

            if (projection.OutOfBoundsDistances[3] == 0)
            {
                if (projection.OutOfBoundsDistances[0] < 0)
                    Gizmos.DrawLine(outDistancePositions[3] + new Vector2(projection.LocalCorners[0].x, 0), new Vector2(outDistancePositions[1].x, outDistancePositions[3].y));
                else if (projection.OutOfBoundsDistances[1] > 0)
                    Gizmos.DrawLine(outDistancePositions[3] + new Vector2(projection.LocalCorners[2].x, 0), new Vector2(outDistancePositions[1].x, outDistancePositions[3].y));
            }
            else Gizmos.DrawLine(outDistancePositions[3], new Vector2(outDistancePositions[1].x, outDistancePositions[3].y));
        }
        #endregion
    }
}