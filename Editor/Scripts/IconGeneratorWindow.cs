using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace Razluta.UnityIconGenerationFromModels
{
    public class UnityIconGenerationWindow : EditorWindow
    {
        private IconGeneratorSettings settings;
        private VisualElement root;
        private Label prefabCountLabel;
        private Label statusLabel;
        private Button generateButton;
        private Button previewButton;
        private VisualElement pointLightsContainer;
        private Button addPointLightButton;
        private Button setupMockupButton;
        private Button capturePreviewButton;
        private Button collectConfigButton;
        private Button saveConfigButton;
        private Button loadConfigButton;
        private Button viewLastReportButton;
        private Button openReportsButton;
        private DropdownField lightingPresetDropdown;
        private DropdownField iconSizeDropdown;
        private DropdownField exportFormatDropdown;
        private VisualElement additionalSizesContainer;
        private Button addSizeButton;
        private Button titleButton;
        private Button versionButton;
        private UnityIconGenerationTool currentTool;
        
        [MenuItem("Tools/Razluta/Unity Icon Generation From Models")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnityIconGenerationWindow>();
            window.titleContent = new GUIContent("Unity Icon Generation");
            window.minSize = new Vector2(400, 700);
        }
        
        public void CreateGUI()
        {
            settings = new IconGeneratorSettings();
            settings.LoadFromPrefs();
            
            var visualTree = Resources.Load<VisualTreeAsset>("IconGeneratorWindow");
            if (visualTree == null)
            {
                CreateGUIFallback();
                return;
            }
            
            root = rootVisualElement;
            visualTree.CloneTree(root);
            
            BindUIElements();
            UpdatePrefabCount();
            RefreshPointLightsUI();
            SetupLightingPresetDropdown();
            SetupIconSizeDropdown();
            SetupExportFormatDropdown();
            RefreshAdditionalSizesUI();
            UpdateReportButtonsState();
        }
        
        private void CreateGUIFallback()
        {
            root = rootVisualElement;
            
            var scrollView = new ScrollView();
            root.Add(scrollView);
            
            var container = new VisualElement();
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            scrollView.Add(container);
            
            // Title button (hyperlinked)
            titleButton = new Button(() => OpenGitHubPage()) { text = "Unity Icon Generation From Models" };
            titleButton.name = "title-button";
            titleButton.style.fontSize = 18;
            titleButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleButton.style.marginBottom = 5;
            titleButton.style.unityTextAlign = TextAnchor.MiddleCenter;
            titleButton.style.backgroundColor = new Color(0, 0, 0, 0);
            titleButton.style.borderTopWidth = 0;
            titleButton.style.borderBottomWidth = 0;
            titleButton.style.borderLeftWidth = 0;
            titleButton.style.borderRightWidth = 0;
            container.Add(titleButton);
            
            // Version button (hyperlinked)
            versionButton = new Button(() => OpenChangelogPage()) { text = "v1.1.0" };
            versionButton.name = "version-button";
            versionButton.style.fontSize = 12;
            versionButton.style.marginBottom = 10;
            versionButton.style.unityTextAlign = TextAnchor.MiddleCenter;
            versionButton.style.backgroundColor = new Color(0, 0, 0, 0);
            versionButton.style.borderTopWidth = 0;
            versionButton.style.borderBottomWidth = 0;
            versionButton.style.borderLeftWidth = 0;
            versionButton.style.borderRightWidth = 0;
            versionButton.style.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            container.Add(versionButton);
            
            var inputFoldout = new Foldout { text = "Input Settings", value = true };
            container.Add(inputFoldout);
            
            var inputFolder = new ObjectField("Input Folder") { objectType = typeof(DefaultAsset) };
            inputFolder.name = "input-folder";
            inputFoldout.Add(inputFolder);
            
            var prefabPrefix = new TextField("Prefab Name Prefix");
            prefabPrefix.name = "prefab-prefix";
            inputFoldout.Add(prefabPrefix);
            
            prefabCountLabel = new Label("Found Prefabs: 0");
            prefabCountLabel.name = "prefab-count";
            prefabCountLabel.style.marginTop = 5;
            prefabCountLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            inputFoldout.Add(prefabCountLabel);
            
            var outputFoldout = new Foldout { text = "Output Settings", value = true };
            container.Add(outputFoldout);
            
            var outputFolder = new ObjectField("Output Folder") { objectType = typeof(DefaultAsset) };
            outputFolder.name = "output-folder";
            outputFoldout.Add(outputFolder);
            
            iconSizeDropdown = new DropdownField("Icon Size");
            iconSizeDropdown.name = "icon-size-dropdown";
            outputFoldout.Add(iconSizeDropdown);
            
            exportFormatDropdown = new DropdownField("Export Format");
            exportFormatDropdown.name = "export-format-dropdown";
            outputFoldout.Add(exportFormatDropdown);
            
            var additionalSizesLabel = new Label("Additional Sizes");
            additionalSizesLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            additionalSizesLabel.style.marginTop = 10;
            outputFoldout.Add(additionalSizesLabel);
            
            additionalSizesContainer = new VisualElement();
            additionalSizesContainer.name = "additional-sizes-container";
            additionalSizesContainer.style.marginTop = 5;
            outputFoldout.Add(additionalSizesContainer);
            
            addSizeButton = new Button(() => AddSizeVariant()) { text = "Add Size Variant" };
            addSizeButton.name = "add-size-button";
            addSizeButton.style.height = 25;
            addSizeButton.style.marginTop = 5;
            outputFoldout.Add(addSizeButton);
            
            var cameraFoldout = new Foldout { text = "Camera Settings", value = false };
            container.Add(cameraFoldout);
            
            var cameraPosition = new Vector3Field("Camera Position");
            cameraPosition.name = "camera-position";
            cameraFoldout.Add(cameraPosition);
            
            var cameraRotation = new Vector3Field("Camera Rotation");
            cameraRotation.name = "camera-rotation";
            cameraFoldout.Add(cameraRotation);
            
            var cameraFov = new FloatField("Field of View");
            cameraFov.name = "camera-fov";
            cameraFoldout.Add(cameraFov);
            
            var backgroundColor = new ColorField("Background Color");
            backgroundColor.name = "background-color";
            cameraFoldout.Add(backgroundColor);
            
            var lightingFoldout = new Foldout { text = "Lighting Settings", value = false };
            container.Add(lightingFoldout);
            
            lightingPresetDropdown = new DropdownField("Lighting Preset");
            lightingPresetDropdown.name = "lighting-preset-dropdown";
            lightingPresetDropdown.style.marginBottom = 10;
            lightingFoldout.Add(lightingPresetDropdown);
            
            var mainLightLabel = new Label("Main Light");
            mainLightLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            lightingFoldout.Add(mainLightLabel);
            
            var mainLightDirection = new Vector3Field("Direction");
            mainLightDirection.name = "main-light-direction";
            lightingFoldout.Add(mainLightDirection);
            
            var mainLightColor = new ColorField("Color");
            mainLightColor.name = "main-light-color";
            lightingFoldout.Add(mainLightColor);
            
            var mainLightIntensity = new FloatField("Intensity");
            mainLightIntensity.name = "main-light-intensity";
            lightingFoldout.Add(mainLightIntensity);
            
            var fillLightLabel = new Label("Fill Light");
            fillLightLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            fillLightLabel.style.marginTop = 10;
            lightingFoldout.Add(fillLightLabel);
            
            var fillLightDirection = new Vector3Field("Direction");
            fillLightDirection.name = "fill-light-direction";
            lightingFoldout.Add(fillLightDirection);
            
            var fillLightColor = new ColorField("Color");
            fillLightColor.name = "fill-light-color";
            lightingFoldout.Add(fillLightColor);
            
            var fillLightIntensity = new FloatField("Intensity");
            fillLightIntensity.name = "fill-light-intensity";
            lightingFoldout.Add(fillLightIntensity);
            
            var pointLightsLabel = new Label("Additional Point Lights");
            pointLightsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            pointLightsLabel.style.marginTop = 10;
            lightingFoldout.Add(pointLightsLabel);
            
            pointLightsContainer = new VisualElement();
            pointLightsContainer.name = "point-lights-container";
            pointLightsContainer.style.marginTop = 5;
            lightingFoldout.Add(pointLightsContainer);
            
            addPointLightButton = new Button(() => AddPointLight()) { text = "Add Point Light" };
            addPointLightButton.name = "add-point-light-button";
            addPointLightButton.style.height = 25;
            addPointLightButton.style.marginTop = 5;
            lightingFoldout.Add(addPointLightButton);
            
            var advancedFoldout = new Foldout { text = "Advanced Settings", value = false };
            container.Add(advancedFoldout);
            
            var objectScale = new FloatField("Object Scale");
            objectScale.name = "object-scale";
            advancedFoldout.Add(objectScale);
            
            var objectPosition = new Vector3Field("Object Position");
            objectPosition.name = "object-position";
            advancedFoldout.Add(objectPosition);
            
            var objectRotation = new Vector3Field("Object Rotation");
            objectRotation.name = "object-rotation";
            advancedFoldout.Add(objectRotation);
            
            var autoCenter = new Toggle("Auto Center");
            autoCenter.name = "auto-center";
            advancedFoldout.Add(autoCenter);
            
            var autoFit = new Toggle("Auto Fit");
            autoFit.name = "auto-fit";
            advancedFoldout.Add(autoFit);
            
            var configPresetsLabel = new Label("Configuration Presets");
            configPresetsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            configPresetsLabel.style.marginTop = 20;
            configPresetsLabel.style.marginBottom = 5;
            container.Add(configPresetsLabel);
            
            var configButtonRow = new VisualElement();
            configButtonRow.style.flexDirection = FlexDirection.Row;
            configButtonRow.style.marginBottom = 10;
            container.Add(configButtonRow);
            
            saveConfigButton = new Button(() => SaveConfiguration()) { text = "Save Configuration" };
            saveConfigButton.name = "save-config-button";
            saveConfigButton.style.height = 30;
            saveConfigButton.style.flexGrow = 1;
            saveConfigButton.style.marginRight = 2;
            configButtonRow.Add(saveConfigButton);
            
            loadConfigButton = new Button(() => LoadConfiguration()) { text = "Load Configuration" };
            loadConfigButton.name = "load-config-button";
            loadConfigButton.style.height = 30;
            loadConfigButton.style.flexGrow = 1;
            loadConfigButton.style.marginLeft = 2;
            configButtonRow.Add(loadConfigButton);
            
            var previewConfigLabel = new Label("Preview & Configuration");
            previewConfigLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            previewConfigLabel.style.marginBottom = 5;
            container.Add(previewConfigLabel);
            
            var buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.marginBottom = 5;
            container.Add(buttonRow);
            
            setupMockupButton = new Button(() => SetupSceneMockup()) { text = "Setup Scene Mockup" };
            setupMockupButton.name = "setup-mockup-button";
            setupMockupButton.style.height = 30;
            setupMockupButton.style.flexGrow = 1;
            setupMockupButton.style.marginRight = 2;
            buttonRow.Add(setupMockupButton);
            
            capturePreviewButton = new Button(() => CapturePreview()) { text = "Capture Preview" };
            capturePreviewButton.name = "capture-preview-button";
            capturePreviewButton.style.height = 30;
            capturePreviewButton.style.flexGrow = 1;
            capturePreviewButton.style.marginLeft = 2;
            buttonRow.Add(capturePreviewButton);
            
            collectConfigButton = new Button(() => CollectSceneConfiguration()) { text = "Collect Scene Configuration" };
            collectConfigButton.name = "collect-config-button";
            collectConfigButton.style.height = 30;
            collectConfigButton.style.marginBottom = 10;
            container.Add(collectConfigButton);
            
            // Add Generation Reports section
            var reportsLabel = new Label("Generation Reports");
            reportsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            reportsLabel.style.marginTop = 10;
            reportsLabel.style.marginBottom = 5;
            container.Add(reportsLabel);
            
            var reportButtonRow = new VisualElement();
            reportButtonRow.style.flexDirection = FlexDirection.Row;
            reportButtonRow.style.marginBottom = 10;
            container.Add(reportButtonRow);
            
            viewLastReportButton = new Button(() => ViewLastReport()) { text = "View Last Report" };
            viewLastReportButton.name = "view-last-report-button";
            viewLastReportButton.style.height = 30;
            viewLastReportButton.style.flexGrow = 1;
            viewLastReportButton.style.marginRight = 2;
            reportButtonRow.Add(viewLastReportButton);
            
            openReportsButton = new Button(() => OpenReportsFolder()) { text = "Open Reports Folder" };
            openReportsButton.name = "open-reports-button";
            openReportsButton.style.height = 30;
            openReportsButton.style.flexGrow = 1;
            openReportsButton.style.marginLeft = 2;
            reportButtonRow.Add(openReportsButton);
            
            previewButton = new Button(() => PreviewSettings()) { text = "Preview Settings" };
            previewButton.name = "preview-button";
            previewButton.style.height = 30;
            previewButton.style.marginBottom = 5;
            container.Add(previewButton);
            
            generateButton = new Button(() => GenerateIcons()) { text = "Generate Icons" };
            generateButton.name = "generate-button";
            generateButton.style.height = 40;
            generateButton.style.fontSize = 14;
            generateButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            container.Add(generateButton);
            
            statusLabel = new Label("");
            statusLabel.name = "status-label";
            statusLabel.style.marginTop = 10;
            statusLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            container.Add(statusLabel);
            
            BindUIElementsFallback();
        }
        
        private void BindUIElements()
        {
            prefabCountLabel = root.Q<Label>("prefab-count");
            statusLabel = root.Q<Label>("status-label");
            generateButton = root.Q<Button>("generate-button");
            previewButton = root.Q<Button>("preview-button");
            pointLightsContainer = root.Q<VisualElement>("point-lights-container");
            addPointLightButton = root.Q<Button>("add-point-light-button");
            setupMockupButton = root.Q<Button>("setup-mockup-button");
            capturePreviewButton = root.Q<Button>("capture-preview-button");
            collectConfigButton = root.Q<Button>("collect-config-button");
            saveConfigButton = root.Q<Button>("save-config-button");
            loadConfigButton = root.Q<Button>("load-config-button");
            viewLastReportButton = root.Q<Button>("view-last-report-button");
            openReportsButton = root.Q<Button>("open-reports-button");
            lightingPresetDropdown = root.Q<DropdownField>("lighting-preset-dropdown");
            iconSizeDropdown = root.Q<DropdownField>("icon-size-dropdown");
            exportFormatDropdown = root.Q<DropdownField>("export-format-dropdown");
            additionalSizesContainer = root.Q<VisualElement>("additional-sizes-container");
            addSizeButton = root.Q<Button>("add-size-button");
            titleButton = root.Q<Button>("title-button");
            versionButton = root.Q<Button>("version-button");
            
            var inputFolder = root.Q<ObjectField>("input-folder");
            if (inputFolder != null)
            {
                inputFolder.value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(settings.inputFolderPath);
                inputFolder.RegisterValueChangedCallback(evt => {
                    if (evt.newValue != null)
                        settings.inputFolderPath = AssetDatabase.GetAssetPath(evt.newValue);
                    UpdatePrefabCount();
                    UpdateReportButtonsState();
                    settings.SaveToPrefs();
                });
            }
            
            var prefabPrefix = root.Q<TextField>("prefab-prefix");
            if (prefabPrefix != null)
            {
                prefabPrefix.value = settings.prefabNamePrefix;
                prefabPrefix.RegisterValueChangedCallback(evt => {
                    settings.prefabNamePrefix = evt.newValue;
                    UpdatePrefabCount();
                    settings.SaveToPrefs();
                });
            }
            
            var outputFolder = root.Q<ObjectField>("output-folder");
            if (outputFolder != null)
            {
                outputFolder.value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(settings.outputFolderPath);
                outputFolder.RegisterValueChangedCallback(evt => {
                    if (evt.newValue != null)
                        settings.outputFolderPath = AssetDatabase.GetAssetPath(evt.newValue);
                    UpdateReportButtonsState();
                    settings.SaveToPrefs();
                });
            }
            
            BindField<Vector3Field, Vector3>("camera-position", settings.cameraPosition, val => settings.cameraPosition = val);
            BindField<Vector3Field, Vector3>("camera-rotation", settings.cameraRotation, val => settings.cameraRotation = val);
            BindField<FloatField, float>("camera-fov", settings.cameraFOV, val => settings.cameraFOV = val);
            BindField<ColorField, Color>("background-color", settings.backgroundColor, val => settings.backgroundColor = val);
            
            BindField<Vector3Field, Vector3>("main-light-direction", settings.mainLightDirection, val => {
                settings.mainLightDirection = val;
                settings.OnLightingChanged();
                UpdateLightingPresetDropdown();
            });
            BindField<ColorField, Color>("main-light-color", settings.mainLightColor, val => {
                settings.mainLightColor = val;
                settings.OnLightingChanged();
                UpdateLightingPresetDropdown();
            });
            BindField<FloatField, float>("main-light-intensity", settings.mainLightIntensity, val => {
                settings.mainLightIntensity = val;
                settings.OnLightingChanged();
                UpdateLightingPresetDropdown();
            });
            BindField<Vector3Field, Vector3>("fill-light-direction", settings.fillLightDirection, val => {
                settings.fillLightDirection = val;
                settings.OnLightingChanged();
                UpdateLightingPresetDropdown();
            });
            BindField<ColorField, Color>("fill-light-color", settings.fillLightColor, val => {
                settings.fillLightColor = val;
                settings.OnLightingChanged();
                UpdateLightingPresetDropdown();
            });
            BindField<FloatField, float>("fill-light-intensity", settings.fillLightIntensity, val => {
                settings.fillLightIntensity = val;
                settings.OnLightingChanged();
                UpdateLightingPresetDropdown();
            });
            
            BindField<FloatField, float>("object-scale", settings.objectScale, val => settings.objectScale = val);
            BindField<Vector3Field, Vector3>("object-position", settings.objectPosition, val => settings.objectPosition = val);
            BindField<Vector3Field, Vector3>("object-rotation", settings.objectRotation, val => settings.objectRotation = val);
            BindField<Toggle, bool>("auto-center", settings.autoCenter, val => settings.autoCenter = val);
            BindField<Toggle, bool>("auto-fit", settings.autoFit, val => settings.autoFit = val);
            
            if (generateButton != null)
            {
                generateButton.clicked += GenerateIcons;
                generateButton.SetEnabled(true);
            }
            if (previewButton != null)
                previewButton.clicked += PreviewSettings;
            if (addPointLightButton != null)
                addPointLightButton.clicked += AddPointLight;
            if (setupMockupButton != null)
                setupMockupButton.clicked += SetupSceneMockup;
            if (capturePreviewButton != null)
                capturePreviewButton.clicked += CapturePreview;
            if (collectConfigButton != null)
                collectConfigButton.clicked += CollectSceneConfiguration;
            if (saveConfigButton != null)
                saveConfigButton.clicked += SaveConfiguration;
            if (loadConfigButton != null)
                loadConfigButton.clicked += LoadConfiguration;
            if (viewLastReportButton != null)
                viewLastReportButton.clicked += ViewLastReport;
            if (openReportsButton != null)
                openReportsButton.clicked += OpenReportsFolder;
            if (addSizeButton != null)
                addSizeButton.clicked += AddSizeVariant;
            if (titleButton != null)
                titleButton.clicked += OpenGitHubPage;
            if (versionButton != null)
                versionButton.clicked += OpenChangelogPage;
        }
        
        private void BindUIElementsFallback()
        {
            BindField<Vector3Field, Vector3>("camera-position", settings.cameraPosition, val => settings.cameraPosition = val);
            BindField<Vector3Field, Vector3>("camera-rotation", settings.cameraRotation, val => settings.cameraRotation = val);
            BindField<FloatField, float>("camera-fov", settings.cameraFOV, val => settings.cameraFOV = val);
            BindField<ColorField, Color>("background-color", settings.backgroundColor, val => settings.backgroundColor = val);
            BindField<Vector3Field, Vector3>("main-light-direction", settings.mainLightDirection, val => settings.mainLightDirection = val);
            BindField<ColorField, Color>("main-light-color", settings.mainLightColor, val => settings.mainLightColor = val);
            BindField<FloatField, float>("main-light-intensity", settings.mainLightIntensity, val => settings.mainLightIntensity = val);
            BindField<Vector3Field, Vector3>("fill-light-direction", settings.fillLightDirection, val => settings.fillLightDirection = val);
            BindField<ColorField, Color>("fill-light-color", settings.fillLightColor, val => settings.fillLightColor = val);
            BindField<FloatField, float>("fill-light-intensity", settings.fillLightIntensity, val => settings.fillLightIntensity = val);
            BindField<FloatField, float>("object-scale", settings.objectScale, val => settings.objectScale = val);
            BindField<Vector3Field, Vector3>("object-position", settings.objectPosition, val => settings.objectPosition = val);
            BindField<Vector3Field, Vector3>("object-rotation", settings.objectRotation, val => settings.objectRotation = val);
            BindField<Toggle, bool>("auto-center", settings.autoCenter, val => settings.autoCenter = val);
            BindField<Toggle, bool>("auto-fit", settings.autoFit, val => settings.autoFit = val);
        }
        
        private void BindField<TField, TValue>(string fieldName, TValue initialValue, System.Action<TValue> onValueChanged)
            where TField : BaseField<TValue>
        {
            var field = root.Q<TField>(fieldName);
            if (field != null)
            {
                field.value = initialValue;
                field.RegisterValueChangedCallback(evt => {
                    onValueChanged(evt.newValue);
                    settings.SaveToPrefs();
                });
            }
        }
        
        private void OpenGitHubPage()
        {
            Application.OpenURL("https://github.com/razluta/UnityIconGenerationFromModels");
        }
        
        private void OpenChangelogPage()
        {
            Application.OpenURL("https://github.com/razluta/UnityIconGenerationFromModels/blob/main/CHANGELOG.md");
        }
        
        private void UpdatePrefabCount()
        {
            if (prefabCountLabel == null) return;
            
            int count = 0;
            if (AssetDatabase.IsValidFolder(settings.inputFolderPath))
            {
                var guids = AssetDatabase.FindAssets("t:Prefab", new[] { settings.inputFolderPath });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null && prefab.name.StartsWith(settings.prefabNamePrefix))
                    {
                        count++;
                    }
                }
            }
            
            prefabCountLabel.text = $"Found Prefabs: {count}";
        }
        
        private void UpdateReportButtonsState()
        {
            if (viewLastReportButton == null || openReportsButton == null) return;
            
            bool hasValidOutputPath = !string.IsNullOrEmpty(settings.outputFolderPath) && 
                                    AssetDatabase.IsValidFolder(settings.outputFolderPath);
            
            if (hasValidOutputPath)
            {
                var reportsFolder = Path.Combine(settings.outputFolderPath, "Reports");
                bool hasReports = Directory.Exists(reportsFolder) && 
                                Directory.GetFiles(reportsFolder, "IconGeneration_Report_*.txt").Length > 0;
                
                viewLastReportButton.SetEnabled(hasReports);
                openReportsButton.SetEnabled(true);
                
                if (!hasReports)
                {
                    viewLastReportButton.tooltip = "No reports found. Generate icons first to create reports.";
                }
                else
                {
                    viewLastReportButton.tooltip = "View the most recent generation report";
                }
                
                openReportsButton.tooltip = "Open the reports folder in file explorer";
            }
            else
            {
                viewLastReportButton.SetEnabled(false);
                openReportsButton.SetEnabled(false);
                viewLastReportButton.tooltip = "Select an output folder first";
                openReportsButton.tooltip = "Select an output folder first";
            }
        }
        
        private void ViewLastReport()
        {
            try
            {
                var reportsFolder = Path.Combine(settings.outputFolderPath, "Reports");
                
                if (!Directory.Exists(reportsFolder))
                {
                    EditorUtility.DisplayDialog("No Reports Found", 
                        "No reports folder found. Generate some icons first to create reports.", "OK");
                    return;
                }
                
                // Find the most recent report file
                var reportFiles = Directory.GetFiles(reportsFolder, "IconGeneration_Report_*.txt");
                
                if (reportFiles.Length == 0)
                {
                    EditorUtility.DisplayDialog("No Reports Found", 
                        "No report files found in the reports folder.", "OK");
                    return;
                }
                
                // Sort by creation time and get the most recent
                Array.Sort(reportFiles, (x, y) => File.GetCreationTime(y).CompareTo(File.GetCreationTime(x)));
                var latestReport = reportFiles[0];
                
                // Read and display the report
                var reportContent = File.ReadAllText(latestReport);
                ShowReportWindow(reportContent, Path.GetFileName(latestReport));
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to load report: {e.Message}", "OK");
            }
        }
        
        private void OpenReportsFolder()
        {
            try
            {
                var reportsFolder = Path.Combine(settings.outputFolderPath, "Reports");
                
                if (!Directory.Exists(reportsFolder))
                {
                    Directory.CreateDirectory(reportsFolder);
                }
                
                // Open the folder in the system file explorer
                EditorUtility.RevealInFinder(reportsFolder);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to open reports folder: {e.Message}", "OK");
            }
        }
        
        private void ShowReportWindow(string reportContent, string reportName)
        {
            var reportWindow = GetWindow<GenerationReportWindow>();
            reportWindow.titleContent = new GUIContent($"Generation Report - {reportName}");
            reportWindow.SetReportContent(reportContent, reportName);
            reportWindow.Show();
        }
        
        private void AddPointLight()
        {
            settings.AddPointLight();
            RefreshPointLightsUI();
        }
        
        private void RemovePointLight(int index)
        {
            settings.RemovePointLight(index);
            RefreshPointLightsUI();
        }
        
        private void SetupLightingPresetDropdown()
        {
            if (lightingPresetDropdown == null) return;
            
            var presetNames = new List<string>();
            var presetTypes = LightingPresets.GetAllPresetTypes();
            
            foreach (var presetType in presetTypes)
            {
                presetNames.Add(LightingPresets.GetPresetDisplayName(presetType));
            }
            
            lightingPresetDropdown.choices = presetNames;
            lightingPresetDropdown.value = LightingPresets.GetPresetDisplayName(settings.currentLightingPreset);
            
            lightingPresetDropdown.RegisterValueChangedCallback(evt => {
                var selectedIndex = lightingPresetDropdown.choices.IndexOf(evt.newValue);
                if (selectedIndex >= 0 && selectedIndex < presetTypes.Length)
                {
                    var selectedPreset = presetTypes[selectedIndex];
                    settings.ApplyLightingPreset(selectedPreset);
                    RefreshLightingUIFromSettings();
                    RefreshPointLightsUI();
                }
            });
        }
        
        private void UpdateLightingPresetDropdown()
        {
            if (lightingPresetDropdown == null) return;
            lightingPresetDropdown.value = LightingPresets.GetPresetDisplayName(settings.currentLightingPreset);
        }
        
        private void RefreshLightingUIFromSettings()
        {
            RefreshField<Vector3Field, Vector3>("main-light-direction", settings.mainLightDirection);
            RefreshField<ColorField, Color>("main-light-color", settings.mainLightColor);
            RefreshField<FloatField, float>("main-light-intensity", settings.mainLightIntensity);
            RefreshField<Vector3Field, Vector3>("fill-light-direction", settings.fillLightDirection);
            RefreshField<ColorField, Color>("fill-light-color", settings.fillLightColor);
            RefreshField<FloatField, float>("fill-light-intensity", settings.fillLightIntensity);
        }
        
        private void SetupIconSizeDropdown()
        {
            if (iconSizeDropdown == null) return;
            
            var availableSizes = IconGeneratorSettings.GetAvailableSizes();
            var sizeNames = new List<string>();
            
            foreach (var size in availableSizes)
            {
                sizeNames.Add(IconGeneratorSettings.GetSizeDisplayName(size));
            }
            
            iconSizeDropdown.choices = sizeNames;
            iconSizeDropdown.value = IconGeneratorSettings.GetSizeDisplayName(settings.iconSize);
            
            iconSizeDropdown.RegisterValueChangedCallback(evt => {
                var selectedIndex = iconSizeDropdown.choices.IndexOf(evt.newValue);
                if (selectedIndex >= 0 && selectedIndex < availableSizes.Length)
                {
                    settings.iconSize = availableSizes[selectedIndex];
                    settings.SaveToPrefs();
                }
            });
        }
        
        private void SetupExportFormatDropdown()
        {
            if (exportFormatDropdown == null) return;
            
            var formatNames = new List<string> { "PNG", "TGA" };
            exportFormatDropdown.choices = formatNames;
            exportFormatDropdown.value = settings.exportFormat == ExportFormat.PNG ? "PNG" : "TGA";
            
            exportFormatDropdown.RegisterValueChangedCallback(evt => {
                settings.exportFormat = evt.newValue == "PNG" ? ExportFormat.PNG : ExportFormat.TGA;
                settings.SaveToPrefs();
            });
        }
        
        private void AddSizeVariant()
        {
            var availableSizes = IconGeneratorSettings.GetAvailableSizes();
            var firstAvailableSize = availableSizes.FirstOrDefault(size => 
                size != settings.iconSize && !settings.additionalSizes.Contains(size));
            
            if (firstAvailableSize > 0)
            {
                settings.additionalSizes.Add(firstAvailableSize);
                settings.SaveToPrefs();
                RefreshAdditionalSizesUI();
            }
            else
            {
                EditorUtility.DisplayDialog("No More Sizes", "All available sizes are already selected.", "OK");
            }
        }
        
        private void RemoveSizeVariant(int size)
        {
            settings.additionalSizes.Remove(size);
            settings.SaveToPrefs();
            RefreshAdditionalSizesUI();
        }
        
        private void RefreshAdditionalSizesUI()
        {
            if (additionalSizesContainer == null) return;
            
            additionalSizesContainer.Clear();
            
            foreach (var size in settings.additionalSizes)
            {
                CreateAdditionalSizeUI(size);
            }
        }
        
        private void CreateAdditionalSizeUI(int size)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.marginBottom = 2;
            
            var label = new Label(IconGeneratorSettings.GetSizeDisplayName(size));
            label.style.flexGrow = 1;
            container.Add(label);
            
            var removeButton = new Button(() => RemoveSizeVariant(size)) { text = "Remove" };
            removeButton.style.height = 20;
            removeButton.style.width = 60;
            container.Add(removeButton);
            
            additionalSizesContainer.Add(container);
        }
        
        private void SaveConfiguration()
        {
            var preset = ConfigurationPreset.FromSettings(settings);
            
            var presetName = EditorUtility.SaveFilePanel(
                "Save Configuration Preset",
                "Assets",
                "IconConfiguration",
                "json"
            );
            
            if (!string.IsNullOrEmpty(presetName))
            {
                preset.presetName = System.IO.Path.GetFileNameWithoutExtension(presetName);
                if (ConfigurationPresetsManager.SaveConfiguration(preset, presetName))
                {
                    statusLabel.text = "Configuration saved successfully!";
                }
            }
        }
        
        private void LoadConfiguration()
        {
            var preset = ConfigurationPresetsManager.LoadConfiguration();
            if (preset != null)
            {
                ConfigurationPresetsManager.ShowConfigurationInfo(preset);
                
                var result = EditorUtility.DisplayDialog(
                    "Load Configuration",
                    $"Load configuration '{preset.presetName}'?\n\nThis will overwrite your current settings.",
                    "Load",
                    "Cancel"
                );
                
                if (result)
                {
                    preset.ApplyToSettings(settings);
                    RefreshAllUIFromSettings();
                    statusLabel.text = $"Configuration '{preset.presetName}' loaded successfully!";
                }
            }
        }
        
        private void RefreshPointLightsUI()
        {
            if (pointLightsContainer == null) return;
            
            pointLightsContainer.Clear();
            
            for (int i = 0; i < settings.pointLights.Count; i++)
            {
                CreatePointLightUI(i);
            }
        }
        
        private void CreatePointLightUI(int index)
        {
            var pointLight = settings.pointLights[index];
            
            var container = new VisualElement();
            container.style.marginBottom = 10;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            container.style.paddingTop = 5;
            container.style.paddingBottom = 5;
            container.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.2f);
            container.style.borderTopLeftRadius = 4;
            container.style.borderTopRightRadius = 4;
            container.style.borderBottomLeftRadius = 4;
            container.style.borderBottomRightRadius = 4;
            
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            container.Add(header);
            
            var title = new Label($"Point Light {index + 1}");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(title);
            
            var removeButton = new Button(() => RemovePointLight(index)) { text = "Remove" };
            removeButton.style.height = 20;
            removeButton.style.width = 60;
            header.Add(removeButton);
            
            var enabledToggle = new Toggle("Enabled");
            enabledToggle.value = pointLight.enabled;
            enabledToggle.RegisterValueChangedCallback(evt => {
                pointLight.enabled = evt.newValue;
                settings.SaveToPrefs();
            });
            container.Add(enabledToggle);
            
            var positionField = new Vector3Field("Position");
            positionField.value = pointLight.position;
            positionField.RegisterValueChangedCallback(evt => {
                pointLight.position = evt.newValue;
                settings.SaveToPrefs();
            });
            container.Add(positionField);
            
            var colorField = new ColorField("Color");
            colorField.value = pointLight.color;
            colorField.RegisterValueChangedCallback(evt => {
                pointLight.color = evt.newValue;
                settings.SaveToPrefs();
            });
            container.Add(colorField);
            
            var intensityField = new FloatField("Intensity");
            intensityField.value = pointLight.intensity;
            intensityField.RegisterValueChangedCallback(evt => {
                pointLight.intensity = evt.newValue;
                settings.SaveToPrefs();
            });
            container.Add(intensityField);
            
            var rangeField = new FloatField("Range");
            rangeField.value = pointLight.range;
            rangeField.RegisterValueChangedCallback(evt => {
                pointLight.range = evt.newValue;
                settings.SaveToPrefs();
            });
            container.Add(rangeField);
            
            pointLightsContainer.Add(container);
        }
        
        private void SetupSceneMockup()
        {
            var mockupTool = new SceneMockupTool(settings);
            mockupTool.SetupMockupScene();
            statusLabel.text = "Scene mockup created! Check your scene view.";
        }
        
        private void CapturePreview()
        {
            var mockupTool = new SceneMockupTool(settings);
            var previewTexture = mockupTool.CapturePreview();
            
            if (previewTexture != null)
            {
                ShowPreviewWindow(previewTexture);
                statusLabel.text = "Preview captured!";
            }
            else
            {
                statusLabel.text = "Failed to capture preview. Make sure scene mockup is set up.";
            }
        }
        
        private void CollectSceneConfiguration()
        {
            var mockupTool = new SceneMockupTool(settings);
            if (mockupTool.CollectSceneConfiguration())
            {
                RefreshAllUIFromSettings();
                statusLabel.text = "Scene configuration collected and updated!";
            }
            else
            {
                statusLabel.text = "No mockup scene found. Please setup scene mockup first.";
            }
        }
        
        private void RefreshAllUIFromSettings()
        {
            var inputFolder = root.Q<ObjectField>("input-folder");
            if (inputFolder != null)
                inputFolder.value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(settings.inputFolderPath);
                
            var prefabPrefix = root.Q<TextField>("prefab-prefix");
            if (prefabPrefix != null)
                prefabPrefix.value = settings.prefabNamePrefix;
                
            var outputFolder = root.Q<ObjectField>("output-folder");
            if (outputFolder != null)
                outputFolder.value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(settings.outputFolderPath);
            
            RefreshField<Vector3Field, Vector3>("camera-position", settings.cameraPosition);
            RefreshField<Vector3Field, Vector3>("camera-rotation", settings.cameraRotation);
            RefreshField<FloatField, float>("camera-fov", settings.cameraFOV);
            RefreshField<ColorField, Color>("background-color", settings.backgroundColor);
            RefreshField<Vector3Field, Vector3>("main-light-direction", settings.mainLightDirection);
            RefreshField<ColorField, Color>("main-light-color", settings.mainLightColor);
            RefreshField<FloatField, float>("main-light-intensity", settings.mainLightIntensity);
            RefreshField<Vector3Field, Vector3>("fill-light-direction", settings.fillLightDirection);
            RefreshField<ColorField, Color>("fill-light-color", settings.fillLightColor);
            RefreshField<FloatField, float>("fill-light-intensity", settings.fillLightIntensity);
            RefreshField<FloatField, float>("object-scale", settings.objectScale);
            RefreshField<Vector3Field, Vector3>("object-position", settings.objectPosition);
            RefreshField<Vector3Field, Vector3>("object-rotation", settings.objectRotation);
            RefreshField<Toggle, bool>("auto-center", settings.autoCenter);
            RefreshField<Toggle, bool>("auto-fit", settings.autoFit);
            
            RefreshPointLightsUI();
            UpdatePrefabCount();
            UpdateLightingPresetDropdown();
            RefreshAdditionalSizesUI();
            UpdateReportButtonsState();
        }
        
        private void RefreshField<TField, TValue>(string fieldName, TValue value)
            where TField : BaseField<TValue>
        {
            var field = root.Q<TField>(fieldName);
            if (field != null)
                field.value = value;
        }
        
        private void ShowPreviewWindow(Texture2D previewTexture)
        {
            var previewWindow = GetWindow<IconPreviewWindow>();
            previewWindow.titleContent = new GUIContent("Icon Preview");
            previewWindow.SetPreviewTexture(previewTexture);
            previewWindow.Show();
        }
        
        private void PreviewSettings()
        {
            var message = $"Current Settings:\n\n" +
                         $"Input Folder: {settings.inputFolderPath}\n" +
                         $"Prefab Prefix: {settings.prefabNamePrefix}\n" +
                         $"Output Folder: {settings.outputFolderPath}\n" +
                         $"Icon Size: {settings.iconSize}x{settings.iconSize}\n" +
                         $"Additional Sizes: {settings.additionalSizes.Count}\n" +
                         $"Export Format: {settings.exportFormat}\n" +
                         $"Camera Position: {settings.cameraPosition}\n" +
                         $"Camera FOV: {settings.cameraFOV}°\n" +
                         $"Auto Center: {settings.autoCenter}\n" +
                         $"Auto Fit: {settings.autoFit}\n" +
                         $"Point Lights: {settings.pointLights.Count}";
            
            EditorUtility.DisplayDialog("Unity Icon Generation Settings", message, "OK");
        }
        
        private void GenerateIcons()
        {
            Debug.Log("Generate Icons button clicked");
            Debug.Log($"Input folder: '{settings.inputFolderPath}' (Valid: {AssetDatabase.IsValidFolder(settings.inputFolderPath)})");
            Debug.Log($"Output folder: '{settings.outputFolderPath}'");
            Debug.Log($"Prefab prefix: '{settings.prefabNamePrefix}'");
            
            if (string.IsNullOrEmpty(settings.inputFolderPath) || !AssetDatabase.IsValidFolder(settings.inputFolderPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid input folder.", "OK");
                return;
            }
            
            if (string.IsNullOrEmpty(settings.outputFolderPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid output folder.", "OK");
                return;
            }
            
            if (string.IsNullOrEmpty(settings.prefabNamePrefix))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a prefab name prefix.", "OK");
                return;
            }
            
            generateButton.SetEnabled(false);
            statusLabel.text = "Generating icons...";
            
            currentTool = new UnityIconGenerationTool(settings);
            currentTool.GenerateIcons(
                onProgress: (progress) => {
                    statusLabel.text = progress;
                    Repaint();
                },
                onComplete: () => {
                    generateButton.SetEnabled(true);
                    statusLabel.text = "Icons generated successfully! Check the Reports folder for detailed logs.";
                    
                    // Enable the view report button and update state
                    UpdateReportButtonsState();
                    
                    Repaint();
                }
            );
        }
    }
    
    public class IconPreviewWindow : EditorWindow
    {
        private Texture2D previewTexture;
        
        public void SetPreviewTexture(Texture2D texture)
        {
            previewTexture = texture;
            minSize = new Vector2(texture.width + 20, texture.height + 60);
            maxSize = minSize;
        }
        
        private void OnGUI()
        {
            if (previewTexture != null)
            {
                GUILayout.Label("Icon Preview:", EditorStyles.boldLabel);
                GUILayout.Space(5);
                
                var rect = GUILayoutUtility.GetRect(previewTexture.width, previewTexture.height);
                GUI.DrawTexture(rect, previewTexture, ScaleMode.ScaleToFit, true);
                
                GUILayout.Space(10);
                if (GUILayout.Button("Save Preview"))
                {
                    SavePreview();
                }
            }
        }
        
        private void SavePreview()
        {
            if (previewTexture == null) return;
            
            var path = EditorUtility.SaveFilePanel("Save Preview", "Assets", "IconPreview", "png");
            if (!string.IsNullOrEmpty(path))
            {
                var pngData = previewTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(path, pngData);
                
                if (path.StartsWith(Application.dataPath))
                {
                    AssetDatabase.Refresh();
                }
                
                Debug.Log($"Preview saved to: {path}");
            }
        }
    }
}