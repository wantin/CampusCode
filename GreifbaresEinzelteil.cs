using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class GreifbaresEinzelteil : OVRGrabbable {
	
	public HashSet<GreifbaresEinzelteil> direktVerbundene = new HashSet<GreifbaresEinzelteil>();
	
	public void verbinden(GreifbaresEinzelteil anderes) {
		direktVerbundene.Add(anderes);
		anderes.GetComponent<GreifbaresEinzelteil>().direktVerbundene.Add(this);
		Debug.Log("neue Verbindung " + alleVerbundenen(new HashSet<GreifbaresEinzelteil>()).ToString());
	}

	public void trennen(GreifbaresEinzelteil anderes) {
		direktVerbundene.Remove(anderes);
		anderes.GetComponent<GreifbaresEinzelteil>().direktVerbundene.Remove(this);
		Debug.Log("getrennte Verbindung");
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
	
	// Diese Funktion braucht es vermutlich nicht mehr, da einfach GrabBegin im MehrfachGreifer 
	// mehrfach ausgelöst wird
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
