using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using LensOfLegends.CameraEquipment;
using LensOfLegends.Gallery;
using LensOfLegends.Photography;
using LensOfLegends.Player;
using LensOfLegends.Regions;
using LensOfLegends.Travel;

namespace LensOfLegends.Core
{
    public enum GameFlowState
    {
        MainMenu = 0,
        IntroductoryCinematic = 1,
        TravelMap = 2,
        RegionalIntroduction = 3,
        FreeExploration = 4,
        PhotoPreviewModal = 5,
        PhotoJournalScreen = 6,
        CommunityDemoScreen = 7,
        SettingsModal = 8
    }

    /// <summary>
    /// Coordinates the overall game flow, scene transitions, save state, progression unlocks,
    /// and runtime environment lifecycle.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameFlowState CurrentState { get; private set; } = GameFlowState.MainMenu;
        public GameFlowState PreviousReturnState { get; private set; } = GameFlowState.MainMenu;
        public RegionData CurrentRegion { get; private set; }
        public PlayerSaveStateData SaveData { get; private set; }

        public string ActiveToastMessage { get; private set; } = string.Empty;
        private float _toastTimer = 0f;

        public bool IsInPlayableExploration =>
            CurrentState == GameFlowState.FreeExploration;

        public bool IsModalOverlayOpen =>
            CurrentState != GameFlowState.FreeExploration;

        public event Action<GameFlowState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SaveData = SaveSystem.LoadOrCreateNew();
            CurrentRegion = IndiaDestinationCatalog.GetDestinationById(SaveData.lastVisitedRegionId);
        }

        private void Update()
        {
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.unscaledDeltaTime;
                if (_toastTimer <= 0f)
                    ActiveToastMessage = string.Empty;
            }

            // Global navigation shortcuts while exploring
            if (CurrentState == GameFlowState.FreeExploration)
            {
                if (Input.GetKeyDown(KeyCode.J))
                {
                    OpenPhotoJournal(GameFlowState.FreeExploration);
                }
                else if (Input.GetKeyDown(KeyCode.M))
                {
                    ReturnToTravelMap();
                }
                else if (Input.GetKeyDown(KeyCode.K))
                {
                    OpenCommunityScreen(GameFlowState.FreeExploration);
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (PhotographySystem.Instance != null && PhotographySystem.Instance.IsInCameraMode)
                        PhotographySystem.Instance.SetCameraMode(false);
                    else
                        OpenSettings(GameFlowState.FreeExploration);
                }
            }
            else if (CurrentState == GameFlowState.PhotoPreviewModal ||
                     CurrentState == GameFlowState.PhotoJournalScreen ||
                     CurrentState == GameFlowState.CommunityDemoScreen ||
                     CurrentState == GameFlowState.SettingsModal)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseOverlayToPreviousState();
                }
            }
        }

        public void SetFlowState(GameFlowState nextState)
        {
            CurrentState = nextState;
            OnStateChanged?.Invoke(CurrentState);
        }

        public void StartNewJourneyWithIntro()
        {
            CurrentRegion = IndiaDestinationCatalog.GetAllDestinations()[0];
            EnsurePlayableEnvironmentBuilt(CurrentRegion);

            SetFlowState(GameFlowState.IntroductoryCinematic);
            if (CinematicDirector.Instance != null)
            {
                CinematicDirector.Instance.PlayIntroductoryJourneyCinematic(() =>
                {
                    SaveData.hasSeenIntroCinematic = true;
                    SaveProgress();
                    OpenTravelMap();
                });
            }
            else
            {
                OpenTravelMap();
            }
        }

        public void ContinueSavedJourney()
        {
            RegionData target = IndiaDestinationCatalog.GetDestinationById(SaveData.lastVisitedRegionId);
            if (target == null || target.playableState != DestinationPlayableState.PlayableVerticalSlice)
                target = IndiaDestinationCatalog.GetAllDestinations()[0];

            BeginRegionalJourney(target);
        }

        public void OpenTravelMap()
        {
            if (PhotographySystem.Instance != null)
                PhotographySystem.Instance.SetCameraMode(false);

            TryLoadSceneIfRegistered("TravelMap");
            if (ProceduralSoundscapeGenerator.Instance != null && TravelMapController.Instance != null)
            {
                ProceduralSoundscapeGenerator.Instance.ApplyRegionSoundscape(TravelMapController.Instance.SelectedDestination);
            }
            SetFlowState(GameFlowState.TravelMap);
        }

        public void ReturnToMainMenu()
        {
            if (PhotographySystem.Instance != null)
                PhotographySystem.Instance.SetCameraMode(false);

            TryLoadSceneIfRegistered("MainMenu");
            SetFlowState(GameFlowState.MainMenu);
        }

        public void ReturnToTravelMap()
        {
            OpenTravelMap();
        }

        public void BeginRegionalJourney(RegionData region)
        {
            if (region == null) return;
            CurrentRegion = region;
            SaveData.lastVisitedRegionId = region.regionId;
            SaveProgress();

            TryLoadSceneIfRegistered(region.targetSceneName);
            EnsurePlayableEnvironmentBuilt(region);

            if (ProceduralSoundscapeGenerator.Instance != null)
                ProceduralSoundscapeGenerator.Instance.ApplyRegionSoundscape(region);

            SetFlowState(GameFlowState.RegionalIntroduction);
        }

        public void EnterFreeExplorationFromRegionalIntro(bool playArrivalCinematic)
        {
            EnsurePlayableEnvironmentBuilt(CurrentRegion);

            Vector3 spawnPos = new Vector3(0f, 1.0f, -10.5f);
            if (FirstPersonExplorerController.Instance != null)
            {
                FirstPersonExplorerController.Instance.TeleportToSpawn(spawnPos, 0f);
            }

            if (playArrivalCinematic && CinematicDirector.Instance != null)
            {
                SetFlowState(GameFlowState.IntroductoryCinematic);
                CinematicDirector.Instance.PlayRegionalArrivalCinematic(
                    spawnPos + new Vector3(0f, 0.72f, 0f),
                    Quaternion.identity,
                    () => SetFlowState(GameFlowState.FreeExploration));
            }
            else
            {
                SetFlowState(GameFlowState.FreeExploration);
            }
        }

        public void OpenPhotoPreviewModal()
        {
            PreviousReturnState = GameFlowState.FreeExploration;
            SetFlowState(GameFlowState.PhotoPreviewModal);
        }

        public void OpenPhotoJournal(GameFlowState returnState)
        {
            PreviousReturnState = returnState;
            SetFlowState(GameFlowState.PhotoJournalScreen);
        }

        public void OpenCommunityScreen(GameFlowState returnState)
        {
            PreviousReturnState = returnState;
            SetFlowState(GameFlowState.CommunityDemoScreen);
        }

        public void OpenSettings(GameFlowState returnState)
        {
            PreviousReturnState = returnState;
            SetFlowState(GameFlowState.SettingsModal);
        }

        public void CloseOverlayToPreviousState()
        {
            if (PreviousReturnState == GameFlowState.FreeExploration)
                SetFlowState(GameFlowState.FreeExploration);
            else if (PreviousReturnState == GameFlowState.TravelMap)
                SetFlowState(GameFlowState.TravelMap);
            else
                SetFlowState(GameFlowState.MainMenu);
        }

        public void EnsurePlayableEnvironmentBuilt(RegionData region)
        {
            if (region == null)
                region = IndiaDestinationCatalog.GetAllDestinations()[0];

            if (NorthKeralaTheyyamEnvironmentBuilder.Instance == null)
            {
                GameObject envBuilderObj = new GameObject("NorthKeralaTheyyamEnvironmentBuilder");
                DontDestroyOnLoad(envBuilderObj);
                envBuilderObj.AddComponent<NorthKeralaTheyyamEnvironmentBuilder>();
            }

            NorthKeralaTheyyamEnvironmentBuilder.Instance.BuildEnvironment(region);

            if (FirstPersonExplorerController.Instance == null)
            {
                GameObject playerObj = new GameObject("PlayerExplorer_FirstPerson");
                DontDestroyOnLoad(playerObj);
                playerObj.AddComponent<CharacterController>();
                FirstPersonExplorerController ctrl = playerObj.AddComponent<FirstPersonExplorerController>();
                ctrl.TeleportToSpawn(new Vector3(0f, 1.0f, -10.5f), 0f);
            }
        }

        private void TryLoadSceneIfRegistered(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            Scene active = SceneManager.GetActiveScene();
            if (active.name == sceneName) return;

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        public void RecordCapturedPhoto(PhotoMetadata meta)
        {
            if (meta == null) return;
            SaveData.capturedPhotos.Insert(0, meta);
            SaveData.photographerXp += 50;

            // Check camera equipment unlock milestone (3 captures unlocks Kala-Rasa 6x6 Medium Format)
            if (SaveData.capturedPhotos.Count >= 3 && !SaveData.unlockedCameraProfileIds.Contains("cam_heritage_mf"))
            {
                SaveData.unlockedCameraProfileIds.Add("cam_heritage_mf");
                if (!SaveData.earnedBadges.Contains("Milestone: Kala-Rasa 6x6 Medium Format Unlocked"))
                    SaveData.earnedBadges.Add("Milestone: Kala-Rasa 6x6 Medium Format Unlocked");
                ShowToastNotification("Equipment Unlocked! You earned the Kala-Rasa 6x6 Medium Format Camera (Press [4] in Camera Mode).");
            }

            SaveProgress();
        }

        public bool RecordDiscoveredPoi(string poiId)
        {
            if (string.IsNullOrEmpty(poiId)) return false;
            if (SaveData.discoveredPoiIds.Contains(poiId)) return false;

            SaveData.discoveredPoiIds.Add(poiId);
            SaveData.photographerXp += 75;
            if (SaveData.discoveredPoiIds.Count >= 3 && !SaveData.earnedBadges.Contains("Malabar Cultural Chronicler"))
            {
                SaveData.earnedBadges.Add("Malabar Cultural Chronicler");
            }
            SaveProgress();
            return true;
        }

        public bool IsPoiDiscovered(string poiId)
        {
            return SaveData != null && SaveData.discoveredPoiIds.Contains(poiId);
        }

        public bool IsAssignmentCompleted(string assignmentId)
        {
            return SaveData != null && SaveData.completedAssignmentIds.Contains(assignmentId);
        }

        public bool IsCameraProfileUnlocked(string profileId)
        {
            return SaveData != null && SaveData.unlockedCameraProfileIds.Contains(profileId);
        }

        public void RecordCompletedAssignment(string assignmentId, string badgeName, int xp)
        {
            if (!SaveData.completedAssignmentIds.Contains(assignmentId))
                SaveData.completedAssignmentIds.Add(assignmentId);
            if (!string.IsNullOrEmpty(badgeName) && !SaveData.earnedBadges.Contains(badgeName))
                SaveData.earnedBadges.Add(badgeName);

            SaveData.photographerXp += xp;
            SaveProgress();
            ShowToastNotification("Story Assignment Completed! Earned Badge: " + badgeName + " (+" + xp + " XP)");
        }

        public void RecordCommunityPublishReward()
        {
            SaveData.photographerXp += 50;
            if (!SaveData.earnedBadges.Contains("Community Storyteller"))
                SaveData.earnedBadges.Add("Community Storyteller");
            SaveProgress();
        }

        public void SaveProgress()
        {
            SaveSystem.Save(SaveData);
        }

        public void ShowToastNotification(string message, float duration = 5.0f)
        {
            ActiveToastMessage = message;
            _toastTimer = duration;
        }
    }
}
