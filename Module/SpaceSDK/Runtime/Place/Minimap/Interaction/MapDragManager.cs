using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MaxstXR.Place
{
    public class MapDragManager : InjectorBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [DI(DIScope.component, DIComponent.place)] public XrSettings XrSettings { get; }
        [DI(DIScope.component, DIComponent.minimap)] public MinimapViewModel MinimapViewModel { get; }
        [DI(DIScope.component, DIComponent.minimap)] public MinimapInputOp MinimapInputOp { get; }

        [field: SerializeField] public UnityEvent<Vector3> OnPositionWhenDrag { get; private set; } = new();
        [field: SerializeField] public UnityEvent<Vector3> OnPositionWhenClick { get; private set; } = new();

        [SerializeField] private RawImage minimapImage;

        private Vector2 position = Vector2.zero;
        private bool isEnter = false;
        private bool isDragging = false;
        private bool isDragAllow = true;
        private Plane plane = new(Vector3.up, Vector3.zero);

        private void OnEnable()
        {
            MinimapViewModel.VisibleScreenPoint.AddObserver(this, OnVisibleScreenPoint);
            MinimapViewModel.VisibleSize.AddObserver(this, OnVisibleSize);
            MinimapViewModel.DragAllow.AddObserver(this, OnDragAllow);
            MinimapViewModel.ZoomInOut.AddObserver(this, OnZoomInOut);
        }

        private void OnDisable()
        {
            MinimapViewModel.VisibleScreenPoint.RemoveAllObserver(this);
            MinimapViewModel.VisibleSize.RemoveAllObserver(this);
            MinimapViewModel.DragAllow.RemoveAllObserver(this);
            MinimapViewModel.ZoomInOut.RemoveAllObserver(this);
        }

        private void Update()
        {
            if (isEnter && MinimapInputOp.IsPlatformStandalone())
            {
                if (MinimapInputOp.ZoomAsWheel(
                    XrSettings.MinimapCamera, Input.mouseScrollDelta.y))
                {
                    MinimapViewModel.MeasureCameraRenderSize(XrSettings.MinimapCamera);
                    MinimapViewModel.MeasureCameraVisibleSize(XrSettings.MinimapCamera);
                }
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!isDragAllow) return;
            MeasureLocalPosition(eventData, out var local);
            OnStorePrevPosition(local);
            MinimapViewModel.CameraModeEvent.Post(MinimapCameraMode.Free);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!isDragAllow) return;
            if (MinimapInputOp.IsPlatformStandalone())
            {
                ProcessMove(eventData);
            }
            else
            {
                if (Input.touchCount > 1)
                {
                    //Debug.Log($"OnDrag {Input.touchCount}");
                    MinimapInputOp.ZoomAsPinch(XrSettings.MinimapCamera);
                    MinimapViewModel.MeasureCameraRenderSize(XrSettings.MinimapCamera);
                    MinimapViewModel.MeasureCameraVisibleSize(XrSettings.MinimapCamera);
                }
                else
                {
                    ProcessMove(eventData);
                }
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!isDragAllow) return;
            OnStorePrevPosition(Vector2.zero);

            if (isDragging)
            {
                isDragging = false;
                OnPositionWhenDrag.Invoke(XrSettings.MinimapCamera.transform.position);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            isEnter = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            isEnter = false;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (isDragging) return;

            MeasureHitPosition(eventData, out var hit);
            //Debug.Log($"OnPointerClick : {localPoint}/{rect}/{hit}");
            OnPositionWhenClick.Invoke(hit);
        }

        private void OnStorePrevPosition(Vector2 position)
        {
            this.position = position;
        }

        private void MeasureLocalPosition(PointerEventData eventData, out Vector2 localPosition)
        {
            if(minimapImage == null)
            {
                localPosition = Vector2.zero;
                Debug.LogWarning("MinmapImage is Null");
                return;
            }

            var rectTransform = minimapImage.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
                eventData.position, eventData.pressEventCamera, out localPosition);

            var rect = rectTransform.rect;
            localPosition.x /= rect.width;
            localPosition.y /= rect.height;
            localPosition += rectTransform.pivot;
        }

        private void MeasureHitPosition(PointerEventData eventData, out Vector3 hit)
        {
            MeasureLocalPosition(eventData, out var localPoint);

            var ray = XrSettings.MinimapCamera.ViewportPointToRay(localPoint);
            plane.Raycast(ray, out var d);

            hit = ray.GetPoint(d);
        }

        private void ProcessMove(PointerEventData eventData)
        {
            isDragging = true;
            MeasureLocalPosition(eventData, out var localPosition);
            var delta = position - localPosition;
            position = localPosition;
            //Debug.Log($"OnDrag {currentNormalizePosition.x.ToString("N6")} / {normalizeDelta[0].x.ToString("N6")}");
            MinimapInputOp.PanTranslate(XrSettings.MinimapCamera, ref delta);
        }

        private void OnVisibleScreenPoint(Vector2 lbScreenPoint,
                Vector2 rtScreenPoint, Vector2 rbScreenPoint)
        {
            MinimapInputOp.Env.UpdateScreenSize(
                Math.Abs(rtScreenPoint.x - lbScreenPoint.x),
                Math.Abs(rtScreenPoint.y - lbScreenPoint.y));
            //Debug.Log($"OnVisibleScreenPoint {lbScreenPoint}/{rtScreenPoint}/{MinimapInputOp.Env.ScreenWidth},{MinimapInputOp.Env.ScreenHeight}");
        }

        private void OnVisibleSize(float w, float h)
        {
            MinimapInputOp.Env.UpdateVisibleSize(w, h);
        }

        private void OnDragAllow(bool ret)
        {
            isDragAllow = ret;
        }

        private void OnZoomInOut(float mouseScrollDeltaY)
        {
            //Debug.Log($"OnZoomInOut {mouseScrollDeltaY}");
            if (MinimapInputOp.ZoomAsWheel(
                    XrSettings.MinimapCamera, mouseScrollDeltaY))
            {
                MinimapViewModel.MeasureCameraRenderSize(XrSettings.MinimapCamera);
                MinimapViewModel.MeasureCameraVisibleSize(XrSettings.MinimapCamera);
            }
        }
    }
}
