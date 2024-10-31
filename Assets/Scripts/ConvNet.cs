using UnityEngine;
using System;
using System.IO;
using ConvNetSharp.Core;
using ConvNetSharp.Core.Serialization;
using ConvNetSharp.Core.Layers.Double;
using ConvNetSharp.Volume;
using ConvNetSharp.Volume.Double;


public class ConvNet : MonoBehaviour
{
    private Net<double> net;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        LoadNetwork();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public double DetectTriangle(Volume<double> image)
    {
        this.net.Forward(image);
        var prediction = this.net.GetPrediction();
        
        return prediction[0];
    }

    public void LoadNetwork()
    {
        string filePath = Application.dataPath + "/convnet.json";

        try
        {
            // Read the file content
            string json = File.ReadAllText(filePath);

            var deserialized = SerializationExtensions.FromJson<double>(json);

            this.net = deserialized;
        }
        catch (Exception ex)
        {
            Debug.Log($"An error occurred while loading the file: {ex.Message}");
        }
    }
}
