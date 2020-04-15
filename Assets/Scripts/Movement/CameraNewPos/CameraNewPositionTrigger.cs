using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNewPositionTrigger : MonoBehaviour
{
    private CameraNewPosition cameraNewPosition;
    public string cameraPositionParent;
    public bool fastTransition = false;

	void Start ()
    {
        cameraNewPosition = GameObject.Find(cameraPositionParent).GetComponent<CameraNewPosition>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
            cameraNewPosition.changeCam(fastTransition);
    }
}
