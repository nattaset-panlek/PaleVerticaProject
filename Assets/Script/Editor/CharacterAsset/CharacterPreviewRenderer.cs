using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreSystem.Editor.CharacterAsset
{
    public static class CharacterPreviewRenderer
    {
        #region [Variables/Fields]
        public const int CellSize = 256;
        public const float OrthographicSize = 0.6f;
        public const float FrameCenterY = 0.1f;
        public static readonly Color Background = new Color(0.82f, 0.82f, 0.82f, 1f);
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static bool RenderSheet(IReadOnlyList<GameObject> prefabs, string clipName, IReadOnlyList<float> normalizedTimes, string outputPath, List<string> log)
        {
            if (prefabs == null || prefabs.Count == 0 || normalizedTimes == null || normalizedTimes.Count == 0)
            {
                log.Add("Nothing to render.");
                return false;
            }
            Scene scene = EditorSceneManager.NewPreviewScene();
            GameObject root = new GameObject("[PreviewRoot]");
            SceneManager.MoveGameObjectToScene(root, scene);
            RenderTexture target = new RenderTexture(CellSize, CellSize, 24, RenderTextureFormat.ARGB32);
            Texture2D cell = new Texture2D(CellSize, CellSize, TextureFormat.RGBA32, false);
            Texture2D sheet = new Texture2D(CellSize * normalizedTimes.Count, CellSize * prefabs.Count, TextureFormat.RGBA32, false);
            try
            {
                Camera camera = CreateCamera(root.transform, scene, target);
                for (int row = 0; row < prefabs.Count; row++)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[row], scene);
                    instance.transform.position = Vector3.zero;
                    Animator animator = instance.GetComponentInChildren<Animator>();
                    AnimationClip clip = FindClip(animator, clipName);
                    if (clip == null) log.Add($"{prefabs[row].name}: no clip '{clipName}' on its Animator; rendered the rest pose.");
                    camera.Render();
                    for (int column = 0; column < normalizedTimes.Count; column++)
                    {
                        if (clip != null) clip.SampleAnimation(animator.gameObject, normalizedTimes[column] * clip.length);
                        camera.Render();
                        RenderTexture previous = RenderTexture.active;
                        RenderTexture.active = target;
                        cell.ReadPixels(new Rect(0, 0, CellSize, CellSize), 0, 0);
                        cell.Apply();
                        RenderTexture.active = previous;
                        sheet.SetPixels(column * CellSize, (prefabs.Count - 1 - row) * CellSize, CellSize, CellSize, cell.GetPixels());
                    }
                    Object.DestroyImmediate(instance);
                }
                sheet.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)));
                File.WriteAllBytes(outputPath, sheet.EncodeToPNG());
                log.Add($"Rendered {prefabs.Count} character(s) x {normalizedTimes.Count} frame(s) of '{clipName}' to {outputPath}.");
                return true;
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(cell);
                Object.DestroyImmediate(sheet);
                target.Release();
                Object.DestroyImmediate(target);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
        #endregion

        #region [Private Methods]
        private static Camera CreateCamera(Transform parent, Scene scene, RenderTexture target)
        {
            GameObject cameraObject = new GameObject("PreviewCamera", typeof(Camera));
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position = new Vector3(0f, FrameCenterY, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.scene = scene;
            camera.orthographic = true;
            camera.orthographicSize = OrthographicSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.targetTexture = target;
            camera.enabled = false;
            return camera;
        }

        private static AnimationClip FindClip(Animator animator, string clipName)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return null;
            return animator.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name == clipName);
        }
        #endregion
    }
}
