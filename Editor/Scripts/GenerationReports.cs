using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Razluta.UnityIconGenerationFromModels.Editor
{
    /// <summary>
    /// Represents a single log entry in the generation process
    /// </summary>
    [Serializable]
    public class GenerationLogEntry
    {
        public DateTime timestamp;
        public LogLevel level;
        public string message;
        public string prefabName;
        public string folderPath;
        public float processingTime;
        public Dictionary<string, object> metadata;
        
        public GenerationLogEntry(LogLevel level, string message, string prefabName = "", string folderPath = "", float processingTime = 0f)
        {
            this.timestamp = DateTime.Now;
            this.level = level;
            this.message = message;
            this.prefabName = prefabName;
            this.folderPath = folderPath;
            this.processingTime = processingTime;
            this.metadata = new Dictionary<string, object>();
        }
        
        public void AddMetadata(string key, object value)
        {
            metadata[key] = value;
        }
        
        public string GetFormattedMessage()
        {
            var sb = new StringBuilder();
            sb.Append($"[{timestamp:HH:mm:ss}] ");
            sb.Append($"[{level}] ");
            
            if (!string.IsNullOrEmpty(prefabName))
                sb.Append($"[{prefabName}] ");
                
            sb.Append(message);
            
            if (processingTime > 0)
                sb.Append($" ({processingTime:F2}s)");
                
            return sb.ToString();
        }
        
        public string GetLevelIcon()
        {
            return level switch
            {
                LogLevel.Info => "ℹ️",
                LogLevel.Success => "✅",
                LogLevel.Warning => "⚠️",
                LogLevel.Error => "❌",
                LogLevel.Debug => "🔍",
                _ => "📝"
            };
        }
    }
    
    /// <summary>
    /// Log level enumeration
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Success,
        Warning,
        Error
    }
    
    /// <summary>
    /// Comprehensive generation report with statistics and logs
    /// </summary>
    [Serializable]
    public class GenerationReport
    {
        [Header("Session Information")]
        public DateTime sessionStartTime;
        public DateTime sessionEndTime;
        public float totalProcessingTime;
        public string unityVersion;
        public string toolVersion = "1.2.0";
        
        [Header("Processing Statistics")]
        public int totalPrefabs;
        public int processedPrefabs;
        public int successfulPrefabs;
        public int failedPrefabs;
        public int skippedPrefabs;
        
        [Header("Configuration")]
        public RenderQualityPreset qualityPreset;
        public AntiAliasingLevel antiAliasingLevel;
        public string outputFormat;
        public List<int> iconSizes;
        public int folderCount;
        
        [Header("Performance Metrics")]
        public float averageProcessingTime;
        public float fastestProcessingTime;
        public float slowestProcessingTime;
        public string fastestPrefab;
        public string slowestPrefab;
        
        [Header("Logs")]
        public List<GenerationLogEntry> logEntries;
        
        public GenerationReport()
        {
            sessionStartTime = DateTime.Now;
            unityVersion = Application.unityVersion;
            logEntries = new List<GenerationLogEntry>();
            iconSizes = new List<int>();
            
            // Initialize performance metrics
            fastestProcessingTime = float.MaxValue;
            slowestProcessingTime = 0f;
        }
        
        public void StartSession()
        {
            sessionStartTime = DateTime.Now;
            LogInfo("Icon generation session started");
        }
        
        public void EndSession()
        {
            sessionEndTime = DateTime.Now;
            totalProcessingTime = (float)(sessionEndTime - sessionStartTime).TotalSeconds;
            
            CalculateStatistics();
            LogInfo($"Icon generation session completed. Total time: {totalProcessingTime:F2}s");
        }
        
        public void LogInfo(string message, string prefabName = "", string folderPath = "", float processingTime = 0f)
        {
            var entry = new GenerationLogEntry(LogLevel.Info, message, prefabName, folderPath, processingTime);
            logEntries.Add(entry);
            Debug.Log(entry.GetFormattedMessage());
        }
        
        public void LogSuccess(string message, string prefabName = "", string folderPath = "", float processingTime = 0f)
        {
            var entry = new GenerationLogEntry(LogLevel.Success, message, prefabName, folderPath, processingTime);
            logEntries.Add(entry);
            Debug.Log(entry.GetFormattedMessage());
            successfulPrefabs++;
        }
        
        public void LogWarning(string message, string prefabName = "", string folderPath = "", float processingTime = 0f)
        {
            var entry = new GenerationLogEntry(LogLevel.Warning, message, prefabName, folderPath, processingTime);
            logEntries.Add(entry);
            Debug.LogWarning(entry.GetFormattedMessage());
        }
        
        public void LogError(string message, string prefabName = "", string folderPath = "", float processingTime = 0f)
        {
            var entry = new GenerationLogEntry(LogLevel.Error, message, prefabName, folderPath, processingTime);
            logEntries.Add(entry);
            Debug.LogError(entry.GetFormattedMessage());
            failedPrefabs++;
        }
        
        public void LogDebug(string message, string prefabName = "", string folderPath = "", float processingTime = 0f)
        {
            var entry = new GenerationLogEntry(LogLevel.Debug, message, prefabName, folderPath, processingTime);
            logEntries.Add(entry);
            
            #if UNITY_EDITOR
            if (Debug.isDebugBuild)
                Debug.Log(entry.GetFormattedMessage());
            #endif
        }
        
        public void UpdateProcessingTime(string prefabName, float processingTime)
        {
            if (processingTime < fastestProcessingTime)
            {
                fastestProcessingTime = processingTime;
                fastestPrefab = prefabName;
            }
            
            if (processingTime > slowestProcessingTime)
            {
                slowestProcessingTime = processingTime;
                slowestPrefab = prefabName;
            }
            
            processedPrefabs++;
        }
        
        private void CalculateStatistics()
        {
            var processingTimes = logEntries
                .Where(entry => entry.processingTime > 0)
                .Select(entry => entry.processingTime)
                .ToList();
                
            if (processingTimes.Count > 0)
            {
                averageProcessingTime = processingTimes.Average();
            }
            
            // Reset fastest time if no valid entries
            if (fastestProcessingTime == float.MaxValue)
                fastestProcessingTime = 0f;
        }
        
        public string GetSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Icon Generation Report - {sessionStartTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Unity Version: {unityVersion}");
            sb.AppendLine($"Tool Version: {toolVersion}");
            sb.AppendLine();
            
            sb.AppendLine("Processing Statistics:");
            sb.AppendLine($"  Total Prefabs: {totalPrefabs}");
            sb.AppendLine($"  Successful: {successfulPrefabs}");
            sb.AppendLine($"  Failed: {failedPrefabs}");
            sb.AppendLine($"  Skipped: {skippedPrefabs}");
            sb.AppendLine($"  Success Rate: {(totalPrefabs > 0 ? (successfulPrefabs * 100.0f / totalPrefabs):0):F1}%");
            sb.AppendLine();
            
            sb.AppendLine("Performance Metrics:");
            sb.AppendLine($"  Total Processing Time: {totalProcessingTime:F2}s");
            sb.AppendLine($"  Average per Prefab: {averageProcessingTime:F2}s");
            if (!string.IsNullOrEmpty(fastestPrefab))
                sb.AppendLine($"  Fastest: {fastestPrefab} ({fastestProcessingTime:F2}s)");
            if (!string.IsNullOrEmpty(slowestPrefab))
                sb.AppendLine($"  Slowest: {slowestPrefab} ({slowestProcessingTime:F2}s)");
            sb.AppendLine();
            
            sb.AppendLine("Configuration:");
            sb.AppendLine($"  Quality Preset: {qualityPreset}");
            sb.AppendLine($"  Anti-Aliasing: {antiAliasingLevel}");
            sb.AppendLine($"  Output Format: {outputFormat}");
            sb.AppendLine($"  Icon Sizes: {string.Join(", ", iconSizes)}");
            sb.AppendLine($"  Input Folders: {folderCount}");
            
            return sb.ToString();
        }
        
        public string GetDetailedLog()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetSummary());
            sb.AppendLine();
            sb.AppendLine("Detailed Log:");
            sb.AppendLine(new string('-', 50));
            
            foreach (var entry in logEntries)
            {
                sb.AppendLine($"{entry.GetLevelIcon()} {entry.GetFormattedMessage()}");
            }
            
            return sb.ToString();
        }
        
        public void ExportToFile(string filePath)
        {
            try
            {
                var content = GetDetailedLog();
                File.WriteAllText(filePath, content);
                LogInfo($"Report exported to: {filePath}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to export report: {ex.Message}");
            }
        }
        
        public void ExportToJson(string filePath)
        {
            try
            {
                var json = JsonUtility.ToJson(this, true);
                File.WriteAllText(filePath, json);
                LogInfo($"Report exported to JSON: {filePath}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to export JSON report: {ex.Message}");
            }
        }
        
        public List<GenerationLogEntry> GetEntriesByLevel(LogLevel level)
        {
            return logEntries.Where(entry => entry.level == level).ToList();
        }
        
        public List<GenerationLogEntry> GetErrorEntries()
        {
            return GetEntriesByLevel(LogLevel.Error);
        }
        
        public List<GenerationLogEntry> GetWarningEntries()
        {
            return GetEntriesByLevel(LogLevel.Warning);
        }
        
        public bool HasErrors => GetErrorEntries().Count > 0;
        public bool HasWarnings => GetWarningEntries().Count > 0;
    }
    
    /// <summary>
    /// Manages generation reports and provides UI integration
    /// </summary>
    public static class GenerationReportManager
    {
        private static GenerationReport currentReport;
        private static readonly List<GenerationReport> recentReports = new List<GenerationReport>();
        private const int MaxRecentReports = 10;
        
        public static GenerationReport CurrentReport => currentReport;
        public static List<GenerationReport> RecentReports => recentReports;
        
        public static GenerationReport StartNewReport()
        {
            // Archive current report if it exists
            if (currentReport != null)
            {
                ArchiveReport(currentReport);
            }
            
            currentReport = new GenerationReport();
            currentReport.StartSession();
            
            return currentReport;
        }
        
        public static void CompleteCurrentReport()
        {
            if (currentReport != null)
            {
                currentReport.EndSession();
                
                // Auto-save report
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var reportsFolder = Path.Combine(Application.dataPath, "../Reports");
                
                if (!Directory.Exists(reportsFolder))
                    Directory.CreateDirectory(reportsFolder);
                    
                var reportPath = Path.Combine(reportsFolder, $"IconGeneration_Report_{timestamp}.txt");
                currentReport.ExportToFile(reportPath);
                
                var jsonPath = Path.Combine(reportsFolder, $"IconGeneration_Report_{timestamp}.json");
                currentReport.ExportToJson(jsonPath);
            }
        }
        
        private static void ArchiveReport(GenerationReport report)
        {
            recentReports.Insert(0, report);
            
            // Keep only recent reports
            while (recentReports.Count > MaxRecentReports)
            {
                recentReports.RemoveAt(recentReports.Count - 1);
            }
        }
        
        public static void ClearRecentReports()
        {
            recentReports.Clear();
        }
        
        public static string GetReportsFolder()
        {
            return Path.Combine(Application.dataPath, "../Reports");
        }
        
        public static void OpenReportsFolder()
        {
            var reportsFolder = GetReportsFolder();
            if (Directory.Exists(reportsFolder))
            {
                EditorUtility.RevealInFinder(reportsFolder);
            }
            else
            {
                Directory.CreateDirectory(reportsFolder);
                EditorUtility.RevealInFinder(reportsFolder);
            }
        }
    }
}