using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Threading.Tasks;
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

    public async void LoadNetwork()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "convnet.json");

        UnityWebRequest request = UnityWebRequest.Get(filePath);
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;

            var deserialized = SerializationExtensions.FromJson<double>(json);

            this.net = deserialized;
        }
        else
        {
            Debug.LogError("Cannot load file at " + filePath);
        }
    }
}
