﻿using System.Collections;
using System.Collections.Generic;
using SharpNeat.Phenomes;
using UnityEngine;

public class NeatDriftCarController : UnitController
{
	public void GetInput()
	{
		if (controlByHuman)
		{
			m_horizontalInput = Input.GetAxis("Horizontal");
			m_verticalInput = Input.GetAxis("Vertical");
			m_breakInput = (Input.GetAxis("Jump") != 0) ? true : false;
		}
		else
		{
			ISignalArray inputArr = box.InputSignalArray;

			inputArr[0] = aSensor;
			inputArr[1] = bSensor;
			inputArr[2] = cSensor;
			inputArr[3] = rpm;

			box.Activate();

			ISignalArray outputArr = box.OutputSignalArray;
			m_horizontalInput = (float)outputArr[0];
			m_verticalInput = (float)outputArr[1];
			m_breakInput = (outputArr[2] > 0.5) ? true : false;

			// From (0, 1) to (-1, 1)
			m_horizontalInput = m_horizontalInput * 2 - 1;
			m_verticalInput = (m_verticalInput * 2 - 1);
		}
	}

	//private void Steer()
	//{
	//	m_steeringAngle = maxSteerAngle * m_horizontalInput;
	//	frontDriverW.steerAngle = m_steeringAngle;
	//	frontPassengerW.steerAngle = m_steeringAngle;
	//}
	private void Steer()
	{
		if (m_horizontalInput > 0)
		{
			wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * Mathf.Clamp(m_horizontalInput, -1, 1);
			wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * Mathf.Clamp(m_horizontalInput, -1, 1);
		}
		else if (m_horizontalInput < 0)
		{
			wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * Mathf.Clamp(m_horizontalInput, -1, 1);
			wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * Mathf.Clamp(m_horizontalInput, -1, 1);
		}
	}

	//private void Accelerate()
	//{
	//	frontDriverW.motorTorque = m_verticalInput * motorForce;
	//	frontPassengerW.motorTorque = m_verticalInput * motorForce;
	//	rpm = Mathf.Max(frontDriverW.rpm, frontPassengerW.rpm);
	//}

	private void Accelerate()
    {
		float velocity = 0.0f;
		wheels[0].motorTorque = (totalPower / 2);
		wheels[1].motorTorque = (totalPower / 2);
		for (int i = 2; i < wheels.Length; i++)
		{
			if (m_breakInput)
			{
				wheels[i].brakeTorque = (breakForce / 2);
				forwardFriction = wheels[i].forwardFriction;
				forwardFriction.stiffness = Mathf.SmoothDamp(forwardFriction.stiffness, drift, ref velocity, Time.deltaTime * 1);
				wheels[i].forwardFriction = forwardFriction;

				sidewaysFriction = wheels[i].sidewaysFriction;
				sidewaysFriction.stiffness = Mathf.SmoothDamp(sidewaysFriction.stiffness, drift, ref velocity, Time.deltaTime * 1);
				wheels[i].sidewaysFriction = sidewaysFriction;
			}
			else
			{
				wheels[i].brakeTorque = 0;
				forwardFriction = wheels[i].forwardFriction;
				forwardFriction.stiffness = 1;
				wheels[i].forwardFriction = forwardFriction;

				sidewaysFriction = wheels[i].sidewaysFriction;
				sidewaysFriction.stiffness = 1;
				wheels[i].sidewaysFriction = sidewaysFriction;
			}
		}

		KPH = rigidbody.velocity.magnitude * 3.6f;
	}

	private void UpdateWheelPoses()
	{
		UpdateWheelPose(frontDriverW, frontDriverT);
		UpdateWheelPose(frontPassengerW, frontPassengerT);
		UpdateWheelPose(rearDriverW, rearDriverT);
		UpdateWheelPose(rearPassengerW, rearPassengerT);
	}

	private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
	{
		Vector3 _pos = _transform.position;
		Quaternion _quat = _transform.rotation;

		_collider.GetWorldPose(out _pos, out _quat);

		_transform.position = _pos;
		_transform.rotation = _quat;
	}

	private void FixedUpdate()
	{
		GetInput();
		addDownForce();
		Steer();
		UpdateWheelPoses();
		calEnginePower();

		InputSensors();
		CalculateFitness();
	}

	private void InputSensors()
	{
		Vector3 a = (transform.forward + transform.right);
		Vector3 b = (transform.forward);
		Vector3 c = (transform.forward - transform.right);
		int scale = 1;

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
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
	}

	private void CalculateFitness()
	{
		timeSinceStart += Time.deltaTime;
		if (wheels[0].rpm < 0 || wheels[2].rpm < 0)
		{
			totalDistanceTravelled -= Vector3.Distance(transform.position, lastPosition);
		}
		else
		{
			totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
		}

		//totalDistanceTravelled = Vector3.Distance(transform.position, startPosition);

		lastPosition = transform.position;
		avgSpeed = totalDistanceTravelled / timeSinceStart;

		overallFitness = (totalDistanceTravelled * distanceMultiplier) + (avgSpeed * avgSpeedMultiplier);


		if (overallFitness >= 5000)
		{
			Reset();
		}

		if (transform.position.y < -10)
		{
			Reset();
		}

	}

	private void Start()
	{
		startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		startRotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
		rigidbody = GetComponent<Rigidbody>();

		//wheelT[0] = wheelMeshes.transform.Find("FrontLeftWheel").gameObject.GetComponent<Transform>();
		//wheelT[1] = wheelMeshes.transform.Find("FrontRightWheel").gameObject.GetComponent<Transform>();
		//wheelT[2] = wheelMeshes.transform.Find("RearLeftWheel").gameObject.GetComponent<Transform>();
		//wheelT[3] = wheelMeshes.transform.Find("RearRightWheel").gameObject.GetComponent<Transform>();

		//wheels[0] = wheelColliders.transform.Find("FrontLeftWheel").gameObject.GetComponent<WheelCollider>();
		//wheels[1] = wheelColliders.transform.Find("FrontRightWheel").gameObject.GetComponent<WheelCollider>();
		//wheels[2] = wheelColliders.transform.Find("RearLeftWheel").gameObject.GetComponent<WheelCollider>();
		//wheels[3] = wheelColliders.transform.Find("RearRightWheel").gameObject.GetComponent<WheelCollider>();

		wheelT[0] = frontDriverT;
		wheelT[1] = frontPassengerT;
		wheelT[2] = rearDriverT;
		wheelT[3] = rearPassengerT;

		wheels[0] = frontDriverW;
		wheels[1] = frontPassengerW;
		wheels[2] = rearDriverW;
		wheels[3] = rearPassengerW;
	}

	private void OnCollisionEnter(Collision collision)
	{
		Reset();
	}

	public override void Stop()
	{
		this.IsRunning = false;
	}

	public override void Activate(IBlackBox box)
	{
		this.box = box;
		this.IsRunning = true;
	}

	public override float GetFitness()
	{
		CalculateFitness();
		bestOverallFitness = Mathf.Max(bestOverallFitness, overallFitness);
		var fit = bestOverallFitness; //cache the fitness value
		overallFitness = 0;//reset fitness value each time we start a new training cycle
		bestOverallFitness = 0; //reset fitness value each time we start a new training cycle
		if (fit < 0 || double.IsNaN(fit) || double.IsInfinity(fit))
			fit = 0;

		return fit;
	}


	private void calEnginePower()
	{
		wheelRPM();
		if (m_verticalInput != 0)
		{
			rigidbody.drag = 0.005f;
		}
		if (m_verticalInput == 0)
		{
			rigidbody.drag = 0.15f;
		}

		totalPower = enginePower.Evaluate(engineRPM) * (gears[gearNum]) * m_verticalInput;
		float velocity = 0.0f;
		engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + (Mathf.Abs(wheelsRPM) * 3.6f * (gears[gearNum])), ref velocity, smoothTime);
		gearShifter();
		Accelerate();
	}

	private void wheelRPM()
	{
		float sum = 0;
		for (int i = 0; i < wheels.Length; i++)
		{
			sum += wheels[i].rpm;
        }
        rpm = wheels[0].rpm;
        wheelsRPM = sum / 4;
	}

	private void addDownForce()
	{
		rigidbody.AddForce(-transform.up * 100.0f * rigidbody.velocity.magnitude);
	}

	private void gearShifter()
	{
		if (engineRPM > maxRPM && gearNum < gears.Length - 1)
		{
			gearNum++;
		}
		if (engineRPM < minRPM && gearNum > 0)
		{
			gearNum--;
		}
	}


	private Rigidbody rigidbody;
	private IBlackBox box;
	private Vector3 lastPosition;
	private Vector3 startPosition;
	private Vector3 startRotation;


	public WheelCollider frontDriverW, frontPassengerW;
	public WheelCollider rearDriverW, rearPassengerW;
	public Transform frontDriverT, frontPassengerT;
	public Transform rearDriverT, rearPassengerT;
	public bool showSensor = true;
	public bool controlByHuman = true;
	public float maxSteerAngle = 30;
	public float motorForce = 5000;
	public float distanceMultiplier = 2.4f;
	public float avgSpeedMultiplier = 0.02f;
	public float overallFitness = 0f;
	public float bestOverallFitness = 0f;
	public float timeSinceStart = 0f;
	public float rpm;
	public float aSensor;
	public float bSensor;
	public float cSensor;
	public float totalDistanceTravelled;
	public float avgSpeed;
	public float m_horizontalInput;
	public float m_verticalInput;
	public float m_steeringAngle;

	private GameObject wheelColliders, wheelMeshes;
	private WheelCollider[] wheels = new WheelCollider[4];
	private Transform[] wheelT = new Transform[4];
	private float wheelsRPM;
	private WheelFrictionCurve forwardFriction, sidewaysFriction;


	public bool m_breakInput;
	public float drift = .3f;
	public float radius = 3;
	public float breakForce = 5000;
	public float smoothTime = 0.01f;
	public float maxRPM = 5600.0f;
	public float minRPM = 3000.0f;
	public float[] gears;
	public int gearNum = 0;
	public float totalPower = 0;
	public float KPH;
	public float engineRPM;
	public AnimationCurve enginePower;
	public GameObject centerOfMass;

	public bool IsRunning { get; set; }
}
