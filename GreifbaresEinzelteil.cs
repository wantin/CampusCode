using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class GreifbaresEinzelteil : OVRGrabbable {
	
	public HashSet<GreifbaresEinzelteil> direktVerbundene = new HashSet<GreifbaresEinzelteil>();
	// GreifDistanz kleiner 0 um zu zeigen, dass der Wert derzeit nicht relevant ist
	// float ist nicht nullable
	public float GreifDistanz = -1;
	
	public void verbinden(GreifbaresEinzelteil anderes) {
		direktVerbundene.Add(anderes);
		anderes.GetComponent<GreifbaresEinzelteil>().direktVerbundene.Add(this);
		Debug.Log("neue Verbindung " + alleVerbundenen(new HashSet<GreifbaresEinzelteil>()).ToString());
	}

	public void trennen(GreifbaresEinzelteil anderes) {
		direktVerbundene.Remove(anderes);
		anderes.GetComponent<GreifbaresEinzelteil>().direktVerbundene.Remove(this);
	}
		
	//Diese rekursive Funktion muss initial mit einem leeren Hashset aufgerufen werden 
	public HashSet<GreifbaresEinzelteil> alleVerbundenen(HashSet<GreifbaresEinzelteil> schonGefundene) {
		
		foreach (GreifbaresEinzelteil k in direktVerbundene) {
			if (!schonGefundene.Contains(k)) {
				schonGefundene.Add(k);
				k.alleVerbundenen(schonGefundene);
			}
		}

		return schonGefundene;
	}

	// mache ich jetzt in dem MehrfachGreifer mit kOffsetPosition.
	//TODO: verhält es sich gut, oder sollte ich das eher wie hier versuchen?
//	public override void GrabBegin(OVRGrabber hand, Collider grabPoint) {
//		base.GrabBegin(hand, grabPoint);
//		GreifDistanz = (hand.GetComponent<Rigidbody>().position 
//		                - GetComponent<Rigidbody>().position).magnitude;
//	}

	public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity) {
		base.GrabEnd(linearVelocity, angularVelocity);
		GreifDistanz = -1;
	}

//	 Diese Funktion braucht es vermutlich nicht mehr, da einfach GrabBegin im MehrfachGreifer 
//	 mehrfach ausgelöst wird
//	public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
//	{
//		m_grabbedBy = hand;
//		m_grabbedCollider = grabPoint;
//		gameObject.GetComponent<Rigidbody>().isKinematic = true;
//		
//		//da diese Funktion selbst rekursiv durch die Verbindungen geht reichen hier die direkten
//		foreach (GreifbaresEinzelteil k in direktVerbundene) { 
//			if (k != this) {
//				if (!k.isGrabbed) {
//					k.GrabBegin(hand, k.GetComponentInChildren<Collider>()); //reicht dieser Collider, oder brauche ich den nächsten?
//				}
//			}
//		}
//	}
	
}
