# Changelog

All notable changes to the Unity Icon Generation From Models tool will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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