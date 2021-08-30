using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour {
    private Collider DieserCollider;
    private Collider AndererCollider;
    [SerializeField] private Rigidbody Spielzeug;

    private void Start() {
        DieserCollider = GetComponent<Collider>();
        if (AndererCollider != null) {
            Debug.Log("da war schon ein Anderer collider?!");
        }
        AndererCollider = null;
        if (Spielzeug == null) Spielzeug = GetComponentInParent<Rigidbody>();
        
    }

    private void OnTriggerEnter(Collider other) {

        if (other.tag == this.tag && tag != "Untagged") {
            Debug.Log("beide sind " + tag);
            AndererCollider = other;
            Andocken(AndererCollider.GetComponentInParent<Rigidbody>());
        }
    }

    private void OnTriggerExit(Collider other) {
        AndererCollider = null;
    }

    private void FixedUpdate() {
        
        //Aufeinanderbewegen
        if (AndererCollider != null) {
            Vector3 Dorthin = AndererCollider.bounds.center - DieserCollider.bounds.center;
            // Die Kraft wird stärker, je näher sie sind. Zu der Länge etwas hinzuaddiert, damit nicht
            // durch 0 geteilt wird
            Spielzeug.AddForce(Dorthin * (10f / (Dorthin.sqrMagnitude + .01f)));
        }
    }

    private void Andocken(Rigidbody Anderer) {
        
        if (Anderer!=null){
            Spielzeug.gameObject.AddComponent<FixedJoint>();
            Spielzeug.gameObject.GetComponent<FixedJoint>().connectedBody=Anderer;
        }
    }
}
