using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Razluta.UnityIconGenerationFromModels
{
    [System.Serializable]
    public class ConfigurationPreset
    {
        [Header("Metadata")]
        public string presetName = "Untitled Configuration";
        public string description = "";
        public string createdBy = "";
        public string dateCreated = "";
        public string version = "1.0";

        [Header("Input Settings")]
        public string inputFolderPath = "Assets/Prefabs";
        public string prefabNamePrefix = "Item_";
        
        [Header("Output Settings")]
        public string outputFolderPath = "Assets/GeneratedIcons";
        public int iconWidth = 256;
        public int iconHeight = 256;
        
        [Header("Camera Settings")]
        public Vector3 cameraPosition = new Vector3(0, 0, -3);
        public Vector3 cameraRotation = new Vector3(0, 0, 0);
        public float cameraFOV = 60f;
        public Color backgroundColor = Color.clear;
        
        [Header("Lighting Settings")]
        public LightingPresetType lightingPresetType = LightingPresetType.Custom;
        public Vector3 mainLightDirection = new Vector3(-30, 30, 0);
        public Color mainLightColor = Color.white;
        public float mainLightIntensity = 1f;
        public Vector3 fillLightDirection = new Vector3(30, -30, 180);
        public Color fillLightColor = Color.white;
        public float fillLightIntensity = 0.5f;
        public List<PointLightSettings> pointLights = new List<PointLightSettings>();
        
        [Header("Advanced Settings")]
        public float objectScale = 1f;
        public Vector3 objectPosition = Vector3.zero;
        public Vector3 objectRotation = Vector3.zero;
        public bool autoCenter = true;
        public bool autoFit = true;

        public static ConfigurationPreset FromSettings(IconGeneratorSettings settings)
        {
            var preset = new ConfigurationPreset();
            
            // Metadata
            preset.dateCreated = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            preset.createdBy = System.Environment.UserName;
            
            // Copy all settings
            preset.inputFolderPath = settings.inputFolderPath;
            preset.prefabNamePrefix = settings.prefabNamePrefix;
            preset.outputFolderPath = settings.outputFolderPath;
            preset.iconWidth = settings.iconWidth;
            preset.iconHeight = settings.iconHeight;
            
            preset.cameraPosition = settings.cameraPosition;
            preset.cameraRotation = settings.cameraRotation;
            preset.cameraFOV = settings.cameraFOV;
            preset.backgroundColor = settings.backgroundColor;
            
            preset.lightingPresetType = settings.currentLightingPreset;
            preset.mainLightDirection = settings.mainLightDirection;
            preset.mainLightColor = settings.mainLightColor;
            preset.mainLightIntensity = settings.mainLightIntensity;
            preset.fillLightDirection = settings.fillLightDirection;
            preset.fillLightColor = settings.fillLightColor;
            preset.fillLightIntensity = settings.fillLightIntensity;
            
            // Deep copy point lights
            preset.pointLights = new List<PointLightSettings>();
            foreach (var pointLight in settings.pointLights)
            {
                preset.pointLights.Add(new PointLightSettings(
                    pointLight.position,
                    pointLight.color,
                    pointLight.intensity,
                    pointLight.range
                ) { enabled = pointLight.enabled });
            }
            
            preset.objectScale = settings.objectScale;
            preset.objectPosition = settings.objectPosition;
            preset.objectRotation = settings.objectRotation;
            preset.autoCenter = settings.autoCenter;
            preset.autoFit = settings.autoFit;
            
            return preset;
        }

        public void ApplyToSettings(IconGeneratorSettings settings)
        {
            // Don't copy input/output paths - those are project-specific
            // settings.inputFolderPath = inputFolderPath;
            // settings.outputFolderPath = outputFolderPath;
            
            settings.prefabNamePrefix = prefabNamePrefix;
            settings.iconWidth = iconWidth;
            settings.iconHeight = iconHeight;
            
            settings.cameraPosition = cameraPosition;
            settings.cameraRotation = cameraRotation;
            settings.cameraFOV = cameraFOV;
            settings.backgroundColor = backgroundColor;
            
            settings.currentLightingPreset = lightingPresetType;
            settings.mainLightDirection = mainLightDirection;
            settings.mainLightColor = mainLightColor;
            settings.mainLightIntensity = mainLightIntensity;
            settings.fillLightDirection = fillLightDirection;
            settings.fillLightColor = fillLightColor;
            settings.fillLightIntensity = fillLightIntensity;
            
            // Deep copy point lights
            settings.pointLights.Clear();
            foreach (var pointLight in pointLights)
            {
                settings.pointLights.Add(new PointLightSettings(
                    pointLight.position,
                    pointLight.color,
                    pointLight.intensity,
                    pointLight.range
                ) { enabled = pointLight.enabled });
            }
            
            settings.objectScale = objectScale;
            settings.objectPosition = objectPosition;
            settings.objectRotation = objectRotation;
            settings.autoCenter = autoCenter;
            settings.autoFit = autoFit;
            
            settings.SaveToPrefs();
        }
    }

    public static class ConfigurationPresetsManager
    {
        private static readonly string DefaultPresetsFolder = "Assets/IconGeneratorConfigurations";

        public static bool SaveConfiguration(ConfigurationPreset preset, string filePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    // Ensure default folder exists
                    if (!Directory.Exists(DefaultPresetsFolder))
                    {
                        Directory.CreateDirectory(DefaultPresetsFolder);
                        AssetDatabase.Refresh();
                    }

                    filePath = EditorUtility.SaveFilePanel(
                        "Save Configuration Preset",
                        DefaultPresetsFolder,
                        preset.presetName,
                        "json"
                    );
                }

                if (string.IsNullOrEmpty(filePath))
                    return false;

                string json = JsonUtility.ToJson(preset, true);
                File.WriteAllText(filePath, json);

                // Refresh AssetDatabase if file is in project
                if (filePath.StartsWith(Application.dataPath))
                {
                    AssetDatabase.Refresh();
                }

                Debug.Log($"Configuration saved to: {filePath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save configuration: {e.Message}");
                EditorUtility.DisplayDialog("Save Error", $"Failed to save configuration:\n{e.Message}", "OK");
                return false;
            }
        }

        public static ConfigurationPreset LoadConfiguration(string filePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = EditorUtility.OpenFilePanel(
                        "Load Configuration Preset",
                        DefaultPresetsFolder,
                        "json"
                    );
                }

                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return null;

                string json = File.ReadAllText(filePath);
                var preset = JsonUtility.FromJson<ConfigurationPreset>(json);

                Debug.Log($"Configuration loaded from: {filePath}");
                return preset;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load configuration: {e.Message}");
                EditorUtility.DisplayDialog("Load Error", $"Failed to load configuration:\n{e.Message}", "OK");
                return null;
            }
        }

        public static void ShowConfigurationInfo(ConfigurationPreset preset)
        {
            if (preset == null) return;

            var message = $"Configuration: {preset.presetName}\n\n" +
                         $"Description: {preset.description}\n" +
                         $"Created by: {preset.createdBy}\n" +
                         $"Date: {preset.dateCreated}\n" +
                         $"Version: {preset.version}\n\n" +
                         $"Lighting Preset: {LightingPresets.GetPresetDisplayName(preset.lightingPresetType)}\n" +
                         $"Point Lights: {preset.pointLights.Count}\n" +
                         $"Icon Size: {preset.iconWidth}x{preset.iconHeight}\n" +
                         $"Auto Center: {preset.autoCenter}\n" +
                         $"Auto Fit: {preset.autoFit}";

            EditorUtility.DisplayDialog("Configuration Info", message, "OK");
        }

        public static List<string> GetRecentConfigurations()
        {
            var recentFiles = new List<string>();
            
            if (Directory.Exists(DefaultPresetsFolder))
            {
                var files = Directory.GetFiles(DefaultPresetsFolder, "*.json");
                foreach (var file in files)
                {
                    recentFiles.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            
            return recentFiles;
        }
    }
}