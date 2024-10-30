using UnityEngine;
using System.IO;

public class DataSets
{
    private const string trainingLabelFile = "../DataSet/Training/labels.txt";
    private const string trainingImageFile = "../DataSet/training.data";
    private const string testingLabelFile = "../DataSet/Test/labels.txt";
    private const string testingImageFile = "../DataSet/test.data";

    public DataSet Train { get; set; }

    public DataSet Validation { get; set; }

    public DataSet Test { get; set; }

    public bool Load(int validationSize = 10)
    {
        var trainingLabelFilePath = Path.Combine(Application.dataPath, trainingLabelFile);
        var trainingImageFilePath = Path.Combine(Application.dataPath, trainingImageFile);
        var testingLabelFilePath = Path.Combine(Application.dataPath, testingLabelFile);
        var testingImageFilePath = Path.Combine(Application.dataPath, testingImageFile);

        Debug.Log("Loading the datasets...");
        var trainImages = DataReader.Load(trainingLabelFilePath, trainingImageFilePath, 6);
        var testingImages = DataReader.Load(testingLabelFilePath, testingImageFilePath, 6);

        var valiationImages = trainImages.GetRange(trainImages.Count - validationSize, validationSize);
        trainImages = trainImages.GetRange(0, trainImages.Count - validationSize);

        if (trainImages.Count == 0 || valiationImages.Count == 0 || trainImages.Count == 0)
        {
            Debug.Log("Missing training/testing files.");
            return false;
        }

        this.Train = new DataSet(trainImages);
        this.Validation = new DataSet(valiationImages);
        this.Test = new DataSet(testingImages);

        return true;
    }
}