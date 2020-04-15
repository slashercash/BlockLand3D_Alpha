using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentWall : MonoBehaviour {

    public Animator animator;
    private MeshRenderer meshRenderer;
    public string lineCoverObject;
    public bool isInside;

    void Start()
    {
        meshRenderer = GameObject.Find(lineCoverObject).GetComponent<MeshRenderer>();
    }

    public void transision()
    {
        if (isInside)
        {
            isInside = false;
            animator.SetBool("IsTransparent", false);
        }
        else
        {
            isInside = true;
            animator.SetBool("IsTransparent", true);
            LineCoverOff();
        }
    }

    // Wird aufgerufen sobald das Objekt nicht mehr transparent ist
    public void LineCoverOn()
    {
        meshRenderer.enabled = true;
    }

    public void LineCoverOff()
    {
        meshRenderer.enabled = false;
    }
}
