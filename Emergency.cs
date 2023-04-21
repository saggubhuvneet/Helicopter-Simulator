using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emergency : MonoBehaviour
{
    [SerializeField] GameObject Helicopter;
    [SerializeField] GameObject Helicopter_structure;
    [SerializeField] GameObject fire;
    [SerializeField] GameObject MainRotor;
    [SerializeField] GameObject DroopRotor;

    int _randomNO;
    float dynamicRollValue;

    bool isEngineFailed = false;
    bool isRotorFail = false;
    bool isServoFail = false;
    bool isGovernerFail = false;
    bool isDynamicRoll = false;
    bool isDroopStopFail = false;

    public static bool istailRoterCableRubture = false;
    public static bool isEngienFailedTosart = false;
    public static bool isTotalElectricFailure = false;

    private void Update()
    {
        if (isEngineFailed == true) 
        { 
            Helicopter.transform.Rotate(new Vector3(0, 90, 0) * Time.deltaTime);
            Debug.Log("Engine Falure");
        }
        if (isRotorFail == true) { Helicopter.transform.Rotate(new Vector3(0, -90, 0) * Time.deltaTime); }
        if (isServoFail == true) { Helicopter.transform.Rotate(new Vector3(-20, 0, -15) * Time.deltaTime); }
        if (isDynamicRoll == true)
        {
            Helicopter.transform.Rotate(new Vector3(0, 0, dynamicRollValue) * Time.deltaTime);
        }
        if (isGovernerFail == true)
        {
            if (_randomNO == 0)
            {
                Helicopter.transform.position = new Vector3(Helicopter.transform.position.x, Mathf.Lerp(Helicopter.transform.position.y, Helicopter.transform.position.y + 10f, Time.deltaTime), Helicopter.transform.position.z);
            }
            if (_randomNO == 1)
            {
                Helicopter.transform.position = new Vector3(Helicopter.transform.position.x, Mathf.Lerp(Helicopter.transform.position.y, Helicopter.transform.position.y - 10f, Time.deltaTime), Helicopter.transform.position.z);
                Helicopter.transform.Rotate(new Vector3(0, 15, 0) * Time.deltaTime);

            }
        }


        if (Input.GetKeyDown(KeyCode.F2))
        {
            isEngineFailed = false;
            isRotorFail = false;
            isServoFail = false;
            Helicopter_structure.GetComponent<Animator>().Play("idle");
            if (isDroopStopFail == true)
            {
                MainRotor.SetActive(false);
                DroopRotor.SetActive(true);
                DroopRotor.GetComponent<Animator>().Play("activeDroop");
            }

        }
    }
    // call emergency engine fire------------
    //helicopter yaw right
    public void EngineFailure()
    {
        isEngineFailed = true;

    }
    // call emergency tail Rotor falire
    // helicopter yaw left
    public void RotorFailure()
    {
        isRotorFail = true;
    }

    // call emergency Servo fail 
    // pitch up and roll
    public void ServoFail()
    {
        isServoFail = true;
    }

    // Tail rotor rupture
    // rudders will not work 
    public void TailRotorCableRupture()
    {
        istailRoterCableRubture = true;
    }

    // Governer fail
    // gain in height OR lose in height and Yaw to the left
    public void GovernerFail()
    {
        isGovernerFail = true;
        _randomNO = Random.Range(0, 2);
        StartCoroutine(StopGovernerFail());
    }
    IEnumerator StopGovernerFail()
    {
        yield return new WaitForSeconds(5f);
        isGovernerFail = false;
        isDynamicRoll = false;
    }

    // Engine Failed to start
    // engine will not start
    public void EngineFailedToStart(bool toggle)
    {
        if (toggle == true)
        {
            isEngienFailedTosart = true;
        }
        if (toggle == false)
        {
            isEngienFailedTosart = false;
        }
    }

    // Electric Failure
    // waring lights, eletric dials, and raido cmmunications will not work
    public void ElectricFailure()
    {
        isTotalElectricFailure = true;
    }

    // blade staling
    // whole helicopter will shake 
    public void BladeStaling(bool toggle)
    {
        if (toggle == true)
        {
            Helicopter_structure.GetComponent<Animator>().Play("shake");
        }
        if (toggle == false)
        {
            Helicopter_structure.GetComponent<Animator>().Play("idle");
        }
    }

    // Engine fire
    // sparks and smoke
    public void EngineFire()
    {
        fire.SetActive(true);
    }

    // clutch unity fail
    // Gain in height
    public void ClutchUnitFailure()
    {
        _randomNO = 1;
        isGovernerFail = true;
    }

    // Dynamic roll over
    // when helicopter is landed on a sloy surface and we take off, it continously rolling with same angle

    public void DynamicRoll()
    {
        dynamicRollValue = Helicopter.transform.eulerAngles.z;
        isDynamicRoll = true;
        StartCoroutine(StopGovernerFail());
    }

    // main rotors blade will bend drastactily down

    public void DroopStopFaliure()
    {
        isDroopStopFail = true;
    }

}
