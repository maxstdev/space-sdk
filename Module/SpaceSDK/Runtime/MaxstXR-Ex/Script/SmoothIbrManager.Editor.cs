#if UNITY_EDITOR

using UnityEngine;

namespace MaxstXR.Extension
{
	[ExecuteInEditMode]
	public partial class SmoothIbrManager : MonoBehaviour
	{
		public void Update()
		{
			if (Application.isEditor && !Application.isPlaying)
			{
				IbrCullBack.SetVector("_runtimeData", new Vector4(1, 0, 0, 0));
				IbrCullBack.SetColor("_Color", meshColor);
				UpdateMaterialEditor(IbrCullBack);
				UpdateMaterialEditor(IbrCullFront);
			}
		}
	}
}
#endif