using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;

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
        
        [MenuItem("Tools/Razluta/Unity Icon Generation From Models")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnityIconGenerationWindow>();
            window.titleContent = new GUIContent("Unity Icon Generation");
            window.minSize = new Vector2(400, 600);
        }
        
        public void CreateGUI()
        {
            settings = new IconGeneratorSettings();
            settings.LoadFromPrefs();
            
            // Load UXML
            var visualTree = Resources.Load<VisualTreeAsset>("IconGeneratorWindow");
            if (visualTree == null)
            {
                // Fallback: create UXML programmatically if resource loading fails
                CreateGUIFallback();
                return;
            }
            
            root = rootVisualElement;
            visualTree.CloneTree(root);
            
            BindUIElements();
            UpdatePrefabCount();
            RefreshPointLightsUI();
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
            
            // Title
            var title = new Label("Unity Icon Generation From Models");
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 10;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            container.Add(title);
            
            // Input Settings
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
            
            // Output Settings
            var outputFoldout = new Foldout { text = "Output Settings", value = true };
            container.Add(outputFoldout);
            
            var outputFolder = new ObjectField("Output Folder") { objectType = typeof(DefaultAsset) };
            outputFolder.name = "output-folder";
            outputFoldout.Add(outputFolder);
            
            var iconWidth = new IntegerField("Icon Width");
            iconWidth.name = "icon-width";
            outputFoldout.Add(iconWidth);
            
            var iconHeight = new IntegerField("Icon Height");
            iconHeight.name = "icon-height";
            outputFoldout.Add(iconHeight);
            
            // Camera Settings
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
            
            // Lighting Settings
            var lightingFoldout = new Foldout { text = "Lighting Settings", value = false };
            container.Add(lightingFoldout);
            
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
            
            // Point Lights Section
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
            
            // Preview & Configuration Buttons
            var previewConfigLabel = new Label("Preview & Configuration");
            previewConfigLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            previewConfigLabel.style.marginTop = 20;
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
            setupMockupButton.style.marginRight = 5;
            buttonRow.Add(setupMockupButton);
            
            capturePreviewButton = new Button(() => CapturePreview()) { text = "Capture Preview" };
            capturePreviewButton.name = "capture-preview-button";
            capturePreviewButton.style.height = 30;
            capturePreviewButton.style.flexGrow = 1;
            capturePreviewButton.style.marginLeft = 5;
            buttonRow.Add(capturePreviewButton);
            
            collectConfigButton = new Button(() => CollectSceneConfiguration()) { text = "Collect Scene Configuration" };
            collectConfigButton.name = "collect-config-button";
            collectConfigButton.style.height = 30;
            collectConfigButton.style.marginBottom = 10;
            container.Add(collectConfigButton);
            
            // Advanced Settings
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
            
            // Buttons
            previewButton = new Button(() => PreviewSettings()) { text = "Preview Settings" };
            previewButton.name = "preview-button";
            previewButton.style.height = 30;
            previewButton.style.marginTop = 20;
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
            // Get references to UI elements
            prefabCountLabel = root.Q<Label>("prefab-count");
            statusLabel = root.Q<Label>("status-label");
            generateButton = root.Q<Button>("generate-button");
            previewButton = root.Q<Button>("preview-button");
            pointLightsContainer = root.Q<VisualElement>("point-lights-container");
            addPointLightButton = root.Q<Button>("add-point-light-button");
            setupMockupButton = root.Q<Button>("setup-mockup-button");
            capturePreviewButton = root.Q<Button>("capture-preview-button");
            collectConfigButton = root.Q<Button>("collect-config-button");
            
            // Bind input settings
            var inputFolder = root.Q<ObjectField>("input-folder");
            if (inputFolder != null)
            {
                inputFolder.value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(settings.inputFolderPath);
                inputFolder.RegisterValueChangedCallback(evt => {
                    if (evt.newValue != null)
                        settings.inputFolderPath = AssetDatabase.GetAssetPath(evt.newValue);
                    UpdatePrefabCount();
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
            
            // Bind output settings
            var outputFolder = root.Q<ObjectField>("output-folder");
            if (outputFolder != null)
            {
                outputFolder.value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(settings.outputFolderPath);
                outputFolder.RegisterValueChangedCallback(evt => {
                    if (evt.newValue != null)
                        settings.outputFolderPath = AssetDatabase.GetAssetPath(evt.newValue);
                    settings.SaveToPrefs();
                });
            }
            
            BindField<IntegerField, int>("icon-width", settings.iconWidth, val => settings.iconWidth = val);
            BindField<IntegerField, int>("icon-height", settings.iconHeight, val => settings.iconHeight = val);
            
            // Bind camera settings
            BindField<Vector3Field, Vector3>("camera-position", settings.cameraPosition, val => settings.cameraPosition = val);
            BindField<Vector3Field, Vector3>("camera-rotation", settings.cameraRotation, val => settings.cameraRotation = val);
            BindField<FloatField, float>("camera-fov", settings.cameraFOV, val => settings.cameraFOV = val);
            BindField<ColorField, Color>("background-color", settings.backgroundColor, val => settings.backgroundColor = val);
            
            // Bind lighting settings
            BindField<Vector3Field, Vector3>("main-light-direction", settings.mainLightDirection, val => settings.mainLightDirection = val);
            BindField<ColorField, Color>("main-light-color", settings.mainLightColor, val => settings.mainLightColor = val);
            BindField<FloatField, float>("main-light-intensity", settings.mainLightIntensity, val => settings.mainLightIntensity = val);
            BindField<Vector3Field, Vector3>("fill-light-direction", settings.fillLightDirection, val => settings.fillLightDirection = val);
            BindField<ColorField, Color>("fill-light-color", settings.fillLightColor, val => settings.fillLightColor = val);
            BindField<FloatField, float>("fill-light-intensity", settings.fillLightIntensity, val => settings.fillLightIntensity = val);
            
            // Bind advanced settings
            BindField<FloatField, float>("object-scale", settings.objectScale, val => settings.objectScale = val);
            BindField<Vector3Field, Vector3>("object-position", settings.objectPosition, val => settings.objectPosition = val);
            BindField<Vector3Field, Vector3>("object-rotation", settings.objectRotation, val => settings.objectRotation = val);
            BindField<Toggle, bool>("auto-center", settings.autoCenter, val => settings.autoCenter = val);
            BindField<Toggle, bool>("auto-fit", settings.autoFit, val => settings.autoFit = val);
            
            // Bind buttons
            if (generateButton != null)
                generateButton.clicked += GenerateIcons;
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
        }
        
        private void BindUIElementsFallback()
        {
            // Get references to UI elements
            var inputFolder = root.Q<ObjectField>("input-folder");
            if (inputFolder != null)
            {
                inputFolder.value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(settings.inputFolderPath);
                inputFolder.RegisterValueChangedCallback(evt => {
                    if (evt.newValue != null)
                        settings.inputFolderPath = AssetDatabase.GetAssetPath(evt.newValue);
                    UpdatePrefabCount();
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
                    settings.SaveToPrefs();
                });
            }
            
            // Bind all other fields using the helper method
            BindField<IntegerField, int>("icon-width", settings.iconWidth, val => settings.iconWidth = val);
            BindField<IntegerField, int>("icon-height", settings.iconHeight, val => settings.iconHeight = val);
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
        
        private void RefreshPointLightsUI()
        {
            if (pointLightsContainer == null) return;
            
            // Clear existing UI
            pointLightsContainer.Clear();
            
            // Add UI for each point light
            for (int i = 0; i < settings.pointLights.Count; i++)
            {
                CreatePointLightUI(i);
            }
        }
        
        private void CreatePointLightUI(int index)
        {
            var pointLight = settings.pointLights[index];
            
            // Container for this point light
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
            
            // Header with title and remove button
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
            
            // Enabled toggle
            var enabledToggle = new Toggle("Enabled");
            enabledToggle.value = pointLight.enabled;
            enabledToggle.RegisterValueChangedCallback(evt => {
                pointLight.enabled = evt.newValue;
                settings.SaveToPrefs();
            });
            container.Add(enabledToggle);
            
            // Position field
            var positionField = new Vector3Field("Position");
            positionField.value = pointLight.position;
            positionField.RegisterValueChangedCallback(evt => {
                pointLight.position = evt.newValue;
                settings.SaveToPrefs();
            });
            container.Add(positionField);
            
            // Color field
            var colorField = new ColorField("Color");
            colorField.value = pointLight.color;
            colorField.RegisterValueChangedCallback(evt => {
                pointLight.color = evt.newValue;
                settings.SaveToPrefs();
            });
            container.Add(colorField);
            
            // Intensity field
            var intensityField = new FloatField("Intensity");
            intensityField.value = pointLight.intensity;
            intensityField.RegisterValueChangedCallback(evt => {
                pointLight.intensity = evt.newValue;
                settings.SaveToPrefs();
            });
            container.Add(intensityField);
            
            // Range field
            var rangeField = new FloatField("Range");
            rangeField.value = pointLight.range;
            rangeField.RegisterValueChangedCallback(evt => {
                pointLight.range = evt.newValue;
                settings.SaveToPrefs();
            });
            container.Add(rangeField);
            
            pointLightsContainer.Add(container);
        }
        
        private void PreviewSettings()
        {
            var message = $"Current Settings:\n\n" +
                         $"Input Folder: {settings.inputFolderPath}\n" +
                         $"Prefab Prefix: {settings.prefabNamePrefix}\n" +
                         $"Output Folder: {settings.outputFolderPath}\n" +
                         $"Icon Size: {settings.iconWidth}x{settings.iconHeight}\n" +
                         $"Camera Position: {settings.cameraPosition}\n" +
                         $"Camera FOV: {settings.cameraFOV}°\n" +
                         $"Auto Center: {settings.autoCenter}\n" +
                         $"Auto Fit: {settings.autoFit}\n" +
                         $"Point Lights: {settings.pointLights.Count}";
            
            EditorUtility.DisplayDialog("Unity Icon Generation Settings", message, "OK");
        }
        
        private void GenerateIcons()
        {
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
            
            var tool = new UnityIconGenerationTool(settings);
            tool.GenerateIcons(
                onProgress: (progress) => {
                    statusLabel.text = progress;
                    Repaint();
                },
                onComplete: () => {
                    generateButton.SetEnabled(true);
                    statusLabel.text = "Icons generated successfully!";
                    Repaint();
                }
            );
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
                // Refresh UI to show updated settings
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
            // Refresh all UI elements with current settings values
            var inputFolder = root.Q<ObjectField>("input-folder");
            if (inputFolder != null)
                inputFolder.value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(settings.inputFolderPath);
                
            var prefabPrefix = root.Q<TextField>("prefab-prefix");
            if (prefabPrefix != null)
                prefabPrefix.value = settings.prefabNamePrefix;
                
            var outputFolder = root.Q<ObjectField>("output-folder");
            if (outputFolder != null)
                outputFolder.value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(settings.outputFolderPath);
            
            RefreshField<IntegerField, int>("icon-width", settings.iconWidth);
            RefreshField<IntegerField, int>("icon-height", settings.iconHeight);
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
    }
    
    public class IconPreviewWindow : EditorWindow
    {
        private Texture2D previewTexture;
        
        public void SetPreviewTexture(Texture2D texture)
        {
            previewTexture = texture;
            minSize = new Vector2(texture.width + 20, texture.height + 40);
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
}