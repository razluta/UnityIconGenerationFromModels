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
            return new List<string>();
        }
        
        public List<int> GetAllSizes()
        {
            var allSizes = new List<int> { mainIconSize };
            allSizes.AddRange(additionalSizes);
            return allSizes;
        }
        
        public int GetTotalIconCount()
        {
            return GetAllSizes().Count;
        }
        
        public float GetEstimatedProcessingTime()
        {
            return 10.0f;
        }
        
        public void ApplyQualityPreset(RenderQualityPreset preset)
        {
            // Placeholder
        }
        
        public IconGeneratorSettings Clone()
        {
            return new IconGeneratorSettings();
        }
        
        public string GetConfigurationSummary()
        {
            return "Configuration summary";
        }
    }
    
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
    
    [Serializable]
    public enum RenderQualityPreset
    {
        Draft = 0,
        Standard = 1,
        HighQuality = 2
    }
    
    [Serializable]
    public enum AntiAliasingLevel
    {
        None = 1,
        MSAA2x = 2,
        MSAA4x = 4,
        MSAA8x = 8,
        MSAA16x = 16
    }
    
    [Serializable]
    public class QualitySettings
    {
        public RenderQualityPreset renderQualityPreset = RenderQualityPreset.Standard;
        public AntiAliasingLevel antiAliasingLevel = AntiAliasingLevel.MSAA8x;
        public float renderScale = 1.0f;
        
        public static QualitySettings GetPresetConfiguration(RenderQualityPreset preset)
        {
            return new QualitySettings();
        }
        
        public string GetPerformanceImpactDescription()
        {
            return "Standard quality";
        }
    }
    
    [Serializable]
    public class MultiFolderManager
    {
        public List<InputFolderConfiguration> InputFolders = new List<InputFolderConfiguration>();
        
        public MultiFolderManager()
        {
            if (InputFolders.Count == 0)
            {
                InputFolders.Add(new InputFolderConfiguration());
            }
        }
        
        public void AddFolder() 
        { 
            InputFolders.Add(new InputFolderConfiguration());
        }
        
        public void RemoveFolder(int index) 
        { 
            if (index >= 0 && index < InputFolders.Count && InputFolders.Count > 1)
            {
                InputFolders.RemoveAt(index);
            }
        }
        
        public List<InputFolderConfiguration> GetValidFolders() 
        { 
            return InputFolders.Where(f => f.IsValid()).ToList();
        }
        
        public int GetTotalPrefabCount() 
        { 
            return GetValidFolders().Sum(f => f.GetPrefabs().Count);
        }
        
        public string GetSummary() 
        { 
            var count = GetTotalPrefabCount();
            return count > 0 ? $"1 folder with {count} prefabs" : "No folders configured";
        }
        
        public List<string> ValidateConfiguration() 
        { 
            return new List<string>();
        }
    }
    
    [Serializable]
    public class InputFolderConfiguration
    {
        public string folderPath = "";
        public string prefabPrefix = "";
        public bool isEnabled = true;
        public bool useCustomSettings = false;
        public Vector3 customObjectScale = Vector3.one;
        public Vector3 customObjectPosition = Vector3.zero;
        public Vector3 customObjectRotation = Vector3.zero;
        
        public bool IsValid() 
        { 
            return !string.IsNullOrEmpty(folderPath) && 
                   System.IO.Directory.Exists(folderPath) && 
                   !string.IsNullOrEmpty(prefabPrefix) &&
                   isEnabled;
        }
        
        public List<GameObject> GetPrefabs() 
        { 
            var prefabs = new List<GameObject>();
            
            if (!IsValid()) return prefabs;
            
            try
            {
                var prefabFiles = System.IO.Directory.GetFiles(folderPath, "*.prefab", System.IO.SearchOption.TopDirectoryOnly)
                    .Where(file => System.IO.Path.GetFileNameWithoutExtension(file).StartsWith(prefabPrefix))
                    .ToArray();
                    
                foreach (var prefabFile in prefabFiles)
                {
                    var relativePath = prefabFile.Replace(Application.dataPath, "Assets");
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
                    
                    if (prefab != null)
                    {
                        prefabs.Add(prefab);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading prefabs from {folderPath}: {ex.Message}");
            }
            
            return prefabs;
        }
        
        public string GetDisplayName() 
        { 
            if (string.IsNullOrEmpty(folderPath))
                return "Empty Folder";
                
            var folderName = System.IO.Path.GetFileName(folderPath);
            var prefabCount = GetPrefabs().Count;
            
            return $"{folderName} ({prefabPrefix}*) - {prefabCount} prefabs";
        }
    }
    
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