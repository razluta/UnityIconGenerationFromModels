using System;
using UnityEngine;

namespace Razluta.UnityIconGenerationFromModels.Editor
{
    /// <summary>
    /// Defines render quality presets for icon generation
    /// </summary>
    [Serializable]
    public enum RenderQualityPreset
    {
        Draft = 0,      // Fast rendering for quick previews
        Standard = 1,   // Balanced quality and speed
        HighQuality = 2 // Maximum quality, slower rendering
    }

    /// <summary>
    /// Anti-aliasing level options
    /// </summary>
    [Serializable]
    public enum AntiAliasingLevel
    {
        None = 1,
        MSAA2x = 2,
        MSAA4x = 4,
        MSAA8x = 8,
        MSAA16x = 16
    }

    /// <summary>
    /// Comprehensive quality settings for icon generation
    /// </summary>
    [Serializable]
    public class QualitySettings
    {
        [Header("Quality Presets")]
        public RenderQualityPreset renderQualityPreset = RenderQualityPreset.Standard;
        
        [Header("Anti-Aliasing")]
        public AntiAliasingLevel antiAliasingLevel = AntiAliasingLevel.MSAA8x;
        
        [Header("Advanced Quality Settings")]
        [Range(0.5f, 2.0f)]
        public float renderScale = 1.0f;
        
        [Range(1, 16)]
        public int anisotropicFiltering = 4;
        
        public bool enableHDR = true;
        public bool enableShadows = true;
        
        [Range(256, 4096)]
        public int shadowResolution = 1024;
        
        [Range(0.1f, 10.0f)]
        public float shadowDistance = 5.0f;
        
        /// <summary>
        /// Get quality preset configuration
        /// </summary>
        public static QualitySettings GetPresetConfiguration(RenderQualityPreset preset)
        {
            var settings = new QualitySettings();
            
            switch (preset)
            {
                case RenderQualityPreset.Draft:
                    settings.renderQualityPreset = RenderQualityPreset.Draft;
                    settings.antiAliasingLevel = AntiAliasingLevel.MSAA2x;
                    settings.renderScale = 0.75f;
                    settings.anisotropicFiltering = 1;
                    settings.enableHDR = false;
                    settings.enableShadows = false;
                    settings.shadowResolution = 256;
                    settings.shadowDistance = 2.0f;
                    break;
                    
                case RenderQualityPreset.Standard:
                    settings.renderQualityPreset = RenderQualityPreset.Standard;
                    settings.antiAliasingLevel = AntiAliasingLevel.MSAA8x;
                    settings.renderScale = 1.0f;
                    settings.anisotropicFiltering = 4;
                    settings.enableHDR = true;
                    settings.enableShadows = true;
                    settings.shadowResolution = 1024;
                    settings.shadowDistance = 5.0f;
                    break;
                    
                case RenderQualityPreset.HighQuality:
                    settings.renderQualityPreset = RenderQualityPreset.HighQuality;
                    settings.antiAliasingLevel = AntiAliasingLevel.MSAA16x;
                    settings.renderScale = 1.5f;
                    settings.anisotropicFiltering = 16;
                    settings.enableHDR = true;
                    settings.enableShadows = true;
                    settings.shadowResolution = 2048;
                    settings.shadowDistance = 10.0f;
                    break;
            }
            
            return settings;
        }
        
        /// <summary>
        /// Get performance impact description for current settings
        /// </summary>
        public string GetPerformanceImpactDescription()
        {
            var impact = GetPerformanceImpactLevel();
            
            switch (impact)
            {
                case PerformanceImpact.Low:
                    return "⚡ Fast - Good for quick previews and iterations";
                case PerformanceImpact.Medium:
                    return "⚖️ Balanced - Good quality with reasonable speed";
                case PerformanceImpact.High:
                    return "🐌 Slow - Maximum quality, longer processing time";
                case PerformanceImpact.VeryHigh:
                    return "🔥 Very Slow - Premium quality, significant processing time";
                default:
                    return "📊 Custom settings";
            }
        }
        
        /// <summary>
        /// Calculate performance impact level based on current settings
        /// </summary>
        public PerformanceImpact GetPerformanceImpactLevel()
        {
            int impactScore = 0;
            
            // Anti-aliasing impact
            impactScore += (int)antiAliasingLevel switch
            {
                1 => 0,   // None
                2 => 1,   // 2x
                4 => 2,   // 4x
                8 => 3,   // 8x
                16 => 5,  // 16x
                _ => 2
            };
            
            // Render scale impact
            if (renderScale >= 1.5f) impactScore += 3;
            else if (renderScale >= 1.2f) impactScore += 2;
            else if (renderScale >= 1.0f) impactScore += 1;
            
            // Other settings impact
            if (enableHDR) impactScore += 1;
            if (enableShadows) impactScore += 2;
            if (shadowResolution >= 2048) impactScore += 2;
            else if (shadowResolution >= 1024) impactScore += 1;
            
            if (anisotropicFiltering >= 16) impactScore += 2;
            else if (anisotropicFiltering >= 8) impactScore += 1;
            
            return impactScore switch
            {
                <= 3 => PerformanceImpact.Low,
                <= 6 => PerformanceImpact.Medium,
                <= 10 => PerformanceImpact.High,
                _ => PerformanceImpact.VeryHigh
            };
        }
    }
    
    /// <summary>
    /// Performance impact levels
    /// </summary>
    public enum PerformanceImpact
    {
        Low,
        Medium,
        High,
        VeryHigh
    }
}