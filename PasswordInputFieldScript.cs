using UnityEngine;
using UnityEngine.UI;

public class PasswordInputFieldScript : MonoBehaviour
{
    public InputField passwordInputField;
    public Button showPasswordButton;
    public char passwordChar = '*';

    private bool shouldHideMobileInput = true;

    private void Awake()
    {
        // Set the input type to password
        passwordInputField.inputType = InputField.InputType.Password;

        // Add an event listener for the button click
        showPasswordButton.onClick.AddListener(OnShowPasswordButtonClick);
    }

    private void OnShowPasswordButtonClick()
    {
        // Toggle the "Should Hide Mobile Input" setting
        shouldHideMobileInput = !shouldHideMobileInput;
        passwordInputField.shouldHideMobileInput = shouldHideMobileInput;

        // Update the text display
        UpdateTextDisplay();
    }

    private void UpdateTextDisplay()
    {
        // Update the text display to show the password or asterisks
        if (shouldHideMobileInput)
        {
            passwordInputField.text = new string(passwordChar, passwordInputField.text.Length);
        }
        else
        {
            passwordInputField.text = passwordInputField.text;
        }
    }
}
