using UnityEngine;
using System.IO;

public class UIController : MonoBehaviour
{
    public Painter painter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
             Application.Quit();
        }
    }

    public void ClearButton_OnClick()
    {
        painter.ClearCanvas();
    }

    public void SaveButton_OnClick()
    {
        Texture2D texture = painter.GetScaledImage(128, 128);

        string baseName = "image";
        string directory = Application.dataPath + "/SavedTextures";

        // Get the first available file name
        string availableFileName = GetAvailableFileName(baseName, directory);

        SaveTextureToPNG(texture, availableFileName);
    }

    public static void SaveTextureToPNG(Texture2D texture, string filePath)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null. Cannot save to PNG.");
            return;
        }

        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("File path is invalid. Cannot save texture.");
            return;
        }

        try
        {
            // Encode texture into PNG format
            byte[] pngData = texture.EncodeToPNG();

            if (pngData != null)
            {
                // Create the directory if it doesn't exist
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Write the PNG file to disk
                File.WriteAllBytes(filePath, pngData);
                Debug.Log($"Texture saved to {filePath}");
            }
            else
            {
                Debug.LogError("Failed to encode texture to PNG.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"An error occurred while saving texture to PNG: {e.Message}");
        }
    }

    public static string GetAvailableFileName(string baseName, string directory)
    {
        const string extension = ".png";
        const int maxFiles = 1000; // Since 'XXX' is three digits, range is from 000 to 999

        // Ensure the directory exists
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        for (int i = 0; i < maxFiles; i++)
        {
            // Format the file name with leading zeros (e.g., 'name_001.png')
            string fileName = $"{baseName}_{i:D3}{extension}";
            string fullPath = Path.Combine(directory, fileName);

            // Check if the file exists
            if (!File.Exists(fullPath))
            {
                // Return the first available file name
                return fullPath;
            }
        }

        // If all file names are taken, throw an exception or handle accordingly
        throw new System.Exception("All file names are taken. Cannot find an available file name.");
    }
}
