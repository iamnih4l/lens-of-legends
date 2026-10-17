using System;
using System.Collections.Generic;
using UnityEngine;
using LensOfLegends.CameraEquipment;
using LensOfLegends.Core;
using LensOfLegends.Gallery;
using LensOfLegends.Regions;
using LensOfLegends.Story;

namespace LensOfLegends.Photography
{
    public class PhotographyTargetMarker : MonoBehaviour
    {
        public string subjectId;
        public string displayName;
        public string category;
        public string associatedPoiId;
        public float optimalDistanceMeters = 6.5f;
        public Bounds worldBounds;
    }

    /// <summary>
    /// Primary gameplay system: Dedicated Camera Mode, optical & exposure settings,
    /// subject framing detection, RenderTexture capture, simulated optical post-processing,
    /// and integration with the persistent Photo Journal.
    /// </summary>
    public class PhotographySystem : MonoBehaviour
    {
        public static PhotographySystem Instance { get; private set; }

        public bool IsInCameraMode { get; private set; }
        public PhotographySettings Settings { get; private set; } = new PhotographySettings();
        public List<CameraProfile> AvailableProfiles { get; private set; } = new List<CameraProfile>();
        public int CurrentProfileIndex { get; private set; } = 0;

        public CameraProfile ActiveProfile =>
            (AvailableProfiles != null && AvailableProfiles.Count > 0)
                ? AvailableProfiles[Mathf.Clamp(CurrentProfileIndex, 0, AvailableProfiles.Count - 1)]
                : null;

        public string CurrentFramedSubjectName { get; private set; } = "Open Landscape / Scene Composition";
        public string CurrentFramedPoiId { get; private set; } = string.Empty;
        public float CurrentSubjectDistanceMeters { get; private set; } = 8.0f;
        public float CurrentCircleOfConfusion { get; private set; } = 0.05f;
        public bool IsSubjectInSharpFocus => CurrentCircleOfConfusion < 0.22f;
        public float ShutterFlashAlpha { get; private set; } = 0f;

        public Texture2D LastCapturedTexture { get; private set; }
        public PhotoMetadata LastCapturedMetadata { get; private set; }

        public event Action<bool> OnCameraModeChanged;
        public event Action<PhotoMetadata, Texture2D> OnPhotoCaptured;

        private Camera _mainCamera;
        private RenderTexture _rollingFrameA;
        private RenderTexture _rollingFrameB;
        private float _rollingBufferTimer;

        private int _isoIndex = 3; // 800
        private int _shutterIndex = 2; // 1/125s
        private int _apertureIndex = 2; // f/2.8

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            AvailableProfiles = CameraProfile.GetDefaultProfiles();
            ApplyCameraProfile(0);
        }

        private void Update()
        {
            if (ShutterFlashAlpha > 0f)
            {
                ShutterFlashAlpha = Mathf.MoveTowards(ShutterFlashAlpha, 0f, Time.unscaledDeltaTime * 3.2f);
            }

            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsInPlayableExploration)
            {
                if (IsInCameraMode)
                    SetCameraMode(false);
                return;
            }

            EnsureCameraReference();

            // Toggle Camera Mode with Right Mouse Button or V key (when no modal UI is blocking)
            if (!GameStateManager.Instance.IsModalOverlayOpen)
            {
                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.V))
                {
                    SetCameraMode(!IsInCameraMode);
                }
            }

            if (!IsInCameraMode)
            {
                if (_mainCamera != null)
                {
                    _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, 60f, Time.deltaTime * 10f);
                }
                return;
            }

            UpdateFramedSubjectDetection();
            HandleCameraModeInput();
            UpdateCameraOpticsLive();
            UpdateSlowShutterHistoryBuffer();
        }

        public void SetCameraMode(bool enabled)
        {
            if (IsInCameraMode == enabled) return;
            IsInCameraMode = enabled;

            EnsureCameraReference();
            if (IsInCameraMode && ActiveProfile != null)
            {
                Settings.focalLengthMm = Mathf.Clamp(
                    Settings.focalLengthMm,
                    ActiveProfile.minFocalLengthMm,
                    ActiveProfile.maxFocalLengthMm);
            }

            OnCameraModeChanged?.Invoke(IsInCameraMode);
        }

        public void ApplyCameraProfile(int index)
        {
            if (AvailableProfiles == null || AvailableProfiles.Count == 0) return;
            index = Mathf.Clamp(index, 0, AvailableProfiles.Count - 1);

            CameraProfile candidate = AvailableProfiles[index];
            bool unlocked = candidate.unlockedByDefault ||
                            (GameStateManager.Instance != null &&
                             GameStateManager.Instance.IsCameraProfileUnlocked(candidate.profileId));

            if (!unlocked)
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.ShowToastNotification(
                        "Camera Locked: Capture " + candidate.requiredPhotosToUnlock + " photographs in your Journal to unlock " + candidate.cameraName + ".");
                }
                return;
            }

            CurrentProfileIndex = index;
            Settings.focalLengthMm = Mathf.Clamp(
                candidate.defaultFocalLengthMm,
                candidate.minFocalLengthMm,
                candidate.maxFocalLengthMm);
            Settings.apertureFStop = Mathf.Clamp(
                Settings.apertureFStop,
                candidate.minApertureFStop,
                candidate.maxApertureFStop);
        }

        private void HandleCameraModeInput()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.IsModalOverlayOpen)
                return;

            CameraProfile profile = ActiveProfile;
            if (profile == null) return;

            // 1-4 keys switch Camera Equipment Profiles
            if (Input.GetKeyDown(KeyCode.Alpha1)) ApplyCameraProfile(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ApplyCameraProfile(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ApplyCameraProfile(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) ApplyCameraProfile(3);

            // Zoom / Focal Length: Mouse Scroll or Up/Down arrows
            float scroll = Input.mouseScrollDelta.y;
            if (Input.GetKey(KeyCode.UpArrow)) scroll += Time.deltaTime * 8f;
            if (Input.GetKey(KeyCode.DownArrow)) scroll -= Time.deltaTime * 8f;
            if (Mathf.Abs(scroll) > 0.001f)
            {
                Settings.focalLengthMm = Mathf.Clamp(
                    Settings.focalLengthMm + scroll * 6.0f,
                    profile.minFocalLengthMm,
                    profile.maxFocalLengthMm);
            }

            // Autofocus toggle (F) and Manual Focus distance ([ and ])
            if (Input.GetKeyDown(KeyCode.F))
            {
                Settings.autoFocusEnabled = !Settings.autoFocusEnabled;
            }
            if (Input.GetKey(KeyCode.LeftBracket))
            {
                Settings.autoFocusEnabled = false;
                Settings.focusDistanceMeters = Mathf.Clamp(Settings.focusDistanceMeters - Time.deltaTime * 6f, 1.0f, 50f);
            }
            if (Input.GetKey(KeyCode.RightBracket))
            {
                Settings.autoFocusEnabled = false;
                Settings.focusDistanceMeters = Mathf.Clamp(Settings.focusDistanceMeters + Time.deltaTime * 6f, 1.0f, 50f);
            }

            // Aperture (Q / E)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _apertureIndex = Mathf.Max(0, _apertureIndex - 1);
                Settings.apertureFStop = Mathf.Clamp(
                    PhotographySettings.SupportedApertures[_apertureIndex],
                    profile.minApertureFStop,
                    profile.maxApertureFStop);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                _apertureIndex = Mathf.Min(PhotographySettings.SupportedApertures.Length - 1, _apertureIndex + 1);
                Settings.apertureFStop = Mathf.Clamp(
                    PhotographySettings.SupportedApertures[_apertureIndex],
                    profile.minApertureFStop,
                    profile.maxApertureFStop);
            }

            // Exposure Compensation (- / =)
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                Settings.exposureCompensationEv = Mathf.Clamp(Settings.exposureCompensationEv - 0.3f, -2.0f, 2.0f);
            }
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                Settings.exposureCompensationEv = Mathf.Clamp(Settings.exposureCompensationEv + 0.3f, -2.0f, 2.0f);
            }

            // ISO cycle (I)
            if (Input.GetKeyDown(KeyCode.I))
            {
                _isoIndex = (_isoIndex + 1) % PhotographySettings.SupportedIsoValues.Length;
                Settings.isoSensitivity = PhotographySettings.SupportedIsoValues[_isoIndex];
            }

            // Shutter Speed cycle (O)
            if (Input.GetKeyDown(KeyCode.O))
            {
                _shutterIndex = (_shutterIndex + 1) % PhotographySettings.SupportedShutterDenominators.Length;
                Settings.shutterSpeedDenominator = PhotographySettings.SupportedShutterDenominators[_shutterIndex];
            }

            // White Balance / Color Profile cycle (C)
            if (Input.GetKeyDown(KeyCode.C))
            {
                int nextColor = ((int)Settings.colorProfile + 1) % 5;
                Settings.colorProfile = (WhiteBalanceColorProfile)nextColor;
            }

            // Rule of Thirds Grid toggle (G)
            if (Input.GetKeyDown(KeyCode.G))
            {
                Settings.showRuleOfThirdsGrid = !Settings.showRuleOfThirdsGrid;
            }

            // Capture Photograph: Left Mouse Button or Space or Return
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                CapturePhotographNow();
            }
        }

        private void UpdateFramedSubjectDetection()
        {
            EnsureCameraReference();
            if (_mainCamera == null) return;

            CurrentFramedSubjectName = "Malabar Sacred Grove Atmosphere";
            CurrentFramedPoiId = string.Empty;
            CurrentSubjectDistanceMeters = 10.0f;

            Ray centerRay = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(centerRay, out RaycastHit hit, 65f))
            {
                CurrentSubjectDistanceMeters = hit.distance;
                PhotographyTargetMarker marker = hit.collider.GetComponentInParent<PhotographyTargetMarker>();
                if (marker != null)
                {
                    CurrentFramedSubjectName = marker.displayName;
                    CurrentFramedPoiId = marker.associatedPoiId;
                }
            }
            else
            {
                // Check frustum for nearest PhotographyTargetMarker near center of screen
                PhotographyTargetMarker[] markers = FindObjectsOfType<PhotographyTargetMarker>();
                float bestScore = float.MaxValue;
                foreach (PhotographyTargetMarker m in markers)
                {
                    if (m == null) continue;
                    Vector3 vp = _mainCamera.WorldToViewportPoint(m.transform.position);
                    if (vp.z > 0.5f && vp.z < 55f && vp.x >= 0.15f && vp.x <= 0.85f && vp.y >= 0.15f && vp.y <= 0.85f)
                    {
                        float centerDist = Vector2.Distance(new Vector2(vp.x, vp.y), new Vector2(0.5f, 0.5f));
                        if (centerDist < bestScore)
                        {
                            bestScore = centerDist;
                            CurrentFramedSubjectName = m.displayName;
                            CurrentFramedPoiId = m.associatedPoiId;
                            CurrentSubjectDistanceMeters = vp.z;
                        }
                    }
                }
            }

            if (Settings.autoFocusEnabled)
            {
                Settings.focusDistanceMeters = Mathf.Lerp(
                    Settings.focusDistanceMeters,
                    Mathf.Clamp(CurrentSubjectDistanceMeters, 1.2f, 45f),
                    Time.deltaTime * 8f);
            }

            float profileDof = ActiveProfile != null ? ActiveProfile.depthOfFieldStrengthMultiplier : 1.0f;
            CurrentCircleOfConfusion = Settings.CalculateSimulatedCircleOfConfusion(CurrentSubjectDistanceMeters, profileDof);
        }

        private void UpdateCameraOpticsLive()
        {
            if (_mainCamera == null) return;
            float targetFov = Settings.CalculateVerticalFieldOfView();
            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFov, Time.deltaTime * 12f);
        }

        private void UpdateSlowShutterHistoryBuffer()
        {
            if (_mainCamera == null) return;
            _rollingBufferTimer += Time.deltaTime;
            if (_rollingBufferTimer < 0.08f) return;
            _rollingBufferTimer = 0f;

            const int width = 960;
            const int height = 540;
            if (_rollingFrameA == null)
            {
                _rollingFrameA = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                _rollingFrameB = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            }

            // Swap A and B and render current camera view into A for slow-shutter blending
            RenderTexture temp = _rollingFrameB;
            _rollingFrameB = _rollingFrameA;
            _rollingFrameA = temp;

            RenderTexture prevTarget = _mainCamera.targetTexture;
            _mainCamera.targetTexture = _rollingFrameA;
            _mainCamera.Render();
            _mainCamera.targetTexture = prevTarget;
        }

        public PhotoMetadata CapturePhotographNow()
        {
            EnsureCameraReference();
            if (_mainCamera == null) return null;

            const int captureWidth = 960;
            const int captureHeight = 540;

            RenderTexture rt = RenderTexture.GetTemporary(captureWidth, captureHeight, 24, RenderTextureFormat.ARGB32);
            RenderTexture prevCameraTarget = _mainCamera.targetTexture;
            RenderTexture prevActive = RenderTexture.active;

            // Render strictly the 3D scene camera without UI overlays
            _mainCamera.targetTexture = rt;
            _mainCamera.Render();
            _mainCamera.targetTexture = prevCameraTarget;

            RenderTexture.active = rt;
            Texture2D finalPhoto = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
            finalPhoto.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);

            // Optional slow-shutter frame blend when shutter speed <= 1/60s
            Color[] previousPixels = null;
            if (Settings.shutterSpeedDenominator <= 60f && _rollingFrameB != null)
            {
                RenderTexture.active = _rollingFrameB;
                Texture2D prevTex = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
                prevTex.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
                previousPixels = prevTex.GetPixels();
                Destroy(prevTex);
            }

            RenderTexture.active = prevActive;
            RenderTexture.ReleaseTemporary(rt);

            // Apply Camera Profile & Camera Settings optical/color simulation pipeline
            ApplyOpticalAndColorPipeline(finalPhoto, previousPixels, captureWidth, captureHeight);
            finalPhoto.Apply();

            ShutterFlashAlpha = 0.85f;
            if (ProceduralSoundscapeGenerator.Instance != null)
            {
                ProceduralSoundscapeGenerator.Instance.PlayMechanicalShutterClick();
            }

            // Evaluate composition quality & feedback
            float score = EvaluateCompositionScore(out string feedback);
            RegionData activeRegion = GameStateManager.Instance != null
                ? GameStateManager.Instance.CurrentRegion
                : IndiaDestinationCatalog.GetAllDestinations()[0];

            string camName = ActiveProfile != null ? ActiveProfile.cameraName : "Standard Travel Camera";
            PhotoMetadata meta = null;

            if (PhotoJournalManager.Instance != null)
            {
                meta = PhotoJournalManager.Instance.SaveNewCaptureToJournal(
                    finalPhoto,
                    activeRegion != null ? activeRegion.regionId : "kerala_theyyam",
                    activeRegion != null ? activeRegion.regionTitle : "North Kerala — Theyyam",
                    CurrentFramedSubjectName,
                    CurrentFramedPoiId,
                    camName,
                    Settings.focalLengthMm,
                    Settings.apertureFStop,
                    Settings.isoSensitivity,
                    Settings.GetFormattedShutterSpeed(),
                    Settings.GetColorProfileDisplayName(),
                    score,
                    feedback);
            }

            if (!string.IsNullOrEmpty(CurrentFramedPoiId) && CulturalDiscoveryManager.Instance != null)
            {
                CulturalDiscoveryManager.Instance.DiscoverPoiById(CurrentFramedPoiId);
            }

            if (StoryAssignmentSystem.Instance != null && meta != null)
            {
                StoryAssignmentSystem.Instance.EvaluatePhotoAgainstAssignments(meta);
            }

            LastCapturedTexture = finalPhoto;
            LastCapturedMetadata = meta;
            OnPhotoCaptured?.Invoke(meta, finalPhoto);

            return meta;
        }

        private void ApplyOpticalAndColorPipeline(Texture2D tex, Color[] slowShutterPrevPixels, int width, int height)
        {
            Color[] pixels = tex.GetPixels();
            Color[] sourceCopy = (Color[])pixels.Clone();

            CameraProfile profile = ActiveProfile;
            float exposureMult = Settings.CalculateExposureMultiplier();
            Color wbMult = Settings.GetWhiteBalanceMultiplier(out float wbSat, out bool isMonochrome);
            Color profileTint = profile != null ? profile.colorTintBias : Color.white;
            float totalSat = wbSat * (profile != null ? profile.saturationMultiplier : 1.0f);
            float contrast = profile != null ? profile.contrastMultiplier : 1.0f;
            float vignetteStrength = profile != null ? profile.naturalVignetteIntensity : 0.15f;
            float isoGrain = Mathf.Clamp01((Settings.isoSensitivity - 100f) / 6400f) * 0.08f +
                             (profile != null ? profile.baseFilmGrain : 0.01f);
            float slowShutterBlend = Settings.shutterSpeedDenominator <= 30f ? 0.45f :
                                     (Settings.shutterSpeedDenominator <= 60f ? 0.28f : 0f);
            bool addBorder = profile != null && profile.includeAnalogFilmBorder;
            float cocBlurStrength = CurrentCircleOfConfusion;

            System.Random rng = new System.Random(1337 + Mathf.RoundToInt(Time.time * 100f));

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / height - 0.5f;
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;

                    if (addBorder && (x < 14 || x >= width - 14 || y < 14 || y >= height - 14))
                    {
                        pixels[idx] = new Color(0.08f, 0.07f, 0.06f, 1f);
                        continue;
                    }

                    Color c = sourceCopy[idx];

                    // 1. Slow shutter motion trail blend
                    if (slowShutterPrevPixels != null && slowShutterBlend > 0f && idx < slowShutterPrevPixels.Length)
                    {
                        c = Color.Lerp(c, slowShutterPrevPixels[idx], slowShutterBlend);
                    }

                    // 2. Simulated Depth-of-Field softness (radial + focus-plane CoC neighbor sample)
                    float nx = (float)x / width - 0.5f;
                    float radialDistSq = nx * nx + ny * ny;
                    float localBlurWeight = Mathf.Clamp01(cocBlurStrength + radialDistSq * (5.6f / Mathf.Max(1.4f, Settings.apertureFStop)) * 0.45f);
                    if (localBlurWeight > 0.18f && x > 2 && x < width - 3 && y > 2 && y < height - 3)
                    {
                        Color neighborAvg = (
                            sourceCopy[idx - 2] +
                            sourceCopy[idx + 2] +
                            sourceCopy[(y - 2) * width + x] +
                            sourceCopy[(y + 2) * width + x]) * 0.25f;
                        c = Color.Lerp(c, neighborAvg, localBlurWeight * 0.75f);
                    }

                    // 3. Exposure & White Balance
                    c.r *= exposureMult * wbMult.r * profileTint.r;
                    c.g *= exposureMult * wbMult.g * profileTint.g;
                    c.b *= exposureMult * wbMult.b * profileTint.b;

                    // 4. Soft shoulder highlight roll-off
                    c.r = c.r / (1f + c.r * 0.22f);
                    c.g = c.g / (1f + c.g * 0.22f);
                    c.b = c.b / (1f + c.b * 0.22f);

                    // 5. Contrast around midpoint
                    c.r = (c.r - 0.5f) * contrast + 0.5f;
                    c.g = (c.g - 0.5f) * contrast + 0.5f;
                    c.b = (c.b - 0.5f) * contrast + 0.5f;

                    // 6. Saturation / Monochrome
                    float luma = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                    if (isMonochrome)
                    {
                        c.r = luma * wbMult.r;
                        c.g = luma * wbMult.g;
                        c.b = luma * wbMult.b;
                    }
                    else
                    {
                        c.r = Mathf.Lerp(luma, c.r, totalSat);
                        c.g = Mathf.Lerp(luma, c.g, totalSat);
                        c.b = Mathf.Lerp(luma, c.b, totalSat);
                    }

                    // 7. Optical Vignette
                    float vig = 1.0f - radialDistSq * vignetteStrength * 2.2f;
                    c.r *= vig;
                    c.g *= vig;
                    c.b *= vig;

                    // 8. ISO / Analog Film Grain
                    if (isoGrain > 0.005f)
                    {
                        float noise = ((float)rng.NextDouble() - 0.5f) * isoGrain;
                        c.r += noise;
                        c.g += noise;
                        c.b += noise;
                    }

                    c.r = Mathf.Clamp01(c.r);
                    c.g = Mathf.Clamp01(c.g);
                    c.b = Mathf.Clamp01(c.b);
                    c.a = 1.0f;
                    pixels[idx] = c;
                }
            }

            tex.SetPixels(pixels);
        }

        private float EvaluateCompositionScore(out string feedback)
        {
            float score = 68f;
            List<string> notes = new List<string>();

            if (!string.IsNullOrEmpty(CurrentFramedPoiId))
            {
                score += 14f;
                notes.Add("Strong cultural subject (" + CurrentFramedSubjectName + ")");
            }
            else
            {
                notes.Add("Atmospheric free-form composition");
            }

            if (IsSubjectInSharpFocus)
            {
                score += 10f;
                notes.Add("Tack-sharp focus lock");
            }
            else
            {
                score -= 6f;
                notes.Add("Soft focus / experimental bokeh");
            }

            float evMult = Settings.CalculateExposureMultiplier();
            if (evMult >= 0.7f && evMult <= 1.6f)
            {
                score += 8f;
                notes.Add("Balanced nighttime firelight exposure");
            }
            else if (evMult > 1.6f)
            {
                notes.Add("High-key bright exposure");
            }
            else
            {
                notes.Add("Low-key dramatic shadow exposure");
            }

            score = Mathf.Clamp(score, 40f, 100f);
            feedback = string.Join(" • ", notes.ToArray());
            return score;
        }

        private void EnsureCameraReference()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;
        }
    }
}
