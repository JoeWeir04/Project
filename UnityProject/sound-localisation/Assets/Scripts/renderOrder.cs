using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class renderOrder : MonoBehaviour
    
{
    public Renderer arrowRenderer;
    public Renderer coneRenderer;
    void Start()
    {
    if (arrowRenderer != null)
        arrowRenderer.material.renderQueue = 3010;

    if (coneRenderer != null)
        coneRenderer.material.renderQueue = 3000;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
