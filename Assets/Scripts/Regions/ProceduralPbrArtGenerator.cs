using System.Collections.Generic;
using UnityEngine;
using LensOfLegends.Core;

namespace LensOfLegends.Regions
{
    /// <summary>
    /// Procedural PBR Material and Mesh Generator for North Kerala — Theyyam Sacred Grove.
    /// Synthesizes authentic regional textures (512x512 Mukhathezhuthu facial painting,
    /// porous Malabar laterite stone, carved rosewood timber, terracotta tiles, and Kerala bell-metal bronze)
    /// and custom procedural meshes (lathe-turned Nilavilakku lamps, arched palm fronds, curved trunks,
    /// and the iconic Thiru Mudi &amp; Uduthukettu Theyyam attire).
    /// </summary>
    public static class ProceduralPbrArtGenerator
    {
        private static Material _lateriteEarthMat;
        private static Material _lateriteStoneMat;
        private static Material _carvedRosewoodMat;
        private static Material _terracottaTileMat;
        private static Material _bellMetalBronzeMat;
        private static Material _tropicalFoliageMat;
        private static Material _mukhathezhuthuFaceMat;
        private static Material _thiruMudiHeaddressMat;
        private static Material _uduthukettuSkirtMat;

        private static Mesh _latheNilavilakkuMesh;
        private static Mesh _archedPalmFrondMesh;
        private static Mesh _curvedThiruMudiMesh;
        private static Mesh _pleatedUduthukettuMesh;
        private static readonly Dictionary<string, Mesh> _palmTrunkCache = new Dictionary<string, Mesh>();

        // =========================================================================
        // PBR MATERIAL GENERATION
        // =========================================================================

        public static Material GetLateriteEarthGroundMaterial()
        {
            if (_lateriteEarthMat != null) return _lateriteEarthMat;
            Texture2D tex = GenerateNoiseTexture(256, new Color(0.55f, 0.23f, 0.14f), new Color(0.40f, 0.16f, 0.09f), 0.08f);
            _lateriteEarthMat = ShaderMaterialHelper.CreateLitMaterial("Mat_MalabarLateriteEarth", new Color(0.52f, 0.22f, 0.13f), 0.15f, 0.0f);
            AssignMainTexture(_lateriteEarthMat, tex);
            return _lateriteEarthMat;
        }

        public static Material GetLateriteStoneMaterial()
        {
            if (_lateriteStoneMat != null) return _lateriteStoneMat;
            Texture2D tex = GeneratePorousLateriteTexture(256);
            _lateriteStoneMat = ShaderMaterialHelper.CreateLitMaterial("Mat_PorousLateriteMasonry", new Color(0.58f, 0.26f, 0.16f), 0.18f, 0.0f);
            AssignMainTexture(_lateriteStoneMat, tex);
            return _lateriteStoneMat;
        }

        public static Material GetCarvedRosewoodMaterial()
        {
            if (_carvedRosewoodMat != null) return _carvedRosewoodMat;
            Texture2D tex = GenerateWoodGrainTexture(256, new Color(0.24f, 0.12f, 0.08f), new Color(0.14f, 0.07f, 0.04f));
            _carvedRosewoodMat = ShaderMaterialHelper.CreateLitMaterial("Mat_KeralaCarvedRosewood", new Color(0.22f, 0.11f, 0.07f), 0.45f, 0.0f);
            AssignMainTexture(_carvedRosewoodMat, tex);
            return _carvedRosewoodMat;
        }

        public static Material GetTerracottaTileMaterial()
        {
            if (_terracottaTileMat != null) return _terracottaTileMat;
            Texture2D tex = GenerateTerracottaTileTexture(256);
            _terracottaTileMat = ShaderMaterialHelper.CreateLitMaterial("Mat_TerracottaRoofTile", new Color(0.68f, 0.27f, 0.15f), 0.32f, 0.0f);
            AssignMainTexture(_terracottaTileMat, tex);
            return _terracottaTileMat;
        }

        public static Material GetBellMetalBronzeMaterial()
        {
            if (_bellMetalBronzeMat != null) return _bellMetalBronzeMat;
            _bellMetalBronzeMat = ShaderMaterialHelper.CreateLitMaterial("Mat_KeralaBellMetalBronze", new Color(0.82f, 0.62f, 0.26f), 0.72f, 0.82f);
            return _bellMetalBronzeMat;
        }

        public static Material GetTropicalFoliageMaterial()
        {
            if (_tropicalFoliageMat != null) return _tropicalFoliageMat;
            _tropicalFoliageMat = ShaderMaterialHelper.CreateLitMaterial("Mat_TropicalSacredGroveFoliage", new Color(0.12f, 0.32f, 0.14f), 0.22f, 0.0f);
            return _tropicalFoliageMat;
        }

        public static Material GetMukhathezhuthuFaceMaterial()
        {
            if (_mukhathezhuthuFaceMat != null) return _mukhathezhuthuFaceMat;
            Texture2D tex = GenerateMukhathezhuthuFaceTexture(512);
            _mukhathezhuthuFaceMat = ShaderMaterialHelper.CreateLitMaterial("Mat_Theyyam_Mukhathezhuthu", Color.white, 0.38f, 0.0f);
            AssignMainTexture(_mukhathezhuthuFaceMat, tex);
            return _mukhathezhuthuFaceMat;
        }

        public static Material GetThiruMudiHeaddressMaterial()
        {
            if (_thiruMudiHeaddressMat != null) return _thiruMudiHeaddressMat;
            Texture2D tex = GenerateThiruMudiPatternTexture(256);
            _thiruMudiHeaddressMat = ShaderMaterialHelper.CreateLitMaterial("Mat_Theyyam_ThiruMudi", Color.white, 0.42f, 0.15f);
            AssignMainTexture(_thiruMudiHeaddressMat, tex);
            return _thiruMudiHeaddressMat;
        }

        public static Material GetUduthukettuSkirtMaterial()
        {
            if (_uduthukettuSkirtMat != null) return _uduthukettuSkirtMat;
            Texture2D tex = GenerateUduthukettuPleatTexture(256);
            _uduthukettuSkirtMat = ShaderMaterialHelper.CreateLitMaterial("Mat_Theyyam_Uduthukettu", Color.white, 0.35f, 0.0f);
            AssignMainTexture(_uduthukettuSkirtMat, tex);
            return _uduthukettuSkirtMat;
        }

        private static void AssignMainTexture(Material mat, Texture2D tex)
        {
            if (mat == null || tex == null) return;
            mat.mainTexture = tex;
            if (mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", tex);
        }

        // =========================================================================
        // PROCEDURAL PROCEDURAL TEXTURE SYNTHESIS
        // =========================================================================

        private static Texture2D GenerateNoiseTexture(int size, Color baseCol, Color darkCol, float freq)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * freq, y * freq);
                    colors[y * size + x] = Color.Lerp(darkCol, baseCol, n);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Texture2D GeneratePorousLateriteTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            Color baseRed = new Color(0.62f, 0.27f, 0.17f);
            Color darkPore = new Color(0.28f, 0.11f, 0.06f);
            Color highlight = new Color(0.74f, 0.35f, 0.22f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n1 = Mathf.PerlinNoise(x * 0.06f, y * 0.06f);
                    float n2 = Mathf.PerlinNoise(x * 0.22f, y * 0.22f);
                    Color c = Color.Lerp(baseRed, highlight, n1 * 0.6f);
                    if (n2 < 0.28f)
                    {
                        c = Color.Lerp(darkPore, c, n2 / 0.28f);
                    }
                    colors[y * size + x] = c;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateWoodGrainTexture(int size, Color baseWood, Color darkWood)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float grain = Mathf.Sin((x * 0.18f) + Mathf.PerlinNoise(x * 0.02f, y * 0.08f) * 8f);
                    grain = (grain + 1f) * 0.5f;
                    colors[y * size + x] = Color.Lerp(darkWood, baseWood, grain);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateTerracottaTileTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            Color tileBase = new Color(0.72f, 0.29f, 0.16f);
            Color tileShade = new Color(0.48f, 0.18f, 0.09f);

            for (int y = 0; y < size; y++)
            {
                float verticalRow = Mathf.Repeat(y / (float)size * 8f, 1f);
                float scallop = Mathf.Pow(Mathf.Sin(verticalRow * Mathf.PI), 0.7f);
                for (int x = 0; x < size; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.2f;
                    colors[y * size + x] = Color.Lerp(tileShade, tileBase, scallop + noise);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateMukhathezhuthuFaceTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];

            Color vermilion = new Color(0.82f, 0.12f, 0.08f);
            Color whiteChutti = new Color(0.98f, 0.98f, 0.95f);
            Color kohlBlack = new Color(0.06f, 0.06f, 0.08f);
            Color turmericYellow = new Color(0.96f, 0.76f, 0.18f);

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / size;
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float symX = Mathf.Abs(nx - 0.5f) * 2f; // Symmetrical 0 (center) to 1 (outer)

                    Color c = vermilion;

                    // Central forehead decorative symbol (Thilakam & White Chutti ridges)
                    if (symX < 0.22f && ny > 0.55f && ny < 0.88f)
                    {
                        float band = Mathf.Sin((ny - 0.55f) * 32f);
                        c = band > 0.2f ? whiteChutti : turmericYellow;
                    }

                    // Eye area kohl framing
                    if (symX > 0.18f && symX < 0.52f && ny > 0.42f && ny < 0.58f)
                    {
                        float eyeDist = Vector2.Distance(new Vector2(symX, ny), new Vector2(0.35f, 0.50f));
                        if (eyeDist < 0.09f) c = kohlBlack;
                        else if (eyeDist < 0.13f) c = whiteChutti;
                    }

                    // Lower jaw white rice-paste Chutti boundary
                    if (ny < 0.36f && symX > (0.36f - ny * 0.8f))
                    {
                        c = whiteChutti;
                    }

                    colors[y * size + x] = c;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateThiruMudiPatternTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            Color deepRed = new Color(0.76f, 0.10f, 0.08f);
            Color goldLeaf = new Color(0.92f, 0.76f, 0.25f);
            Color silver = new Color(0.88f, 0.90f, 0.94f);

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / size;
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float stripe = Mathf.Sin(nx * Mathf.PI * 16f);
                    Color c = stripe > 0.5f ? goldLeaf : deepRed;
                    if (Mathf.Repeat(ny * 8f, 1f) < 0.15f)
                    {
                        c = silver;
                    }
                    colors[y * size + x] = c;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateUduthukettuPleatTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            Color crimson = new Color(0.78f, 0.14f, 0.10f);
            Color cream = new Color(0.95f, 0.92f, 0.86f);
            Color goldTrim = new Color(0.90f, 0.72f, 0.24f);

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / size;
                for (int x = 0; x < size; x++)
                {
                    float pleat = Mathf.Repeat(x / (float)size * 24f, 1f);
                    Color c = pleat < 0.5f ? crimson : cream;
                    if (ny < 0.12f || ny > 0.88f)
                    {
                        c = goldTrim;
                    }
                    colors[y * size + x] = c;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        // =========================================================================
        // PROCEDURAL MESH GENERATION
        // =========================================================================

        public static Mesh GetLatheNilavilakkuLampMesh()
        {
            if (_latheNilavilakkuMesh != null) return _latheNilavilakkuMesh;

            // Defined by a lathe radius profile along Y
            float[] ySteps = new float[] { 0.0f, 0.08f, 0.18f, 0.65f, 0.85f, 1.25f, 1.35f, 1.48f, 1.62f, 1.75f };
            float[] rSteps = new float[] { 0.42f, 0.38f, 0.16f, 0.10f, 0.15f, 0.10f, 0.48f, 0.52f, 0.14f, 0.02f };

            _latheNilavilakkuMesh = GenerateLatheMesh(ySteps, rSteps, 16);
            _latheNilavilakkuMesh.name = "Mesh_LatheNilavilakkuLamp";
            return _latheNilavilakkuMesh;
        }

        public static Mesh GetArchedPalmFrondMesh()
        {
            if (_archedPalmFrondMesh != null) return _archedPalmFrondMesh;

            Mesh mesh = new Mesh();
            mesh.name = "Mesh_ArchedPalmFrond";

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            int segments = 12;
            float totalLen = 4.2f;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float z = t * totalLen;
                float y = Mathf.Sin(t * Mathf.PI * 0.85f) * 0.85f - (t * 0.45f);
                float width = Mathf.Sin(t * Mathf.PI) * 0.55f;

                verts.Add(new Vector3(-width, y, z));
                verts.Add(new Vector3(width, y, z));
                uvs.Add(new Vector2(0f, t));
                uvs.Add(new Vector2(1f, t));

                if (i > 0)
                {
                    int baseIdx = (i - 1) * 2;
                    tris.Add(baseIdx);
                    tris.Add(baseIdx + 1);
                    tris.Add(baseIdx + 2);

                    tris.Add(baseIdx + 1);
                    tris.Add(baseIdx + 3);
                    tris.Add(baseIdx + 2);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            _archedPalmFrondMesh = mesh;
            return _archedPalmFrondMesh;
        }

        public static Mesh GetCurvedPalmTrunkMesh(float height, float lean)
        {
            string key = height.ToString("F1") + "_" + lean.ToString("F2");
            if (_palmTrunkCache.TryGetValue(key, out Mesh cached) && cached != null)
                return cached;

            int rings = 10;
            int radialSegments = 10;
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            for (int r = 0; r <= rings; r++)
            {
                float t = (float)r / rings;
                float y = t * height;
                float xOffset = Mathf.Pow(t, 1.4f) * lean;
                float radius = Mathf.Lerp(0.32f, 0.20f, t);

                for (int s = 0; s <= radialSegments; s++)
                {
                    float angle = (float)s / radialSegments * Mathf.PI * 2.0f;
                    float vx = xOffset + Mathf.Cos(angle) * radius;
                    float vz = Mathf.Sin(angle) * radius;

                    verts.Add(new Vector3(vx, y, vz));
                    uvs.Add(new Vector2((float)s / radialSegments, t * 4f));
                }
            }

            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < radialSegments; s++)
                {
                    int curr = r * (radialSegments + 1) + s;
                    int next = curr + radialSegments + 1;

                    tris.Add(curr);
                    tris.Add(next);
                    tris.Add(curr + 1);

                    tris.Add(curr + 1);
                    tris.Add(next);
                    tris.Add(next + 1);
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "Mesh_PalmTrunk_" + key;
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            _palmTrunkCache[key] = mesh;
            return mesh;
        }

        public static Mesh GetCurvedThiruMudiMesh()
        {
            if (_curvedThiruMudiMesh != null) return _curvedThiruMudiMesh;

            Mesh mesh = new Mesh();
            mesh.name = "Mesh_Theyyam_ThiruMudi";

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            int archSteps = 16;
            for (int i = 0; i <= archSteps; i++)
            {
                float t = (float)i / archSteps;
                float angle = Mathf.Lerp(0.08f, Mathf.PI - 0.08f, t);
                float x = Mathf.Cos(angle) * 1.35f;
                float innerY = Mathf.Sin(angle) * 2.2f;
                float outerY = Mathf.Sin(angle) * 3.1f;
                float z = -Mathf.Pow(x / 1.35f, 2f) * 0.35f;

                verts.Add(new Vector3(x, innerY, z));
                verts.Add(new Vector3(x, outerY, z + 0.08f));
                uvs.Add(new Vector2(t, 0f));
                uvs.Add(new Vector2(t, 1f));

                if (i > 0)
                {
                    int baseIdx = (i - 1) * 2;
                    tris.Add(baseIdx);
                    tris.Add(baseIdx + 1);
                    tris.Add(baseIdx + 2);

                    tris.Add(baseIdx + 1);
                    tris.Add(baseIdx + 3);
                    tris.Add(baseIdx + 2);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            _curvedThiruMudiMesh = mesh;
            return _curvedThiruMudiMesh;
        }

        public static Mesh GetPleatedUduthukettuMesh()
        {
            if (_pleatedUduthukettuMesh != null) return _pleatedUduthukettuMesh;

            Mesh mesh = new Mesh();
            mesh.name = "Mesh_Theyyam_Uduthukettu";

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            int pleats = 24;
            for (int i = 0; i <= pleats; i++)
            {
                float angle = (float)i / pleats * Mathf.PI * 2f;
                float pleatWave = (i % 2 == 0) ? 0.09f : -0.09f;

                float topR = 0.42f;
                float botR = 1.15f + pleatWave;

                float tx = Mathf.Cos(angle) * topR;
                float tz = Mathf.Sin(angle) * topR;
                float bx = Mathf.Cos(angle) * botR;
                float bz = Mathf.Sin(angle) * botR;

                verts.Add(new Vector3(tx, 0.45f, tz));
                verts.Add(new Vector3(bx, -0.65f, bz));
                uvs.Add(new Vector2((float)i / pleats, 1f));
                uvs.Add(new Vector2((float)i / pleats, 0f));

                if (i > 0)
                {
                    int baseIdx = (i - 1) * 2;
                    tris.Add(baseIdx);
                    tris.Add(baseIdx + 1);
                    tris.Add(baseIdx + 2);

                    tris.Add(baseIdx + 1);
                    tris.Add(baseIdx + 3);
                    tris.Add(baseIdx + 2);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            _pleatedUduthukettuMesh = mesh;
            return _pleatedUduthukettuMesh;
        }

        private static Mesh GenerateLatheMesh(float[] ySteps, float[] rSteps, int radialSegments)
        {
            Mesh mesh = new Mesh();
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            int rings = ySteps.Length;
            for (int r = 0; r < rings; r++)
            {
                float y = ySteps[r];
                float radius = rSteps[r];

                for (int s = 0; s <= radialSegments; s++)
                {
                    float angle = (float)s / radialSegments * Mathf.PI * 2.0f;
                    float x = Mathf.Cos(angle) * radius;
                    float z = Mathf.Sin(angle) * radius;

                    verts.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2((float)s / radialSegments, (float)r / (rings - 1)));
                }
            }

            for (int r = 0; r < rings - 1; r++)
            {
                for (int s = 0; s < radialSegments; s++)
                {
                    int curr = r * (radialSegments + 1) + s;
                    int next = curr + radialSegments + 1;

                    tris.Add(curr);
                    tris.Add(next);
                    tris.Add(curr + 1);

                    tris.Add(curr + 1);
                    tris.Add(next);
                    tris.Add(next + 1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
