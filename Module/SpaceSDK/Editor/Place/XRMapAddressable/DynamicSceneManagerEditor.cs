#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace MaxstXR.Place
{
#if UNITY_EDITOR
    [CustomEditor(typeof(DynamicSceneManager))]
    public class DynamicSceneManagerEditor : Editor
    {
        private const int maxHeight = 25;
        private DynamicSceneManager dynamicSceneManager = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            /*
            if (PrefabUtility.GetPrefabType(target) == PrefabType.Prefab)
            {
                return;
            }
            */

            bool isDirty = false;
            dynamicSceneManager = (DynamicSceneManager)target;

            EditorGUILayout.Separator();

            var observer = dynamicSceneManager.GetComponent<NavigationLocationObserver>();
            if (observer) 
            {
                var poiContainer = dynamicSceneManager.GetComponent<PoiContainer>();
                var minimapPoiContainer = dynamicSceneManager.GetComponent<MinimapPoiContainer>();

                if (GUILayout.Button(new GUIContent("Remove to NavigationLocationObserver"),
                        GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(maxHeight)))
                {
                    DestroyImmediate(observer);
                    if (poiContainer) DestroyImmediate(poiContainer);
                    if (minimapPoiContainer) DestroyImmediate(minimapPoiContainer);
                    isDirty = true;
                }

                if (poiContainer)
                {
                    if (GUILayout.Button(new GUIContent("Disable PoI Loading"), 
                        GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(maxHeight)))
                    {
                        DestroyImmediate(poiContainer);
                        observer.IsWorldContent = false;
                        isDirty = true;
                    }
                }
                else
                {
                    if (GUILayout.Button(new GUIContent("Enable PoI Loading"), 
                        GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(maxHeight)))
                    {
                        dynamicSceneManager.gameObject.TryGetOrAddComponent<PoiContainer>();
                        observer.IsWorldContent = true;
                        isDirty = true;
                    }
                }

                if (minimapPoiContainer)
                {
                    if (GUILayout.Button(new GUIContent("Disable Minimap PoI Loading"),
                        GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(maxHeight)))
                    {
                        DestroyImmediate(minimapPoiContainer);
                        observer.IsMinimapContent = false;
                        isDirty = true;
                    }
                }
                else
                {
                    if (GUILayout.Button(new GUIContent("Enable Minimap PoI Loading"),
                        GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(maxHeight)))
                    {
                        dynamicSceneManager.gameObject.TryGetOrAddComponent<MinimapPoiContainer>();
                        observer.IsMinimapContent = true;
                        isDirty = true;
                    }
                }
            }
            else
            {
                var content = new GUIContent("Add to NavigationLocationObserver");
                if (GUILayout.Button(content, GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(maxHeight)))
                {
                    dynamicSceneManager.gameObject.TryGetOrAddComponent<NavigationLocationObserver>();
                    isDirty = true;
                }
            }

            
            GUILayout.Space(10);


            if (GUI.changed && isDirty)
            {
                EditorUtility.SetDirty(dynamicSceneManager);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
#endif
}
