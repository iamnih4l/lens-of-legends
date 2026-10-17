using System;
using System.IO;
using UnityEngine;
using LensOfLegends.Gallery;

namespace LensOfLegends.Core
{
    /// <summary>
    /// Manages local persistence in Application.persistentDataPath.
    /// Stores relative filenames for captured photographs so saved paths remain valid across restarts.
    /// Gracefully recovers from missing or corrupted save files.
    /// </summary>
    public static class SaveSystem
    {
        private const string SaveFileName = "LensOfLegends_SaveState.json";
        private const string PhotosSubfolderName = "LensOfLegends_Photos";

        public static string GetPhotosDirectoryPath()
        {
            string dir = Path.Combine(Application.persistentDataPath, PhotosSubfolderName);
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LensOfLegends.SaveSystem] Could not create photos directory: " + ex.Message);
            }
            return dir;
        }

        public static string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }

        public static string ResolvePhotoFullPath(string relativeFileName)
        {
            if (string.IsNullOrEmpty(relativeFileName))
                return string.Empty;
            string safeFileName = Path.GetFileName(relativeFileName);
            return Path.Combine(GetPhotosDirectoryPath(), safeFileName);
        }

        public static void Save(PlayerSaveStateData data)
        {
            if (data == null) return;
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string path = GetSaveFilePath();
                string tempPath = path + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(tempPath, path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LensOfLegends.SaveSystem] Failed to write local save file: " + ex.Message);
            }
        }

        public static PlayerSaveStateData LoadOrCreateNew()
        {
            string path = GetSaveFilePath();
            if (!File.Exists(path))
            {
                return CreateDefaultState();
            }

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return CreateDefaultState();

                PlayerSaveStateData loaded = JsonUtility.FromJson<PlayerSaveStateData>(json);
                if (loaded == null)
                    return CreateDefaultState();

                EnsureListsInitialized(loaded);
                return loaded;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LensOfLegends.SaveSystem] Corrupted or unreadable save file detected; initializing clean state. Details: " + ex.Message);
                return CreateDefaultState();
            }
        }

        public static bool HasExistingSaveFile()
        {
            return File.Exists(GetSaveFilePath());
        }

        private static PlayerSaveStateData CreateDefaultState()
        {
            PlayerSaveStateData state = new PlayerSaveStateData();
            EnsureListsInitialized(state);
            state.unlockedCameraProfileIds.Add("cam_basic_travel");
            state.unlockedCameraProfileIds.Add("cam_pro_mirrorless");
            state.unlockedCameraProfileIds.Add("cam_vintage_film");
            return state;
        }

        private static void EnsureListsInitialized(PlayerSaveStateData state)
        {
            if (state.discoveredPoiIds == null) state.discoveredPoiIds = new System.Collections.Generic.List<string>();
            if (state.completedAssignmentIds == null) state.completedAssignmentIds = new System.Collections.Generic.List<string>();
            if (state.unlockedCameraProfileIds == null) state.unlockedCameraProfileIds = new System.Collections.Generic.List<string>();
            if (state.earnedBadges == null) state.earnedBadges = new System.Collections.Generic.List<string>();
            if (state.capturedPhotos == null) state.capturedPhotos = new System.Collections.Generic.List<PhotoMetadata>();
        }
    }
}
