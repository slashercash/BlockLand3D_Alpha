using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideChanger : MonoBehaviour {

    public Animator animator;
    private MeshRenderer meshRendererOutside1;
    private MeshRenderer meshRendererOutside2;
    private MeshRenderer meshRendererInside1;
    private MeshRenderer meshRendererInside2;
    private CameraMovement cameraMovement;
    private new Light light;
    public bool isInside = false;
    public string outsideObject1;
    public string outsideObject2;
    public string insideObject1;
    public string insideObject2;

    void Start ()
    {
        meshRendererOutside1 = GameObject.Find(outsideObject1).GetComponent<MeshRenderer>();
        meshRendererOutside2 = GameObject.Find(outsideObject2).GetComponent<MeshRenderer>();
        meshRendererInside1 = GameObject.Find(insideObject1).GetComponent<MeshRenderer>();
        meshRendererInside2 = GameObject.Find(insideObject2).GetComponent<MeshRenderer>();
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        light = GameObject.Find("Directional Light").GetComponent<Light>();
        ChangeMeshRenderer();
    }
	
    public void FadeInside()
    {
        if(isInside)
        {
            isInside = false;
        }
        else
        {
            isInside = true;
        }
        animator.SetBool("FadeOut", true);
    }

    // Wird aufgerufen wenn Fadeout animation durchlaufen ist
    private void ChangeMeshRenderer()
    {
        if (isInside)
        {
            meshRendererOutside1.enabled = false;
            meshRendererOutside2.enabled = false;
            meshRendererInside1.enabled = true;
            meshRendererInside2.enabled = true;
            light.enabled = false;
        }
        else
        {
            meshRendererOutside1.enabled = true;
            meshRendererOutside2.enabled = true;
            meshRendererInside1.enabled = false;
            meshRendererInside2.enabled = false;
            light.enabled = true;
        }
        cameraMovement.startFastTransision();
        animator.SetBool("FadeOut", false);
    }
}
