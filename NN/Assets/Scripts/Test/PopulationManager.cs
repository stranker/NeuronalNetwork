using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PopulationManager : MonoBehaviour
{
    public GameObject TankPrefab;

    public int PopulationCount = 40;

    public Vector3 SceneHalfExtents = new Vector3 (20.0f, 0.0f, 20.0f);

    public float GenerationDuration = 20.0f;
    public int IterationCount = 1;

    public int EliteCount = 4;
    public float MutationChance = 0.10f;
    public float MutationRate = 0.01f;

    public int InputsCount = 4;
    public int HiddenLayers = 1;
    public int OutputsCount = 2;
    public int NeuronsCountPerHL = 7;
    public float Bias = 1f;
    public float P = 0.5f;


    GeneticAlgorithm genAlg;

    List<Tank> populationGOs = new List<Tank>();
    List<Genome> population = new List<Genome>();
    List<NeuralNetwork> brains = new List<NeuralNetwork>();
     
    float accumTime = 0;
    bool isRunning = false;

    public int generation {
        get; private set;
    }

    public float bestFitness 
    {
        get; private set;
    }

    public float avgFitness 
    {
        get; private set;
    }

    public float worstFitness 
    {
        get; private set;
    }

    private float getBestFitness()
    {
        float fitness = 0;
        foreach(Genome g in population)
        {
            if (fitness < g.fitness)
                fitness = g.fitness;
        }

        return fitness;
    }

    private float getAvgFitness()
    {
        float fitness = 0;
        foreach(Genome g in population)
        {
            fitness += g.fitness;
        }

        return fitness / population.Count;
    }

    private float getWorstFitness()
    {
        float fitness = float.MaxValue;
        foreach(Genome g in population)
        {
            if (fitness > g.fitness)
                fitness = g.fitness;
        }

        return fitness;
    }



    static PopulationManager instance = null;

    public static PopulationManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PopulationManager>();

            return instance;
        }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
    }

    public void StartSimulation()
    {
        // Create and confiugre the Genetic Algorithm
        genAlg = new GeneticAlgorithm(EliteCount, MutationChance, MutationRate);

        GenerateInitialPopulation();

        isRunning = true;
    }

    public void PauseSimulation()
    {
        isRunning = !isRunning;
    }

    public void StopSimulation()
    {
        isRunning = false;

        generation = 0;

        foreach (Tank tank in populationGOs)
        {
            tank.DestroyBullet();
        }

        // Destroy previous tanks (if there are any)
        DestroyTanks();

    }

    // Generate the random initial population
    void GenerateInitialPopulation()
    {
        generation = 0;

        // Destroy previous tanks (if there are any)
        DestroyTanks();
        
        for (int i = 0; i < PopulationCount; i++)
        {
            NeuralNetwork brain = CreateBrain();
            
            Genome genome = new Genome(brain.GetTotalWeightsCount());

            brain.SetWeights(genome.genome);
            brains.Add(brain);

            population.Add(genome);
            populationGOs.Add(CreateTank(genome, brain));
        }

        accumTime = 0.0f;
    }

    // Creates a new NeuralNetwork
    NeuralNetwork CreateBrain()
    {
        NeuralNetwork brain = new NeuralNetwork();

        // Add first neuron layer that has as many neurons as inputs
        brain.AddFirstNeuronLayer(InputsCount, Bias, P);

        for (int i = 0; i < HiddenLayers; i++)
        {
            // Add each hidden layer with custom neurons count
            brain.AddNeuronLayer(NeuronsCountPerHL, Bias, P);
        }

        // Add the output layer with as many neurons as outputs
        brain.AddNeuronLayer(OutputsCount, Bias, P);

        return brain;
    }

    // Evolve!!!
    void Epoch()
    {
        // Increment generation counter
        generation++;

        // Calculate best, average and worst fitness
        bestFitness = getBestFitness();
        avgFitness = getAvgFitness();
        worstFitness = getWorstFitness();

        // Evolve each genome and create a new array of genomes
        Genome[] newGenomes = genAlg.Epoch(population.ToArray());

        // Clear current population
        population.Clear();

        // Add new population
        population.AddRange(newGenomes);

        // Set the new genomes as each NeuralNetwork weights
        for (int i = 0; i < PopulationCount; i++)
        {
            NeuralNetwork brain = brains[i];

            brain.SetWeights(newGenomes[i].genome);

            populationGOs[i].SetBrain(newGenomes[i], brain);
            populationGOs[i].transform.position = GetRandomPos();
            populationGOs[i].transform.rotation = GetRandomRot();
        }
    }

    // Update is called once per frame
    void FixedUpdate () 
	{
        if (!isRunning)
            return;
        
        float dt = Time.fixedDeltaTime;

        for (int i = 0; i < Mathf.Clamp((float)(IterationCount / 100.0f) * 50, 1, 50); i++)
        {
            foreach (Tank t in populationGOs)
            {

                GameObject tank = GetNearestTank(t.transform.position, t);

                t.SetNearestTank(tank);

                // Think!! 
                t.Think(dt);

                // Just adjust tank position when reaching world extents
                Vector3 pos = t.transform.position;
                if (pos.x > SceneHalfExtents.x)
                    pos.x -= SceneHalfExtents.x * 2;
                else if (pos.x < -SceneHalfExtents.x)
                    pos.x += SceneHalfExtents.x * 2;

                if (pos.z > SceneHalfExtents.z)
                    pos.z -= SceneHalfExtents.z * 2;
                else if (pos.z < -SceneHalfExtents.z)
                    pos.z += SceneHalfExtents.z * 2;

                // Set tank position
                t.transform.position = pos;
            }

            // Check the time to evolve
            accumTime += dt;
            if (accumTime >= GenerationDuration)
            {
                accumTime -= GenerationDuration;
                Epoch();
                break;
            }
        }
	}

#region Helpers
    Tank CreateTank(Genome genome, NeuralNetwork brain)
    {
        Vector3 position = GetRandomPos();
        GameObject go = Instantiate<GameObject>(TankPrefab, position, GetRandomRot());
        Tank t = go.GetComponent<Tank>();
        t.SetBrain(genome, brain);
        return t;
    }

    void DestroyTanks()
    {
        foreach (Tank go in populationGOs)
            Destroy(go.gameObject);

        populationGOs.Clear();
        population.Clear();
        brains.Clear();
    }

    public void RelocateTank(GameObject nearTank)
    {
        nearTank.transform.position = GetRandomPos();
    }

    Vector3 GetRandomPos()
    {
        return new Vector3(UnityEngine.Random.value * SceneHalfExtents.x * 2.0f - SceneHalfExtents.x, 0.0f, UnityEngine.Random.value * SceneHalfExtents.z * 2.0f - SceneHalfExtents.z); 
    }

    Quaternion GetRandomRot()
    {
        return Quaternion.AngleAxis(UnityEngine.Random.value * 360.0f, Vector3.up);
    }

    GameObject GetNearestTank(Vector3 pos, Tank self)
    {
        Tank nearest = populationGOs[0];
        float distance = (pos - nearest.transform.position).sqrMagnitude;

        foreach (Tank tank in populationGOs)
        {
            if (tank != self)
            {
                float newDist = (tank.transform.position - pos).sqrMagnitude;
                if (newDist < distance)
                {
                    nearest = tank;
                    distance = newDist;
                }
            }
        }

        return nearest.gameObject;
    }   

#endregion

}
