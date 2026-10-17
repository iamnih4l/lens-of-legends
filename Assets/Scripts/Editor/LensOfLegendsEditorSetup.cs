#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LensOfLegends.CameraEquipment;
using LensOfLegends.Core;
using LensOfLegends.Regions;

namespace LensOfLegends.EditorTools
{
    /// <summary>
    /// Provides a one-click Unity Editor menu utility ("Lens of Legends > Generate & Wire Project Assets & Scenes")
    /// for beginners so ScriptableObject assets, scenes, and EditorBuildSettings can also be inspected
    /// and saved directly inside the Unity Editor.
    /// </summary>
    public static class LensOfLegendsEditorSetup
    {
        [MenuItem("Lens of Legends/1. Generate & Wire All Scenes, ScriptableObjects & Build Settings", priority = 1)]
        public static void GenerateAndWireAllAssets()
        {
            EnsureFolders();
            GenerateRegionDataAssets();
            GenerateCameraProfileAssets();
            GenerateAndRegisterScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Lens of Legends Setup Complete",
                "All RegionData ScriptableObjects, CameraProfile ScriptableObjects, and Scenes (MainMenu, TravelMap, Theyyam_Ritual_Scene) have been generated and registered in Build Settings.\n\nPress Play in the Unity Editor to start Lens of Legends!",
                "OK");
        }

        [MenuItem("Lens of Legends/2. Open MainMenu Scene", priority = 2)]
        public static void OpenMainMenuScene()
        {
            if (File.Exists("Assets/Scenes/MainMenu.unity"))
                EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        }

        [MenuItem("Lens of Legends/3. Open Theyyam Ritual Playable Scene", priority = 3)]
        public static void OpenTheyyamScene()
        {
            if (File.Exists("Assets/Scenes/Regions/Theyyam_Ritual_Scene.unity"))
                EditorSceneManager.OpenScene("Assets/Scenes/Regions/Theyyam_Ritual_Scene.unity");
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                "Assets/Scenes",
                "Assets/Scenes/Regions",
                "Assets/Resources/Regions",
                "Assets/Resources/CameraProfiles"
            };
            foreach (string f in folders)
            {
                if (!Directory.Exists(f))
                    Directory.CreateDirectory(f);
            }
        }

        private static void GenerateRegionDataAssets()
        {
            List<RegionData> regions = IndiaDestinationCatalog.GetAllDestinations();
            foreach (RegionData r in regions)
            {
                string assetPath = "Assets/Resources/Regions/" + r.name + ".asset";
                if (AssetDatabase.LoadAssetAtPath<RegionData>(assetPath) == null)
                {
                    RegionData copy = Object.Instantiate(r);
                    AssetDatabase.CreateAsset(copy, assetPath);
                }
            }
        }

        private static void GenerateCameraProfileAssets()
        {
            List<CameraProfile> profiles = CameraProfile.GetDefaultProfiles();
            foreach (CameraProfile p in profiles)
            {
                string assetPath = "Assets/Resources/CameraProfiles/" + p.name + ".asset";
                if (AssetDatabase.LoadAssetAtPath<CameraProfile>(assetPath) == null)
                {
                    CameraProfile copy = Object.Instantiate(p);
                    AssetDatabase.CreateAsset(copy, assetPath);
                }
            }
        }

        private static void GenerateAndRegisterScenes()
        {
            string[] scenePaths =
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/TravelMap.unity",
                "Assets/Scenes/Regions/Theyyam_Ritual_Scene.unity"
            };

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
            foreach (string path in scenePaths)
            {
                Scene s = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                GameObject boot = new GameObject("LensOfLegendsSceneBootstrapper");
                boot.AddComponent<LensOfLegendsBootstrapper>();
                EditorSceneManager.SaveScene(s, path);
                buildScenes.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
        }
    }
}
#endif
