using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public List<GameObject> canvases;
    public bool isActive;

    private void Start()
    {
        foreach( var canvas in canvases)
        {
            canvas.gameObject.SetActive(true);
        }
        CloseAllCanvas();
    }

    public void ToggleOptions()
    {
        ToggleCanvas(0);
    }

    public void ToggleGraphics()
    {
        ToggleCanvas(1);
    }

    public void ToggleInput()
    {
        ToggleCanvas(2);
    }
    public void ToggleAudio()
    {
        ToggleCanvas(3);
    }


    private void ToggleCanvas(int canvasIndex)
    {
        if (canvasIndex < 0 || canvasIndex >= canvases.Count)
        {
            Debug.LogError("Canvas index out of range.");
            return;
        }

        isActive = canvases[canvasIndex].transform.GetComponent<CanvasGroup>().alpha == 1;
        for (int i = 0; i < canvases.Count; i++)
        {
            if (i == canvasIndex)
            {
                canvases[i].transform.GetComponent<CanvasGroup>().alpha = 1;
                canvases[i].transform.GetComponent<CanvasGroup>().interactable = true;
                canvases[i].transform.GetComponent<CanvasGroup>().blocksRaycasts = true;
            }
            else if (isActive)
            {
                canvases[i].transform.GetComponent<CanvasGroup>().alpha = 0;
                canvases[i].transform.GetComponent<CanvasGroup>().interactable = false;
                canvases[i].transform.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }
    }

    public void CloseAllCanvas()
    {
        foreach (GameObject go in canvases)
        {
            go.transform.GetComponent<CanvasGroup>().alpha = 0;
            go.transform.GetComponent<CanvasGroup>().interactable = false;
            go.transform.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }
}
