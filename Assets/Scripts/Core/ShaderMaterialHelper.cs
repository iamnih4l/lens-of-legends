using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace LensOfLegends.Core
{
    /// <summary>
    /// Provides render-pipeline-agnostic Material creation so the game renders cleanly
    /// whether the Unity project is opened in Universal Render Pipeline (URP) or the
    /// Built-in Render Pipeline. Never produces magenta/broken shaders.
    /// </summary>
    public static class ShaderMaterialHelper
    {
        private static Shader _cachedLitShader;
        private static Shader _cachedParticleShader;

        public static bool IsUsingURP()
        {
            RenderPipelineAsset rp = GraphicsSettings.currentRenderPipeline;
            if (rp == null) rp = QualitySettings.renderPipeline;
            return rp != null && rp.GetType().Name.Contains("Universal");
        }

        public static Shader GetLitShader()
        {
            if (_cachedLitShader != null) return _cachedLitShader;

            if (IsUsingURP())
            {
                _cachedLitShader = Shader.Find("Universal Render Pipeline/Lit");
                if (_cachedLitShader == null)
                    _cachedLitShader = Shader.Find("Universal Render Pipeline/Simple Lit");
            }

            if (_cachedLitShader == null)
                _cachedLitShader = Shader.Find("Standard");

            if (_cachedLitShader == null)
                _cachedLitShader = Shader.Find("Legacy Shaders/Diffuse");

            return _cachedLitShader;
        }

        public static Shader GetParticleAdditiveShader()
        {
            if (_cachedParticleShader != null) return _cachedParticleShader;

            if (IsUsingURP())
            {
                _cachedParticleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            }

            if (_cachedParticleShader == null)
                _cachedParticleShader = Shader.Find("Particles/Standard Unlit");

            if (_cachedParticleShader == null)
                _cachedParticleShader = Shader.Find("Legacy Shaders/Particles/Additive");

            if (_cachedParticleShader == null)
                _cachedParticleShader = GetLitShader();

            return _cachedParticleShader;
        }

        public static Material CreateLitMaterial(string name, Color albedo, float smoothness = 0.25f, float metallic = 0.0f)
        {
            Material mat = new Material(GetLitShader());
            mat.name = name;
            SetColorAndSurface(mat, albedo, smoothness, metallic);
            return mat;
        }

        public static Material CreateEmissiveMaterial(string name, Color baseColor, Color emissionColor, float intensity = 2.0f)
        {
            Material mat = CreateLitMaterial(name, baseColor, 0.5f, 0.1f);
            mat.EnableKeyword("_EMISSION");
            Color finalEmission = emissionColor * intensity;
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", finalEmission);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            return mat;
        }

        public static void SetColorAndSurface(Material mat, Color albedo, float smoothness, float metallic)
        {
            if (mat == null) return;
            mat.color = albedo;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", albedo);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", albedo);
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness"))
                mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", metallic);
        }
    }
}
