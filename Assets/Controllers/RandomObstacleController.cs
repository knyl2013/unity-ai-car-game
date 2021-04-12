using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObstacleController : MonoBehaviour
{
    void Start()
    {
        if (!noLoop) speed += Random.Range(-1f, 1f);
        current = period;
    }

    void Update()
    {
        GetComponent<Rigidbody>().velocity = new Vector3(speed, 0, 0);

        if (current <= 0 && !done)
        {
            current = period;
            float diff = Mathf.Abs(lastX - transform.position.x);
            if (diff < epislion)
            {
                if (noLoop)
                {
                    speed = 0;
                    done = true;
                }
                else
                {
                    restCur++;
                    if (restCur >= restPeriod)
                    {
                        restCur = 0;
                        speed *= -1;
                    }
                }
            }
            lastX = transform.position.x;
        }
        current--;
    }

    public float current;
    public float epislion = 0.001f;
    public float speed = 2f;
    public const float period = 100;
    public float nextActionTime;
    public float lastX = -1;
    public bool noLoop = false;
    public int restPeriod = 5;
    public int restCur = 0;
    private bool done = false;
}
