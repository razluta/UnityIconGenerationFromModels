using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System;
using System.Diagnostics;

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
        private GenerationReport generationReport;
        private Stopwatch sessionStopwatch;
        private Stopwatch prefabStopwatch;
        
        public UnityIconGenerationTool(IconGeneratorSettings settings)
        {
            this.settings = settings;
            this.generationReport = new GenerationReport();
            this.sessionStopwatch = new Stopwatch();
            this.prefabStopwatch = new Stopwatch();
        }
        
        public void GenerateIcons(System.Action<string> onProgress = null, System.Action onComplete = null)
        {
            // Initialize reporting
            generationReport.StartGeneration(settings);
            sessionStopwatch.Start();
            
            UnityEngine.Debug.Log($"Starting icon generation session: {generationReport.sessionId}");
            
            var prefabs = FindPrefabsWithPrefix();
            generationReport.totalPrefabsFound = prefabs.Count;
            
            if (prefabs.Count == 0)
            {
                var errorMsg = $"No prefabs found with prefix '{settings.prefabNamePrefix}' in folder '{settings.inputFolderPath}'";
                UnityEngine.Debug.LogWarning(errorMsg);
                generationReport.AddGlobalError(errorMsg);
                CompleteGeneration(onComplete);
                return;
            }
            
            UnityEngine.Debug.Log($"Found {prefabs.Count} prefabs to process");
            
            // Handle scene management
            if (!HandleScenePreparation())
            {
                generationReport.AddGlobalError("Scene preparation failed");
                CompleteGeneration(onComplete);
                return;
            }
            
            bool renderSceneSetup = false;
            try
            {
                SetupRenderScene();
                renderSceneSetup = true;
                EnsureOutputDirectoryExists();
                
                for (int i = 0; i < prefabs.Count; i++)
                {
                    var prefab = prefabs[i];
                    var progress = $"Processing {prefab.name} ({i + 1}/{prefabs.Count})";
                    onProgress?.Invoke(progress);
                    
                    // Start timing this prefab
                    prefabStopwatch.Restart();
                    var reportEntry = generationReport.StartPrefabProcessing(prefab.name, AssetDatabase.GetAssetPath(prefab));
                    
                    try
                    {
                        GenerateIconForPrefab(prefab, reportEntry);
                        
                        prefabStopwatch.Stop();
                        reportEntry.processingTimeSeconds = (float)prefabStopwatch.Elapsed.TotalSeconds;
                        generationReport.CompletePrefabProcessing(reportEntry, true);
                        
                        UnityEngine.Debug.Log($"Successfully processed {prefab.name} in {reportEntry.processingTimeSeconds:F2}s");
                    }
                    catch (Exception e)
                    {
                        prefabStopwatch.Stop();
                        reportEntry.processingTimeSeconds = (float)prefabStopwatch.Elapsed.TotalSeconds;
                        var errorMsg = $"Failed to process {prefab.name}: {e.Message}";
                        UnityEngine.Debug.LogError(errorMsg);
                        generationReport.CompletePrefabProcessing(reportEntry, false, e.Message);
                    }
                    
                    EditorUtility.DisplayProgressBar("Generating Icons", progress, (float)i / prefabs.Count);
                }
            }
            catch (Exception e)
            {
                var errorMsg = $"Critical error during icon generation: {e.Message}";
                UnityEngine.Debug.LogError(errorMsg);
                generationReport.AddGlobalError(errorMsg);
            }
            finally
            {
                // Always clean up and restore, even if something goes wrong
                if (renderSceneSetup)
                {
                    CleanupRenderScene();
                }
                RestoreOriginalScene();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
                
                CompleteGeneration(onComplete);
            }
        }
        
        private void CompleteGeneration(System.Action onComplete)
        {
            sessionStopwatch.Stop();
            generationReport.EndGeneration();
            
            // Log summary
            var successRate = generationReport.totalPrefabsProcessed > 0 ? 
                (generationReport.successfulPrefabs * 100.0f / generationReport.totalPrefabsProcessed) : 0;
            
            UnityEngine.Debug.Log($"Icon generation completed! " +
                                $"Processed: {generationReport.totalPrefabsProcessed}, " +
                                $"Success: {generationReport.successfulPrefabs}, " +
                                $"Failed: {generationReport.failedPrefabs}, " +
                                $"Success Rate: {successRate:F1}%, " +
                                $"Total Time: {generationReport.totalDurationSeconds:F2}s");
            
            // Save reports
            try
            {
                generationReport.SaveToFile();
                generationReport.SaveJsonReport();
                
                // Show completion dialog with summary
                ShowCompletionDialog();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to save generation reports: {e.Message}");
                generationReport.AddGlobalError($"Failed to save reports: {e.Message}");
            }
            
            onComplete?.Invoke();
        }
        
        private void ShowCompletionDialog()
        {
            var message = $"Icon Generation Complete!\n\n" +
                         $"📊 Session Summary:\n" +
                         $"• Processed: {generationReport.successfulPrefabs}/{generationReport.totalPrefabsProcessed} prefabs\n" +
                         $"• Generated: {generationReport.totalIconsGenerated} icons\n" +
                         $"• Total Size: {FormatFileSize(generationReport.totalFileSizeBytes)}\n" +
                         $"• Duration: {generationReport.totalDurationSeconds:F1}s\n\n";
            
            if (generationReport.failedPrefabs > 0)
            {
                message += $"⚠️ {generationReport.failedPrefabs} prefabs failed to process.\n";
            }
            
            if (generationReport.globalErrors.Count > 0 || generationReport.warnings.Count > 0)
            {
                message += $"⚠️ {generationReport.globalErrors.Count} errors, {generationReport.warnings.Count} warnings.\n";
            }
            
            message += $"\n📁 Check the Reports folder in your output directory for detailed logs.";
            
            EditorUtility.DisplayDialog("Icon Generation Complete", message, "OK");
        }
        
        private bool HandleScenePreparation()
        {
            try
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
                                    generationReport.AddGlobalError("Scene save cancelled - no save location specified");
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
                                generationReport.AddGlobalError("Failed to save the current scene");
                                return false;
                            }
                            
                            hadUnsavedChanges = false;
                            UnityEngine.Debug.Log($"Scene saved successfully: {originalScenePath}");
                            break;
                            
                        case 1: // Continue without Saving
                            generationReport.AddWarning("Continuing without saving current scene");
                            break;
                            
                        case 2: // Cancel
                            generationReport.AddGlobalError("Icon generation cancelled by user");
                            return false;
                            
                        default:
                            return false;
                    }
                }
                
                return true;
            }
            catch (Exception e)
            {
                generationReport.AddGlobalError($"Scene preparation failed: {e.Message}");
                return false;
            }
        }
        
        private void RestoreOriginalScene()
        {
            try
            {
                // If we had an original scene with a valid path, reload it
                if (!string.IsNullOrEmpty(originalScenePath))
                {
                    EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
                    UnityEngine.Debug.Log($"Restored original scene: {originalScenePath}");
                }
                else if (originalScene.IsValid())
                {
                    // If the scene was never saved but is still valid, try to keep it active
                    UnityEngine.Debug.Log("Keeping unsaved scene active.");
                }
                else
                {
                    // Create a new empty scene as fallback
                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                    UnityEngine.Debug.Log("Created new default scene as fallback.");
                }
            }
            catch (Exception e)
            {
                var errorMsg = $"Failed to restore original scene: {e.Message}";
                UnityEngine.Debug.LogError(errorMsg);
                generationReport.AddGlobalError(errorMsg);
                
                // Try to create new scene as last resort
                try
                {
                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                }
                catch (Exception fallbackE)
                {
                    generationReport.AddGlobalError($"Failed to create fallback scene: {fallbackE.Message}");
                }
            }
        }
        
        private List<GameObject> FindPrefabsWithPrefix()
        {
            var prefabs = new List<GameObject>();
            
            try
            {
                if (!AssetDatabase.IsValidFolder(settings.inputFolderPath))
                {
                    var errorMsg = $"Input folder '{settings.inputFolderPath}' does not exist!";
                    generationReport.AddGlobalError(errorMsg);
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
                
                if (prefabs.Count == 0 && guids.Length > 0)
                {
                    generationReport.AddWarning($"Found {guids.Length} prefabs in folder, but none match prefix '{settings.prefabNamePrefix}'");
                }
            }
            catch (Exception e)
            {
                generationReport.AddGlobalError($"Error finding prefabs: {e.Message}");
            }
            
            return prefabs;
        }
        
        private void SetupRenderScene()
        {
            try
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
                
                UnityEngine.Debug.Log($"Render scene setup complete with {pointLights.Count} additional point lights.");
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to setup render scene: {e.Message}", e);
            }
        }
        
        private void GenerateIconForPrefab(GameObject prefab, GenerationReportEntry reportEntry)
        {
            try
            {
                // Instantiate prefab in render scene
                currentObject = UnityEngine.Object.Instantiate(prefab);
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
                    try
                    {
                        GenerateIconAtSize(iconName, size, fileExtension, reportEntry);
                    }
                    catch (Exception e)
                    {
                        var iconInfo = new GeneratedIconInfo($"{iconName}_Icon_{size}x{size}.{fileExtension}", "", size, settings.exportFormat);
                        iconInfo.success = false;
                        iconInfo.errorMessage = e.Message;
                        generationReport.AddIconGenerated(reportEntry, iconInfo);
                        
                        UnityEngine.Debug.LogError($"Failed to generate {size}x{size} icon for {prefab.name}: {e.Message}");
                    }
                }
            }
            finally
            {
                // Cleanup
                if (currentObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(currentObject);
                }
            }
        }
        
        private void GenerateIconAtSize(string iconName, int size, string fileExtension, GenerationReportEntry reportEntry)
        {
            RenderTexture renderTexture = null;
            Texture2D texture2D = null;
            
            try
            {
                // Create render texture
                renderTexture = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
                renderTexture.antiAliasing = 8;
                
                // Render to texture
                renderCamera.targetTexture = renderTexture;
                renderCamera.Render();
                
                // Convert to Texture2D
                RenderTexture.active = renderTexture;
                texture2D = new Texture2D(size, size, TextureFormat.RGBA32, false);
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
                
                // Create and add icon info to report
                var iconInfo = new GeneratedIconInfo(fileName, savePath, size, settings.exportFormat);
                iconInfo.fileSizeBytes = fileData.Length;
                generationReport.AddIconGenerated(reportEntry, iconInfo);
                
                UnityEngine.Debug.Log($"Generated {size}x{size} icon: {fileName} ({FormatFileSize(fileData.Length)})");
            }
            catch (Exception e)
            {
                // Create failed icon info
                var fileName = size == settings.iconSize ? 
                    $"{iconName}_Icon.{fileExtension}" : 
                    $"{iconName}_Icon_{size}x{size}.{fileExtension}";
                    
                var iconInfo = new GeneratedIconInfo(fileName, "", size, settings.exportFormat);
                iconInfo.success = false;
                iconInfo.errorMessage = e.Message;
                generationReport.AddIconGenerated(reportEntry, iconInfo);
                
                throw new Exception($"Failed to generate {size}x{size} icon: {e.Message}", e);
            }
            finally
            {
                // Cleanup
                if (renderTexture != null)
                {
                    RenderTexture.active = null;
                    renderCamera.targetTexture = null;
                    UnityEngine.Object.DestroyImmediate(renderTexture);
                }
                
                if (texture2D != null)
                {
                    UnityEngine.Object.DestroyImmediate(texture2D);
                }
            }
        }
        
        private void AdjustObjectForRendering()
        {
            try
            {
                var renderer = currentObject.GetComponentInChildren<Renderer>();
                if (renderer == null) 
                {
                    generationReport.AddWarning($"No renderer found on {currentObject.name} - skipping auto-adjustment");
                    return;
                }
                
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
            catch (Exception e)
            {
                generationReport.AddWarning($"Failed to auto-adjust object {currentObject.name}: {e.Message}");
            }
        }
        
        private void EnsureOutputDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(settings.outputFolderPath))
                {
                    Directory.CreateDirectory(settings.outputFolderPath);
                    UnityEngine.Debug.Log($"Created output directory: {settings.outputFolderPath}");
                }
                
                // Also create Reports subfolder
                var reportsFolder = Path.Combine(settings.outputFolderPath, "Reports");
                if (!Directory.Exists(reportsFolder))
                {
                    Directory.CreateDirectory(reportsFolder);
                    UnityEngine.Debug.Log($"Created reports directory: {reportsFolder}");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to create output directory: {e.Message}", e);
            }
        }
        
        private void CleanupRenderScene()
        {
            try
            {
                if (renderScene.IsValid())
                {
                    EditorSceneManager.CloseScene(renderScene, true);
                    UnityEngine.Debug.Log("Render scene cleaned up successfully.");
                }
            }
            catch (Exception e)
            {
                var errorMsg = $"Failed to cleanup render scene: {e.Message}";
                UnityEngine.Debug.LogError(errorMsg);
                generationReport.AddGlobalError(errorMsg);
            }
        }
        
        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }
        
        // Public method to get the current generation report
        public GenerationReport GetGenerationReport()
        {
            return generationReport;
        }
    }
}