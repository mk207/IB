using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform Cam;
    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position + Cam.forward);
        //transform.LookAt(Cam.position - transform.position);
    }
}
