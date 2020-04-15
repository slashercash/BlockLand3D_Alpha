using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentWallTrigger : MonoBehaviour
{
    private TransparentWall transparentWall;
    public string transparentObject;

    private void Start()
    {
        transparentWall = GameObject.Find(transparentObject).GetComponent<TransparentWall>();
    }

    private void OnTriggerEnter(Collider other)
    {
        transparentWall.transision();
    }
}
