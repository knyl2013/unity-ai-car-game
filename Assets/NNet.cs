using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using System;
using Random = UnityEngine.Random;
using System.IO;
using Newtonsoft.Json;

public class NNet
{
    public static int inputNeuronCount = 4; // Three Sensors
    public static int outputNeuronCount = 2; // (motor, steering)

    // Create a 1 x inputCount Matrix
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, inputNeuronCount);

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();
    // Create a 1 x 2 Matrix
    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, outputNeuronCount);

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<float> biases = new List<float>();

    public float fitness;

    public int hiddenLayerCount;
    public int hiddenNeuronCount;

    public NNet(int hiddenLayerCount, int hiddentNeuronCount)
    {
        this.hiddenLayerCount = hiddenLayerCount;
        this.hiddenNeuronCount = hiddentNeuronCount;
        Initialise();
    }


    public void Initialise()
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();

        weights.Add(Matrix<float>.Build.Dense(inputNeuronCount, hiddenNeuronCount));
        

        for (int i = 0; i < hiddenLayerCount; ++i)
        {
            hiddenLayers.Add(Matrix<float>.Build.Dense(1, hiddenNeuronCount));
            biases.Add(Random.Range(-1f, 1f));
        }

        for (int i = 1; i < hiddenLayerCount; ++i)
        {
            weights.Add(Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount));
        }

        weights.Add(Matrix<float>.Build.Dense(hiddenNeuronCount, outputNeuronCount));
        
        RandomiseWeights();
    }

    public void RandomiseWeights()
    {
        foreach (Matrix<float> mat in weights)
        {
            for (int r = 0; r < mat.RowCount; ++r)
            {
                for (int c = 0; c < mat.ColumnCount; ++c)
                {
                    mat[r, c] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    public (float, float) RunNetwork (float a, float b, float c, float d)
    {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;
        inputLayer[0, 3] = d;

        inputLayer = inputLayer.PointwiseTanh(); // [-1, 1]

        hiddenLayers[0] = (inputLayer * weights[0] + biases[0]).PointwiseTanh();

        for (int i = 1; i < hiddenLayers.Count; ++i)
        {
            hiddenLayers[i] = (hiddenLayers[i - 1] * weights[i] + biases[i]).PointwiseTanh();
        }

        outputLayer = (hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1] + biases[biases.Count - 1]).PointwiseTanh();

        //float accerlation = (float)Math.Tanh(outputLayer[0, 0]);

        //float turning = (float)Math.Tanh(outputLayer[0, 1]);

        return (Sigmoid(outputLayer[0, 0]), outputLayer[0, 1]);
    }

    public (float, float) RunNetwork(float a, float b, float c)
    {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;

        inputLayer = inputLayer.PointwiseTanh(); // [-1, 1]

        hiddenLayers[0] = (inputLayer * weights[0] + biases[0]).PointwiseTanh();

        for (int i = 1; i < hiddenLayers.Count; ++i)
        {
            hiddenLayers[i] = (hiddenLayers[i - 1] * weights[i] + biases[i]).PointwiseTanh();
        }

        outputLayer = (hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1] + biases[biases.Count - 1]).PointwiseTanh();

        for (int i = 0; i < hiddenLayers.Count; ++i)
        {
            hiddenLayers[i].Clear();
        } 
        //float accerlation = (float)Math.Tanh(outputLayer[0, 0]);

        //float turning = (float)Math.Tanh(outputLayer[0, 1]);

        return (Sigmoid(outputLayer[0, 0]), outputLayer[0, 1]);
    }

    public float Sigmoid(float s)
    {
        return 1 / (1 + Mathf.Exp(-s));
    }

    public NNet Clone()
    {
        NNet cloned = new NNet(this.hiddenLayerCount, this.hiddenNeuronCount);
        List<Matrix<float>> clonedWeights = new List<Matrix<float>>();
        foreach (Matrix<float> mat in this.weights)
        {
            //Matrix<float> clonedMat = Matrix<float>.Build.Dense(mat.RowCount, mat.ColumnCount);
            //for (int r = 0; r < mat.RowCount; ++r)
            //for (int c = 0; c < mat.ColumnCount; ++c)
            //clonedMat[r, c] = mat[r, c];
            //clonedWeights.Add(clonedMat);
            clonedWeights.Add(mat.Clone());
        }
        List<float> clonedBiases = new List<float>();
        foreach (float x in this.biases)
        {
            clonedBiases.Add(x);
        }
        cloned.weights = clonedWeights;
        cloned.biases = clonedBiases;
        cloned.fitness = this.fitness;
        return cloned;
    }

    public void Mutate()
    {
        Matrix<float> mat = this.weights[Mathf.RoundToInt(Random.Range(0, this.weights.Count - 1))];
        int r = Mathf.RoundToInt(Random.Range(0, mat.RowCount - 1));
        int c = Mathf.RoundToInt(Random.Range(0, mat.ColumnCount - 1));
        mat[r, c] += Random.Range(-1f, 1f);
        int biaIndex = Mathf.RoundToInt(Random.Range(0, biases.Count - 1));
        biases[biaIndex] = Random.Range(-1f, 1f); 
    }

    public void SaveNetwork()

    {

        string prefix = "Assets/Resources/GA_pretrained/";

        string ID = DateTime.Now.Ticks.ToString();

        //string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        //Debug.Log(path);

        string json = Save();

        File.WriteAllText(prefix + ID + ".txt", json);

        Debug.Log("Saved!");

        //string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        //if (!System.IO.Directory.Exists(path + "\\SavedBrains"))

        //{

        //    Directory.CreateDirectory(path + "\\SavedBrains");

        //}

        //path = path + "\\SavedBrains";

        //string ID = DateTime.Now.Ticks.ToString();

        //string json = JsonConvert.SerializeObject(inputLayer.ToArray());

        //File.WriteAllText(path + "\\inputLayer_" + ID + ".txt", json);

        //json = JsonConvert.SerializeObject(hiddenLayers.ToArray());

        //File.WriteAllText(path + "\\hiddenLayers_" + ID + ".txt", json);

        //json = JsonConvert.SerializeObject(outputLayer.ToArray());

        //File.WriteAllText(path + "\\outputLayers_" + ID + ".txt", json);

        //json = JsonConvert.SerializeObject(weights.ToArray());

        //File.WriteAllText(path + "\\weights_" + ID + ".txt", json);

        //json = JsonConvert.SerializeObject(biases.ToArray());

        //File.WriteAllText(path + "\\biases_" + ID + ".txt", json);

    }

    public void LoadNetwork(string ID)
    {
        string prefix = "Assets/Resources/GA_pretrained/";
        //string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //Debug.Log("Before: " + this.weights[0][1, 0]);
        Load(File.ReadAllText(prefix + ID + ".txt"));
        //Debug.Log("After: " + this.weights[0][1, 0]);
    }

    public void Load(string json) // loads a json string into the current NNet
    {
        SavedNetwork save = JsonConvert.DeserializeObject<SavedNetwork>(json);

        if (save.inputCount == inputNeuronCount && save.outputCount == outputNeuronCount && save.hiddenLayerCount == hiddenLayerCount && save.hiddenNeuronCount == hiddenNeuronCount)
        {
            this.weights = WeightArrayToMatrix(save.weights);
            this.biases = save.biases;
        }
        else
        {
            Debug.LogError("Saved NNet is incompatible with current NNet");
        }
    }


    public string Save() // returns biases + weights formated to json
    {
        SavedNetwork save = new SavedNetwork();

        save.inputCount = inputNeuronCount;
        save.outputCount = outputNeuronCount;
        save.hiddenLayerCount = hiddenLayerCount;
        save.hiddenNeuronCount = hiddenNeuronCount;

        save.weights = WeightMatrixToArray(weights);
        save.biases = biases;

        string json = JsonConvert.SerializeObject(save);
        return json;
    }

    private static List<float[,]> WeightMatrixToArray(List<Matrix<float>> weights) // converts list of matrix to list of float array to be serialised
    {
        List<float[,]> newWeights = new List<float[,]>();
        foreach (Matrix<float> w in weights)
        {
            newWeights.Add(w.ToArray());
        }
        return newWeights;
    }

    private static List<Matrix<float>> WeightArrayToMatrix(List<float[,]> weights) // converts de serialised float array to list of matrices 
    {
        List<Matrix<float>> newWeights = new List<Matrix<float>>();
        foreach (float[,] w in weights)
        {
            newWeights.Add(Matrix<float>.Build.DenseOfArray(w));
        }
        return newWeights;
    }


    private class SavedNetwork // structure to save data
    {
        public int inputCount;
        public int outputCount;
        public int hiddenLayerCount;
        public int hiddenNeuronCount;

        public List<float[,]> weights;
        public List<float> biases;
    }


    public void Print()
    {
        string ret = "Weights: \n";

        foreach (Matrix<float> mat in weights)
        {
            for (int r = 0; r < mat.RowCount; ++r)
            {
                for (int c = 0; c < mat.ColumnCount; ++c)
                {
                    ret += mat[r, c] + " ";
                }
                ret += "\n";
            }

            ret += "\n\n";
        }

        ret += "Biases: \n";

        foreach (float f in this.biases)
        {
            ret += f + " ";
        }


        Debug.Log(ret);
    }

}
