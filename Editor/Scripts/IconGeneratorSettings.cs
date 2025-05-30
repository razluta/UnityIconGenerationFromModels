using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Razluta.UnityIconGenerationFromModels
{
    [System.Serializable]
    public class PointLightSettings
    {
        public Vector3 position = new Vector3(2f, 2f, 2f);
        public Color color = Color.white;
        public float intensity = 1f;
        public float range = 10f;
        public bool enabled = true;
        
        public PointLightSettings()
        {
        }
        
        public PointLightSettings(Vector3 pos, Color col, float intens, float r)
        {
            position = pos;
            color = col;
            intensity = intens;
            range = r;
            enabled = true;
        }
    }

    [System.Serializable]
    public class IconGeneratorSettings
    {
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
        public Vector3 mainLightDirection = new Vector3(-30, 30, 0);
        public Color mainLightColor = Color.white;
        public float mainLightIntensity = 1f;
        public Vector3 fillLightDirection = new Vector3(30, -30, 180);
        public Color fillLightColor = Color.white;
        public float fillLightIntensity = 0.5f;
        
        [Header("Additional Point Lights")]
        public List<PointLightSettings> pointLights = new List<PointLightSettings>();
        
        [Header("Advanced Settings")]
        public float objectScale = 1f;
        public Vector3 objectPosition = Vector3.zero;
        public Vector3 objectRotation = Vector3.zero;
        public bool autoCenter = true;
        public bool autoFit = true;
        
        public void SaveToPrefs()
        {
            EditorPrefs.SetString("UnityIconGen_InputFolder", inputFolderPath);
            EditorPrefs.SetString("UnityIconGen_PrefabPrefix", prefabNamePrefix);
            EditorPrefs.SetString("UnityIconGen_OutputFolder", outputFolderPath);
            EditorPrefs.SetInt("UnityIconGen_IconWidth", iconWidth);
            EditorPrefs.SetInt("UnityIconGen_IconHeight", iconHeight);
            
            EditorPrefs.SetString("UnityIconGen_CameraPos", JsonUtility.ToJson(cameraPosition));
            EditorPrefs.SetString("UnityIconGen_CameraRot", JsonUtility.ToJson(cameraRotation));
            EditorPrefs.SetFloat("UnityIconGen_CameraFOV", cameraFOV);
            EditorPrefs.SetString("UnityIconGen_BackgroundColor", JsonUtility.ToJson(backgroundColor));
            
            EditorPrefs.SetString("UnityIconGen_MainLightDir", JsonUtility.ToJson(mainLightDirection));
            EditorPrefs.SetString("UnityIconGen_MainLightColor", JsonUtility.ToJson(mainLightColor));
            EditorPrefs.SetFloat("UnityIconGen_MainLightIntensity", mainLightIntensity);
            EditorPrefs.SetString("UnityIconGen_FillLightDir", JsonUtility.ToJson(fillLightDirection));
            EditorPrefs.SetString("UnityIconGen_FillLightColor", JsonUtility.ToJson(fillLightColor));
            EditorPrefs.SetFloat("UnityIconGen_FillLightIntensity", fillLightIntensity);
            
            // Save point lights
            EditorPrefs.SetInt("UnityIconGen_PointLightCount", pointLights.Count);
            for (int i = 0; i < pointLights.Count; i++)
            {
                var light = pointLights[i];
                EditorPrefs.SetString($"UnityIconGen_PointLight_{i}_Position", JsonUtility.ToJson(light.position));
                EditorPrefs.SetString($"UnityIconGen_PointLight_{i}_Color", JsonUtility.ToJson(light.color));
                EditorPrefs.SetFloat($"UnityIconGen_PointLight_{i}_Intensity", light.intensity);
                EditorPrefs.SetFloat($"UnityIconGen_PointLight_{i}_Range", light.range);
                EditorPrefs.SetBool($"UnityIconGen_PointLight_{i}_Enabled", light.enabled);
            }
            
            EditorPrefs.SetFloat("UnityIconGen_ObjectScale", objectScale);
            EditorPrefs.SetString("UnityIconGen_ObjectPos", JsonUtility.ToJson(objectPosition));
            EditorPrefs.SetString("UnityIconGen_ObjectRot", JsonUtility.ToJson(objectRotation));
            EditorPrefs.SetBool("UnityIconGen_AutoCenter", autoCenter);
            EditorPrefs.SetBool("UnityIconGen_AutoFit", autoFit);
        }
        
        public void LoadFromPrefs()
        {
            inputFolderPath = EditorPrefs.GetString("UnityIconGen_InputFolder", inputFolderPath);
            prefabNamePrefix = EditorPrefs.GetString("UnityIconGen_PrefabPrefix", prefabNamePrefix);
            outputFolderPath = EditorPrefs.GetString("UnityIconGen_OutputFolder", outputFolderPath);
            iconWidth = EditorPrefs.GetInt("UnityIconGen_IconWidth", iconWidth);
            iconHeight = EditorPrefs.GetInt("UnityIconGen_IconHeight", iconHeight);
            
            if (EditorPrefs.HasKey("UnityIconGen_CameraPos"))
                JsonUtility.FromJsonOverwrite(EditorPrefs.GetString("UnityIconGen_CameraPos"), cameraPosition);
            if (EditorPrefs.HasKey("UnityIconGen_CameraRot"))
                JsonUtility.FromJsonOverwrite(EditorPrefs.GetString("UnityIconGen_CameraRot"), cameraRotation);
            cameraFOV = EditorPrefs.GetFloat("UnityIconGen_CameraFOV", cameraFOV);
            if (EditorPrefs.HasKey("UnityIconGen_BackgroundColor"))
                JsonUtility.FromJsonOverwrite(EditorPrefs.GetString("UnityIconGen_BackgroundColor"), backgroundColor);
            
            if (EditorPrefs.HasKey("UnityIconGen_MainLightDir"))
                JsonUtility.FromJsonOverwrite(EditorPrefs.GetString("UnityIconGen_MainLightDir"), mainLightDirection);
            if (EditorPrefs.HasKey("UnityIconGen_MainLightColor"))
                JsonUtility.FromJsonOverwrite(EditorPrefs.GetString("UnityIconGen_MainLightColor"), mainLightColor);
            mainLightIntensity = EditorPrefs.GetFloat("UnityIconGen_MainLightIntensity", mainLightIntensity);
            if (EditorPrefs.HasKey("UnityIconGen_FillLightDir"))
                JsonUtility.FromJsonOverwrite(EditorPrefs.GetString("UnityIconGen_FillLightDir"), fillLightDirection);
            if (EditorPrefs.HasKey("UnityIconGen_FillLightColor"))
                JsonUtility.FromJsonOverwrite(EditorPrefs.GetString("UnityIconGen_FillLightColor"), fillLightColor);
            fillLightIntensity = EditorPrefs.GetFloat("UnityIconGen_FillLightIntensity", fillLightIntensity);
            
            // Load point lights
            int pointLightCount = EditorPrefs.GetInt("UnityIconGen_PointLightCount", 0);
            pointLights.Clear();
            for (int i = 0; i < pointLightCount; i++)
            {
                var light = new PointLightSettings();
                if (EditorPrefs.HasKey($"UnityIconGen_PointLight_{i}_Position"))
                    JsonUtility.FromJsonOverwrite(EditorPrefs.GetString($"UnityIconGen_PointLight_{i}_Position"), light.position);
                if (EditorPrefs.HasKey($"UnityIconGen_PointLight_{i}_Color"))
                    JsonUtility.FromJsonOverwrite(EditorPrefs.GetString($"UnityIconGen_PointLight_{i}_Color"), light.color);
                light.intensity = EditorPrefs.GetFloat($"UnityIconGen_PointLight_{i}_Intensity", 1f);
                light.range = EditorPrefs.GetFloat($"UnityIconGen_PointLight_{i}_Range", 10f);
                light.enabled = EditorPrefs.GetBool($"UnityIconGen_PointLight_{i}_Enabled", true);
                pointLights.Add(light);
            }
            
            objectScale = EditorPrefs.GetFloat("UnityIconGen_ObjectScale", objectScale);
            if (EditorPrefs.HasKey("UnityIconGen_ObjectPos"))
                JsonUtility.FromJsonOverwrite(EditorPrefs.GetString("UnityIconGen_ObjectPos"), objectPosition);
            if (EditorPrefs.HasKey("UnityIconGen_ObjectRot"))
                JsonUtility.FromJsonOverwrite(EditorPrefs.GetString("UnityIconGen_ObjectRot"), objectRotation);
            autoCenter = EditorPrefs.GetBool("UnityIconGen_AutoCenter", autoCenter);
            autoFit = EditorPrefs.GetBool("UnityIconGen_AutoFit", autoFit);
        }
        
        public void AddPointLight()
        {
            pointLights.Add(new PointLightSettings());
            SaveToPrefs();
        }
        
        public void RemovePointLight(int index)
        {
            if (index >= 0 && index < pointLights.Count)
            {
                pointLights.RemoveAt(index);
                SaveToPrefs();
            }
        }
    }
}