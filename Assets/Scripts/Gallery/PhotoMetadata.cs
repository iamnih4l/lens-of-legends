using System;
using System.Collections.Generic;

namespace LensOfLegends.Gallery
{
    /// <summary>
    /// Serializable metadata for a single player-captured photograph.
    /// Stores relative file name inside Application.persistentDataPath so paths remain
    /// valid even if the game folder or OS user directory changes.
    /// </summary>
    [Serializable]
    public class PhotoMetadata
    {
        public string photoId;
        public string relativeFileName; // Stored inside persistentDataPath/LensOfLegends_Photos/
        public string regionId;
        public string regionDisplayName;
        public string primarySubjectTag;
        public string culturalPoiTag;
        public string captureTimestampIso;
        public string cameraProfileName;
        public float focalLengthMm;
        public float apertureFStop;
        public int isoSensitivity;
        public string shutterSpeedLabel;
        public string colorProfileLabel;
        public float compositionScore; // 0..100 based on framing, focus accuracy, and exposure
        public string compositionFeedback;
        public bool isPublishedToDemoCommunity;
        public bool isGenuinePlayerCapture = true;
    }

    [Serializable]
    public class PlayerSaveStateData
    {
        public string saveVersion = "1.0.0";
        public string lastVisitedRegionId = "kerala_theyyam";
        public bool hasSeenIntroCinematic = false;
        public int photographerXp = 0;
        public List<string> discoveredPoiIds = new List<string>();
        public List<string> completedAssignmentIds = new List<string>();
        public List<string> unlockedCameraProfileIds = new List<string>();
        public List<string> earnedBadges = new List<string>();
        public List<PhotoMetadata> capturedPhotos = new List<PhotoMetadata>();
        public float masterVolume = 0.85f;
        public float mouseSensitivity = 2.0f;
    }
}
