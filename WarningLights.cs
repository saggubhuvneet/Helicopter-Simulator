using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningLights : MonoBehaviour
{
    [Header("Warning Lights")]
    [SerializeField] GameObject light0;
    [SerializeField] GameObject light1;
    [SerializeField] GameObject light2;
    [SerializeField] GameObject light3;
    [SerializeField] GameObject light4;
    [SerializeField] GameObject light5;
    [SerializeField] GameObject light6;
    [SerializeField] GameObject light7;
    [SerializeField] GameObject light8;


    private void Update()
    {
        if (Emergency.isTotalElectricFailure == true)
        {
            light0.GetComponent<Renderer>().material.color = Color.white;
            light1.GetComponent<Renderer>().material.color = Color.white;
            light2.GetComponent<Renderer>().material.color = Color.white;
            light3.GetComponent<Renderer>().material.color = Color.white;
            light4.GetComponent<Renderer>().material.color = Color.white;
            light5.GetComponent<Renderer>().material.color = Color.white;
            light6.GetComponent<Renderer>().material.color = Color.white;
            light7.GetComponent<Renderer>().material.color = Color.white;
            light8.GetComponent<Renderer>().material.color = Color.white;


        }


    }


    //--------------------------------------------------------------------------------------Warning Lights
    public void SwitchControl(int val)
    {
        if (Emergency.isTotalElectricFailure == true)
            return;


        if (val == 1) { light0.GetComponent<Renderer>().material.color = Color.red; }
        if (val == 3) { light2.GetComponent<Renderer>().material.color = Color.red; }
        if (val == 4) { light3.GetComponent<Renderer>().material.color = Color.red; }
        if (val == 5) { light4.GetComponent<Renderer>().material.color = Color.red; }
        if (val == 6) { light5.GetComponent<Renderer>().material.color = Color.red; }
        if (val == 7) { light6.GetComponent<Renderer>().material.color = Color.red; }
        if (val == 8) { light7.GetComponent<Renderer>().material.color = Color.red; }
        if (val == 9) { light8.GetComponent<Renderer>().material.color = Color.red; }
        if (val == 2) { light1.GetComponent<Renderer>().material.color = Color.red; }


    }
}
