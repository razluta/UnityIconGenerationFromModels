using UnityEngine;
using System.Collections.Generic;

namespace Razluta.UnityIconGenerationFromModels
{
    public enum LightingPresetType
    {
        Custom = 0,
        Studio = 1,
        Dramatic = 2,
        Soft = 3,
        ProductShot = 4,
        Cinematic = 5,
        Technical = 6
    }

    [System.Serializable]
    public class LightingPresetData
    {
        public Vector3 mainLightDirection;
        public Color mainLightColor;
        public float mainLightIntensity;
        public Vector3 fillLightDirection;
        public Color fillLightColor;
        public float fillLightIntensity;
        public List<PointLightSettings> pointLights;

        public LightingPresetData()
        {
            pointLights = new List<PointLightSettings>();
        }
    }

    public static class LightingPresets
    {
        private static Dictionary<LightingPresetType, LightingPresetData> presets;

        static LightingPresets()
        {
            InitializePresets();
        }

        private static void InitializePresets()
        {
            presets = new Dictionary<LightingPresetType, LightingPresetData>();

            // Studio - Balanced, professional lighting
            presets[LightingPresetType.Studio] = new LightingPresetData
            {
                mainLightDirection = new Vector3(-30f, 45f, 0f),
                mainLightColor = Color.white,
                mainLightIntensity = 1.2f,
                fillLightDirection = new Vector3(30f, -15f, 180f),
                fillLightColor = new Color(0.9f, 0.95f, 1f, 1f), // Slightly cool
                fillLightIntensity = 0.4f,
                pointLights = new List<PointLightSettings>
                {
                    new PointLightSettings(new Vector3(1.5f, 1f, -1f), Color.white, 0.8f, 8f),
                    new PointLightSettings(new Vector3(-1.5f, 0.5f, 1f), new Color(1f, 0.9f, 0.8f, 1f), 0.6f, 6f)
                }
            };

            // Dramatic - High contrast with strong shadows
            presets[LightingPresetType.Dramatic] = new LightingPresetData
            {
                mainLightDirection = new Vector3(-60f, 60f, -30f),
                mainLightColor = new Color(1f, 0.95f, 0.9f, 1f), // Warm white
                mainLightIntensity = 2.0f,
                fillLightDirection = new Vector3(45f, -30f, 150f),
                fillLightColor = new Color(0.7f, 0.8f, 1f, 1f), // Cool fill
                fillLightIntensity = 0.2f,
                pointLights = new List<PointLightSettings>
                {
                    new PointLightSettings(new Vector3(2f, 2f, -2f), new Color(1f, 0.8f, 0.6f, 1f), 1.5f, 10f)
                }
            };

            // Soft - Even, diffused lighting
            presets[LightingPresetType.Soft] = new LightingPresetData
            {
                mainLightDirection = new Vector3(-15f, 30f, 0f),
                mainLightColor = Color.white,
                mainLightIntensity = 0.8f,
                fillLightDirection = new Vector3(15f, -15f, 160f),
                fillLightColor = Color.white,
                fillLightIntensity = 0.7f,
                pointLights = new List<PointLightSettings>
                {
                    new PointLightSettings(new Vector3(1f, 1f, 1f), Color.white, 0.5f, 12f),
                    new PointLightSettings(new Vector3(-1f, 1f, 1f), Color.white, 0.5f, 12f),
                    new PointLightSettings(new Vector3(0f, 2f, -1f), Color.white, 0.4f, 10f)
                }
            };

            // Product Shot - Clean, minimal shadows
            presets[LightingPresetType.ProductShot] = new LightingPresetData
            {
                mainLightDirection = new Vector3(0f, 45f, 0f),
                mainLightColor = Color.white,
                mainLightIntensity = 1.0f,
                fillLightDirection = new Vector3(0f, -30f, 180f),
                fillLightColor = Color.white,
                fillLightIntensity = 0.8f,
                pointLights = new List<PointLightSettings>
                {
                    new PointLightSettings(new Vector3(2f, 0f, 0f), Color.white, 0.6f, 8f),
                    new PointLightSettings(new Vector3(-2f, 0f, 0f), Color.white, 0.6f, 8f),
                    new PointLightSettings(new Vector3(0f, 0f, 2f), Color.white, 0.4f, 6f),
                    new PointLightSettings(new Vector3(0f, 0f, -2f), Color.white, 0.4f, 6f)
                }
            };

            // Cinematic - Moody, atmospheric lighting
            presets[LightingPresetType.Cinematic] = new LightingPresetData
            {
                mainLightDirection = new Vector3(-45f, 30f, -60f),
                mainLightColor = new Color(1f, 0.9f, 0.7f, 1f), // Warm orange
                mainLightIntensity = 1.8f,
                fillLightDirection = new Vector3(60f, -45f, 120f),
                fillLightColor = new Color(0.6f, 0.7f, 1f, 1f), // Cool blue
                fillLightIntensity = 0.3f,
                pointLights = new List<PointLightSettings>
                {
                    new PointLightSettings(new Vector3(-2f, 1f, -1f), new Color(1f, 0.6f, 0.3f, 1f), 1.2f, 8f),
                    new PointLightSettings(new Vector3(1f, -0.5f, 2f), new Color(0.3f, 0.6f, 1f, 1f), 0.8f, 12f)
                }
            };

            // Technical - Flat, even lighting for documentation
            presets[LightingPresetType.Technical] = new LightingPresetData
            {
                mainLightDirection = new Vector3(0f, 0f, 0f),
                mainLightColor = Color.white,
                mainLightIntensity = 1.0f,
                fillLightDirection = new Vector3(0f, 0f, 180f),
                fillLightColor = Color.white,
                fillLightIntensity = 1.0f,
                pointLights = new List<PointLightSettings>
                {
                    new PointLightSettings(new Vector3(3f, 0f, 0f), Color.white, 0.8f, 15f),
                    new PointLightSettings(new Vector3(-3f, 0f, 0f), Color.white, 0.8f, 15f),
                    new PointLightSettings(new Vector3(0f, 3f, 0f), Color.white, 0.8f, 15f),
                    new PointLightSettings(new Vector3(0f, -3f, 0f), Color.white, 0.8f, 15f),
                    new PointLightSettings(new Vector3(0f, 0f, 3f), Color.white, 0.8f, 15f),
                    new PointLightSettings(new Vector3(0f, 0f, -3f), Color.white, 0.8f, 15f)
                }
            };
        }

        public static LightingPresetData GetPreset(LightingPresetType presetType)
        {
            if (presets.ContainsKey(presetType))
            {
                return presets[presetType];
            }
            return null;
        }

        public static string GetPresetDisplayName(LightingPresetType presetType)
        {
            switch (presetType)
            {
                case LightingPresetType.Custom: return "Custom";
                case LightingPresetType.Studio: return "Studio";
                case LightingPresetType.Dramatic: return "Dramatic";
                case LightingPresetType.Soft: return "Soft";
                case LightingPresetType.ProductShot: return "Product Shot";
                case LightingPresetType.Cinematic: return "Cinematic";
                case LightingPresetType.Technical: return "Technical";
                default: return "Custom";
            }
        }

        public static LightingPresetType[] GetAllPresetTypes()
        {
            return new LightingPresetType[]
            {
                LightingPresetType.Custom,
                LightingPresetType.Studio,
                LightingPresetType.Dramatic,
                LightingPresetType.Soft,
                LightingPresetType.ProductShot,
                LightingPresetType.Cinematic,
                LightingPresetType.Technical
            };
        }

        public static bool IsLightingMatchingPreset(IconGeneratorSettings settings, LightingPresetType presetType)
        {
            if (presetType == LightingPresetType.Custom) return true;

            var preset = GetPreset(presetType);
            if (preset == null) return false;

            // Check if current lighting matches the preset (with small tolerance for floating point comparison)
            const float tolerance = 0.01f;
            
            if (!Vector3.Distance(settings.mainLightDirection, preset.mainLightDirection).Equals(0f) ||
                !Mathf.Approximately(settings.mainLightIntensity, preset.mainLightIntensity) ||
                !Vector3.Distance(settings.fillLightDirection, preset.fillLightDirection).Equals(0f) ||
                !Mathf.Approximately(settings.fillLightIntensity, preset.fillLightIntensity))
            {
                return false;
            }

            // Check point lights count and basic properties
            if (settings.pointLights.Count != preset.pointLights.Count)
                return false;

            return true;
        }
    }
}