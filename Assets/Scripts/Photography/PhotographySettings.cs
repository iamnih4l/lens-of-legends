using System;
using UnityEngine;

namespace LensOfLegends.Photography
{
    public enum WhiteBalanceColorProfile
    {
        NaturalNight_4200K = 0,
        MalabarFirelightWarm_3200K = 1,
        MonsoonTwilightCool_6500K = 2,
        KodachromeHeritage = 3,
        SilverGelatinMonochrome = 4
    }

    /// <summary>
    /// Holds live camera settings controlled by the player in Photography Mode.
    /// Every setting directly influences the live viewfinder and captured image output.
    /// Note: Depth-of-field, ISO grain, and slow-shutter motion trails are implemented via
    /// deterministic render/pixel processing so they work reliably across any Unity setup.
    /// </summary>
    [Serializable]
    public class PhotographySettings
    {
        public float focalLengthMm = 35f;
        public float focusDistanceMeters = 6.0f;
        public bool autoFocusEnabled = true;
        public float apertureFStop = 2.8f;
        public float exposureCompensationEv = 0.0f;
        public int isoSensitivity = 800;
        public float shutterSpeedDenominator = 125f; // e.g., 125 means 1/125s
        public WhiteBalanceColorProfile colorProfile = WhiteBalanceColorProfile.NaturalNight_4200K;
        public bool showRuleOfThirdsGrid = true;

        public static readonly int[] SupportedIsoValues = { 100, 200, 400, 800, 1600, 3200, 6400 };
        public static readonly float[] SupportedShutterDenominators = { 30f, 60f, 125f, 250f, 500f, 1000f };
        public static readonly float[] SupportedApertures = { 1.4f, 2.0f, 2.8f, 4.0f, 5.6f, 8.0f, 11.0f, 16.0f };

        public float CalculateVerticalFieldOfView()
        {
            // 24mm vertical full-frame sensor height: FOV = 2 * atan(12 / focalLength)
            float clampedFocal = Mathf.Clamp(focalLengthMm, 14f, 250f);
            float fovRad = 2.0f * Mathf.Atan(12.0f / clampedFocal);
            return Mathf.Clamp(fovRad * Mathf.Rad2Deg, 8f, 90f);
        }

        public float CalculateExposureMultiplier()
        {
            // Combines Exposure Compensation EV, ISO gain relative to ISO 800 baseline,
            // and shutter/aperture interplay in an intuitive way for players
            float evFactor = Mathf.Pow(2f, exposureCompensationEv * 0.65f);
            float isoFactor = Mathf.Pow(isoSensitivity / 800f, 0.35f);
            float shutterFactor = Mathf.Pow(125f / Mathf.Max(15f, shutterSpeedDenominator), 0.18f);
            return Mathf.Clamp(evFactor * isoFactor * shutterFactor, 0.25f, 3.2f);
        }

        public float CalculateSimulatedCircleOfConfusion(float subjectDistanceMeters, float profileDofMultiplier)
        {
            float distanceDelta = Mathf.Abs(subjectDistanceMeters - focusDistanceMeters);
            float normalizedDelta = distanceDelta / Mathf.Max(1.5f, focusDistanceMeters);
            float apertureFactor = 5.6f / Mathf.Max(1.2f, apertureFStop);
            float focalFactor = Mathf.Clamp(focalLengthMm / 50f, 0.5f, 2.5f);
            return Mathf.Clamp01(normalizedDelta * apertureFactor * focalFactor * profileDofMultiplier * 0.45f);
        }

        public Color GetWhiteBalanceMultiplier(out float saturationScale, out bool isMonochrome)
        {
            saturationScale = 1.0f;
            isMonochrome = false;
            switch (colorProfile)
            {
                case WhiteBalanceColorProfile.MalabarFirelightWarm_3200K:
                    saturationScale = 1.15f;
                    return new Color(1.14f, 0.96f, 0.80f);
                case WhiteBalanceColorProfile.MonsoonTwilightCool_6500K:
                    saturationScale = 0.95f;
                    return new Color(0.84f, 0.96f, 1.16f);
                case WhiteBalanceColorProfile.KodachromeHeritage:
                    saturationScale = 1.22f;
                    return new Color(1.08f, 0.98f, 0.88f);
                case WhiteBalanceColorProfile.SilverGelatinMonochrome:
                    saturationScale = 0.0f;
                    isMonochrome = true;
                    return new Color(1.02f, 1.0f, 0.96f);
                case WhiteBalanceColorProfile.NaturalNight_4200K:
                default:
                    saturationScale = 1.04f;
                    return Color.white;
            }
        }

        public string GetFormattedShutterSpeed()
        {
            return "1/" + Mathf.RoundToInt(shutterSpeedDenominator) + "s";
        }

        public string GetColorProfileDisplayName()
        {
            switch (colorProfile)
            {
                case WhiteBalanceColorProfile.MalabarFirelightWarm_3200K: return "Firelight Warm (3200K)";
                case WhiteBalanceColorProfile.MonsoonTwilightCool_6500K: return "Monsoon Cool (6500K)";
                case WhiteBalanceColorProfile.KodachromeHeritage: return "Heritage Slide Film";
                case WhiteBalanceColorProfile.SilverGelatinMonochrome: return "Silver Gelatin B&W";
                default: return "Natural Night (4200K)";
            }
        }
    }
}
