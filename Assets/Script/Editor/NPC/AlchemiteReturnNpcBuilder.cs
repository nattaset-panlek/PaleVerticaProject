using System.Collections.Generic;
using System.Linq;
using CoreSystem.Editor.CharacterAsset;
using CoreSystem.Entity;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor.NPC
{
    public static class AlchemiteReturnNpcBuilder
    {
        #region [Variables/Fields]
        public const string SandBoxScenePath = "Assets/Scenes/SandBox.unity";
        public const string PreviewFolder = "Logs/CharacterPreview";
        public const string ReferencePrefabPath = "Assets/Addressable/Prefab/Entity/Ally/Ally.prefab";
        public static readonly Vector3 AlchemistSandBoxPosition = new Vector3(-2.2f, 0.85f, 0f);
        public static readonly Vector3 BlacksmithSandBoxPosition = new Vector3(-3.2f, 0.85f, 0f);
        public static readonly float[] PreviewTimes = { 0f, 0.25f, 0.5f, 0.75f };

        private const string MenuRoot = "Pale Vertica/Entity/AlchemiteReturn NPCs/";
        private const string PsbFolder = "Assets/Image/Character/";
        private const string PrefabFolder = "Assets/Addressable/Prefab/Entity/NPC/";

        public static readonly NpcPrefabSpec Alchemist = new NpcPrefabSpec("Alchemist", PsbFolder + "Alchemist.psb", PrefabFolder + "Alchemist.prefab", "alchemist", 2001, CharacterTag.Merchant, "Socket", "Socket");
        public static readonly NpcPrefabSpec Shopkeeper = new NpcPrefabSpec("Shopkeeper", PsbFolder + "Shopkeeper.psb", PrefabFolder + "Shopkeeper.prefab", "shopkeeper", 2003, CharacterTag.Villager);
        public static readonly NpcPrefabSpec OldMan = new NpcPrefabSpec("OldMan", PsbFolder + "OldMan.psb", PrefabFolder + "OldMan.prefab", "oldman", 2004, CharacterTag.Villager);
        public static readonly NpcPrefabSpec Blacksmith = new NpcPrefabSpec("Blacksmith", PsbFolder + "Blacksmith.psb", PrefabFolder + "Blacksmith.prefab", "blacksmith", 2002, CharacterTag.Merchant, "Refinement", "Refine");
        public static readonly NpcPrefabSpec[] All = { Alchemist, Shopkeeper, OldMan, Blacksmith };
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [MenuItem(MenuRoot + "Build Prefabs")]
        public static void BuildMenu()
        {
            List<string> log = new List<string>();
            bool ok = BuildAll(log);
            Report(log, ok);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Alchemist.PrefabPath);
            if (prefab == null) return;
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        [MenuItem(MenuRoot + "Place Alchemist In Scene")]
        public static void PlaceMenu()
        {
            PlaceInOpenScene(Alchemist);
        }

        [MenuItem(MenuRoot + "Place Blacksmith In Scene")]
        public static void PlaceBlacksmithMenu()
        {
            PlaceInOpenScene(Blacksmith);
        }

        [MenuItem(MenuRoot + "Apply Standard To PSBs")]
        public static void StandardizeMenu()
        {
            List<string> log = new List<string>();
            Report(log, StandardizeAll(log));
        }

        [MenuItem(MenuRoot + "Render Preview")]
        public static void PreviewMenu()
        {
            List<string> log = new List<string>();
            bool ok = RenderPreview(log);
            Report(log, ok);
            if (ok) EditorUtility.RevealInFinder(PreviewFolder + "/AlchemiteReturn_Idle.png");
        }

        public static bool BuildAll(List<string> log)
        {
            CharacterAssetSettings settings = CharacterAssetSettings.Instance;
            bool ok = true;
            foreach (NpcPrefabSpec spec in All)
            {
                if (NpcPrefabBuilder.Build(spec, settings, log) == null) ok = false;
            }
            AssetDatabase.SaveAssets();
            return ok;
        }

        public static bool StandardizeAll(List<string> log)
        {
            CharacterAssetSettings settings = CharacterAssetSettings.Instance;
            bool ok = true;
            foreach (NpcPrefabSpec spec in All)
            {
                AssetDatabase.ImportAsset(spec.PsbPath, ImportAssetOptions.ForceUpdate);
                CharacterPsbStandardizer.ApplyStandard(spec.PsbPath, settings, log);
                List<CharacterCheckResult> results = CharacterPsbValidator.Validate(spec.PsbPath, settings);
                foreach (CharacterCheckResult result in results)
                {
                    if (result.Severity >= CharacterCheckSeverity.Warning) log.Add($"{spec.Name}: {result.Severity} {result.Message}");
                }
                CharacterCheckSeverity worst = CharacterPsbValidator.WorstSeverity(results);
                log.Add($"{spec.Name}: Validate {worst}");
                if (worst == CharacterCheckSeverity.Error) ok = false;
            }
            return ok;
        }

        public static bool RenderPreview(List<string> log)
        {
            List<GameObject> prefabs = new List<GameObject> { AssetDatabase.LoadAssetAtPath<GameObject>(ReferencePrefabPath) };
            prefabs.AddRange(All.Select(s => AssetDatabase.LoadAssetAtPath<GameObject>(s.PrefabPath)));
            if (prefabs.Any(p => p == null))
            {
                log.Add("A prefab is missing; run Build Prefabs first.");
                return false;
            }
            bool idle = CharacterPreviewRenderer.RenderSheet(prefabs, "Idle", PreviewTimes, PreviewFolder + "/AlchemiteReturn_Idle.png", log);
            bool walk = CharacterPreviewRenderer.RenderSheet(prefabs, "walk", PreviewTimes, PreviewFolder + "/AlchemiteReturn_Walk.png", log);
            return idle && walk;
        }

        public static void BuildBatch()
        {
            List<string> log = new List<string>();
            bool ok = false;
            try
            {
                ok = BuildAll(log);
                GameObject alchemist = AssetDatabase.LoadAssetAtPath<GameObject>(Alchemist.PrefabPath);
                if (ok && alchemist != null) ok = NpcPrefabBuilder.PlaceInSceneFile(SandBoxScenePath, alchemist, AlchemistSandBoxPosition, log);
                GameObject blacksmith = AssetDatabase.LoadAssetAtPath<GameObject>(Blacksmith.PrefabPath);
                if (ok && blacksmith != null) ok = NpcPrefabBuilder.PlaceInSceneFile(SandBoxScenePath, blacksmith, BlacksmithSandBoxPosition, log);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                ok = false;
            }
            Report(log, ok);
            EditorApplication.Exit(ok ? 0 : 1);
        }

        public static void StandardizeBatch()
        {
            List<string> log = new List<string>();
            bool ok = false;
            try
            {
                ok = StandardizeAll(log) && RenderPreview(log);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            Report(log, ok);
            EditorApplication.Exit(ok ? 0 : 1);
        }

        public static void PreviewBatch()
        {
            List<string> log = new List<string>();
            bool ok = false;
            try
            {
                ok = RenderPreview(log);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            Report(log, ok);
            EditorApplication.Exit(ok ? 0 : 1);
        }
        #endregion

        #region [Private Methods]
        private static void PlaceInOpenScene(NpcPrefabSpec spec)
        {
            List<string> log = new List<string>();
            GameObject prefab = NpcPrefabBuilder.Build(spec, CharacterAssetSettings.Instance, log);
            if (prefab == null)
            {
                Report(log, false);
                return;
            }
            SceneView view = SceneView.lastActiveSceneView;
            NpcPrefabBuilder.PlaceInOpenScene(prefab, view != null ? view.pivot : Vector3.zero, "Place " + spec.Name);
        }

        private static void Report(List<string> log, bool ok)
        {
            string text = "[AlchemiteReturnNpcs] " + (ok ? "Done" : "Finished with problems") + "\n" + string.Join("\n", log);
            if (ok) Debug.Log(text);
            else Debug.LogWarning(text);
        }
        #endregion
    }
}
