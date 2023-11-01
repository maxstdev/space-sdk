using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
using System.Collections;

public partial class IbrManager : MonoBehaviour
{
    public enum PovType : uint
    {
        Primary = 0u, Secondary = 1u
    }

    #region Public Properties
    private TextureManager _textureManager;
    private TextureManager TextureManager {
        get
        {
            if (null == _textureManager)
                _textureManager = FindObjectOfType<TextureManager>(true);
            return _textureManager;
        }
    }

    #endregion

    #region Serialized Fields

    [SerializeField]
    private UnityEvent _onEnable;
    [SerializeField]
    private Material IbrCullBack, IbrCullFront;

    [SerializeField]
    private Color meshColor;

    [SerializeField]
    private Vector4 _frameData = Vector4.zero;
    
    [SerializeField]
    private Matrix4x4[] _frameViewMatrices = new Matrix4x4[2];
    
    [SerializeField]
    private Texture2D[] _frameTextures = new Texture2D[2];
    
    [SerializeField]
    private string[] _frameSpots = new string[2];
    
    [SerializeField]
    private string[] _frameNames = new string[2];

    [SerializeField]
    private Texture2D _frameTextureInEditor = null;

    #endregion

    public static int kCurrTexId = Shader.PropertyToID("_MainTex");
    public static int kNextTexId = Shader.PropertyToID("_MainTex2");
    public static int kNextTexId_1 = Shader.PropertyToID("_MainTex2_1");
    public static int kNextTexId_2 = Shader.PropertyToID("_MainTex2_2");
    public static int kNextTexId_3 = Shader.PropertyToID("_MainTex2_3");
    public static int kNextTexId_4 = Shader.PropertyToID("_MainTex2_4");
    public static int kNextTexId_5 = Shader.PropertyToID("_MainTex2_5");
    public static int kNextTexId_6 = Shader.PropertyToID("_MainTex2_6");
    public static int kNextTexId_7 = Shader.PropertyToID("_MainTex2_7");
    public static int kNextTexId_8 = Shader.PropertyToID("_MainTex2_8");
    public static int kTextureCount = Shader.PropertyToID("_textureCount");
    public static int kValidTexture1 = Shader.PropertyToID("_validTexture1");
    public static int kValidTexture2 = Shader.PropertyToID("_validTexture2");
    public static int kValidTexture3 = Shader.PropertyToID("_validTexture3");
    public static int kValidTexture4 = Shader.PropertyToID("_validTexture4");
    public static int kValidTexture5 = Shader.PropertyToID("_validTexture5");
    public static int kValidTexture6 = Shader.PropertyToID("_validTexture6");
    public static int kValidTexture7 = Shader.PropertyToID("_validTexture7");
    public static int kValidTexture8 = Shader.PropertyToID("_validTexture8");
    public static int kFrameDataId = Shader.PropertyToID("frame_Data");
    public static int kFrameViewMatricesId = Shader.PropertyToID("frame_MatrixV");

    public async Task StartFrom(IPov pov)
    {
        _frameData.x = 0.0f;
        _frameSpots[0] = pov.Spot;
        _frameNames[0] = pov.Name;
        _frameViewMatrices[0] = pov.WorldToLocalMatrix;
        _frameTextureInEditor = await TextureManager.LoadTexture(pov.Spot, pov.Name);
        IbrCullBack.SetVector("_runtimeData", new Vector4(1, 0, 0, 0));

        UpdateMaterialEditor(IbrCullBack);
        UpdateMaterialEditor(IbrCullFront);
    }

    private void UpdateMaterialEditor(Material m)
    {
        SetFrameData(m, _frameData);
        SetFrameViewMatrices(m, _frameViewMatrices);
        if (null != _frameTextureInEditor)
            SetFrameTextureCurrent(m, _frameTextureInEditor);
    }


    public void SetPov(IPov pov, PovType povType) {

        var index = (uint) povType;
        _frameSpots[index] = pov.Spot;
        _frameNames[index] = pov.Name;
        _frameViewMatrices[index] = pov.WorldToLocalMatrix;
    }

    public async Task StartFromRealTime(IPov pov)
    {
        _frameData.x = 1.0f;
        _frameSpots[0] = pov.Spot;
        _frameNames[0] = pov.Name;
        _frameViewMatrices[0] = pov.WorldToLocalMatrix;
        _frameViewMatrices[1] = pov.WorldToLocalMatrix;
        TextureManager.ScheduleIfNotLoaded(_frameSpots[0], _frameNames[0]);
        _frameTextures[0] = await TextureManager.GetLoadedTexture(_frameSpots[0], _frameNames[0]);
        TextureManager.AllowToBeUnloaded(_frameSpots[0], _frameNames[0]);
        _frameTextures[1] = _frameTextures[0];
        IbrCullBack.SetVector("_runtimeData", new Vector4(1, 0, 0, 0));
        UpdateMaterials();
    }

    public async Task UntilReady(PovType povType)
    {
        var index = (uint)povType;

        TextureManager.ScheduleIfNotLoaded(_frameSpots[index], _frameNames[index]);
        _frameTextures[index] = await TextureManager.GetLoadedTexture(_frameSpots[index], _frameNames[index]);
    }

    #region Unity Messages

    protected virtual void Awake()
    {
        IbrCullBack.SetColor("_Color", new Color(0, 0, 0, 0));
        IbrCullBack.SetVector("_runtimeData", new Vector4(0, 0, 0, 0));
    }

    protected virtual async void OnEnable()
    {
        _frameData.x = 0.0f;

        await UntilReady(PovType.Primary);
        _onEnable.Invoke();
        UpdateMaterials();

    }

    #endregion

    public void HandleAnimationStarted()
    {
        _frameData.x = 0.0f;

        TextureManager.PreventToBeUnloaded(_frameSpots[0], _frameNames[0]);

        UpdateMaterials();
    }

    public void HandleAnimationUpdated(float t)
    {
        _frameData.x = Mathf.Clamp01(t);

        SetFrameData(IbrCullBack, _frameData);
        SetFrameData(IbrCullFront, _frameData);
    }

    public void HandleAnimationFinished()
    {
        _frameData.x = 1.0f;

        TextureManager.AllowToBeUnloaded(_frameSpots[0], _frameNames[0]);

        _frameSpots[0] = _frameSpots[1];
        _frameNames[0] = _frameNames[1];
        _frameViewMatrices[0] = _frameViewMatrices[1];
        _frameTextures[0] = _frameTextures[1];

        UpdateMaterials();
    }

    private void UpdateMaterial(Material m)
    {
        SetFrameData(m, _frameData);
        SetFrameViewMatrices(m, _frameViewMatrices);
        if (null != _frameTextures[0])
            SetFrameTextureCurrent(m, _frameTextures[0]);
        if (null != _frameTextures[1])
            SetFrameTextureNext(m, _frameTextures[1]);
    }

    public void ClearAllSplitMaterial()
    {
        ClearSplitMaterial(IbrCullBack);
        ClearSplitMaterial(IbrCullFront);
    }

    private void ClearSplitMaterial(Material m)
    {
        m.SetInt(kTextureCount, 0);
        m.SetInt(kValidTexture1, 0);
        m.SetInt(kValidTexture2, 0);
        m.SetInt(kValidTexture3, 0);
        m.SetInt(kValidTexture4, 0);
        m.SetInt(kValidTexture5, 0);
        m.SetInt(kValidTexture6, 0);
        m.SetInt(kValidTexture7, 0);
        m.SetInt(kValidTexture8, 0);
    }

    private void UpdateCurrentMaterial(Material m, Texture2D texture)
    {
        SetFrameData(m, _frameData);
        SetFrameViewMatrices(m, _frameViewMatrices);
        SetFrameTextureNext(m, texture);
        m.SetInt(kTextureCount, 0);
    }

    private void UpdateCurrentMaterial_array(Material m, Texture2D[] textures)
    {
        SetFrameData(m, _frameData);
        SetFrameViewMatrices(m, _frameViewMatrices);
        m.SetTexture(kNextTexId_1, textures[0]);
        m.SetTexture(kNextTexId_2, textures[1]);
        m.SetTexture(kNextTexId_3, textures[2]);
        m.SetTexture(kNextTexId_4, textures[3]);
        m.SetTexture(kNextTexId_5, textures[4]);
        m.SetTexture(kNextTexId_6, textures[5]);
        m.SetTexture(kNextTexId_7, textures[6]);
        m.SetTexture(kNextTexId_8, textures[7]);
        m.SetInt(kTextureCount, 1);

    }

    private void UpdateCurrentMaterial_split(Material m, Texture2D texture, int textureNumber)
    {
        SetFrameData(m, _frameData);
        SetFrameViewMatrices(m, _frameViewMatrices);
        if (textureNumber == 1)
        {
            m.SetTexture(kNextTexId_1, texture);
            m.SetInt(kValidTexture1, 1);
        }
        else if (textureNumber == 2)
        {
            m.SetTexture(kNextTexId_2, texture);
            m.SetInt(kValidTexture2, 1);
        }
        else if (textureNumber == 3)
        {
            m.SetTexture(kNextTexId_3, texture);
            m.SetInt(kValidTexture3, 1);
        }
        else if (textureNumber == 4)
        {
            m.SetTexture(kNextTexId_4, texture);
            m.SetInt(kValidTexture4, 1);
        }
        else if (textureNumber == 5)
        {
            m.SetTexture(kNextTexId_5, texture);
            m.SetInt(kValidTexture5, 1);
        }
        else if (textureNumber == 6)
        {
            m.SetTexture(kNextTexId_6, texture);
            m.SetInt(kValidTexture6, 1);
        }
        else if (textureNumber == 7)
        {
            m.SetTexture(kNextTexId_7, texture);
            m.SetInt(kValidTexture7, 1);
        }
        else if (textureNumber == 8)
        {
            m.SetTexture(kNextTexId_8, texture);
            m.SetInt(kValidTexture8, 1);
        }
    }

    private void UpdateMaterials()
    {
        UpdateMaterial(IbrCullBack);
        UpdateMaterial(IbrCullFront);
    }

    private void UpdateCurrentMaterials(Texture2D texture)
    {
        UpdateCurrentMaterial(IbrCullBack, texture);
        UpdateCurrentMaterial(IbrCullFront, texture);
    }

    private void UpdateCurrentMaterials_array(Texture2D[] textures)
    {
        UpdateCurrentMaterial_array(IbrCullBack, textures);
        UpdateCurrentMaterial_array(IbrCullFront, textures);
    }

    private void UpdateCurrentMaterials_split(int number, Texture2D texture)
    {
        UpdateCurrentMaterial_split(IbrCullBack, texture, number);
        UpdateCurrentMaterial_split(IbrCullFront, texture, number);
    }

    public IEnumerator UpdateHighResolutionMaterial(Texture2D texture)
    {
        UpdateCurrentMaterials(texture);
        yield return new WaitForEndOfFrame();
    }

    public IEnumerator UpdateHighResolutionMaterial_array(Texture2D[] textures)
    {
        UpdateCurrentMaterials_array(textures);
        yield return new WaitForEndOfFrame();
    }


    public IEnumerator UpdateHighResolutionMaterial_Dictionary(int number, Texture2D texture)
    {
        UpdateCurrentMaterials_split(number, texture);
        yield return new WaitForEndOfFrame();
    }

    public static void SetFrameData(Material material, Vector4 frameData)
    {
        material.SetVector(kFrameDataId, frameData);
    }

    public static void SetFrameViewMatrices(Material material, Matrix4x4[] frameViewMatrices)
    {
        material.SetMatrixArray(kFrameViewMatricesId, frameViewMatrices);
    }

    public static void SetFrameTextureCurrent(Material material, Texture current)
    {
        material.SetTexture(kCurrTexId, current);
    }

    public static void SetFrameTextureNext(Material material, Texture next)
    {
        material.SetTexture(kNextTexId, next);
    }
}
