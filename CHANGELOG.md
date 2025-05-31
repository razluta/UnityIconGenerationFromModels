# Changelog

All notable changes to the Unity Icon Generation From Models tool will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2025-05-30

### Added
- **Multiple Export Formats**: PNG and TGA format support with format-specific encoding
- **Size Variants System**: Generate multiple icon sizes simultaneously (16x16 to 4096x4096)
- **Smart Size Management**: Power-of-2 dropdown replacing manual width/height input
- **Professional File Naming**: Clean names for main size, size suffixes for variants
- **Additional Size Controls**: Dynamic UI for adding/removing size variants
- **Format Selection**: Dropdown for choosing between PNG and TGA export
- **Complete Icon Sets**: Generate full icon families with single click

### Changed
- **Icon Size Input**: Replaced separate width/height fields with single size dropdown
- **Default Icon Size**: Changed from 256x256 to 512x512 (professional standard)
- **Output Settings UI**: Reorganized for better workflow with format and size options
- **File Naming Convention**: Enhanced naming system for multi-size outputs

### Technical Improvements
- **Multi-format Encoding**: Support for both PNG and TGA texture encoding
- **Efficient Rendering**: Single scene setup with multiple render passes for different sizes
- **Smart File Management**: Automatic file extension and naming based on format and size
- **Settings Persistence**: Additional sizes and export format preferences saved

## [1.0.0] - 2025-05-30

### Added
- Initial release of Unity Icon Generation From Models tool
- Batch processing of prefabs with configurable name prefix filtering
- Advanced lighting system with main light, fill light, and unlimited point lights
- Visual scene mockup system for real-time configuration
- Preview capture functionality with dedicated preview window
- Scene configuration collection (UI → Scene → Manual Adjustment → UI workflow)
- Smart scene management with unsaved changes detection and restoration
- Customizable camera positioning and settings
- Auto-centering and auto-fitting capabilities
- Transparent PNG output with configurable dimensions
- Unity UI Toolkit-based modern editor interface
- Persistent settings using EditorPrefs with point light support
- Real-time prefab count display
- Progress tracking during batch generation
- Comprehensive error handling and validation
- Sample prefabs and UI screenshots for testing

### Technical Features
- Unity 6 compatibility (minimum version requirement)
- Clean temporary scene management with automatic restoration
- Anti-aliased rendering (8x MSAA) for high-quality output
- RGBA32 render texture format for transparency support
- Automatic resource cleanup and memory management
- Assembly definition files for proper package structure
- SceneMockupTool for visual configuration and preview
- IconPreviewWindow for dedicated preview display with save functionality
- Round-trip configuration system (settings ↔ scene objects)

### User Experience Features
- Visual scene mockup creation and manipulation
- Real-time preview capture before batch generation
- Manual lighting and camera adjustment using Unity's familiar tools
- Automatic scene state preservation and restoration
- Dynamic point light creation and management
- Individual point light controls (position, color, intensity, range, enabled)
- Organized UI with collapsible sections and clear workflow
- Comprehensive error handling with helpful user messages
- Debug logging for troubleshooting generation issues

### Workflow Improvements
- Three-stage workflow: Setup → Preview → Generate
- Non-destructive scene management
- Visual feedback for all operations
- Iterative refinement capabilities
- Batch processing with individual prefab progress tracking