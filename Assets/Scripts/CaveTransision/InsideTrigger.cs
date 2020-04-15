using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideTrigger : MonoBehaviour
{
    private InsideChanger insideChanger;

    private void Start()
    {
        insideChanger = GameObject.Find("InsideChanger").GetComponent<InsideChanger>();
    }

    private void OnTriggerEnter(Collider other)
    {
        insideChanger.FadeInside();
    }
}
