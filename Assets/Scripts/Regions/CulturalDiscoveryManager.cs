using System;
using System.Collections.Generic;
using UnityEngine;
using LensOfLegends.Core;

namespace LensOfLegends.Regions
{
    /// <summary>
    /// Tracks cultural Points of Interest in the active region, detects proximity or player inspection,
    /// and notifies the journal and UI when new cultural entries are discovered.
    /// Strictly distinguishes verified cultural facts from narrative storyteller notes.
    /// </summary>
    public class CulturalDiscoveryManager : MonoBehaviour
    {
        public static CulturalDiscoveryManager Instance { get; private set; }

        private readonly List<PointOfInterest> _activePois = new List<PointOfInterest>();
        public PointOfInterest NearestInspectablePoi { get; private set; }
        public CulturalPointOfInterestDefinition LastDiscoveredPoi { get; private set; }

        public event Action<CulturalPointOfInterestDefinition> OnPoiDiscovered;

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

        public void RegisterPoi(PointOfInterest poi)
        {
            if (poi != null && !_activePois.Contains(poi))
                _activePois.Add(poi);
        }

        public void ClearRegionalPois()
        {
            _activePois.Clear();
            NearestInspectablePoi = null;
        }

        private void Update()
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsInPlayableExploration)
                return;

            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 playerPos = cam.transform.position;
            PointOfInterest closest = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < _activePois.Count; i++)
            {
                PointOfInterest poi = _activePois[i];
                if (poi == null || poi.Definition == null) continue;

                float dist = Vector3.Distance(playerPos, poi.transform.position);
                if (dist <= poi.Definition.discoveryRadius && dist < bestDist)
                {
                    bestDist = dist;
                    closest = poi;
                }

                // Automatically discover when walking within close range (4.8m)
                if (!poi.IsDiscovered && dist <= 4.8f)
                {
                    UnlockPoi(poi);
                }
            }

            NearestInspectablePoi = closest;

            // Allow manual inspection with E key when near any POI
            if (NearestInspectablePoi != null && Input.GetKeyDown(KeyCode.E) && !GameStateManager.Instance.IsModalOverlayOpen)
            {
                UnlockPoi(NearestInspectablePoi, forceShowBanner: true);
            }
        }

        public void DiscoverPoiById(string poiId)
        {
            if (string.IsNullOrEmpty(poiId)) return;
            for (int i = 0; i < _activePois.Count; i++)
            {
                if (_activePois[i] != null && _activePois[i].Definition != null && _activePois[i].Definition.poiId == poiId)
                {
                    UnlockPoi(_activePois[i]);
                    return;
                }
            }
        }

        private void UnlockPoi(PointOfInterest poi, bool forceShowBanner = false)
        {
            if (poi == null || poi.Definition == null) return;
            bool wasAlreadyDiscovered = poi.IsDiscovered;
            poi.MarkDiscovered();
            LastDiscoveredPoi = poi.Definition;

            if (GameStateManager.Instance != null)
            {
                bool newlyAdded = GameStateManager.Instance.RecordDiscoveredPoi(poi.Definition.poiId);
                if (newlyAdded || forceShowBanner || !wasAlreadyDiscovered)
                {
                    OnPoiDiscovered?.Invoke(poi.Definition);
                }
            }
        }
    }
}
