using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oculus.Platform;
using Unity.Mathematics;
using UnityEngine;

public class MagnetVerbindung : MonoBehaviour {
	
	[SerializeField] private GameObject ZweiteHälfte; //Vermutlich brauche ich nur GameObject oder RigidBody
	[SerializeField] private Transform DieserVerbindungspunkt;
	[SerializeField] private Transform ZweiterVerbindungspunkt;
	// Die Verwendung von Zusätzlichen Transforms ist notwendig, da manche Teile mehrere Verbindungen haben
	// und die Transforms auf der Verbindenden Ebene sein sollen
	[SerializeField] private float Reichweite = 10f;
	private Rigidbody DieserKörper;
	private Rigidbody ZweiterKörper;
	private bool Fixiert;
	private FixedJoint Fixierung;
	
	private void Awake() {
		DieserKörper = GetComponent<Rigidbody>();
		ZweiterKörper = ZweiteHälfte.GetComponent<Rigidbody>();
		Fixiert = false;
		if (!DieserVerbindungspunkt) DieserVerbindungspunkt = transform;
		if (!ZweiterVerbindungspunkt) ZweiterVerbindungspunkt = ZweiteHälfte.transform;
	}

	private void Update() {
		float distanz = Vector3.Distance(DieserVerbindungspunkt.position, ZweiterVerbindungspunkt.position);
		if (!Fixiert) {
			if (distanz <= Reichweite) {
				if (distanz < 0.01f) Fixieren();
				else Annähern();
			}
		}
		else if (distanz > Reichweite/4) {
			Lösen();
		}
	}

	/*private void OnCollisionEnter(Collision other) {
		if (ZweiteHälfte.GetComponents<Collider>().Contains(other.collider)) {
			Debug.Log("Collision!!!");
			DieserKörper.velocity = new Vector3(0f, 0f, 0f);	//DieserKörper.velocity + ZweiterKörper.velocity;
			ZweiterKörper.velocity = new Vector3(0f, 0f, 0f);	//DieserKörper.velocity + ZweiterKörper.velocity;
		}
	}*/

	private void Annähern() {
		
		DieserKörper.isKinematic = true;
		ZweiterKörper.isKinematic = true;
		
		Vector3 dieseVerschiebung = DieserKörper.position - DieserVerbindungspunkt.position;
		Vector3 zweiteVerschiebung = ZweiterKörper.position - ZweiterVerbindungspunkt.position;
		Vector3 zielPunkt = (DieserVerbindungspunkt.position + ZweiterVerbindungspunkt.position) / 2;
		Quaternion zielRotation = Quaternion.Lerp(DieserKörper.rotation, ZweiterKörper.rotation, 0.5f);
		if (!ZweiteHälfte.GetComponent<GreifbaresEinzelteil>().isGrabbed) {
			//ZweiterKörper.MovePosition(zielPunkt - dieseVerschiebung);
			//ZweiterKörper.MoveRotation(zielRotation);
			ZweiterKörper.position = zielPunkt - dieseVerschiebung;
			ZweiterKörper.rotation = zielRotation;
		}
		if (!gameObject.GetComponent<GreifbaresEinzelteil>().isGrabbed) {
			//DieserKörper.MovePosition(zielPunkt - zweiteVerschiebung);
			//DieserKörper.MoveRotation(zielRotation);
			DieserKörper.position = zielPunkt - zweiteVerschiebung;
			DieserKörper.rotation = zielRotation;
		}

	}

	private void Fixieren() { //TODO: gucken, ob das hier okay ist oder wie weiter
		
		Vector3 dieseVerschiebung = DieserKörper.position - DieserVerbindungspunkt.position;
		Vector3 zweiteVerschiebung = ZweiterKörper.position - ZweiterVerbindungspunkt.position;
		Vector3 zielPunkt;
		Quaternion zielRotation;

		if (ZweiteHälfte.GetComponent<GreifbaresEinzelteil>().isGrabbed) {
			if (gameObject.GetComponent<GreifbaresEinzelteil>().isGrabbed) { //keiner ist frei
				Debug.Log("Beide Teile sind gegriffen");
				return;
			}
			else { //nur dieser ist frei
				zielPunkt = DieserVerbindungspunkt.position;
				zielRotation = DieserKörper.rotation;
			}
		}
		else {
			if (gameObject.GetComponent<GreifbaresEinzelteil>().isGrabbed) { //nur der zweite ist frei
				zielPunkt = ZweiterVerbindungspunkt.position;
				zielRotation = ZweiterKörper.rotation;
			}
			else { //beide sind frei
				zielPunkt = (DieserVerbindungspunkt.position + ZweiterVerbindungspunkt.position) / 2;
				zielRotation = Quaternion.Lerp(DieserKörper.rotation, ZweiterKörper.rotation, 0.5f);
			}
		}

		ZweiterKörper.position = zielPunkt - dieseVerschiebung;
		ZweiterKörper.rotation = zielRotation;

		DieserKörper.position = zielPunkt - zweiteVerschiebung;
		DieserKörper.rotation = zielRotation;

		//Fixierung = gameObject.AddComponent<FixedJoint>();
		//Fixierung.connectedBody = ZweiterKörper;
		Fixiert = true;
		Debug.Log("fixiert!!!");
		if (!ZweiteHälfte.GetComponent<GreifbaresEinzelteil>().isGrabbed) {
			ZweiterKörper.isKinematic = false;
		}

		if (!gameObject.GetComponent<GreifbaresEinzelteil>().isGrabbed) {
			DieserKörper.isKinematic = false;
		}
		Debug.Log("ist da ein Einzelteil? " + ZweiteHälfte.GetComponent<GreifbaresEinzelteil>());
		gameObject.GetComponent<GreifbaresEinzelteil>().verbinden(ZweiteHälfte.GetComponent<GreifbaresEinzelteil>());
	}

	private void Lösen() {
		Fixiert = false;
		//Fixierung.breakForce = 0f;
		if (!ZweiteHälfte.GetComponent<GreifbaresEinzelteil>().isGrabbed) {
			ZweiterKörper.isKinematic = false;
		}
		if (!gameObject.GetComponent<GreifbaresEinzelteil>().isGrabbed) {
			DieserKörper.isKinematic = false;
		}
		gameObject.GetComponent<GreifbaresEinzelteil>().trennen(ZweiteHälfte.GetComponent<GreifbaresEinzelteil>());
	}
}
