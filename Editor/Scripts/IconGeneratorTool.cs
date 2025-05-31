using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace Razluta.UnityIconGenerationFromModels.Editor
{
    /// <summary>
    /// Core icon generation tool with v1.2.0 features:
    /// - Quality settings and performance optimization
    /// - Multi-folder processing
    /// - Comprehensive reporting and logging
    /// - Enhanced error handling and progress tracking
    /// </summary>
    public static class IconGeneratorTool
    {
        private static GenerationReport currentReport;
        private static bool isProcessing = false;
        private static System.Action<float, string> progressCallback;
        
        /// <summary>
        /// Generate icons using the provided settings
        /// </summary>
        public static void GenerateIcons(IconGeneratorSettings settings, System.Action<float, string> onProgress = null)
        {
            if (isProcessing)
            {
                Debug.LogWarning("Icon generation already in progress");
                return;
            }
            
            progressCallback = onProgress;
            EditorApplication.update += UpdateProgress;
            
            try
            {
                EditorCoroutineUtility.StartCoroutine(GenerateIconsCoroutine(settings), null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to start icon generation: {ex.Message}");
                CleanupGeneration();
            }
        }
        
        private static IEnumerator GenerateIconsCoroutine(IconGeneratorSettings settings)
        {
            isProcessing = true;
            
            // Initialize report
            if (settings.enableReports)
            {
                currentReport = GenerationReportManager.StartNewReport();
                ConfigureReport(settings);
            }
            
            var originalScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var originalCameraSetup = StoreCameraSetup();
            var originalQualitySettings = StoreQualitySettings();
            var originalLightingSettings = StoreLightingSettings();
            
            try
            {
                ReportProgress(0f, "Initializing generation...");
                
                // Validate settings
                var issues = settings.ValidateSettings();
                if (issues.Count > 0)
                {
                    foreach (var issue in issues)
                    {
                        currentReport?.LogError($"Validation failed: {issue}");
                    }
                    yield break;
                }
                
                // Apply quality settings
                ApplyQualitySettings(settings.qualitySettings);
                
                // Get all prefabs to process
                var allPrefabs = settings.multiFolderManager.GetAllPrefabs();
                currentReport?.LogInfo($"Found {allPrefabs.Count} prefabs to process across {settings.multiFolderManager.GetValidFolders().Count} folders");
                
                if (allPrefabs.Count == 0)
                {
                    currentReport?.LogWarning("No prefabs found to process");
                    yield break;
                }
                
                // Create temporary scene
                var tempScene = CreateTemporaryScene();
                
                // Setup lighting and camera
                SetupLighting(settings.lightingConfiguration);
                var renderCamera = SetupCamera(settings);
                
                var totalOperations = allPrefabs.Count * settings.GetAllSizes().Count;
                var currentOperation = 0;
                
                // Process each prefab
                foreach (var prefabInfo in allPrefabs)
                {
                    if (!isProcessing) break; // Check for cancellation
                    
                    var prefabStartTime = Time.realtimeSinceStartup;
                    
                    try
                    {
                        ReportProgress(
                            (float)currentOperation / totalOperations, 
                            $"Processing {prefabInfo.prefabName}..."
                        );
                        
                        currentReport?.LogInfo($"Starting processing", prefabInfo.prefabName);
                        
                        // Process this prefab for all sizes
                        yield return ProcessPrefabAllSizes(prefabInfo, settings, renderCamera);
                        
                        var processingTime = Time.realtimeSinceStartup - prefabStartTime;
                        currentReport?.UpdateProcessingTime(prefabInfo.prefabName, processingTime);
                        currentReport?.LogSuccess($"Completed processing", prefabInfo.prefabName, "", processingTime);
                        
                        prefabInfo.processed = true;
                        prefabInfo.processingTime = processingTime;
                        
                        currentOperation += settings.GetAllSizes().Count;
                        
                        // Memory cleanup
                        if (settings.enableMemoryOptimization && currentOperation % 10 == 0)
                        {
                            yield return CleanupMemory();
                        }
                    }
                    catch (Exception ex)
                    {
                        var processingTime = Time.realtimeSinceStartup - prefabStartTime;
                        prefabInfo.hadError = true;
                        prefabInfo.errorMessage = ex.Message;
                        prefabInfo.processingTime = processingTime;
                        
                        currentReport?.LogError($"Failed to process: {ex.Message}", prefabInfo.prefabName, "", processingTime);
                        
                        if (settings.pauseOnError)
                        {
                            Debug.LogError($"Processing paused due to error with {prefabInfo.prefabName}: {ex.Message}");
                            break;
                        }
                        
                        currentOperation += settings.GetAllSizes().Count;
                    }
                    
                    yield return null; // Allow Unity to update
                }
                
                ReportProgress(1f, "Finalizing generation...");
                
                // Export configuration if requested
                if (settings.exportConfigurationWithIcons)
                {
                    ExportConfigurationFile(settings);
                }
                
                currentReport?.LogInfo("Icon generation completed successfully");
            }
            finally
            {
                // Cleanup and restore
                RestoreQualitySettings(originalQualitySettings);
                RestoreLightingSettings(originalLightingSettings);
                RestoreCameraSetup(originalCameraSetup);
                RestoreScene(originalScene);
                
                if (settings.enableReports)
                {
                    GenerationReportManager.CompleteCurrentReport();
                }
                
                CleanupGeneration();
            }
        }
        
        private static IEnumerator ProcessPrefabAllSizes(PrefabProcessingInfo prefabInfo, IconGeneratorSettings settings, Camera renderCamera)
        {
            // Instantiate prefab in temporary scene
            var instance = InstantiatePrefabInScene(prefabInfo, settings);
            
            try
            {
                // Apply transform settings
                ApplyTransformSettings(instance, prefabInfo, settings);
                
                // Position camera for this object
                if (settings.autoFit || settings.autoCenter)
                {
                    PositionCameraForObject(renderCamera, instance, settings);
                }
                
                // Generate icons for all sizes
                foreach (var size in settings.GetAllSizes())
                {
                    if (!isProcessing) break; // Check for cancellation
                    
                    yield return GenerateIconForSize(prefabInfo, instance, size, settings, renderCamera);
                    yield return null; // Allow Unity to update
                }
            }
            finally
            {
                // Clean up prefab instance
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
        }
        
        private static IEnumerator GenerateIconForSize(PrefabProcessingInfo prefabInfo, GameObject instance, int size, IconGeneratorSettings settings, Camera renderCamera)
        {
            var actualSize = Mathf.RoundToInt(size * settings.qualitySettings.renderScale);
            
            // Create render texture with quality settings
            var renderTexture = CreateRenderTexture(actualSize, settings.qualitySettings);
            
            try
            {
                renderCamera.targetTexture = renderTexture;
                
                // Render the frame
                renderCamera.Render();
                
                // Read pixels from render texture
                RenderTexture.active = renderTexture;
                var texture = new Texture2D(actualSize, actualSize, TextureFormat.ARGB32, false);
                texture.ReadPixels(new Rect(0, 0, actualSize, actualSize), 0, 0);
                texture.Apply();
                
                // Scale down if render scale was used
                if (settings.qualitySettings.renderScale != 1.0f)
                {
                    texture = ScaleTexture(texture, size);
                }
                
                // Save to file
                var filename = GenerateFilename(prefabInfo.prefabName, size, settings);
                var filepath = Path.Combine(settings.outputFolderPath, filename);
                
                SaveTextureToFile(texture, filepath, settings.exportFormat);
                
                currentReport?.LogSuccess($"Generated {size}x{size} icon: {filename}", prefabInfo.prefabName);
            }
            finally
            {
                // Cleanup
                RenderTexture.active = null;
                renderCamera.targetTexture = null;
                
                if (renderTexture != null)
                {
                    renderTexture.Release();
                    UnityEngine.Object.DestroyImmediate(renderTexture);
                }
            }
            
            yield return null;
        }
        
        private static RenderTexture CreateRenderTexture(int size, QualitySettings qualitySettings)
        {
            var format = qualitySettings.enableHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
            var renderTexture = new RenderTexture(size, size, 24, format);
            
            renderTexture.antiAliasing = (int)qualitySettings.antiAliasingLevel;
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.anisoLevel = qualitySettings.anisotropicFiltering;
            renderTexture.Create();
            
            return renderTexture;
        }
        
        private static Texture2D ScaleTexture(Texture2D source, int targetSize)
        {
            var result = new Texture2D(targetSize, targetSize, TextureFormat.ARGB32, false);
            var pixels = new Color[targetSize * targetSize];
            
            float scaleX = (float)source.width / targetSize;
            float scaleY = (float)source.height / targetSize;
            
            for (int y = 0; y < targetSize; y++)
            {
                for (int x = 0; x < targetSize; x++)
                {
                    var sourceX = Mathf.FloorToInt(x * scaleX);
                    var sourceY = Mathf.FloorToInt(y * scaleY);
                    pixels[y * targetSize + x] = source.GetPixel(sourceX, sourceY);
                }
            }
            
            result.SetPixels(pixels);
            result.Apply();
            
            UnityEngine.Object.DestroyImmediate(source);
            return result;
        }
        
        private static GameObject InstantiatePrefabInScene(PrefabProcessingInfo prefabInfo, IconGeneratorSettings settings)
        {
            var instance = UnityEngine.Object.Instantiate(prefabInfo.prefab);
            instance.name = prefabInfo.prefabName + "_Instance";
            return instance;
        }
        
        private static void ApplyTransformSettings(GameObject instance, PrefabProcessingInfo prefabInfo, IconGeneratorSettings settings)
        {
            var transform = instance.transform;
            
            // Get transform settings (custom or global)
            var (scale, position, rotation) = prefabInfo.GetTransformSettings();
            
            // Apply global settings if not using custom
            if (!prefabInfo.sourceFolder.useCustomSettings)
            {
                scale = settings.objectScale;
                position = settings.objectPosition;
                rotation = settings.objectRotation;
            }
            
            transform.localScale = scale;
            transform.position = position;
            transform.eulerAngles = rotation;
        }
        
        private static void PositionCameraForObject(Camera camera, GameObject targetObject, IconGeneratorSettings settings)
        {
            if (settings.autoCenter || settings.autoFit)
            {
                var bounds = CalculateObjectBounds(targetObject);
                
                if (settings.autoCenter)
                {
                    // Center camera on object
                    var targetPosition = bounds.center + (camera.transform.position - Vector3.zero);
                    camera.transform.position = targetPosition;
                }
                
                if (settings.autoFit)
                {
                    // Adjust camera distance to fit object
                    var maxExtent = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                    var distance = maxExtent / (2f * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad));
                    
                    var direction = (camera.transform.position - bounds.center).normalized;
                    camera.transform.position = bounds.center + direction * distance * 1.2f; // Add padding
                }
            }
        }
        
        private static Bounds CalculateObjectBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(obj.transform.position, Vector3.one);
            
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1))
            {
                bounds.Encapsulate(renderer.bounds);
            }
            
            return bounds;
        }
        
        private static string GenerateFilename(string prefabName, int size, IconGeneratorSettings settings)
        {
            var baseName = prefabName.Replace("_Instance", "");
            var extension = settings.exportFormat == ExportFormat.PNG ? "png" : "tga";
            
            // Main size gets clean filename, others get size suffix
            if (size == settings.mainIconSize)
            {
                return $"{baseName}_Icon.{extension}";
            }
            else
            {
                return $"{baseName}_Icon_{size}x{size}.{extension}";
            }
        }
        
        private static void SaveTextureToFile(Texture2D texture, string filepath, ExportFormat format)
        {
            byte[] data;
            
            switch (format)
            {
                case ExportFormat.PNG:
                    data = texture.EncodeToPNG();
                    break;
                case ExportFormat.TGA:
                    data = texture.EncodeToTGA();
                    break;
                default:
                    throw new NotSupportedException($"Export format {format} is not supported");
            }
            
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));
            File.WriteAllBytes(filepath, data);
            
            UnityEngine.Object.DestroyImmediate(texture);
        }
        
        private static UnityEngine.SceneManagement.Scene CreateTemporaryScene()
        {
            return UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Additive
            );
        }
        
        private static Camera SetupCamera(IconGeneratorSettings settings)
        {
            var cameraGO = new GameObject("IconGenerator_Camera");
            var camera = cameraGO.AddComponent<Camera>();
            
            camera.transform.position = settings.cameraPosition;
            camera.transform.eulerAngles = settings.cameraRotation;
            camera.fieldOfView = settings.fieldOfView;
            camera.backgroundColor = settings.backgroundColor;
            camera.clearFlags = CameraClearFlags.Color;
            camera.cullingMask = ~0; // Render everything
            
            return camera;
        }
        
        private static void SetupLighting(LightingConfiguration config)
        {
            // Remove existing lights
            var existingLights = UnityEngine.Object.FindObjectsOfType<Light>();
            foreach (var light in existingLights)
            {
                UnityEngine.Object.DestroyImmediate(light.gameObject);
            }
            
            // Setup main light
            if (config.enableMainLight)
            {
                var mainLightGO = new GameObject("IconGenerator_MainLight");
                var mainLight = mainLightGO.AddComponent<Light>();
                mainLight.type = LightType.Directional;
                mainLight.color = config.mainLightColor;
                mainLight.intensity = config.mainLightIntensity;
                mainLight.transform.eulerAngles = config.mainLightDirection;
                mainLight.shadows = LightShadows.Soft;
            }
            
            // Setup fill light
            if (config.enableFillLight)
            {
                var fillLightGO = new GameObject("IconGenerator_FillLight");
                var fillLight = fillLightGO.AddComponent<Light>();
                fillLight.type = LightType.Directional;
                fillLight.color = config.fillLightColor;
                fillLight.intensity = config.fillLightIntensity;
                fillLight.transform.eulerAngles = config.fillLightDirection;
                fillLight.shadows = LightShadows.None;
            }
            
            // Setup point lights
            for (int i = 0; i < config.pointLights.Count; i++)
            {
                var pointLightConfig = config.pointLights[i];
                if (!pointLightConfig.enabled) continue;
                
                var pointLightGO = new GameObject($"IconGenerator_PointLight_{i}");
                var pointLight = pointLightGO.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.color = pointLightConfig.color;
                pointLight.intensity = pointLightConfig.intensity;
                pointLight.range = pointLightConfig.range;
                pointLight.transform.position = pointLightConfig.position;
                pointLight.shadows = LightShadows.Soft;
            }
            
            // Setup ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = config.ambientColor;
            RenderSettings.ambientIntensity = config.ambientIntensity;
        }
        
        private static void ApplyQualitySettings(QualitySettings qualitySettings)
        {
            // Apply Unity's quality settings based on our configuration
            QualitySettings.antiAliasing = (int)qualitySettings.antiAliasingLevel;
            QualitySettings.anisotropicFiltering = qualitySettings.anisotropicFiltering > 1 ? 
                UnityEngine.AnisotropicFiltering.Enable : UnityEngine.AnisotropicFiltering.Disable;
            
            // Shadow settings
            if (qualitySettings.enableShadows)
            {
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = qualitySettings.shadowResolution >= 2048 ? 
                    ShadowResolution.VeryHigh : qualitySettings.shadowResolution >= 1024 ? 
                    ShadowResolution.High : ShadowResolution.Medium;
                QualitySettings.shadowDistance = qualitySettings.shadowDistance;
            }
            else
            {
                QualitySettings.shadows = ShadowQuality.Disable;
            }
        }
        
        private static void ExportConfigurationFile(IconGeneratorSettings settings)
        {
            try
            {
                var configPath = Path.Combine(settings.outputFolderPath, "IconGenerationConfig.json");
                var json = JsonUtility.ToJson(settings, true);
                File.WriteAllText(configPath, json);
                
                currentReport?.LogInfo($"Configuration exported to: {configPath}");
            }
            catch (Exception ex)
            {
                currentReport?.LogError($"Failed to export configuration: {ex.Message}");
            }
        }
        
        private static IEnumerator CleanupMemory()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            yield return null;
        }
        
        private static void ReportProgress(float progress, string status)
        {
            progressCallback?.Invoke(progress, status);
        }
        
        private static void ConfigureReport(IconGeneratorSettings settings)
        {
            if (currentReport == null) return;
            
            currentReport.qualityPreset = settings.qualitySettings.renderQualityPreset;
            currentReport.antiAliasingLevel = settings.qualitySettings.antiAliasingLevel;
            currentReport.outputFormat = settings.exportFormat.ToString();
            currentReport.iconSizes = settings.GetAllSizes();
            currentReport.folderCount = settings.multiFolderManager.GetValidFolders().Count;
            currentReport.totalPrefabs = settings.multiFolderManager.GetTotalPrefabCount();
        }
        
        // Backup and restore methods
        private static CameraSetup StoreCameraSetup()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null) return new CameraSetup();
            
            return new CameraSetup
            {
                position = mainCamera.transform.position,
                rotation = mainCamera.transform.rotation,
                fieldOfView = mainCamera.fieldOfView,
                backgroundColor = mainCamera.backgroundColor,
                clearFlags = mainCamera.clearFlags
            };
        }
        
        private static void RestoreCameraSetup(CameraSetup setup)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null || setup == null) return;
            
            mainCamera.transform.position = setup.position;
            mainCamera.transform.rotation = setup.rotation;
            mainCamera.fieldOfView = setup.fieldOfView;
            mainCamera.backgroundColor = setup.backgroundColor;
            mainCamera.clearFlags = setup.clearFlags;
        }
        
        private static QualitySettingsBackup StoreQualitySettings()
        {
            return new QualitySettingsBackup
            {
                antiAliasing = QualitySettings.antiAliasing,
                anisotropicFiltering = QualitySettings.anisotropicFiltering,
                shadows = QualitySettings.shadows,
                shadowResolution = QualitySettings.shadowResolution,
                shadowDistance = QualitySettings.shadowDistance
            };
        }
        
        private static void RestoreQualitySettings(QualitySettingsBackup backup)
        {
            if (backup == null) return;
            
            QualitySettings.antiAliasing = backup.antiAliasing;
            QualitySettings.anisotropicFiltering = backup.anisotropicFiltering;
            QualitySettings.shadows = backup.shadows;
            QualitySettings.shadowResolution = backup.shadowResolution;
            QualitySettings.shadowDistance = backup.shadowDistance;
        }
        
        private static LightingSettingsBackup StoreLightingSettings()
        {
            return new LightingSettingsBackup
            {
                ambientMode = RenderSettings.ambientMode,
                ambientLight = RenderSettings.ambientLight,
                ambientIntensity = RenderSettings.ambientIntensity
            };
        }
        
        private static void RestoreLightingSettings(LightingSettingsBackup backup)
        {
            if (backup == null) return;
            
            RenderSettings.ambientMode = backup.ambientMode;
            RenderSettings.ambientLight = backup.ambientLight;
            RenderSettings.ambientIntensity = backup.ambientIntensity;
        }
        
        private static void RestoreScene(UnityEngine.SceneManagement.Scene originalScene)
        {
            if (originalScene.IsValid())
            {
                UnityEditor.SceneManagement.EditorSceneManager.SetActiveScene(originalScene);
            }
        }
        
        private static void CleanupGeneration()
        {
            isProcessing = false;
            progressCallback = null;
            EditorApplication.update -= UpdateProgress;
        }
        
        private static void UpdateProgress()
        {
            // This method is called during EditorApplication.update
            // Used to keep the UI responsive during processing
        }
        
        /// <summary>
        /// Cancel the current generation process
        /// </summary>
        public static void CancelGeneration()
        {
            if (isProcessing)
            {
                isProcessing = false;
                currentReport?.LogWarning("Generation cancelled by user");
                Debug.Log("Icon generation cancelled");
            }
        }
        
        /// <summary>
        /// Check if generation is currently in progress
        /// </summary>
        public static bool IsProcessing => isProcessing;
    }
    
    // Backup data structures
    [Serializable]
    public class CameraSetup
    {
        public Vector3 position;
        public Quaternion rotation;
        public float fieldOfView;
        public Color backgroundColor;
        public CameraClearFlags clearFlags;
    }
    
    [Serializable]
    public class QualitySettingsBackup
    {
        public int antiAliasing;
        public UnityEngine.AnisotropicFiltering anisotropicFiltering;
        public ShadowQuality shadows;
        public ShadowResolution shadowResolution;
        public float shadowDistance;
    }
    
    [Serializable]
    public class LightingSettingsBackup
    {
        public UnityEngine.Rendering.AmbientMode ambientMode;
        public Color ambientLight;
        public float ambientIntensity;
    }
    
    /// <summary>
    /// Simple coroutine utility for Editor scripts
    /// </summary>
    public static class EditorCoroutineUtility
    {
        public static void StartCoroutine(IEnumerator routine, object owner)
        {
            EditorApplication.update += () => UpdateCoroutine(routine);
        }
        
        private static void UpdateCoroutine(IEnumerator routine)
        {
            if (!routine.MoveNext())
            {
                EditorApplication.update -= () => UpdateCoroutine(routine);
            }
        }
    }
}