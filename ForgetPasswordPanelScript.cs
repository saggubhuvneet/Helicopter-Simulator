using UnityEngine;
using UnityEngine.UI;

public class ForgetPasswordPanelScript : MonoBehaviour
{
    [SerializeField]
    public InputField usernameInput;
    [SerializeField]
    public InputField specialKeyInput;
    [SerializeField]
    public InputField newPasswordInput;
    [SerializeField]
    public InputField confirmPasswordInput;

    [SerializeField]
    public GameObject loginPanel;
    [SerializeField]
    public GameObject createAccountPanel;
    [SerializeField]
    public GameObject forgetPasswordPanel;
    [SerializeField]
    public Text messageText;

    private string hardcodedSpecialKey = "O2I";
    public void OnForgetPasswordConfirmButtonClick()
    {
        if (PlayerPrefs.HasKey(usernameInput.text) && specialKeyInput.text == hardcodedSpecialKey)
        {
            // Show the new password fields
            newPasswordInput.gameObject.SetActive(true);
            confirmPasswordInput.gameObject.SetActive(true);
        }
        else
        {
            // Display an error message
            messageText.text = "Invalid username!";
        }
    }
    public void OnChangePasswordConfirmButtonClick()
    {
        // Check if the password and confirm password match
        if (newPasswordInput.text == confirmPasswordInput.text && specialKeyInput.text == hardcodedSpecialKey)
        {
            // Save the new password
            PlayerPrefs.SetString(usernameInput.text, newPasswordInput.text);
            messageText.text = "New Passwords Created";

            // Switch back to the Login panel
            loginPanel.SetActive(true);
            createAccountPanel.SetActive(false);
            forgetPasswordPanel.SetActive(false);
        }
        if(newPasswordInput.text != confirmPasswordInput.text)
        {
            // Display an error message
            messageText.text = "Passwords do not match!";
        }
        else
        {
            messageText.text = "Spacial key is not Match!";
        }       
    }
    public void OnLogInPanelButtonClick()
    {
        // Switch to the Create Account panel
        loginPanel.SetActive(true);
        createAccountPanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }
}
