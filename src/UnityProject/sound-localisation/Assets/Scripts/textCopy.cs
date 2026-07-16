using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
public class textCopy : MonoBehaviour
{
    public TMP_Text ownExText;
    public TMP_Text ownPidText;
    public TMP_Text toCopyExText;
    public TMP_Text toCopyPidText;
 
    void Start()
    {
        
    }


    void Update()
    {
        ownExText.text = toCopyExText.text;
        ownExText.fontSize = toCopyExText.fontSize;
        ownPidText.text = toCopyPidText.text;
        ownPidText.fontSize = toCopyPidText.fontSize;
    }
}
