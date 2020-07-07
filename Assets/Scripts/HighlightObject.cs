using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    List<bool> edgesActiveBools;

    Tile tile;

    void Start()
    {
        tile = GetComponent<Tile>();
        edgesActiveBools = new List<bool>();
    }

    void OnMouseOver()
    {
        foreach(GameObject edge in tile.edges)
        {
            edgesActiveBools.Add(edge.activeSelf);
            edge.active = true;
        }
    }

    void OnMouseExit()
    {
        int i = 0;
        foreach(GameObject edge in tile.edges)
        {
            edge.active = edgesActiveBools[i];
            i += 1;
        }
        edgesActiveBools.Clear();
    }

    public void timedHighlight()
    {
        StartCoroutine(deHighlighhtWaiter());   
    }

    IEnumerator deHighlighhtWaiter()
    {
        OnMouseOver();
        yield return new WaitForSeconds(2);
        OnMouseExit();
    }
}
