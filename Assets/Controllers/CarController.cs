using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponment(typeof(InputManager))]
public class CarController : MonoBehaviour
{
    public InputManager inputManager;
    public List<WheelCollider> throttleWheels;
    public List<WheelCollider> steeringWheels;
    public float maxMotorTorque = 400;
    public float maxSteeringAngle = 10;

    public Vector3 startPosition;


    // Start is called before the first frame update
    void Start()
    {
        inputManager = GetComponent<InputManager>();
        startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        foreach (WheelCollider wheel in throttleWheels)
        {
            wheel.motorTorque = motor;
        }



        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = steering;
        }
    }

    private void OnCollisionEnter(Collision other) {
        
    }
}
