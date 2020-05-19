using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    //When the mouse hovers over the GameObject, it turns to this color (orange)
    public Color m_MouseOverColor = new Color(1f, 0.64f, 0f, 0.8f);

    //This stores the GameObject’s original color
    Color m_OriginalColor;

    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    MeshRenderer m_Renderer;

    // The material we want to highlight, our tiles usually have a base and a second element...
    Material highlightMaterial;

    void Start()
    {
        //Fetch the mesh renderer component from the GameObject
        m_Renderer = GetComponent<MeshRenderer>();
       
        // the "top" of our tile is always the second item in the material array...
        highlightMaterial = m_Renderer.materials[1];

        //Fetch the original color of the GameObject
        m_OriginalColor = highlightMaterial.color;

    }

    void OnMouseOver()
    {
        // Change the color of the GameObject to red when the mouse is over GameObject
        highlightMaterial.color = m_MouseOverColor;
    }

    void OnMouseExit()
    {
        // Reset the color of the GameObject back to normal
        highlightMaterial.color = m_OriginalColor;
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
