using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestrictPosition : MonoBehaviour
{
    Transform initialPosition;
    private void Start()
    {
        //Vector3 initialPosition =  transform.position;
    }
   
    private void Update()
    {
        ClampFunction();
        //initialPosition = Mathf.Clamp(transform.position.x, -1f, 1f);
    }
    void ClampFunction()
    {
        var pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, -0.2f, 0.3f);
        pos.y = Mathf.Clamp(transform.position.y, -0.3f, 0.2f);
        pos.z = Mathf.Clamp(transform.position.z, 0f, 0.3f);

        transform.position = pos;
    }
}
