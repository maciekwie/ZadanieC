using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ConvNetSharp.Core;
using ConvNetSharp.Core.Serialization;
using ConvNetSharp.Core.Layers.Double;
using ConvNetSharp.Core.Training.Double;
using ConvNetSharp.Volume;
using ConvNetSharp.Volume.Double;


public class ConvNetTraining : MonoBehaviour
{
    private readonly CircularBuffer<double> testAccWindow = new CircularBuffer<double>(100);
    private readonly CircularBuffer<double> trainAccWindow = new CircularBuffer<double>(100);

    private Net<double> net;
    private int stepCount;
    private SgdTrainer trainer;

    private DataSets datasets;

    bool training = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        datasets = new DataSets();
        datasets.Load(3);

        // Create network
        this.net = new Net<double>();
        this.net.AddLayer(new InputLayer(32, 32, 1));
        this.net.AddLayer(new ConvLayer(5, 5, 8) { Stride = 1, Pad = 2 });
        this.net.AddLayer(new ReluLayer());
        this.net.AddLayer(new PoolLayer(2, 2) { Stride = 2 });
        this.net.AddLayer(new ConvLayer(5, 5, 16) { Stride = 1, Pad = 2 });
        this.net.AddLayer(new ReluLayer());
        this.net.AddLayer(new PoolLayer(3, 3) { Stride = 3 });
        this.net.AddLayer(new FullyConnLayer(2));
        this.net.AddLayer(new SoftmaxLayer(2));

        this.trainer = new SgdTrainer(this.net)
        {
            LearningRate = 0.01,
            BatchSize = 5,
            Momentum = 0.9
        };
    }

    public void StartTraining()
    {
        training = true;
    }

    public void StopTraining()
    {
        training = false;
    }

    public void SaveNet()
    {
        string filePath = Application.dataPath + "/convnet.json";

        // Serialize to json 
        var json = net.ToJson();

        // Ensure the directory exists
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Write the content to the file
        File.WriteAllText(filePath, json);
    }

    // Update is called once per frame
    void Update()
    {
        if(training)
        {
            var trainSample = datasets.Train.NextBatch(this.trainer.BatchSize);
            Train(trainSample.Item1, trainSample.Item2, trainSample.Item3);

            var testSample = datasets.Test.NextBatch(this.trainer.BatchSize);
            Test(testSample.Item1, testSample.Item3, this.testAccWindow);

            Debug.Log(String.Format("Loss: {0} Train accuracy: {1}% Test accuracy: {2}%", this.trainer.Loss,
                Math.Round(this.trainAccWindow.Items.Average() * 100.0, 2),
                Math.Round(this.testAccWindow.Items.Average() * 100.0, 2)));

            Debug.Log(String.Format("Example seen: {0} Fwd: {1}ms Bckw: {2}ms", this.stepCount,
                Math.Round(this.trainer.ForwardTimeMs, 2),
                Math.Round(this.trainer.BackwardTimeMs, 2)));
        } 
    }

    private void Test(Volume<double> x, int[] labels, CircularBuffer<double> accuracy, bool forward = true)
    {
        if (forward)
        {
            this.net.Forward(x);
        }

        var prediction = this.net.GetPrediction();

        for (var i = 0; i < labels.Length; i++)
        {
            accuracy.Add(labels[i] == prediction[i] ? 1.0 : 0.0);
        }
    }

    private void Train(Volume<double> x, Volume<double> y, int[] labels)
    {
        this.trainer.Train(x, y);

        Test(x, labels, this.trainAccWindow, false);

        this.stepCount += labels.Length;
    }
}
