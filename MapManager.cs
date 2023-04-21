using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public void LoadMap1() // Load Nashik 15km map -- high resolutions 
    {
        SceneManager.LoadScene(2);
    }
   
    public void LoadMap2() // load New york map
    {
        SceneManager.LoadScene(3);
    }
    
    public void LoadMap3() // Load Nashik 50 km map -- low resolution 
    { 
        SceneManager.LoadScene(4);
    }
    
    public void LoadMap4() // load network map -- 15k nashik low resolutions
    { 
        SceneManager.LoadScene(5);
    }
}

