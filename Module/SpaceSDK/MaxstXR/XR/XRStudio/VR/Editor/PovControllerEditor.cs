using MaxstXR.Extension;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(PovController))]
public class PovControllerEditor : Editor
{
    public override async void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Start From This"))
        {
            var cameraGo = GameObject.FindWithTag("360Camera");
            
            if (null == cameraGo)
            {
                Debug.LogError("Cannot Find an 'active' GameObject with tag '360Camera'!");
                return;
            }

            var self = (PovController)target;
            var parentTransform = self.transform.parent.parent;

            // update camera position
            cameraGo.transform.position = self.transform.position;

            // make the pov become the starting point
            var ibr = parentTransform.GetComponent<IbrManager>();
            var sibr = parentTransform.GetComponent<SmoothIbrManager>();
            if (ibr)
            {
                await ibr.StartFrom(self);
            }
            else if (sibr)
            {
                var TextureManager = FindObjectOfType<SmoothTextureManager>(true);
                var keyFrame = new PovKeyFrame(self, self)
                {
                    KeyFrameSource = KeyFrameSource.Editor,
                };
                var _ = await TextureManager.LoadTexture(keyFrame, new CancellationTokenSource(),
                (st) =>
                {
                    sibr.StartFrom(self, st);
                });
            }
			//parentTransform.SendMessage("StartFrom", self, SendMessageOptions.RequireReceiver);

			// apply changes
			if (!Application.isPlaying)
			{
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
        }
    }
}
