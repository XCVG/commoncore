using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tack this onto a GameObject with an InputField to get a sane "on submit" event that gets fired when the user hits enter with the field selected
/// </summary>
[RequireComponent(typeof(InputField))]
public class InputFieldSubmitTackon : MonoBehaviour
{
    public InputField.SubmitEvent OnSubmit;

    private void Start()
    {
        GetComponent<InputField>().onEndEdit.AddListener(HandleInputFieldEnter);
    }

    private void HandleInputFieldEnter(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            OnSubmit.Invoke(text);
    }

}
