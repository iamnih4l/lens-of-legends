using System;
using System.Collections.Generic;
using UnityEngine;
using LensOfLegends.Gallery;

namespace LensOfLegends.Community
{
    [Serializable]
    public class CommunityPostEntry
    {
        public string postId;
        public string photographerHandle;
        public string regionTitle;
        public string caption;
        public string cameraGearSummary;
        public string challengeTag;
        public int likesCount;
        public bool likedByLocalPlayer;
        public bool isSimulatedDemoEntry; // Strictly true for curated offline feed entries, false for player's own published photos
        public PhotoMetadata playerPhotoReference;
        public Color demoCardPrimaryColor;
        public Color demoCardAccentColor;
    }

    [Serializable]
    public class CommunityChallengeDefinition
    {
        public string challengeId;
        public string title;
        public string themeDescription;
        public string targetRegionName;
        public string rewardBadgeTitle;
    }

    /// <summary>
    /// Decouples the in-game Community UI from the underlying data source so a real online backend
    /// can be plugged in later without rewriting UI or gameplay scripts.
    /// </summary>
    public interface ICommunityDataSource
    {
        string DataSourceStatusBanner { get; }
        bool IsOnlineBackendConnected { get; }
        IReadOnlyList<CommunityPostEntry> GetFeedPosts();
        IReadOnlyList<CommunityChallengeDefinition> GetActiveChallenges();
        CommunityPostEntry PublishPlayerPhotograph(PhotoMetadata playerPhoto, string customCaption);
        void ToggleLikePost(string postId);
    }

    /// <summary>
    /// Explicitly labeled Offline Local Demonstration Feed.
    /// Never misleads the player into thinking fake online users are interacting live.
    /// </summary>
    public class OfflineDemoCommunityDataSource : ICommunityDataSource
    {
        private readonly List<CommunityPostEntry> _posts = new List<CommunityPostEntry>();
        private readonly List<CommunityChallengeDefinition> _challenges = new List<CommunityChallengeDefinition>();

        public string DataSourceStatusBanner =>
            "OFFLINE DEMONSTRATION FEED — Simulated Local Community Preview (No live online server connected)";

        public bool IsOnlineBackendConnected => false;

        public OfflineDemoCommunityDataSource()
        {
            InitializeDemoChallenges();
            InitializeDemoPosts();
        }

        private void InitializeDemoChallenges()
        {
            _challenges.Add(new CommunityChallengeDefinition
            {
                challengeId = "chal_nocturne_fire",
                title = "Weekly Theme: Sacred Firelight & Nocturnes",
                themeDescription = "Capture the interplay of oil lamps, embers, and shadow during nighttime rituals in North Kerala.",
                targetRegionName = "North Kerala — Theyyam",
                rewardBadgeTitle = "Community Nocturne Curator"
            });
            _challenges.Add(new CommunityChallengeDefinition
            {
                challengeId = "chal_sacred_architecture",
                title = "Regional Spotlight: Living Heritage Architecture",
                themeDescription = "Document traditional laterite shrines, timber gables, and sacred groves across India.",
                targetRegionName = "All Regions (India-Wide)",
                rewardBadgeTitle = "Architectural Chronicler"
            });
        }

        private void InitializeDemoPosts()
        {
            _posts.Add(new CommunityPostEntry
            {
                postId = "demo_post_01",
                photographerHandle = "[DEMO SAMPLE] @Ananya_MalabarLens",
                regionTitle = "North Kerala — Theyyam (Kannur)",
                caption = "[Offline Demo Entry] Framed the towering Mudi headdress at 85mm f/2.0 just as the Meleri embers rose against the banyan canopy.",
                cameraGearSummary = "Drishti Pro M-IV • 85mm • f/2.0 • ISO 1600 • 1/250s",
                challengeTag = "Sacred Firelight & Nocturnes",
                likesCount = 42,
                likedByLocalPlayer = false,
                isSimulatedDemoEntry = true,
                demoCardPrimaryColor = new Color(0.18f, 0.06f, 0.05f),
                demoCardAccentColor = new Color(0.95f, 0.38f, 0.14f)
            });

            _posts.Add(new CommunityPostEntry
            {
                postId = "demo_post_02",
                photographerHandle = "[DEMO SAMPLE] @Kabir_GhatChronicles",
                regionTitle = "Varanasi — Sacred Riverfront (Preview Concept)",
                caption = "[Offline Demo Entry] Looking forward to the Varanasi chapter—wooden boats slicing through winter mist at Assi Ghat with a 35mm rangefinder.",
                cameraGearSummary = "Chhaya-76 Rangefinder • 35mm • f/4.0 • ISO 400 • 1/125s",
                challengeTag = "Living Heritage Architecture",
                likesCount = 29,
                likedByLocalPlayer = false,
                isSimulatedDemoEntry = true,
                demoCardPrimaryColor = new Color(0.16f, 0.22f, 0.34f),
                demoCardAccentColor = new Color(0.92f, 0.68f, 0.28f)
            });

            _posts.Add(new CommunityPostEntry
            {
                postId = "demo_post_03",
                photographerHandle = "[DEMO SAMPLE] @Tashi_HighAltitude",
                regionTitle = "Ladakh — Monastic Festivals (Preview Concept)",
                caption = "[Offline Demo Entry] High-contrast courtyard shadows and silk brocades in the Indus Valley.",
                cameraGearSummary = "Kala-Rasa 6x6 • 65mm • f/5.6 • ISO 100 • 1/500s",
                challengeTag = "Living Heritage Architecture",
                likesCount = 51,
                likedByLocalPlayer = false,
                isSimulatedDemoEntry = true,
                demoCardPrimaryColor = new Color(0.10f, 0.24f, 0.48f),
                demoCardAccentColor = new Color(0.38f, 0.76f, 0.96f)
            });
        }

        public IReadOnlyList<CommunityPostEntry> GetFeedPosts() => _posts;

        public IReadOnlyList<CommunityChallengeDefinition> GetActiveChallenges() => _challenges;

        public CommunityPostEntry PublishPlayerPhotograph(PhotoMetadata playerPhoto, string customCaption)
        {
            if (playerPhoto == null) return null;
            playerPhoto.isPublishedToDemoCommunity = true;

            CommunityPostEntry entry = new CommunityPostEntry
            {
                postId = "player_post_" + playerPhoto.photoId,
                photographerHandle = "You (Local Player Capture)",
                regionTitle = playerPhoto.regionDisplayName,
                caption = string.IsNullOrEmpty(customCaption)
                    ? "Captured " + playerPhoto.primarySubjectTag + " (" + playerPhoto.compositionFeedback + ")"
                    : customCaption,
                cameraGearSummary = string.Format(
                    "{0} • {1}mm • f/{2:0.0} • ISO {3} • {4}",
                    playerPhoto.cameraProfileName,
                    playerPhoto.focalLengthMm,
                    playerPhoto.apertureFStop,
                    playerPhoto.isoSensitivity,
                    playerPhoto.shutterSpeedLabel),
                challengeTag = "Sacred Firelight & Nocturnes",
                likesCount = 1,
                likedByLocalPlayer = true,
                isSimulatedDemoEntry = false,
                playerPhotoReference = playerPhoto,
                demoCardPrimaryColor = new Color(0.14f, 0.12f, 0.10f),
                demoCardAccentColor = new Color(0.92f, 0.52f, 0.22f)
            };

            _posts.Insert(0, entry);
            return entry;
        }

        public void ToggleLikePost(string postId)
        {
            for (int i = 0; i < _posts.Count; i++)
            {
                if (_posts[i].postId == postId)
                {
                    _posts[i].likedByLocalPlayer = !_posts[i].likedByLocalPlayer;
                    _posts[i].likesCount += _posts[i].likedByLocalPlayer ? 1 : -1;
                    break;
                }
            }
        }
    }
}
