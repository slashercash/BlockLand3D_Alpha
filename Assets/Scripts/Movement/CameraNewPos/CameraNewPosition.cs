using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNewPosition : MonoBehaviour
{
    private CameraMovement cameraMovement;
    public Vector3 newOffset;
    private bool camChanged = false;

	void Start ()
    {
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
    }

    public void changeCam(bool fastTransission)
    {
        if(camChanged)
        {
            cameraMovement.resetOffset();
            camChanged = false;
        }
        else
        {
            cameraMovement.newOffset(newOffset);
            camChanged = true;
        }

        if(fastTransission)
            cameraMovement.fastTransition();

    }
}
