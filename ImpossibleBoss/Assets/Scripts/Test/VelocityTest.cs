using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityTest : MonoBehaviour
{
    
    void Start()
    {
        GetComponent<Rigidbody>().velocity = new Vector3(1.0f, 0.0f, 0.0f);
    }

    
}
