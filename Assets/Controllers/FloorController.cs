using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<NeatRaceCarController>())
        {
            NeatRaceCarController neatRaceCarController = collision.gameObject.GetComponent<NeatRaceCarController>();
            neatRaceCarController.Reset();
        }
        Debug.Log("collision");
        if (collision.gameObject.GetComponent<NeatTestCarController>())
        {
            Debug.Log("detected collision");
            NeatTestCarController neatTestCarController = collision.gameObject.GetComponent<NeatTestCarController>();
            neatTestCarController.Reset();
        }

        if (collision.gameObject.GetComponentInParent<NeatTestCarController>())
        {
            Debug.Log("detected wheel");
            NeatTestCarController neatTestCarController = collision.gameObject.GetComponentInParent<NeatTestCarController>();
            neatTestCarController.Reset();
        }
    }
}
