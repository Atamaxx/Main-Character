using UnityEngine;
using System;
using System.IO;

public class ScreenshotCapture : MonoBehaviour
{
    [Tooltip("Resolution width in pixels")]
    public int resolutionWidth = 3840; // 4K width
    
    [Tooltip("Resolution height in pixels")]
    public int resolutionHeight = 2160; // 4K height
    
    [Tooltip("Output folder path (default: project root/Screenshots)")]
    public string outputFolder = "Assets/Graphics/Screenshots";
    
    [Tooltip("File name format (default: Screenshot_yyyy-MM-dd_HH-mm-ss)")]
    public string fileNameFormat = "Screenshot_{0}";
    
    [Tooltip("Include timestamp in filename")]
    public bool includeTimestamp = true;
    
    [Tooltip("File format (PNG recommended for quality)")]
    public ScreenshotFormat fileFormat = ScreenshotFormat.PNG;
    
    [Tooltip("Key combination: Ctrl+G")]
    public KeyCode screenshotKey = KeyCode.G;
    
    private void Start()
    {
        // Create the output directory if it doesn't exist
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            Debug.Log($"Created screenshot directory: {Path.GetFullPath(outputFolder)}");
        }
    }
    
    private void Update()
    {
        // Check for Ctrl+G key combination
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(screenshotKey))
        {
            CaptureScreenshot();
        }
    }
    
    public void CaptureScreenshot()
    {
        try
        {
            // Generate filename with timestamp if enabled
            string fileName;
            if (includeTimestamp)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                fileName = string.Format(fileNameFormat, timestamp);
            }
            else
            {
                fileName = string.Format(fileNameFormat, "");
            }
            
            // Add extension based on format
            string extension = fileFormat.ToString().ToLower();
            string filePath = Path.Combine(outputFolder, $"{fileName}.{extension}");
            
            // Capture the screenshot at the specified resolution
            ScreenCapture.CaptureScreenshot(filePath, ScreenshotImageFormat());
            
            Debug.Log($"Screenshot captured: {Path.GetFullPath(filePath)} at {resolutionWidth}x{resolutionHeight}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error capturing screenshot: {e.Message}");
        }
    }
    
    // Helper method to convert enum to Unity's format
    private int ScreenshotImageFormat()
    {
        switch (fileFormat)
        {
            case ScreenshotFormat.PNG:
                return 0;
            case ScreenshotFormat.JPG:
                return 1;
            default:
                return 0;
        }
    }
    
    // Enum for file format options
    public enum ScreenshotFormat
    {
        PNG,
        JPG
    }
}