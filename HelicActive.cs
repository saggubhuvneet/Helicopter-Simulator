using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicActive : MonoBehaviour
{
    // helicopter selection in main menu

    [SerializeField] GameObject cheetah, cheetak;
    public void Start()
    {
        if (HeliSelection.Instance.isCheetahActive == true)
            cheetah.SetActive(true);
        else
            cheetak.SetActive(true);

    }
}
