# Unity Icon Generation From Models

A Unity Editor tool for generating transparent icons from 3D model prefabs with customizable rendering environments, featuring visual scene mockups and real-time preview capabilities.

![Unity Icon Generation Tool Interface](Samples/SampleUi.png)

## Features

- **Batch Processing**: Generate icons for multiple prefabs at once
- **Visual Scene Mockup**: Create and adjust lighting setups directly in the scene view
- **Real-time Preview**: Capture and preview icons before batch generation
- **Advanced Lighting System**: Main light + fill light + unlimited point lights
- **Customizable Rendering**: Configure camera angle, lighting, and object positioning
- **Transparent Backgrounds**: Generates PNG icons with transparency
- **Auto-fit and Auto-center**: Automatically position and scale objects for optimal framing
- **Scene Management**: Preserves your current scene and handles unsaved changes
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

### Quick Start Workflow

1. **Basic Setup**:
   - Select your prefabs folder (e.g., `Assets/Prefabs`)
   - Enter the prefix your prefabs start with (e.g., `Item_`)
   - Select or create an output folder for generated icons

2. **Visual Configuration**:
   - Click **"Setup Scene Mockup"** to create a visual preview in your scene
   - Manually adjust camera position, lighting, and settings using Unity's normal tools
   - Click **"Capture Preview"** to see exactly what your icon will look like
   - Click **"Collect Scene Configuration"** to save your adjustments back to the tool

3. **Generate Icons**:
   - Click **"Generate Icons"** to batch process all matching prefabs
   - Your original scene will be preserved and restored automatically

### Advanced Configuration

#### Camera Settings
- **Position**: Where the camera is positioned relative to the object
- **Rotation**: Camera angle (Euler angles)
- **Field of View**: Camera FOV in degrees
- **Background Color**: Usually set to transparent (alpha = 0)

#### Lighting System
- **Main Light**: Primary directional light (key light)
- **Fill Light**: Secondary directional light to reduce harsh shadows
- **Point Lights**: Add unlimited point lights for accent lighting, rim lighting, etc.
   - Each point light has position, color, intensity, range, and enabled settings
   - Use "Add Point Light" button to create new lights
   - Individual remove buttons for each light

#### Advanced Object Settings
- **Object Scale**: Global scale multiplier for all objects
- **Object Position/Rotation**: Override positioning for all objects
- **Auto Center**: Automatically center objects in view
- **Auto Fit**: Automatically scale objects to fit nicely in frame

#### Scene Management
- **Smart Scene Handling**: Automatically detects unsaved changes
- **Save Prompts**: Asks user to save before proceeding (optional)
- **Scene Restoration**: Returns to original scene after processing
- **Temporary Scene Creation**: All rendering happens in isolated temporary scenes

## Preview & Configuration Tools

### Setup Scene Mockup
Creates a complete visual mockup of your icon generation setup in the current scene:
- Camera positioned with your exact settings
- All lights (main, fill, point) created and positioned
- Objects organized under "IconGen_MockupRoot" for easy management
- Allows manual fine-tuning using Unity's familiar scene tools

### Capture Preview
Takes a snapshot using your current configuration:
- Renders at exact icon dimensions
- Shows preview in dedicated window
- Save option to export preview as PNG
- Perfect for testing before batch generation

### Collect Scene Configuration
Reads your manual adjustments back into the tool:
- Automatically detects all mockup objects in scene
- Updates UI with collected settings
- Saves configuration for future use
- Enables iterative refinement workflow

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
│   │   ├── SceneMockupTool.cs                       # Scene mockup and preview system
│   │   └── IconGeneratorSettings.cs                 # Settings management
│   └── Resources/
│       └── IconGeneratorWindow.uxml                 # UI layout definition
└── Samples~/
    ├── ExamplePrefabs/                              # Sample prefabs for testing
    └── SampleUi.png                                 # Tool interface screenshot
```

## Technical Details

### Rendering Process

1. **Scene Management**: Preserves current scene and handles unsaved changes
2. **Temporary Scene Creation**: Creates isolated scene with configured lighting and camera
3. **Individual Processing**: Instantiates each prefab individually
4. **Auto-adjustment**: Applies auto-centering and auto-fitting if enabled
5. **High-Quality Rendering**: Renders to RenderTexture with transparency support and 8x MSAA
6. **PNG Export**: Converts to PNG and saves to output directory
7. **Cleanup & Restoration**: Cleans up temporary objects and restores original scene

### Advanced Lighting System

- **Main Directional Light**: Primary key lighting
- **Fill Directional Light**: Secondary shadow fill lighting
- **Unlimited Point Lights**: Dynamic point light system with individual controls
- **Real-time Mockup**: Visual scene representation for manual adjustment
- **Configuration Round-trip**: UI → Scene → Manual Adjustment → UI → Generation

### Performance Considerations

- Uses anti-aliasing (8x MSAA) for high-quality results
- Processes prefabs one at a time to avoid memory issues
- Cleans up resources after each render
- Shows progress bar during batch processing
- Temporary scene isolation prevents interference with main project

## Troubleshooting

### Common Issues

**"No prefabs found" message**
- Check that your input folder path is correct
- Verify your prefab name prefix matches your actual prefab names
- Make sure prefabs are actually in the specified folder (not subfolders)

**"Generate Icons" button disabled**
- Ensure input folder is selected and valid
- Verify output folder is selected
- Check that prefab name prefix is not empty
- Look at Console for debug messages

**Black icons instead of transparent**
- Ensure background color alpha is set to 0
- Check that your materials support transparency
- Verify render texture format supports alpha channel

**Scene mockup not working**
- Ensure you have an active scene open
- Check Console for error messages
- Try closing and reopening the tool window

**Objects appear too small/large**
- Enable "Auto Fit" for automatic scaling
- Adjust "Object Scale" manually
- Use scene mockup to visually adjust camera position/FOV
- Modify point light positions for better illumination

**Poor lighting/shadows**
- Use "Setup Scene Mockup" to visually adjust lighting
- Add point lights for accent lighting
- Adjust main light and fill light directions and intensities
- Use "Capture Preview" to test lighting before batch generation

### Performance Tips

- Process prefabs in smaller batches if you have memory constraints
- Close other Unity windows during generation to free up resources
- Use lower icon resolutions (128x128) for faster processing during testing
- Use scene mockup and preview to perfect settings before batch processing

### Workflow Tips

- **Start with scene mockup** to get visual feedback on your setup
- **Use capture preview extensively** to test different configurations
- **Iterate quickly** with the mockup → preview → adjust cycle
- **Save different configurations** by using "Collect Scene Configuration"
- **Test with one prefab first** before running full batch generation

## Version History

See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

## Support

For issues, feature requests, or contributions, please visit the [GitHub repository](https://github.com/razluta/UnityIconGenerationFromModels).

## License

This tool is provided as-is for educational and commercial use. See the repository for specific license terms.