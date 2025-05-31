using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace Razluta.UnityIconGenerationFromModels
{
    public class GenerationReportWindow : EditorWindow
    {
        private string reportContent;
        private string reportName;
        private Vector2 scrollPosition;
        private GUIStyle textStyle;
        private bool stylesInitialized = false;
        
        public void SetReportContent(string content, string name)
        {
            reportContent = content;
            reportName = name;
            
            // Set window size based on content
            var lines = content.Split('\n').Length;
            var height = Mathf.Min(600, Mathf.Max(400, lines * 16 + 100));
            var width = 800;
            
            minSize = new Vector2(600, 300);
            maxSize = new Vector2(1200, 800);
            
            // Try to center the window
            var rect = new Rect(100, 100, width, height);
            position = rect;
        }
        
        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            textStyle = new GUIStyle(EditorStyles.textArea);
            textStyle.wordWrap = true;
            textStyle.richText = false;
            textStyle.fontStyle = FontStyle.Normal;
            textStyle.fontSize = 11;
            textStyle.font = EditorGUIUtility.Load("consola.ttf") as Font ?? GUI.skin.font;
            
            stylesInitialized = true;
        }
        
        private void OnGUI()
        {
            InitializeStyles();
            
            if (string.IsNullOrEmpty(reportContent))
            {
                GUILayout.Label("No report content available.", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            EditorGUILayout.BeginVertical();
            
            // Header
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label($"📊 {reportName}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("📋 Copy to Clipboard", EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                CopyToClipboard();
            }
            
            if (GUILayout.Button("💾 Save As...", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                SaveReportAs();
            }
            
            if (GUILayout.Button("📁 Show in Explorer", EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                ShowInExplorer();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Content
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            var contentRect = GUILayoutUtility.GetRect(
                new GUIContent(reportContent), 
                textStyle, 
                GUILayout.ExpandHeight(true), 
                GUILayout.ExpandWidth(true)
            );
            
            EditorGUI.SelectableLabel(contentRect, reportContent, textStyle);
            
            EditorGUILayout.EndScrollView();
            
            // Footer with summary
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            var lines = reportContent.Split('\n').Length;
            var characters = reportContent.Length;
            GUILayout.Label($"📄 {lines} lines, {characters} characters", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            
            var fileInfo = GetReportFileInfo();
            if (fileInfo != null)
            {
                GUILayout.Label($"📅 {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void CopyToClipboard()
        {
            EditorGUIUtility.systemCopyBuffer = reportContent;
            ShowNotification(new GUIContent("Report copied to clipboard!"));
        }
        
        private void SaveReportAs()
        {
            try
            {
                var defaultName = reportName.Replace(".txt", "") + "_Copy.txt";
                var savePath = EditorUtility.SaveFilePanel(
                    "Save Generation Report",
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    defaultName,
                    "txt"
                );
                
                if (!string.IsNullOrEmpty(savePath))
                {
                    File.WriteAllText(savePath, reportContent);
                    ShowNotification(new GUIContent($"Report saved to: {Path.GetFileName(savePath)}"));
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Save Error", $"Failed to save report: {e.Message}", "OK");
            }
        }
        
        private void ShowInExplorer()
        {
            try
            {
                var reportPath = FindReportFilePath();
                if (!string.IsNullOrEmpty(reportPath) && File.Exists(reportPath))
                {
                    EditorUtility.RevealInFinder(reportPath);
                }
                else
                {
                    // Fallback - show the reports directory
                    var reportsFolder = Path.Combine(Application.dataPath, "..", "Assets", "GeneratedIcons", "Reports");
                    if (Directory.Exists(reportsFolder))
                    {
                        EditorUtility.RevealInFinder(reportsFolder);
                    }
                    else
                    {
                        ShowNotification(new GUIContent("Could not locate report file"));
                    }
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to show in explorer: {e.Message}", "OK");
            }
        }
        
        private string FindReportFilePath()
        {
            try
            {
                // Try to find the report file based on the report name
                var possiblePaths = new[]
                {
                    Path.Combine(Application.dataPath, "..", "Assets", "GeneratedIcons", "Reports", reportName),
                    Path.Combine(Application.persistentDataPath, "Reports", reportName),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), reportName)
                };
                
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
                
                // Search in common locations
                var searchFolders = new[]
                {
                    Path.Combine(Application.dataPath, "..", "Assets"),
                    Application.persistentDataPath,
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                
                foreach (var folder in searchFolders)
                {
                    if (Directory.Exists(folder))
                    {
                        var files = Directory.GetFiles(folder, reportName, SearchOption.AllDirectories);
                        if (files.Length > 0)
                        {
                            return files[0];
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors in file searching
            }
            
            return null;
        }
        
        private FileInfo GetReportFileInfo()
        {
            try
            {
                var reportPath = FindReportFilePath();
                if (!string.IsNullOrEmpty(reportPath) && File.Exists(reportPath))
                {
                    return new FileInfo(reportPath);
                }
            }
            catch
            {
                // Ignore errors
            }
            
            return null;
        }
        
        private void OnDestroy()
        {
            // Clean up
            reportContent = null;
            reportName = null;
        }
    }
    
    // Extension class to provide easy access to report viewing
    public static class GenerationReportUtility
    {
        public static void ShowRecentReports(string outputFolderPath)
        {
            try
            {
                var reportsFolder = Path.Combine(outputFolderPath, "Reports");
                
                if (!Directory.Exists(reportsFolder))
                {
                    EditorUtility.DisplayDialog("No Reports Found", 
                        "No reports folder found. Generate some icons first to create reports.", "OK");
                    return;
                }
                
                var reportFiles = Directory.GetFiles(reportsFolder, "IconGeneration_Report_*.txt");
                
                if (reportFiles.Length == 0)
                {
                    EditorUtility.DisplayDialog("No Reports Found", 
                        "No report files found in the reports folder.", "OK");
                    return;
                }
                
                // Show dropdown with available reports
                var reportNames = new string[reportFiles.Length];
                for (int i = 0; i < reportFiles.Length; i++)
                {
                    var fileName = Path.GetFileName(reportFiles[i]);
                    var fileInfo = new FileInfo(reportFiles[i]);
                    reportNames[i] = $"{fileName} ({fileInfo.CreationTime:yyyy-MM-dd HH:mm})";
                }
                
                // Sort by creation time (newest first)
                Array.Sort(reportFiles, (x, y) => File.GetCreationTime(y).CompareTo(File.GetCreationTime(x)));
                Array.Sort(reportNames, (x, y) => 
                {
                    var xTime = DateTime.Parse(x.Substring(x.LastIndexOf('(') + 1).Replace(")", ""));
                    var yTime = DateTime.Parse(y.Substring(y.LastIndexOf('(') + 1).Replace(")", ""));
                    return yTime.CompareTo(xTime);
                });
                
                var selectedIndex = EditorUtility.DisplayDialogComplex(
                    "Select Report to View",
                    $"Found {reportFiles.Length} generation reports. Which would you like to view?",
                    "View Latest",
                    "Cancel",
                    "Show All Reports"
                );
                
                switch (selectedIndex)
                {
                    case 0: // View Latest
                        ViewReport(reportFiles[0]);
                        break;
                    case 2: // Show All Reports
                        EditorUtility.RevealInFinder(reportsFolder);
                        break;
                    // case 1 is Cancel - do nothing
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to show reports: {e.Message}", "OK");
            }
        }
        
        public static void ViewReport(string reportFilePath)
        {
            try
            {
                if (!File.Exists(reportFilePath))
                {
                    EditorUtility.DisplayDialog("Error", "Report file not found.", "OK");
                    return;
                }
                
                var reportContent = File.ReadAllText(reportFilePath);
                var reportName = Path.GetFileName(reportFilePath);
                
                var reportWindow = EditorWindow.GetWindow<GenerationReportWindow>();
                reportWindow.titleContent = new GUIContent($"📊 Generation Report - {reportName}");
                reportWindow.SetReportContent(reportContent, reportName);
                reportWindow.Show();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to open report: {e.Message}", "OK");
            }
        }
    }
}