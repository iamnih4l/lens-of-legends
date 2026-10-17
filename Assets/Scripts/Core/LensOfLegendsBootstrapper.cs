using UnityEngine;
using UnityEngine.SceneManagement;
using LensOfLegends.Community;
using LensOfLegends.Gallery;
using LensOfLegends.Photography;
using LensOfLegends.Regions;
using LensOfLegends.Story;
using LensOfLegends.Travel;
using LensOfLegends.UI;

namespace LensOfLegends.Core
{
    /// <summary>
    /// Self-bootstrapping entry point using [RuntimeInitializeOnLoadMethod].
    /// Ensures that regardless of which scene a beginner opens in Unity (MainMenu, TravelMap,
    /// Theyyam_Ritual_Scene, or even a default blank SampleScene), pressing Play immediately
    /// initializes all game systems, constructs the 3D environment backdrop, and starts the
    /// appropriate state without requiring fragile manual Inspector wiring.
    /// </summary>
    public class LensOfLegendsBootstrapper : MonoBehaviour
    {
        private static bool _hasBootstrapped;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrapOnPlay()
        {
            if (_hasBootstrapped && GameStateManager.Instance != null)
                return;

            _hasBootstrapped = true;

            GameObject systemsRoot = new GameObject("LensOfLegends_CoreSystems");
            DontDestroyOnLoad(systemsRoot);

            if (GameStateManager.Instance == null)
                systemsRoot.AddComponent<GameStateManager>();
            if (ProceduralSoundscapeGenerator.Instance == null)
                systemsRoot.AddComponent<ProceduralSoundscapeGenerator>();
            if (CinematicDirector.Instance == null)
                systemsRoot.AddComponent<CinematicDirector>();
            if (TravelMapController.Instance == null)
                systemsRoot.AddComponent<TravelMapController>();
            if (CulturalDiscoveryManager.Instance == null)
                systemsRoot.AddComponent<CulturalDiscoveryManager>();
            if (StoryAssignmentSystem.Instance == null)
                systemsRoot.AddComponent<StoryAssignmentSystem>();
            if (PhotoJournalManager.Instance == null)
                systemsRoot.AddComponent<PhotoJournalManager>();
            if (PhotographySystem.Instance == null)
                systemsRoot.AddComponent<PhotographySystem>();
            if (CommunityPhotographyManager.Instance == null)
                systemsRoot.AddComponent<CommunityPhotographyManager>();
            if (LensOfLegendsUIManager.Instance == null)
                systemsRoot.AddComponent<LensOfLegendsUIManager>();

            // Build the 3D North Kerala sacred grove backdrop immediately so Main Menu, Travel Map,
            // and Direct Play in Theyyam_Ritual_Scene all have a rich real-time 3D scene
            RegionData kerala = IndiaDestinationCatalog.GetAllDestinations()[0];
            GameStateManager.Instance.EnsurePlayableEnvironmentBuilt(kerala);
            ProceduralSoundscapeGenerator.Instance.ApplyRegionSoundscape(kerala);

            string activeSceneName = SceneManager.GetActiveScene().name;
            if (activeSceneName == "TravelMap")
            {
                GameStateManager.Instance.SetFlowState(GameFlowState.TravelMap);
            }
            else if (activeSceneName == "Theyyam_Ritual_Scene")
            {
                GameStateManager.Instance.SetFlowState(GameFlowState.RegionalIntroduction);
            }
            else
            {
                GameStateManager.Instance.SetFlowState(GameFlowState.MainMenu);
            }
        }
    }
}
