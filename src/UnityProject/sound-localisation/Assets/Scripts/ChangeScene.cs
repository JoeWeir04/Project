using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class ChangeScene : MonoBehaviour
{
    public InputActionReference buttonA;
    public InputActionReference buttonB;
    public string sceneToLoad = "PassthroughScene";
    public float holdDuration = 1.5f;
    private float holdTimer = 0f;
    private bool loading = false;
    public TMP_Text logtext;


    void OnEnable()
    {
        buttonA.action.Enable();
        buttonB.action.Enable();
    }


    void OnDisable()
    {
        buttonA.action.Disable();
        buttonB.action.Disable();
    }


    void Update()
    {
        bool aPressed = buttonA.action.IsPressed();
        bool bPressed = buttonB.action.IsPressed();

        if (aPressed && bPressed && !loading)
        {
            logtext.text = "Both buttons pressed";
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration)
            {
                loading = true;
                SceneManager.LoadScene(sceneToLoad);
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }
}
