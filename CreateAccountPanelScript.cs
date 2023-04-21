using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateAccountPanelScript : MonoBehaviour
{
    [SerializeField]
    public InputField newUsernameInput;
    [SerializeField]
    public InputField newPasswordInput;
    [SerializeField]
    public InputField confirmPasswordInput;
    [SerializeField]
    public InputField specialKeyInput;

    [SerializeField]
    public GameObject loginPanel;
    [SerializeField]
    public GameObject createAccountPanel;
    [SerializeField]
    public GameObject forgetPasswordPanel;
    [SerializeField]
    public Text messageText;

    private string hardcodedSpecialKey = "O2I";

    public void OnCreateAccountConfirmButtonClick()
    {
        // Check if the password and confirm password match
        if (newPasswordInput.text == confirmPasswordInput.text && specialKeyInput.text == hardcodedSpecialKey)
        {
            // Save the new username and password
            PlayerPrefs.SetString(newUsernameInput.text, newPasswordInput.text);
            messageText.text = "Account Created";
            // Switch back to the Login panel
            loginPanel.SetActive(true);
            createAccountPanel.SetActive(false);
            forgetPasswordPanel.SetActive(false);
        }        
        else
        {
            // Display an error message
            messageText.text = "Spacial Key Not Match!";
        }
    }
    public void OnLogInPanelButtonClick()
    {
        // Switch to the Create Account panel
        loginPanel.SetActive(true);
        createAccountPanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }
    public void OnForgetPanelButtonClick()
    {
        // Switch to the Forget Password panel
        loginPanel.SetActive(false);
        createAccountPanel.SetActive(false);
        forgetPasswordPanel.SetActive(true);
    }
    // Return to the main screeen
    public void ReturnToMainScreen()
    {
        SceneManager.LoadScene(0);
    }
}
