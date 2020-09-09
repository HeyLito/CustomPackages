using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using CustomUI.Utils;

namespace CustomUI 
{
    /// <summary>
    /// A MonoBehaviour component that displays UI tooltips on active pointer events.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UI/Tooltip")]
    public class UI_Tooltip : MonoBehaviour, IConstraintable, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IEndDragHandler
    {
        #region Unity Editor
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/UI/Tooltip")]
        private static void CreateObject() 
        {
            var sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            var imageType = UnityEngine.UI.Image.Type.Sliced;

            var obj = UI_MenuItemHelper<UI_Tooltip>.CreateObject("Tooltip", typeof(UnityEngine.UI.Image));
            var tooltipImage = obj.GetComponent<UnityEngine.UI.Image>();
            var contentObj = new GameObject("Content", typeof(Canvas), typeof(UnityEngine.UI.Image));
            var contentImage = contentObj.GetComponent<UnityEngine.UI.Image>();
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentObj.layer = obj.gameObject.layer;
            contentImage.color = new Color(1f, 1f, 1f, 0.5f);
            contentImage.type = tooltipImage.type = imageType;
            contentImage.sprite = tooltipImage.sprite = sprite;
            contentRect.transform.SetParent(obj.transform);
            contentRect.pivot = new Vector2(0.5f, 0f);
            contentRect.sizeDelta = new Vector2(200f, 150f);

            obj.content = contentRect;
            obj.offsetFromPointer = new Vector2(0, 10);
            var offsetProjection = new UI_Projection(obj.content, obj.transform.position + obj.OffsetPositionFromScreen(new Vector3(obj.offsetFromPointer.x, obj.offsetFromPointer.y)));
            obj.content.transform.position = offsetProjection.WorldPosition;
        }

        [SerializeField] private bool debug = false;
        private void OnDrawGizmos()
        {
            if (!debug || !content)
                return;

            DisplayPivot contentDisplayPivot = this.displayPivot;
            if (!Application.isPlaying)
                contentDisplayPivot = DisplayPivot.IdleOnPivot;

            UI_Projection projectionDebug = new UI_Projection(content, PositionFromDisplayPivot(contentDisplayPivot) + OffsetPositionFromScreen(offsetFromPointer), Bounds);
            UI_Projection mirroredProjectionDebug = new UI_Projection(content, PositionFromDisplayPivot(contentDisplayPivot) + MirroredPositionWithConstraint(mirrorConstraint, projectionDebug, OffsetPositionFromScreen(offsetFromPointer)), Bounds);

            UI_Projection.DrawGizmosOutOfBounds(projectionDebug, Color.yellow, new Color(1f, 0.5f, 0f, 1));
            UI_Projection.DrawGizmosCorners(mirroredProjectionDebug.BoundedCorners, Color.blue);
            UI_Projection.DrawGizmosCorners(projectionDebug.WorldCorners, Color.red);
            UI_Projection.DrawGizmosCorners(projectionDebug.BoundedCorners, Color.green);
        }
        #endif
        #endregion

        #region Variables
        public enum MirrorConstraint { None, UpDown, LeftRight, Both }
        public enum ActivationMethod { OnHover = default, OnClick }
        public enum DisplayPivot { FollowPointer = default, IdleOnPointer, IdleOnPivot }

        public RectTransform content = null;
        public Constraint contentConstraint = default;
        public RectTransform otherConstraint = null;
        public MirrorConstraint mirrorConstraint = default;

        public ActivationMethod activationMethod = default;
        public DisplayPivot displayPivot = default;
        public float displayYield = 0f;
        public Vector2 offsetFromPointer = new Vector2(0, 0);

        public UnityEvent onTooltipBegin = new UnityEvent();
        public UnityEvent onTooltipStay = new UnityEvent();
        public UnityEvent onTooltipEnd = new UnityEvent();

        private RectTransform _rect = null;
        private UI_Projection _mainProjection = null;
        private UI_Projection _mirroredProjection = null;
        private bool _flipped = false;
        private bool _renderable = false;
        private Vector2 _pointerPosOnRendered = new Vector2();

        public bool IsActive { get; private set; } = false;
        public Vector2[] Bounds => UI_Constraint.GetBounds(contentConstraint, _rect, otherConstraint);
        private Vector2 ContentPosition => PositionFromDisplayPivot(displayPivot);
        #endregion

        #region Methods
        private void OnEnable() 
        {
            _rect = GetComponent<RectTransform>();
            _pointerPosOnRendered = PositionFromDisplayPivot(DisplayPivot.IdleOnPivot);
        }
        private void Awake()
        {
            if (!Application.IsPlaying(gameObject) || !content)
                return;

            Canvas canvas;
            if (!content.GetComponent<Canvas>())
                canvas = content.gameObject.AddComponent<Canvas>();
            else canvas = content.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;

            UnityEngine.UI.Graphic[] graphics = content.GetComponentsInChildren<UnityEngine.UI.Graphic>();
            for (int i = 0; i < graphics.Length; i++)
                graphics[i].raycastTarget = false;
            content.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (activationMethod == ActivationMethod.OnHover || _renderable)
                TooltipBegin();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            IsActive = false;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (activationMethod == ActivationMethod.OnClick)
            {
                TooltipBegin();
                _renderable = true;
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (activationMethod == ActivationMethod.OnClick) 
            {
                IsActive = false;
                _renderable = false;
            }
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (activationMethod == ActivationMethod.OnClick) 
            {
                IsActive = false;
                _renderable = false;
            }
        }

        private void TooltipBegin()
        {
            if (!content)
                return;

            IsActive = true;
            StopAllCoroutines();
            onTooltipBegin?.Invoke();
            StartCoroutine(TooltipStay());
        }
        private IEnumerator TooltipStayYield()
        {
            float time = 0, timeToYield = displayYield;
            while (time < timeToYield && IsActive && content)
            {
                time += Time.deltaTime;
                yield return null;
            }
        }
        private IEnumerator TooltipStay()
        {
            yield return StartCoroutine(TooltipStayYield());
            if (content) 
            {
                _pointerPosOnRendered = PositionFromDisplayPivot(default);
                _mainProjection = new UI_Projection(content);
                _mirroredProjection = new UI_Projection(content);
                MoveTooltip(ContentPosition);
                content.gameObject.SetActive(true);
                while (IsActive)
                {
                    yield return new WaitForEndOfFrame();
                    onTooltipStay?.Invoke();
                    MoveTooltip(ContentPosition);
                }
            }
            TooltipEnd();
        }
        private void TooltipEnd()
        {
            if (!content)
                return;

            _flipped = false;
            IsActive = false;
            content.gameObject.SetActive(false);
            onTooltipEnd?.Invoke();
        }

        private void MoveTooltip(Vector2 position)
        {
            Vector2 normalPos = position + OffsetPositionFromScreen(offsetFromPointer);
            Vector2 mirrorPos = position + MirroredPositionWithConstraint(mirrorConstraint, _mainProjection, OffsetPositionFromScreen(offsetFromPointer));

            _mainProjection.MoveBoundedProjection(normalPos, Bounds);
            _mirroredProjection.MoveBoundedProjection(mirrorPos, Bounds);

            if (_flipped)
            {
                if (CanFlipToOtherProjection(mirrorConstraint, _mirroredProjection, _mainProjection)) 
                {
                    content.position = _mainProjection.WorldPosition;
                    _flipped = !_flipped;
                }
                else content.position = _mirroredProjection.WorldPosition;
            }
            else
            {
                if (CanFlipToOtherProjection(mirrorConstraint, _mainProjection, _mirroredProjection))
                {
                    content.position = _mirroredProjection.WorldPosition;
                    _flipped = !_flipped;
                }
                else content.position = _mainProjection.WorldPosition;
            }
        }

        private Vector2 PositionFromDisplayPivot(DisplayPivot contentDisplayPivot)
        {
            switch (contentDisplayPivot) 
            {
                default: 
                    return Input.mousePosition;
                    
                case DisplayPivot.IdleOnPointer: 
                    return _pointerPosOnRendered;

                case DisplayPivot.IdleOnPivot: 
                    return transform.position; 
            }
        }
        private Vector2 MirroredPositionWithConstraint(MirrorConstraint mirrorConstraint, UI_Projection projection, Vector2 position) 
        {
            if (contentConstraint == Constraint.None)
                return position;

            switch (mirrorConstraint)
            {
                default:
                    return position;

                case MirrorConstraint.UpDown:
                    position.y *= -1;
                    position.y += ((projection.Pivot.y * 2) - 1) * projection.Size.y;
                    return position;

                case MirrorConstraint.LeftRight:
                    position.x *= -1;
                    position.x += ((projection.Pivot.x * 2) - 1) * projection.Size.x;
                    return position;

                case MirrorConstraint.Both:
                    position.x *= -1;
                    position.y *= -1;
                    position.x += ((projection.Pivot.x * 2) - 1) * projection.Size.x;
                    position.y += ((projection.Pivot.y * 2) - 1) * projection.Size.y;
                    return position;
            }
        }
        private bool CanFlipToOtherProjection(MirrorConstraint mirrorConstraint, UI_Projection currentProjection, UI_Projection otherProjection) 
        {
            switch (mirrorConstraint) 
            {
                default:
                    return false;

                case MirrorConstraint.UpDown:
                    if (currentProjection.OutOfBoundsDistances[2] != 0 || currentProjection.OutOfBoundsDistances[3] != 0) 
                        if(otherProjection.OutOfBoundsDistances[2] == 0 && otherProjection.OutOfBoundsDistances[3] == 0)
                            return true;
                    return false;
                case MirrorConstraint.LeftRight:
                    if (currentProjection.OutOfBoundsDistances[0] != 0 || currentProjection.OutOfBoundsDistances[1] != 0)
                        if (otherProjection.OutOfBoundsDistances[0] == 0 && otherProjection.OutOfBoundsDistances[1] == 0)
                            return true;
                    return false;
                case MirrorConstraint.Both:
                    if (currentProjection.IsOutOfBounds && !otherProjection.IsOutOfBounds)
                        return true;
                    return false;
            }
        }

        public Vector2 OffsetPositionFromScreen(Vector2 position)
        {
            Vector2 constantBounds = new Vector2(Screen.currentResolution.width / 180, Screen.currentResolution.height / 180);
            return new Vector2(position.x * constantBounds.x, position.y * constantBounds.y);
        }
        public Vector3 OffsetPositionFromScreen(Vector3 position)
        {
            Vector2 constantBounds = new Vector2(Screen.currentResolution.width / 180, Screen.currentResolution.height / 180);
            return new Vector2(position.x * constantBounds.x, position.y * constantBounds.y);
        }
        #endregion
    }
}