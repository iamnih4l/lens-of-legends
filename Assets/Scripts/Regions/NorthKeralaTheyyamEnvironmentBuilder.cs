using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using LensOfLegends.Core;
using LensOfLegends.Photography;

namespace LensOfLegends.Regions
{
    /// <summary>
    /// Constructs the realistic-stylized North Kerala — Theyyam Sacred Grove (Kavu) environment.
    /// Uses ProceduralPbrArtGenerator for 512x512 Mukhathezhuthu face-paint textures, normal-mapped
    /// porous Malabar laterite stone, carved Kerala rosewood, scalloped terracotta roof tiles,
    /// lathe-revolved Nilavilakku bronze lamps, curved coconut palm trunks with arched pinnate
    /// fronds, Chuttuvilakku shrine lamp walls, and the custom-meshed Thiru Mudi & Uduthukettu
    /// Theyyam performer rig.
    /// </summary>
    public class NorthKeralaTheyyamEnvironmentBuilder : MonoBehaviour
    {
        public static NorthKeralaTheyyamEnvironmentBuilder Instance { get; private set; }

        private GameObject _environmentRoot;
        private readonly List<Light> _flickeringFireLights = new List<Light>();
        private readonly List<float> _baseLightIntensities = new List<float>();
        private readonly List<Transform> _swayingPalms = new List<Transform>();

        private void Awake()
        {
            Instance = this;
        }

        public void BuildEnvironment(RegionData keralaData)
        {
            ClearExistingEnvironment();

            _environmentRoot = new GameObject("NorthKerala_Theyyam_SacredGrove_Root");
            _flickeringFireLights.Clear();
            _baseLightIntensities.Clear();
            _swayingPalms.Clear();

            ConfigureNighttimeAtmosphere(keralaData);
            BuildLateriteCourtyardAndWalls(_environmentRoot.transform);
            BuildKavuShrineArchitecture(_environmentRoot.transform);
            BuildMeleriBonfire(_environmentRoot.transform, new Vector3(0f, 0f, 0.8f));
            BuildNilavilakkuBronzeLamps(_environmentRoot.transform);
            BuildKurutholeGarlands(_environmentRoot.transform);
            BuildSacredBanyanAndPalmGrove(_environmentRoot.transform);
            BuildChendaPercussionAreaAndStoryteller(_environmentRoot.transform);
            BuildTheyyamPerformer(_environmentRoot.transform, new Vector3(0f, 0f, 4.2f));
            BuildRegionalPointsOfInterest(_environmentRoot.transform, keralaData);
        }

        public void ClearExistingEnvironment()
        {
            if (_environmentRoot != null)
            {
                Destroy(_environmentRoot);
                _environmentRoot = null;
            }
            if (CulturalDiscoveryManager.Instance != null)
            {
                CulturalDiscoveryManager.Instance.ClearRegionalPois();
            }
        }

        private void ConfigureNighttimeAtmosphere(RegionData data)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = data != null ? data.fogColor : new Color(0.07f, 0.04f, 0.04f);
            RenderSettings.fogDensity = data != null ? data.fogDensity : 0.015f;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.08f, 0.11f, 0.22f);
            RenderSettings.ambientEquatorColor = new Color(0.28f, 0.15f, 0.09f);
            RenderSettings.ambientGroundColor = new Color(0.14f, 0.07f, 0.04f);

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.03f, 0.04f, 0.09f);
            }

            // Cool moonlight directional rim light contrasting warm firelight
            GameObject moonObj = new GameObject("Moonlight_Directional");
            moonObj.transform.SetParent(_environmentRoot.transform, false);
            moonObj.transform.rotation = Quaternion.Euler(40f, -32f, 0f);
            Light moonLight = moonObj.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = new Color(0.42f, 0.56f, 0.82f);
            moonLight.intensity = 0.42f;
            moonLight.shadows = LightShadows.Soft;
        }

        private void BuildLateriteCourtyardAndWalls(Transform parent)
        {
            Material lateriteEarthMat = ProceduralPbrArtGenerator.GetLateriteEarthGroundMaterial();
            Material lateriteStoneWallMat = ProceduralPbrArtGenerator.GetLateriteStoneMaterial();
            Material timberMat = ProceduralPbrArtGenerator.GetCarvedRosewoodMaterial();
            Material terracottaMat = ProceduralPbrArtGenerator.GetTerracottaTileMaterial();

            // Outer ground terrain
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Malabar_Laterite_Terrain";
            ground.transform.SetParent(parent, false);
            ground.transform.position = new Vector3(0f, -0.25f, 4f);
            ground.transform.localScale = new Vector3(95f, 0.5f, 95f);
            ground.GetComponent<Renderer>().sharedMaterial = lateriteEarthMat;

            // Sacred performance arena (Kalam) with stepped stone border ring
            GameObject kalamRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            kalamRing.name = "Sacred_Performance_Ground_Kalam";
            kalamRing.transform.SetParent(parent, false);
            kalamRing.transform.position = new Vector3(0f, 0.02f, 4.0f);
            kalamRing.transform.localScale = new Vector3(14.0f, 0.05f, 14.0f);
            kalamRing.GetComponent<Renderer>().sharedMaterial = lateriteEarthMat;

            // Laterite compound walls (Chuttumatil) with sloped tile coping
            CreateCappedLateriteWall(parent, "Wall_North", new Vector3(0f, 1.15f, 24f), new Vector3(34f, 2.3f, 0.85f), lateriteStoneWallMat, terracottaMat);
            CreateCappedLateriteWall(parent, "Wall_East", new Vector3(17f, 1.15f, 4f), new Vector3(0.85f, 2.3f, 40f), lateriteStoneWallMat, terracottaMat);
            CreateCappedLateriteWall(parent, "Wall_West", new Vector3(-17f, 1.15f, 4f), new Vector3(0.85f, 2.3f, 40f), lateriteStoneWallMat, terracottaMat);
            CreateCappedLateriteWall(parent, "Wall_SouthLeft", new Vector3(-9.8f, 1.15f, -16f), new Vector3(14.5f, 2.3f, 0.85f), lateriteStoneWallMat, terracottaMat);
            CreateCappedLateriteWall(parent, "Wall_SouthRight", new Vector3(9.8f, 1.15f, -16f), new Vector3(14.5f, 2.3f, 0.85f), lateriteStoneWallMat, terracottaMat);

            // Traditional Kerala Padippura entrance gateway arch at South (z = -16)
            GameObject padippura = new GameObject("Padippura_Entrance_Gateway");
            padippura.transform.SetParent(parent, false);
            padippura.transform.position = new Vector3(0f, 0f, -16f);
            CreateBox(padippura.transform, "Pillar_L", new Vector3(-2.3f, 1.6f, 0f), new Vector3(0.65f, 3.2f, 0.85f), lateriteStoneWallMat);
            CreateBox(padippura.transform, "Pillar_R", new Vector3(2.3f, 1.6f, 0f), new Vector3(0.65f, 3.2f, 0.85f), lateriteStoneWallMat);
            CreateSlopedRoofTier(padippura.transform, "Padippura_Roof", new Vector3(0f, 3.45f, 0f), new Vector3(6.4f, 0.28f, 2.4f), terracottaMat, timberMat);
        }

        private void CreateCappedLateriteWall(Transform parent, string name, Vector3 pos, Vector3 scale, Material wallMat, Material roofCapMat)
        {
            GameObject wall = CreateBox(parent, name, pos, scale, wallMat);
            Vector3 capScale = new Vector3(scale.x + 0.2f, 0.22f, scale.z + 0.2f);
            CreateBox(parent, name + "_Coping", pos + new Vector3(0f, scale.y * 0.5f + 0.11f, 0f), capScale, roofCapMat);
        }

        private void BuildKavuShrineArchitecture(Transform parent)
        {
            GameObject shrineGroup = new GameObject("Kavu_Sreekovil_Shrine");
            shrineGroup.transform.SetParent(parent, false);
            shrineGroup.transform.position = new Vector3(0f, 0f, 15.5f);

            Material lateriteMat = ProceduralPbrArtGenerator.GetLateriteStoneMaterial();
            Material timberMat = ProceduralPbrArtGenerator.GetCarvedRosewoodMaterial();
            Material terracottaRoofMat = ProceduralPbrArtGenerator.GetTerracottaTileMaterial();
            Material bronzeMat = ProceduralPbrArtGenerator.GetBellMetalBronzeMaterial();
            Material wickFlameMat = ShaderMaterialHelper.CreateEmissiveMaterial(
                "Mat_ChuttuvilakkuFlame", new Color(1.0f, 0.72f, 0.22f), new Color(1.0f, 0.62f, 0.14f), 3.6f);

            // Stepped granite/laterite Adhishthanam plinth & Sopanam curved balustrade steps
            CreateBox(shrineGroup.transform, "Plinth_Base", new Vector3(0f, 0.35f, 0f), new Vector3(9.6f, 0.7f, 7.6f), lateriteMat);
            CreateBox(shrineGroup.transform, "Plinth_Upper", new Vector3(0f, 0.88f, 0f), new Vector3(8.6f, 0.42f, 6.6f), lateriteMat);
            CreateBox(shrineGroup.transform, "Sopanam_Steps", new Vector3(0f, 0.38f, -3.9f), new Vector3(2.6f, 0.72f, 1.5f), lateriteMat);
            CreateBox(shrineGroup.transform, "Sopanam_Balustrade_L", new Vector3(-1.45f, 0.65f, -3.9f), new Vector3(0.28f, 0.95f, 1.6f), lateriteMat);
            CreateBox(shrineGroup.transform, "Sopanam_Balustrade_R", new Vector3(1.45f, 0.65f, -3.9f), new Vector3(0.28f, 0.95f, 1.6f), lateriteMat);

            // Sanctum walls & carved wooden Azhi lattice frame
            CreateBox(shrineGroup.transform, "Sanctum_Core", new Vector3(0f, 2.35f, 0.4f), new Vector3(6.2f, 2.7f, 4.6f), lateriteMat);
            CreateBox(shrineGroup.transform, "Sanctum_DoorFrame", new Vector3(0f, 2.15f, -1.92f), new Vector3(1.65f, 2.1f, 0.22f), timberMat);

            // Chuttuvilakku: 24 glowing tiny brass lamps mounted along the wooden lattice facade
            for (int row = 0; row < 3; row++)
            {
                float ly = 1.45f + row * 0.65f;
                for (int col = -4; col <= 4; col++)
                {
                    if (Mathf.Abs(col) <= 1) continue; // leave sanctum door open
                    float lx = col * 0.65f;
                    GameObject tinyFlame = CreateSphere(
                        shrineGroup.transform,
                        $"Chuttuvilakku_{row}_{col}",
                        new Vector3(lx, ly, -1.96f),
                        new Vector3(0.08f, 0.14f, 0.08f),
                        wickFlameMat);
                }
            }

            // 6 carved rosewood pillars with capitals across the front Mandapam
            for (int i = -2; i <= 2; i++)
            {
                float x = i * 1.75f;
                CreateCylinder(shrineGroup.transform, "RosewoodPillar_" + i, new Vector3(x, 2.35f, -2.65f), new Vector3(0.28f, 1.38f, 0.28f), timberMat);
                CreateBox(shrineGroup.transform, "PillarCapital_" + i, new Vector3(x, 3.55f, -2.65f), new Vector3(0.62f, 0.22f, 0.62f), timberMat);
            }

            // Double-tiered sloped Kerala gable roof with Kimpurusha carved gable face
            CreateSlopedRoofTier(shrineGroup.transform, "Roof_LowerTier", new Vector3(0f, 3.95f, 0f), new Vector3(10.4f, 0.32f, 5.4f), terracottaRoofMat, timberMat);
            CreateSlopedRoofTier(shrineGroup.transform, "Roof_UpperTier", new Vector3(0f, 5.25f, 0.2f), new Vector3(7.0f, 0.32f, 3.9f), terracottaRoofMat, timberMat);

            // 3 golden Thazhikakudam roof finials
            for (int f = -1; f <= 1; f++)
            {
                CreateCylinder(shrineGroup.transform, "Brass_Finial_" + f, new Vector3(f * 1.8f, 6.15f, 0.2f), new Vector3(0.28f, 0.52f, 0.28f), bronzeMat);
            }

            CreateFlickeringPointLight(shrineGroup.transform, "Sanctum_OilLamp_Light", new Vector3(0f, 2.3f, -2.2f), new Color(1.0f, 0.60f, 0.18f), 3.6f, 15f);

            PhotographyTargetMarker marker = shrineGroup.AddComponent<PhotographyTargetMarker>();
            marker.subjectId = "subj_kavu_shrine";
            marker.displayName = "Kavu Sacred Grove Laterite Shrine & Chuttuvilakku Lamps";
            marker.category = "Architecture";
            marker.associatedPoiId = "poi_kavu_shrine";
        }

        private void CreateSlopedRoofTier(Transform parent, string name, Vector3 centerPos, Vector3 panelScale, Material roofMat, Material gableMat)
        {
            GameObject tier = new GameObject(name);
            tier.transform.SetParent(parent, false);
            tier.transform.localPosition = centerPos;

            GameObject frontSlope = CreateBox(tier.transform, "FrontSlope", new Vector3(0f, 0f, -panelScale.z * 0.38f), panelScale, roofMat);
            frontSlope.transform.localRotation = Quaternion.Euler(27f, 0f, 0f);

            GameObject backSlope = CreateBox(tier.transform, "BackSlope", new Vector3(0f, 0f, panelScale.z * 0.38f), panelScale, roofMat);
            backSlope.transform.localRotation = Quaternion.Euler(-27f, 0f, 0f);

            CreateBox(tier.transform, "RidgeBeam", new Vector3(0f, 0.58f, 0f), new Vector3(panelScale.x * 0.98f, 0.32f, 0.42f), gableMat);
        }

        private void BuildMeleriBonfire(Transform parent, Vector3 position)
        {
            GameObject fireRoot = new GameObject("Meleri_Ceremonial_Bonfire");
            fireRoot.transform.SetParent(parent, false);
            fireRoot.transform.position = position;

            Material stoneRingMat = ProceduralPbrArtGenerator.GetLateriteStoneMaterial();
            Material charWoodMat = ProceduralPbrArtGenerator.GetCarvedRosewoodMaterial();
            Material emberCoreMat = ShaderMaterialHelper.CreateEmissiveMaterial(
                "Mat_GlowingEmbers", new Color(1.0f, 0.32f, 0.04f), new Color(1.0f, 0.42f, 0.08f), 3.8f);

            CreateCylinder(fireRoot.transform, "LateriteFireRing", new Vector3(0f, 0.18f, 0f), new Vector3(2.5f, 0.18f, 2.5f), stoneRingMat);
            CreateCylinder(fireRoot.transform, "EmberBed", new Vector3(0f, 0.32f, 0f), new Vector3(1.95f, 0.14f, 1.95f), emberCoreMat);

            // Stacked coconut-wood & tamarind logs in traditional Meleri arrangement
            for (int i = 0; i < 8; i++)
            {
                GameObject log = CreateCylinder(fireRoot.transform, "FireLog_" + i, new Vector3(0f, 0.46f + (i % 2) * 0.12f, 0f), new Vector3(0.20f, 1.05f, 0.20f), charWoodMat);
                log.transform.localRotation = Quaternion.Euler(72f, i * 45f, 0f);
            }

            CreateFlickeringPointLight(fireRoot.transform, "Meleri_PrimaryFireLight", new Vector3(0f, 1.7f, 0f), new Color(1.0f, 0.52f, 0.15f), 5.8f, 26f);
            CreateEmberAndFlameParticleSystems(fireRoot.transform, new Vector3(0f, 0.45f, 0f));

            PhotographyTargetMarker marker = fireRoot.AddComponent<PhotographyTargetMarker>();
            marker.subjectId = "subj_meleri_fire";
            marker.displayName = "Meleri Ritual Bonfire & Rising Embers";
            marker.category = "Rituals & Firelight";
            marker.associatedPoiId = "poi_nilavilakku_lamps";
        }

        private void BuildNilavilakkuBronzeLamps(Transform parent)
        {
            Material bronzeMat = ProceduralPbrArtGenerator.GetBellMetalBronzeMaterial();
            Mesh latheLampMesh = ProceduralPbrArtGenerator.GetLatheNilavilakkuLampMesh();
            Material wickFlameMat = ShaderMaterialHelper.CreateEmissiveMaterial(
                "Mat_LampWickFlame", new Color(1.0f, 0.76f, 0.26f), new Color(1.0f, 0.68f, 0.18f), 4.2f);

            Vector3[] lampPositions = new Vector3[]
            {
                new Vector3(-4.2f, 0f, 5.2f),
                new Vector3(4.2f, 0f, 5.2f),
                new Vector3(-4.8f, 0f, 0.5f),
                new Vector3(4.8f, 0f, 0.5f),
                new Vector3(-3.0f, 0f, 11.2f),
                new Vector3(3.0f, 0f, 11.2f)
            };

            for (int i = 0; i < lampPositions.Length; i++)
            {
                GameObject lamp = CreateCustomMeshObject(
                    parent, "Nilavilakku_BronzeLamp_" + i, latheLampMesh, bronzeMat, lampPositions[i], Vector3.one, addCollider: true);

                // 7 glowing cotton oil wicks around the Thattu oil bowl (y = 1.42m)
                for (int w = 0; w < 7; w++)
                {
                    float ang = w * Mathf.PI * 2f / 7f;
                    Vector3 wickPos = new Vector3(Mathf.Cos(ang) * 0.31f, 1.44f, Mathf.Sin(ang) * 0.31f);
                    CreateSphere(lamp.transform, "WickFlame_" + w, wickPos, new Vector3(0.07f, 0.16f, 0.07f), wickFlameMat);
                }

                CreateFlickeringPointLight(lamp.transform, "LampLight", new Vector3(0f, 1.62f, 0f), new Color(1.0f, 0.64f, 0.20f), 2.3f, 10f);

                if (i == 0)
                {
                    PhotographyTargetMarker marker = lamp.AddComponent<PhotographyTargetMarker>();
                    marker.subjectId = "subj_nilavilakku";
                    marker.displayName = "Nilavilakku Cast-Bronze Standing Oil Lamp (7-Wick)";
                    marker.category = "Traditional Crafts & Firelight";
                    marker.associatedPoiId = "poi_nilavilakku_lamps";
                }
            }
        }

        private void BuildKurutholeGarlands(Transform parent)
        {
            Material kurutholeMat = ShaderMaterialHelper.CreateLitMaterial(
                "Mat_KurutholePalmFringe", new Color(0.86f, 0.78f, 0.36f), 0.35f, 0.0f);

            Vector3[][] spans = new Vector3[][]
            {
                new Vector3[] { new Vector3(-5.0f, 3.6f, 12.5f), new Vector3(5.0f, 3.6f, 12.5f) },
                new Vector3[] { new Vector3(-5.0f, 3.6f, 12.5f), new Vector3(-8.0f, 3.8f, 9.5f) },
                new Vector3[] { new Vector3(5.0f, 3.6f, 12.5f), new Vector3(6.8f, 3.6f, 6.0f) }
            };

            for (int s = 0; s < spans.Length; s++)
            {
                Vector3 a = spans[s][0];
                Vector3 b = spans[s][1];
                int blades = 18;
                for (int i = 0; i <= blades; i++)
                {
                    float t = (float)i / blades;
                    Vector3 pos = Vector3.Lerp(a, b, t);
                    pos.y -= Mathf.Sin(t * Mathf.PI) * 0.55f;
                    CreateBox(parent, $"KurutholeLeaf_{s}_{i}", pos, new Vector3(0.06f, 0.42f, 0.06f), kurutholeMat);
                }
            }
        }

        private void BuildSacredBanyanAndPalmGrove(Transform parent)
        {
            Material barkMat = ProceduralPbrArtGenerator.GetCarvedRosewoodMaterial();
            Material foliageMat = ProceduralPbrArtGenerator.GetTropicalFoliageMaterial();
            Material stoneAltharaMat = ProceduralPbrArtGenerator.GetLateriteStoneMaterial();
            Mesh frondMesh = ProceduralPbrArtGenerator.GetArchedPalmFrondMesh();

            // Sacred Banyan Tree (Aalmaram) on stepped stone Althara platform
            GameObject banyanGroup = new GameObject("Sacred_Banyan_Tree_Althara");
            banyanGroup.transform.SetParent(parent, false);
            banyanGroup.transform.position = new Vector3(-9.5f, 0f, 11.0f);

            CreateCylinder(banyanGroup.transform, "Althara_Base", new Vector3(0f, 0.35f, 0f), new Vector3(5.6f, 0.35f, 5.6f), stoneAltharaMat);
            CreateCylinder(banyanGroup.transform, "Althara_Upper", new Vector3(0f, 0.75f, 0f), new Vector3(4.8f, 0.25f, 4.8f), stoneAltharaMat);
            CreateCylinder(banyanGroup.transform, "Banyan_MainTrunk", new Vector3(0f, 3.8f, 0f), new Vector3(1.55f, 3.2f, 1.55f), barkMat);

            for (int r = 0; r < 12; r++)
            {
                float ang = r * Mathf.PI * 2f / 12f;
                float rad = 1.35f + (r % 3) * 0.35f;
                Vector3 rootPos = new Vector3(Mathf.Cos(ang) * rad, 2.9f, Mathf.Sin(ang) * rad);
                GameObject propRoot = CreateCylinder(banyanGroup.transform, "AerialRoot_" + r, rootPos, new Vector3(0.14f, 2.5f, 0.14f), barkMat);
                propRoot.transform.localRotation = Quaternion.Euler(Mathf.Sin(ang) * 8f, 0f, Mathf.Cos(ang) * 8f);
            }

            CreateSphere(banyanGroup.transform, "Canopy_Main", new Vector3(0f, 7.4f, 0f), new Vector3(9.2f, 3.8f, 9.2f), foliageMat);
            CreateSphere(banyanGroup.transform, "Canopy_East", new Vector3(3.0f, 6.6f, 1.4f), new Vector3(6.2f, 3.0f, 6.2f), foliageMat);
            CreateSphere(banyanGroup.transform, "Canopy_West", new Vector3(-2.8f, 6.6f, -1.5f), new Vector3(6.2f, 3.0f, 6.2f), foliageMat);

            PhotographyTargetMarker banyanMarker = banyanGroup.AddComponent<PhotographyTargetMarker>();
            banyanMarker.subjectId = "subj_banyan";
            banyanMarker.displayName = "Sacred Banyan Tree (Aalmaram) & Stone Althara";
            banyanMarker.category = "Natural Landscapes";
            banyanMarker.associatedPoiId = "poi_sacred_banyan";

            // Organic Curved Malabar Coconut Palms with Arched Pinnate Fronds
            System.Random rng = new System.Random(2026);
            for (int p = 0; p < 26; p++)
            {
                float angle = p * Mathf.PI * 2f / 26f;
                float radius = 13.6f + (float)rng.NextDouble() * 4.5f;
                Vector3 palmPos = new Vector3(Mathf.Cos(angle) * radius, 0f, 4f + Mathf.Sin(angle) * radius);
                if (palmPos.z < -11f && Mathf.Abs(palmPos.x) < 4.5f) continue;

                float height = 7.2f + (float)rng.NextDouble() * 2.8f;
                float lean = 0.55f + (float)rng.NextDouble() * 0.65f;
                Mesh trunkMesh = ProceduralPbrArtGenerator.GetCurvedPalmTrunkMesh(height, lean);

                GameObject palm = CreateCustomMeshObject(
                    parent, "Malabar_CoconutPalm_" + p, trunkMesh, barkMat, palmPos, Vector3.one, addCollider: false);
                palm.transform.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);

                GameObject crown = new GameObject("PalmCrown");
                crown.transform.SetParent(palm.transform, false);
                crown.transform.localPosition = new Vector3(Mathf.Sin(0.85f * Mathf.PI) * lean, height - 0.15f, 0f);

                for (int f = 0; f < 9; f++)
                {
                    float pitch = (f < 5) ? -14f : 12f;
                    GameObject frond = CreateCustomMeshObject(
                        crown.transform, "PinnateFrond_" + f, frondMesh, foliageMat, Vector3.zero, Vector3.one, addCollider: false);
                    frond.transform.localRotation = Quaternion.Euler(pitch, f * 40f, 0f);
                }
                _swayingPalms.Add(crown.transform);
            }
        }

        private void BuildChendaPercussionAreaAndStoryteller(Transform parent)
        {
            Material drumWoodMat = ProceduralPbrArtGenerator.GetCarvedRosewoodMaterial();
            Material drumHeadMat = ShaderMaterialHelper.CreateLitMaterial("Mat_DrumParchment", new Color(0.90f, 0.84f, 0.72f), 0.3f, 0f);
            Material vestiMat = ShaderMaterialHelper.CreateLitMaterial("Mat_KeralaMundu", new Color(0.94f, 0.91f, 0.84f), 0.18f, 0f);
            Material shawlMat = ShaderMaterialHelper.CreateLitMaterial("Mat_CrimsonPonnada", new Color(0.76f, 0.14f, 0.10f), 0.25f, 0f);
            Material skinMat = ShaderMaterialHelper.CreateLitMaterial("Mat_ArtistSkin", new Color(0.45f, 0.28f, 0.19f), 0.32f, 0f);

            GameObject ensemble = new GameObject("Chenda_Melam_Ensemble_Area");
            ensemble.transform.SetParent(parent, false);
            ensemble.transform.position = new Vector3(6.8f, 0f, 6.0f);

            for (int d = 0; d < 4; d++)
            {
                Vector3 offset = new Vector3((d % 2) * 0.85f, 0f, (d - 1.5f) * 1.45f);
                GameObject drummer = new GameObject("Chenda_Artist_" + d);
                drummer.transform.SetParent(ensemble.transform, false);
                drummer.transform.localPosition = offset;
                drummer.transform.localRotation = Quaternion.Euler(0f, -100f, 0f);

                CreateCylinder(drummer.transform, "Mundu", new Vector3(0f, 0.55f, 0f), new Vector3(0.46f, 0.55f, 0.46f), vestiMat);
                CreateBox(drummer.transform, "Torso", new Vector3(0f, 1.32f, 0f), new Vector3(0.44f, 0.56f, 0.26f), skinMat);
                CreateBox(drummer.transform, "RedSash", new Vector3(0f, 1.35f, 0f), new Vector3(0.48f, 0.18f, 0.30f), shawlMat);
                CreateSphere(drummer.transform, "Head", new Vector3(0f, 1.78f, 0f), new Vector3(0.26f, 0.31f, 0.26f), skinMat);

                GameObject chenda = CreateCylinder(drummer.transform, "Chenda_Barrel", new Vector3(0f, 1.05f, 0.38f), new Vector3(0.32f, 0.36f, 0.32f), drumWoodMat);
                chenda.transform.localRotation = Quaternion.Euler(0f, 0f, 76f);
                CreateCylinder(chenda.transform, "Chenda_ParchmentHead", new Vector3(0f, 1.02f, 0f), new Vector3(1.04f, 0.06f, 1.04f), drumHeadMat);
            }

            PhotographyTargetMarker marker = ensemble.AddComponent<PhotographyTargetMarker>();
            marker.subjectId = "subj_chenda_ensemble";
            marker.displayName = "Chenda Melam & Elathalam Percussion Ensemble";
            marker.category = "Music & Performance";
            marker.associatedPoiId = "poi_chenda_ensemble";

            // Mentor Madhavan Nair near entrance
            GameObject mentor = new GameObject("Storyteller_Madhavan_Nair");
            mentor.transform.SetParent(parent, false);
            mentor.transform.position = new Vector3(-3.2f, 0f, -6.5f);
            mentor.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            CreateCylinder(mentor.transform, "Mundu", new Vector3(0f, 0.55f, 0f), new Vector3(0.46f, 0.55f, 0.46f), vestiMat);
            CreateBox(mentor.transform, "Kurta", new Vector3(0f, 1.30f, 0f), new Vector3(0.46f, 0.58f, 0.28f), vestiMat);
            CreateBox(mentor.transform, "Angavastram", new Vector3(-0.14f, 1.32f, 0.02f), new Vector3(0.18f, 0.56f, 0.32f), shawlMat);
            CreateSphere(mentor.transform, "Head", new Vector3(0f, 1.76f, 0f), new Vector3(0.26f, 0.31f, 0.26f), skinMat);
        }

        private void BuildTheyyamPerformer(Transform parent, Vector3 stagePos)
        {
            GameObject performerRoot = new GameObject("Theyyam_Performer_SacredDancer");
            performerRoot.transform.SetParent(parent, false);
            performerRoot.transform.position = stagePos;

            Material facePaintMat = ProceduralPbrArtGenerator.GetMukhathezhuthuFaceMaterial();
            Material mudiMat = ProceduralPbrArtGenerator.GetThiruMudiHeaddressMaterial();
            Material skirtMat = ProceduralPbrArtGenerator.GetUduthukettuSkirtMaterial();
            Material bronzeGoldMat = ProceduralPbrArtGenerator.GetBellMetalBronzeMaterial();
            Material vermilionBodyMat = ShaderMaterialHelper.CreateEmissiveMaterial(
                "Mat_ChayilyamBodyPaint", new Color(0.88f, 0.16f, 0.06f), new Color(0.65f, 0.10f, 0.03f), 0.30f);
            Material silverTinselMat = ShaderMaterialHelper.CreateLitMaterial(
                "Mat_SilverChuttikuzhal", new Color(0.94f, 0.96f, 0.98f), 0.88f, 0.92f);
            Material torchFlameMat = ShaderMaterialHelper.CreateEmissiveMaterial(
                "Mat_ChootuTorchFlame", new Color(1.0f, 0.68f, 0.18f), new Color(1.0f, 0.58f, 0.10f), 4.5f);

            Mesh mudiArchMesh = ProceduralPbrArtGenerator.GetCurvedThiruMudiMesh();
            Mesh uduthukettuMesh = ProceduralPbrArtGenerator.GetPleatedUduthukettuMesh();

            // 1. Articulated Legs with Chilanka Ankle Bells & Thandayam Ornaments
            GameObject leftLegPivot = BuildArticulatedLeg(performerRoot.transform, "LeftLegPivot", new Vector3(-0.22f, 0.88f, 0f), vermilionBodyMat, bronzeGoldMat);
            GameObject rightLegPivot = BuildArticulatedLeg(performerRoot.transform, "RightLegPivot", new Vector3(0.22f, 0.88f, 0f), vermilionBodyMat, bronzeGoldMat);

            // 2. Torso, Pleated 3D Uduthukettu Waist Garment & Beaded Marathakam Breastplate
            GameObject torsoPivot = new GameObject("TorsoPivot");
            torsoPivot.transform.SetParent(performerRoot.transform, false);
            torsoPivot.transform.localPosition = new Vector3(0f, 1.18f, 0f);

            CreateCustomMeshObject(
                torsoPivot.transform, "Uduthukettu_PleatedSkirt", uduthukettuMesh, skirtMat, new Vector3(0f, -0.12f, 0f), Vector3.one, addCollider: false);
            CreateCylinder(torsoPivot.transform, "Aranjanam_GoldenWaistBelt", new Vector3(0f, 0.24f, 0f), new Vector3(0.92f, 0.09f, 0.78f), bronzeGoldMat);
            CreateCylinder(torsoPivot.transform, "UpperTorso_Chayilyam", new Vector3(0f, 0.56f, 0f), new Vector3(0.64f, 0.36f, 0.44f), vermilionBodyMat);
            CreateSphere(torsoPivot.transform, "Marathakam_BeadedBreastplate", new Vector3(0f, 0.58f, 0.14f), new Vector3(0.76f, 0.52f, 0.34f), bronzeGoldMat);

            // Gilded Shoulder Epaulets (Tholvalayam)
            CreateSphere(torsoPivot.transform, "Tholvalayam_Left", new Vector3(-0.46f, 0.78f, 0f), new Vector3(0.28f, 0.32f, 0.28f), bronzeGoldMat);
            CreateSphere(torsoPivot.transform, "Tholvalayam_Right", new Vector3(0.46f, 0.78f, 0f), new Vector3(0.28f, 0.32f, 0.28f), bronzeGoldMat);

            // 3. Head (with 512x512 Mukhathezhuthu Face Paint), Kadhila Ear-Discs & Curved 3D Thiru Mudi Headdress
            GameObject headAndMudiPivot = new GameObject("HeadAndMudiPivot");
            headAndMudiPivot.transform.SetParent(torsoPivot.transform, false);
            headAndMudiPivot.transform.localPosition = new Vector3(0f, 0.94f, 0f);

            GameObject headSphere = CreateSphere(
                headAndMudiPivot.transform, "Mukhathezhuthu_PaintedFace", new Vector3(0f, 0.16f, 0f), new Vector3(0.38f, 0.45f, 0.40f), facePaintMat);
            headSphere.transform.localRotation = Quaternion.Euler(0f, -90f, 0f); // Align UV center (u=0.5) to forward +Z

            // Protruding gilded ear ornaments (Kadhila) & Kireedam crown base
            CreateCylinder(headAndMudiPivot.transform, "Kadhila_EarDisc_L", new Vector3(-0.26f, 0.16f, 0.02f), new Vector3(0.22f, 0.04f, 0.22f), bronzeGoldMat)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreateCylinder(headAndMudiPivot.transform, "Kadhila_EarDisc_R", new Vector3(0.26f, 0.16f, 0.02f), new Vector3(0.22f, 0.04f, 0.22f), bronzeGoldMat)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreateCylinder(headAndMudiPivot.transform, "Kireedam_CrownBase", new Vector3(0f, 0.42f, 0f), new Vector3(0.58f, 0.12f, 0.58f), bronzeGoldMat);

            // Custom 3D Curved Thiru Mudi Arch Mesh
            CreateCustomMeshObject(
                headAndMudiPivot.transform, "ThiruMudi_CurvedSacredHalo", mudiArchMesh, mudiMat, new Vector3(0f, 0.46f, -0.04f), Vector3.one, addCollider: false);

            // 15 Hanging Silver Chuttikuzhal Tinsel Ornaments around the Thiru Mudi rim
            for (int t = 0; t < 15; t++)
            {
                float ang = Mathf.Lerp(0.05f, Mathf.PI - 0.05f, (float)t / 14f);
                float rx = Mathf.Cos(ang) * 1.24f;
                float ry = 0.46f + Mathf.Sin(ang) * 2.76f;
                float rz = -Mathf.Pow(rx / 1.22f, 2f) * 0.32f + 0.08f;
                CreateSphere(headAndMudiPivot.transform, "Chuttikuzhal_SilverDisc_" + t, new Vector3(rx, ry, rz), new Vector3(0.11f, 0.16f, 0.05f), silverTinselMat);
            }

            // 4. Articulated Arms, Paricha Shield, Kadthala Sword & Flaming Chootu Torches
            GameObject leftArmPivot = new GameObject("LeftArmPivot");
            leftArmPivot.transform.SetParent(torsoPivot.transform, false);
            leftArmPivot.transform.localPosition = new Vector3(-0.48f, 0.72f, 0f);
            CreateCylinder(leftArmPivot.transform, "UpperArm_L", new Vector3(-0.18f, -0.18f, 0.12f), new Vector3(0.15f, 0.26f, 0.15f), vermilionBodyMat);
            CreateCylinder(leftArmPivot.transform, "Forearm_Bangles_L", new Vector3(-0.28f, -0.42f, 0.34f), new Vector3(0.17f, 0.22f, 0.17f), bronzeGoldMat);
            GameObject shield = CreateCylinder(leftArmPivot.transform, "Paricha_CeremonialShield", new Vector3(-0.36f, -0.46f, 0.52f), new Vector3(0.68f, 0.05f, 0.68f), bronzeGoldMat);
            shield.transform.localRotation = Quaternion.Euler(75f, -15f, 0f);

            GameObject rightArmPivot = new GameObject("RightArmPivot");
            rightArmPivot.transform.SetParent(torsoPivot.transform, false);
            rightArmPivot.transform.localPosition = new Vector3(0.48f, 0.72f, 0f);
            CreateCylinder(rightArmPivot.transform, "UpperArm_R", new Vector3(0.18f, -0.18f, 0.12f), new Vector3(0.15f, 0.26f, 0.15f), vermilionBodyMat);
            CreateCylinder(rightArmPivot.transform, "Forearm_Bangles_R", new Vector3(0.28f, -0.38f, 0.36f), new Vector3(0.17f, 0.22f, 0.17f), bronzeGoldMat);

            // Curved ceremonial Kadthala sword & Chootu flaming torch
            GameObject sword = CreateBox(rightArmPivot.transform, "Kadthala_RitualSword", new Vector3(0.34f, -0.05f, 0.62f), new Vector3(0.05f, 0.78f, 0.14f), silverTinselMat);
            sword.transform.localRotation = Quaternion.Euler(-22f, 0f, 0f);
            CreateSphere(rightArmPivot.transform, "Chootu_TorchFlame", new Vector3(0.34f, 0.38f, 0.68f), new Vector3(0.28f, 0.45f, 0.28f), torchFlameMat);

            Light torchLight = CreateFlickeringPointLight(
                rightArmPivot.transform,
                "Performer_TorchLight",
                new Vector3(0.36f, 0.45f, 0.72f),
                new Color(1.0f, 0.58f, 0.16f),
                3.6f,
                14f);

            BoxCollider col = performerRoot.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 2.1f, 0f);
            col.size = new Vector3(2.5f, 4.2f, 1.5f);

            PhotographyTargetMarker marker = performerRoot.AddComponent<PhotographyTargetMarker>();
            marker.subjectId = "subj_theyyam_performer";
            marker.displayName = "Theyyam Performer — Towering Thiru Mudi & Mukhathezhuthu";
            marker.category = "Living Ritual Performance";
            marker.associatedPoiId = "poi_theyyam_performer";

            TheyyamPerformerController anim = performerRoot.AddComponent<TheyyamPerformerController>();
            anim.InitializeRig(
                stagePos,
                torsoPivot.transform,
                headAndMudiPivot.transform,
                leftArmPivot.transform,
                rightArmPivot.transform,
                leftLegPivot.transform,
                rightLegPivot.transform,
                torchLight);
        }

        private GameObject BuildArticulatedLeg(Transform parent, string name, Vector3 localPos, Material skinMat, Material goldMat)
        {
            GameObject pivot = new GameObject(name);
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = localPos;

            CreateCylinder(pivot.transform, "Thigh", new Vector3(0f, -0.24f, 0f), new Vector3(0.23f, 0.24f, 0.23f), skinMat);
            CreateCylinder(pivot.transform, "Calf", new Vector3(0f, -0.62f, 0f), new Vector3(0.19f, 0.24f, 0.19f), skinMat);
            CreateCylinder(pivot.transform, "Chilanka_AnkleBells_Upper", new Vector3(0f, -0.68f, 0f), new Vector3(0.27f, 0.06f, 0.27f), goldMat);
            CreateCylinder(pivot.transform, "Chilanka_AnkleBells_Lower", new Vector3(0f, -0.78f, 0f), new Vector3(0.29f, 0.06f, 0.29f), goldMat);
            return pivot;
        }

        private void BuildRegionalPointsOfInterest(Transform parent, RegionData keralaData)
        {
            if (keralaData == null || keralaData.pointsOfInterest == null) return;

            Material poiBeaconMat = ShaderMaterialHelper.CreateEmissiveMaterial(
                "Mat_PoiBeacon", new Color(1.0f, 0.78f, 0.28f), new Color(1.0f, 0.68f, 0.20f), 2.4f);

            for (int i = 0; i < keralaData.pointsOfInterest.Count; i++)
            {
                CulturalPointOfInterestDefinition def = keralaData.pointsOfInterest[i];
                GameObject poiObj = new GameObject("CulturalPOI_" + def.poiId);
                poiObj.transform.SetParent(parent, false);
                poiObj.transform.position = def.worldPosition;

                GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                beacon.name = "DiscoveryMarkerGem";
                beacon.transform.SetParent(poiObj.transform, false);
                beacon.transform.localPosition = new Vector3(0f, 2.4f, 0f);
                beacon.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
                beacon.transform.localRotation = Quaternion.Euler(45f, 45f, 45f);
                beacon.GetComponent<Renderer>().sharedMaterial = poiBeaconMat;
                Collider beaconCol = beacon.GetComponent<Collider>();
                if (beaconCol != null) Destroy(beaconCol);

                PointOfInterest poiComp = poiObj.AddComponent<PointOfInterest>();
                bool alreadyDiscovered = GameStateManager.Instance != null &&
                                         GameStateManager.Instance.IsPoiDiscovered(def.poiId);
                poiComp.Initialize(def, alreadyDiscovered, beacon.transform);

                if (CulturalDiscoveryManager.Instance != null)
                {
                    CulturalDiscoveryManager.Instance.RegisterPoi(poiComp);
                }
            }
        }

        private void CreateEmberAndFlameParticleSystems(Transform parent, Vector3 localPos)
        {
            // 1. Core Volumetric-Style Fire Plume Particle System
            GameObject flameObj = new GameObject("Meleri_FirePlume_VFX");
            flameObj.transform.SetParent(parent, false);
            flameObj.transform.localPosition = localPos;
            flameObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            ParticleSystem flamePs = flameObj.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule fMain = flamePs.main;
            fMain.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 1.1f);
            fMain.startSpeed = new ParticleSystem.MinMaxCurve(1.1f, 2.4f);
            fMain.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.85f);
            fMain.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1.0f, 0.38f, 0.06f, 0.75f),
                new Color(1.0f, 0.78f, 0.22f, 0.85f));
            fMain.maxParticles = 90;

            ParticleSystem.EmissionModule fEmit = flamePs.emission;
            fEmit.rateOverTime = 42f;

            ParticleSystem.ShapeModule fShape = flamePs.shape;
            fShape.shapeType = ParticleSystemShapeType.Cone;
            fShape.angle = 12f;
            fShape.radius = 0.55f;

            ParticleSystemRenderer fRenderer = flameObj.GetComponent<ParticleSystemRenderer>();
            if (fRenderer != null)
            {
                Material flameMat = new Material(ShaderMaterialHelper.GetParticleAdditiveShader());
                flameMat.color = new Color(1.0f, 0.55f, 0.14f, 0.85f);
                fRenderer.sharedMaterial = flameMat;
            }

            // 2. Rising Glowing Sparks & Embers
            GameObject psObj = new GameObject("RisingEmbers_VFX");
            psObj.transform.SetParent(parent, false);
            psObj.transform.localPosition = localPos + new Vector3(0f, 0.2f, 0f);
            psObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 5.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.9f, 2.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.09f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1.0f, 0.42f, 0.08f, 0.95f),
                new Color(1.0f, 0.86f, 0.28f, 0.95f));
            main.maxParticles = 180;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 36f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 22f;
            shape.radius = 0.85f;

            ParticleSystemRenderer psr = psObj.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                Material emberParticleMat = new Material(ShaderMaterialHelper.GetParticleAdditiveShader());
                emberParticleMat.color = new Color(1.0f, 0.62f, 0.18f, 0.95f);
                psr.sharedMaterial = emberParticleMat;
            }
        }

        private Light CreateFlickeringPointLight(Transform parent, string name, Vector3 localPos, Color color, float intensity, float range)
        {
            GameObject lObj = new GameObject(name);
            lObj.transform.SetParent(parent, false);
            lObj.transform.localPosition = localPos;
            Light l = lObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
            l.shadows = LightShadows.Soft;
            _flickeringFireLights.Add(l);
            _baseLightIntensities.Add(intensity);
            return l;
        }

        private static GameObject CreateCustomMeshObject(
            Transform parent, string name, Mesh mesh, Material mat, Vector3 localPos, Vector3 scale, bool addCollider)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            if (addCollider && mesh != null)
            {
                BoxCollider bc = go.AddComponent<BoxCollider>();
                bc.center = mesh.bounds.center;
                bc.size = mesh.bounds.size;
            }
            return go;
        }

        private static GameObject CreateBox(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private static GameObject CreateSphere(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private void Update()
        {
            float t = Time.time;
            for (int i = 0; i < _flickeringFireLights.Count; i++)
            {
                Light l = _flickeringFireLights[i];
                if (l == null) continue;
                float flicker = 0.86f + Mathf.PerlinNoise(t * 7.2f + i * 3.1f, i * 1.7f) * 0.28f;
                l.intensity = _baseLightIntensities[i] * flicker;
            }

            for (int i = 0; i < _swayingPalms.Count; i++)
            {
                Transform crown = _swayingPalms[i];
                if (crown == null) continue;
                float sway = Mathf.Sin(t * 1.35f + i * 0.8f) * 2.8f;
                crown.localRotation = Quaternion.Euler(sway, 0f, sway * 0.65f);
            }
        }
    }
}
