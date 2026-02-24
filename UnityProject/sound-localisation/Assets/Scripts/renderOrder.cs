using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class renderOrder : MonoBehaviour
    
{
    public Renderer[] arrowRenderers;
    public Renderer coneRenderer;
    void Start()
    {
    foreach (Renderer r in arrowRenderers)
        {
            if (r != null)
                r.material.renderQueue = 3010;
        }

    if (coneRenderer != null)
        coneRenderer.material.renderQueue = 3000;
    }
}
