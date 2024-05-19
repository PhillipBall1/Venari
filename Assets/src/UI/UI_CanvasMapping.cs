using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UI_CanvasMapping : MonoBehaviour
{
    public GameObject canvas;

    private void Start()
    {
        if(canvas.name == "World") transform.GetComponent<CanvasGroup>().alpha = 1.0f;
    }
}
