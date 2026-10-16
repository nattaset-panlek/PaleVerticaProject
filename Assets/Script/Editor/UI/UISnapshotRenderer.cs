using System;
using System.Collections.Generic;
using System.Text;
using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.UI;
using CoreSystem.UI.DevTools;
using CoreSystem.UI.Diagnostics;
using CoreSystem.UI.Inspect;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CoreSystem.Editor.UI
{
    public static class UISnapshotRenderer
    {
        #region [Variables/Fields]
        private const string ModalPrefabPath = "Assets/Prefab/Game Page/Global/Global Modal.prefab";
        private const string RegistryPath = "Assets/Prefab/UIElementRegistry.asset";
        private const int ScreenWidth = 1920;
        private const int ScreenHeight = 1080;
        private static readonly Vector2 DevToolSize = new Vector2(1000f, 680f);
        private static readonly Vector2 GallerySize = new Vector2(900f, 760f);
        private static readonly Color Backdrop = new Color(0.16f, 0.18f, 0.22f, 1f);
        #endregion

        #region [Public Methods]
        [MenuItem("Realm of Hero/UI/Render UI Snapshots")]
        public static void RenderFromMenu()
        {
            Debug.Log(RenderAll(false));
            EditorUtility.RevealInFinder(UIDebugSnapshot.OutputDirectory);
        }

        public static void RenderAllBatch()
        {
            int exitCode = 0;
            try
            {
                Debug.Log(RenderAll(true));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static string RenderAll(bool seedPreviewData)
        {
            return Run(true, seedPreviewData, out _);
        }

        public static Dictionary<string, List<UILayoutIssue>> InspectAll()
        {
            Run(false, false, out var results);
            return results;
        }
        #endregion

        #region [Private Methods]
        private static string Run(bool render, bool seedPreviewData, out Dictionary<string, List<UILayoutIssue>> results)
        {
            var registry = AssetDatabase.LoadAssetAtPath<UIElementRegistrySO>(RegistryPath);
            var modalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModalPrefabPath);
            if (registry == null || modalPrefab == null) throw new InvalidOperationException("UIElementRegistry or Global Modal prefab not found.");

            var found = new Dictionary<string, List<UILayoutIssue>>();
            var summary = new StringBuilder($"[UISnapshotRenderer] Output: {UIDebugSnapshot.OutputDirectory}\n");
            Scene scene = EditorSceneManager.NewPreviewScene();

            void Snapshot(Func<string> name, Vector2 size, Action<UIGlobalModal> build)
            {
                var issues = Process(scene, registry, modalPrefab, name, size, build, render, out string fileName);
                found[fileName] = issues;
                int errors = issues.FindAll(issue => issue.Severity == UIIssueSeverity.Error).Count;
                summary.AppendLine($"{fileName}: {issues.Count} issue(s), {errors} error(s)");
            }

            try
            {
                Snapshot(() => "gallery", GallerySize, modal => modal.Build(BuildGallery));

                var player = CreatePreviewPlayer(scene, seedPreviewData);
                var devTool = new GameDevToolUI(player);
                int pageCount = 1;
                for (int page = 0; page < pageCount; page++)
                {
                    int index = page;
                    string title = string.Empty;
                    Snapshot(() => $"devtool_{index + 1:00}_{title}", DevToolSize, modal =>
                    {
                        devTool.Setup(modal);
                        if (modal.Document.TryGetPages(GameDevToolUI.PagesId, out UIPageSet pages))
                        {
                            pageCount = pages.Count;
                            title = pages.GetTitle(index);
                        }
                        modal.ShowPage(GameDevToolUI.PagesId, index);
                    });
                }

                Snapshot(() => "devtool_01b_Database_expanded", DevToolSize, modal =>
                {
                    devTool.Setup(modal);
                    modal.Document.SetState("db.Consumable.open", true);
                    modal.Document.SetState("db.Equipment.open", true);
                    modal.RefreshPage(GameDevToolUI.PagesId);
                });

                Snapshot(() => "deathscreen", new Vector2(256f, 128f), modal => new PlayerDeathScreen().Setup(modal));
                Snapshot(() => "inspect_character", new Vector2(480f, 560f), modal => new CharacterInspectWindow(player).Setup(modal));

                Snapshot(() => "dark_gallery", GallerySize, modal => modal.SetThemeMode(UIThemeMode.Dark).Build(BuildGallery));
                Snapshot(() => "dark_devtool_06_Skills", DevToolSize, modal =>
                {
                    modal.SetThemeMode(UIThemeMode.Dark);
                    devTool.Setup(modal);
                    modal.ShowPage(GameDevToolUI.PagesId, 5);
                });
                Snapshot(() => "dark_inspect_character", new Vector2(480f, 560f), modal =>
                {
                    modal.SetThemeMode(UIThemeMode.Dark);
                    new CharacterInspectWindow(player).Setup(modal);
                });
                Snapshot(() => "dark_deathscreen", new Vector2(256f, 128f), modal =>
                {
                    modal.SetThemeMode(UIThemeMode.Dark);
                    new PlayerDeathScreen().Setup(modal);
                });
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }

            results = found;
            return summary.ToString();
        }

        private static List<UILayoutIssue> Process(Scene scene, UIElementRegistrySO registry, GameObject modalPrefab, Func<string> name, Vector2 windowSize, Action<UIGlobalModal> build, bool render, out string fileName)
        {
            var root = new GameObject("[UISnapshot]");
            SceneManager.MoveGameObjectToScene(root, scene);
            var texture = render ? new RenderTexture(ScreenWidth, ScreenHeight, 24, RenderTextureFormat.ARGB32) : null;

            try
            {
                var camera = CreateCamera(root, scene, texture);
                var canvas = CreateCanvas(root, camera);

                var modal = Object.Instantiate(modalPrefab, canvas.transform, false).GetComponent<UIGlobalModal>();
                modal.SetRegistry(registry).SetWindowSize(windowSize).Center();
                build(modal);
                fileName = name();

                RefreshLayout(canvas);
                SettleScrollRects(modal.gameObject);
                RefreshLayout(canvas);

                var issues = UILayoutInspector.Inspect(modal.WindowRect, modal.Theme);
                if (!render) return issues;

                string basePath = UIDebugSnapshot.GetBasePath(fileName);
                SaveFrame(camera, texture, basePath, string.Empty);
                RectTransform visible = modal.ScrollRect != null && modal.ScrollRect.viewport != null ? modal.ScrollRect.viewport : modal.WindowRect;
                if (UIDebugOverlay.Build(canvas, issues, visible) != null)
                {
                    RefreshLayout(canvas);
                    SaveFrame(camera, texture, basePath, ".issues");
                }
                UIDebugSnapshot.SaveReport(basePath, UILayoutInspector.FormatReport(fileName, modal.WindowRect, issues));
                return issues;
            }
            finally
            {
                Object.DestroyImmediate(root);
                if (texture != null)
                {
                    texture.Release();
                    Object.DestroyImmediate(texture);
                }
            }
        }

        private static Camera CreateCamera(GameObject root, Scene scene, RenderTexture texture)
        {
            var cameraGo = new GameObject("Camera", typeof(Camera));
            cameraGo.transform.SetParent(root.transform, false);
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraGo.GetComponent<Camera>();
            camera.scene = scene;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Backdrop;
            camera.targetTexture = texture;
            camera.enabled = false;
            return camera;
        }

        private static Canvas CreateCanvas(GameObject root, Camera camera)
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(root.transform, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 5f;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ScreenWidth, ScreenHeight);
            scaler.matchWidthOrHeight = 1f;

            if (camera.targetTexture == null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                var rect = (RectTransform)canvasGo.transform;
                rect.sizeDelta = new Vector2(ScreenWidth, ScreenHeight);
            }
            return canvas;
        }

        private static void RefreshLayout(Canvas canvas)
        {
            for (int i = 0; i < 3; i++)
            {
                Canvas.ForceUpdateCanvases();
                foreach (var group in canvas.GetComponentsInChildren<LayoutGroup>(false))
                {
                    LayoutRebuilder.MarkLayoutForRebuild((RectTransform)group.transform);
                }
                Canvas.ForceUpdateCanvases();
            }
        }

        private static void SettleScrollRects(GameObject root)
        {
            foreach (var scrollRect in root.GetComponentsInChildren<ScrollRect>(true))
            {
                if (scrollRect.content == null || scrollRect.viewport == null) continue;
                Vector2 content = scrollRect.content.rect.size;
                Vector2 view = scrollRect.viewport.rect.size;
                if (scrollRect.horizontalScrollbar != null)
                {
                    scrollRect.horizontalScrollbar.gameObject.SetActive(scrollRect.horizontal && content.x > view.x + 0.5f);
                }
                if (scrollRect.verticalScrollbar != null)
                {
                    scrollRect.verticalScrollbar.gameObject.SetActive(scrollRect.vertical && content.y > view.y + 0.5f);
                }
            }
        }

        private static void SaveFrame(Camera camera, RenderTexture texture, string basePath, string suffix)
        {
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = texture;
            var image = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            image.Apply();
            RenderTexture.active = previous;

            UIDebugSnapshot.SaveImage(basePath, suffix, image);
            Object.DestroyImmediate(image);
        }

        private static Character CreatePreviewPlayer(Scene scene, bool seed)
        {
            var go = new GameObject("[PreviewPlayer]");
            SceneManager.MoveGameObjectToScene(go, scene);
            var player = go.AddComponent<Character>();
            player.Initialize();
            if (!seed) return player;

            var items = Database.GetItems;
            for (int i = 0; i < items.Count && i < 14; i += 3) GameData.Player.Inventory.AddItem(items[i]);

            var skills = Database.Skills;
            for (int i = 0; i < skills.Count && i < 2; i++) player.skillCaster.GrantSkill(skills[i]);

            player.PassivePoints = 3;
            player.PassiveNodes.Add(new PassiveNodeRecord(1, (int)AttributeType.STR, 5f));
            return player;
        }

        private static void BuildGallery(UIBuilderContext ui)
        {
            ui.Title("UI Gallery");
            ui.Caption("Every builder element with its default settings. Used to catch layout regressions.");

            ui.Row(row =>
            {
                row.Heading("Header row with actions").Fill();
                row.Button("Add All To Inventory", null, UIVariant.Danger);
                row.Button("Secondary", null, UIVariant.Secondary);
            });

            ui.Row(row =>
            {
                row.Caption("Search").Width(56f);
                row.Input("", null).Fill();
                row.Button("Clear", null, UIVariant.Secondary);
            });

            ui.Row(row =>
            {
                row.Label("Row of buttons").Fill();
                foreach (UIVariant variant in Enum.GetValues(typeof(UIVariant))) row.Button(variant.ToString(), null, variant);
            });

            ui.Panel(panel =>
            {
                panel.Heading("Detail panel");
                panel.KeyValue("Type", "Consumable");
                panel.KeyValue("Rarity", "Common");
                panel.Label("A long description that has to wrap over several lines without overlapping the separator or the button that follows it in this panel.");
                panel.Separator();
                panel.Button("Primary full width", null, UIVariant.Primary).Fill();
            }).CornerRadius(12f);

            ui.Foldout("gallery.fold", "Foldout (expanded)", fold =>
            {
                fold.Grid(grid =>
                {
                    for (int i = 0; i < 9; i++) grid.Button($"Grid item {i + 1}", null);
                }).Cell(132f, 44f).Gap(6f);
            }, true);

            ui.Row(row =>
            {
                row.Toggle("Toggle", true, null);
                row.Progress(0.6f).Width(220f);
                row.Badge("Badge");
                row.Badge("Warn", ui.Theme.Warning);
            });
        }
        #endregion
    }
}
