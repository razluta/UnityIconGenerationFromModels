using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

namespace Razluta.UnityIconGenerationFromModels
{
    [System.Serializable]
    public class GenerationReportEntry
    {
        public string prefabName;
        public string prefabPath;
        public bool success;
        public string errorMessage;
        public float processingTimeSeconds;
        public List<GeneratedIconInfo> generatedIcons;
        public DateTime timestamp;

        public GenerationReportEntry()
        {
            generatedIcons = new List<GeneratedIconInfo>();
            timestamp = DateTime.Now;
        }
    }

    [System.Serializable]
    public class GeneratedIconInfo
    {
        public string fileName;
        public string filePath;
        public int size;
        public ExportFormat format;
        public long fileSizeBytes;
        public bool success;
        public string errorMessage;

        public GeneratedIconInfo(string fileName, string filePath, int size, ExportFormat format)
        {
            this.fileName = fileName;
            this.filePath = filePath;
            this.size = size;
            this.format = format;
            this.success = true;
            this.errorMessage = "";
        }
    }

    [System.Serializable]
    public class GenerationReport
    {
        [Header("Session Information")]
        public string sessionId;
        public DateTime startTime;
        public DateTime endTime;
        public float totalDurationSeconds;
        public string unityVersion;
        public string toolVersion;

        [Header("Configuration Used")]
        public string inputFolderPath;
        public string outputFolderPath;
        public string prefabNamePrefix;
        public int mainIconSize;
        public List<int> additionalSizes;
        public ExportFormat exportFormat;
        public LightingPresetType lightingPreset;
        public int pointLightCount;

        [Header("Processing Results")]
        public int totalPrefabsFound;
        public int totalPrefabsProcessed;
        public int successfulPrefabs;
        public int failedPrefabs;
        public int totalIconsGenerated;
        public int totalIconsFailed;
        public long totalFileSizeBytes;
        public float averageProcessingTimePerPrefab;
        public float fastestPrefabTime;
        public float slowestPrefabTime;
        public string fastestPrefabName;
        public string slowestPrefabName;

        [Header("Detailed Results")]
        public List<GenerationReportEntry> entries;
        public List<string> globalErrors;
        public List<string> warnings;

        public GenerationReport()
        {
            sessionId = Guid.NewGuid().ToString();
            startTime = DateTime.Now;
            unityVersion = Application.unityVersion;
            toolVersion = "1.1.0";
            entries = new List<GenerationReportEntry>();
            globalErrors = new List<string>();
            warnings = new List<string>();
            additionalSizes = new List<int>();
            
            // Initialize timing values
            fastestPrefabTime = float.MaxValue;
            slowestPrefabTime = 0f;
            fastestPrefabName = "";
            slowestPrefabName = "";
        }

        public void StartGeneration(IconGeneratorSettings settings)
        {
            startTime = DateTime.Now;
            
            // Capture configuration
            inputFolderPath = settings.inputFolderPath;
            outputFolderPath = settings.outputFolderPath;
            prefabNamePrefix = settings.prefabNamePrefix;
            mainIconSize = settings.iconSize;
            additionalSizes = new List<int>(settings.additionalSizes);
            exportFormat = settings.exportFormat;
            lightingPreset = settings.currentLightingPreset;
            pointLightCount = settings.pointLights.Count;
        }

        public void EndGeneration()
        {
            endTime = DateTime.Now;
            totalDurationSeconds = (float)(endTime - startTime).TotalSeconds;
            
            // Calculate statistics
            CalculateStatistics();
        }

        public GenerationReportEntry StartPrefabProcessing(string prefabName, string prefabPath)
        {
            var entry = new GenerationReportEntry();
            entry.prefabName = prefabName;
            entry.prefabPath = prefabPath;
            entries.Add(entry);
            return entry;
        }

        public void CompletePrefabProcessing(GenerationReportEntry entry, bool success, string errorMessage = "")
        {
            entry.success = success;
            entry.errorMessage = errorMessage;
            
            if (success)
            {
                successfulPrefabs++;
            }
            else
            {
                failedPrefabs++;
            }
            
            totalPrefabsProcessed++;
            
            // Update timing statistics
            if (entry.processingTimeSeconds < fastestPrefabTime)
            {
                fastestPrefabTime = entry.processingTimeSeconds;
                fastestPrefabName = entry.prefabName;
            }
            
            if (entry.processingTimeSeconds > slowestPrefabTime)
            {
                slowestPrefabTime = entry.processingTimeSeconds;
                slowestPrefabName = entry.prefabName;
            }
        }

        public void AddIconGenerated(GenerationReportEntry entry, GeneratedIconInfo iconInfo)
        {
            entry.generatedIcons.Add(iconInfo);
            
            if (iconInfo.success)
            {
                totalIconsGenerated++;
                totalFileSizeBytes += iconInfo.fileSizeBytes;
            }
            else
            {
                totalIconsFailed++;
            }
        }

        public void AddGlobalError(string error)
        {
            globalErrors.Add($"[{DateTime.Now:HH:mm:ss}] {error}");
        }

        public void AddWarning(string warning)
        {
            warnings.Add($"[{DateTime.Now:HH:mm:ss}] {warning}");
        }

        private void CalculateStatistics()
        {
            if (totalPrefabsProcessed > 0)
            {
                averageProcessingTimePerPrefab = totalDurationSeconds / totalPrefabsProcessed;
            }
            
            // Reset min/max if no successful processing occurred
            if (successfulPrefabs == 0)
            {
                fastestPrefabTime = 0f;
                slowestPrefabTime = 0f;
            }
        }

        public string GenerateTextReport()
        {
            var report = new System.Text.StringBuilder();
            
            // Header
            report.AppendLine("=== UNITY ICON GENERATION REPORT ===");
            report.AppendLine($"Session ID: {sessionId}");
            report.AppendLine($"Generated: {endTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Unity Version: {unityVersion}");
            report.AppendLine($"Tool Version: {toolVersion}");
            report.AppendLine();
            
            // Configuration
            report.AppendLine("=== CONFIGURATION ===");
            report.AppendLine($"Input Folder: {inputFolderPath}");
            report.AppendLine($"Output Folder: {outputFolderPath}");
            report.AppendLine($"Prefab Prefix: '{prefabNamePrefix}'");
            report.AppendLine($"Main Icon Size: {mainIconSize}x{mainIconSize}");
            if (additionalSizes.Count > 0)
            {
                report.AppendLine($"Additional Sizes: {string.Join(", ", additionalSizes.ConvertAll(s => $"{s}x{s}"))}");
            }
            report.AppendLine($"Export Format: {exportFormat}");
            report.AppendLine($"Lighting Preset: {LightingPresets.GetPresetDisplayName(lightingPreset)}");
            report.AppendLine($"Point Lights: {pointLightCount}");
            report.AppendLine();
            
            // Summary
            report.AppendLine("=== SUMMARY ===");
            report.AppendLine($"Total Duration: {totalDurationSeconds:F2} seconds");
            report.AppendLine($"Prefabs Found: {totalPrefabsFound}");
            report.AppendLine($"Prefabs Processed: {totalPrefabsProcessed}");
            report.AppendLine($"Successful: {successfulPrefabs}");
            report.AppendLine($"Failed: {failedPrefabs}");
            report.AppendLine($"Success Rate: {(totalPrefabsProcessed > 0 ? (successfulPrefabs * 100.0f / totalPrefabsProcessed) : 0):F1}%");
            report.AppendLine($"Icons Generated: {totalIconsGenerated}");
            report.AppendLine($"Icons Failed: {totalIconsFailed}");
            report.AppendLine($"Total File Size: {FormatFileSize(totalFileSizeBytes)}");
            
            if (successfulPrefabs > 0)
            {
                report.AppendLine($"Average Time per Prefab: {averageProcessingTimePerPrefab:F2}s");
                report.AppendLine($"Fastest: {fastestPrefabName} ({fastestPrefabTime:F2}s)");
                report.AppendLine($"Slowest: {slowestPrefabName} ({slowestPrefabTime:F2}s)");
            }
            report.AppendLine();
            
            // Global Errors
            if (globalErrors.Count > 0)
            {
                report.AppendLine("=== GLOBAL ERRORS ===");
                foreach (var error in globalErrors)
                {
                    report.AppendLine(error);
                }
                report.AppendLine();
            }
            
            // Warnings
            if (warnings.Count > 0)
            {
                report.AppendLine("=== WARNINGS ===");
                foreach (var warning in warnings)
                {
                    report.AppendLine(warning);
                }
                report.AppendLine();
            }
            
            // Detailed Results
            report.AppendLine("=== DETAILED RESULTS ===");
            foreach (var entry in entries)
            {
                report.AppendLine($"Prefab: {entry.prefabName}");
                report.AppendLine($"  Path: {entry.prefabPath}");
                report.AppendLine($"  Status: {(entry.success ? "SUCCESS" : "FAILED")}");
                report.AppendLine($"  Processing Time: {entry.processingTimeSeconds:F2}s");
                report.AppendLine($"  Timestamp: {entry.timestamp:HH:mm:ss}");
                
                if (!entry.success && !string.IsNullOrEmpty(entry.errorMessage))
                {
                    report.AppendLine($"  Error: {entry.errorMessage}");
                }
                
                if (entry.generatedIcons.Count > 0)
                {
                    report.AppendLine($"  Generated Icons ({entry.generatedIcons.Count}):");
                    foreach (var icon in entry.generatedIcons)
                    {
                        var status = icon.success ? "✓" : "✗";
                        var size = FormatFileSize(icon.fileSizeBytes);
                        report.AppendLine($"    {status} {icon.fileName} ({icon.size}x{icon.size}, {size})");
                        if (!icon.success && !string.IsNullOrEmpty(icon.errorMessage))
                        {
                            report.AppendLine($"      Error: {icon.errorMessage}");
                        }
                    }
                }
                report.AppendLine();
            }
            
            return report.ToString();
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }

        public void SaveToFile(string filePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    var defaultFileName = $"IconGeneration_Report_{startTime:yyyyMMdd_HHmmss}.txt";
                    var reportsFolder = Path.Combine(outputFolderPath, "Reports");
                    
                    if (!Directory.Exists(reportsFolder))
                    {
                        Directory.CreateDirectory(reportsFolder);
                    }
                    
                    filePath = Path.Combine(reportsFolder, defaultFileName);
                }
                
                var textReport = GenerateTextReport();
                File.WriteAllText(filePath, textReport);
                
                Debug.Log($"Generation report saved to: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save generation report: {e.Message}");
            }
        }

        public void SaveJsonReport(string filePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    var defaultFileName = $"IconGeneration_Report_{startTime:yyyyMMdd_HHmmss}.json";
                    var reportsFolder = Path.Combine(outputFolderPath, "Reports");
                    
                    if (!Directory.Exists(reportsFolder))
                    {
                        Directory.CreateDirectory(reportsFolder);
                    }
                    
                    filePath = Path.Combine(reportsFolder, defaultFileName);
                }
                
                var json = JsonUtility.ToJson(this, true);
                File.WriteAllText(filePath, json);
                
                Debug.Log($"Generation report JSON saved to: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save JSON generation report: {e.Message}");
            }
        }
    }
}