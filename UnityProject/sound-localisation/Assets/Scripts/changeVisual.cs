using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;

public class ChangeVisual : MonoBehaviour
{

    public InputActionReference leftPrimaryButton;
    public TMP_Text visualizationText;
    public  List<GameObject> visuals;
    public int visualCounter = 0;
    public bool allowChange = true;


    private void Awake()
    {
        allowChange = true;
        leftPrimaryButton.action.Enable();
        leftPrimaryButton.action.performed += OnButtonPress;
        SetVisual(visualCounter);
    }


    private void OnDestroy()
    {
        leftPrimaryButton.action.performed -= OnButtonPress;
        leftPrimaryButton.action.Disable();
    }


    public void OnButtonPress(InputAction.CallbackContext context)
    {
        if (!allowChange)
        {
            return;
        }
        visualCounter ++;
        if(visualCounter > 5)
        {
            visualCounter = 0;
        }
        SetVisual(visualCounter);
    }




    private void SetVisual(int index)
    {
        foreach (var v in visuals)
        {
            v.SetActive(false);
        }
        if(index < 3)
        {

            visuals[index].SetActive(true);
        }
        else if (index == 3)
        {
            visuals[0].SetActive(true);
            visuals[1].SetActive(true);
        }
        else if (index == 4)
        {
            visuals[0].SetActive(true);
            visuals[2].SetActive(true);
        }
        else if (index == 5)
        {
            visuals[1].SetActive(true);
            visuals[2].SetActive(true);
        }
        if (visualizationText != null)
        {
            visualizationText.text = $"Visulization {index} Selected"; 
        }
           
    }
}
