using MaxstUtils;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MaxstXR.Extension
{
    public class CameraCanvas : InSceneUniqueBehaviour, IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerMoveHandler, ITransitionDelegate
    {
        public static CameraCanvas Instance(GameObject go) => Instance<CameraCanvas>(go);

        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private SmoothCameraManager cameraManager;
        [SerializeField] private GameObject progressPrefeb;

        private Matrix4x4 lastProjectionMatrix = Matrix4x4.identity;
        //ICameraTranslate
        private Vector2 frustumSize = Vector2.zero;
        private Coroutine inputKeyEventCoroutine = null;
        private GameObject progressObject = null;

		private bool isDrag = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            StartInputKeyEvent();
			if (TryGetComponent<Canvas>(out var canvas))
			{
				canvas.worldCamera = cameraManager.GetComponent<Camera>();
			}
		}

        private void Update()
        {
            if (lastProjectionMatrix == mainCamera.projectionMatrix) return;
            lastProjectionMatrix = mainCamera.projectionMatrix;
            mainCamera.fieldOfView = mainCamera.CalculateFieldOfView();
            
            var canvasRectTransform = GetComponent<RectTransform>();
            canvasRectTransform.localPosition = new(0, 0, mainCamera.FarPlaneDistance());
            canvasRectTransform.localScale = new(mainCamera.FrustumWidth(), mainCamera.FrustumHeight(), 1);
            frustumSize.Set(mainCamera.FrustumWidth(), mainCamera.FrustumHeight());
            //Debug.Log($"CameraCanvas {canvasRectTransform.localPosition}/{canvasRectTransform.localScale}");
        }

        private void StartInputKeyEvent()
        {
            StopInputKeyEvent();
            inputKeyEventCoroutine = StartCoroutine(InputKeyEvent());
        }

        private void StopInputKeyEvent()
        {
            if (inputKeyEventCoroutine != null)
            {
                StopCoroutine(inputKeyEventCoroutine);
                inputKeyEventCoroutine = null;
            }
        }

        private IEnumerator InputKeyEvent()
        {
            yield return new WaitWhile(() =>
            {
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)
                    || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S))
                {
                    cameraManager.HandleKeyboardNavigation(this);
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    cameraManager.UpdateInputKeyRotate(false);
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    cameraManager.UpdateInputKeyRotate();
                }
                else if (Input.GetKey(KeyCode.UpArrow))
                {
                    cameraManager.UpdateInputKeyUpDown();
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    cameraManager.UpdateInputKeyUpDown(false);
                }
                else
                {

                }
                return true;
            });
        }


        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log($"CameraCanvas OnPointerClick");
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
					if (isDrag == false)
					{
						cameraManager.HandleMouseNavigation(this);
					}
					
					break;
                case PointerEventData.InputButton.Right:
                    //cameraManager.HandleMouseNavigation(this);
                    break;
                default:
                    break;
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    
                    break;
                default:
                    break;
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {

            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    cameraManager.UpdateInputRotate();
					isDrag = true;

					break;
                default:
                    break;
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
					isDrag = false;

					break;
                default:
                    break;
            }
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (!cameraManager.IsARMode)
            {
                cameraManager.UpdateCursor();
                cameraManager.SearchPovNearCursor();
            }
        }

        void ITransitionDelegate.DownloadStart()
        {
            
        }

        void ITransitionDelegate.DownloadProgess(float f)
        {
            InstantiateProgress();
            //progressObject.GetComponentInChildren<Text>().text = $"{(int)f}%";
        }

        void ITransitionDelegate.DownloadComplete()
        {
            DestroyProgress();
        }

        void ITransitionDelegate.DownloadException(Exception e)
        {
            DestroyProgress();
        }

        void ITransitionDelegate.DownloadException(UnityWebRequest www)
        {
            DestroyProgress();
        }

        private void InstantiateProgress()
        {
            if (!progressObject)
            {
                progressObject = Instantiate(progressPrefeb, rootCanvas.transform);
            }
        }

        private void DestroyProgress()
        {
            if (progressObject)
            {
                if (Application.isPlaying) 
                {
                    GameObject.Destroy(progressObject);
                }
                else
                {
                    GameObject.DestroyImmediate(progressObject);
                }
                progressObject = null;
            }
        }
    }
}
