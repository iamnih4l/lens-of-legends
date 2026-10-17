using System;
using System.Collections.Generic;
using UnityEngine;
using LensOfLegends.Core;
using LensOfLegends.Gallery;

namespace LensOfLegends.Story
{
    [Serializable]
    public class StoryAssignmentDefinition
    {
        public string assignmentId;
        public string regionId;
        public string title;
        public string storytellerName;
        [TextArea(2, 4)]
        public string narrativePrompt;
        public string targetPoiId; // Optional: if empty, any creative capture in region satisfies or contributes
        public bool requireSlowShutter;
        public bool requireWideAngle;
        public string rewardBadgeName;
        public int rewardXp = 100;
    }

    /// <summary>
    /// Lightweight, non-restrictive Story & Optional Assignment system.
    /// Provides narrative context from local mentors (such as Madhavan Nair in North Kerala)
    /// while never restricting what or when the player can photograph.
    /// </summary>
    public class StoryAssignmentSystem : MonoBehaviour
    {
        public static StoryAssignmentSystem Instance { get; private set; }

        public List<StoryAssignmentDefinition> RegionalAssignments { get; private set; } = new List<StoryAssignmentDefinition>();

        public event Action<StoryAssignmentDefinition> OnAssignmentCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildDefaultAssignments();
        }

        private void BuildDefaultAssignments()
        {
            RegionalAssignments = new List<StoryAssignmentDefinition>
            {
                new StoryAssignmentDefinition
                {
                    assignmentId = "assign_kerala_invocation",
                    regionId = "kerala_theyyam",
                    title = "Optional Inspiration I: Face of the Living Ritual",
                    storytellerName = "Madhavan Nair (Local Archivist, Fictional Mentor)",
                    narrativePrompt =
                        "\"In North Malabar, a photograph of Theyyam is not a trophy—it is a witness to centuries of devotion. Frame the performer near the central fire when you feel ready.\"",
                    targetPoiId = "poi_theyyam_performer",
                    requireSlowShutter = false,
                    requireWideAngle = false,
                    rewardBadgeName = "Witness of the Kavu",
                    rewardXp = 150
                },
                new StoryAssignmentDefinition
                {
                    assignmentId = "assign_kerala_fire_trails",
                    regionId = "kerala_theyyam",
                    title = "Optional Inspiration II: Rhythms in Ember & Motion",
                    storytellerName = "Madhavan Nair (Local Archivist, Fictional Mentor)",
                    narrativePrompt =
                        "\"Try experimenting with a slower shutter speed (1/60s or 1/30s using the [O] key) to capture the warmth of the Meleri fire or the motion of the dance.\"",
                    targetPoiId = "",
                    requireSlowShutter = true,
                    requireWideAngle = false,
                    rewardBadgeName = "Painter of Firelight",
                    rewardXp = 200
                },
                new StoryAssignmentDefinition
                {
                    assignmentId = "assign_kerala_architecture",
                    regionId = "kerala_theyyam",
                    title = "Optional Inspiration III: Sanctuary of Laterite & Banyan",
                    storytellerName = "Madhavan Nair (Local Archivist, Fictional Mentor)",
                    narrativePrompt =
                        "\"Document the Kavu shrine architecture or the ancient banyan grove that shelters this community tradition.\"",
                    targetPoiId = "poi_kavu_shrine",
                    requireSlowShutter = false,
                    requireWideAngle = false,
                    rewardBadgeName = "Keeper of Sacred Groves",
                    rewardXp = 150
                }
            };
        }

        public void EvaluatePhotoAgainstAssignments(PhotoMetadata photo)
        {
            if (photo == null || GameStateManager.Instance == null) return;

            for (int i = 0; i < RegionalAssignments.Count; i++)
            {
                StoryAssignmentDefinition assign = RegionalAssignments[i];
                if (GameStateManager.Instance.IsAssignmentCompleted(assign.assignmentId))
                    continue;

                if (assign.regionId != photo.regionId)
                    continue;

                bool matchesPoi = string.IsNullOrEmpty(assign.targetPoiId) ||
                                  photo.culturalPoiTag == assign.targetPoiId ||
                                  (assign.targetPoiId == "poi_kavu_shrine" && photo.culturalPoiTag == "poi_sacred_banyan");

                bool matchesShutter = !assign.requireSlowShutter ||
                                      photo.shutterSpeedLabel == "1/30s" ||
                                      photo.shutterSpeedLabel == "1/60s";

                if (matchesPoi && matchesShutter)
                {
                    GameStateManager.Instance.RecordCompletedAssignment(
                        assign.assignmentId,
                        assign.rewardBadgeName,
                        assign.rewardXp);
                    OnAssignmentCompleted?.Invoke(assign);
                }
            }
        }
    }
}
