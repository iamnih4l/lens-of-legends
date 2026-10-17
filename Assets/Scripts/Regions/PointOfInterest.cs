using UnityEngine;

namespace LensOfLegends.Regions
{
    /// <summary>
    /// Attached to discoverable cultural Points of Interest in a playable region.
    /// Stores both verified cultural facts and clearly labeled narrative storyteller prompts.
    /// </summary>
    public class PointOfInterest : MonoBehaviour
    {
        public CulturalPointOfInterestDefinition Definition;
        public bool IsDiscovered { get; private set; }

        private Transform _beaconVisual;

        public void Initialize(CulturalPointOfInterestDefinition def, bool alreadyDiscovered, Transform beaconVisual)
        {
            Definition = def;
            IsDiscovered = alreadyDiscovered;
            _beaconVisual = beaconVisual;
        }

        public void MarkDiscovered()
        {
            IsDiscovered = true;
        }

        private void Update()
        {
            if (_beaconVisual != null)
            {
                float bob = Mathf.Sin(Time.time * 2.2f) * 0.12f;
                _beaconVisual.localPosition = new Vector3(0f, 2.4f + bob, 0f);
                _beaconVisual.Rotate(Vector3.up, 35f * Time.deltaTime, Space.Self);
            }
        }
    }
}
