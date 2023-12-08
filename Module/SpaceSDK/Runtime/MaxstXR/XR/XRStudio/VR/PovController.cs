using System.IO;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using MaxstXR.Extension;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class PovController : MonoBehaviour, IPov
{
    public string imageFileName = string.Empty;
    public Transform Indicator;
    public float Scale = 0.25f;
    public Texture norTexture = null;
    public Texture selTexture = null;

    private Color defaultColor = new Color(1, 1, 1, 0.2f);
    private Color selectedColor = new Color(0, 1, 1, 0.7f);

    #region Impl. of IPov

    public string Name => string.IsNullOrEmpty(imageFileName) ? gameObject.name : imageFileName;

    public string Spot => transform.parent.name;

    public Matrix4x4 WorldToLocalMatrix => transform.worldToLocalMatrix;

    public Vector3 WorldPosition => transform.position;
    #endregion

    public void PlaceIndicator()
    {
        var ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Indicator.SetParent(transform);
            Indicator.transform.position = hit.point + new Vector3(0, 0.1f, 0);
            Indicator.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }
    }

    public async void StartPlace()
    {
        var self = (PovController)this;
        var cameraGo = GameObject.FindWithTag("360Camera");
        var parentTransform = self.transform.parent.parent;

        // update camera position
        cameraGo.transform.position = self.transform.position;

        var ibr = parentTransform.GetComponent<IbrManager>();
        var sibr = parentTransform.GetComponent<SmoothIbrManager>();
        if (ibr)
        {
            await ibr.StartFrom(self);
        }
        else if (sibr)
        {
            var TextureManager = FindObjectOfType<SmoothTextureManager>(true);
            var keyFrame = new PovKeyFrame(this, this) { KeyFrameSource = KeyFrameSource.Editor, };
            var _ = await TextureManager.LoadTexture(keyFrame, new CancellationTokenSource(),
            (st) =>
            {
                sibr.StartFrom(self, st);
            });
        }
    }

	public void SetSelected(bool isSelected)
	{
        var mat = Indicator.gameObject.GetComponent<MeshRenderer>().material;
        mat.mainTexture = isSelected ? selTexture : norTexture;
        float alpha = isSelected ? 1f : 0f;

        Color baseColor = mat.GetColor("_BaseColor");
        baseColor.a = alpha;
        mat.SetColor("_BaseColor", baseColor);
    }
}