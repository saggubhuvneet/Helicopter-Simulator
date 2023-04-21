using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectFollow : MonoBehaviour
{
    /* to get the disdance between the helicopter and the particle effect
     * and follow the helicopter*/

    [SerializeField] GameObject _helicopter;
    Vector3 _distance;


    private void Start()
    {
        _distance = transform.position - _helicopter.transform.position;
    }
    private void Update()
    {
        transform.position = _helicopter.transform.position + _distance;
    }
}
