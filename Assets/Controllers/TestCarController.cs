using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCarController : MonoBehaviour
{
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        centerOfMass = GameObject.Find("mass");
        rb.centerOfMass = centerOfMass.transform.localPosition;

        wheelColliders = GameObject.Find("WheelCollider");
        wheelMeshes = GameObject.Find("WheelMesh");

        wheelT[0] = wheelMeshes.transform.Find("FrontLeftWheel").gameObject.GetComponent<Transform>();
        wheelT[1] = wheelMeshes.transform.Find("FrontRightWheel").gameObject.GetComponent<Transform>();
        wheelT[2] = wheelMeshes.transform.Find("RearLeftWheel").gameObject.GetComponent<Transform>();
        wheelT[3] = wheelMeshes.transform.Find("RearRightWheel").gameObject.GetComponent<Transform>();

        wheels[0] = wheelColliders.transform.Find("FrontLeftWheel").gameObject.GetComponent<WheelCollider>();
        wheels[1] = wheelColliders.transform.Find("FrontRightWheel").gameObject.GetComponent<WheelCollider>();
        wheels[2] = wheelColliders.transform.Find("RearLeftWheel").gameObject.GetComponent<WheelCollider>();
        wheels[3] = wheelColliders.transform.Find("RearRightWheel").gameObject.GetComponent<WheelCollider>();

    }

    public void GetInput()
    {
        m_horizontalInput = Input.GetAxis("Horizontal");
        m_verticalInput = Input.GetAxis("Vertical");
        m_breakInput = (Input.GetAxis("Jump") != 0) ? true : false;
    }

    private void Steer()
    {

        horizontal = Mathf.Lerp(horizontal, m_horizontalInput, (m_horizontalInput != 0) ? 2 * Time.deltaTime : 3 * 2 * Time.deltaTime);
        if (horizontal > 0)
        {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * Mathf.Clamp(m_horizontalInput, -1, 1);
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * Mathf.Clamp(m_horizontalInput, -1, 1);
        }
        else if (horizontal < 0)
        {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * Mathf.Clamp(m_horizontalInput, -1, 1);
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * Mathf.Clamp(m_horizontalInput, -1, 1);
        }
        else
        {
            wheels[0].steerAngle = 0;
            wheels[1].steerAngle = 0;
        }
    }

    private void Accelerate()
    {
        float velocity = 0.0f;
        wheels[0].motorTorque = (totalPower / 2);
        wheels[1].motorTorque = (totalPower / 2);
        for (int i = 2; i < wheels.Length; i++)
        {
            if (m_breakInput)
            {
                wheels[i].brakeTorque = Mathf.Infinity;
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
                forwardFriction.stiffness = 1.1f;
                wheels[i].forwardFriction = forwardFriction;

                sidewaysFriction = wheels[i].sidewaysFriction;
                sidewaysFriction.stiffness = 1.1f;
                wheels[i].sidewaysFriction = sidewaysFriction;
            }
        }

        KPH = rb.velocity.magnitude * 3.6f;
    }

    private void UpdateWheelPoses()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            UpdateWheelPose(wheels[i], wheelT[i]);
        }
    }

    private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
    {
        Vector3 _pos = _transform.position;
        Quaternion _quat = _transform.rotation;

        _collider.GetWorldPose(out _pos, out _quat);

        _transform.position = _pos;
        _transform.rotation = _quat;
    }

    private void calEnginePower()
    {
        lerpEngine();
        wheelRPM();

        rb.angularDrag = (KPH > 100) ? KPH / 100 : 0;
        rb.drag = 0.15f + (KPH / 40000);

        if (engineRPM >= maxRPM)
        {
            setEngineLerp(maxRPM - 1000);
        }
        if (!engineLerp)
        {
            totalPower = enginePower.Evaluate(engineRPM) * (gears[gearNum] * finalDrive) * m_verticalInput;
            engineRPM = Mathf.Lerp(engineRPM, 1000 + (Mathf.Abs(wheelsRPM) * finalDrive * (gears[gearNum])), (smoothTime * 10) * Time.deltaTime);
        }

        engineLoad = Mathf.Lerp(engineLoad, m_verticalInput - ((engineRPM - 1000) / maxRPM), (smoothTime * 10) * Time.deltaTime);

        gearShifter();
        Accelerate();
    }

    private void setEngineLerp(float num)
    {
        engineLerp = true;
        engineLerpValue = num;
    }

    public void lerpEngine()
    {
        if (engineLerp)
        {
            totalPower = 0;
            engineRPM = Mathf.Lerp(engineRPM, engineLerpValue, 20 * Time.deltaTime);
            engineLerp = engineRPM <= engineLerpValue + 100 ? false : true;
        }
    }

    private void wheelRPM()
    {
        float sum = 0;
        for (int i = 0; i < wheels.Length; i++)
        {
            sum += wheels[i].rpm;
        }
        wheelsRPM = sum / 4;
    }

    private void addDownForce()
    {
        rb.AddForce(-transform.up * 100.0f * rb.velocity.magnitude);
    }

    private void gearShifter()
    {
        if (engineRPM > maxRPM && gearNum < gears.Length - 1 && Time.time >= gearChangeRate)
        {
            gearNum++;
            setEngineLerp(engineRPM - (engineRPM / 3));
            gearChangeRate = Time.time + 1f / 1f;
        }
        if (engineRPM < minRPM && gearNum > 0 && Time.time >= gearChangeRate)
        {
            gearChangeRate = Time.time + 0.15f;
            setEngineLerp(engineRPM + (engineRPM / 2));
            gearNum--;
        }
    }

    private void FixedUpdate()
    {
        GetInput();
        addDownForce();
        Steer();
        UpdateWheelPoses();
        calEnginePower();
    }

    private GameObject wheelColliders, wheelMeshes;
    private WheelCollider[] wheels = new WheelCollider[4];
    private Transform[] wheelT = new Transform[4];
    private float m_horizontalInput, m_verticalInput, wheelsRPM;
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    private bool m_breakInput;
    private Rigidbody rb;
    private GameObject centerOfMass;
    private bool engineLerp;
    private float gearChangeRate;
    private float engineLerpValue;
    private float engineLoad = 1;
    private float horizontal;

    public float timeSinceStoppeed = 0f;
    public float finalDrive = 4f;
    public float drift = .55f;
    public float radius = 6;
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
}