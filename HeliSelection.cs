using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliSelection : MonoBehaviour
{
    // helicopter selection in main menu

    public static HeliSelection Instance;

    public void Awake()
    {
        Instance = this;
    }

    public bool isCheetahActive = true;
    public void OnvalueChaned(int a)
    {
        if (a == 1)
        {
            Debug.Log("chatak");
            isCheetahActive = false;
        }
        else
        {
            Debug.Log("cheetah");
            isCheetahActive = true;
        }
    }
}
