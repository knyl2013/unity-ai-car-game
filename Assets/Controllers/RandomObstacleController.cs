using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObstacleController : MonoBehaviour
{
    void Start()
    {
        period = 1;
        if (!noLoop) speed += Random.Range(-1f, 1f);
    }

    void Update()
    {
        GetComponent<Rigidbody>().velocity = new Vector3(speed, 0, 0);

        if (Time.time > nextActionTime && !done)
        {
            nextActionTime += period;
            float diff = Mathf.Abs(lastX - transform.position.x);
            if (diff < epislion)
            {
                speed *= -1;
                if (noLoop)
                {
                    speed = 0;
                    done = true;
                }
            }
            lastX = transform.position.x;
        }
    }

    public float epislion = 0.001f;
    public float speed = 2f;
    public float period;
    public float nextActionTime;
    public float lastX = -1;
    public bool noLoop = false;
    private bool done = false;
}
