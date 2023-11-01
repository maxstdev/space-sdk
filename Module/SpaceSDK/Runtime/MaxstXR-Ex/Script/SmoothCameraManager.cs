using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using static IbrManager;

namespace MaxstXR.Extension
{
    [RequireComponent(typeof(Camera))]
    public class SmoothCameraManager : MonoBehaviour, IHighQualityDelegate
    {
        private const float UnitStepDistance = 0.7f;
        private const float UnitStepPerSecond = 6.0f;
        public const float DistancePerSecond = UnitStepDistance * UnitStepPerSecond;

        private const float UnitRotateAngle = 5f;
        private const float UnitRotatePerSecond = 24.0f;
        public const float RotatePerSecond = UnitRotateAngle * UnitRotatePerSecond;

        public const float DefaultSecondAtPos = 0.75f;
        public const float DefaultSecondAtRotate = 0.3f;

        public enum State
        {
            Idle,
            Translate,
        }

        [SerializeField] private UnityEvent OnAnimationStarted;
        [SerializeField] private UnityEvent<float, float> OnAnimationUpdated;
        [SerializeField] private UnityEvent OnAnimationFinished;
        [SerializeField] private UnityEvent OnFailToMove;
        [SerializeField] private Transform Cursor;
        [SerializeField] private LayerMask cursorLayerMask;

        [field: SerializeField] public float RotateSpeed { get; private set; } = 2f;
        [field: SerializeField] public float EditorRotateSpeed { get; private set; } = 5f;
        [field: SerializeField] public float EditorKeyRotateSpeed { get; private set; } = 0.7f;
        [field: SerializeField] public bool RotationDirection { get; private set; } = true;

        private readonly Queue<Transition> transitions = new();
        private readonly HighQualityHandler highQualityHandler = new();

        private CursorLockMode _prevCursorLockMode;

        public bool IsARMode { get; private set; } = false;

        public PovController povController = null;

        private SmoothTextureManager _textureManager;
        private SmoothTextureManager TextureManager
        {
            get
            {
                if (!_textureManager)
                    _textureManager = FindObjectOfType<SmoothTextureManager>(true);
                return _textureManager;
            }
        }

        public Camera Camera { get; private set; }

        public KnnManager KnnManager { get; private set; }

        public PovManager PovManager { get; private set; }

        public SmoothIbrManager IbrManager { get; private set; }

        public State CurrentState { get; private set; } = State.Idle;

        private float HorizontalFieldOfView
        {
            get
            {
                // Assume the camera uses Vertical field of view
                return Camera.VerticalToHorizontalFieldOfView(Camera.fieldOfView, Camera.aspect);
            }
        }

        public GameObject sphere;
        public GameObject cursor;

        private bool isSearchPov = false;
        private PovController lastSearchPovController = null;

        public bool minimapAutoMove = false;

        private void Awake()
        {
            IsARMode = XRStudioController.Instance.ARMode;
            if (!IsARMode)
            {
                KnnManager = FindObjectOfType<KnnManager>(true);
                PovManager = FindObjectOfType<PovManager>(true);
                IbrManager = FindObjectOfType<SmoothIbrManager>(true);
                if (IbrManager != null)
                {
                    AddListenerIfNotExists(OnAnimationStarted, IbrManager.HandleAnimationStarted);
                    AddListenerIfNotExists(OnAnimationUpdated, IbrManager.HandleAnimationUpdated);
                    AddListenerIfNotExists(OnAnimationFinished, IbrManager.HandleAnimationFinished);
                }
                Camera = GetComponent<Camera>();
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
                sphere.SetActive(true);
                cursor.SetActive(true);
                this.enabled = true;
            }
            else
            {
                sphere.SetActive(false);
                cursor.SetActive(false);
                this.enabled = false;
            }
        }

        private void OnEnable()
        {
            if (!IsARMode)
            {
                _prevCursorLockMode = UnityEngine.Cursor.lockState;
                UnityEngine.Cursor.lockState = CursorLockMode.None;

                povController = FindNearestPov(transform.position);
                Reload(povController, transform.rotation);
            }
        }

        private void OnDisable()
        {
            if (!IsARMode)
            {
                UnityEngine.Cursor.lockState = _prevCursorLockMode;
                if (CurrentState == State.Translate)
                {
                    FinishAnimating(true);
                }
            }
        }

        private void Update()
        {
            if (!IsARMode)
            {
                switch (CurrentState)
                {
                    case State.Translate:
                        if (transitions.Count > 0)
                        {
                            UpdateAnimation();
                        }
                        break;
                    default: //State.Idle:
                        break;
                }
            }
        }

        private void OnDestroy()
        {
            highQualityHandler.Reset();
        }

        private async void Reload(PovController nextPovController, Quaternion rotation)
        {
            if (!nextPovController) return;

            povController = nextPovController;
            // move the camera to the nearest pov
            transform.SetPositionAndRotation(nextPovController.transform.position, rotation);

            var _ = await TextureManager.LoadTexture(nextPovController, new CancellationTokenSource(),
                (st) =>
                {
                    IbrManager.StartFromRealTime(nextPovController, st);
                    UpdateRealTimePov(povController, nextPovController, rotation, rotation, DefaultSecondAtPos, DefaultSecondAtRotate, PovType.Primary);
                });
        }

        public void HandleMouseNavigation(ITransitionDelegate transitionDelegate)
        {
            var startPoint = transform.position;
            var nextPov = GetNextPovFromPosition(Cursor.transform.position);
            //var nextPovs = GetNextPovAroundFromPosition(Cursor.transform.position, 5);
            PovController nextPovController = nextPov.GetComponent<PovController>();
            PovController currentPovController = povController;
#if false

            if (Physics.Linecast(currentPovController.transform.position, nextPovController.transform.position, 
                out var hit2))//, CameraHandleSO.povLayerMask))
            {
                var hitDistance = hit2.distance;
                float povDistance = Vector3.Distance(nextPovController.transform.position, currentPovController.transform.position);
                if (hitDistance < povDistance)
                {
                    return;
                }
            }
#endif
            if (nextPovController == currentPovController)
            {
                return;
            }

            var transition = GenerateTransition(currentPovController, nextPovController,
                transform.rotation, transform.rotation,
                DefaultSecondAtPos, DefaultSecondAtRotate,
                new CancellationTokenSource(), transitionDelegate);
            if (transition == null)
            {
                //Debug.Log($"HandleMouseNavigation transition is null : {transitions.Count}");
                return;
            }
            transition.SourceObject = transitionDelegate;

            povController = nextPovController;
            HandleLoad2kImage(nextPovController, transition.cancellation,
                (t) =>
                {
                    transition.sharedTexture = t.Retain();
                    transition.isReady = true;
                    //Debug.Log("HandleMouseNavigation HandleLoad2kImage complete");
                }, transition.TransitionDelegate);
            //Debug.Log($"HandleMouseNavigation complete");
        }

        public void HandleKeyboardNavigation(ITransitionDelegate transitionDelegate)
        {
            //if (!CameraHandleSO.isPositionUpdate) return;

            var dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (dir.sqrMagnitude <= 0) return;
            dir = transform.right * dir.x + transform.forward * dir.z;
            dir = dir.normalized;

            PovController currentPovController = povController;
            Transform nextPovTransform = GetNextPovFromDirectionDistance(povController.transform.position, dir, 5);
            if (nextPovTransform == null)
            {
                return;
            }

            PovController nextPovController = nextPovTransform.GetComponent<PovController>();
            if (nextPovController == currentPovController)
            {
                return;
            }
#if false
            if (Physics.Linecast(currentPovController.transform.position, nextPovController.transform.position,
                out RaycastHit hit2, CameraHandleSO.povLayerMask))
            {
                if (hit2.transform.CompareTag("Position"))
                {
                    return;
                }
                var hitDistance = hit2.distance;
                float povDistance = Vector3.Distance(currentPovController.transform.position, nextPovController.transform.position);
                if (hitDistance < povDistance)
                {
                    return;
                }
            }
#endif

            var durationTimeAtPos = Vector3.Distance(currentPovController.transform.position, nextPovController.transform.position) / DistancePerSecond;
            var durationTimeAtRotate = Quaternion.Angle(transform.rotation, transform.rotation) / RotatePerSecond;

            var transition = GenerateTransition(currentPovController, nextPovController,
                transform.rotation, transform.rotation,
                durationTimeAtPos, durationTimeAtRotate,
                new CancellationTokenSource(), transitionDelegate);
            if (transition == null)
            {
                //Debug.Log($"HandleKeyboardNavigation transition is null : {transitions.Count}");
                return;
            }
            transition.SourceObject = transitionDelegate;
            povController = nextPovController;
            HandleLoad2kImage(nextPovController, transition.cancellation,
                (t) =>
                {
                    transition.sharedTexture = t.Retain();
                    transition.isReady = true;
                    //Debug.Log($"HandleKeyboardNavigation HandleLoad2kImage complete : {transition.ToPov.Name}");
                }, transition.TransitionDelegate);
            //Debug.Log($"HandleKeyboardNavigation complete");
        }

        public void HandlePovKeyFrame(PovKeyFrame keyFrame, ITransitionDelegate transitionDelegate)
        {
            //Debug.Log($"HandlePovKeyFrame : {keyFrame.CurrentPov.Name}/{keyFrame.NextPov.Name}");
            PovController currentPovController = keyFrame.CurrentPov;
            PovController nextPovController = keyFrame.NextPov;
            var durationTimeAtPos = keyFrame.DurationTimeAtPos == 0f ?
                Vector3.Distance(keyFrame.CurrentPosition, keyFrame.NextPosition) / DistancePerSecond : keyFrame.DurationTimeAtPos;
            var durationTimeAtRotate = keyFrame.DurationTimeAtRotate == 0f ?
                Quaternion.Angle(keyFrame.CurrentRotate, keyFrame.NextRotate) / RotatePerSecond : keyFrame.DurationTimeAtRotate;

            if (keyFrame.KeyFrameType == KeyFrameType.Wrap
                || keyFrame.KeyFrameType == KeyFrameType.WrapWith8K)
            {
                IbrManager.SetPov(nextPovController.GetComponent<IPov>(), PovType.Primary);
            }
            //Debug.Log($"HandlePovKeyFrame time : {durationTimeAtPos}/{durationTimeAtRotate}/{keyFrame.DurationTimeAtPos}/{keyFrame.DurationTimeAtRotate}");
            var transition = GenerateTransition(currentPovController, nextPovController,
                keyFrame.CurrentRotate, keyFrame.NextRotate,
                durationTimeAtPos, durationTimeAtRotate, 
                new CancellationTokenSource(), transitionDelegate,
                keyFrame.GetHighQualityState());
            if (transition == null)
            {
                //Debug.Log($"HandlePovKeyFrame transition is null : {transitions.Count}");
                return;
            }

            transition.SourceObject = keyFrame;
            povController = nextPovController;
            HandleLoad2kImage(nextPovController, transition.cancellation,
                (t) =>
                {
                    transition.sharedTexture = t.Retain();
                    transition.isReady = true;
                    //Debug.Log("HandlePovKeyFrame HandleLoad2kImage complete");
                }, transition.TransitionDelegate);
            //Debug.Log($"HandlePovKeyFrame complete");
        }

        public void UpdateInputRotate(bool adjustAngleX = true)
        {
            if (CurrentState != State.Idle) return;
            float directionModifier = RotationDirection ? -1f : 1f;
#if UNITY_EDITOR
            transform.Rotate(0f, directionModifier * Input.GetAxis("Mouse X") * EditorRotateSpeed, 0f, Space.World);
            transform.Rotate(-directionModifier * Input.GetAxis("Mouse Y") * EditorRotateSpeed, 0f, 0f, Space.Self);

#else
            transform.Rotate(0f, directionModifier * Input.GetAxis("Mouse X") * RotateSpeed, 0f, Space.World);
            transform.Rotate(-directionModifier * Input.GetAxis("Mouse Y") * RotateSpeed, 0f, 0f, Space.Self);
#endif
            if (adjustAngleX)
            {
                var angles = transform.eulerAngles;
                var symmetricX = Mathf.Asin(Mathf.Sin(Mathf.Deg2Rad * angles.x)) * Mathf.Rad2Deg;
                angles.x = Mathf.Clamp(symmetricX, -50f, 60f - Camera.fieldOfView / 2f);
                angles.z = 0;
                transform.rotation = Quaternion.Euler(angles);
            }
        }

        public void UpdateInputKeyRotate(bool isRight = true)
        {
            if (CurrentState != State.Idle) return;

            int direction = (isRight ? 1 : -1);

#if UNITY_EDITOR
            transform.Rotate(0f, (EditorKeyRotateSpeed * direction), 0f, Space.World);
#else
			transform.Rotate(0f, (EditorKeyRotateSpeed * direction), 0f, Space.World);
#endif
        }

        public void UpdateInputKeyUpDown(bool isUp = true)
        {
            if (CurrentState != State.Idle) return;

            int direction = (isUp ? -1 : 1);

#if UNITY_EDITOR
            transform.Rotate((EditorKeyRotateSpeed * direction), 0f, 0f, Space.Self);
#else
			transform.Rotate((EditorKeyRotateSpeed * direction), 0f, 0f, Space.Self);
#endif
        }

        public void SearchPovNearCursor()
        {
            if (isSearchPov) return;
            isSearchPov = true;
            SearchPovNearCursorAsync(Cursor.transform.position);
        }

        private bool IsScreenArea(Camera cam, Vector3 position)
        {
            var cp = cam.WorldToScreenPoint(position);
            return !(cp.x < 0 || cp.y < 0 || cp.x > Screen.width || cp.y > Screen.height);
        }

        private async void SearchPovNearCursorAsync(Vector3 cursorPosition)
        {
            await Task.Yield();

            var findPov = FindNearestPov(cursorPosition);
            if (findPov && IsScreenArea(Camera, findPov.transform.position))
            {
                if (lastSearchPovController) lastSearchPovController.SetSelected(false);
                //Debug.Log($"SearchPovNearCursorAsync findPov : {findPov.Name}");
                findPov.SetSelected(true);
                lastSearchPovController = findPov;
            }
            else
            {
                if (lastSearchPovController) lastSearchPovController.SetSelected(false);
                lastSearchPovController = null;
            }

            isSearchPov = false;
        }

        private List<int> GenerateBounds(params float[] positions)
        {
            var template = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            var bounds = new List<int>();
            foreach (var position in positions ?? new float[0])
            {
                var angle = position > 360 ? position - 360 : position < 0 ? position + 360 : position;
                switch (angle)
                {
                    case >= 0 and < 90:
                        if (template.Remove(3)) bounds.Add(3);
                        if (template.Remove(7)) bounds.Add(7);
                        break;
                    case >= 90 and < 180:
                        if (template.Remove(4)) bounds.Add(4);
                        if (template.Remove(8)) bounds.Add(8);
                        break;
                    case >= 180 and < 270:
                        if (template.Remove(1)) bounds.Add(1);
                        if (template.Remove(5)) bounds.Add(5);
                        break;
                    default:
                        if (template.Remove(2)) bounds.Add(2);
                        if (template.Remove(6)) bounds.Add(6);
                        break;
                }
            }
            bounds.AddRange(template);
            return bounds;
        }

        public void Reload()
        {
            KnnManager = FindObjectOfType<KnnManager>(true);
            PovManager = FindObjectOfType<PovManager>(true);
            IbrManager = FindObjectOfType<SmoothIbrManager>(true);
            //Debug.Log($"KnnManager Reload {KnnManager}/{PovManager}{IbrManager} ");
            if (IbrManager != null)
            {
                AddListenerIfNotExists(OnAnimationStarted, IbrManager.HandleAnimationStarted);
                AddListenerIfNotExists(OnAnimationUpdated, IbrManager.HandleAnimationUpdated);
                AddListenerIfNotExists(OnAnimationFinished, IbrManager.HandleAnimationFinished);
            }
        }

        private void UpdateRealTimePov(
            PovController currentPovController, PovController nextPovController, 
            Quaternion currentRotation, Quaternion nextRotation,
            float durationTimeAtPos, float durationTimeAtRotate,
            PovType povType = PovType.Secondary,
            ITransitionDelegate transitionDelegate = null)
        {
            // set the primary pov to ibr manager
            IbrManager.SetPov(nextPovController.GetComponent<IPov>(), povType);
            var transition = GenerateTransition(currentPovController, nextPovController, 
                currentRotation, nextRotation,
                durationTimeAtPos, durationTimeAtRotate, 
                new CancellationTokenSource(), transitionDelegate);
            if (transition == null)
            {
                return;
            }

            povController = nextPovController;
            HandleLoad2kImage(nextPovController, transition.cancellation,
                (t) =>
                {
                    transition.sharedTexture = t.Retain();
                    transition.isReady = true;
                    //Debug.Log("UpdateRealTimePov HandleLoad2kImage complete");
                }, transition.TransitionDelegate);
        }

        private async void HandleLoad2kImage(PovController povController, CancellationTokenSource cancellation,
            UnityAction<SmoothSharedTexture> injectAction, ITransitionDelegate transitionDelegate)
        {
            var _ = await TextureManager.LoadTexture(povController, cancellation, injectAction, transitionDelegate);
        }


        void IHighQualityDelegate.Load8kImages(Transition transition, UnityAction<Transition> complete)
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            Vector3 currentPovRotation = povController.transform.rotation.eulerAngles;
            Vector3 povRotation = currentRotation - currentPovRotation;
            float povYRotation = povRotation.y < 0 ? povRotation.y + 360 : povRotation.y;
            float left_pov = povYRotation + HorizontalFieldOfView / 2;
            float right_pov = povYRotation - HorizontalFieldOfView / 2;


            var bounds = GenerateBounds(povYRotation, left_pov, right_pov);
            //Debug.Log($"Load8kImages bounds : {string.Join(",", bounds)}");

            IbrManager.ClearAllSplitMaterial(false);
            IbrManager.ClearAllTextureMaterial();
            LoadAndUpdateHighResolution(povController, transition, bounds, complete);
        }

        private IEnumerator HighResolutionOnMainthread(Transition transition, SmoothSharedTexture st)
        {
            yield return new WaitForEndOfFrame();
            if (transition.ToPov == povController)
            {
                switch (transition.highQualityState)
                {
                    case HighQualityState.Download:
                        highQualityHandler.Assign(st);
                        break;
                    case HighQualityState.Apply:
                        IbrManager.UpdateHighResolutionMaterial(st);
                        break;
                    default:
                        st.Release();
                        break;
                }
            }
            else
            {
                st.Release();
            }
        }

        private IEnumerator CompleteOnMainthread(Transition transition, UnityAction<Transition> complete)
        {
            yield return new WaitForEndOfFrame();
            complete?.Invoke(transition);
        }

        private async void LoadAndUpdateHighResolution(PovController povController,
            Transition transition, List<int> bounds, UnityAction<Transition> complete)
        {
            await TextureManager.LoadTextureBounds(povController, bounds, highQualityHandler.cancellation,
                (st) =>
                {
                    if (this) StartCoroutine(HighResolutionOnMainthread(transition, st));
                });

            if (this) StartCoroutine(CompleteOnMainthread(transition, complete));
        }

        private void HandleTexture(PovController toPov, SmoothSharedTexture st)
        {
            IbrManager.SetPov(toPov, PovType.Secondary);
            IbrManager.UpdateFrameTexture(st, PovType.Secondary);
        }

        private PovController FindNearestPov(Vector3 position)
        {
            var nearestObject = GetNextPovFromPosition(position);
            if (nearestObject == null)
            {
                return null;
            }

            return nearestObject.GetComponent<PovController>();
        }

        private Transform GetNextPovFromPosition(Vector3 position)
        {
            if (KnnManager)
            {
                var findObject = KnnManager.FindNearest(position);
                if (findObject) return findObject.transform;
            }
            return null;
        }

        private GameObject[] GetNextPovAroundFromPosition(Vector3 position, float distance)
        {
            if (KnnManager)
            {
                return KnnManager.FindWithinRange(position, distance);
            }
            else
            {
                return null;
            }
        }

        private Transform GetNextPovFromDirectionDistance(Vector3 position, Vector3 direction, float distnace)
        {
            var povs = GetNextPovAroundFromPosition(position, distnace);
            if (povs == null)
            {
                return null;
            }

            var filteredProximalPovs = new List<Transform>();
            foreach (var eachGameObject in povs)
            {
                if (eachGameObject.activeSelf)
                {
                    filteredProximalPovs.Add(eachGameObject.transform);
                }
            }

            var nearestCandidate = SelectOrderedFilteredByAngleDistance(povController.transform,
                filteredProximalPovs.ToArray(), direction, 40);

            if (null != nearestCandidate) return nearestCandidate;

            return null;
        }

        private Transform GetNextPovFromDirection(Vector3 direction)
        {
            var m = direction.sqrMagnitude;
            if (m > 1) { direction /= m; }

            var neighborObjects = KnnManager.FindNearestK(transform.position, 5);
            var displacements = neighborObjects.Select(go => go.transform.position - transform.position).ToArray();
            var dirSims = displacements.Select(v => v.normalized).Select(v => Vector3.Dot(v, direction));
            (float _, int i) = displacements.Select((v, i) => (v.sqrMagnitude, i)).Min();
            (float _, int j) = dirSims.Select((s, j) => (s, j)).Where((s, j) => i != j).Max();

            var nearestObject = neighborObjects[j];
            return nearestObject.transform;
        }

        public void UpdateCursor(RectTransform targetRectTransform = null)
        {
            var ray = new Ray();
            int displayIndex = 0;
            float distanceToClipPlane = Camera.farClipPlane;
            Camera.ScreenPointToRayFromRectTransform(targetRectTransform, ref ray, ref displayIndex, ref distanceToClipPlane);

            if (Physics.Raycast(ray, out RaycastHit hit, distanceToClipPlane, cursorLayerMask))
            {
                Cursor.transform.position = hit.point;
                Cursor.rotation = Quaternion.FromToRotation(Vector3.forward, -hit.normal);
                Cursor.gameObject.SetActive(true);
            }
            else
            {
                Cursor.gameObject.SetActive(false);
            }
        }

        public void UpdateCursorTexture(Texture2D texture)
        {
            Cursor.GetComponent<MeshRenderer>().material.mainTexture = texture;
        }

        private Transition GenerateTransition(PovController fromPov, PovController toPov,
            Quaternion fromRotation, Quaternion toRotation,
            float durationTimeAtPos, float durationTimeAtRotate,
            CancellationTokenSource cancellation, ITransitionDelegate transitionDelegate, 
            HighQualityState highQualityState = HighQualityState.Download)
        {
            if (transitions.Count > 1)
            {
                //마지막 데이터를 변경하는 것은 애니메이션 360 이미지 매핑이 틀어지는 현상 발생 가능성 있음.
                //Debug.Log($"GenerateTransition transitions.Count : {transitions.Count}");
                return null;
            }

            if (transitions.Count == 1)
            {
                var last = transitions.Last();
                if (last.highQualityState != HighQualityState.Cancel)
                {
                    last.highQualityState = HighQualityState.Cancel;
                    highQualityHandler.RequestCancel();
                }
            }

            //Debug.Log($"GenerateTransition : {durationTimeAtPos}/{durationTimeAtRotate}");

            var transition = new Transition(fromPov, toPov,
                fromRotation, toRotation,
                durationTimeAtPos, durationTimeAtRotate,
                cancellation, transitionDelegate)
            {
                highQualityState = highQualityState,
            };
            transitions.Enqueue(transition);
            CurrentState = State.Translate;
            return transition;
        }

        private Transition GenerateMinimapMoveTransition(PovController fromPov, PovController toPov,
            Quaternion fromRotation, Quaternion toRotation, CancellationTokenSource cancellation,
            ITransitionDelegate downloadDelegate, bool isLast = false)
        {
            //if (transitions.Count > 1)
            //{
            //    //마지막 데이터를 변경하는 것은 애니메이션 360 이미지 매핑이 틀어지는 현상 발생 가능성 있음.
            //    return null;
            //}

            var transition = new Transition(fromPov, toPov,
                fromRotation, toRotation,
                DefaultSecondAtPos, DefaultSecondAtRotate,
                cancellation, downloadDelegate)
            {
                highQualityState = HighQualityState.Download,
            };
            transitions.Enqueue(transition);
            CurrentState = State.Translate;

            return transition;
        }

        private void UpdateAnimation()
        {
            if (transitions.TryPeek(out var transition))
            {
                if (transition.isReady == false)
                {
                    if (transition.OnWaitForReady())
                    {
                        transitions.Dequeue();
                        OnFailToMove?.Invoke();
                    }
                    //Debug.Log($"UpdateAnimation transition.isReady is false : {transition.ToPov.Name}");
                    return;
                }

                OnFirstAnimation(transition);
                transition.UpdateAnimationParameter(OnAnimationUpdated);
                transition.UpdatePositionAndRotation(transform);
                OnLastAnimation(transition);
            }
        }

        private void OnFirstAnimation(Transition transition)
        {
            if (transition.IsAnimationStart())
            {
                //Debug.Log($"OnFirstAnimation : {transition.ToPov}/{transition.highQualityState}");
                HandleTexture(transition.ToPov, transition.sharedTexture);
                OnAnimationStarted.Invoke();
                transition.TransitionDelegate?.AnimationStarted(transition.FromPov, transition.SourceObject);
                if (transition.highQualityState == HighQualityState.Download)
                {
                    highQualityHandler.Config(transition, this);
                }
                else
                {
                    IbrManager.ClearAllTextureMaterial();
                }
            }
        }

        private void OnLastAnimation(Transition transition)
        {
            if (transition.IsAnimationEnd())
            {
                FinishAnimating(false);

                //Debug.Log($"OnLastAnimation : {transition.ToPov.Name}/{transition.highQualityState}");
                if (transition.highQualityState == HighQualityState.Download)
                {
                    transition.highQualityState = HighQualityState.Apply;
                    highQualityHandler.ToInject(IbrManager);
                }

                //Debug.Log($"OnLastAnimation fromPov -> toPov : {transition.fromPov.Name} /\n {transition.toPov.Name}");
            }
        }

        private void FinishAnimating(bool force = false)
        {
            if (force)
            {
                if (transitions.Count > 0)
                {
                    var lastTransition = transitions.Last();
                    lastTransition.ForceTimeEnd();
                    lastTransition.UpdatePositionAndRotation(transform);
                }

                foreach (var t in transitions)
                {
                    t.sharedTexture?.Release();
                    t.sharedTexture = null;
                    t.highQualityState = HighQualityState.Cancel;
                }
                transitions.Clear();
            }
            else
            {
                var transition = transitions.Dequeue();
                transition.sharedTexture?.Release();
                transition.sharedTexture = null;
                transition.TransitionDelegate?.AnimationFinished(transition.ToPov, transition.SourceObject);
                OnAnimationFinished.Invoke();
#if false
                if (minimapAutoMove == true)
                {
                    povController = transition.ToPov;

                    if (transitions.Count == 0)
                    {
                        minimapAutoMove = false;
                        UpdateRealTimePov(transition.FromPov,
                            transition.ToPov,
                            this.transform.rotation,
                            transition.ToRotation,
                            1,
                            1,
                            PovType.Primary);
                        SequenceFinished.Invoke();
                    }
                }
#endif
            }
            CurrentState = transitions.Count == 0 ? State.Idle : State.Translate;
        }

        public bool IsAddableTransitions()
        {
            return transitions.Count < 2;
        }

        private static void AddListenerIfNotExists(UnityEvent unityEventBase, UnityAction unityAction)
        {
            if (!CheckListenerExists(unityEventBase, nameof(unityAction)))
                unityEventBase.AddListener(unityAction);
        }

        private static void AddListenerIfNotExists<T0>(UnityEvent<T0> unityEventBase, UnityAction<T0> unityAction)
        {
            if (!CheckListenerExists(unityEventBase, nameof(unityAction)))
                unityEventBase.AddListener(unityAction);
        }

        private static void AddListenerIfNotExists<T0, T1>(UnityEvent<T0, T1> unityEventBase, UnityAction<T0, T1> unityAction)
        {
            if (!CheckListenerExists(unityEventBase, nameof(unityAction)))
                unityEventBase.AddListener(unityAction);
        }

        private static bool CheckListenerExists(UnityEventBase unityEventBase, string methodName)
        {
            var eventCount = unityEventBase.GetPersistentEventCount();
            if (0 == eventCount) return false;
            return Enumerable.Range(0, eventCount).Select(unityEventBase.GetPersistentMethodName).Contains(methodName);
        }

        private static IEnumerable<Transform> SelectOrderedFilteredByAngle(Transform current, Transform[] candidates, Vector3 direction, float angle)
        {
            var angles = SelectAngle(current, candidates, direction).ToArray();
            return SelectOrderedFiltered(candidates, angles, angle).ToArray();
        }

        private static Transform SelectOrderedFilteredByAngleDistance(Transform current, Transform[] candidates, Vector3 direction, float angle)
        {
            if (candidates.Length == 0)
            {
                return null;
            }

            var angles = SelectAngle(current, candidates, direction).ToArray();

            List<Transform> filterByAngle = new List<Transform>();
            for (int i = 0; i < angles.Length; i++)
            {
                float selectAngle = angles[i];
                if (selectAngle < angle)
                {
                    filterByAngle.Add(candidates[i]);
                }
            }

            float shortDistance = 100;
            Transform shortTransform = null;
            foreach (Transform eachTransform in filterByAngle)
            {
                float distance = Vector3.Distance(eachTransform.transform.position, current.position);
                if (shortDistance > distance)
                {
                    shortTransform = eachTransform;
                    shortDistance = distance;
                }
            }

            return shortTransform;
        }

        private static IEnumerable<float> SelectAngle(Transform current, Transform[] candidates, Vector3 direction)
        {
            return candidates.Select(candidate => (candidate.position - current.position).normalized)
                .Select(d => Mathf.Acos(Vector3.Dot(d, direction)) * Mathf.Rad2Deg);
        }

        private static IEnumerable<Transform> SelectOrderedFiltered(Transform[] povs, float[] values, float maxValue)
        {
            return povs.Select((pov, index) => (pov, value: values[index]))
                .OrderBy(pair => pair.value)
                .Where(pair => pair.value <= maxValue)
                .Select(pair => pair.pov);
        }

        public void WarpToPose(Vector3 nearestPosition, Quaternion nearestRotation)
        {
            povController = FindNearestPov(nearestPosition);
            if (povController)
            {
                UpdateRealTimePov(povController, povController, nearestRotation, nearestRotation, DefaultSecondAtPos, DefaultSecondAtRotate, PovType.Primary);
            }
        }

        public bool WarpEffectToPose(Vector3 nearestPosition, Quaternion nearestRotation, ITransitionDelegate transitionDelegate)
        {
            var nextPovController = FindNearestPov(nearestPosition);

            Debug.Log($"WarpEffectToPose next : {nextPovController}");
            if (nextPovController)
            {
                var durationTimeAtPos = Vector3.Distance(povController.transform.position, nextPovController.transform.position) / DistancePerSecond;
                var durationTimeAtRotate = Quaternion.Angle(nearestRotation, nearestRotation) / RotatePerSecond;
                UpdateRealTimePov(povController, nextPovController, nearestRotation, nearestRotation, 
                    durationTimeAtPos, durationTimeAtRotate, PovType.Secondary, transitionDelegate);
                return true;
            }
            return false;
        }

        public bool WarpEffectToPoseDiraction(Vector3 nearestPosition, ITransitionDelegate transitionDelegate)
        {
            var nextPovController = FindNearestPov(nearestPosition);
            var nearestRotation = povController.transform.position.ToRotate(nextPovController.transform.position, transform.rotation);
            Debug.Log($"WarpEffectToPoseDiraction next : {nextPovController}");
            if (nextPovController)
            {
                var durationTimeAtPos = Vector3.Distance(povController.transform.position, nextPovController.transform.position) / DistancePerSecond;
                var durationTimeAtRotate = Quaternion.Angle(povController.transform.rotation, nearestRotation) / RotatePerSecond;
                UpdateRealTimePov(povController, nextPovController, nearestRotation, nearestRotation,
                    durationTimeAtPos, durationTimeAtRotate, PovType.Secondary, transitionDelegate);
                return true;
            }
            return false;
        }

        public void ChangedLayerMask(string layerName, bool isShow = true)
        {
            int getLayer = LayerMask.GetMask(layerName);
            if (getLayer == 0)
                return;

            if (isShow == true)
            {
                Camera.cullingMask |= getLayer;
            }
            else
            {
                Camera.cullingMask &= ~(getLayer);
            }

            //defaultRenderMask = Camera.cullingMask;
        }

        public async UniTask<(PovController, Quaternion)> MinimapMovePov(Vector3 destPosition, PovController startPov, 
            Quaternion startQuater, bool isLast = false, ITransitionDelegate downloadDelegate = null)
        {
            var threshold = 0.0f;
            var distance = 0.0f;

            var nextPovController = FindNearestPov(destPosition);
            if (povController == nextPovController)
            {
                return (povController, Camera.transform.rotation);
            }

            distance = Vector3.Distance(startPov.transform.position, destPosition);
            if (distance < 3.0f)
            {
                return (startPov, startQuater);
            }

            await UniTask.Delay(100);

            Vector3 destPos = new Vector3(nextPovController.transform.position.x, Camera.transform.position.y, nextPovController.transform.position.z);
            Vector3 startPos = (startPov == null ? Camera.transform.position : new Vector3(startPov.transform.position.x, Camera.transform.position.y, startPov.transform.position.z));

            Vector3 dir = destPos - startPos;// transform.position;

            if (dir == Vector3.zero)
            {
                return (nextPovController, startQuater);
            }

            Quaternion destRot = Quaternion.LookRotation(dir);
            if (Quaternion.Angle(destRot, startQuater) < threshold)
            {
                destRot = startQuater;
            }

#if true
            var transition = GenerateMinimapMoveTransition(startPov, nextPovController,
                (startQuater == null ? transform.rotation : startQuater), destRot, new CancellationTokenSource(), downloadDelegate, isLast);
            if (transition == null)
            {
                return (null, startQuater);
            }

            HandleLoad2kImage(nextPovController, transition.cancellation,
                (t) =>
                {
                    transition.sharedTexture = t.Retain();
                    transition.isReady = true;
                    //Debug.Log("UpdateRealTimePov HandleLoad2kImage complete");
                }, transition.TransitionDelegate);
#else
            UpdateRealTimePov(nextPovController, destRot);
#endif
            //await UniTask.WaitUntil(() => transition.isReady == true);

            return (nextPovController, destRot);
        }


        public async UniTask RotatePov(PovController startPov,
            Quaternion startQuaternion,
            Quaternion endQuaternion,
            bool isLookAround,
            bool isLast = false)
        {
#if true
            var transition = GenerateMinimapMoveTransition(startPov, startPov,
                startQuaternion, endQuaternion, new CancellationTokenSource(), null, isLast);

            //transition.isReady = true;
            HandleLoad2kImage(startPov, transition.cancellation,
            (t) =>
            {
                transition.sharedTexture = t.Retain();
                transition.isReady = true;
                //transition.duration = 7.0f;
                //transition.isSequenceRotation = isLookAround;
                //Debug.Log("UpdateRealTimePov HandleLoad2kImage complete");
            }, transition.TransitionDelegate);

            if (transition == null)
            {
                return;
            }
#endif
        }

    }
}