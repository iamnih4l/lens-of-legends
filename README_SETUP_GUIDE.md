# Lens of Legends — Unity Implementation & Developer Guide

**Project Title:** Lens of Legends: An Open-World Indian Cultural Photography Adventure

**Engine Target:** Unity 2022.3 LTS / Unity 6 (URP & Built-In Render Pipeline Compatible)

**Playable Chapter I:** North Kerala — Theyyam in the Sacred Grove (`Theyyam_Ritual_Scene.unity`)

**India-Wide Structured Preview Chapters:** Varanasi, Rajasthan, Kolkata, Nagaland, Ladakh

## 1. Architecture & Self-Bootstrapping Design

To ensure a smooth experience for a beginner in Unity without fragile manual Inspector wiring:

1. **Zero-Config Runtime Bootstrapper (`LensOfLegendsBootstrapper.cs`)**: Uses `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]` to automatically instantiate and wire all core managers (`GameStateManager`, `PhotographySystem`, `PhotoJournalManager`, `TravelMapController`, `CulturalDiscoveryManager`, `StoryAssignmentSystem`, `CommunityPhotographyManager`, `CinematicDirector`, `ProceduralSoundscapeGenerator`, and `LensOfLegendsUIManager`) the moment you press **Play** in any scene.

2. **Render-Pipeline-Agnostic Shader Helper (`ShaderMaterialHelper.cs`)**: Automatically detects whether your Unity project uses **Universal Render Pipeline (URP)** (`Universal Render Pipeline/Lit`) or the **Built-In Render Pipeline** (`Standard`) and assigns material properties (`_BaseColor`, `_Color`, `_Smoothness`, `_Glossiness`, `_EmissionColor`) accordingly so materials never appear magenta.

3. **One-Click Unity Editor Utility (`LensOfLegendsEditorSetup.cs`)**: Adds a custom top-menu item in Unity (`Lens of Legends > 1. Generate & Wire All Scenes, ScriptableObjects & Build Settings`) that generates native `.asset` files for all 6 `RegionData` chapters and 4 `CameraProfile` equipment profiles, and registers `MainMenu.unity`, `TravelMap.unity`, and `Theyyam_Ritual_Scene.unity` in `EditorBuildSettings`.

## 2. Project File Structure Created

| Path | Responsibility | 
| ----- | ----- | 
| `Packages/manifest.json` | Declares Unity UGUI, Audio, ImageConversion, JSONSerialize, ParticleSystem, Physics, ScreenCapture, and Video modules. | 
| `ProjectSettings/ProjectSettings.asset` | Configures `activeInputHandler: 2` (Both Legacy & New Input), Linear Color Space, and 1920x1080 default resolution. | 
| `ProjectSettings/EditorBuildSettings.asset` | Registers `MainMenu.unity` (0), `TravelMap.unity` (1), and `Theyyam_Ritual_Scene.unity` (2). | 
| `Assets/Scenes/MainMenu.unity` | Main Menu entry scene with live 3D sacred grove background. | 
| `Assets/Scenes/TravelMap.unity` | Interactive India-wide Travel Map scene. | 
| `Assets/Scenes/Regions/Theyyam_Ritual_Scene.unity` | Playable Chapter I: North Kerala — Theyyam Sacred Grove (`Kavu`) scene. | 
| `Assets/Scripts/Core/LensOfLegendsBootstrapper.cs` | Auto-initializes all game systems on Play. | 
| `Assets/Scripts/Core/GameStateManager.cs` | Manages the end-to-end state machine, scene transitions, XP, badges, and camera unlocks. | 
| `Assets/Scripts/Core/SaveSystem.cs` | Local JSON persistence in `Application.persistentDataPath` + `LensOfLegends_Photos/` directory management. | 
| `Assets/Scripts/Core/ShaderMaterialHelper.cs` | URP and Built-In shader compatibility layer. | 
| `Assets/Scripts/Core/CinematicDirector.cs` | In-engine introductory camera sequence and regional arrival sweep + optional `VideoPlayer` hook. | 
| `Assets/Scripts/Core/ProceduralSoundscapeGenerator.cs` | Synthesizes regional night crickets, wind, crackling fire, Chenda-inspired percussion pulse, footsteps, and mechanical shutter click. | 
| `Assets/Scripts/Regions/RegionData.cs` | `ScriptableObject` definition for Indian regional destinations and Cultural Points of Interest. | 
| `Assets/Scripts/Regions/IndiaDestinationCatalog.cs` | Defines all 6 destinations across India with verified cultural background and distinct lighting palettes. | 
| `Assets/Scripts/Regions/NorthKeralaTheyyamEnvironmentBuilder.cs` | Builds the Kavu shrine, laterite walls, Meleri bonfire, Nilavilakku bronze lamps, banyan & palm grove, Chenda ensemble, and POIs. | 
| `Assets/Scripts/Regions/TheyyamPerformerController.cs` | Animates the articulated procedural Theyyam performer through ritual phases before the fire. | 
| `Assets/Scripts/Regions/PointOfInterest.cs` | World-space discoverable cultural landmark component. | 
| `Assets/Scripts/Regions/CulturalDiscoveryManager.cs` | Proximity and camera-based discovery system separating verified cultural facts from narrative notes. | 
| `Assets/Scripts/Travel/TravelMapController.cs` | Destination selection and preview chapter safeguards. | 
| `Assets/Scripts/Player/FirstPersonExplorerController.cs` | WASD + Mouse look first-person controller with collision and footsteps. | 
| `Assets/Scripts/Camera/CameraProfile.cs` | 4 distinct camera bodies (`Yatra Compact DX-35`, `Drishti Pro M-IV`, `Chhaya-76 Rangefinder`, and unlockable `Kala-Rasa 6x6`). | 
| `Assets/Scripts/Photography/PhotographySettings.cs` | Optical FOV, exposure EV, Circle-of-Confusion, and white balance calculations. | 
| `Assets/Scripts/Photography/PhotographySystem.cs` | Dedicated camera mode, live raycast subject recognition, slow-shutter frame buffer, `RenderTexture` capture, optical pixel pipeline, and PNG encoding. | 
| `Assets/Scripts/Gallery/PhotoMetadata.cs` | Serializable metadata for captured photos and player save state. | 
| `Assets/Scripts/Gallery/PhotoJournalManager.cs` | Persistent local gallery loader with graceful missing/corrupted file handling. | 
| `Assets/Scripts/Story/StoryAssignmentSystem.cs` | Optional storyteller photography inspirations and milestone badges. | 
| `Assets/Scripts/Community/ICommunityDataSource.cs` | Clean interface & `OfflineDemoCommunityDataSource` clearly labeled as an offline demonstration feed. | 
| `Assets/Scripts/Community/CommunityPhotographyManager.cs` | Publishes player captures locally to the offline community demo feed. | 
| `Assets/Scripts/UI/LensOfLegendsUIManager.cs` | Complete UI for Main Menu, Travel Map, Exploration HUD, Camera Viewfinder, Photo Preview, Photo Journal, and Community. | 
| `Assets/Scripts/Editor/LensOfLegendsEditorSetup.cs` | One-click Unity Editor setup menu utility. | 

## 3. Exact Instructions to Open and Run the Game in Unity

1. Open **Unity Hub** and click **Add > Add project from disk**, selecting the project root folder containing `Assets/`, `Packages/`, and `ProjectSettings/` (recommended Unity version: **Unity 2022.3 LTS** or **Unity 6**).

2. Once the Unity Editor finishes importing scripts, click the top menu bar:

   **`Lens of Legends > 1. Generate & Wire All Scenes, ScriptableObjects & Build Settings`**

   *(Optional convenience step: even if you skip this menu, `LensOfLegendsBootstrapper` will automatically initialize the entire game when you press Play in any scene).*

3. Open `Assets/Scenes/MainMenu.unity` (or click **`Lens of Legends > 2. Open MainMenu Scene`**) and press the **Play** button at the top of the Unity Editor.

4. **Controls in Play Mode**:

   * **Move**: `W`, `A`, `S`, `D` (Hold `Left Shift` to walk faster)

   * **Look / Aim**: Move Mouse (Hold `Alt` at any time to free the cursor for clicking HUD buttons)

   * **Enter / Exit Camera Mode**: `Right Mouse Button` or `V`

   * **Capture Photograph**: `Left Mouse Button`, `Space`, or `Enter` (while in Camera Mode)

   * **Zoom (Focal Length)**: `Mouse Scroll Wheel` or `Up/Down Arrows`

   * **Switch Camera Equipment Profile**: Keys `1`, `2`, `3`, `4`

   * **Adjust Aperture (Simulated DoF)**: `Q` / `E`

   * **Toggle Autofocus / Manual Focus**: `F` (Manual focus distance: `[` and `]`)

   * **Exposure Compensation (-2.0 to +2.0 EV)**: `-` and `=`

   * **Cycle ISO (100–6400)**: `I`

   * **Cycle Shutter Speed (1/30s–1/1000s)**: `O`

   * **Cycle White Balance / Color Profile**: `C`

   * **Toggle Rule-of-Thirds Grid**: `G`

   * **Inspect Nearby Cultural Point of Interest**: `E`

   * **Open Personal Photo Journal**: `J`

   * **Open Community Demo Feed**: `K`

   * **Open India Travel Map**: `M`