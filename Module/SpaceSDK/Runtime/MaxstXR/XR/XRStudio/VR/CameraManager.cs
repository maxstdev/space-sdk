using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    public enum State
    {
        Idle, Moving
    }

    #region Serialized Fields
    
    [SerializeField]
    private UnityEvent OnAnimationStarted;

    [SerializeField]
    private UnityEvent<float> OnAnimationUpdated;

    [SerializeField]
    private UnityEvent OnAnimationFinished;

    [SerializeField]
    private Transform Cursor;

    public float Speed = 0.1f, RotateSpeed = 0.01f;
    
    #endregion

    #region Private Fields

    private State _currentState = State.Idle;
    private float _t;
    private bool _keepPosition = false, _keepRotation = false;
    private Vector3 _animateFromPosition, _animateToPosition;
    private Quaternion _animateFromRotation, _animateToRotation;
    private CursorLockMode _prevCursorLockMode;
    private bool isARMode;
    private Coroutine image8kCoroutine;

    private bool isMoveEnable = true;
    public bool isUpDownEnable = true;

    private Vector3 startPoint;
    private Vector3 direction;

    public PovController povController = null;

    private float m_DoubleClickSecond = 0.25f;
    private bool m_IsOneClick = false;
    private double m_Timer = 0;

    #endregion

    #region Public Properties

    private TextureManager _textureManager;
    private TextureManager TextureManager
    {
        get
        {
            if (null == _textureManager)
                _textureManager = FindObjectOfType<TextureManager>(true);
            return _textureManager;
        }
    }


    public Camera Camera { get; private set; }

    public KnnManager KnnManager { get; private set; }

    public PovManager PovManager { get; private set; }

    public IbrManager IbrManager { get; private set; }

    public State CurrentState
    {
        get { return _currentState; }
        set
        {
            if (value == _currentState) return;

            switch (_currentState)
            {
                case State.Idle:
                    _t = 0f;
                    OnAnimationStarted.Invoke();
                    break;
                case State.Moving:
                    _t = 1f;
                    OnAnimationFinished.Invoke();
                    break;
                default:
                    return; // never happens
            }

            _currentState = value;
        }
    }

    public bool Moving { get => CurrentState == State.Moving; }

    private float HorizontalFieldOfView
    {
        get
        {
            // Assume the camera uses Vertical field of view
            return Camera.VerticalToHorizontalFieldOfView(Camera.fieldOfView, Camera.aspect);
        }
    }

    public bool IgnoreKeyboardNavigation { get; private set; }
    public bool IgnoreMouseNavigation { get; private set; }

    public GameObject sphere;
    public GameObject cursor;

    private PovController readyNextPovController;
    private Vector3 readyNextPovRotation;
    private System.Action<int> movePointAction;

    #endregion

    #region Unity Messages

    void Awake()
    {
        isARMode = XRStudioController.Instance.ARMode;
        if(!isARMode)
        {
            if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                Speed = 4;
                RotateSpeed = 5;
            }
            KnnManager = FindObjectOfType<KnnManager>(true);
            PovManager = FindObjectOfType<PovManager>(true);
            IbrManager = FindObjectOfType<IbrManager>(true);
            AddListenerIfNotExists(OnAnimationStarted, IbrManager.HandleAnimationStarted);
            AddListenerIfNotExists(OnAnimationUpdated, IbrManager.HandleAnimationUpdated);
            AddListenerIfNotExists(OnAnimationFinished, IbrManager.HandleAnimationFinished);
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

    async void OnEnable()
    {
        if (!isARMode)
        {
            _prevCursorLockMode = UnityEngine.Cursor.lockState;
            UnityEngine.Cursor.lockState = CursorLockMode.None;

            IgnoreKeyboardNavigation = false;
            IgnoreMouseNavigation = false;

            await InitializeNavigation();

            //var nearestObject = GetNextPovFromPosition(transform.position);
            //transform.position = nearestObject.transform.position;
            //IbrManager.SetPov(nearestObject.GetComponent<IPov>(), IbrManager.PovType.Primary);
        }
    }

    void OnDisable()
    {
        if (!isARMode)
        {
            UnityEngine.Cursor.lockState = _prevCursorLockMode;
            if (Moving) { FinishAnimating(); }
        }
    }

    void Start()
    {
        if (!isARMode)
        {
            if (null == Cursor) { Debug.LogError("Cursor is null"); }
            FinishAnimating();
        }
    }

    async void Update()
    {
        if (!isARMode)
        {
            if (Moving)
            {
                if (_t < 1f)
                {
                    UpdateAnimationParameter();
                }
                else
                {
                    FinishAnimating();
                }
                return;
            }

            if (!IgnoreKeyboardNavigation)
            {
                IgnoreKeyboardNavigation = true;
                await HandleKeyboardNavigation();
                IgnoreKeyboardNavigation = false;
            }

            if (!IgnoreMouseNavigation)
            {
                IgnoreMouseNavigation = true;
                await HandleMouseNavigation();
                IgnoreMouseNavigation = false;
            }

            if (readyNextPovController != null)
            {
                await MoveToPov(readyNextPovController, readyNextPovRotation);
                readyNextPovController = null;
                readyNextPovRotation = Vector3.zero;
            }

            UpdateCursor();
        }
    }

    public void SetExtraPovMoveAction(System.Action<int> movePoint)
    {
        this.movePointAction = movePoint;
    }

    public void SetNextPov(PovController pov)
    {
        readyNextPovController = pov;
        readyNextPovRotation = Vector3.zero;
    }

    public void SetNextPov(Vector3 position, Vector3 rotation)
    {
        var nextPov = GetNextPovFromPosition(position);
        PovController nextPovController = nextPov.GetComponent<PovController>();
        //readyNextPovController = nextPovController;
        // readyNextPovRotation = rotation;

        transform.position = position;
        transform.eulerAngles = rotation;

        Task.Run(() => MoveRealTimeNavigation(nextPovController, rotation));

    }

    private int[] CheckBounds(float left, float right, float center)
    {
        Dictionary<int, int> bounds = new Dictionary<int, int>();
        int[] center_bound = CheckBound(center);
        int[] left_bound = CheckBound(left);
        int[] right_bound = CheckBound(right);

        List<int> temp = new List<int>();
        temp.AddRange(center_bound);
        temp.AddRange(left_bound);
        temp.AddRange(right_bound);

        foreach (int eachInt in temp)
        {
            bounds[eachInt] = 1;
        }

        return bounds.Keys.ToArray();
    }

    private int[] CheckBound(float position)
    {
        List<int> result = new List<int>();
        if (position > 360)
        {
            position = position - 360;
        }

        if (position < 0)
        {
            position = position + 360;
        }

        if (position >= 0 && position < 90)
        {
            result.Add(3);
            result.Add(7);
        }
        else if (position >= 90 && position < 180)
        {
            result.Add(4);
            result.Add(8);
        }
        else if (position >= 180 && position < 270)
        {
            result.Add(1);
            result.Add(5);
        }
        else if (position >= 270 && position < 360)
        {
            result.Add(2);
            result.Add(6);
        }

        return result.ToArray();
    }

    bool isRotate = false;

    void LateUpdate()
    {
        if (!isARMode)
        {
            if (!isMoveEnable)
            {
                return;
            }

            if (Moving)
            {
                UpdatePositionAndRotation();
            }
            else if (Input.GetMouseButton(0))
            {
                if (isUpDownEnable)
                {
                    float angle = transform.rotation.eulerAngles.y;
                    transform.Rotate(0f, Input.GetAxis("Mouse X") * RotateSpeed, 0f, Space.World);
                    transform.Rotate((-Input.GetAxis("Mouse Y")) * RotateSpeed, 0f, 0f, Space.Self);
                    angle = Mathf.Abs(transform.rotation.eulerAngles.y - angle);

                    if (angle > 0.1)
                    {
                        isRotate = true;
                    }
                }

            }
            var angles = transform.eulerAngles;
            var symmetricX = Mathf.Asin(Mathf.Sin(Mathf.Deg2Rad * angles.x)) * Mathf.Rad2Deg;
            angles.x = Mathf.Clamp(symmetricX, -50f, 60f - Camera.fieldOfView / 2f);
            angles.z = 0;

            if (isUpDownEnable)
            {
                transform.rotation = Quaternion.Euler(angles);
            }


            if (Input.GetMouseButtonUp(0) && isRotate)
            {
                isRotate = false;
                if (image8kCoroutine != null)
                {
                    StopCoroutine(image8kCoroutine);
                }

                Vector3 currentRotation = transform.rotation.eulerAngles;
                Vector3 currentPovRotation = povController.transform.rotation.eulerAngles;

                Vector3 povRotation = currentRotation - currentPovRotation;
                float povYRotation = povRotation.y;
                if (povYRotation < 0)
                {
                    povYRotation = povYRotation + 360;
                }

                float left_pov = povYRotation + HorizontalFieldOfView / 2;
                float right_pov = povYRotation - HorizontalFieldOfView / 2;


                int[] bounds = CheckBounds(left_pov, right_pov, povYRotation);

                image8kCoroutine = StartCoroutine(LoadTextures8k_bound_Background(false, povController.gameObject, bounds, process: (bound, texture, returnPovController) =>
                {
                    if (returnPovController == povController)
                    {
                        StartCoroutine(IbrManager.UpdateHighResolutionMaterial_Dictionary(bound, texture));
                    }
                }, complete: () =>
                {
                    image8kCoroutine = null;
                }));
            }
        }
    }

    public async void LoadRealTimePov(Vector3 position, Vector3 rotation)
    {
        var nextPov = GetNextPovFromPosition(position);
        PovController nextPovController = nextPov.GetComponent<PovController>();
        this.povController = nextPovController;

        await IbrManager.StartFromRealTime(nextPovController);
        transform.SetPositionAndRotation(nextPovController.transform.position, Quaternion.Euler(rotation));
        IbrManager.SetPov(nextPovController, IbrManager.PovType.Primary);

        povController.PlaceIndicator();
        this.povController = nextPovController;

        if (image8kCoroutine != null)
        {
            StopCoroutine(image8kCoroutine);
        }

        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 currentPovRotation = povController.transform.rotation.eulerAngles;
        Vector3 povRotation = currentRotation - currentPovRotation;
        float povYRotation = povRotation.y;
        if (povYRotation < 0)
        {
            povYRotation = povYRotation + 360;
        }

        float left_pov = povYRotation + HorizontalFieldOfView / 2;
        float right_pov = povYRotation - HorizontalFieldOfView / 2;


        int[] bounds = CheckBounds(left_pov, right_pov, povYRotation);

        IbrManager.ClearAllSplitMaterial();
        image8kCoroutine = StartCoroutine(LoadTextures8k_bound_Background(true, povController.gameObject, bounds, process: (bound, texture, returnPovController) =>
        {
            if (returnPovController == povController)
            {
                StartCoroutine(IbrManager.UpdateHighResolutionMaterial_Dictionary(bound, texture));
            }
        }, complete: () => {
            image8kCoroutine = null;
        }));
    }

    public async void LoadRealTimePov(PovController nextPovController, Vector3 rotation)
    {
        // update camera position
        this.povController = nextPovController;
        await IbrManager.StartFromRealTime(nextPovController);
        transform.SetPositionAndRotation(nextPovController.transform.position, Quaternion.Euler(rotation));
        IbrManager.SetPov(nextPovController, IbrManager.PovType.Primary);

        nextPovController.PlaceIndicator();

        if (image8kCoroutine != null)
        {
            StopCoroutine(image8kCoroutine);
        }

        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 currentPovRotation = povController.transform.rotation.eulerAngles;
        Vector3 povRotation = currentRotation - currentPovRotation;
        float povYRotation = povRotation.y;
        if (povYRotation < 0)
        {
            povYRotation = povYRotation + 360;
        }

        float left_pov = povYRotation + HorizontalFieldOfView / 2;
        float right_pov = povYRotation - HorizontalFieldOfView / 2;

        int[] bounds = CheckBounds(left_pov, right_pov, povYRotation);

        IbrManager.ClearAllSplitMaterial();
        image8kCoroutine = StartCoroutine(LoadTextures8k_bound_Background(true, povController.gameObject, bounds, process: (bound, texture, returnPovController) =>
        {
            if (returnPovController == povController)
            {
                StartCoroutine(IbrManager.UpdateHighResolutionMaterial_Dictionary(bound, texture));
            }
        }, complete: () => {
            image8kCoroutine = null;
        }));

    }

    #endregion

    #region Private Methods

    private async Task InitializeNavigation()
    {
        // find the nearest pov
        var nearestObject = GetNextPovFromPosition(transform.position);

        // move the camera to the nearest pov
        transform.position = nearestObject.transform.position;

        // set the primary pov to ibr manager
        IbrManager.SetPov(nearestObject.GetComponent<IPov>(), IbrManager.PovType.Primary);

        //// find the current spot
        //var currSpotCtrl = FindSpotController(IbrManager.PrimarySpot);

        //currSpotCtrl[0].PlaceIndicators();

        PovController nextPovController = nearestObject.GetComponent<PovController>();
        if (nextPovController == povController)
        {
            return;
        }
        povController = nextPovController;

        await IbrManager.StartFromRealTime(povController);

        if (image8kCoroutine != null)
        {
            StopCoroutine(image8kCoroutine);
        }

        await HandleNavigation(povController.transform, Vector3.zero);

        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 currentPovRotation = povController.transform.rotation.eulerAngles;
        Vector3 povRotation = currentRotation - currentPovRotation;
        float povYRotation = povRotation.y;
        if (povYRotation < 0)
        {
            povYRotation = povYRotation + 360;
        }

        float left_pov = povYRotation + HorizontalFieldOfView / 2;
        float right_pov = povYRotation - HorizontalFieldOfView / 2;


        int[] bounds = CheckBounds(left_pov, right_pov, povYRotation);

        IbrManager.ClearAllSplitMaterial();
        image8kCoroutine = StartCoroutine(LoadTextures8k_bound_Background(true, povController.gameObject, bounds, process: (bound, texture, returnPovController) =>
        {
            if (returnPovController == povController)
            {
                StartCoroutine(IbrManager.UpdateHighResolutionMaterial_Dictionary(bound, texture));
            }
        }, complete: () =>
        {
            image8kCoroutine = null;
        }));
    }

    private async Task MoveRealTimeNavigation(PovController pov, Vector3 rotation)
    {
        transform.position = pov.transform.position;
        transform.eulerAngles = rotation;

        // set the primary pov to ibr manager
        IbrManager.SetPov(pov, IbrManager.PovType.Primary);

        // find the current spot
        //var currSpotCtrl = FindSpotController(pov.name);

        //currSpotCtrl[0].PlaceIndicators();

        povController = pov;

        await IbrManager.StartFromRealTime(povController);

        if (image8kCoroutine != null)
        {
            StopCoroutine(image8kCoroutine);
        }


        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 currentPovRotation = povController.transform.rotation.eulerAngles;
        Vector3 povRotation = currentRotation - currentPovRotation;
        float povYRotation = povRotation.y;
        if (povYRotation < 0)
        {
            povYRotation = povYRotation + 360;
        }

        float left_pov = povYRotation + HorizontalFieldOfView / 2;
        float right_pov = povYRotation - HorizontalFieldOfView / 2;


        int[] bounds = CheckBounds(left_pov, right_pov, povYRotation);

        IbrManager.ClearAllSplitMaterial();
        image8kCoroutine = StartCoroutine(LoadTextures8k_bound_Background(true, povController.gameObject, bounds, process: (bound, texture, returnPovController) =>
        {
            if (returnPovController == povController)
            {
                StartCoroutine(IbrManager.UpdateHighResolutionMaterial_Dictionary(bound, texture));
            }
        }, complete: () => {
            image8kCoroutine = null;
        }));
    }


    private async Task HandleKeyboardNavigation()
    {
        var dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (dir.sqrMagnitude <= 0) return;
        dir = transform.right * dir.x + transform.forward * dir.z;
        dir = dir.normalized;

        int layerMask = 1 << LayerMask.NameToLayer("Raycast");
        float hitDistance = 7;
        startPoint = transform.position;


        Transform nextPovTransform = GetNextPovFromDirectionDistance(transform.position, dir, 5);

        if (nextPovTransform == null)
        {
            return;
        }

        PovController nextPovController = nextPovTransform.GetComponent<PovController>();

        if (nextPovController == povController)
        {
            return;
        }

        startPoint = transform.position;

        if (Physics.Linecast(startPoint, nextPovController.transform.position, out RaycastHit hit2, layerMask))
        {
            if (hit2.transform.tag == "Position")
            {
                return;
            }
            hitDistance = hit2.distance;
            float povDistance = Vector3.Distance(nextPovController.transform.position, transform.position);
            if (hitDistance < povDistance)
            {
                return;
            }
        }

        povController = nextPovController;

        if (image8kCoroutine != null)
        {
            StopCoroutine(image8kCoroutine);
        }

        await HandleNavigation(povController.transform, Vector3.zero);

        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 currentPovRotation = povController.transform.rotation.eulerAngles;
        Vector3 povRotation = currentRotation - currentPovRotation;
        float povYRotation = povRotation.y;
        if (povYRotation < 0)
        {
            povYRotation = povYRotation + 360;
        }

        float left_pov = povYRotation + HorizontalFieldOfView / 2;
        float right_pov = povYRotation - HorizontalFieldOfView / 2;


        int[] bounds = CheckBounds(left_pov, right_pov, povYRotation);

        IbrManager.ClearAllSplitMaterial();
        image8kCoroutine = StartCoroutine(LoadTextures8k_bound_Background(true, povController.gameObject, bounds, process: (bound, texture, returnPovController) =>
        {
            if (returnPovController == povController)
            {
                StartCoroutine(IbrManager.UpdateHighResolutionMaterial_Dictionary(bound, texture));
            }
        }, complete: () => { }));
    }

    private async Task HandleMouseNavigation()
    {
        bool doubleClick = false;

        if (m_IsOneClick && ((Time.time - m_Timer) > m_DoubleClickSecond))
        {
            m_IsOneClick = false;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (!m_IsOneClick)
            {
                m_Timer = Time.time;
                m_IsOneClick = true;
            }
            else if (m_IsOneClick && ((Time.time - m_Timer) < m_DoubleClickSecond))
            {
                m_IsOneClick = false;
                doubleClick = true;
            }
        }
        if (null == Cursor) return;
        if (!Cursor.gameObject.activeSelf) return;
        if (!Input.GetMouseButtonUp(1) && !doubleClick) return;

        isRotate = false;
        int layerMask = 1 << LayerMask.NameToLayer("Raycast");
        float hitDistance = 30;
        startPoint = transform.position;

        var nextPov = GetNextPovFromPosition(Cursor.transform.position);
        //var nextPovs = GetNextPovAroundFromPosition(Cursor.transform.position, 5);
        PovController nextPovController = nextPov.GetComponent<PovController>();

        if (Physics.Linecast(startPoint, nextPovController.transform.position, out RaycastHit hit2, layerMask))
        {
            hitDistance = hit2.distance;
            float povDistance = Vector3.Distance(nextPovController.transform.position, transform.position);
            if (hitDistance < povDistance)
            {
                return;
            }
        }

        if (nextPovController == povController)
        {
            return;
        }
        povController = nextPovController;

        await HandleNavigation(nextPov, Vector3.zero);

        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 currentPovRotation = povController.transform.rotation.eulerAngles;
        Vector3 povRotation = currentRotation - currentPovRotation;
        float povYRotation = povRotation.y;
        if (povYRotation < 0)
        {
            povYRotation = povYRotation + 360;
        }

        float left_pov = povYRotation + HorizontalFieldOfView / 2;
        float right_pov = povYRotation - HorizontalFieldOfView / 2;


        int[] bounds = CheckBounds(left_pov, right_pov, povYRotation);

        if (image8kCoroutine != null)
        {
            StopCoroutine(image8kCoroutine);
        }

        IbrManager.ClearAllSplitMaterial();
        image8kCoroutine = StartCoroutine(LoadTextures8k_bound_Background(true, povController.gameObject, bounds, process: (bound, texture, returnPovController) =>
        {
            if (returnPovController == povController)
            {
                StartCoroutine(IbrManager.UpdateHighResolutionMaterial_Dictionary(bound, texture));
            }
        }, complete: () => {
            image8kCoroutine = null;
        }));
    }

    private async Task HandleNavigation(Transform nextPov)
    {
        TextureManager.SetFirst(false);
        IbrManager.SetPov(nextPov.GetComponent<IPov>(), IbrManager.PovType.Secondary);
        await IbrManager.UntilReady(IbrManager.PovType.Secondary);
        StartAnimating(nextPov.position);
    }

    private async Task HandleNavigation(Transform nextPov, Vector3 rotation)
    {
        TextureManager.SetFirst(false);
        IbrManager.SetPov(nextPov.GetComponent<IPov>(), IbrManager.PovType.Secondary);

        await IbrManager.UntilReady(IbrManager.PovType.Secondary);

        if (rotation != Vector3.zero)
        {
            StartAnimating(nextPov.position, Quaternion.Euler(rotation));
        }
        else
        {
            StartAnimating(nextPov.position);
        }
    }

    private Transform GetNextPovFromPosition(Vector3 position)
    {
        var nearestObject = KnnManager.FindNearest(position);
        return nearestObject.transform;
    }

    private GameObject[] GetNextPovAroundFromPosition(Vector3 position, float distance)
    {
        var nearestObject = KnnManager.FindWithinRange(position, distance);
        return nearestObject;
    }

    private Transform GetNextPovFromDirectionDistance(Vector3 position, Vector3 direction, float distnace)
    {

        GameObject[] povs = GetNextPovAroundFromPosition(position, distnace);

        if (povs == null)
        {
            return null;
        }

        List<Transform> filteredProximalPovs = new List<Transform>();
        foreach (GameObject eachGameObject in povs)
        {
            if (eachGameObject.activeSelf)
            {
                filteredProximalPovs.Add(eachGameObject.transform);
            }
        }

        var nearestCandidate = SelectOrderedFilteredByAngleDistance(povController.transform, filteredProximalPovs.ToArray(), direction, 40);

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

    private void UpdateCursor()
    {
        if (Physics.Raycast(Camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
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

    /// <summary>
    /// Keep the position, and rotate to new orientation
    /// </summary>
    /// <param name="toRotation">the target rotation</param>
    private void StartAnimating(Quaternion toRotation)
    {
        StartAnimating(transform.position, toRotation);
    }

    /// <summary>
    /// Keep the orientation, and translate to new position
    /// </summary>
    /// <param name="toPosition">the target position</param>
    private void StartAnimating(Vector3 toPosition)
    {
        StartAnimating(toPosition, transform.rotation);
    }

    /// <summary>
    /// Translate and rotate to new position and orientation
    /// </summary>
    /// <param name="toPosition">the target position</param>
    /// <param name="toRotation">the target rotation</param>
    private void StartAnimating(Vector3 toPosition, Quaternion toRotation)
    {
        StartAnimating(transform.position, toPosition, transform.rotation, toRotation);
    }

    /// <summary>
    /// Translate and rotate from a pose to another
    /// </summary>
    /// <param name="fromPosition">the initial position</param>
    /// <param name="toPosition">the target position</param>
    /// <param name="fromRotation">the initial rotation</param>
    /// <param name="toRotation">the target rotation</param>
    private void StartAnimating(Vector3 fromPosition, Vector3 toPosition, Quaternion fromRotation, Quaternion toRotation)
    {
        SetPositionInfo(fromPosition, toPosition);
        SetRotationInfo(fromRotation, toRotation);
        CurrentState = State.Moving;
    }

    /// <summary>
    /// Set position-related info for animation
    /// </summary>
    /// <param name="fromPosition">the initial position</param>
    /// <param name="toPosition">the target position</param>
    private void SetPositionInfo(Vector3 fromPosition, Vector3 toPosition)
    {
        _keepPosition = fromPosition == toPosition;
        _animateFromPosition = fromPosition;
        _animateToPosition = toPosition;
    }

    /// <summary>
    /// Set rotation-related info for animation
    /// </summary>
    /// <param name="fromRotation">the initial rotation</param>
    /// <param name="toRotation">the target rotation</param>
    private void SetRotationInfo(Quaternion fromRotation, Quaternion toRotation)
    {
        _keepRotation = fromRotation == toRotation;
        _animateFromRotation = fromRotation;
        _animateToRotation = toRotation;
    }

    /// <summary>
    /// Let the animation progress
    /// </summary>
    private void UpdateAnimationParameter()
    {
        _t = Mathf.Clamp01(_t + Time.smoothDeltaTime * Speed);
        OnAnimationUpdated.Invoke(_t);
    }

    /// <summary>
    /// Apply new position and rotation based on the animation-related info
    /// </summary>
    private void UpdatePositionAndRotation()
    {
        var nextPosition = Vector3.Lerp(_animateFromPosition, _animateToPosition, _keepPosition ? 0 : _t);
        var nextRotation = Quaternion.Slerp(_animateFromRotation, _animateToRotation, _keepRotation ? 0 : _t);
        transform.SetPositionAndRotation(nextPosition, nextRotation);
    }

    /// <summary>
    /// Finalize any on-going animation
    /// </summary>
    private void FinishAnimating()
    {
        _t = 1f;
        CurrentState = State.Idle;
        SetPositionInfo(Vector3.zero, Vector3.zero);
        SetRotationInfo(Quaternion.identity, Quaternion.identity);
    }

    #endregion

    #region Private Static Methods

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

    #endregion

    public async Task MoveToPov(PovController nexPov, Vector3 rotation)
    {
        PovController nextPovController = nexPov;

        if (nextPovController == povController)
        {
            return;
        }
        povController = nextPovController;

        if (image8kCoroutine != null)
        {
            StopCoroutine(image8kCoroutine);
        }


        await HandleNavigation(nextPovController.transform, rotation);

        Vector3 currentRotation = transform.rotation.eulerAngles;

        Vector3 currentPovRotation = povController.transform.rotation.eulerAngles;
        if (rotation != Vector3.zero)
        {
            currentPovRotation = rotation;
        }
        Vector3 povRotation = currentRotation - currentPovRotation;
        float povYRotation = povRotation.y;
        if (povYRotation < 0)
        {
            povYRotation = povYRotation + 360;
        }

        float left_pov = povYRotation + HorizontalFieldOfView / 2;
        float right_pov = povYRotation - HorizontalFieldOfView / 2;

        //transform.rotation = Quaternion.Euler(currentPovRotation.x, currentPovRotation.y, currentPovRotation.z);


        int[] bounds = CheckBounds(left_pov, right_pov, povYRotation);

        IbrManager.ClearAllSplitMaterial();
        image8kCoroutine = StartCoroutine(LoadTextures8k_bound_Background(true, povController.gameObject, bounds, process: (bound, texture, returnPovController) =>
        {
            if (returnPovController == povController)
            {
                StartCoroutine(IbrManager.UpdateHighResolutionMaterial_Dictionary(bound, texture));
            }
        }, complete: () => {
            image8kCoroutine = null;
        }));
    }

    private IEnumerator LoadTextures4k_Background(GameObject povs, System.Action<Texture2D, PovController> complete)
    {
        PovController povController = povs.GetComponent<PovController>();

        Texture2D result_texture = null;

        //yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(TextureManager.LoadTextureBackgroud(povController.Spot, povController.Name, "image4k", complete: (result) =>
        {
            result_texture = result;
        }));


        complete(result_texture, povController);
    }

    private IEnumerator LoadTextures8k_Background(GameObject povs, System.Action<Texture2D, PovController> complete)
    {
        PovController povController = povs.GetComponent<PovController>();
        Debug.Log(povController.Name);

        Texture2D result_texture = null;

        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(TextureManager.LoadTextureBackgroud(povController.Spot, povController.Name, "image8k", complete: (result) =>
        {
            result_texture = result;
        }));


        complete(result_texture, povController);
    }

    private IEnumerator LoadTextures8k_bound_Background(bool clearTexture, GameObject povs, int[] bounds, System.Action<int, Texture2D, PovController> process, System.Action complete)
    {
        PovController povController = povs.GetComponent<PovController>();


        if (clearTexture)
        {
            TextureManager.ClearBoundTexture();
        }

        yield return StartCoroutine(TextureManager.LoadTextureBackgroud_bounds(povController.Spot, povController.Name, "image8k_split", bounds, process: (bound, result) => {

            process(bound, result, povController);
        }, complete: () =>
        {
            complete();
        }));

    }

    private IEnumerator LoadTextures8k_rotation_bound_Background(bool clearTexture, GameObject povs, int[] bounds, System.Action<int, Texture2D, PovController> process, System.Action complete)
    {
        PovController povController = povs.GetComponent<PovController>();

        yield return StartCoroutine(TextureManager.LoadTextureBackgroud_rotation_bounds(povController.Spot, povController.Name, "image8k_split", bounds, process: (bound, result) => {

            process(bound, result, povController);
        }, complete: () =>
        {
            complete();
        }));

    }

    private IEnumerator LoadTextures8k_split_Background(GameObject povs, System.Action<Texture2D[], PovController> complete)
    {
        PovController povController = povs.GetComponent<PovController>();

        Texture2D[] result_texture = null;

        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TextureManager.LoadTextureBackgroud_array(povController.Spot, povController.Name, "image8k_split", complete: (result) =>
        {
            result_texture = result;
        }));


        complete(result_texture, povController);
    }
}
