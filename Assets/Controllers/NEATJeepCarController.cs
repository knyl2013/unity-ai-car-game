using System.Collections;
using System.Collections.Generic;
using SharpNeat.Phenomes;
using UnityEngine;

public class NEATJeepCarController : UnitController
{
    private Vector3 startPosition, startRotation;
    private NNet network;

    // Start is called before the first frame update
    [Range(-1f, 1f)]
    public float a, t;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultipler = 2.4f;
    public float avgSpeedMultipiler = 0.02f;
    public float sensorMultipler = 0.1f;
    public float bestOverallFitness = 0f;

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    public float aSensor, bSensor, cSensor;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    private IBlackBox box;

    public bool controlByHuman = false;


    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = new NNet(LAYERS, NEURONS);
    }

    public void Reset()
    {
        //network.Initialise(LAYERS, NEURONS);
        bestOverallFitness = Mathf.Max(bestOverallFitness, overallFitness);
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Death();
        //Debug.Log("Enter collision");
        Reset();
    }

    private void FixedUpdate()
    {
        InputSensors();
        lastPosition = transform.position;

        // Neural network code here
        if (!controlByHuman)
        {
            ISignalArray inputArr = box.InputSignalArray;

            //(a, t) = network.RunNetwork(aSensor, bSensor, cSensor);

            inputArr[0] = aSensor;
            inputArr[1] = bSensor;
            inputArr[2] = cSensor;

            box.Activate();

            ISignalArray outputArr = box.OutputSignalArray;
            a = (float)outputArr[0];
            t = (float)outputArr[1];

            // From (0, 1) to (-1, 1)
            //a = a * 2 - 1;
            t = t * 2 - 1;

            //a *= maxMotorTorque;
            //steering *= maxSteeringAngle;

            //Debug.Log((a, t));
        }
        


        MoveCar(a, t);

        timeSinceStart += Time.deltaTime;

        CalculateFitness();

        //a = 0;
        //t = 0;
    }

    private void CalculateFitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = (totalDistanceTravelled * distanceMultipler) + (avgSpeed * avgSpeedMultipiler) + ((aSensor + bSensor + cSensor) / 3 * sensorMultipler);

        //if (timeSinceStart > 20 && overallFitness < 40)
        //{
        //    //Death();
        //    Reset();
        //}

        if (overallFitness >= 1000)
        {
            // Saves network to a JSON
            //Death();
            Reset();
        }

    }

    private void InputSensors()
    {
        //Debug.Log("Enter InputSensors()");
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);
        int scale = 50;

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit))
        {
            aSensor = hit.distance / scale; // Normalize value
            // Debug.Log("A: " + aSensor);
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = b;
        if (Physics.Raycast(r, out hit))
        {
            bSensor = hit.distance / scale; // Normalize value
            // Debug.Log("B: " + bSensor);
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = c;
        if (Physics.Raycast(r, out hit))
        {
            cSensor = hit.distance / scale; // Normalize value
            // Debug.Log("C: " + cSensor);
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }
    }

    private Vector3 inp;
    private float RATE = 0.02f;
    private float MAG = 11.4f;
    public void MoveCar(float v, float h)
    {
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, v * MAG), RATE);
        //inp = new Vector3(0, 0, v * MAG * RATE);
        inp = transform.TransformDirection(inp);
        //inp = new Vector3(inp.x, 0, inp.z);
        //print(inp);
        // Debug.Log(inp);
        // Debug.Log("Before:");
        // Debug.Log(transform.position);
        transform.position += inp;
        // Debug.Log("After:");
        // Debug.Log(transform.position);
        // transform.position = new Vector3(transform.position.x, startPosition.y, transform.position.z);
        //transform.position.y = Mathf.Min(startPosition.y, transform.position.y);
        transform.eulerAngles += new Vector3(0, (h * 90) * RATE, 0);

    }

    public void ResetWithNetwork(NNet net)
    {
        network = net;
    }

    private void Death()
    {
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
    }

    public override float GetFitness()
    {
        CalculateFitness();
        bestOverallFitness = Mathf.Max(bestOverallFitness, overallFitness);
        //if (bestFitness >= fitnessToSave)
        //{
        //    SaveNetwork();
        //}
        var fit = bestOverallFitness;//cache the fitness value
        overallFitness = 0;//reset fitness value each time we start a new training cycle
        bestOverallFitness = 0; //reset fitness value each time we start a new training cycle
        if (fit < 0 || double.IsNaN(fit) || double.IsInfinity(fit))
            fit = 0;

        return fit;
    }

    public override void Stop()
    {
        this.IsRunning = false;
    }

    public bool IsRunning { get; set; }

    public override void Activate(IBlackBox box)
    {
        this.box = box;
        this.IsRunning = true;
    }


}
