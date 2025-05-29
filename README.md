# Unity Icon Generation From Models

A Unity Editor tool for generating transparent icons from 3D model prefabs with customizable rendering environments.

## Features

- **Batch Processing**: Generate icons for multiple prefabs at once
- **Customizable Rendering**: Configure camera angle, lighting, and object positioning
- **Transparent Backgrounds**: Generates PNG icons with transparency
- **Auto-fit and Auto-center**: Automatically position and scale objects for optimal framing
- **Unity 6 Compatible**: Built with Unity's UI Toolkit for modern editor integration
- **Persistent Settings**: Remembers your configuration between sessions

## Installation

### Via Git URL (Recommended)

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button and select "Add package from git URL"
3. Enter: `https://github.com/razluta/UnityIconGenerationFromModels.git`
4. Click "Add"

### Manual Installation

1. Download or clone this repository
2. Copy the entire folder to your project's `Packages` directory
3. Unity will automatically detect and import the package

## Usage

### Opening the Tool

1. Go to `Tools > Razluta > Unity Icon Generation From Models` in the Unity menu bar
2. The Unity Icon Generation window will open

### Basic Setup

1. **Input Settings**:
    - Select your prefabs folder (e.g., `Assets/Prefabs`)
    - Enter the prefix your prefabs start with (e.g., `Item_`)
    - The tool will show how many matching prefabs were found

2. **Output Settings**:
    - Select or create an output folder for generated icons
    - Set the desired icon dimensions (default: 256x256)

3. **Click "Generate Icons"** to start the batch process

### Advanced Configuration

#### Camera Settings
- **Position**: Where the camera is positioned relative to the object
- **Rotation**: Camera angle (Euler angles)
- **Field of View**: Camera FOV in degrees
- **Background Color**: Usually set to transparent (alpha = 0)

#### Lighting Settings
- **Main Light**: Primary directional light (key light)
- **Fill Light**: Secondary light to reduce harsh shadows
- Configure direction, color, and intensity for each light

#### Advanced Object Settings
- **Object Scale**: Global scale multiplier for all objects
- **Object Position/Rotation**: Override positioning for all objects
- **Auto Center**: Automatically center objects in view
- **Auto Fit**: Automatically scale objects to fit nicely in frame

## File Structure

```
com.razluta.unity-icon-generation-from-models/
├── package.json                                    # Package manifest
├── README.md                                       # This file
├── CHANGELOG.md                                    # Version history
├── Runtime/
│   └── UnityIconGenerationFromModels.Runtime.asmdef # Runtime assembly definition
├── Editor/
│   ├── UnityIconGenerationFromModels.Editor.asmdef  # Editor assembly definition
│   ├── Scripts/
│   │   ├── UnityIconGenerationWindow.cs             # Main UI window
│   │   ├── UnityIconGenerationTool.cs               # Core generation logic
│   │   └── IconGeneratorSettings.cs                 # Settings management
│   └── UI/
│       └── IconGeneratorWindow.uxml                 # UI layout definition
└── Samples~/
    └── ExamplePrefabs/                              # Sample prefabs for testing
```

## Technical Details

### Rendering Process

1. Creates a temporary scene with configured lighting and camera
2. Instantiates each prefab individually
3. Applies auto-centering and auto-fitting if enabled
4. Renders to a RenderTexture with transparency support
5. Converts to PNG and saves to the output directory
6. Cleans up temporary objects and scene

### Performance Considerations

- Uses anti-aliasing (8x MSAA) for high-quality results
- Processes prefabs one at a time to avoid memory issues
- Cleans up resources after each render
- Shows progress bar during batch processing

## Troubleshooting

### Common Issues

**"No prefabs found" message**
- Check that your input folder path is correct
- Verify your prefab name prefix matches your actual prefab names
- Make sure prefabs are actually in the specified folder (not subfolders)

**Black icons instead of transparent**
- Ensure background color alpha is set to 0
- Check that your materials support transparency
- Verify render texture format supports alpha channel

**Objects appear too small/large**
- Enable "Auto Fit" for automatic scaling
- Adjust "Object Scale" manually
- Modify camera position/FOV for better framing

**Poor lighting/shadows**
- Adjust main light and fill light directions
- Increase fill light intensity to reduce harsh shadows
- Consider the lighting direction relative to your object's shape

### Performance Tips

- Process prefabs in smaller batches if you have memory constraints
- Close other Unity windows during generation to free up resources
- Use lower icon resolutions (128x128) for faster processing during testing

## Version History

See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

## Support

For issues, feature requests, or contributions, please visit the [GitHub repository](https://github.com/razluta/UnityIconGenerationFromModels).

## License

This tool is provided as-is for educational and commercial use. See the repository for specific license terms. # UI layout definition
└── Samples~/
└── ExamplePrefabs/              # Sample prefabs for testing
```

## Technical Details

### Rendering Process

1. Creates a temporary scene with configured lighting and camera
2. Instantiates each prefab individually
3. Applies auto-centering and auto-fitting if enabled
4. Renders to a RenderTexture with transparency support
5. Converts to PNG and saves to the output directory
6. Cleans up temporary objects and scene

### Performance Considerations

- Uses anti-aliasing (8x MSAA) for high-quality results
- Processes prefabs one at a time to avoid memory issues
- Cleans up resources after each render
- Shows progress bar during batch processing

## Troubleshooting

### Common Issues

**"No prefabs found" message**
- Check that your input folder path is correct
- Verify your prefab name prefix matches your actual prefab names
- Make sure prefabs are actually in the specified folder (not subfolders)

**Black icons instead of transparent**
- Ensure background color alpha is set to 0
- Check that your materials support transparency
- Verify render texture format supports alpha channel

**Objects appear too small/large**
- Enable "Auto Fit" for automatic scaling
- Adjust "Object Scale" manually
- Modify camera position/FOV for better framing

**Poor lighting/shadows**
- Adjust main light and fill light directions
- Increase fill light intensity to reduce harsh shadows
- Consider the lighting direction relative to your object's shape

### Performance Tips

- Process prefabs in smaller batches if you have memory constraints
- Close other Unity windows during generation to free up resources
- Use lower icon resolutions (128x128) for faster processing during testing

## Version History

See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

## Support

For issues, feature requests, or contributions, please visit the [GitHub repository](https://github.com/razluta/icon-generator).

## License

This tool is provided as-is for educational and commercial use. See the repository for specific license terms.