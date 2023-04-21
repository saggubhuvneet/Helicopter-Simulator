using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]


public class RainControll : MonoBehaviour
{
    private ParticleSystem rain;
    // Start is called before the first frame update
    void Start()
    {
        rain = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame

    public void OnRainValue(float value)
    {
        var emission = rain.emission;
        emission.rateOverTime = value;

    }
}
