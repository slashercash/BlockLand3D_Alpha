using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset;
    private Vector3 originalOffset;
    private Vector3 relativePos;
    private bool fastTransitionBool;

    void Start()
    {
        offset = transform.position - player.transform.position;
        originalOffset = offset;
    }

    void LateUpdate ()
    {
        if(!fastTransitionBool)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position + offset, Time.deltaTime * 10);
        }
        relativePos = player.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(relativePos);
	}

    public void newOffset(Vector3 newOffset)
    {
        offset += newOffset;
    }

    public void resetOffset()
    {
        offset = originalOffset;
    }

    public void fastTransition()
    {
        fastTransitionBool = true;
    }

    public void startFastTransision()
    {
        if(fastTransitionBool)
        {
            transform.position = transform.position = player.transform.position + offset;
            fastTransitionBool = false;
        }
    }
}