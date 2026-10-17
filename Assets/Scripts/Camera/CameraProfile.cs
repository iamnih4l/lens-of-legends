using System;
using System.Collections.Generic;
using UnityEngine;

namespace LensOfLegends.CameraEquipment
{
    public enum SensorFormatStyle
    {
        CompactAPS_C = 0,
        FullFrameMirrorless = 1,
        AnalogRangefinder35mm = 2,
        MediumFormatHeritage = 3
    }

    /// <summary>
    /// Defines an extensible camera equipment profile with genuinely distinct optical,
    /// framing, and image-processing characteristics.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCameraProfile", menuName = "Lens of Legends/Camera Profile", order = 2)]
    public class CameraProfile : ScriptableObject
    {
        public string profileId;
        public string cameraName;
        public string subtitle;
        public SensorFormatStyle sensorStyle;
        public bool unlockedByDefault = true;
        public int requiredPhotosToUnlock = 0;

        [Header("Optical Range")]
        public float minFocalLengthMm = 28f;
        public float maxFocalLengthMm = 70f;
        public float defaultFocalLengthMm = 35f;
        public float minApertureFStop = 2.8f;
        public float maxApertureFStop = 16.0f;

        [Header("Image & Viewfinder Characteristics")]
        [Range(0f, 2f)] public float depthOfFieldStrengthMultiplier = 0.8f;
        [Range(0.7f, 1.4f)] public float contrastMultiplier = 1.0f;
        [Range(0f, 1.5f)] public float saturationMultiplier = 1.0f;
        [Range(0f, 0.6f)] public float naturalVignetteIntensity = 0.15f;
        [Range(0f, 0.35f)] public float baseFilmGrain = 0.02f;
        public Color colorTintBias = Color.white;
        public bool includeAnalogFilmBorder = false;
        public bool supportsTelemetryHistogram = false;

        [TextArea(2, 4)]
        public string equipmentDescription;

        private static List<CameraProfile> _defaultProfiles;

        public static List<CameraProfile> GetDefaultProfiles()
        {
            if (_defaultProfiles != null && _defaultProfiles.Count > 0 && _defaultProfiles[0] != null)
                return _defaultProfiles;

            _defaultProfiles = new List<CameraProfile>();

            // 1. Basic Travel Camera
            CameraProfile basic = CreateInstance<CameraProfile>();
            basic.name = "CameraProfile_01_BasicTravel";
            basic.profileId = "cam_basic_travel";
            basic.cameraName = "Yatra Compact DX-35";
            basic.subtitle = "Lightweight Everyday Travel Camera (28–70mm f/2.8–f/16)";
            basic.sensorStyle = SensorFormatStyle.CompactAPS_C;
            basic.unlockedByDefault = true;
            basic.requiredPhotosToUnlock = 0;
            basic.minFocalLengthMm = 28f;
            basic.maxFocalLengthMm = 70f;
            basic.defaultFocalLengthMm = 35f;
            basic.minApertureFStop = 2.8f;
            basic.maxApertureFStop = 16f;
            basic.depthOfFieldStrengthMultiplier = 0.75f;
            basic.contrastMultiplier = 1.04f;
            basic.saturationMultiplier = 1.05f;
            basic.naturalVignetteIntensity = 0.12f;
            basic.baseFilmGrain = 0.01f;
            basic.colorTintBias = new Color(1.0f, 1.0f, 1.0f);
            basic.includeAnalogFilmBorder = false;
            basic.supportsTelemetryHistogram = false;
            basic.equipmentDescription =
                "Reliable standard zoom travel camera with balanced color reproduction and responsive framing for street and architectural discovery.";
            _defaultProfiles.Add(basic);

            // 2. Professional Mirrorless Camera
            CameraProfile mirrorless = CreateInstance<CameraProfile>();
            mirrorless.name = "CameraProfile_02_ProMirrorless";
            mirrorless.profileId = "cam_pro_mirrorless";
            mirrorless.cameraName = "Drishti Pro M-IV Full-Frame";
            mirrorless.subtitle = "Professional Mirrorless System (18–200mm f/1.4–f/16)";
            mirrorless.sensorStyle = SensorFormatStyle.FullFrameMirrorless;
            mirrorless.unlockedByDefault = true;
            mirrorless.requiredPhotosToUnlock = 0;
            mirrorless.minFocalLengthMm = 18f;
            mirrorless.maxFocalLengthMm = 200f;
            mirrorless.defaultFocalLengthMm = 50f;
            mirrorless.minApertureFStop = 1.4f;
            mirrorless.maxApertureFStop = 16f;
            mirrorless.depthOfFieldStrengthMultiplier = 1.35f;
            mirrorless.contrastMultiplier = 1.08f;
            mirrorless.saturationMultiplier = 1.08f;
            mirrorless.naturalVignetteIntensity = 0.08f;
            mirrorless.baseFilmGrain = 0.005f;
            mirrorless.colorTintBias = new Color(1.0f, 0.99f, 1.02f);
            mirrorless.includeAnalogFilmBorder = false;
            mirrorless.supportsTelemetryHistogram = true;
            mirrorless.equipmentDescription =
                "Full-frame sensor with ultra-wide 18mm to 200mm telephoto reach, wide f/1.4 aperture for strong subject isolation, and precision exposure telemetry.";
            _defaultProfiles.Add(mirrorless);

            // 3. Vintage Film Camera
            CameraProfile vintage = CreateInstance<CameraProfile>();
            vintage.name = "CameraProfile_03_VintageFilm";
            vintage.profileId = "cam_vintage_film";
            vintage.cameraName = "Chhaya-76 Mechanical Rangefinder";
            vintage.subtitle = "1976 Brass 35mm Analog Film Camera (35–85mm f/2.0–f/11)";
            vintage.sensorStyle = SensorFormatStyle.AnalogRangefinder35mm;
            vintage.unlockedByDefault = true;
            vintage.requiredPhotosToUnlock = 0;
            vintage.minFocalLengthMm = 35f;
            vintage.maxFocalLengthMm = 85f;
            vintage.defaultFocalLengthMm = 45f;
            vintage.minApertureFStop = 2.0f;
            vintage.maxApertureFStop = 11f;
            vintage.depthOfFieldStrengthMultiplier = 1.05f;
            vintage.contrastMultiplier = 1.15f;
            vintage.saturationMultiplier = 0.92f;
            vintage.naturalVignetteIntensity = 0.34f;
            vintage.baseFilmGrain = 0.09f;
            vintage.colorTintBias = new Color(1.06f, 0.97f, 0.86f);
            vintage.includeAnalogFilmBorder = true;
            vintage.supportsTelemetryHistogram = false;
            vintage.equipmentDescription =
                "Warm amber halation, tactile silver-halide film grain, pronounced optical vignette, and a stamped darkroom negative border.";
            _defaultProfiles.Add(vintage);

            // 4. Unlockable Heritage Medium Format Camera (Progression Reward)
            CameraProfile heritage = CreateInstance<CameraProfile>();
            heritage.name = "CameraProfile_04_HeritageMediumFormat";
            heritage.profileId = "cam_heritage_mf";
            heritage.cameraName = "Kala-Rasa 6x6 Studio Twin-Lens";
            heritage.subtitle = "Unlockable Milestone Camera (Unlocked after 3 Journal Captures)";
            heritage.sensorStyle = SensorFormatStyle.MediumFormatHeritage;
            heritage.unlockedByDefault = false;
            heritage.requiredPhotosToUnlock = 3;
            heritage.minFocalLengthMm = 24f;
            heritage.maxFocalLengthMm = 120f;
            heritage.defaultFocalLengthMm = 65f;
            heritage.minApertureFStop = 1.8f;
            heritage.maxApertureFStop = 16f;
            heritage.depthOfFieldStrengthMultiplier = 1.65f;
            heritage.contrastMultiplier = 1.12f;
            heritage.saturationMultiplier = 1.14f;
            heritage.naturalVignetteIntensity = 0.22f;
            heritage.baseFilmGrain = 0.03f;
            heritage.colorTintBias = new Color(1.03f, 0.98f, 0.94f);
            heritage.includeAnalogFilmBorder = true;
            heritage.supportsTelemetryHistogram = true;
            heritage.equipmentDescription =
                "Rewarded to dedicated field photographers. Produces rich tonal roll-off, deep medium-format depth separation, and exhibition-style borders.";
            _defaultProfiles.Add(heritage);

            return _defaultProfiles;
        }
    }
}
