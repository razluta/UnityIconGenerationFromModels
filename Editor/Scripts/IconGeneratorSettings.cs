using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Razluta.UnityIconGenerationFromModels.Editor
{
    [Serializable]
    public class IconGeneratorSettings
    {
        [Header("Basic Settings")]
        public string version = "1.2.0";
        public string outputFolderPath = "";
        public ExportFormat exportFormat = ExportFormat.PNG;
        public int mainIconSize = 512;
        public List<int> additionalSizes = new List<int>();
        
        [Header("Camera Settings")]
        public Vector3 cameraPosition = new Vector3(1.5f, 1.5f, 1.5f);
        public Vector3 cameraRotation = new Vector3(15f, -30f, 0f);
        public float fieldOfView = 30f;
        public Color backgroundColor = new Color(0f, 0f, 0f, 0f);
        
        [Header("Object Transform")]
        public Vector3 objectScale = Vector3.one;
        public Vector3 objectPosition = Vector3.zero;
        public Vector3 objectRotation = Vector3.zero;
        public bool autoCenter = true;
        public bool autoFit = true;
        
        [Header("Lighting")]
        public LightingPreset lightingPreset = LightingPreset.Studio;
        
        // References to classes defined in other files
        public QualitySettings qualitySettings;
        public MultiFolderManager multiFolderManager;
        public LightingConfiguration lightingConfiguration;
        public bool enableReports = true;
        public bool showDetailedProgress = true;
        public bool pauseOnError = false;
        public bool exportConfigurationWithIcons = false;
        public bool enableMemoryOptimization = true;
        
        public IconGeneratorSettings()
        {
            additionalSizes = new List<int> { 64, 128, 256, 1024 };
            qualitySettings = new QualitySettings();
            multiFolderManager = new MultiFolderManager();
            lightingConfiguration = new LightingConfiguration();
        }
        
        public List<string> ValidateSettings()
        {
            var issues = new List<string>();
            
            if (string.IsNullOrEmpty(outputFolderPath))
            {
                issues.Add("Output folder path is required");
            }
            
            if (multiFolderManager != null)
            {
                var folderIssues = multiFolderManager.ValidateConfiguration();
                issues.AddRange(folderIssues);
            }
            
            return issues;
        }
        
        public List<int> GetAllSizes()
        {
            var allSizes = new List<int> { mainIconSize };
            allSizes.AddRange(additionalSizes);
            return allSizes;
        }
        
        public int GetTotalIconCount()
        {
            var prefabCount = multiFolderManager?.GetTotalPrefabCount() ?? 0;
            var sizeCount = GetAllSizes().Count;
            return prefabCount * sizeCount;
        }
        
        public float GetEstimatedProcessingTime()
        {
            var prefabCount = multiFolderManager?.GetTotalPrefabCount() ?? 0;
            var sizeCount = GetAllSizes().Count;
            var totalOperations = prefabCount * sizeCount;
            
            float baseTimePerOperation = qualitySettings?.renderQualityPreset switch
            {
                RenderQualityPreset.Draft => 0.5f,
                RenderQualityPreset.Standard => 1.5f,
                RenderQualityPreset.HighQuality => 4.0f,
                _ => 1.5f
            } ?? 1.5f;
            
            return totalOperations * baseTimePerOperation;
        }
        
        public void ApplyQualityPreset(RenderQualityPreset preset)
        {
            if (qualitySettings != null)
            {
                qualitySettings = QualitySettings.GetPresetConfiguration(preset);
            }
        }
        
        public IconGeneratorSettings Clone()
        {
            var json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<IconGeneratorSettings>(json);
        }
        
        public string GetConfigurationSummary()
        {
            var summary = new System.Text.StringBuilder();
            
            summary.AppendLine($"Quality: {qualitySettings?.renderQualityPreset} ({qualitySettings?.antiAliasingLevel})");
            summary.AppendLine($"Input: {multiFolderManager?.GetSummary()}");
            summary.AppendLine($"Output: {exportFormat}, {GetAllSizes().Count} sizes");
            summary.AppendLine($"Lighting: {lightingPreset}");
            summary.AppendLine($"Estimated time: {GetEstimatedProcessingTime():F1}s");
            
            return summary.ToString();
        }
    }
    
    // Only include enums and classes that aren't defined elsewhere
    [Serializable]
    public enum ExportFormat
    {
        PNG,
        TGA
    }
    
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
    
    // Only include these if they're not defined in other files you've created
    [Serializable]
    public class LightingConfiguration
    {
        public bool enableMainLight = true;
        public Vector3 mainLightDirection = new Vector3(-30f, 50f, -30f);
        public Color mainLightColor = Color.white;
        public float mainLightIntensity = 1.2f;
        
        public bool enableFillLight = true;
        public Vector3 fillLightDirection = new Vector3(30f, 10f, 30f);
        public Color fillLightColor = new Color(0.8f, 0.9f, 1.0f);
        public float fillLightIntensity = 0.4f;
        
        public List<PointLightConfiguration> pointLights = new List<PointLightConfiguration>();
        public Color ambientColor = new Color(0.2f, 0.2f, 0.25f);
        public float ambientIntensity = 0.3f;
        
        public LightingConfiguration()
        {
            if (pointLights == null)
                pointLights = new List<PointLightConfiguration>();
                
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
}