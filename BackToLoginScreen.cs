using UnityEngine;
using UnityEngine.SceneManagement;


public class BackToLoginScreen : MonoBehaviour
{
    //  Back to Login screen 
    public void ReturnToMainScreen()
    {
        SceneManager.LoadScene(0);
    }
}
