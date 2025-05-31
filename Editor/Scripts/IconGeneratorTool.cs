using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace Razluta.UnityIconGenerationFromModels
{
    public class UnityIconGenerationTool
    {
        private IconGeneratorSettings settings;
        private Scene renderScene;
        private Scene originalScene;
        private string originalScenePath;
        private bool hadUnsavedChanges;
        private Camera renderCamera;
        private Light mainLight;
        private Light fillLight;
        private List<Light> pointLights = new List<Light>();
        private GameObject currentObject;
        
        public UnityIconGenerationTool(IconGeneratorSettings settings)
        {
            this.settings = settings;
        }
        
        public void GenerateIcons(System.Action<string> onProgress = null, System.Action onComplete = null)
        {
            var prefabs = FindPrefabsWithPrefix();
            if (prefabs.Count == 0)
            {
                Debug.LogWarning($"No prefabs found with prefix '{settings.prefabNamePrefix}' in folder '{settings.inputFolderPath}'");
                onComplete?.Invoke();
                return;
            }
            
            // Handle scene management
            if (!HandleScenePreparation())
            {
                onComplete?.Invoke();
                return;
            }
            
            SetupRenderScene();
            EnsureOutputDirectoryExists();
            
            try
            {
                for (int i = 0; i < prefabs.Count; i++)
                {
                    var prefab = prefabs[i];
                    var progress = $"Processing {prefab.name} ({i + 1}/{prefabs.Count})";
                    onProgress?.Invoke(progress);
                    
                    GenerateIconForPrefab(prefab);
                    
                    EditorUtility.DisplayProgressBar("Generating Icons", progress, (float)i / prefabs.Count);
                }
            }
            finally
            {
                // Always clean up and restore, even if something goes wrong
                CleanupRenderScene();
                RestoreOriginalScene();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            }
            
            Debug.Log($"Successfully generated {prefabs.Count} icons in '{settings.outputFolderPath}'");
            onComplete?.Invoke();
        }
        
        private bool HandleScenePreparation()
        {
            // Get the current active scene
            originalScene = SceneManager.GetActiveScene();
            originalScenePath = originalScene.path;
            hadUnsavedChanges = originalScene.isDirty;
            
            // If the scene has unsaved changes, ask the user to save
            if (hadUnsavedChanges)
            {
                int option = EditorUtility.DisplayDialogComplex(
                    "Unsaved Scene Changes",
                    $"The current scene '{originalScene.name}' has unsaved changes.\n\n" +
                    "The Icon Generation tool needs to create a temporary scene for rendering. " +
                    "Would you like to save your current scene first?",
                    "Save and Continue",
                    "Continue without Saving", 
                    "Cancel"
                );
                
                switch (option)
                {
                    case 0: // Save and Continue
                        if (string.IsNullOrEmpty(originalScenePath))
                        {
                            // Scene has never been saved, prompt for save location
                            originalScenePath = EditorUtility.SaveFilePanel(
                                "Save Scene",
                                "Assets",
                                originalScene.name,
                                "unity"
                            );
                            
                            if (string.IsNullOrEmpty(originalScenePath))
                            {
                                Debug.LogWarning("Icon generation cancelled - no save location specified.");
                                return false;
                            }
                            
                            // Convert absolute path to relative path
                            if (originalScenePath.StartsWith(Application.dataPath))
                            {
                                originalScenePath = "Assets" + originalScenePath.Substring(Application.dataPath.Length);
                            }
                        }
                        
                        if (!EditorSceneManager.SaveScene(originalScene, originalScenePath))
                        {
                            Debug.LogError("Failed to save the current scene. Icon generation cancelled.");
                            return false;
                        }
                        
                        hadUnsavedChanges = false;
                        Debug.Log($"Scene saved successfully: {originalScenePath}");
                        break;
                        
                    case 1: // Continue without Saving
                        Debug.LogWarning("Continuing without saving current scene. Your changes will be preserved.");
                        break;
                        
                    case 2: // Cancel
                        Debug.Log("Icon generation cancelled by user.");
                        return false;
                        
                    default:
                        return false;
                }
            }
            
            return true;
        }
        
        private void RestoreOriginalScene()
        {
            // If we had an original scene with a valid path, reload it
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                try
                {
                    EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
                    Debug.Log($"Restored original scene: {originalScenePath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to restore original scene '{originalScenePath}': {e.Message}");
                }
            }
            else if (originalScene.IsValid())
            {
                // If the scene was never saved but is still valid, try to keep it active
                // This handles the case where user chose "Continue without Saving"
                Debug.Log("Keeping unsaved scene active.");
            }
            else
            {
                // Create a new empty scene as fallback
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                Debug.Log("Created new default scene as fallback.");
            }
        }
        
        private List<GameObject> FindPrefabsWithPrefix()
        {
            var prefabs = new List<GameObject>();
            
            if (!AssetDatabase.IsValidFolder(settings.inputFolderPath))
            {
                Debug.LogError($"Input folder '{settings.inputFolderPath}' does not exist!");
                return prefabs;
            }
            
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { settings.inputFolderPath });
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null && prefab.name.StartsWith(settings.prefabNamePrefix))
                {
                    prefabs.Add(prefab);
                }
            }
            
            return prefabs;
        }
        
        private void SetupRenderScene()
        {
            // Create temporary scene for rendering
            renderScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(renderScene);
            
            // Setup camera
            var cameraGO = new GameObject("RenderCamera");
            renderCamera = cameraGO.AddComponent<Camera>();
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = settings.backgroundColor;
            renderCamera.fieldOfView = settings.cameraFOV;
            renderCamera.transform.position = settings.cameraPosition;
            renderCamera.transform.rotation = Quaternion.Euler(settings.cameraRotation);
            renderCamera.renderingPath = RenderingPath.Forward;
            
            // Setup main light
            var mainLightGO = new GameObject("MainLight");
            mainLight = mainLightGO.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.color = settings.mainLightColor;
            mainLight.intensity = settings.mainLightIntensity;
            mainLight.transform.rotation = Quaternion.Euler(settings.mainLightDirection);
            
            // Setup fill light
            var fillLightGO = new GameObject("FillLight");
            fillLight = fillLightGO.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = settings.fillLightColor;
            fillLight.intensity = settings.fillLightIntensity;
            fillLight.transform.rotation = Quaternion.Euler(settings.fillLightDirection);
            
            // Setup additional point lights
            pointLights.Clear();
            for (int i = 0; i < settings.pointLights.Count; i++)
            {
                var pointLightSettings = settings.pointLights[i];
                if (!pointLightSettings.enabled) continue;
                
                var pointLightGO = new GameObject($"PointLight_{i}");
                var pointLight = pointLightGO.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.color = pointLightSettings.color;
                pointLight.intensity = pointLightSettings.intensity;
                pointLight.range = pointLightSettings.range;
                pointLight.transform.position = pointLightSettings.position;
                
                pointLights.Add(pointLight);
            }
            
            Debug.Log($"Render scene setup complete with {pointLights.Count} additional point lights.");
        }
        
        private void GenerateIconForPrefab(GameObject prefab)
        {
            // Instantiate prefab in render scene
            currentObject = Object.Instantiate(prefab);
            currentObject.transform.position = settings.objectPosition;
            currentObject.transform.rotation = Quaternion.Euler(settings.objectRotation);
            currentObject.transform.localScale = Vector3.one * settings.objectScale;
            
            // Auto-center and fit if enabled
            if (settings.autoCenter || settings.autoFit)
            {
                AdjustObjectForRendering();
            }
            
            // Generate icons for all sizes
            var allSizes = settings.GetAllSizes();
            var iconName = prefab.name.Replace(settings.prefabNamePrefix, "");
            var fileExtension = settings.GetFileExtension();
            
            foreach (var size in allSizes)
            {
                GenerateIconAtSize(iconName, size, fileExtension);
            }
            
            // Cleanup
            Object.DestroyImmediate(currentObject);
        }
        
        private void GenerateIconAtSize(string iconName, int size, string fileExtension)
        {
            // Create render texture
            var renderTexture = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
            renderTexture.antiAliasing = 8;
            
            // Render to texture
            renderCamera.targetTexture = renderTexture;
            renderCamera.Render();
            
            // Convert to Texture2D
            RenderTexture.active = renderTexture;
            var texture2D = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture2D.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            texture2D.Apply();
            
            // Save file
            byte[] fileData;
            string fileName;
            
            if (settings.exportFormat == ExportFormat.PNG)
            {
                fileData = texture2D.EncodeToPNG();
                fileName = size == settings.iconSize ? 
                    $"{iconName}_Icon.{fileExtension}" : 
                    $"{iconName}_Icon_{size}x{size}.{fileExtension}";
            }
            else // TGA
            {
                fileData = texture2D.EncodeToTGA();
                fileName = size == settings.iconSize ? 
                    $"{iconName}_Icon.{fileExtension}" : 
                    $"{iconName}_Icon_{size}x{size}.{fileExtension}";
            }
            
            var savePath = Path.Combine(settings.outputFolderPath, fileName);
            File.WriteAllBytes(savePath, fileData);
            
            // Cleanup
            RenderTexture.active = null;
            renderCamera.targetTexture = null;
            Object.DestroyImmediate(renderTexture);
            Object.DestroyImmediate(texture2D);
        }
        
        private void AdjustObjectForRendering()
        {
            var renderer = currentObject.GetComponentInChildren<Renderer>();
            if (renderer == null) return;
            
            var bounds = renderer.bounds;
            
            if (settings.autoCenter)
            {
                currentObject.transform.position = -bounds.center + settings.objectPosition;
            }
            
            if (settings.autoFit)
            {
                var maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                var targetSize = 2f; // Adjust this value to control how much of the view the object fills
                var scale = targetSize / maxSize * settings.objectScale;
                currentObject.transform.localScale = Vector3.one * scale;
            }
        }
        
        private void EnsureOutputDirectoryExists()
        {
            if (!Directory.Exists(settings.outputFolderPath))
            {
                Directory.CreateDirectory(settings.outputFolderPath);
            }
        }
        
        private void CleanupRenderScene()
        {
            if (renderScene.IsValid())
            {
                EditorSceneManager.CloseScene(renderScene, true);
                Debug.Log("Render scene cleaned up successfully.");
            }
        }
    }
}