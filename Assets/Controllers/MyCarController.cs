using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//RequireComponent(typeof(NNet));

public class MyCarController : MonoBehaviour
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

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    public float aSensor, bSensor, cSensor;

    public float rpm;

    public bool movingForward;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;
    public bool runBest = false;
    public string ID = "";

    private Rigidbody rigidbody;
    public bool controlByHuman;


    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        rigidbody = GetComponent<Rigidbody>();
        network = new NNet(LAYERS, NEURONS);

        if (runBest)
        {
            network.LoadNetwork(ID);
            Debug.Log("Input: (0, 0, 0), Output: " + network.RunNetwork(0f, 0f, 0f));
            Debug.Log("Input: (0.5, 0.5, 0.5), Output: " + network.RunNetwork(0.5f, 0.5f, 0.5f));
            Debug.Log("Input: (1, 1, 1), Output: " + network.RunNetwork(1f, 1f, 1f));
        }

        network.Print();
    }

    public void Reset()
    {
        //network.Initialise(LAYERS, NEURONS);
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Death();
        Reset();
    }

    private void FixedUpdate()
    {
        InputSensors();
        lastPosition = transform.position;

        // Neural network code here
        if (!controlByHuman)
            (a, t) = network.RunNetwork(aSensor, bSensor, cSensor);

        //Debug.Log((a, t));

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

        if (timeSinceStart > 20 && overallFitness < 40)
        {
            Death();
            Reset();
        }

        if (overallFitness >= 5000)
        {
            // Saves network to a JSON
            //Debug.Log("Input: (0, 0, 0), Output: " + network.RunNetwork(0f, 0f, 0f));
            //Debug.Log("Input: (0.5, 0.5, 0.5), Output: " + network.RunNetwork(0.5f, 0.5f, 0.5f));
            //Debug.Log("Input: (1, 1, 1), Output: " + network.RunNetwork(1f, 1f, 1f));
            //network.Print();
            //network.SaveNetwork();
            Death();
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

        if (Physics.Raycast(r, out hit)) {
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

        //var velocity = rigidbody.velocity;
        //var localVel = transform.InverseTransformDirection(velocity);
        //movingForward = localVel.z > 0;



        //if (movingForward)
        //{
        //    rpm = rigidbody.velocity.magnitude * 3.6f;
        //}
        //else
        //{
        //    rpm = -rigidbody.velocity.magnitude * 3.6f;
        //}

    }

    public void ResetWithNetwork(NNet net)
    {
        network = net;
    }

    private void Death()
    {
        if (!runBest)
        {
            GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
        }
    }


}



/*
 Weights: 
0.3004971 1.252031 0.489248 -0.08979702 0.2085536 -0.3554655 -0.5145617 0.05196011 0.3396485 0.7871299 
-0.3509442 -0.06777707 -0.6813309 0.1993397 -1.247378 -1.675257 -0.2831541 -1.625249 -0.8799553 -0.595892 
0.5873142 -0.9833903 -0.08485818 0.6591334 0.4834194 1.793062 0.9870279 -0.3219013 0.38044 -0.3036634 
-0.2154254 -0.4140428 -0.03126442 0.8976433 0.3911082 -0.9654531 -0.6790106 0.584285 -0.02702844 -0.6802425 


-0.4904114 -0.8388591 
-0.7213833 0.872452 
-0.4098536 -0.8425779 
0.1504377 -0.7945564 
-0.9317698 0.1002508 
-0.2246093 -0.7769811 
-0.1107043 -0.1328086 
-0.5468252 0.0936836 
-0.04389846 0.9536641 
0.477201 0.6567857 


Biases: 
0.1515723 
 */

/*
 Weights: 
0.3004971 1.252031 0.489248 -0.08979702 0.2085536 -0.3554655 -0.5145617 0.05196011 0.3396485 0.7871299 
-0.3509442 -0.06777707 -0.6813309 0.1993397 -1.247378 -1.675257 -0.2831541 -1.625249 -0.8799553 -0.595892 
0.5873142 -0.9833903 -0.08485818 0.6591334 0.4834194 1.793062 0.9870279 -0.3219013 0.38044 -0.3036634 
-0.2154254 -0.4140428 -0.03126442 0.8976433 0.3911082 -0.9654531 -0.6790106 0.584285 -0.02702844 -0.6802425 


-0.4904114 -0.8388591 
-0.7213833 0.872452 
-0.4098536 -0.8425779 
0.1504377 -0.7945564 
-0.9317698 0.1002508 
-0.2246093 -0.7769811 
-0.1107043 -0.1328086 
-0.5468252 0.0936836 
-0.04389846 0.9536641 
0.477201 0.6567857 


Biases: 
0.1515723 
 */

/*
 Weights: 
0.3004971 1.252031 0.489248 -0.08979702 0.2085536 -0.3554655 -0.5145617 0.05196011 0.3396485 0.7871299 
-0.3509442 -0.06777707 -0.6813309 0.1993397 -1.247378 -1.675257 -0.2831541 -1.625249 -0.8799553 -0.595892 
0.5873142 -0.9833903 -0.08485818 0.6591334 0.4834194 1.793062 0.9870279 -0.3219013 0.38044 -0.3036634 
-0.2154254 -0.4140428 -0.03126442 0.8976433 0.3911082 -0.9654531 -0.6790106 0.584285 -0.02702844 -0.6802425 


-0.4904114 -0.8388591 
-0.7213833 0.872452 
-0.4098536 -0.8425779 
0.1504377 -0.7945564 
-0.9317698 0.1002508 
-0.2246093 -0.7769811 
-0.1107043 -0.1328086 
-0.5468252 0.0936836 
-0.04389846 0.9536641 
0.477201 0.6567857 


Biases: 
0.1515723 

 */