# Unity Icon Generation From Models v1.2.0

*The creation of this tool was performed with support from various AI tools.*

> ⚡ **NEW in v1.2.0**: Quality presets, multi-folder processing, and comprehensive generation reports!

A Unity Editor tool for generating transparent icons from 3D model prefabs with customizable rendering environments, featuring professional quality controls, multi-folder batch processing, and detailed generation reports.

![Unity Icon Generation Tool](Samples~/SampleUi.png)

## ✨ What's New in v1.2.0

### 🎨 Quality Settings System
- **Render Quality Presets**: Choose from "Draft" (fast), "Standard", or "High Quality" (premium results)
- **Anti-aliasing Options**: Configurable AA levels (2x, 4x, 8x, 16x) with performance impact indicators
- **Advanced Quality Controls**: Render scale, HDR, shadows, anisotropic filtering
- **Performance Feedback**: Real-time indicators showing speed vs quality trade-offs

### 📁 Multi-Folder Processing
- **Multiple Input Folders**: Add unlimited input folders with individual settings
- **Folder-Specific Configuration**: Custom transform settings per folder
- **Unified Processing**: Process all folders in a single batch operation
- **Smart Management**: Enable/disable folders, individual remove buttons

### 📊 Generation Reports
- **Detailed Processing Logs**: Track what was processed, errors, timing information
- **Export Reports**: Save generation reports to disk for record keeping
- **Real-time Progress**: Live updates during processing with detailed status
- **Performance Metrics**: Processing time per prefab and total generation time
- **Error Tracking**: Comprehensive error reporting with suggested solutions

## 🚀 Key Features

### Core Functionality
- **Multiple Export Formats**: Generate icons in PNG or TGA format
- **Size Variants**: Create multiple icon sizes simultaneously (16x16 to 4096x4096)
- **Professional Lighting Presets**: Built-in presets (Studio, Dramatic, Soft, Product Shot, Cinematic, Technical)
- **Complete Configuration Management**: Save and load entire tool configurations as reusable presets
- **Batch Processing**: Generate icons for multiple prefabs at once across multiple folders

### Visual Tools
- **Visual Scene Mockup**: Create and adjust lighting setups directly in the scene view
- **Real-time Preview**: Capture and preview icons before batch generation
- **Advanced Lighting System**: Main light + fill light + unlimited point lights with individual controls
- **Customizable Rendering**: Configure camera angle, lighting, and object positioning

### Quality & Performance
- **Quality Presets**: Optimized configurations for different use cases
- **Transparent Backgrounds**: Generates PNG/TGA icons with transparency
- **Auto-fit and Auto-center**: Automatically position and scale objects for optimal framing
- **Memory Optimization**: Efficient processing with configurable memory management
- **Scene Management**: Preserves your current scene and handles unsaved changes intelligently

### Unity Integration
- **Unity 6 Compatible**: Built with Unity's UI Toolkit for modern editor integration
- **Persistent Settings**: Remembers your configuration between sessions
- **Quick Access Links**: Title and version are hyperlinked to GitHub repository and changelog

## 📦 Installation

### Option A: Unity Package Manager (Recommended)
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button and select "Add package from git URL"
3. Enter: `https://github.com/razluta/UnityIconGenerationFromModels.git`
4. Click "Add"

### Option B: Manual Installation
1. Download or clone this repository
2. Copy the entire folder to your project's `Packages` directory
3. Unity will automatically detect and import the package

## 🎯 Quick Start Guide

### 1. Open the Tool
- Go to `Tools > Razluta > Unity Icon Generation From Models` in the Unity menu bar
- The Unity Icon Generation window will open

### 2. Configure Quality Settings (NEW!)
- **Choose Quality Preset**: Select "Draft" for fast previews, "Standard" for balanced results, or "High Quality" for premium output
- **Adjust Anti-aliasing**: Higher values = better quality but slower processing
- **Monitor Performance Impact**: The tool shows real-time feedback on speed vs quality trade-offs

### 3. Setup Input Folders (Enhanced!)
- **Primary Folder**: Select your prefabs folder (e.g., `Assets/Prefabs`)
- **Add More Folders**: Click "➕ Add Input Folder" to process multiple directories
- **Configure Each Folder**:
    - Set folder path and prefab prefix for each
    - Optionally enable custom transform settings per folder
    - Enable/disable folders as needed

### 4. Configure Output
- **Output Folder**: Select or create an output folder for generated icons
- **Format**: Choose PNG (recommended) or TGA format
- **Icon Sizes**:
    - Select main size from dropdown (16x16 to 4096x4096, default 512x512)
    - Add additional size variants for complete icon sets
- **Size Management**: Add/remove size variants with simple UI controls

### 5. Setup Lighting (Optional)
- **Quick Start**: Select a lighting preset from the dropdown (Studio, Dramatic, etc.)
- **Custom Setup**: Manually configure main light, fill light, and point lights
- **Professional Workflow**: Use scene mockup for visual adjustment

### 6. Visual Configuration (Recommended)
- **Scene Mockup**: Click "Setup Scene Mockup" to create a visual preview in your scene
- **Manual Adjustment**: Use Unity's normal tools to fine-tune camera position and lighting
- **Preview**: Click "Capture Preview" to see exactly what your icons will look like
- **Collect Settings**: Click "Collect Scene Configuration" to save adjustments back to the tool

### 7. Save Configuration (Optional)
- **Save Setup**: Click "Save Configuration" to save your entire setup as a reusable preset
- **Load Setup**: Perfect for different object types (weapons, armor, consumables, etc.)

### 8. Generate Icons
- **Validation**: The tool automatically validates your settings and shows any issues
- **Estimation**: View estimated processing time based on your quality settings
- **Generate**: Click "Generate Icons" to batch process all matching prefabs
- **Progress Tracking**: Monitor real-time progress with detailed status updates
- **Reports**: View comprehensive generation reports with performance metrics

## ⚙️ Configuration Reference

### Quality Settings
- **Draft Mode**: Fast rendering for quick previews and iterations
    - 2x Anti-aliasing, reduced render scale, shadows disabled
    - ⚡ **Performance**: Very fast, good for testing
- **Standard Mode**: Balanced quality and speed for production use
    - 8x Anti-aliasing, full render scale, shadows enabled
    - ⚖️ **Performance**: Good balance of quality and speed
- **High Quality Mode**: Maximum quality for premium results
    - 16x Anti-aliasing, 1.5x render scale, high-resolution shadows
    - 🐌 **Performance**: Slower but premium quality

### Multi-Folder Configuration
- **Primary Folder**: Main prefabs directory
- **Additional Folders**: Unlimited additional input directories
- **Per-Folder Settings**:
    - Custom prefab prefix patterns
    - Individual transform overrides (scale, position, rotation)
    - Enable/disable toggle for selective processing

### Generation Reports
- **Automatic Export**: Reports saved to `ProjectFolder/Reports/`
- **Report Contents**:
    - Processing statistics (success/failure rates)
    - Performance metrics (timing per prefab)
    - Detailed logs with timestamps
    - Configuration summary
    - Error analysis with suggestions
- **Export Formats**: Both human-readable text and structured JSON

### Camera Settings
- **Position**: Where the camera is positioned relative to the object
- **Rotation**: Camera angle (Euler angles)
- **Field of View**: Camera FOV in degrees
- **Background Color**: Usually set to transparent (alpha = 0)

### Professional Lighting Presets
Choose from optimized lighting configurations for different scenarios:
- **Studio**: Balanced professional lighting with warm key and cool fill
- **Dramatic**: High contrast with strong shadows and cinematic feel
- **Soft**: Even, diffused lighting perfect for delicate objects
- **Product Shot**: Clean, minimal shadows with 360° coverage
- **Cinematic**: Moody atmospheric lighting with warm/cool contrast
- **Technical**: Flat, even documentation lighting for technical drawings
- **Custom**: Your own manual lighting configuration

### Lighting System
- **Main Light**: Primary directional light (key light)
- **Fill Light**: Secondary directional light to reduce harsh shadows
- **Point Lights**: Add unlimited point lights for accent lighting, rim lighting, etc.
- **Advanced Controls**: Each light has position, color, intensity, range, and enabled settings
- **Smart Management**: Add/remove point lights with individual controls

### Output Configuration
- **Export Formats**:
    - **PNG**: Lossless compression, excellent for UI and web use
    - **TGA**: Uncompressed, perfect for game engines and professional pipelines
- **Size Management**:
    - Main size gets clean filename: `Sword_Icon.png`
    - Additional sizes get size suffix: `Sword_Icon_256x256.png`
- **Batch Generation**: All sizes generated simultaneously for each prefab

### Advanced Object Controls
- **Object Scale**: Global scale multiplier for all objects
- **Object Position/Rotation**: Override positioning for all objects
- **Auto Center**: Automatically center objects in view
- **Auto Fit**: Automatically scale objects to fit nicely in frame

## 🔧 Advanced Workflows

### Team Collaboration
1. **Save Configurations**: Create standardized setups for different asset types
2. **Share Config Files**: Team members can load the same settings for consistency
3. **Version Control**: Include configuration files in your repository
4. **Quality Standards**: Use consistent quality presets across the team

### Performance Optimization
1. **Use Draft Mode**: For quick iterations and testing
2. **Batch Processing**: Process multiple folders efficiently
3. **Memory Management**: Enable memory optimization for large batches
4. **Progressive Quality**: Start with Draft, refine with High Quality

### Production Pipeline Integration
1. **Configuration Export**: Save settings alongside generated icons
2. **Report Analysis**: Review generation reports for optimization opportunities
3. **Error Handling**: Use detailed error logs to identify and fix issues
4. **Automation**: Save configurations for automated processing

## 📁 Project Structure

```
com.razluta.unity-icon-generation-from-models/
├── package.json                    # Package manifest (v1.2.0)
├── README.md                       # This documentation
├── CHANGELOG.md                    # Version history with v1.2.0 features
├── Runtime/
│   └── UnityIconGenerationFromModels.Runtime.asmdef
├── Editor/
│   ├── UnityIconGenerationFromModels.Editor.asmdef
│   ├── Scripts/
│   │   ├── UnityIconGenerationWindow.cs      # Updated UI (v1.2.0)
│   │   ├── UnityIconGenerationTool.cs        # Enhanced core logic
│   │   ├── QualitySettings.cs                # NEW: Quality system
│   │   ├── MultiFolderSystem.cs              # NEW: Multi-folder processing
│   │   ├── GenerationReports.cs              # NEW: Reporting system
│   │   ├── IconGeneratorSettings.cs          # Updated settings (v1.2.0)
│   │   ├── SceneMockupTool.cs                # Scene mockup system
│   │   ├── LightingPresets.cs                # Professional lighting
│   │   └── ConfigurationPresets.cs           # Configuration management
│   └── Resources/
│       └── IconGeneratorWindow.uxml          # UI layout definition
└── Samples~/
    ├── ExamplePrefabs/                       # Sample prefabs for testing
    └── SampleUi.png                          # Tool interface screenshot
```

## 🚀 Processing Workflow (v1.2.0)

The enhanced processing workflow includes all new v1.2.0 features:

1. **Quality Configuration**: Apply selected quality preset and performance settings
2. **Multi-Folder Validation**: Validate all configured input folders and their prefabs
3. **Report Initialization**: Start generation report with detailed logging
4. **Scene Management**: Create temporary scene and preserve original
5. **Lighting Setup**: Configure professional lighting based on selected preset
6. **Camera Positioning**: Setup camera with quality-optimized render settings
7. **Batch Processing**: Process all prefabs from all folders with progress tracking
8. **Quality Rendering**: Render at configured quality with anti-aliasing and effects
9. **Multi-Size Export**: Generate all requested icon sizes for each prefab
10. **Report Generation**: Save comprehensive reports with performance metrics
11. **Cleanup & Restoration**: Clean up resources and restore original scene

## 🛠️ Troubleshooting (Updated for v1.2.0)

### Quality Issues
**Poor icon quality despite High Quality settings**
- Check that your materials support the selected rendering features
- Verify anti-aliasing is working (check render pipeline compatibility)
- Use render scale > 1.0 for super-sampling
- Enable HDR for better lighting quality
- Check shadow settings if using shadow-casting lights

**Icons appear pixelated or blurry**
- Increase anti-aliasing level (try 8x or 16x MSAA)
- Use higher render scale (1.2x - 1.5x)
- Ensure main icon size matches your target use case
- Check that additional sizes are being generated correctly

### Multi-Folder Issues
**"No prefabs found" message**
- Check that each folder path is correct and accessible
- Verify prefab name prefixes match your actual prefab names
- Make sure prefabs are in the specified folders (not subfolders)
- Check that folders are enabled in the configuration

**Some folders not processing**
- Verify each folder is enabled (checkbox in UI)
- Check folder validation messages in the reports
- Ensure all folder paths exist and are accessible
- Review generation reports for folder-specific errors

### Performance Issues
**Generation taking too long**
- Switch to Draft quality preset for faster processing
- Reduce anti-aliasing level (use 2x or 4x instead of 16x)
- Disable shadows and HDR for speed
- Enable memory optimization
- Process folders in smaller batches

**Out of memory errors**
- Enable memory optimization in advanced settings
- Use lower render scales
- Process fewer prefabs at once
- Close other Unity windows during generation
- Use lower icon resolutions for testing

### Report Issues
**Reports not generating**
- Check that "Enable Reports" is turned on
- Verify write permissions to the Reports folder
- Check Unity Console for detailed error messages
- Try manually creating the Reports folder in your project directory

**Missing performance metrics**
- Ensure generation completed successfully
- Check that timing information is available in logs
- Verify reports are being saved (check Reports folder)

### General Issues
**"Generate Icons" button disabled**
- Check validation messages - resolve all warnings/errors
- Ensure at least one input folder is configured and valid
- Verify output folder is selected and accessible
- Check that prefab name prefixes are not empty

**Generated icons in wrong format**
- Check Export Format dropdown in Output Settings
- PNG recommended for most use cases
- TGA for professional/engine pipelines

**Missing size variants**
- Verify additional sizes are added in Output Settings
- Check output folder for files with size suffixes (e.g., `_256x256`)
- Ensure sufficient disk space for multiple large files

**Black icons instead of transparent backgrounds**
- Ensure background color alpha is set to 0
- Check that your materials support transparency
- Verify render texture format supports alpha channel
- Test with simple materials first

### Scene and Lighting Issues
**Scene mockup not working**
- Ensure you have an active scene open
- Check Console for error messages about scene operations
- Try closing and reopening the tool window
- Verify Unity scene management permissions

**Objects appear too small/large in icons**
- Enable "Auto Fit" for automatic scaling
- Adjust "Object Scale" manually in advanced settings
- Use scene mockup to visually adjust camera position/FOV
- Check per-folder custom transform settings

**Poor lighting/shadows**
- Use "Setup Scene Mockup" to visually adjust lighting
- Try different lighting presets (Studio, Product Shot, etc.)
- Add point lights for accent lighting
- Adjust main light and fill light directions and intensities
- Use "Capture Preview" to test lighting before batch generation

### Memory and Performance Optimization
**Tool freezing during generation**
- Enable memory optimization
- Process prefabs in smaller batches
- Close other Unity windows during generation
- Use lower icon resolutions (128x128) for testing
- Monitor system memory usage

**Slow preview generation**
- Use Draft quality for previews
- Reduce preview size temporarily
- Disable unnecessary visual effects
- Use fewer point lights in lighting setup

## 💡 Best Practices (v1.2.0)

### Quality Management
**Start with presets, then customize**
- Choose appropriate quality preset for your use case
- Use Draft for rapid iteration and testing
- Switch to High Quality for final production renders
- Monitor performance impact indicators

**Quality vs Speed Balance**
- Use Standard quality for most production work
- Reserve High Quality for hero assets or marketing materials
- Use Draft quality during development and testing
- Consider render scale for super-sampling when needed

### Multi-Folder Organization
**Organize by asset type**
- Separate folders for weapons, armor, consumables, etc.
- Use consistent prefab naming conventions
- Configure folder-specific transform settings when needed
- Save different configurations for different asset categories

**Efficient batch processing**
- Group similar assets in the same folders
- Use consistent transform settings when possible
- Process related assets together for efficiency
- Test with small batches before full production runs

### Lighting & Visual Setup
**Professional lighting workflow**
- Start with lighting presets for consistent results
- Use scene mockup extensively for visual feedback
- Capture previews to test different configurations
- Iterate quickly with preset → mockup → preview → adjust cycle

**Lighting best practices**
- Combine presets with manual adjustments for unique looks
- Use Studio preset for clean, professional results
- Try Product Shot for e-commerce style icons
- Use Dramatic preset for hero assets and marketing

### Output & Format Management
**Size planning strategy**
- Choose main size based on primary target platform
- Add common variants (64x64, 128x128, 256x256, 512x512, 1024x1024)
- Consider platform-specific requirements (iOS, Android, PC, Console)
- Test file sizes for large variants (2048x2048+)

**Format selection guidelines**
- Use PNG for most projects (better compression, wider compatibility)
- Consider TGA for game engines requiring uncompressed formats
- PNG preferred for UI systems and web use
- TGA better for professional pipelines requiring exact pixel data

### Project Organization
**Configuration management**
- Save configurations for different asset types ("WeaponIcons.json", "ArmorIcons.json")
- Share configurations with team for consistent visual style
- Version control configuration files alongside your project
- Document your standard configurations for team reference

**Workflow optimization**
- Test with one prefab first before running full batch generation
- Use reports to identify and fix common issues
- Set up automated quality checks using generation reports
- Create standard operating procedures for your team

### Performance Optimization
**Processing efficiency**
- Use memory optimization for large batches
- Process similar assets together
- Monitor generation reports for performance bottlenecks
- Close unnecessary Unity windows during processing

**Quality optimization**
- Profile different quality settings for your specific assets
- Use render scale strategically (higher for small details, standard for large objects)
- Balance anti-aliasing with processing time requirements
- Consider HDR only when lighting quality is critical

## 📊 Version History

### [1.2.0] - 2025-05-30 (Current)
- ✨ **NEW**: Quality settings system with Draft/Standard/High Quality presets
- ✨ **NEW**: Multi-folder processing with unlimited input folders
- ✨ **NEW**: Comprehensive generation reports and logging
- 🔧 **Enhanced**: UI layout with logical grouping and better organization
- 🔧 **Enhanced**: Performance optimization and memory management
- 🔧 **Enhanced**: Error handling and user feedback

### [1.1.0] - 2025-05-29
- Professional lighting presets and visual scene mockup system
- Configuration save/load system and advanced lighting controls
- Complete UI overhaul using Unity's UI Toolkit
- Auto-fit, auto-center, and transparent background support

### [1.0.0] - 2025-05-28
- Initial release with basic icon generation functionality
- Customizable camera settings and simple lighting controls
- PNG export and batch processing capabilities

## 🤝 Contributing

We welcome contributions to improve the Unity Icon Generation tool! Here's how you can help:

### Reporting Issues
- Use the [GitHub Issues](https://github.com/razluta/UnityIconGenerationFromModels/issues) page
- Include Unity version, tool version, and detailed reproduction steps
- Attach generation reports when reporting processing issues

### Feature Requests
- Check existing issues to avoid duplicates
- Describe the use case and expected behavior
- Consider how the feature fits with existing quality/performance systems

### Development
- Fork the repository and create feature branches
- Follow existing code style and documentation patterns
- Test thoroughly with different Unity versions
- Update documentation for new features

## 📄 License

This project is licensed under the MIT License - see the repository for details.

## 🙏 Acknowledgments

- Created with support from various AI tools for development assistance
- Unity Technologies for the excellent Editor framework and UI Toolkit
- The Unity community for feedback and feature suggestions
- Contributors and users who help improve the tool

## 🔗 Links

- **Repository**: [https://github.com/razluta/UnityIconGenerationFromModels](https://github.com/razluta/UnityIconGenerationFromModels)
- **Issues**: [Report bugs and request features](https://github.com/razluta/UnityIconGenerationFromModels/issues)
- **Changelog**: [Detailed version history](https://github.com/razluta/UnityIconGenerationFromModels/blob/main/CHANGELOG.md)

---

**Made with ❤️ for the Unity community**

*For support, issues, or feature requests, please visit the [GitHub repository](https://github.com/razluta/UnityIconGenerationFromModels).*