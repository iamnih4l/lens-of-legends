using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LensOfLegends.Core;

namespace LensOfLegends.Gallery
{
    /// <summary>
    /// Manages the player's personal in-game Photo Journal.
    /// Strictly stores and displays genuine player-captured photographs saved locally on disk.
    /// Gracefully handles missing or corrupted image files without crashing.
    /// </summary>
    public class PhotoJournalManager : MonoBehaviour
    {
        public static PhotoJournalManager Instance { get; private set; }

        private readonly Dictionary<string, Texture2D> _loadedTextureCache = new Dictionary<string, Texture2D>();

        public event Action<PhotoMetadata, Texture2D> OnPhotoAddedToJournal;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public IReadOnlyList<PhotoMetadata> GetCapturedPhotos()
        {
            if (GameStateManager.Instance == null || GameStateManager.Instance.SaveData == null)
                return Array.Empty<PhotoMetadata>();
            return GameStateManager.Instance.SaveData.capturedPhotos;
        }

        public PhotoMetadata SaveNewCaptureToJournal(
            Texture2D capturedTexture,
            string regionId,
            string regionTitle,
            string subjectTag,
            string poiTag,
            string cameraName,
            float focalLengthMm,
            float apertureFStop,
            int iso,
            string shutterSpeed,
            string colorProfile,
            float compositionScore,
            string compositionFeedback)
        {
            if (capturedTexture == null)
                return null;

            string timestampId = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string photoId = "LOL_" + timestampId;
            string relativeFileName = photoId + ".png";
            string fullPath = SaveSystem.ResolvePhotoFullPath(relativeFileName);

            try
            {
                byte[] pngBytes = capturedTexture.EncodeToPNG();
                if (pngBytes != null && pngBytes.Length > 0)
                {
                    File.WriteAllBytes(fullPath, pngBytes);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LensOfLegends.PhotoJournal] Failed to write PNG to disk: " + ex.Message);
            }

            PhotoMetadata meta = new PhotoMetadata
            {
                photoId = photoId,
                relativeFileName = relativeFileName,
                regionId = regionId,
                regionDisplayName = regionTitle,
                primarySubjectTag = string.IsNullOrEmpty(subjectTag) ? "Free Exploration Composition" : subjectTag,
                culturalPoiTag = poiTag ?? string.Empty,
                captureTimestampIso = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                cameraProfileName = cameraName,
                focalLengthMm = Mathf.Round(focalLengthMm),
                apertureFStop = (float)Math.Round(apertureFStop, 1),
                isoSensitivity = iso,
                shutterSpeedLabel = shutterSpeed,
                colorProfileLabel = colorProfile,
                compositionScore = Mathf.Round(compositionScore),
                compositionFeedback = compositionFeedback,
                isPublishedToDemoCommunity = false,
                isGenuinePlayerCapture = true
            };

            _loadedTextureCache[photoId] = capturedTexture;

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.RecordCapturedPhoto(meta);
            }

            OnPhotoAddedToJournal?.Invoke(meta, capturedTexture);
            return meta;
        }

        /// <summary>
        /// Attempts to load a player-captured photograph texture from disk.
        /// Returns false if the file is missing or corrupted so the UI can show a clean fallback notice.
        /// </summary>
        public bool TryGetPhotoTexture(PhotoMetadata meta, out Texture2D texture, out string statusMessage)
        {
            texture = null;
            statusMessage = "OK";

            if (meta == null)
            {
                statusMessage = "Missing metadata entry.";
                return false;
            }

            if (_loadedTextureCache.TryGetValue(meta.photoId, out Texture2D cached) && cached != null)
            {
                texture = cached;
                return true;
            }

            string fullPath = SaveSystem.ResolvePhotoFullPath(meta.relativeFileName);
            if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
            {
                statusMessage = "Image file missing on disk (" + meta.relativeFileName + ").";
                return false;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(fullPath);
                if (bytes == null || bytes.Length < 16)
                {
                    statusMessage = "Image file is empty or truncated.";
                    return false;
                }

                Texture2D loaded = new Texture2D(2, 2, TextureFormat.RGB24, false);
                if (!loaded.LoadImage(bytes))
                {
                    Destroy(loaded);
                    statusMessage = "Corrupted PNG image data.";
                    return false;
                }

                loaded.name = meta.photoId;
                _loadedTextureCache[meta.photoId] = loaded;
                texture = loaded;
                return true;
            }
            catch (Exception ex)
            {
                statusMessage = "Error reading image file: " + ex.Message;
                return false;
            }
        }

        public void DeletePhotoFromJournal(PhotoMetadata meta)
        {
            if (meta == null || GameStateManager.Instance == null) return;

            if (_loadedTextureCache.TryGetValue(meta.photoId, out Texture2D tex))
            {
                if (tex != null) Destroy(tex);
                _loadedTextureCache.Remove(meta.photoId);
            }

            string fullPath = SaveSystem.ResolvePhotoFullPath(meta.relativeFileName);
            try
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LensOfLegends.PhotoJournal] Could not delete image file: " + ex.Message);
            }

            GameStateManager.Instance.SaveData.capturedPhotos.Remove(meta);
            GameStateManager.Instance.SaveProgress();
        }
    }
}
