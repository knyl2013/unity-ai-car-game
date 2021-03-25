using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpNeat.Phenomes;

public class TestSmartUnitController : UnitController
{
    IBlackBox box;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hi");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Activate(IBlackBox box)
    {
        this.box = box;
        this.IsRunning = true;
    }

    public override float GetFitness()
    {
        // CalculateFitness();
        
        // var fit = overallFitness;//cache the fitness value
        // overallFitness = 0;//reset fitness value each time we start a new training cycle

        // if (fit < 0)
        //     fit = 0;
        var fit = 0;

        return fit;

    }

    public override void Stop()
    {
        this.IsRunning = false;
    }

    public bool IsRunning { get; set; }
}
