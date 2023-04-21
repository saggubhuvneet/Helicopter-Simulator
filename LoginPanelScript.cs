using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginPanelScript : MonoBehaviour
{
    [SerializeField]
    public InputField usernameInput;
    [SerializeField]
    public InputField passwordInput;

    [SerializeField]
    public GameObject loginPanel;
    [SerializeField]
    public GameObject createAccountPanel;
    [SerializeField]
    public GameObject forgetPasswordPanel;
    [SerializeField]
    public Text messageText;

    public void OnLoginButtonClick()
    {
        if (PlayerPrefs.HasKey(usernameInput.text) && PlayerPrefs.GetString(usernameInput.text) == passwordInput.text)
        {
            // Switch to the main scene
            SceneManager.LoadScene(1);
        }
        else
        {
            // Display an error message
            messageText.text = "Invalid username or password!";
        }
    }
    public void OnCreateAccountButtonClick()
    {
        // Switch to the Create Account panel
        loginPanel.SetActive(false);
        createAccountPanel.SetActive(true);
        forgetPasswordPanel.SetActive(false);
    }
    public void OnForgetPasswordButtonClick()
    {
        // Switch to the Forget Password panel
        loginPanel.SetActive(false);
        createAccountPanel.SetActive(false);
        forgetPasswordPanel.SetActive(true);
    }
}
