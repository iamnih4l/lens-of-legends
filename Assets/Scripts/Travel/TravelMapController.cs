using System;
using System.Collections.Generic;
using UnityEngine;
using LensOfLegends.Core;
using LensOfLegends.Regions;

namespace LensOfLegends.Travel
{
    /// <summary>
    /// Controls the India-wide Travel Map & Destination Selection system.
    /// Allows the player to inspect all 6 regional chapters across India, view their distinct
    /// cultural themes, palettes, and planned photography subjects, and launch playable regions
    /// while preventing unfinished preview chapters from opening broken or empty scenes.
    /// </summary>
    public class TravelMapController : MonoBehaviour
    {
        public static TravelMapController Instance { get; private set; }

        public List<RegionData> AllDestinations { get; private set; } = new List<RegionData>();
        public int SelectedDestinationIndex { get; private set; } = 0;

        public RegionData SelectedDestination =>
            (AllDestinations != null && AllDestinations.Count > 0)
                ? AllDestinations[Mathf.Clamp(SelectedDestinationIndex, 0, AllDestinations.Count - 1)]
                : null;

        public event Action<RegionData> OnDestinationSelected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            AllDestinations = IndiaDestinationCatalog.GetAllDestinations();
        }

        public void SelectDestinationByIndex(int index)
        {
            if (AllDestinations == null || AllDestinations.Count == 0) return;
            SelectedDestinationIndex = Mathf.Clamp(index, 0, AllDestinations.Count - 1);
            OnDestinationSelected?.Invoke(SelectedDestination);
        }

        public void SelectDestinationById(string regionId)
        {
            if (AllDestinations == null) return;
            for (int i = 0; i < AllDestinations.Count; i++)
            {
                if (AllDestinations[i].regionId == regionId)
                {
                    SelectDestinationByIndex(i);
                    return;
                }
            }
        }

        public void TravelToSelectedDestination()
        {
            RegionData target = SelectedDestination;
            if (target == null || GameStateManager.Instance == null) return;

            if (target.playableState != DestinationPlayableState.PlayableVerticalSlice)
            {
                GameStateManager.Instance.ShowToastNotification(
                    "Preview Chapter: " + target.regionTitle + " is a structured future destination in the India-wide expansion roadmap. Select Chapter I (North Kerala — Theyyam) to enter the playable vertical slice.");
                return;
            }

            GameStateManager.Instance.BeginRegionalJourney(target);
        }
    }
}
