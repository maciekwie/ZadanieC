using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Collections.Generic;
using ConvNetSharp.Core;
using ConvNetSharp.Core.Serialization;
using ConvNetSharp.Volume;
using ConvNetSharp.Volume.Double;

public class UIController : MonoBehaviour
{
    public Painter painter;
    public ConvNet convNet;
    public TextMeshProUGUI text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        convNet.LoadNetwork();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
             Application.Quit();
        }

        if (Time.time - painter.drawTime > 1f)
        {
            if (painter.shapeClassified == false)
            {
                text.text = "Detecting...";

                int width = 32;
                int height = 32;

                Texture2D texture = painter.GetScaledImage(width, height);

                var dataShape = new Shape(width, height, 1, 1);
                var data = new double[dataShape.TotalLength];

                Volume<double> image = BuilderInstance.Volume.From(data, dataShape);

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        Color color = texture.GetPixel(x, y);
                        double pixelVal = (color.r + color.g + color.b) / 3.0;
                        image.Set(x, y, 0, 0, pixelVal);
                    }
                }

                double prediction = convNet.DetectTriangle(image);
                if (prediction == 1)
                {
                    text.text = "Traingle";
                }
                else
                {
                    text.text = "No traingle";
                }
            }

            painter.shapeClassified = true;
        }
        else
        {
            text.text = "Drawing";
        }
    }

    public void ClearButton_OnClick()
    {
        painter.ClearCanvas();
    }

    public void SaveButton_OnClick()
    {
        Texture2D texture = painter.GetScaledImage(32, 32);

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
