using System;
using UnityEditor;
using UnityEngine;

namespace CardboardXRDemo
{
    public class ExportUnityPackage
    {
        [MenuItem("CardboardXR/ExportPackage")]
        private static void ExportPackage()
        {
            string fileLocation = EditorUtility.SaveFilePanel(
                "Export Unity Package", String.Empty, "CardboardXR",
                "unitypackage");
            Debug.Log("Ready to export, location=" + fileLocation);
            AssetDatabase.ExportPackage("Assets/Cardboard", fileLocation,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
            Debug.Log("Package exported, location=" + fileLocation);
        }
    }
}