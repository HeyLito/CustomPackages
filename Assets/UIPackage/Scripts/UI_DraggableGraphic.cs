using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using CustomUI.Utils;

namespace CustomUI 
{
    /// <summary>
    /// A MonoBehaviour component that handles simple drag movement.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UI/Draggable Graphic")]
    public class UI_DraggableGraphic : MonoBehaviour, IConstraintable, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region Unity Editor
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/UI/Draggable Graphic")]
        private static void CreateDraggableGraphic()
        {
            var sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            var imageType = UnityEngine.UI.Image.Type.Sliced;

            var obj = UI_MenuItemHelper<UI_DraggableGraphic>.CreateObject("Draggable Graphic", typeof(UnityEngine.UI.Image));
            obj.targetRect = obj._rect;
            var image = obj.gameObject.GetComponent<UnityEngine.UI.Image>();
            image.type = imageType;
            image.sprite = sprite;
        }

        [UnityEditor.MenuItem("GameObject/UI/Draggable Window")]
        private static void CreateDraggableWindow()
        {
            var sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            var imageType = UnityEngine.UI.Image.Type.Sliced;

            var obj = UI_MenuItemHelper<UI_DraggableGraphic>.CreateObject("Header", typeof(UnityEngine.UI.Image));
            var window = new GameObject("Draggable Window", typeof(UnityEngine.UI.Image));
            window.gameObject.layer = obj.gameObject.layer;
            obj.targetRect = obj._rect;
            var windowImage = window.GetComponent<UnityEngine.UI.Image>();
            var image = obj.gameObject.GetComponent<UnityEngine.UI.Image>();
            image.type = windowImage.type = imageType;
            image.sprite = windowImage.sprite = sprite;
            windowImage.color = new Color(0.75f, 0.75f, 0.75f, 1);

            window.transform.SetParent(obj.transform.parent);
            obj.transform.SetParent(window.transform);
            obj.transform.localPosition = window.transform.localPosition = new Vector3();
            var windowRect = window.GetComponent<RectTransform>();
            var objRect = obj.GetComponent<RectTransform>();
            objRect.pivot = new Vector2(0.5f, 1);
            objRect.anchorMin = new Vector2(0, 1);
            objRect.anchorMax = new Vector2(1, 1);
            objRect.sizeDelta = new Vector2(0, 50);
            windowRect.sizeDelta = new Vector2(200, 200);

            obj.targetRect = windowRect;
            obj.dragPivot = DragPivot.PointerWorldPosition;
        }

        [SerializeField] private bool debug = false;
        private void OnDrawGizmos() 
        {
            if (!debug || !targetRect)
                return;

            UI_Projection projectionDebug = new UI_Projection(targetRect, targetRect.position, Bounds);
            UI_Projection.DrawGizmosOutOfBounds(projectionDebug, Color.yellow, new Color(1f, 0.5f, 0f, 1));
            UI_Projection.DrawGizmosCorners(projectionDebug.WorldCorners, Color.red);
            UI_Projection.DrawGizmosCorners(projectionDebug.BoundedCorners, Color.green);
        }
        #endif
        #endregion

        #region Variables
        /// <summary>
        /// An enum type that handles drag positions.
        /// </summary>
        public enum DragPivot { TargetsRectPivot = default, ThisRectPivot, PointerLocalPosition, PointerWorldPosition }

        /// <summary>
        /// When dragging is active, this rect will become the main target to move.
        /// </summary>
        public RectTransform targetRect = null;
        /// <summary>
        /// An enum that determines the border-like restraints that could be applied to the \"Target Rect\". The target's edges will be prevented from going out of these bounds when dragging is active.
        /// </summary>
        public Constraint dragConstraint = default;
        /// <summary>
        /// When "dragConstraint" is set to "Other", this creates a custom constraint boundaries using another Rect's position and sizes.
        /// </summary>
        public RectTransform otherConstraint = null;
        /// <summary>
        /// Determines the target's drag position in relation to this enum.
        /// </summary>
        public DragPivot dragPivot = default;

        private Vector3 _dis = new Vector3();
        private RectTransform _rect = null;
        private UI_Projection _projection = null;
        
        /// <summary>
        /// Activates when pointer begins to drag a Graphic.
        /// </summary>
        public UnityEvent onDragBegin = new UnityEvent();
        /// <summary>
        /// Active when pointer is dragging a Graphic.
        /// </summary>
        public UnityEvent onDrag = new UnityEvent();
        /// <summary>
        /// Activates when pointer exits dragging a Graphic.
        /// </summary>
        public UnityEvent onDragEnd = new UnityEvent();

        public Vector2[] Bounds => UI_Constraint.GetBounds(dragConstraint, _rect, otherConstraint);
        #endregion

        #region Methods
        private void OnEnable()
        {
            _rect = GetComponent<RectTransform>();
            if (!targetRect) targetRect = _rect;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            switch (dragPivot)
            {
                default:
                    _dis = new Vector3();
                    break;

                case DragPivot.ThisRectPivot:
                    _dis = -_rect.localPosition;
                    break;

                case DragPivot.PointerLocalPosition:
                    _dis = _rect.transform.position - ConvertToVector3(eventData.position);
                    break;

                case DragPivot.PointerWorldPosition:
                    _dis = targetRect.transform.position - ConvertToVector3(eventData.position);
                    break;
            }
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            _projection = new UI_Projection(targetRect);
            onDragBegin?.Invoke();
        }
        public void OnDrag(PointerEventData eventData)
        {
            _projection.MoveBoundedProjection(ConvertToVector3(eventData.position) + _dis, Bounds);
            targetRect.transform.position = _projection.WorldPosition;

            onDrag?.Invoke();
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            onDragEnd?.Invoke();
        }

        private Vector3 ConvertToVector3(Vector2 vector2)
        {
            return new Vector3(vector2.x, vector2.y);
        }
        #endregion
    }
}