using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpNeat.Phenomes;

// [RequireComponment(typeof(InputManager))]
public class GARaceCarController : MonoBehaviour
{
    // public InputManager inputManager;
    public List<WheelCollider> throttleWheels;
    public List<WheelCollider> steeringWheels;
    public float maxMotorTorque = 400;
    public float maxSteeringAngle = 10;

    public float motor;
    public float steering;

    public float aSensor, bSensor, cSensor;

    public Vector3 startPosition;

    public Vector3 startRotation;

    public float overallFitness = 0f;

    public float timeSinceStart = 0f;

    private float totalDistanceTravelled;
    private float avgSpeed;

    public float distanceMultiplier = 2.4f;
    public float avgSpeedMultiplier = 0.02f;

    public float fitnessToSave = 1000f;

    private Vector3 lastPosition;

    private IBlackBox box;

    private float bestFitness = 0f;

    private float penalty = 100f;

    public float epsilon = 0.001f;

    public bool controlByHuman = false;

    public float rpm = 0f;

    public bool showSensor = true;

    private NNet network;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    private void Awake()
    {
        network = new NNet(LAYERS, NEURONS);
    }

    // Start is called before the first frame update
    void Start()
    {
        // inputManager = GetComponent<InputManager>();
        startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        startRotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
    }

    bool Dead()
    {
        return false;
        //return aSensor <= epsilon || bSensor <= epsilon || cSensor <= epsilon;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        InputSensors();

        //if (Dead()) { Reset();  return; }

        if (controlByHuman)
        {
            motor = maxMotorTorque * Input.GetAxis("Vertical");
            steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        }
        else
        {
            //ISignalArray inputArr = box.InputSignalArray;
            //inputArr[0] = aSensor;
            //inputArr[1] = bSensor;
            //inputArr[2] = cSensor;
            //inputArr[3] = rpm;
            //inputArr[3] = transform.position.x;
            //inputArr[4] = transform.position.z;
            // inputArr[3] = overallFitness;

            //box.Activate();

            //ISignalArray outputArr = box.OutputSignalArray;
            //motor = (float)outputArr[0];
            //steering = (float)outputArr[1];

            // From (0, 1) to (-1, 1)
            //motor = motor * 2 - 1;
            //steering = steering * 2 - 1;

            

            (motor, steering) = network.RunNetwork(aSensor, bSensor, cSensor, rpm);

            motor *= maxMotorTorque;
            steering *= maxSteeringAngle;
        }

        MoveCar(motor, steering);

        timeSinceStart += Time.deltaTime;

        CalcFitness();
    }

    public void CalcFitness()
    {
        // Debug.Log("Last Poisition");
        // Debug.Log(lastPosition);
        // Debug.Log("Current Poisition");
        // Debug.Log(transform.position);

        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;
        overallFitness = totalDistanceTravelled * distanceMultiplier + avgSpeed * avgSpeedMultiplier;
        lastPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    private void InputSensors()
    {
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);
        int scale = 100;

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit))
        {
            aSensor = hit.distance / scale; // Normalize value
            // Debug.Log("A: " + aSensor);
            if (showSensor)
            {
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
        }

        r.direction = b;
        if (Physics.Raycast(r, out hit))
        {
            bSensor = hit.distance / scale; // Normalize value
            // Debug.Log("B: " + bSensor);
            if (showSensor)
            {
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
        }

        r.direction = c;
        if (Physics.Raycast(r, out hit))
        {
            cSensor = hit.distance / scale; // Normalize value
            // Debug.Log("C: " + cSensor);
            if (showSensor)
            {
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
        }
    }

    private void MoveCar(float motor, float steering)
    {
        foreach (WheelCollider wheel in throttleWheels)
        {
            wheel.motorTorque = motor;
            float rpmScale = 10000;
            rpm = (wheel.rpm + rpmScale) / (2*rpmScale);
        }
        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = steering;
        }

    }

    public void Reset()
    {
        bestFitness = Mathf.Max(bestFitness, overallFitness);
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
        steering = 0;
        motor = 0;
        //foreach (WheelCollider wheel in throttleWheels)
        //{
        //    wheel.brakeTorque = Mathf.Infinity;
        //}
        //Debug.Log("reset");
    }
    private void OnCollisionEnter(Collision collision)
    {
        Death();
        //Reset();
        //Reset();
        //if (collision.gameObject.GetComponent<RoadCourse>())
        //{
        //    Reset();
        //}
        //overallFitness -= penalty;
    }

    public float GetFitness()
    {
        CalcFitness();
        bestFitness = Mathf.Max(bestFitness, overallFitness);
        //if (bestFitness >= fitnessToSave)
        //{
        //    SaveNetwork();
        //}
        var fit = bestFitness;//cache the fitness value
        overallFitness = 0;//reset fitness value each time we start a new training cycle
        bestFitness = 0; //reset fitness value each time we start a new training cycle
        if (fit < 0 || double.IsNaN(fit) || double.IsInfinity(fit))
            fit = 0;

        return fit;
    }


    public void SaveNetwork()
    {

    }

    public void ResetWithNetwork(NNet net)
    {
        network = net;
    }

    private void Death()
    {
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
        Reset();
    }
}
