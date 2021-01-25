using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonEventHandler : MonoBehaviour
{
    private List<GameObject> gameObjects;
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = FindObjectOfType<Button>();
        button.onClick.AddListener(() => SwitchMode());
    }

    private void SwitchMode()
    {
        gameObjects = new List<GameObject>();
        gameObjects.AddRange(FindObjectsOfType<GameObject>());
        gameObjects.RemoveAt(0);
       
        foreach (var item in gameObjects)
        {
            var meshRenderers = new List<MeshRenderer>();
            meshRenderers.AddRange(item.GetComponentsInChildren<MeshRenderer>());

            foreach (var renderer in meshRenderers)
            {
                renderer.enabled = !renderer.enabled;
            }
        }
    }
}
