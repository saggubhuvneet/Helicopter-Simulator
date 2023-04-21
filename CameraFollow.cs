using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public void Start()
    {
        transform.parent = GameObject.Find("Cheetah").transform;
        transform.position = new Vector3(0, 39.112f, 0);

    }
   
}
