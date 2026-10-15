#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace EggMerge.EditorTools
{
    /// <summary>
    /// Prevents magenta/pink UI on Android when UI shaders get stripped.
    /// </summary>
    public class EnsureUiShaders : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Include(new[]
            {
                "UI/Default",
                "UI/Default Font",
                "UI/DefaultETC1",
                "Sprites/Default",
                "Legacy Shaders/Transparent/Diffuse",
                "Unlit/Color",
                "Unlit/Transparent"
            });
        }

        [MenuItem("EggMerge/Ensure UI Shaders Included")]
        public static void MenuEnsure()
        {
            Include(new[]
            {
                "UI/Default",
                "UI/Default Font",
                "UI/DefaultETC1",
                "Sprites/Default",
                "Unlit/Color",
                "Unlit/Transparent"
            });
            Debug.Log("UI shaders added to Always Included Shaders.");
        }

        static void Include(string[] names)
        {
            var graphicsSettingsObj =
                AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/GraphicsSettings.asset");
            if (graphicsSettingsObj == null)
                return;

            var so = new SerializedObject(graphicsSettingsObj);
            var arr = so.FindProperty("m_AlwaysIncludedShaders");
            if (arr == null)
                return;

            foreach (var name in names)
            {
                var shader = Shader.Find(name);
                if (shader == null)
                    continue;

                var exists = false;
                for (var i = 0; i < arr.arraySize; i++)
                {
                    if (arr.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                    {
                        exists = true;
                        break;
                    }
                }

                if (exists)
                    continue;

                arr.InsertArrayElementAtIndex(arr.arraySize);
                arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = shader;
            }

            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
