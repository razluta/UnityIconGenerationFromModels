using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Razluta.UnityIconGenerationFromModels.Editor
{
    /// <summary>
    /// Represents a single input folder configuration
    /// </summary>
    [Serializable]
    public class InputFolderConfiguration
    {
        [Header("Folder Settings")]
        public string folderPath = "";
        public string prefabPrefix = "";
        public bool isEnabled = true;
        
        [Header("Override Settings (Optional)")]
        public bool useCustomSettings = false;
        public Vector3 customObjectScale = Vector3.one;
        public Vector3 customObjectPosition = Vector3.zero;
        public Vector3 customObjectRotation = Vector3.zero;
        
        public InputFolderConfiguration()
        {
            folderPath = "";
            prefabPrefix = "";
            isEnabled = true;
            useCustomSettings = false;
            customObjectScale = Vector3.one;
            customObjectPosition = Vector3.zero;
            customObjectRotation = Vector3.zero;
        }
        
        public InputFolderConfiguration(string path, string prefix)
        {
            folderPath = path;
            prefabPrefix = prefix;
            isEnabled = true;
            useCustomSettings = false;
            customObjectScale = Vector3.one;
            customObjectPosition = Vector3.zero;
            customObjectRotation = Vector3.zero;
        }
        
        /// <summary>
        /// Validate if this folder configuration is valid
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(folderPath) && 
                   Directory.Exists(folderPath) && 
                   !string.IsNullOrEmpty(prefabPrefix) &&
                   isEnabled;
        }
        
        /// <summary>
        /// Get all valid prefabs from this folder
        /// </summary>
        public List<GameObject> GetPrefabs()
        {
            var prefabs = new List<GameObject>();
            
            if (!IsValid())
                return prefabs;
                
            try
            {
                var prefabFiles = Directory.GetFiles(folderPath, "*.prefab", SearchOption.TopDirectoryOnly)
                    .Where(file => Path.GetFileNameWithoutExtension(file).StartsWith(prefabPrefix))
                    .ToArray();
                    
                foreach (var prefabFile in prefabFiles)
                {
                    var relativePath = prefabFile.Replace(Application.dataPath, "Assets");
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
                    
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
        
        /// <summary>
        /// Get display name for UI
        /// </summary>
        public string GetDisplayName()
        {
            if (string.IsNullOrEmpty(folderPath))
                return "Empty Folder";
                
            var folderName = Path.GetFileName(folderPath);
            var prefabCount = GetPrefabs().Count;
            
            return $"{folderName} ({prefabPrefix}*) - {prefabCount} prefabs";
        }
    }
    
    /// <summary>
    /// Manages multiple input folders for batch processing
    /// </summary>
    [Serializable]
    public class MultiFolderManager
    {
        [SerializeField]
        private List<InputFolderConfiguration> inputFolders = new List<InputFolderConfiguration>();
        
        public List<InputFolderConfiguration> InputFolders => inputFolders;
        
        public MultiFolderManager()
        {
            // Start with one default folder
            if (inputFolders.Count == 0)
            {
                inputFolders.Add(new InputFolderConfiguration());
            }
        }
        
        /// <summary>
        /// Add a new input folder
        /// </summary>
        public void AddFolder()
        {
            inputFolders.Add(new InputFolderConfiguration());
        }
        
        /// <summary>
        /// Add a new input folder with specific settings
        /// </summary>
        public void AddFolder(string path, string prefix)
        {
            inputFolders.Add(new InputFolderConfiguration(path, prefix));
        }
        
        /// <summary>
        /// Remove folder at specific index
        /// </summary>
        public void RemoveFolder(int index)
        {
            if (index >= 0 && index < inputFolders.Count && inputFolders.Count > 1)
            {
                inputFolders.RemoveAt(index);
            }
        }
        
        /// <summary>
        /// Get all valid folders
        /// </summary>
        public List<InputFolderConfiguration> GetValidFolders()
        {
            return inputFolders.Where(folder => folder.IsValid()).ToList();
        }
        
        /// <summary>
        /// Get all prefabs from all valid folders
        /// </summary>
        public List<PrefabProcessingInfo> GetAllPrefabs()
        {
            var allPrefabs = new List<PrefabProcessingInfo>();
            
            foreach (var folder in GetValidFolders())
            {
                var prefabs = folder.GetPrefabs();
                
                foreach (var prefab in prefabs)
                {
                    var processingInfo = new PrefabProcessingInfo
                    {
                        prefab = prefab,
                        sourceFolder = folder,
                        prefabName = prefab.name
                    };
                    
                    allPrefabs.Add(processingInfo);
                }
            }
            
            return allPrefabs;
        }
        
        /// <summary>
        /// Get total prefab count across all folders
        /// </summary>
        public int GetTotalPrefabCount()
        {
            return GetValidFolders().Sum(folder => folder.GetPrefabs().Count);
        }
        
        /// <summary>
        /// Get summary of all folders
        /// </summary>
        public string GetSummary()
        {
            var validFolders = GetValidFolders();
            var totalPrefabs = GetTotalPrefabCount();
            
            if (validFolders.Count == 0)
                return "No valid folders configured";
                
            if (validFolders.Count == 1)
                return $"1 folder with {totalPrefabs} prefabs";
                
            return $"{validFolders.Count} folders with {totalPrefabs} total prefabs";
        }
        
        /// <summary>
        /// Validate all folders and return issues
        /// </summary>
        public List<string> ValidateConfiguration()
        {
            var issues = new List<string>();
            
            var validFolders = GetValidFolders();
            if (validFolders.Count == 0)
            {
                issues.Add("No valid input folders configured");
            }
            
            for (int i = 0; i < inputFolders.Count; i++)
            {
                var folder = inputFolders[i];
                
                if (!folder.isEnabled)
                    continue;
                    
                if (string.IsNullOrEmpty(folder.folderPath))
                {
                    issues.Add($"Folder {i + 1}: Path is empty");
                }
                else if (!Directory.Exists(folder.folderPath))
                {
                    issues.Add($"Folder {i + 1}: Path does not exist ({folder.folderPath})");
                }
                
                if (string.IsNullOrEmpty(folder.prefabPrefix))
                {
                    issues.Add($"Folder {i + 1}: Prefix is empty");
                }
                
                if (folder.IsValid() && folder.GetPrefabs().Count == 0)
                {
                    issues.Add($"Folder {i + 1}: No matching prefabs found ({folder.prefabPrefix}*)");
                }
            }
            
            return issues;
        }
    }
    
    /// <summary>
    /// Information about a prefab to be processed
    /// </summary>
    [Serializable]
    public class PrefabProcessingInfo
    {
        public GameObject prefab;
        public InputFolderConfiguration sourceFolder;
        public string prefabName;
        public bool processed = false;
        public bool hadError = false;
        public string errorMessage = "";
        public float processingTime = 0f;
        
        /// <summary>
        /// Get the transform settings to use for this prefab
        /// </summary>
        public (Vector3 scale, Vector3 position, Vector3 rotation) GetTransformSettings()
        {
            if (sourceFolder.useCustomSettings)
            {
                return (sourceFolder.customObjectScale, 
                       sourceFolder.customObjectPosition, 
                       sourceFolder.customObjectRotation);
            }
            
            // Return default/global settings
            return (Vector3.one, Vector3.zero, Vector3.zero);
        }
    }
}