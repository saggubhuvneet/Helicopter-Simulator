using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class WeatherControl : MonoBehaviour
{
    [SerializeField] Material sky1;
    [SerializeField] Material sky2;
    [SerializeField] Material sky3;
    [SerializeField] Material sky4;
    [SerializeField] Material sky5;
    [SerializeField] Material sky6;

    [SerializeField] Light _light;

    public void SkyManager(int var)
    {
        if(var == 1){RenderSettings.skybox = sky1;}
        if(var == 2){RenderSettings.skybox = sky2;}
        if(var == 3){RenderSettings.skybox = sky3;}
        if(var == 4){RenderSettings.skybox = sky4;}
        if(var == 5){RenderSettings.skybox = sky5;}
        if(var == 6){RenderSettings.skybox = sky6;}
    }

    public void DayNight(float temp)
    {
        _light.intensity = temp;
    }
}
