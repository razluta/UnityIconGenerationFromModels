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
        private Camera renderCamera;
        private Light mainLight;
        private Light fillLight;
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
            
            SetupRenderScene();
            EnsureOutputDirectoryExists();
            
            for (int i = 0; i < prefabs.Count; i++)
            {
                var prefab = prefabs[i];
                var progress = $"Processing {prefab.name} ({i + 1}/{prefabs.Count})";
                onProgress?.Invoke(progress);
                
                GenerateIconForPrefab(prefab);
                
                EditorUtility.DisplayProgressBar("Generating Icons", progress, (float)i / prefabs.Count);
            }
            
            CleanupRenderScene();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            
            Debug.Log($"Successfully generated {prefabs.Count} icons in '{settings.outputFolderPath}'");
            onComplete?.Invoke();
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
            // Create temporary scene
            renderScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            
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
            
            // Create render texture
            var renderTexture = new RenderTexture(settings.iconWidth, settings.iconHeight, 24, RenderTextureFormat.ARGB32);
            renderTexture.antiAliasing = 8;
            
            // Render to texture
            renderCamera.targetTexture = renderTexture;
            renderCamera.Render();
            
            // Convert to Texture2D
            RenderTexture.active = renderTexture;
            var texture2D = new Texture2D(settings.iconWidth, settings.iconHeight, TextureFormat.RGBA32, false);
            texture2D.ReadPixels(new Rect(0, 0, settings.iconWidth, settings.iconHeight), 0, 0);
            texture2D.Apply();
            
            // Save as PNG
            var pngData = texture2D.EncodeToPNG();
            var iconName = prefab.name.Replace(settings.prefabNamePrefix, "");
            var savePath = Path.Combine(settings.outputFolderPath, $"{iconName}_Icon.png");
            File.WriteAllBytes(savePath, pngData);
            
            // Cleanup
            RenderTexture.active = null;
            renderCamera.targetTexture = null;
            Object.DestroyImmediate(renderTexture);
            Object.DestroyImmediate(texture2D);
            Object.DestroyImmediate(currentObject);
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
            }
        }
    }
}