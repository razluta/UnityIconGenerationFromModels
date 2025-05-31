# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2025-05-30

### Added
- **Quality Settings Section**: New comprehensive quality control system
    - **Render Quality Presets**: "Draft" (fast), "Standard", and "High Quality" (slow but premium results)
    - **Anti-aliasing Options**: Configurable AA levels (2x, 4x, 8x, 16x) with speed trade-off indicators
    - **Quality vs Speed Indicators**: Visual feedback on rendering performance impact
- **Multi-folder Processing**: Enhanced batch processing capabilities
    - **Additional Input Folders**: Add unlimited input folders using "Add Folder" button
    - **Folder Management**: Individual remove buttons for each additional folder
    - **Unified Processing**: All folders processed in a single batch operation
- **Generation Reports**: Comprehensive logging and reporting system
    - **Detailed Processing Logs**: What prefabs were processed, errors encountered, timing information
    - **Export Reports**: Save generation reports to disk for record keeping
    - **Real-time Progress**: Live updates during batch processing with detailed status
    - **Performance Metrics**: Processing time per prefab and total generation time
    - **Error Tracking**: Detailed error reporting with suggested solutions

### Enhanced
- **UI Layout**: Reorganized interface with logical grouping of quality, input, and output settings
- **Performance Optimization**: Better memory management during batch processing
- **Error Handling**: Improved error detection and user-friendly error messages
- **Progress Feedback**: Enhanced progress bar with detailed status information

### Technical
- Updated Unity compatibility to 2022.3 LTS and higher
- Improved code architecture for better maintainability
- Enhanced configuration system to support new quality and multi-folder features
- Better resource cleanup and memory management

## [1.1.0] - 2025-05-29

### Added
- Professional lighting presets (Studio, Dramatic, Soft, Product Shot, Cinematic, Technical)
- Visual scene mockup system with real-time preview
- Configuration save/load system for reusable setups
- Advanced lighting system with unlimited point lights
- Auto-fit and auto-center functionality
- Multiple export formats (PNG, TGA) with size variants
- Intelligent scene management and restoration

### Enhanced
- Complete UI overhaul using Unity's UI Toolkit
- Batch processing with progress tracking
- Transparent background support
- Professional anti-aliasing (8x MSAA)

## [1.0.0] - 2025-05-28

### Added
- Initial release of Unity Icon Generation From Models
- Basic icon generation from 3D prefabs
- Customizable camera settings
- Simple lighting controls
- PNG export functionality
- Batch processing for multiple prefabs

[1.2.0]: https://github.com/razluta/UnityIconGenerationFromModels/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/razluta/UnityIconGenerationFromModels/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/razluta/UnityIconGenerationFromModels/releases/tag/v1.0.0