using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Razluta.UnityIconGenerationFromModels
{
    public class SceneMockupTool
    {
        private IconGeneratorSettings settings;
        private const string MOCKUP_CAMERA_NAME = "IconGen_MockupCamera";
        private const string MOCKUP_MAIN_LIGHT_NAME = "IconGen_MainLight";
        private const string MOCKUP_FILL_LIGHT_NAME = "IconGen_FillLight";
        private const string MOCKUP_POINT_LIGHT_PREFIX = "IconGen_PointLight_";
        private const string MOCKUP_ROOT_NAME = "IconGen_MockupRoot";
        
        public SceneMockupTool(IconGeneratorSettings settings)
        {
            this.settings = settings;
        }
        
        public void SetupMockupScene()
        {
            // Clean up existing mockup if it exists
            CleanupExistingMockup();
            
            // Create root object for organization
            var mockupRoot = new GameObject(MOCKUP_ROOT_NAME);
            mockupRoot.transform.position = Vector3.zero;
            
            // Setup camera
            var cameraGO = new GameObject(MOCKUP_CAMERA_NAME);
            cameraGO.transform.parent = mockupRoot.transform;
            var camera = cameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = settings.backgroundColor;
            camera.fieldOfView = settings.cameraFOV;
            camera.transform.position = settings.cameraPosition;
            camera.transform.rotation = Quaternion.Euler(settings.cameraRotation);
            
            // Setup main light
            var mainLightGO = new GameObject(MOCKUP_MAIN_LIGHT_NAME);
            mainLightGO.transform.parent = mockupRoot.transform;
            var mainLight = mainLightGO.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.color = settings.mainLightColor;
            mainLight.intensity = settings.mainLightIntensity;
            mainLight.transform.rotation = Quaternion.Euler(settings.mainLightDirection);
            
            // Setup fill light
            var fillLightGO = new GameObject(MOCKUP_FILL_LIGHT_NAME);
            fillLightGO.transform.parent = mockupRoot.transform;
            var fillLight = fillLightGO.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = settings.fillLightColor;
            fillLight.intensity = settings.fillLightIntensity;
            fillLight.transform.rotation = Quaternion.Euler(settings.fillLightDirection);
            
            // Setup point lights
            for (int i = 0; i < settings.pointLights.Count; i++)
            {
                var pointLightSettings = settings.pointLights[i];
                if (!pointLightSettings.enabled) continue;
                
                var pointLightGO = new GameObject($"{MOCKUP_POINT_LIGHT_PREFIX}{i}");
                pointLightGO.transform.parent = mockupRoot.transform;
                var pointLight = pointLightGO.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.color = pointLightSettings.color;
                pointLight.intensity = pointLightSettings.intensity;
                pointLight.range = pointLightSettings.range;
                pointLight.transform.position = pointLightSettings.position;
            }
            
            // Select the camera so user can see the setup
            Selection.activeGameObject = cameraGO;
            SceneView.lastActiveSceneView?.FrameSelected();
            
            Debug.Log($"Scene mockup created with {settings.pointLights.Count} point lights. Camera and lights are ready for adjustment.");
        }
        
        public Texture2D CapturePreview()
        {
            var camera = GameObject.Find(MOCKUP_CAMERA_NAME)?.GetComponent<Camera>();
            if (camera == null)
            {
                Debug.LogWarning("No mockup camera found. Please setup scene mockup first.");
                return null;
            }
            
            // Create render texture
            var renderTexture = new RenderTexture(settings.iconWidth, settings.iconHeight, 24, RenderTextureFormat.ARGB32);
            renderTexture.antiAliasing = 8;
            
            // Render to texture
            camera.targetTexture = renderTexture;
            camera.Render();
            
            // Convert to Texture2D
            RenderTexture.active = renderTexture;
            var texture2D = new Texture2D(settings.iconWidth, settings.iconHeight, TextureFormat.RGBA32, false);
            texture2D.ReadPixels(new Rect(0, 0, settings.iconWidth, settings.iconHeight), 0, 0);
            texture2D.Apply();
            
            // Cleanup
            RenderTexture.active = null;
            camera.targetTexture = null;
            Object.DestroyImmediate(renderTexture);
            
            return texture2D;
        }
        
        public bool CollectSceneConfiguration()
        {
            var mockupRoot = GameObject.Find(MOCKUP_ROOT_NAME);
            if (mockupRoot == null)
            {
                Debug.LogWarning("No mockup scene found. Please setup scene mockup first.");
                return false;
            }
            
            // Collect camera settings
            var camera = GameObject.Find(MOCKUP_CAMERA_NAME)?.GetComponent<Camera>();
            if (camera != null)
            {
                settings.cameraPosition = camera.transform.position;
                settings.cameraRotation = camera.transform.rotation.eulerAngles;
                settings.cameraFOV = camera.fieldOfView;
                settings.backgroundColor = camera.backgroundColor;
            }
            
            // Collect main light settings
            var mainLight = GameObject.Find(MOCKUP_MAIN_LIGHT_NAME)?.GetComponent<Light>();
            if (mainLight != null)
            {
                settings.mainLightDirection = mainLight.transform.rotation.eulerAngles;
                settings.mainLightColor = mainLight.color;
                settings.mainLightIntensity = mainLight.intensity;
            }
            
            // Collect fill light settings
            var fillLight = GameObject.Find(MOCKUP_FILL_LIGHT_NAME)?.GetComponent<Light>();
            if (fillLight != null)
            {
                settings.fillLightDirection = fillLight.transform.rotation.eulerAngles;
                settings.fillLightColor = fillLight.color;
                settings.fillLightIntensity = fillLight.intensity;
            }
            
            // Collect point light settings
            settings.pointLights.Clear();
            for (int i = 0; i < 100; i++) // Check up to 100 point lights
            {
                var pointLightGO = GameObject.Find($"{MOCKUP_POINT_LIGHT_PREFIX}{i}");
                if (pointLightGO == null) continue;
                
                var pointLight = pointLightGO.GetComponent<Light>();
                if (pointLight != null && pointLight.type == LightType.Point)
                {
                    var pointLightSettings = new PointLightSettings();
                    pointLightSettings.position = pointLight.transform.position;
                    pointLightSettings.color = pointLight.color;
                    pointLightSettings.intensity = pointLight.intensity;
                    pointLightSettings.range = pointLight.range;
                    pointLightSettings.enabled = pointLight.enabled;
                    
                    settings.pointLights.Add(pointLightSettings);
                }
            }
            
            // Save updated settings
            settings.SaveToPrefs();
            
            Debug.Log($"Collected configuration: Camera, Main Light, Fill Light, and {settings.pointLights.Count} Point Lights");
            return true;
        }
        
        public void CleanupMockupScene()
        {
            CleanupExistingMockup();
            Debug.Log("Mockup scene cleaned up.");
        }
        
        private void CleanupExistingMockup()
        {
            // Find and destroy existing mockup objects
            var existingRoot = GameObject.Find(MOCKUP_ROOT_NAME);
            if (existingRoot != null)
            {
                Object.DestroyImmediate(existingRoot);
            }
            
            // Also clean up any orphaned objects (in case hierarchy was modified)
            var orphanedObjects = new List<GameObject>();
            
            var camera = GameObject.Find(MOCKUP_CAMERA_NAME);
            if (camera != null) orphanedObjects.Add(camera);
            
            var mainLight = GameObject.Find(MOCKUP_MAIN_LIGHT_NAME);
            if (mainLight != null) orphanedObjects.Add(mainLight);
            
            var fillLight = GameObject.Find(MOCKUP_FILL_LIGHT_NAME);
            if (fillLight != null) orphanedObjects.Add(fillLight);
            
            // Clean up point lights
            for (int i = 0; i < 100; i++)
            {
                var pointLight = GameObject.Find($"{MOCKUP_POINT_LIGHT_PREFIX}{i}");
                if (pointLight != null) orphanedObjects.Add(pointLight);
            }
            
            foreach (var obj in orphanedObjects)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}