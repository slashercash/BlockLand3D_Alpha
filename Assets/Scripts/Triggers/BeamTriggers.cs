using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamTriggers : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        transform.gameObject.SetActive(false);
    }
}
