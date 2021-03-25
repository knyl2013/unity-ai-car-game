using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using MathNet.Numerics.LinearAlgebra;
using System;
using Random = UnityEngine.Random;
using System.IO;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    //public GARaceCarController controller;
    public MyCarController controller;

    [Header("Controls")]
    public int initialPopulation = 50;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.8f;

    [Header("Crossover Controls")]
    public int bestAgentSelection = 10;
    public int worstAgentSelection = 1;
    public int numberToCrossover = 70;

    private List<int> genePool = new List<int>();
    private int naturallySelected;

    private NNet[] population;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome;
    public float bestFitness;
    public float worstFitness;
    public float totalFitness;

    private void Start()
    {
        CreatePopulation();
        ResetToCurrentGenome();
    }

    private void CreatePopulation()
    {
        population = new NNet[initialPopulation];
        fillPopulationWithRandomValues(population, 0);
        ResetToCurrentGenome();
    }

    private void fillPopulationWithRandomValues(NNet[] newPopulation, int startingIndex)
    {
        //Debug.Log(newPopulation.Length);
        while (startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new NNet(controller.LAYERS, controller.NEURONS);
            ++startingIndex;
        }
    }

    private void ResetToCurrentGenome()
    {
        controller.ResetWithNetwork(population[currentGenome]);
    }

    public void Death(float fitness, NNet network)
    {
        //if (fitness >= 1000)
        //{
        //    //network.SaveNetwork();
        //    string prefix = "Assets/Resources/GA_pretrained/";

        //    string ID = "Over1000";

        //    //string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        //    //Debug.Log(path);

        //    string json = network.Save();

        //    File.WriteAllText(prefix + ID + ".txt", json);

        //    Debug.Log("Saved!");
        //}
        if (currentGenome < population.Length - 1)
        {
            population[currentGenome].fitness = fitness;
            //Debug.Log("fitness: " + fitness);
            ++currentGenome;
            ResetToCurrentGenome();
        }
        else
        {
            //RePopulation();
            Evolve();
            currentGenome = 0;
            ResetToCurrentGenome();
        }
    }

    private void RePopulate()
    {
        genePool.Clear();
        ++currentGeneration;
        naturallySelected = 0;

        //NNet[] newPopulation = PickBestPopulation();
    }

    private void GenerateGenePool()
    {
        genePool.Clear();

        for (int i = 0; i < bestAgentSelection; ++i)
        {
            //newPopulation[i] = population[i].Clone();
            //newPopulation[i].fintess = 0;

            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; ++c)
            {
                genePool.Add(i);
            }
        }

        //for (int c = 0, i = population.Length-1; c < worstAgentSelection; ++c, --i)
        //{
        //    int f = Mathf.RoundToInt(population[i].fitness * 10);

        //    for (int k = 0; k < f; ++k)
        //    {
        //        genePool.Add(i);
        //    }
        //}
    }

    private void Crossover (List<NNet> newPopulation)
    {
        //Debug.Log("genePool: " + genePool.Count);
        for (int i = 0; i < numberToCrossover; i+=2)
        {
            if (genePool.Count == 0) break;
            int p1Idx = genePool[Random.Range(0, genePool.Count)];
            int p2Idx = genePool[Random.Range(0, genePool.Count)];
            //Debug.Log("p1Idx: " + p1Idx);
            //Debug.Log("p2Idx: " + p2Idx);
            NNet p1 = population[p1Idx];
            NNet p2 = population[p2Idx];

            NNet child1 = new NNet(p1.hiddenLayerCount, p2.hiddenNeuronCount);
            NNet child2 = new NNet(p1.hiddenLayerCount, p2.hiddenNeuronCount);

            for (int k = 0; k < child1.weights.Count; ++k)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.weights[k] = p1.weights[k];
                    child2.weights[k] = p2.weights[k];
                }
                else
                {
                    child1.weights[k] = p2.weights[k];
                    child2.weights[k] = p1.weights[k];
                }
            }

            for (int k = 0; k < child1.biases.Count; ++k)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.biases[k] = p1.biases[k];
                    child2.biases[k] = p2.biases[k];
                }
                else
                {
                    child1.biases[k] = p2.biases[k];
                    child2.biases[k] = p1.biases[k];
                }
            }



            //    for (int w = 0; w < child.weights.Count; ++w)
            //{
            //    if (Random.Range(0.0f, 1.0f) < 0.5f)
            //    {
            //        child.weights[w] = p1.weights[w];
            //    }
            //    else
            //    {
            //        child.weights[w] = p2.weights[w];
            //    }

            newPopulation.Add(child1);
            newPopulation.Add(child2);
        }
    }

    private void SortPopulation()
    {
        Array.Sort(population, new Comparison<NNet>((i1, i2) => i1.fitness.CompareTo(i2.fitness)));
        Array.Reverse(population);

    }

    private void Mutation(List<NNet> newPopulation)
    {
        for (int i = bestAgentSelection+1; i < newPopulation.Count; ++i)
        {
            if (Random.Range(0.0f, 1.0f) < mutationRate)
            {
                newPopulation[i].Mutate();
            }
        }
    }

    private void Selection(List<NNet> newPopulation)
    {
        for (int i = 0; i < bestAgentSelection; ++i)
        {
            newPopulation.Add(population[i].Clone());
        }
    }
    

    private void Evolve()
    {
        totalFitness = 0;

        bestFitness = 0;

        worstFitness = (float) 1e9 + 7;

        List<NNet> newPopulation = new List<NNet>();

        SortPopulation();

        GenerateGenePool();

        ++currentGeneration;

        Selection(newPopulation);

        Crossover(newPopulation);

        NNet anyNet = newPopulation[0];

        while (newPopulation.Count < initialPopulation)
        {
            newPopulation.Add(new NNet(anyNet.hiddenLayerCount, anyNet.hiddenNeuronCount));
        }

        Mutation(newPopulation);


        for (int i = 0; i < initialPopulation; ++i)
        {
            bestFitness = Math.Max(bestFitness, this.population[i].fitness);
            worstFitness = Math.Min(worstFitness, this.population[i].fitness);
            totalFitness += this.population[i].fitness;
            this.population[i] = newPopulation[i];
        }

        //if (currentGeneration % 10 == 0)
        Debug.Log("Best Fitness of Generation " + currentGeneration + ": " + bestFitness);
    }
}
