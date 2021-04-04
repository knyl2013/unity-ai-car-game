using System.Collections;
using System.Collections.Generic;
using SharpNeat.Phenomes;
using UnityEngine;

public class NeatMotionCarController : UnitController
{
	public void GetInput()
	{
		if (controlByHuman)
		{
			m_horizontalInput = Input.GetAxis("Horizontal");
			m_verticalInput = Input.GetAxis("Vertical");
		}
		else
		{
			ISignalArray inputArr = box.InputSignalArray;

			inputArr[0] = aSensor;
			inputArr[1] = bSensor;
			inputArr[2] = cSensor;
			inputArr[3] = rpm;
			inputArr[4] = lastA;
			inputArr[5] = lastB;
			inputArr[6] = lastC;

			box.Activate();

			ISignalArray outputArr = box.OutputSignalArray;
			m_horizontalInput = (float)outputArr[0];
			m_verticalInput = (float)outputArr[1];

			// From (0, 1) to (-1, 1)
			m_horizontalInput = m_horizontalInput * 2 - 1;
			m_verticalInput = (m_verticalInput * 2 - 1);


		}
	}

	private void Steer()
	{
		m_steeringAngle = maxSteerAngle * m_horizontalInput;
		frontDriverW.steerAngle = m_steeringAngle;
		frontPassengerW.steerAngle = m_steeringAngle;
	}

	private void Accelerate()
	{
		frontDriverW.motorTorque = m_verticalInput * motorForce;
		frontPassengerW.motorTorque = m_verticalInput * motorForce;
		//rpm = Mathf.Max(frontDriverW.rpm, frontPassengerW.rpm) / rpmScale;
		if (movingForward)
		{
			rpm = rigidbody.velocity.magnitude * 3.6f;
		}
		else
		{
			rpm = -rigidbody.velocity.magnitude * 3.6f;
		}
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
		Steer();
		Accelerate();
		UpdateWheelPoses();
		InputSensors();
		CalculateFitness();
		GetDirection();
	}

	private void InputSensors()
	{
		lastA = aSensor;
		lastB = bSensor;
		lastC = cSensor;

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
		rpm = 0f;
	}

	private void CalculateFitness()
	{
		timeSinceStart += Time.deltaTime;
		if (rpm > 0)
		{
			totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
		}
		else
		{
			totalDistanceTravelled -= Vector3.Distance(transform.position, lastPosition);
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

	public void GetDirection()
	{
		var velocity = rigidbody.velocity;
		var localVel = transform.InverseTransformDirection(velocity);
		movingForward = localVel.z > 0;
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
	public bool movingForward = false;
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
	public float lastA;
	public float lastB;
	public float lastC;
	public float totalDistanceTravelled;
	public float avgSpeed;
	public float m_horizontalInput;
	public float m_verticalInput;
	public float m_steeringAngle;
	public float rpmScale = 1000f;
	

	public bool IsRunning { get; set; }
}
