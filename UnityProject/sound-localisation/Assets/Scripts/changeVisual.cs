using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;

public class ChangeVisual : MonoBehaviour
{
    // Start is called before the first frame update

    public InputActionReference leftPrimaryButton;
    public TMP_Text visualizationText;
    public  List<GameObject> visuals;
    public int visualCounter = 0;

    


    private void Awake()
    {
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
        visualCounter ++;
        if(visualCounter > 3)
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
        if (index == 2)
        {
            visuals[0].SetActive(true);
            visuals[1].SetActive(true);
        }
            {
            visuals[index].SetActive(true);
            }  
        
            
        visualizationText.text = $"Visulization {index} Selected";    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
