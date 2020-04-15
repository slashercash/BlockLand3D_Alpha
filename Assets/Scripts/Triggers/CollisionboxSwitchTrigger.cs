using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionboxSwitchTrigger : MonoBehaviour
{
    // Dieser Skript wird auf einen Trigger Gesetzt. Läuft der Spieler in den Trigger, Deaktiviert er das ParentObjekt des Triggers.

    private GameObject collisionboxSwitch;
    public string objectTagToTrigger;

	void Start ()
    {
        collisionboxSwitch = this.transform.parent.gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == objectTagToTrigger)
            collisionboxSwitch.SetActive(false);
    }
}
