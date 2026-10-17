using System.Collections.Generic;
using UnityEngine;
using LensOfLegends.CameraEquipment;
using LensOfLegends.Community;
using LensOfLegends.Core;
using LensOfLegends.Gallery;
using LensOfLegends.Photography;
using LensOfLegends.Regions;
using LensOfLegends.Story;
using LensOfLegends.Travel;

namespace LensOfLegends.UI
{
    /// <summary>
    /// Complete user interface for Lens of Legends:
    /// - Main Menu (New Journey, Continue, Travel Map, Photo Journal, Community, Settings, Quit)
    /// - India-Wide Travel Map (Interactive map of all 6 Indian destinations + detailed cultural/visual cards)
    /// - Regional Introduction Modal (Separating Storyteller Narrative from Verified Cultural Facts)
    /// - Minimal Exploration HUD & Cultural Discovery Popups
    /// - Dedicated Camera Viewfinder HUD (Rule-of-Thirds grid, Focus Lock, Exposure meter, Camera Profiles, Lens/ISO/Shutter/WB controls)
    /// - Photo Capture Preview Modal
    /// - Personal Photo Journal Gallery (with full-screen lightbox & Discovered Cultural Entries tab)
    /// - Offline Community Demonstration Interface (clearly labeled as a local simulated demo feed)
    /// </summary>
    public class LensOfLegendsUIManager : MonoBehaviour
    {
        public static LensOfLegendsUIManager Instance { get; private set; }

        private Texture2D _solidTex;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _badgeStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _accentButtonStyle;
        private GUIStyle _telemetryStyle;
        private bool _stylesInitialized;

        // Journal state
        private int _journalTabIndex = 0; // 0 = Captured Photographs, 1 = Cultural Discoveries & Story Assignments
        private PhotoMetadata _selectedFullSizePhoto = null;
        private Vector2 _journalScroll;
        private Vector2 _communityScroll;
        private Vector2 _mapDetailsScroll;

        // Cultural Discovery Banner state
        private CulturalPointOfInterestDefinition _recentDiscoveredPoi;
        private float _poiBannerTimer = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _solidTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _solidTex.SetPixel(0, 0, Color.white);
            _solidTex.Apply();
        }

        private void Start()
        {
            if (PhotographySystem.Instance != null)
            {
                PhotographySystem.Instance.OnPhotoCaptured += HandlePhotoCaptured;
            }
            if (CulturalDiscoveryManager.Instance != null)
            {
                CulturalDiscoveryManager.Instance.OnPoiDiscovered += HandlePoiDiscovered;
            }
        }

        private void HandlePhotoCaptured(PhotoMetadata meta, Texture2D tex)
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OpenPhotoPreviewModal();
            }
        }

        private void HandlePoiDiscovered(CulturalPointOfInterestDefinition poi)
        {
            _recentDiscoveredPoi = poi;
            _poiBannerTimer = 8.5f;
        }

        private void Update()
        {
            if (_poiBannerTimer > 0f)
            {
                _poiBannerTimer -= Time.unscaledDeltaTime;
            }
        }

        private void EnsureStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            _titleStyle.normal.textColor = new Color(0.98f, 0.94f, 0.86f);

            _subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            _subtitleStyle.normal.textColor = new Color(0.95f, 0.64f, 0.28f);

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                richText = true
            };
            _bodyStyle.normal.textColor = new Color(0.90f, 0.88f, 0.84f);

            _badgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _badgeStyle.normal.textColor = new Color(1.0f, 0.88f, 0.65f);

            _telemetryStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _telemetryStyle.normal.textColor = new Color(0.96f, 0.92f, 0.80f);

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _buttonStyle.normal.textColor = new Color(0.96f, 0.94f, 0.90f);

            _accentButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _accentButtonStyle.normal.textColor = new Color(1.0f, 0.92f, 0.72f);
        }

        private void OnGUI()
        {
            EnsureStyles();
            if (GameStateManager.Instance == null) return;

            GameFlowState state = GameStateManager.Instance.CurrentState;

            switch (state)
            {
                case GameFlowState.MainMenu:
                    DrawMainMenu();
                    break;
                case GameFlowState.IntroductoryCinematic:
                    DrawCinematicLetterboxOverlay();
                    break;
                case GameFlowState.TravelMap:
                    DrawIndiaTravelMapScreen();
                    break;
                case GameFlowState.RegionalIntroduction:
                    DrawRegionalIntroductionModal();
                    break;
                case GameFlowState.FreeExploration:
                    if (PhotographySystem.Instance != null && PhotographySystem.Instance.IsInCameraMode)
                        DrawPhotographyViewfinderHUD();
                    else
                        DrawMinimalExplorationHUD();
                    break;
                case GameFlowState.PhotoPreviewModal:
                    DrawPhotoCapturePreviewModal();
                    break;
                case GameFlowState.PhotoJournalScreen:
                    DrawPhotoJournalScreen();
                    break;
                case GameFlowState.CommunityDemoScreen:
                    DrawCommunityDemoScreen();
                    break;
                case GameFlowState.SettingsModal:
                    DrawSettingsModal();
                    break;
            }

            // Shutter flash effect
            if (PhotographySystem.Instance != null && PhotographySystem.Instance.ShutterFlashAlpha > 0.01f)
            {
                DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(1f, 0.97f, 0.90f, PhotographySystem.Instance.ShutterFlashAlpha));
            }

            // Toast notification banner
            if (!string.IsNullOrEmpty(GameStateManager.Instance.ActiveToastMessage))
            {
                Rect toastRect = new Rect(Screen.width * 0.5f - 320f, 18f, 640f, 42f);
                DrawPanel(toastRect, new Color(0.10f, 0.08f, 0.07f, 0.94f), new Color(0.94f, 0.52f, 0.18f, 1f));
                GUI.Label(new Rect(toastRect.x + 14f, toastRect.y + 8f, toastRect.width - 28f, 28f),
                    GameStateManager.Instance.ActiveToastMessage, _badgeStyle);
            }
        }

        private void DrawMainMenu()
        {
            float sw = Screen.width;
            float sh = Screen.height;

            DrawRect(new Rect(0, 0, sw, sh), new Color(0.04f, 0.04f, 0.06f, 0.68f));

            Rect card = new Rect(60f, sh * 0.5f - 250f, 520f, 500f);
            DrawPanel(card, new Color(0.08f, 0.07f, 0.09f, 0.94f), new Color(0.88f, 0.46f, 0.18f, 0.9f));

            GUI.Label(new Rect(card.x + 28f, card.y + 24f, card.width - 56f, 36f), "LENS OF LEGENDS", _titleStyle);
            GUI.Label(new Rect(card.x + 28f, card.y + 60f, card.width - 56f, 24f),
                "An Open-World Indian Cultural Photography Adventure", _subtitleStyle);

            GUI.Label(new Rect(card.x + 28f, card.y + 92f, card.width - 56f, 58f),
                "Travel across India with your camera to document living rituals, sacred groves, riverfront ghats, desert festivals, public art, and mountain monasteries. Chapter I (North Kerala — Theyyam) is fully playable.",
                _bodyStyle);

            float btnY = card.y + 162f;
            float btnW = card.width - 56f;
            float btnH = 38f;
            float gap = 46f;

            if (GUI.Button(new Rect(card.x + 28f, btnY, btnW, btnH), "New Journey (Intro Cinematic -> India Travel Map)", _accentButtonStyle))
            {
                GameStateManager.Instance.StartNewJourneyWithIntro();
            }

            btnY += gap;
            bool hasSave = SaveSystem.HasExistingSaveFile();
            string continueLabel = hasSave
                ? "Continue Journey (North Kerala — " + GameStateManager.Instance.SaveData.capturedPhotos.Count + " Photos in Journal)"
                : "Enter Chapter I Directly (North Kerala — Theyyam)";
            if (GUI.Button(new Rect(card.x + 28f, btnY, btnW, btnH), continueLabel, _buttonStyle))
            {
                GameStateManager.Instance.ContinueSavedJourney();
            }

            btnY += gap;
            if (GUI.Button(new Rect(card.x + 28f, btnY, btnW, btnH), "India Travel Map (6 Regional Destinations)", _buttonStyle))
            {
                GameStateManager.Instance.OpenTravelMap();
            }

            btnY += gap;
            if (GUI.Button(new Rect(card.x + 28f, btnY, btnW, btnH),
                "Personal Photo Journal (" + GameStateManager.Instance.SaveData.capturedPhotos.Count + " Captures)", _buttonStyle))
            {
                GameStateManager.Instance.OpenPhotoJournal(GameFlowState.MainMenu);
            }

            btnY += gap;
            if (GUI.Button(new Rect(card.x + 28f, btnY, btnW, btnH), "Community Photography Feed [Offline Demo]", _buttonStyle))
            {
                GameStateManager.Instance.OpenCommunityScreen(GameFlowState.MainMenu);
            }

            btnY += gap;
            if (GUI.Button(new Rect(card.x + 28f, btnY, (btnW - 12f) * 0.5f, btnH), "Settings & Controls", _buttonStyle))
            {
                GameStateManager.Instance.OpenSettings(GameFlowState.MainMenu);
            }

            if (GUI.Button(new Rect(card.x + 28f + (btnW + 12f) * 0.5f, btnY, (btnW - 12f) * 0.5f, btnH), "Quit Game", _buttonStyle))
            {
                Application.Quit();
            }

            // Right-side Photographer Identity Summary Card
            Rect rightCard = new Rect(sw - 420f, sh - 170f, 360f, 120f);
            DrawPanel(rightCard, new Color(0.08f, 0.07f, 0.09f, 0.88f), new Color(0.55f, 0.38f, 0.22f, 0.7f));
            GUI.Label(new Rect(rightCard.x + 18f, rightCard.y + 14f, 324f, 22f), "FIELD PHOTOGRAPHER RECORD", _subtitleStyle);
            GUI.Label(new Rect(rightCard.x + 18f, rightCard.y + 40f, 324f, 68f),
                string.Format(
                    "Photographer XP: {0}  |  Saved Photos: {1}\nCultural POIs Discovered: {2} / 5\nUnlocked Cameras: {3} / 4",
                    GameStateManager.Instance.SaveData.photographerXp,
                    GameStateManager.Instance.SaveData.capturedPhotos.Count,
                    GameStateManager.Instance.SaveData.discoveredPoiIds.Count,
                    GameStateManager.Instance.SaveData.unlockedCameraProfileIds.Count),
                _bodyStyle);
        }

        private void DrawCinematicLetterboxOverlay()
        {
            float sw = Screen.width;
            float sh = Screen.height;
            float barH = Mathf.Max(78f, sh * 0.12f);

            DrawRect(new Rect(0, 0, sw, barH), new Color(0.02f, 0.02f, 0.03f, 0.96f));
            DrawRect(new Rect(0, sh - barH, sw, barH), new Color(0.02f, 0.02f, 0.03f, 0.96f));

            if (CinematicDirector.Instance != null)
            {
                GUI.Label(new Rect(40f, 18f, sw - 260f, 28f), CinematicDirector.Instance.CurrentTitle, _subtitleStyle);
                GUI.Label(new Rect(40f, sh - barH + 16f, sw - 280f, 48f), CinematicDirector.Instance.CurrentSubtitle, _bodyStyle);

                if (GUI.Button(new Rect(sw - 190f, sh - barH + 20f, 150f, 34f), "Skip Sequence [Space]", _buttonStyle))
                {
                    CinematicDirector.Instance.SkipCinematic();
                }
            }
        }

        private void DrawIndiaTravelMapScreen()
        {
            float sw = Screen.width;
            float sh = Screen.height;

            DrawRect(new Rect(0, 0, sw, sh), new Color(0.05f, 0.06f, 0.09f, 0.95f));

            // Top bar
            DrawPanel(new Rect(24f, 18f, sw - 48f, 58f), new Color(0.09f, 0.09f, 0.12f, 0.98f), new Color(0.85f, 0.48f, 0.18f, 0.8f));
            GUI.Label(new Rect(42f, 28f, 520f, 34f), "INDIA TRAVEL MAP — CULTURAL PHOTOGRAPHY CHAPTERS", _titleStyle);

            if (GUI.Button(new Rect(sw - 390f, 29f, 115f, 34f), "Photo Journal", _buttonStyle))
                GameStateManager.Instance.OpenPhotoJournal(GameFlowState.TravelMap);
            if (GUI.Button(new Rect(sw - 265f, 29f, 115f, 34f), "Community", _buttonStyle))
                GameStateManager.Instance.OpenCommunityScreen(GameFlowState.TravelMap);
            if (GUI.Button(new Rect(sw - 140f, 29f, 100f, 34f), "Main Menu", _buttonStyle))
                GameStateManager.Instance.ReturnToMainMenu();

            // Left panel: Interactive Map of India with 6 destination pins + chapter list
            Rect mapPanel = new Rect(24f, 88f, sw * 0.46f - 30f, sh - 112f);
            DrawPanel(mapPanel, new Color(0.07f, 0.09f, 0.13f, 0.96f), new Color(0.35f, 0.42f, 0.55f, 0.6f));

            GUI.Label(new Rect(mapPanel.x + 18f, mapPanel.y + 14f, mapPanel.width - 36f, 24f),
                "SELECT A REGIONAL CHAPTER ACROSS INDIA", _subtitleStyle);

            // Stylized India Subcontinent Map Canvas
            Rect indiaCanvas = new Rect(mapPanel.x + 18f, mapPanel.y + 44f, mapPanel.width - 36f, mapPanel.height * 0.46f);
            DrawPanel(indiaCanvas, new Color(0.05f, 0.08f, 0.12f, 1f), new Color(0.25f, 0.35f, 0.48f, 0.5f));

            List<RegionData> regions = TravelMapController.Instance != null
                ? TravelMapController.Instance.AllDestinations
                : IndiaDestinationCatalog.GetAllDestinations();

            int selectedIdx = TravelMapController.Instance != null ? TravelMapController.Instance.SelectedDestinationIndex : 0;

            for (int i = 0; i < regions.Count; i++)
            {
                RegionData r = regions[i];
                float pinX = indiaCanvas.x + 28f + r.normalizedIndiaMapPosition.x * (indiaCanvas.width - 170f);
                float pinY = indiaCanvas.y + indiaCanvas.height - 28f - r.normalizedIndiaMapPosition.y * (indiaCanvas.height - 56f);

                bool isSelected = (i == selectedIdx);
                bool isPlayable = (r.playableState == DestinationPlayableState.PlayableVerticalSlice);

                Color pinColor = isPlayable
                    ? new Color(0.95f, 0.46f, 0.14f, 1f)
                    : (isSelected ? new Color(0.35f, 0.75f, 0.95f, 1f) : new Color(0.35f, 0.40f, 0.48f, 0.9f));

                Rect pinRect = new Rect(pinX, pinY - 13f, 155f, 26f);
                DrawPanel(pinRect, pinColor, Color.white);
                string pinLabel = (i + 1) + ". " + r.regionTitle.Split('—')[0].Trim() + (isPlayable ? " ★" : "");
                if (GUI.Button(pinRect, pinLabel, _badgeStyle))
                {
                    if (TravelMapController.Instance != null)
                        TravelMapController.Instance.SelectDestinationByIndex(i);
                    if (ProceduralSoundscapeGenerator.Instance != null)
                        ProceduralSoundscapeGenerator.Instance.ApplyRegionSoundscape(r);
                }
            }

            // Chapter list buttons below the map canvas
            float listStartY = indiaCanvas.yMax + 12f;
            float rowH = Mathf.Min(42f, (mapPanel.yMax - listStartY - 12f) / regions.Count);
            for (int i = 0; i < regions.Count; i++)
            {
                RegionData r = regions[i];
                bool isSelected = (i == selectedIdx);
                bool isPlayable = (r.playableState == DestinationPlayableState.PlayableVerticalSlice);
                string statusTag = isPlayable ? "[PLAYABLE CHAPTER]" : "[PREVIEW / FUTURE]";

                Rect rowRect = new Rect(mapPanel.x + 18f, listStartY + i * rowH, mapPanel.width - 36f, rowH - 4f);
                if (isSelected)
                    DrawRect(rowRect, new Color(0.92f, 0.45f, 0.16f, 0.28f));

                if (GUI.Button(rowRect, (i + 1) + ". " + r.regionTitle + "  —  " + statusTag, isSelected ? _accentButtonStyle : _buttonStyle))
                {
                    if (TravelMapController.Instance != null)
                        TravelMapController.Instance.SelectDestinationByIndex(i);
                    if (ProceduralSoundscapeGenerator.Instance != null)
                        ProceduralSoundscapeGenerator.Instance.ApplyRegionSoundscape(r);
                }
            }

            // Right panel: Selected Destination Inspection & Launch
            RegionData sel = regions[Mathf.Clamp(selectedIdx, 0, regions.Count - 1)];
            Rect detailPanel = new Rect(mapPanel.xMax + 16f, 88f, sw - mapPanel.xMax - 40f, sh - 112f);
            DrawPanel(detailPanel, new Color(0.08f, 0.08f, 0.11f, 0.96f), sel.accentUiColor);

            float dx = detailPanel.x + 22f;
            float dw = detailPanel.width - 44f;
            float dy = detailPanel.y + 18f;

            GUI.Label(new Rect(dx, dy, dw, 32f), sel.regionTitle, _titleStyle);
            dy += 34f;
            GUI.Label(new Rect(dx, dy, dw, 22f), sel.chapterSubtitle + "  •  " + sel.stateOrTerritory, _subtitleStyle);
            dy += 28f;

            // Visual lighting palette swatch bar
            DrawRect(new Rect(dx, dy, dw * 0.25f, 14f), sel.skyTopColor);
            DrawRect(new Rect(dx + dw * 0.25f, dy, dw * 0.25f, 14f), sel.skyHorizonColor);
            DrawRect(new Rect(dx + dw * 0.50f, dy, dw * 0.25f, 14f), sel.keyLightColor);
            DrawRect(new Rect(dx + dw * 0.75f, dy, dw * 0.25f, 14f), sel.ambientLightColor);
            dy += 22f;

            Rect scrollRect = new Rect(dx, dy, dw, detailPanel.height - 175f);
            Rect contentRect = new Rect(0f, 0f, dw - 24f, 410f);
            _mapDetailsScroll = GUI.BeginScrollView(scrollRect, _mapDetailsScroll, contentRect);

            string detailsText =
                "<b>Regional Visual & Cultural Theme:</b>\n" + sel.themeSummary + "\n\n" +
                "<b>Verified Cultural Background:</b>\n" + sel.verifiedCulturalBackground + "\n" +
                "Reference Source: " + sel.primaryReferenceUrl + "\n\n" +
                "<b>Narrative Journey Context:</b>\n" + sel.regionalIntroductionNarrative + "\n\n" +
                "<b>Environmental Soundscape Profile:</b>\n" + sel.ambientSoundscapeDescription + "\n\n" +
                "<b>Key Photography Subjects:</b>\n• " + string.Join("\n• ", sel.plannedPhotographySubjects.ToArray());

            GUI.Label(new Rect(0f, 0f, contentRect.width, 400f), detailsText, _bodyStyle);
            GUI.EndScrollView();

            // Action button at bottom of detail panel
            Rect actionRect = new Rect(dx, detailPanel.yMax - 58f, dw, 42f);
            if (sel.playableState == DestinationPlayableState.PlayableVerticalSlice)
            {
                if (GUI.Button(actionRect, "TRAVEL TO " + sel.regionTitle.ToUpper() + " (ENTER PLAYABLE REGION)", _accentButtonStyle))
                {
                    if (TravelMapController.Instance != null)
                        TravelMapController.Instance.TravelToSelectedDestination();
                }
            }
            else
            {
                if (GUI.Button(actionRect, "PREVIEW CHAPTER ONLY — Future Destination (Click to View Notice)", _buttonStyle))
                {
                    if (TravelMapController.Instance != null)
                        TravelMapController.Instance.TravelToSelectedDestination();
                }
            }
        }

        private void DrawRegionalIntroductionModal()
        {
            float sw = Screen.width;
            float sh = Screen.height;
            RegionData r = GameStateManager.Instance.CurrentRegion ?? IndiaDestinationCatalog.GetAllDestinations()[0];

            DrawRect(new Rect(0, 0, sw, sh), new Color(0.03f, 0.03f, 0.05f, 0.80f));

            Rect modal = new Rect(sw * 0.5f - 380f, sh * 0.5f - 240f, 760f, 480f);
            DrawPanel(modal, new Color(0.08f, 0.07f, 0.09f, 0.97f), r.accentUiColor);

            float mx = modal.x + 28f;
            float mw = modal.width - 56f;
            GUI.Label(new Rect(mx, modal.y + 22f, mw, 32f), r.regionTitle, _titleStyle);
            GUI.Label(new Rect(mx, modal.y + 56f, mw, 24f), r.chapterSubtitle, _subtitleStyle);

            GUI.Label(new Rect(mx, modal.y + 92f, mw, 110f),
                "<b>1. VERIFIED CULTURAL TRADITION (Authentic Historical & Cultural Context)</b>\n" +
                r.verifiedCulturalBackground + "\nSource: " + r.primaryReferenceUrl,
                _bodyStyle);

            GUI.Label(new Rect(mx, modal.y + 212f, mw, 105f),
                "<b>2. STORYTELLER INTRODUCTION (Fictional Narrative Layer)</b>\n" +
                r.regionalIntroductionNarrative,
                _bodyStyle);

            GUI.Label(new Rect(mx, modal.y + 322f, mw, 76f),
                "<b>EXPLORATION & PHOTOGRAPHY CONTROLS</b>\n" +
                "• Move: <b>WASD</b> (Hold Shift to walk faster)  |  Look: <b>Mouse</b>\n" +
                "• Enter/Exit Camera Mode: <b>Right-Click</b> or <b>[V]</b>  |  Capture Photo: <b>Left-Click</b> or <b>[Space]</b>\n" +
                "• Inspect Cultural Point of Interest: <b>[E]</b>  |  Photo Journal: <b>[J]</b>  |  Travel Map: <b>[M]</b>",
                _bodyStyle);

            if (GUI.Button(new Rect(mx, modal.yMax - 60f, (mw - 16f) * 0.55f, 40f),
                "Begin with Arrival Camera Sweep -> Explore", _accentButtonStyle))
            {
                GameStateManager.Instance.EnterFreeExplorationFromRegionalIntro(playArrivalCinematic: true);
            }

            if (GUI.Button(new Rect(mx + (mw - 16f) * 0.55f + 16f, modal.yMax - 60f, (mw - 16f) * 0.45f, 40f),
                "Skip Camera Sweep -> Explore Immediately", _buttonStyle))
            {
                GameStateManager.Instance.EnterFreeExplorationFromRegionalIntro(playArrivalCinematic: false);
            }
        }

        private void DrawMinimalExplorationHUD()
        {
            float sw = Screen.width;
            float sh = Screen.height;

            // Minimal top-left region badge
            Rect topBar = new Rect(20f, 18f, 410f, 54f);
            DrawPanel(topBar, new Color(0.06f, 0.06f, 0.08f, 0.78f), new Color(0.85f, 0.45f, 0.18f, 0.6f));
            GUI.Label(new Rect(32f, 24f, 386f, 22f), "NORTH KERALA — THEYYAM IN THE SACRED GROVE", _subtitleStyle);
            GUI.Label(new Rect(32f, 44f, 386f, 22f),
                "[Right-Click / V] Camera Mode   [J] Journal   [K] Community   [M] Map", _bodyStyle);

            // Minimal top-right optional story assignment prompt
            if (StoryAssignmentSystem.Instance != null)
            {
                Rect assignBox = new Rect(sw - 370f, 18f, 350f, 92f);
                DrawPanel(assignBox, new Color(0.06f, 0.06f, 0.08f, 0.76f), new Color(0.45f, 0.40f, 0.35f, 0.5f));
                GUI.Label(new Rect(assignBox.x + 12f, assignBox.y + 8f, 326f, 20f),
                    "OPTIONAL PHOTOGRAPHY INSPIRATIONS", _subtitleStyle);

                int shown = 0;
                foreach (var a in StoryAssignmentSystem.Instance.RegionalAssignments)
                {
                    bool done = GameStateManager.Instance.IsAssignmentCompleted(a.assignmentId);
                    string line = (done ? "[✓] " : "[ ] ") + a.title.Replace("Optional Inspiration ", "");
                    GUI.Label(new Rect(assignBox.x + 12f, assignBox.y + 28f + shown * 19f, 326f, 20f), line, _bodyStyle);
                    shown++;
                    if (shown >= 3) break;
                }
            }

            // Proximity prompt when near a Cultural Point of Interest
            if (CulturalDiscoveryManager.Instance != null && CulturalDiscoveryManager.Instance.NearestInspectablePoi != null)
            {
                PointOfInterest poi = CulturalDiscoveryManager.Instance.NearestInspectablePoi;
                Rect promptRect = new Rect(sw * 0.5f - 230f, sh - 135f, 460f, 38f);
                DrawPanel(promptRect, new Color(0.08f, 0.07f, 0.09f, 0.90f), new Color(0.95f, 0.68f, 0.22f, 0.9f));
                GUI.Label(new Rect(promptRect.x + 12f, promptRect.y + 8f, promptRect.width - 24f, 24f),
                    "Press [E] to Inspect Cultural Note: " + poi.Definition.title, _badgeStyle);
            }

            // Recent Cultural Discovery non-intrusive card at bottom-left
            if (_poiBannerTimer > 0f && _recentDiscoveredPoi != null)
            {
                Rect poiCard = new Rect(20f, sh - 205f, 520f, 180f);
                DrawPanel(poiCard, new Color(0.07f, 0.06f, 0.08f, 0.94f), new Color(0.95f, 0.58f, 0.20f, 0.9f));
                GUI.Label(new Rect(poiCard.x + 16f, poiCard.y + 10f, 488f, 22f),
                    "CULTURAL POINT OF INTEREST: " + _recentDiscoveredPoi.title, _subtitleStyle);
                GUI.Label(new Rect(poiCard.x + 16f, poiCard.y + 34f, 488f, 138f),
                    "<b>" + _recentDiscoveredPoi.verifiedCulturalInformation + "</b>\n" +
                    "<i>" + _recentDiscoveredPoi.narrativeStorytellerNote + "</i>",
                    _bodyStyle);
            }
        }

        private void DrawPhotographyViewfinderHUD()
        {
            float sw = Screen.width;
            float sh = Screen.height;
            PhotographySystem photoSys = PhotographySystem.Instance;
            if (photoSys == null) return;

            PhotographySettings s = photoSys.Settings;
            CameraProfile profile = photoSys.ActiveProfile;

            // Simulated Depth-of-Field peripheral softness indicator bars
            float coc = photoSys.CurrentCircleOfConfusion;
            if (coc > 0.08f)
            {
                Color dofTint = new Color(0.04f, 0.03f, 0.05f, Mathf.Clamp(coc * 0.45f, 0.05f, 0.35f));
                DrawRect(new Rect(0, 0, sw, 48f), dofTint);
                DrawRect(new Rect(0, sh - 68f, sw, 68f), dofTint);
            }

            // Viewfinder outer framing border
            float marginX = sw * 0.06f;
            float marginY = sh * 0.08f;
            Rect vfRect = new Rect(marginX, marginY, sw - marginX * 2f, sh - marginY * 2f - 32f);
            DrawOutline(vfRect, new Color(1f, 1f, 1f, 0.45f), 2f);

            // Rule of Thirds Grid
            if (s.showRuleOfThirdsGrid)
            {
                Color gridCol = new Color(1f, 1f, 1f, 0.22f);
                DrawRect(new Rect(vfRect.x + vfRect.width / 3f, vfRect.y, 1f, vfRect.height), gridCol);
                DrawRect(new Rect(vfRect.x + vfRect.width * 2f / 3f, vfRect.y, 1f, vfRect.height), gridCol);
                DrawRect(new Rect(vfRect.x, vfRect.y + vfRect.height / 3f, vfRect.width, 1f), gridCol);
                DrawRect(new Rect(vfRect.x, vfRect.y + vfRect.height * 2f / 3f, vfRect.width, 1f), gridCol);
            }

            // Center Focus Reticle & Focus Lock Indicator
            bool inFocus = photoSys.IsSubjectInSharpFocus;
            Color focusColor = inFocus
                ? new Color(0.32f, 0.96f, 0.48f, 0.9f)
                : new Color(0.98f, 0.68f, 0.20f, 0.9f);
            Rect reticle = new Rect(sw * 0.5f - 45f, vfRect.center.y - 32f, 90f, 64f);
            DrawOutline(reticle, focusColor, 2f);

            string focusModeTag = s.autoFocusEnabled ? "AF-C" : "MF";
            string focusStatus = string.Format(
                "{0} {1} • Dist: {2:0.0}m (Focus Plane: {3:0.0}m)",
                focusModeTag,
                inFocus ? "[SHARP FOCUS LOCK]" : "[SOFT BOKEH / OUT OF FOCUS]",
                photoSys.CurrentSubjectDistanceMeters,
                s.focusDistanceMeters);
            GUI.Label(new Rect(sw * 0.5f - 220f, reticle.yMax + 6f, 440f, 22f), focusStatus, _badgeStyle);

            // Top Viewfinder Telemetry Bar (Camera Profile & Framed Subject)
            Rect topBar = new Rect(vfRect.x, 12f, vfRect.width, 38f);
            DrawPanel(topBar, new Color(0.05f, 0.05f, 0.07f, 0.88f), new Color(0.88f, 0.48f, 0.18f, 0.7f));
            string camLabel = profile != null ? profile.cameraName : "Camera";
            GUI.Label(new Rect(topBar.x + 14f, topBar.y + 8f, topBar.width - 28f, 22f),
                string.Format(
                    "CAMERA [1-4]: {0}   |   SUBJECT: {1}   |   COLOR [C]: {2}",
                    camLabel,
                    photoSys.CurrentFramedSubjectName,
                    s.GetColorProfileDisplayName()),
                _telemetryStyle);

            // Bottom Viewfinder Optical & Exposure Deck
            Rect botBar = new Rect(vfRect.x, sh - 82f, vfRect.width, 68f);
            DrawPanel(botBar, new Color(0.05f, 0.05f, 0.07f, 0.92f), new Color(0.88f, 0.48f, 0.18f, 0.85f));

            string evSign = s.exposureCompensationEv >= 0f ? "+" : "";
            string telemetryLine1 = string.Format(
                "FOCAL [Scroll]: {0:0}mm     APERTURE [Q/E]: f/{1:0.0} (Sim DoF)     SHUTTER [O]: {2}     ISO [I]: {3}     EXP [-/=]: {4}{5:0.1} EV     GRID [G]: {6}",
                s.focalLengthMm,
                s.apertureFStop,
                s.GetFormattedShutterSpeed(),
                s.isoSensitivity,
                evSign,
                s.exposureCompensationEv,
                s.showRuleOfThirdsGrid ? "ON" : "OFF");

            string telemetryLine2 =
                "[Left-Click / Space] CAPTURE PHOTOGRAPH     [F] Toggle AF/MF (Manual: [ / ])     [1/2/3/4] Switch Camera Body     [Right-Click / V] Exit Camera";

            GUI.Label(new Rect(botBar.x + 14f, botBar.y + 8f, botBar.width - 28f, 24f), telemetryLine1, _telemetryStyle);
            GUI.Label(new Rect(botBar.x + 14f, botBar.y + 36f, botBar.width - 28f, 22f), telemetryLine2, _bodyStyle);
        }

        private void DrawPhotoCapturePreviewModal()
        {
            float sw = Screen.width;
            float sh = Screen.height;
            PhotographySystem photoSys = PhotographySystem.Instance;
            if (photoSys == null || photoSys.LastCapturedMetadata == null)
            {
                GameStateManager.Instance.CloseOverlayToPreviousState();
                return;
            }

            PhotoMetadata meta = photoSys.LastCapturedMetadata;
            Texture2D tex = photoSys.LastCapturedTexture;

            DrawRect(new Rect(0, 0, sw, sh), new Color(0.03f, 0.03f, 0.05f, 0.86f));

            Rect modal = new Rect(sw * 0.5f - 390f, sh * 0.5f - 265f, 780f, 530f);
            DrawPanel(modal, new Color(0.08f, 0.08f, 0.10f, 0.98f), new Color(0.95f, 0.54f, 0.18f, 1f));

            GUI.Label(new Rect(modal.x + 24f, modal.y + 16f, modal.width - 48f, 28f),
                "PHOTOGRAPH CAPTURED & SAVED TO PERSONAL JOURNAL", _titleStyle);

            if (tex != null)
            {
                Rect imgRect = new Rect(modal.x + 24f, modal.y + 52f, 512f, 288f);
                DrawOutline(imgRect, Color.white, 2f);
                GUI.DrawTexture(imgRect, tex, ScaleMode.ScaleToFit);
            }

            Rect metaRect = new Rect(modal.x + 552f, modal.y + 52f, 204f, 288f);
            string metaText =
                "<b>Subject:</b>\n" + meta.primarySubjectTag + "\n\n" +
                "<b>Region:</b>\n" + meta.regionDisplayName + "\n\n" +
                "<b>Camera & Settings:</b>\n" +
                meta.cameraProfileName + "\n" +
                meta.focalLengthMm + "mm • f/" + meta.apertureFStop + "\n" +
                "ISO " + meta.isoSensitivity + " • " + meta.shutterSpeedLabel + "\n" +
                meta.colorProfileLabel + "\n\n" +
                "<b>Composition Rating:</b> " + meta.compositionScore + " / 100";
            GUI.Label(metaRect, metaText, _bodyStyle);

            GUI.Label(new Rect(modal.x + 24f, modal.y + 352f, modal.width - 48f, 44f),
                "<b>Field Notes:</b> " + meta.compositionFeedback +
                "\n<b>Saved Locally:</b> " + SaveSystem.ResolvePhotoFullPath(meta.relativeFileName),
                _bodyStyle);

            float btnY = modal.yMax - 56f;
            if (GUI.Button(new Rect(modal.x + 24f, btnY, 230f, 38f), "Continue Exploring [Esc]", _accentButtonStyle))
            {
                GameStateManager.Instance.CloseOverlayToPreviousState();
            }

            if (GUI.Button(new Rect(modal.x + 268f, btnY, 230f, 38f), "Open in Photo Journal", _buttonStyle))
            {
                GameStateManager.Instance.OpenPhotoJournal(GameFlowState.FreeExploration);
            }

            if (GUI.Button(new Rect(modal.x + 512f, btnY, 244f, 38f), "Share to Local Community Demo", _buttonStyle))
            {
                if (CommunityPhotographyManager.Instance != null)
                    CommunityPhotographyManager.Instance.PublishPhotoToDemoFeed(meta);
            }
        }

        private void DrawPhotoJournalScreen()
        {
            float sw = Screen.width;
            float sh = Screen.height;

            DrawRect(new Rect(0, 0, sw, sh), new Color(0.05f, 0.05f, 0.07f, 0.96f));

            Rect panel = new Rect(32f, 24f, sw - 64f, sh - 48f);
            DrawPanel(panel, new Color(0.08f, 0.08f, 0.11f, 0.98f), new Color(0.88f, 0.48f, 0.18f, 0.85f));

            GUI.Label(new Rect(panel.x + 24f, panel.y + 18f, 540f, 32f),
                "PERSONAL PHOTOGRAPHER'S TRAVEL JOURNAL", _titleStyle);

            if (GUI.Button(new Rect(panel.xMax - 520f, panel.y + 18f, 180f, 34f),
                "Photographs (" + GameStateManager.Instance.SaveData.capturedPhotos.Count + ")",
                _journalTabIndex == 0 ? _accentButtonStyle : _buttonStyle))
            {
                _journalTabIndex = 0;
                _selectedFullSizePhoto = null;
            }

            if (GUI.Button(new Rect(panel.xMax - 330f, panel.y + 18f, 190f, 34f),
                "Cultural Notes & Badges",
                _journalTabIndex == 1 ? _accentButtonStyle : _buttonStyle))
            {
                _journalTabIndex = 1;
                _selectedFullSizePhoto = null;
            }

            if (GUI.Button(new Rect(panel.xMax - 130f, panel.y + 18f, 106f, 34f), "Close [Esc]", _buttonStyle))
            {
                _selectedFullSizePhoto = null;
                GameStateManager.Instance.CloseOverlayToPreviousState();
            }

            if (_selectedFullSizePhoto != null)
            {
                DrawFullSizePhotoLightbox(panel, _selectedFullSizePhoto);
                return;
            }

            if (_journalTabIndex == 0)
            {
                DrawPhotoJournalGrid(panel);
            }
            else
            {
                DrawCulturalDiscoveriesAndBadgesTab(panel);
            }
        }

        private void DrawPhotoJournalGrid(Rect panel)
        {
            IReadOnlyList<PhotoMetadata> photos = PhotoJournalManager.Instance != null
                ? PhotoJournalManager.Instance.GetCapturedPhotos()
                : GameStateManager.Instance.SaveData.capturedPhotos;

            GUI.Label(new Rect(panel.x + 24f, panel.y + 58f, panel.width - 48f, 22f),
                "Persistent Storage Path: " + SaveSystem.GetPhotosDirectoryPath() + " (Only genuine player captures are stored here)",
                _bodyStyle);

            if (photos == null || photos.Count == 0)
            {
                GUI.Label(new Rect(panel.x + 24f, panel.y + 110f, panel.width - 48f, 60f),
                    "Your personal Photo Journal is currently empty.\nEnter North Kerala — Theyyam, press [Right-Click] or [V] to raise your camera, and press [Left-Click] or [Space] to capture your first photograph.",
                    _subtitleStyle);
                return;
            }

            Rect scrollArea = new Rect(panel.x + 24f, panel.y + 88f, panel.width - 48f, panel.height - 108f);
            int cols = Mathf.Max(2, Mathf.FloorToInt((scrollArea.width - 24f) / 300f));
            int rows = Mathf.CeilToInt((float)photos.Count / cols);
            Rect contentRect = new Rect(0f, 0f, scrollArea.width - 24f, rows * 255f + 20f);

            _journalScroll = GUI.BeginScrollView(scrollArea, _journalScroll, contentRect);

            for (int i = 0; i < photos.Count; i++)
            {
                PhotoMetadata p = photos[i];
                int col = i % cols;
                int row = i / cols;
                Rect card = new Rect(col * 305f, row * 250f, 290f, 238f);
                DrawPanel(card, new Color(0.11f, 0.11f, 0.14f, 1f), new Color(0.45f, 0.38f, 0.30f, 0.7f));

                Rect thumbRect = new Rect(card.x + 10f, card.y + 10f, 270f, 145f);
                if (PhotoJournalManager.Instance != null &&
                    PhotoJournalManager.Instance.TryGetPhotoTexture(p, out Texture2D tex, out string statusMsg))
                {
                    GUI.DrawTexture(thumbRect, tex, ScaleMode.ScaleAndCrop);
                }
                else
                {
                    DrawRect(thumbRect, new Color(0.18f, 0.12f, 0.12f, 1f));
                    GUI.Label(new Rect(thumbRect.x + 10f, thumbRect.y + 45f, thumbRect.width - 20f, 50f),
                        "Missing/Corrupted Image Handled Gracefully:\n" + statusMsg, _bodyStyle);
                }

                GUI.Label(new Rect(card.x + 10f, card.y + 160f, 270f, 20f), p.primarySubjectTag, _subtitleStyle);
                GUI.Label(new Rect(card.x + 10f, card.y + 180f, 270f, 20f),
                    p.regionDisplayName + " • " + p.captureTimestampIso, _bodyStyle);

                if (GUI.Button(new Rect(card.x + 10f, card.y + 204f, 130f, 26f), "View Full Size", _buttonStyle))
                {
                    _selectedFullSizePhoto = p;
                }
                if (GUI.Button(new Rect(card.x + 150f, card.y + 204f, 130f, 26f),
                    p.isPublishedToDemoCommunity ? "Shared in Demo" : "Share to Demo", _buttonStyle))
                {
                    if (CommunityPhotographyManager.Instance != null)
                        CommunityPhotographyManager.Instance.PublishPhotoToDemoFeed(p);
                }
            }

            GUI.EndScrollView();
        }

        private void DrawFullSizePhotoLightbox(Rect panel, PhotoMetadata p)
        {
            Rect imgArea = new Rect(panel.x + 28f, panel.y + 68f, panel.width * 0.64f, panel.height - 96f);
            DrawPanel(imgArea, new Color(0.04f, 0.04f, 0.05f, 1f), Color.white);

            if (PhotoJournalManager.Instance != null &&
                PhotoJournalManager.Instance.TryGetPhotoTexture(p, out Texture2D tex, out string statusMsg))
            {
                GUI.DrawTexture(new Rect(imgArea.x + 8f, imgArea.y + 8f, imgArea.width - 16f, imgArea.height - 16f), tex, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.Label(new Rect(imgArea.x + 24f, imgArea.y + 40f, imgArea.width - 48f, 80f),
                    "Could not display image file: " + statusMsg, _subtitleStyle);
            }

            Rect infoArea = new Rect(imgArea.xMax + 20f, panel.y + 68f, panel.xMax - imgArea.xMax - 44f, panel.height - 96f);
            DrawPanel(infoArea, new Color(0.10f, 0.10f, 0.13f, 1f), new Color(0.85f, 0.48f, 0.18f, 0.7f));

            string details =
                "<b>Subject:</b>\n" + p.primarySubjectTag + "\n\n" +
                "<b>Region:</b>\n" + p.regionDisplayName + "\n\n" +
                "<b>Captured:</b> " + p.captureTimestampIso + "\n\n" +
                "<b>Camera Equipment:</b>\n" + p.cameraProfileName + "\n\n" +
                "<b>Optical & Exposure Settings:</b>\n" +
                "Focal Length: " + p.focalLengthMm + "mm\n" +
                "Aperture: f/" + p.apertureFStop + "\n" +
                "Shutter Speed: " + p.shutterSpeedLabel + "\n" +
                "ISO: " + p.isoSensitivity + "\n" +
                "Color Profile: " + p.colorProfileLabel + "\n\n" +
                "<b>Composition Notes:</b>\n" + p.compositionFeedback;

            GUI.Label(new Rect(infoArea.x + 16f, infoArea.y + 16f, infoArea.width - 32f, infoArea.height - 110f), details, _bodyStyle);

            if (GUI.Button(new Rect(infoArea.x + 16f, infoArea.yMax - 84f, infoArea.width - 32f, 32f), "Back to Journal Grid", _accentButtonStyle))
            {
                _selectedFullSizePhoto = null;
            }
            if (GUI.Button(new Rect(infoArea.x + 16f, infoArea.yMax - 44f, infoArea.width - 32f, 32f), "Delete Photograph", _buttonStyle))
            {
                if (PhotoJournalManager.Instance != null)
                    PhotoJournalManager.Instance.DeletePhotoFromJournal(p);
                _selectedFullSizePhoto = null;
            }
        }

        private void DrawCulturalDiscoveriesAndBadgesTab(Rect panel)
        {
            RegionData kerala = IndiaDestinationCatalog.GetAllDestinations()[0];
            float x = panel.x + 24f;
            float y = panel.y + 68f;
            float w = panel.width - 48f;

            string badges = GameStateManager.Instance.SaveData.earnedBadges.Count > 0
                ? string.Join("  •  ", GameStateManager.Instance.SaveData.earnedBadges.ToArray())
                : "Capture photographs and discover Cultural POIs to earn Photographer Badges.";

            GUI.Label(new Rect(x, y, w, 24f), "EARNED PHOTOGRAPHER BADGES & MILESTONES: " + badges, _subtitleStyle);
            y += 32f;

            for (int i = 0; i < kerala.pointsOfInterest.Count; i++)
            {
                var poi = kerala.pointsOfInterest[i];
                bool disc = GameStateManager.Instance.IsPoiDiscovered(poi.poiId);
                Rect row = new Rect(x, y, w, 72f);
                DrawPanel(row, new Color(0.11f, 0.11f, 0.14f, 0.95f),
                    disc ? new Color(0.32f, 0.85f, 0.48f, 0.8f) : new Color(0.4f, 0.4f, 0.45f, 0.5f));

                string title = (disc ? "[DISCOVERED] " : "[UNDISCOVERED IN GROVE] ") + poi.title + " (" + poi.category + ")";
                GUI.Label(new Rect(row.x + 14f, row.y + 6f, row.width - 28f, 20f), title, _subtitleStyle);
                string body = disc
                    ? poi.verifiedCulturalInformation + " (" + poi.verifiedReferenceSource + ")"
                    : "Approach this location in the North Kerala sacred grove or photograph it to record its cultural entry.";
                GUI.Label(new Rect(row.x + 14f, row.y + 28f, row.width - 28f, 40f), body, _bodyStyle);
                y += 78f;
            }
        }

        private void DrawCommunityDemoScreen()
        {
            float sw = Screen.width;
            float sh = Screen.height;

            DrawRect(new Rect(0, 0, sw, sh), new Color(0.04f, 0.05f, 0.08f, 0.96f));

            Rect panel = new Rect(32f, 24f, sw - 64f, sh - 48f);
            DrawPanel(panel, new Color(0.08f, 0.09f, 0.12f, 0.98f), new Color(0.92f, 0.52f, 0.20f, 0.85f));

            GUI.Label(new Rect(panel.x + 24f, panel.y + 16f, 620f, 32f),
                "IN-GAME PHOTOGRAPHY COMMUNITY", _titleStyle);

            if (GUI.Button(new Rect(panel.xMax - 130f, panel.y + 18f, 106f, 34f), "Close [Esc]", _buttonStyle))
            {
                GameStateManager.Instance.CloseOverlayToPreviousState();
            }

            ICommunityDataSource ds = CommunityPhotographyManager.Instance != null
                ? CommunityPhotographyManager.Instance.DataSource
                : null;
            if (ds == null) return;

            // Prominent Offline Demo Banner
            Rect demoBanner = new Rect(panel.x + 24f, panel.y + 56f, panel.width - 48f, 34f);
            DrawPanel(demoBanner, new Color(0.24f, 0.14f, 0.06f, 0.95f), new Color(0.96f, 0.64f, 0.18f, 1f));
            GUI.Label(new Rect(demoBanner.x + 12f, demoBanner.y + 6f, demoBanner.width - 24f, 22f),
                ds.DataSourceStatusBanner, _badgeStyle);

            // Left column: Themed Photography Challenges
            Rect leftCol = new Rect(panel.x + 24f, panel.y + 100f, 310f, panel.height - 120f);
            DrawPanel(leftCol, new Color(0.10f, 0.11f, 0.15f, 0.96f), new Color(0.4f, 0.45f, 0.55f, 0.6f));
            GUI.Label(new Rect(leftCol.x + 14f, leftCol.y + 12f, 282f, 24f), "THEMED PHOTO CHALLENGES", _subtitleStyle);

            float cy = leftCol.y + 42f;
            foreach (var ch in ds.GetActiveChallenges())
            {
                GUI.Label(new Rect(leftCol.x + 14f, cy, 282f, 22f), ch.title, _subtitleStyle);
                GUI.Label(new Rect(leftCol.x + 14f, cy + 22f, 282f, 64f),
                    ch.themeDescription + "\n<b>Region:</b> " + ch.targetRegionName + "\n<b>Badge:</b> " + ch.rewardBadgeTitle,
                    _bodyStyle);
                cy += 102f;
            }

            // Right column: Community Feed Cards
            Rect feedArea = new Rect(leftCol.xMax + 16f, panel.y + 100f, panel.xMax - leftCol.xMax - 40f, panel.height - 120f);
            var posts = ds.GetFeedPosts();
            Rect feedContent = new Rect(0f, 0f, feedArea.width - 24f, posts.Count * 145f + 20f);

            _communityScroll = GUI.BeginScrollView(feedArea, _communityScroll, feedContent);
            for (int i = 0; i < posts.Count; i++)
            {
                CommunityPostEntry post = posts[i];
                Rect card = new Rect(0f, i * 140f, feedContent.width, 130f);
                DrawPanel(card, new Color(0.11f, 0.12f, 0.16f, 0.98f), post.demoCardAccentColor);

                Rect artBox = new Rect(card.x + 12f, card.y + 12f, 180f, 106f);
                if (!post.isSimulatedDemoEntry && post.playerPhotoReference != null &&
                    PhotoJournalManager.Instance != null &&
                    PhotoJournalManager.Instance.TryGetPhotoTexture(post.playerPhotoReference, out Texture2D pTex, out _))
                {
                    GUI.DrawTexture(artBox, pTex, ScaleMode.ScaleAndCrop);
                }
                else
                {
                    DrawRect(artBox, post.demoCardPrimaryColor);
                    DrawRect(new Rect(artBox.x, artBox.yMax - 28f, artBox.width, 28f), post.demoCardAccentColor);
                    GUI.Label(new Rect(artBox.x + 8f, artBox.y + 28f, artBox.width - 16f, 40f),
                        "[DEMO SAMPLE CARD]\n" + post.regionTitle, _badgeStyle);
                }

                float tx = artBox.xMax + 16f;
                float tw = card.width - 216f;
                GUI.Label(new Rect(tx, card.y + 10f, tw - 120f, 22f),
                    post.photographerHandle + "  •  " + post.regionTitle, _subtitleStyle);
                GUI.Label(new Rect(tx, card.y + 34f, tw, 44f), post.caption, _bodyStyle);
                GUI.Label(new Rect(tx, card.y + 82f, tw - 130f, 22f),
                    "Gear: " + post.cameraGearSummary + "  |  Challenge: " + post.challengeTag, _bodyStyle);

                string likeLabel = (post.likedByLocalPlayer ? "★ Appreciated (" : "☆ Appreciate (") + post.likesCount + ")";
                if (GUI.Button(new Rect(card.xMax - 155f, card.y + 82f, 140f, 30f), likeLabel, _buttonStyle))
                {
                    ds.ToggleLikePost(post.postId);
                }
            }
            GUI.EndScrollView();
        }

        private void DrawSettingsModal()
        {
            float sw = Screen.width;
            float sh = Screen.height;

            DrawRect(new Rect(0, 0, sw, sh), new Color(0.03f, 0.03f, 0.05f, 0.85f));
            Rect modal = new Rect(sw * 0.5f - 260f, sh * 0.5f - 190f, 520f, 380f);
            DrawPanel(modal, new Color(0.09f, 0.09f, 0.12f, 0.98f), new Color(0.88f, 0.48f, 0.18f, 0.9f));

            GUI.Label(new Rect(modal.x + 24f, modal.y + 20f, 472f, 30f), "SETTINGS & NAVIGATION", _titleStyle);

            float vol = GameStateManager.Instance.SaveData.masterVolume;
            GUI.Label(new Rect(modal.x + 24f, modal.y + 68f, 240f, 22f), "Master Audio Volume: " + Mathf.RoundToInt(vol * 100f) + "%", _bodyStyle);
            float newVol = GUI.HorizontalSlider(new Rect(modal.x + 250f, modal.y + 74f, 230f, 20f), vol, 0f, 1f);
            if (Mathf.Abs(newVol - vol) > 0.005f)
            {
                GameStateManager.Instance.SaveData.masterVolume = newVol;
                if (ProceduralSoundscapeGenerator.Instance != null)
                    ProceduralSoundscapeGenerator.Instance.SetMasterVolume(newVol);
            }

            float sens = GameStateManager.Instance.SaveData.mouseSensitivity;
            GUI.Label(new Rect(modal.x + 24f, modal.y + 108f, 240f, 22f), "Mouse Look Sensitivity: " + sens.ToString("0.0"), _bodyStyle);
            GameStateManager.Instance.SaveData.mouseSensitivity =
                GUI.HorizontalSlider(new Rect(modal.x + 250f, modal.y + 114f, 230f, 20f), sens, 0.4f, 5.0f);

            float by = modal.y + 160f;
            if (GUI.Button(new Rect(modal.x + 24f, by, 472f, 36f), "Resume / Close Settings [Esc]", _accentButtonStyle))
            {
                GameStateManager.Instance.SaveProgress();
                GameStateManager.Instance.CloseOverlayToPreviousState();
            }
            by += 46f;
            if (GUI.Button(new Rect(modal.x + 24f, by, 472f, 36f), "Return to India Travel Map", _buttonStyle))
            {
                GameStateManager.Instance.SaveProgress();
                GameStateManager.Instance.OpenTravelMap();
            }
            by += 46f;
            if (GUI.Button(new Rect(modal.x + 24f, by, 472f, 36f), "Return to Main Menu", _buttonStyle))
            {
                GameStateManager.Instance.SaveProgress();
                GameStateManager.Instance.ReturnToMainMenu();
            }
        }

        private void DrawPanel(Rect r, Color bg, Color border)
        {
            DrawRect(r, bg);
            DrawOutline(r, border, 1.5f);
        }

        private void DrawRect(Rect r, Color c)
        {
            Color prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, _solidTex);
            GUI.color = prev;
        }

        private void DrawOutline(Rect r, Color c, float thickness)
        {
            DrawRect(new Rect(r.x, r.y, r.width, thickness), c);
            DrawRect(new Rect(r.x, r.yMax - thickness, r.width, thickness), c);
            DrawRect(new Rect(r.x, r.y, thickness, r.height), c);
            DrawRect(new Rect(r.xMax - thickness, r.y, thickness, r.height), c);
        }
    }
}
