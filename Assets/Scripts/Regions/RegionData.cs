using System;
using System.Collections.Generic;
using UnityEngine;

namespace LensOfLegends.Regions
{
    public enum DestinationPlayableState
    {
        PlayableVerticalSlice = 0,
        PreviewFutureChapter = 1
    }

    [Serializable]
    public class CulturalPointOfInterestDefinition
    {
        public string poiId;
        public string title;
        public string category; // Architecture, Rituals, Traditional Crafts, Music & Performance, Natural Landscape, Folklore
        [TextArea(2, 5)]
        public string verifiedCulturalInformation;
        public string verifiedReferenceSource;
        [TextArea(2, 4)]
        public string narrativeStorytellerNote; // Explicitly separated fictional/storyteller layer
        public Vector3 worldPosition;
        public float discoveryRadius = 6.5f;
    }

    [Serializable]
    public class RegionalPhotographySubjectDefinition
    {
        public string subjectId;
        public string displayName;
        public string category;
        public string compositionTip;
        public Vector3 localOffset;
    }

    /// <summary>
    /// ScriptableObject and serializable definition for each regional destination across India.
    /// Keeps the game's architecture expandable so new chapters across India can be added
    /// without modifying core gameplay systems.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRegionData", menuName = "Lens of Legends/Region Data", order = 1)]
    public class RegionData : ScriptableObject
    {
        [Header("Identity & Geography")]
        public string regionId;
        public string regionTitle;
        public string stateOrTerritory;
        public string chapterSubtitle;
        public DestinationPlayableState playableState = DestinationPlayableState.PreviewFutureChapter;
        public string targetSceneName;
        public Vector2 normalizedIndiaMapPosition; // (0..1, 0..1) on the India Travel Map UI

        [Header("Cultural & Visual Direction")]
        [TextArea(2, 4)]
        public string themeSummary;
        [TextArea(3, 6)]
        public string regionalIntroductionNarrative;
        [TextArea(3, 6)]
        public string verifiedCulturalBackground;
        public string primaryReferenceUrl;

        [Header("Atmospheric & Lighting Palette")]
        public Color skyTopColor = new Color(0.05f, 0.07f, 0.14f);
        public Color skyHorizonColor = new Color(0.22f, 0.11f, 0.08f);
        public Color ambientLightColor = new Color(0.24f, 0.16f, 0.14f);
        public Color keyLightColor = new Color(1.0f, 0.62f, 0.28f);
        public Color accentUiColor = new Color(0.92f, 0.45f, 0.18f);
        public float fogDensity = 0.018f;
        public Color fogColor = new Color(0.09f, 0.06f, 0.06f);

        [Header("Environmental Soundscape Profile")]
        public string soundscapeProfileId;
        public string ambientSoundscapeDescription;
        public float baseWindIntensity = 0.3f;
        public float nightInsectsIntensity = 0.6f;
        public float percussionRhythmTempoBpm = 96f;

        [Header("Planned & Discoverable Content")]
        public List<string> plannedPhotographySubjects = new List<string>();
        public List<CulturalPointOfInterestDefinition> pointsOfInterest = new List<CulturalPointOfInterestDefinition>();
    }
}
