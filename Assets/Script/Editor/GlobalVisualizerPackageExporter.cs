using System.IO;
using UnityEditor;
using UnityEngine;

namespace PaleVertica.EditorTools
{
    public static class GlobalVisualizerPackageExporter
    {
        #region [Variables/Fields]
        private const string PackagePath = "Packages/com.globalvisualizer.core";
        private const string OutputDirectory = "Builds/Releases";
        private const string PackageName = "GlobalVisualizer-v1.0.0.unitypackage";
        #endregion

        #region [Public Methods]
        [MenuItem("Tools/Global Visualizer/Export .unitypackage")]
        public static void Export()
        {
            string fullDir = Path.GetFullPath(OutputDirectory);
            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
            }

            string destination = Path.Combine(OutputDirectory, PackageName).Replace("\\", "/");
            AssetDatabase.ExportPackage(PackagePath, destination, ExportPackageOptions.Recurse);
            Debug.Log($"[Global Visualizer] Exported package to: {destination}");
        }
        #endregion

        #region [Private Methods]
        [InitializeOnLoadMethod]
        private static void AutoExportOnce()
        {
            string destination = Path.Combine(OutputDirectory, PackageName).Replace("\\", "/");
            if (File.Exists(destination)) return;
            EditorApplication.delayCall += Export;
        }
        #endregion
    }
}
