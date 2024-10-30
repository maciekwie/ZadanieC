using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class DataReader
{
    public static List<DataEntry> Load(string labelFile, string imageFile, int maxItem = -1)
    {
        var label = LoadLabels(labelFile, maxItem);
        var images = LoadImages(imageFile, maxItem);

        if (label.Count == 0 || images.Count == 0)
        {
            return new List<DataEntry>();
        }

        return label.Select((t, i) => new DataEntry { Label = t, Image = images[i] }).ToList();
    }

    private static List<byte[]> LoadImages(string filename, int maxItem = -1)
    {
        var result = new List<byte[]>();

        using (FileStream fs = new FileStream(filename, FileMode.Open))
        using (BinaryReader br = new BinaryReader(fs))
        {
            // Read the magic number
            int magicNumber = ReadBigEndianInt32(br);
            if (magicNumber != 2051)
            {
                throw new Exception("Invalid magic number in MNIST image file!");
            }

            // Read the number of images, rows, and columns
            int numberOfImages = ReadBigEndianInt32(br);
            int numberOfRows = ReadBigEndianInt32(br);
            int numberOfCols = ReadBigEndianInt32(br);

            int imageSize = numberOfRows * numberOfCols;

            // Read each image
            for (int i = 0; i < numberOfImages; i++)
            {
                byte[] imageData = br.ReadBytes(imageSize);
                if (imageData.Length != imageSize)
                {
                    throw new Exception($"Unexpected end of file at image {i}.");
                }
                result.Add(imageData);
            }
        }

        return result;
    }

    private static int ReadBigEndianInt32(BinaryReader br)
    {
        byte[] bytes = br.ReadBytes(4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes); // Convert big-endian to little-endian
        return BitConverter.ToInt32(bytes, 0);
    }

    private static List<int> LoadLabels(string filename, int maxItem = -1)
    {
        var result = new List<int>();

        try
        {
            // Read all text from the file
            string text = File.ReadAllText(filename);

            // Split the text by commas
            string[] tokens = text.Split(',');

            // Parse each token into an integer and add to the list
            foreach (string token in tokens)
            {
                int num;
                if (int.TryParse(token.Trim(), out num))
                {
                    result.Add(num);
                }
                else
                {
                    // Handle invalid number formats
                    Debug.Log($"Invalid number format: '{token}'");
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions such as file not found
            Debug.Log("An error occurred: " + ex.Message);
        }

        return result;
    }
}
