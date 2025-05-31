using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Razluta.UnityIconGenerationFromModels.Editor
{
    /// <summary>
    /// Main Unity Editor window for Icon Generation Tool v1.2.0
    /// Features quality settings, multi-folder processing, and generation reports
    /// </summary>
    public class IconGeneratorWindow : EditorWindow
    {
        [SerializeField]
        private IconGeneratorSettings settings = new IconGeneratorSettings();
        
        [SerializeField]
        private bool showQualitySettings = true;
        [SerializeField]
        private bool showInputSettings = true;
        [SerializeField]
        private bool showOutputSettings = true;
        [SerializeField]
        private bool showLightingSettings = false;
        [SerializeField]
        private bool showAdvancedSettings = false;
        [SerializeField]
        private bool showReportsPanel = false;
        
        private Vector2 scrollPosition;
        private bool isProcessing = false;
        private float processingProgress = 0f;
        private string processingStatus = "";
        private GenerationReport currentReport;
        
        // UI Style constants
        private const float SECTION_PADDING = 10f;
        private const float BUTTON_HEIGHT = 25f;
        private const float FIELD_HEIGHT = 18f;
        
        [MenuItem("Tools/Razluta/Unity Icon Generation From Models")]
        public static void ShowWindow()
        {
            var window = GetWindow<IconGeneratorWindow>();
            window.titleContent = new GUIContent("Icon Generator v1.2.0");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            LoadSettings();
        }
        
        private void OnDisable()
        {
            SaveSettings();
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawHeader();
            DrawQualitySection();
            DrawInputSection();
            DrawOutputSection();
            DrawLightingSection();
            DrawAdvancedSection();
            DrawGenerationControls();
            DrawProgressSection();
            DrawReportsSection();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.Space(5);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                
                var titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter
                };
                
                if (GUILayout.Button("Unity Icon Generation v1.2.0", titleStyle, GUILayout.Height(30)))
                {
                    Application.OpenURL("https://github.com/razluta/UnityIconGenerationFromModels");
                }
                
                GUILayout.FlexibleSpace();
            }
            
            EditorGUILayout.Space(10);
        }
        
        private void DrawQualitySection()
        {
            showQualitySettings = EditorGUILayout.BeginFoldoutHeaderGroup(showQualitySettings, "🎨 Quality Settings");
            
            if (showQualitySettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Space(5);
                
                // Quality Preset Selection
                var newQualityPreset = (RenderQualityPreset)EditorGUILayout.EnumPopup(
                    new GUIContent("Render Quality Preset", "Choose preset for speed vs quality balance"),
                    settings.qualitySettings.renderQualityPreset
                );
                
                if (newQualityPreset != settings.qualitySettings.renderQualityPreset)
                {
                    settings.ApplyQualityPreset(newQualityPreset);
                }
                
                // Performance Impact Display
                EditorGUILayout.Space(3);
                var impactDescription = settings.qualitySettings.GetPerformanceImpactDescription();
                var impactStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    fontSize = 11,
                    wordWrap = true
                };
                EditorGUILayout.LabelField(impactDescription, impactStyle);
                
                EditorGUILayout.Space(5);
                
                // Anti-aliasing Settings
                settings.qualitySettings.antiAliasingLevel = (AntiAliasingLevel)EditorGUILayout.EnumPopup(
                    new GUIContent("Anti-aliasing", "Higher values = better quality but slower processing"),
                    settings.qualitySettings.antiAliasingLevel
                );
                
                // Advanced Quality Settings
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Advanced Quality", EditorStyles.boldLabel);
                
                settings.qualitySettings.renderScale = EditorGUILayout.Slider(
                    new GUIContent("Render Scale", "Render at higher resolution then downscale for better quality"),
                    settings.qualitySettings.renderScale, 0.5f, 2.0f
                );
                
                settings.qualitySettings.enableHDR = EditorGUILayout.Toggle(
                    new GUIContent("Enable HDR", "High Dynamic Range rendering for better lighting"),
                    settings.qualitySettings.enableHDR
                );
                
                settings.qualitySettings.enableShadows = EditorGUILayout.Toggle(
                    new GUIContent("Enable Shadows", "Render shadows for more realistic icons"),
                    settings.qualitySettings.enableShadows
                );
                
                if (settings.qualitySettings.enableShadows)
                {
                    EditorGUI.indentLevel++;
                    settings.qualitySettings.shadowResolution = EditorGUILayout.IntSlider(
                        new GUIContent("Shadow Resolution", "Higher values = better shadow quality"),
                        settings.qualitySettings.shadowResolution, 256, 4096
                    );
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawInputSection()
        {
            showInputSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showInputSettings, "📁 Input Folders");
            
            if (showInputSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Space(5);
                
                // Multi-folder management
                var folders = settings.multiFolderManager.InputFolders;
                
                for (int i = 0; i < folders.Count; i++)
                {
                    EditorGUILayout.Space(5);
                    
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"Folder {i + 1}", EditorStyles.boldLabel, GUILayout.Width(60));
                            
                            folders[i].isEnabled = EditorGUILayout.Toggle(
                                new GUIContent("Enabled", "Include this folder in processing"),
                                folders[i].isEnabled,
                                GUILayout.Width(60)
                            );
                            
                            GUILayout.FlexibleSpace();
                            
                            if (folders.Count > 1 && GUILayout.Button("✕", GUILayout.Width(25), GUILayout.Height(20)))
                            {
                                settings.multiFolderManager.RemoveFolder(i);
                                break;
                            }
                        }
                        
                        using (new EditorGUI.DisabledScope(!folders[i].isEnabled))
                        {
                            // Folder path selection
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                folders[i].folderPath = EditorGUILayout.TextField(
                                    new GUIContent("Path", "Folder containing prefabs to process"),
                                    folders[i].folderPath
                                );
                                
                                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                                {
                                    var selectedPath = EditorUtility.OpenFolderPanel(
                                        "Select Prefabs Folder",
                                        string.IsNullOrEmpty(folders[i].folderPath) ? Application.dataPath : folders[i].folderPath,
                                        ""
                                    );
                                    
                                    if (!string.IsNullOrEmpty(selectedPath))
                                    {
                                        folders[i].folderPath = selectedPath;
                                    }
                                }
                            }
                            
                            // Prefab prefix
                            folders[i].prefabPrefix = EditorGUILayout.TextField(
                                new GUIContent("Prefix", "Prefix that prefab names must start with"),
                                folders[i].prefabPrefix
                            );
                            
                            // Custom settings toggle
                            folders[i].useCustomSettings = EditorGUILayout.Toggle(
                                new GUIContent("Custom Transform", "Override global transform settings for this folder"),
                                folders[i].useCustomSettings
                            );
                            
                            if (folders[i].useCustomSettings)
                            {
                                EditorGUI.indentLevel++;
                                folders[i].customObjectScale = EditorGUILayout.Vector3Field("Scale", folders[i].customObjectScale);
                                folders[i].customObjectPosition = EditorGUILayout.Vector3Field("Position", folders[i].customObjectPosition);
                                folders[i].customObjectRotation = EditorGUILayout.Vector3Field("Rotation", folders[i].customObjectRotation);
                                EditorGUI.indentLevel--;
                            }
                            
                            // Folder status
                            if (folders[i].isEnabled)
                            {
                                var displayName = folders[i].GetDisplayName();
                                var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                                {
                                    fontStyle = FontStyle.Italic
                                };
                                EditorGUILayout.LabelField(displayName, statusStyle);
                            }
                        }
                    }
                }
                
                EditorGUILayout.Space(5);
                
                // Add folder button
                if (GUILayout.Button("➕ Add Input Folder", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    settings.multiFolderManager.AddFolder();
                }
                
                // Multi-folder summary
                EditorGUILayout.Space(5);
                var summary = settings.multiFolderManager.GetSummary();
                var summaryStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold
                };
                EditorGUILayout.LabelField(summary, summaryStyle);
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawOutputSection()
        {
            showOutputSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showOutputSettings, "💾 Output Settings");
            
            if (showOutputSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Space(5);
                
                // Output folder selection
                using (new EditorGUILayout.HorizontalScope())
                {
                    settings.outputFolderPath = EditorGUILayout.TextField(
                        new GUIContent("Output Folder", "Where generated icons will be saved"),
                        settings.outputFolderPath
                    );
                    
                    if (GUILayout.Button("Browse", GUILayout.Width(60)))
                    {
                        var selectedPath = EditorUtility.OpenFolderPanel(
                            "Select Output Folder",
                            string.IsNullOrEmpty(settings.outputFolderPath) ? Application.dataPath : settings.outputFolderPath,
                            ""
                        );
                        
                        if (!string.IsNullOrEmpty(selectedPath))
                        {
                            settings.outputFolderPath = selectedPath;
                        }
                    }
                }
                
                // Export format
                settings.exportFormat = (ExportFormat)EditorGUILayout.EnumPopup(
                    new GUIContent("Export Format", "PNG recommended for most use cases, TGA for professional pipelines"),
                    settings.exportFormat
                );
                
                EditorGUILayout.Space(8);
                
                // Icon sizes
                EditorGUILayout.LabelField("Icon Sizes", EditorStyles.boldLabel);
                
                settings.mainIconSize = EditorGUILayout.IntPopup(
                    new GUIContent("Main Size", "Primary icon size (gets clean filename)"),
                    settings.mainIconSize,
                    new string[] { "16x16", "32x32", "64x64", "128x128", "256x256", "512x512", "1024x1024", "2048x2048", "4096x4096" },
                    new int[] { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 }
                );
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Additional Sizes", EditorStyles.boldLabel);
                
                // Additional sizes management
                for (int i = 0; i < settings.additionalSizes.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        settings.additionalSizes[i] = EditorGUILayout.IntPopup(
                            settings.additionalSizes[i],
                            new string[] { "16x16", "32x32", "64x64", "128x128", "256x256", "512x512", "1024x1024", "2048x2048", "4096x4096" },
                            new int[] { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 }
                        );
                        
                        if (GUILayout.Button("✕", GUILayout.Width(25)))
                        {
                            settings.additionalSizes.RemoveAt(i);
                            break;
                        }
                    }
                }
                
                if (GUILayout.Button("➕ Add Size Variant", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    settings.additionalSizes.Add(256);
                }
                
                // Output summary
                EditorGUILayout.Space(5);
                var totalIcons = settings.GetTotalIconCount();
                var outputSummary = $"Will generate {totalIcons} icons total ({settings.GetAllSizes().Count} sizes × {settings.multiFolderManager.GetTotalPrefabCount()} prefabs)";
                EditorGUILayout.LabelField(outputSummary, EditorStyles.helpBox);
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawLightingSection()
        {
            showLightingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showLightingSettings, "💡 Lighting Settings");
            
            if (showLightingSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Space(5);
                
                // Lighting preset selection
                var newLightingPreset = (LightingPreset)EditorGUILayout.EnumPopup(
                    new GUIContent("Lighting Preset", "Professional lighting configurations"),
                    settings.lightingPreset
                );
                
                if (newLightingPreset != settings.lightingPreset)
                {
                    settings.lightingPreset = newLightingPreset;
                    // Apply preset configuration would go here
                }
                
                EditorGUILayout.Space(8);
                
                // Main light settings
                EditorGUILayout.LabelField("Main Light", EditorStyles.boldLabel);
                settings.lightingConfiguration.enableMainLight = EditorGUILayout.Toggle("Enable Main Light", settings.lightingConfiguration.enableMainLight);
                
                if (settings.lightingConfiguration.enableMainLight)
                {
                    EditorGUI.indentLevel++;
                    settings.lightingConfiguration.mainLightDirection = EditorGUILayout.Vector3Field("Direction", settings.lightingConfiguration.mainLightDirection);
                    settings.lightingConfiguration.mainLightColor = EditorGUILayout.ColorField("Color", settings.lightingConfiguration.mainLightColor);
                    settings.lightingConfiguration.mainLightIntensity = EditorGUILayout.Slider("Intensity", settings.lightingConfiguration.mainLightIntensity, 0f, 3f);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space(5);
                
                // Fill light settings
                EditorGUILayout.LabelField("Fill Light", EditorStyles.boldLabel);
                settings.lightingConfiguration.enableFillLight = EditorGUILayout.Toggle("Enable Fill Light", settings.lightingConfiguration.enableFillLight);
                
                if (settings.lightingConfiguration.enableFillLight)
                {
                    EditorGUI.indentLevel++;
                    settings.lightingConfiguration.fillLightDirection = EditorGUILayout.Vector3Field("Direction", settings.lightingConfiguration.fillLightDirection);
                    settings.lightingConfiguration.fillLightColor = EditorGUILayout.ColorField("Color", settings.lightingConfiguration.fillLightColor);
                    settings.lightingConfiguration.fillLightIntensity = EditorGUILayout.Slider("Intensity", settings.lightingConfiguration.fillLightIntensity, 0f, 2f);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space(8);
                
                // Point lights
                EditorGUILayout.LabelField("Point Lights", EditorStyles.boldLabel);
                
                for (int i = 0; i < settings.lightingConfiguration.pointLights.Count; i++)
                {
                    var pointLight = settings.lightingConfiguration.pointLights[i];
                    
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            pointLight.enabled = EditorGUILayout.Toggle($"Point Light {i + 1}", pointLight.enabled);
                            GUILayout.FlexibleSpace();
                            
                            if (GUILayout.Button("✕", GUILayout.Width(25)))
                            {
                                settings.lightingConfiguration.RemovePointLight(i);
                                break;
                            }
                        }
                        
                        if (pointLight.enabled)
                        {
                            EditorGUI.indentLevel++;
                            pointLight.position = EditorGUILayout.Vector3Field("Position", pointLight.position);
                            pointLight.color = EditorGUILayout.ColorField("Color", pointLight.color);
                            pointLight.intensity = EditorGUILayout.Slider("Intensity", pointLight.intensity, 0f, 3f);
                            pointLight.range = EditorGUILayout.Slider("Range", pointLight.range, 1f, 20f);
                            EditorGUI.indentLevel--;
                        }
                    }
                }
                
                if (GUILayout.Button("➕ Add Point Light", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    settings.lightingConfiguration.AddPointLight();
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawAdvancedSection()
        {
            showAdvancedSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvancedSettings, "⚙️ Advanced Settings");
            
            if (showAdvancedSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Space(5);
                
                // Camera settings
                EditorGUILayout.LabelField("Camera Settings", EditorStyles.boldLabel);
                settings.cameraPosition = EditorGUILayout.Vector3Field("Position", settings.cameraPosition);
                settings.cameraRotation = EditorGUILayout.Vector3Field("Rotation", settings.cameraRotation);
                settings.fieldOfView = EditorGUILayout.Slider("Field of View", settings.fieldOfView, 10f, 120f);
                settings.backgroundColor = EditorGUILayout.ColorField("Background Color", settings.backgroundColor);
                
                EditorGUILayout.Space(8);
                
                // Object transform
                EditorGUILayout.LabelField("Object Transform", EditorStyles.boldLabel);
                settings.objectScale = EditorGUILayout.Vector3Field("Scale", settings.objectScale);
                settings.objectPosition = EditorGUILayout.Vector3Field("Position", settings.objectPosition);
                settings.objectRotation = EditorGUILayout.Vector3Field("Rotation", settings.objectRotation);
                
                settings.autoCenter = EditorGUILayout.Toggle(
                    new GUIContent("Auto Center", "Automatically center objects in view"),
                    settings.autoCenter
                );
                
                settings.autoFit = EditorGUILayout.Toggle(
                    new GUIContent("Auto Fit", "Automatically scale objects to fit nicely in frame"),
                    settings.autoFit
                );
                
                EditorGUILayout.Space(8);
                
                // Processing options
                EditorGUILayout.LabelField("Processing Options", EditorStyles.boldLabel);
                
                settings.pauseOnError = EditorGUILayout.Toggle(
                    new GUIContent("Pause on Error", "Stop processing when an error occurs"),
                    settings.pauseOnError
                );
                
                settings.enableMemoryOptimization = EditorGUILayout.Toggle(
                    new GUIContent("Memory Optimization", "Use memory-efficient processing (slower but uses less RAM)"),
                    settings.enableMemoryOptimization
                );
                
                settings.exportConfigurationWithIcons = EditorGUILayout.Toggle(
                    new GUIContent("Export Configuration", "Save configuration file alongside icons"),
                    settings.exportConfigurationWithIcons
                );
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawGenerationControls()
        {
            EditorGUILayout.Space(10);
            
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("🚀 Generation Controls", EditorStyles.boldLabel);
                
                EditorGUILayout.Space(5);
                
                // Validation and summary
                var issues = settings.ValidateSettings();
                if (issues.Count > 0)
                {
                    foreach (var issue in issues)
                    {
                        EditorGUILayout.HelpBox(issue, MessageType.Warning);
                    }
                }
                else
                {
                    var estimatedTime = settings.GetEstimatedProcessingTime();
                    var summaryText = $"Ready to generate {settings.GetTotalIconCount()} icons\nEstimated time: {estimatedTime:F1} seconds";
                    EditorGUILayout.HelpBox(summaryText, MessageType.Info);
                }
                
                EditorGUILayout.Space(5);
                
                // Generation controls
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(isProcessing || issues.Count > 0))
                    {
                        if (GUILayout.Button("🎬 Setup Scene Mockup", GUILayout.Height(30)))
                        {
                            SetupSceneMockup();
                        }
                        
                        if (GUILayout.Button("📸 Capture Preview", GUILayout.Height(30)))
                        {
                            CapturePreview();
                        }
                    }
                }
                
                EditorGUILayout.Space(3);
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(isProcessing))
                    {
                        if (GUILayout.Button("💾 Save Configuration", GUILayout.Height(25)))
                        {
                            SaveConfiguration();
                        }
                        
                        if (GUILayout.Button("📂 Load Configuration", GUILayout.Height(25)))
                        {
                            LoadConfiguration();
                        }
                    }
                }
                
                EditorGUILayout.Space(5);
                
                // Main generation button
                using (new EditorGUI.DisabledScope(isProcessing || issues.Count > 0))
                {
                    var buttonText = isProcessing ? "⏳ Processing..." : "🎨 Generate Icons";
                    var buttonColor = isProcessing ? Color.yellow : Color.green;
                    
                    var originalColor = GUI.backgroundColor;
                    GUI.backgroundColor = buttonColor;
                    
                    if (GUILayout.Button(buttonText, GUILayout.Height(40)))
                    {
                        StartGeneration();
                    }
                    
                    GUI.backgroundColor = originalColor;
                }
                
                if (isProcessing)
                {
                    if (GUILayout.Button("⏹️ Cancel Processing", GUILayout.Height(25)))
                    {
                        CancelGeneration();
                    }
                }
            }
        }
        
        private void DrawProgressSection()
        {
            if (isProcessing)
            {
                EditorGUILayout.Space(5);
                
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("📊 Processing Progress", EditorStyles.boldLabel);
                    
                    var progressRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
                    EditorGUI.ProgressBar(progressRect, processingProgress, processingStatus);
                    
                    EditorGUILayout.Space(3);
                    
                    if (currentReport != null && settings.showDetailedProgress)
                    {
                        var recentLogs = currentReport.logEntries.TakeLast(3).ToList();
                        foreach (var log in recentLogs)
                        {
                            var logStyle = new GUIStyle(EditorStyles.miniLabel)
                            {
                                wordWrap = true
                            };
                            EditorGUILayout.LabelField($"{log.GetLevelIcon()} {log.message}", logStyle);
                        }
                    }
                }
            }
        }
        
        private void DrawReportsSection()
        {
            showReportsPanel = EditorGUILayout.BeginFoldoutHeaderGroup(showReportsPanel, "📊 Generation Reports");
            
            if (showReportsPanel)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Space(5);
                
                // Reports settings
                settings.enableReports = EditorGUILayout.Toggle(
                    new GUIContent("Enable Reports", "Generate detailed reports during processing"),
                    settings.enableReports
                );
                
                settings.showDetailedProgress = EditorGUILayout.Toggle(
                    new GUIContent("Show Detailed Progress", "Display recent log entries during processing"),
                    settings.showDetailedProgress
                );
                
                EditorGUILayout.Space(8);
                
                // Reports management
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("📁 Open Reports Folder", GUILayout.Height(25)))
                    {
                        GenerationReportManager.OpenReportsFolder();
                    }
                    
                    if (GUILayout.Button("🗑️ Clear Recent Reports", GUILayout.Height(25)))
                    {
                        GenerationReportManager.ClearRecentReports();
                    }
                }
                
                EditorGUILayout.Space(5);
                
                // Current report summary
                if (currentReport != null)
                {
                    EditorGUILayout.LabelField("Current Session Report", EditorStyles.boldLabel);
                    
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var summary = currentReport.GetSummary();
                        var summaryStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                        {
                            fontSize = 10
                        };
                        EditorGUILayout.LabelField(summary, summaryStyle);
                        
                        if (currentReport.HasErrors || currentReport.HasWarnings)
                        {
                            EditorGUILayout.Space(3);
                            
                            if (currentReport.HasErrors)
                            {
                                EditorGUILayout.HelpBox($"❌ {currentReport.GetErrorEntries().Count} errors occurred", MessageType.Error);
                            }
                            
                            if (currentReport.HasWarnings)
                            {
                                EditorGUILayout.HelpBox($"⚠️ {currentReport.GetWarningEntries().Count} warnings", MessageType.Warning);
                            }
                        }
                    }
                }
                
                // Recent reports
                var recentReports = GenerationReportManager.RecentReports;
                if (recentReports.Count > 0)
                {
                    EditorGUILayout.Space(8);
                    EditorGUILayout.LabelField("Recent Reports", EditorStyles.boldLabel);
                    
                    foreach (var report in recentReports.Take(5))
                    {
                        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                        {
                            var reportLabel = $"{report.sessionStartTime:MM/dd HH:mm} - {report.successfulPrefabs}/{report.totalPrefabs} icons";
                            EditorGUILayout.LabelField(reportLabel, GUILayout.ExpandWidth(true));
                            
                            if (GUILayout.Button("View", GUILayout.Width(40)))
                            {
                                // Show report details - could open in separate window
                                Debug.Log(report.GetDetailedLog());
                            }
                        }
                    }
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        // Implementation methods
        private void SetupSceneMockup()
        {
            // Implementation for scene mockup setup
            Debug.Log("Setting up scene mockup...");
        }
        
        private void CapturePreview()
        {
            // Implementation for preview capture
            Debug.Log("Capturing preview...");
        }
        
        private void StartGeneration()
        {
            isProcessing = true;
            processingProgress = 0f;
            processingStatus = "Starting generation...";
            
            if (settings.enableReports)
            {
                currentReport = GenerationReportManager.StartNewReport();
                currentReport.qualityPreset = settings.qualitySettings.renderQualityPreset;
                currentReport.antiAliasingLevel = settings.qualitySettings.antiAliasingLevel;
                currentReport.outputFormat = settings.exportFormat.ToString();
                currentReport.iconSizes = settings.GetAllSizes();
                currentReport.folderCount = settings.multiFolderManager.GetValidFolders().Count;
                currentReport.totalPrefabs = settings.multiFolderManager.GetTotalPrefabCount();
            }
            
            // Start the actual generation process using the updated IconGeneratorTool
            IconGeneratorTool.GenerateIcons(settings, OnProgressUpdate);
        }
        
        private void OnProgressUpdate(float progress, string status)
        {
            processingProgress = progress;
            processingStatus = status;
            Repaint();
            
            if (progress >= 1.0f)
            {
                isProcessing = false;
                processingProgress = 0f;
                processingStatus = "";
                
                if (currentReport != null)
                {
                    GenerationReportManager.CompleteCurrentReport();
                }
            }
        }
        
        private void CancelGeneration()
        {
            isProcessing = false;
            processingProgress = 0f;
            processingStatus = "";
            
            IconGeneratorTool.CancelGeneration();
            
            if (currentReport != null)
            {
                currentReport.LogWarning("Generation cancelled by user");
                GenerationReportManager.CompleteCurrentReport();
            }
            
            Debug.Log("Generation cancelled");
        }
        
        private void SaveConfiguration()
        {
            var savePath = EditorUtility.SaveFilePanel(
                "Save Configuration",
                Application.dataPath,
                "IconGeneratorConfig",
                "json"
            );
            
            if (!string.IsNullOrEmpty(savePath))
            {
                try
                {
                    var json = JsonUtility.ToJson(settings, true);
                    File.WriteAllText(savePath, json);
                    Debug.Log($"Configuration saved to: {savePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to save configuration: {ex.Message}");
                }
            }
        }
        
        private void LoadConfiguration()
        {
            var loadPath = EditorUtility.OpenFilePanel(
                "Load Configuration",
                Application.dataPath,
                "json"
            );
            
            if (!string.IsNullOrEmpty(loadPath) && File.Exists(loadPath))
            {
                try
                {
                    var json = File.ReadAllText(loadPath);
                    settings = JsonUtility.FromJson<IconGeneratorSettings>(json);
                    Debug.Log($"Configuration loaded from: {loadPath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load configuration: {ex.Message}");
                }
            }
        }
        
        private void LoadSettings()
        {
            var settingsJson = EditorPrefs.GetString("IconGenerator_Settings_v1.2.0", "");
            if (!string.IsNullOrEmpty(settingsJson))
            {
                try
                {
                    settings = JsonUtility.FromJson<IconGeneratorSettings>(settingsJson);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to load saved settings: {ex.Message}");
                    settings = new IconGeneratorSettings();
                }
            }
        }
        
        private void SaveSettings()
        {
            try
            {
                var settingsJson = JsonUtility.ToJson(settings);
                EditorPrefs.SetString("IconGenerator_Settings_v1.2.0", settingsJson);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to save settings: {ex.Message}");
            }
        }
    }
}