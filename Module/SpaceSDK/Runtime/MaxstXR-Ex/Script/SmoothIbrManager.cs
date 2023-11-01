using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
using System.Collections;
using System;
using static IbrManager;

namespace MaxstXR.Extension
{
    public partial class SmoothIbrManager : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private UnityEvent _onEnable;
        [SerializeField] private Material IbrCullBack, IbrCullFront;

        [SerializeField] private Color meshColor;

        [SerializeField] private Vector4 _frameData = Vector4.zero;

        [SerializeField] private Matrix4x4[] _frameViewMatrices = new Matrix4x4[2];
        [SerializeField] private SmoothSharedTexture[] _frameTextures = new SmoothSharedTexture[2];
        [SerializeField] private string[] _frameSpots = new string[2];
        [SerializeField] private string[] _frameNames = new string[2];
        [SerializeField] private SmoothSharedTexture _frameTextureInEditor = null;

        private SmoothSharedTexture[] _frontBoundTextures = new SmoothSharedTexture[9];
        private SmoothSharedTexture[] _backBoundTextures = new SmoothSharedTexture[9];

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

        public void StartFrom(IPov pov, SmoothSharedTexture st)
        {
            _frameData.x = 0.0f;
            _frameSpots[0] = pov.Spot;
            _frameNames[0] = pov.Name;
            _frameViewMatrices[0] = pov.WorldToLocalMatrix;
            SmoothSharedTexture.Assign(ref _frameTextureInEditor, st);
            IbrCullBack.SetVector("_runtimeData", new Vector4(1, 0, 0, 0));

            UpdateMaterialEditor(IbrCullBack);
            UpdateMaterialEditor(IbrCullFront);
        }

        public void SetIbrMaterials(Material back, Material front)
        {
            IbrCullBack = back;
            IbrCullFront = front;
        }

        private void UpdateMaterialEditor(Material m)
        {
            SetFrameData(m, _frameData);
            SetFrameViewMatrices(m, _frameViewMatrices);
            if (null != _frameTextureInEditor?.Texture2d)
            {
                SetFrameTextureCurrent(m, _frameTextureInEditor.Texture2d);
            }
        }

        public void SetPov(IPov pov, PovType povType)
        {
            var index = (uint)povType;
            _frameSpots[index] = pov.Spot;
            _frameNames[index] = pov.Name;
            _frameViewMatrices[index] = pov.WorldToLocalMatrix;
            //Debug.Log($"IbrManager SetPov : {index}/{pov.Spot}/{pov.Name}");
        }

        public void StartFromRealTime(IPov pov, SmoothSharedTexture st)
        {
            _frameData.x = 1.0f;
            _frameSpots[0] = pov.Spot;
            _frameNames[0] = pov.Name;
            _frameSpots[1] = pov.Spot;
            _frameNames[1] = pov.Name;
            //Debug.Log($"IbrManager StartFromRealTime : {pov.Spot}/{pov.Name}");
            _frameViewMatrices[0] = pov.WorldToLocalMatrix;
            _frameViewMatrices[1] = pov.WorldToLocalMatrix;
            SmoothSharedTexture.Assign(ref _frameTextures[0], st);
            SmoothSharedTexture.Assign(ref _frameTextures[1], st);
            IbrCullBack.SetVector("_runtimeData", new Vector4(1, 0, 0, 0));
            UpdateMaterials();
        }

        public void UpdateFrameTexture(SmoothSharedTexture st, PovType povType)
        {
            SmoothSharedTexture.Assign(ref _frameTextures[(uint)povType], st);
            UpdateMaterials();
        }

        #region Unity Messages

        protected virtual void Awake()
        {
            //IbrCullFront = Resources.Load("IbrCullFront") as Material;
            //IbrCullBack = Resources.Load("IbrCullBack") as Material;

            if (IbrCullBack)
            {
                IbrCullBack.SetColor("_Color", new Color(0, 0, 0, 0));
                IbrCullBack.SetVector("_runtimeData", new Vector4(0, 0, 0, 0));
            }
        }

        protected virtual void OnEnable()
        {
            _frameData.x = 0.0f;
            _onEnable?.Invoke();
            UpdateMaterials();
        }

        private void OnDestroy()
        {
            ClearAllSplitMaterial(true);
            ClearAllTextureMaterial();

            ExtinctionTextureMaterial(IbrCullFront);
            ExtinctionTextureMaterial(IbrCullBack);
        }

        #endregion

        public void HandleAnimationStarted()
        {
            _frameData.x = 0.0f;
            UpdateMaterials();
        }

        public void HandleAnimationUpdated(float timeAtPos, float timeAtRoate)
        {
            _frameData.x = Mathf.Clamp01(timeAtPos);

            SetFrameData(IbrCullBack, _frameData);
            SetFrameData(IbrCullFront, _frameData);
        }

        public void HandleAnimationFinished()
        {
            _frameData.x = 1.0f;

            _frameSpots[0] = _frameSpots[1];
            _frameNames[0] = _frameNames[1];
            _frameViewMatrices[0] = _frameViewMatrices[1];
            SmoothSharedTexture.Assign(ref _frameTextures[0], _frameTextures[1]);

            UpdateMaterials();
        }

        private void UpdateMaterial(Material m)
        {
            SetFrameData(m, _frameData);
            SetFrameViewMatrices(m, _frameViewMatrices);
            if (null != _frameTextures[0]?.Texture2d)
                SetFrameTextureCurrent(m, _frameTextures[0].Texture2d);
            if (null != _frameTextures[1]?.Texture2d)
                SetFrameTextureNext(m, _frameTextures[1].Texture2d);
        }

        public void ClearAllSplitMaterial(bool resetCount = true)
        {
            ClearSplitMaterial(IbrCullBack, resetCount);
            ClearSplitMaterial(IbrCullFront, resetCount);
        }

        public void ClearAllTextureMaterial()
        {
            ClearTextureMaterial(IbrCullBack);
            ClearTextureMaterial(IbrCullFront);
        }

        public void ExtinctionTextureMaterial(Material m)
        {
            m.SetTexture(kCurrTexId, null);
            m.SetTexture(kNextTexId, null);
        }

        private void ClearTextureMaterial(Material m)
        {
            for (int i = 1; i < _frontBoundTextures.Length; ++i)
            {
                _frontBoundTextures[i]?.Release();
                _frontBoundTextures[i] = null;
            }

            for (int i = 1; i < _backBoundTextures.Length; ++i)
            {
                _backBoundTextures[i]?.Release();
                _backBoundTextures[i] = null;
            }

            m.SetTexture(kNextTexId_1, null);
            m.SetTexture(kNextTexId_2, null);
            m.SetTexture(kNextTexId_3, null);
            m.SetTexture(kNextTexId_4, null);
            m.SetTexture(kNextTexId_5, null);
            m.SetTexture(kNextTexId_6, null);
            m.SetTexture(kNextTexId_7, null);
            m.SetTexture(kNextTexId_8, null);
            m.SetInt(kValidTexture1, 0);
            m.SetInt(kValidTexture2, 0);
            m.SetInt(kValidTexture3, 0);
            m.SetInt(kValidTexture4, 0);
            m.SetInt(kValidTexture5, 0);
            m.SetInt(kValidTexture6, 0);
            m.SetInt(kValidTexture7, 0);
            m.SetInt(kValidTexture8, 0);
        }

        private void ClearSplitMaterial(Material m, bool resetCount = true)
        {
            if (resetCount)
            {
                m.SetInt(kTextureCount, 0);
                m.SetTexture(kNextTexId, null);
                m.SetTexture(kCurrTexId, null);
            }
            m.SetInt(kValidTexture1, 0);
            m.SetInt(kValidTexture2, 0);
            m.SetInt(kValidTexture3, 0);
            m.SetInt(kValidTexture4, 0);
            m.SetInt(kValidTexture5, 0);
            m.SetInt(kValidTexture6, 0);
            m.SetInt(kValidTexture7, 0);
            m.SetInt(kValidTexture8, 0);
        }

        private void UpdateCurrentMaterial_split(Material m, SmoothSharedTexture[] textures, SmoothSharedTexture st)
        {
            if (!st.Index.HasValue) return;

            SetFrameData(m, _frameData);
            SetFrameViewMatrices(m, _frameViewMatrices);
            var textureNumber = st.Index.Value;
            SmoothSharedTexture.Assign(ref textures[textureNumber], st);
            if (textureNumber == 1)
            {
                m.SetTexture(kNextTexId_1, st.Texture2d);
                m.SetInt(kValidTexture1, 1);
            }
            else if (textureNumber == 2)
            {
                m.SetTexture(kNextTexId_2, st.Texture2d);
                m.SetInt(kValidTexture2, 1);
            }
            else if (textureNumber == 3)
            {
                m.SetTexture(kNextTexId_3, st.Texture2d);
                m.SetInt(kValidTexture3, 1);
            }
            else if (textureNumber == 4)
            {
                m.SetTexture(kNextTexId_4, st.Texture2d);
                m.SetInt(kValidTexture4, 1);
            }
            else if (textureNumber == 5)
            {
                m.SetTexture(kNextTexId_5, st.Texture2d);
                m.SetInt(kValidTexture5, 1);
            }
            else if (textureNumber == 6)
            {
                m.SetTexture(kNextTexId_6, st.Texture2d);
                m.SetInt(kValidTexture6, 1);
            }
            else if (textureNumber == 7)
            {
                m.SetTexture(kNextTexId_7, st.Texture2d);
                m.SetInt(kValidTexture7, 1);
            }
            else if (textureNumber == 8)
            {
                m.SetTexture(kNextTexId_8, st.Texture2d);
                m.SetInt(kValidTexture8, 1);
            }
        }

        private void UpdateMaterials()
        {
            if (IbrCullBack) UpdateMaterial(IbrCullBack);
            if (IbrCullFront) UpdateMaterial(IbrCullFront);
        }

        private void UpdateCurrentMaterials_split(SmoothSharedTexture st)
        {
            UpdateCurrentMaterial_split(IbrCullBack, _backBoundTextures, st);
            UpdateCurrentMaterial_split(IbrCullFront, _frontBoundTextures, st);
        }

        public void UpdateHighResolutionMaterial(SmoothSharedTexture st)
        {
            if (st.Texture2d != null)
            {
                UpdateCurrentMaterials_split(st);
            }
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

}
