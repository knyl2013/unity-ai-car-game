using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObstacleCourseController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        optimizer = evaluator.GetComponent<Optimizer>();
        //ResetObstacles();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Time.time > nextActionTime)
        //{
        //    nextActionTime += period;
        //    ResetObstacles();
        //}
        //Debug.Log(optimizer.Generation + 1);
        if (optimizer.Generation >= nextActionTime)
        {
            nextActionTime = optimizer.Generation + period;
            ResetObstacles();
        }
    }
    void ResetObstacles()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.CompareTag("Obstacle")) continue;
            RandomObstacleController rc = child.gameObject.GetComponent<RandomObstacleController>();
            rc.speed = 10;
            if (Random.Range(0f, 2f) > 1f)
            {
                rc.speed *= -1;
            }
            rc.noLoop = true;
        }
    }

    public int period = 5;
    public float nextActionTime;
    private Optimizer optimizer;
    public GameObject evaluator;
}
