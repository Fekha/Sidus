using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSortingOrder : MonoBehaviour
{
    public int sortingOrder = 1; // Order in layer for Sprite Renderer
    public string sortingLayerName = "UI"; // Sorting Layer Name

    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = sortingLayerName;
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }
}