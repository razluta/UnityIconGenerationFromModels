using System;
using System.Collections.Generic;
using UnityEngine;

namespace Razluta.UnityIconGenerationFromModels.Editor
{
    /// <summary>
    /// Complete settings configuration for Unity Icon Generation Tool v1.2.0
    /// Includes all new quality, multi-folder, and reporting features
    /// </summary>
    [Serializable]
    public class IconGeneratorSettings
    {
        [Header("Version Information")]
        public string version = "1.2.0";
        
        [Header("Quality Settings")]
        public QualitySettings qualitySettings = new QualitySettings();
        
        [Header("Multi-Folder Input")]
        public MultiFolderManager multiFolderManager = new MultiFolderManager();
        
        [Header("Output Configuration")]
        public string outputFolderPath = "";
        public ExportFormat exportFormat = ExportFormat.PNG;
        public int mainIconSize = 512;
        public List<int> additionalSizes = new List<int>();
        
        [Header("Camera Settings")]
        public Vector3 cameraPosition = new Vector3(1.5f, 1.5f, 1.5f);
        public Vector3 cameraRotation = new Vector3(15f, -30f, 0f);
        public float fieldOfView = 30f;
        public Color backgroundColor = new Color(0f, 0f, 0f, 0f); // Transparent
        
        [Header("Object Transform")]
        public Vector3 objectScale = Vector3.one;
        public Vector3 objectPosition = Vector3.zero;
        public Vector3 objectRotation = Vector3.zero;
        public bool autoCenter = true;
        public bool autoFit = true;
        
        [Header("Lighting Configuration")]
        public LightingPreset lightingPreset = LightingPreset.Studio;
        public LightingConfiguration lightingConfiguration = new LightingConfiguration();
        
        [Header("Advanced Options")]
        public bool enableReports = true;
        public bool showDetailedProgress = true;
        public bool pauseOnError = false;
        public bool exportConfigurationWithIcons = false;
        
        [Header("Performance")]
        public bool useAsyncProcessing = true;
        public int maxConcurrentOperations = 1;
        public bool enableMemoryOptimization = true;
        
        public IconGeneratorSettings()
        {
            // Initialize with sensible defaults
            version = "1.2.0";
            qualitySettings = new QualitySettings();
            multiFolderManager = new MultiFolderManager();
            lightingConfiguration = new LightingConfiguration();
            
            // Default additional sizes
            additionalSizes = new List<int> { 64, 128, 256, 1024 };
        }
        
        /// <summary>
        /// Validate current settings and return any issues
        /// </summary>
        public List<string> ValidateSettings()
        {
            var issues = new List<string>();
            
            // Validate output folder
            if (string.IsNullOrEmpty(outputFolderPath))
            {
                issues.Add("Output folder path is required");
            }
            else if (!System.IO.Directory.Exists(outputFolderPath))
            {
                issues.Add($"Output folder does not exist: {outputFolderPath}");
            }
            
            // Validate multi-folder configuration
            var folderIssues = multiFolderManager.ValidateConfiguration();
            issues.AddRange(folderIssues);
            
            // Validate icon sizes
            if (mainIconSize <= 0)
            {
                issues.Add("Main icon size must be greater than 0");
            }
            
            foreach (var size in additionalSizes)
            {
                if (size <= 0)
                {
                    issues.Add($"Invalid additional size: {size}");
                }
            }
            
            // Validate camera settings
            if (fieldOfView <= 0 || fieldOfView >= 180)
            {
                issues.Add("Field of view must be between 0 and 180 degrees");
            }
            
            return issues;
        }
        
        /// <summary>
        /// Get all icon sizes (main + additional)
        /// </summary>
        public List<int> GetAllSizes()
        {
            var allSizes = new List<int> { mainIconSize };
            allSizes.AddRange(additionalSizes);
            return allSizes;
        }
        
        /// <summary>
        /// Get total number of icons that will be generated
        /// </summary>
        public int GetTotalIconCount()
        {
            var prefabCount = multiFolderManager.GetTotalPrefabCount();
            var sizeCount = GetAllSizes().Count;
            return prefabCount * sizeCount;
        }
        
        /// <summary>
        /// Get estimated processing time based on current settings
        /// </summary>
        public float GetEstimatedProcessingTime()
        {
            var prefabCount = multiFolderManager.GetTotalPrefabCount();
            var sizeCount = GetAllSizes().Count;
            var totalOperations = prefabCount * sizeCount;
            
            // Base time per operation (seconds)
            float baseTimePerOperation = qualitySettings.renderQualityPreset switch
            {
                RenderQualityPreset.Draft => 0.5f,
                RenderQualityPreset.Standard => 1.5f,
                RenderQualityPreset.HighQuality => 4.0f,
                _ => 1.5f
            };
            
            // Adjust for anti-aliasing
            float aaMultiplier = qualitySettings.antiAliasingLevel switch
            {
                AntiAliasingLevel.None => 0.8f,
                AntiAliasingLevel.MSAA2x => 1.0f,
                AntiAliasingLevel.MSAA4x => 1.2f,
                AntiAliasingLevel.MSAA8x => 1.5f,
                AntiAliasingLevel.MSAA16x => 2.0f,
                _ => 1.0f
            };
            
            // Adjust for render scale
            float scaleMultiplier = Mathf.Pow(qualitySettings.renderScale, 1.5f);
            
            return totalOperations * baseTimePerOperation * aaMultiplier * scaleMultiplier;
        }
        
        /// <summary>
        /// Apply quality preset and update settings accordingly
        /// </summary>
        public void ApplyQualityPreset(RenderQualityPreset preset)
        {
            qualitySettings = QualitySettings.GetPresetConfiguration(preset);
        }
        
        /// <summary>
        /// Clone current settings
        /// </summary>
        public IconGeneratorSettings Clone()
        {
            var json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<IconGeneratorSettings>(json);
        }
        
        /// <summary>
        /// Get summary of current configuration
        /// </summary>
        public string GetConfigurationSummary()
        {
            var summary = new System.Text.StringBuilder();
            
            summary.AppendLine($"Quality: {qualitySettings.renderQualityPreset} ({qualitySettings.antiAliasingLevel})");
            summary.AppendLine($"Input: {multiFolderManager.GetSummary()}");
            summary.AppendLine($"Output: {exportFormat}, {GetAllSizes().Count} sizes");
            summary.AppendLine($"Lighting: {lightingPreset}");
            summary.AppendLine($"Estimated time: {GetEstimatedProcessingTime():F1}s");
            
            return summary.ToString();
        }
    }
    
    /// <summary>
    /// Export format options
    /// </summary>
    [Serializable]
    public enum ExportFormat
    {
        PNG,
        TGA
    }
    
    /// <summary>
    /// Lighting preset enumeration (existing, expanded for v1.2.0)
    /// </summary>
    [Serializable]
    public enum LightingPreset
    {
        Custom,
        Studio,
        Dramatic,
        Soft,
        ProductShot,
        Cinematic,
        Technical
    }
    
    /// <summary>
    /// Point light configuration
    /// </summary>
    [Serializable]
    public class PointLightConfiguration
    {
        public bool enabled = true;
        public Vector3 position = Vector3.zero;
        public Color color = Color.white;
        public float intensity = 1.0f;
        public float range = 10.0f;
        
        public PointLightConfiguration()
        {
            enabled = true;
            position = Vector3.zero;
            color = Color.white;
            intensity = 1.0f;
            range = 10.0f;
        }
        
        public PointLightConfiguration(Vector3 pos, Color col, float inten, float ran)
        {
            enabled = true;
            position = pos;
            color = col;
            intensity = inten;
            range = ran;
        }
    }
    
    /// <summary>
    /// Complete lighting configuration
    /// </summary>
    [Serializable]
    public class LightingConfiguration
    {
        [Header("Main Light")]
        public bool enableMainLight = true;
        public Vector3 mainLightDirection = new Vector3(-30f, 50f, -30f);
        public Color mainLightColor = Color.white;
        public float mainLightIntensity = 1.2f;
        
        [Header("Fill Light")]
        public bool enableFillLight = true;
        public Vector3 fillLightDirection = new Vector3(30f, 10f, 30f);
        public Color fillLightColor = new Color(0.8f, 0.9f, 1.0f);
        public float fillLightIntensity = 0.4f;
        
        [Header("Point Lights")]
        public List<PointLightConfiguration> pointLights = new List<PointLightConfiguration>();
        
        [Header("Environment")]
        public Color ambientColor = new Color(0.2f, 0.2f, 0.25f);
        public float ambientIntensity = 0.3f;
        
        public LightingConfiguration()
        {
            // Initialize with default point lights
            if (pointLights.Count == 0)
            {
                pointLights.Add(new PointLightConfiguration(
                    new Vector3(2f, 2f, 2f), 
                    Color.white, 
                    0.8f, 
                    5f
                ));
            }
        }
        
        public void AddPointLight()
        {
            pointLights.Add(new PointLightConfiguration());
        }
        
        public void RemovePointLight(int index)
        {
            if (index >= 0 && index < pointLights.Count)
            {
                pointLights.RemoveAt(index);
            }
        }
        
        public int GetEnabledPointLightCount()
        {
            return pointLights.Count(light => light.enabled);
        }
    }
}