using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

// RL Agent of TestCar
public class TestCarAgentController : Agent
{
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
		rpm = Mathf.Max(frontDriverW.rpm, frontPassengerW.rpm);
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
	public void GetInput()
	{
		if (controlByHuman)
		{
			m_horizontalInput = Input.GetAxis("Horizontal");
			m_verticalInput = Input.GetAxis("Vertical");
		}
		else
        {
			RequestAction();
        }
		//else
		//{
		//	ISignalArray inputArr = box.InputSignalArray;

		//	inputArr[0] = aSensor;
		//	inputArr[1] = bSensor;
		//	inputArr[2] = cSensor;
		//	inputArr[3] = rpm;

		//	box.Activate();

		//	ISignalArray outputArr = box.OutputSignalArray;
		//	m_horizontalInput = (float)outputArr[0];
		//	m_verticalInput = (float)outputArr[1];

		//	// From (0, 1) to (-1, 1)
		//	m_horizontalInput = m_horizontalInput * 2 - 1;
		//	m_verticalInput = (m_verticalInput * 2 - 1);
		//}
	}
	private void FixedUpdate()
	{
        GetInput();
        Steer();
		Accelerate();
		UpdateWheelPoses();
		InputSensors();
        CalculateReward();
		//if (timeSinceStart > 300)
  //      {
		//	EndEpisode();
		//	Reset();
		//}
		//Academy.Instance.EnvironmentStep();
	}

	private void CalculateReward()
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

		lastPosition = transform.position;
		avgSpeed = totalDistanceTravelled / timeSinceStart;

		if (timeSinceStart < 300)
			overallFitness = (totalDistanceTravelled * distanceMultiplier) + (avgSpeed * avgSpeedMultiplier);

        //AddReward(overallFitness);

        if (Vector3.Distance(transform.position, lastPosition) > 0)
        {
			//AddReward(1f);
			AddReward(rpm * 0.1f);
        }
        //if (rpm < 0)
        //{
        //    AddReward(-0.5f);
        //}
    }

	private void OnCollisionEnter(Collision collision)
	{
		fits.Add(overallFitness);
        Debug.Log(overallFitness);
        if (round == 10)
        {
            string s = "";
            foreach (float f in fits)
            {
                s = s + ", " + f;
            }
            Debug.Log(s);
            round = 0;
        }
        round++;
        AddReward(-1000000f);
        EndEpisode();
        Reset();
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

	public override void CollectObservations(VectorSensor sensor)
	{
        sensor.AddObservation(aSensor);
        sensor.AddObservation(bSensor);
        sensor.AddObservation(cSensor);
        sensor.AddObservation(rpm);
        //Debug.Log("Collect Observations");
    }

	public override void OnActionReceived(ActionBuffers actionBuffers)

	{
		//Debug.Log("Received Actions");
		//Debug.Log(actionBuffers.ContinuousActions);
		var continuousActions = actionBuffers.ContinuousActions;
		//var moveX = Mathf.Clamp(continuousActions[0], -1f, 1f) * m_InvertMult;
		//var moveY = Mathf.Clamp(continuousActions[1], -1f, 1f);
		//var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f) * m_InvertMult;
		//Debug.Log(continuousActions[0]);
		m_horizontalInput = Mathf.Clamp(continuousActions[0], -1f, 1f);
		m_verticalInput = Mathf.Clamp(continuousActions[1], 0, 1f);
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

	public override void Initialize() {
		//rBody = GetComponent<Rigidbody>();
		startPosition = transform.position;
		startRotation = transform.eulerAngles;
		fits = new List<float>();
	}

	public override void OnEpisodeBegin() { }


	private Vector3 lastPosition;
	private Vector3 startPosition;
	private Vector3 startRotation;
	private int round = 0;

	public WheelCollider frontDriverW, frontPassengerW;
	public WheelCollider rearDriverW, rearPassengerW;
	public Transform frontDriverT, frontPassengerT;
	public Transform rearDriverT, rearPassengerT;
	public bool showSensor = true;
	public bool controlByHuman = true;
	public float maxSteerAngle = 30;
	public float motorForce = 500;
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
	public List<float> fits;
}
