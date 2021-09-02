using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MehrfachGreifer : OVRGrabber {
	protected Dictionary<GreifbaresEinzelteil, Tuple<Quaternion, Vector3>> IndirektGegriffene;

	protected override void Awake() {
		base.Awake();
		IndirektGegriffene = new Dictionary<GreifbaresEinzelteil, Tuple<Quaternion, Vector3>>();
	}

	protected override void MoveGrabbedObject(Vector3 pos, Quaternion rot, bool forceTeleport = false) {

		if (m_grabbedObj == null) return;
		base.MoveGrabbedObject(pos, rot, forceTeleport);

		if (!(m_grabbedObj is GreifbaresEinzelteil)) return;

		foreach (var k in IndirektGegriffene) {
			Rigidbody grabbedRigidbody = k.Key.grabbedRigidbody;
			Vector3 grabbablePosition = pos + rot * k.Value.Item2;
			Quaternion grabbableRotation = rot * k.Value.Item1;

			if (forceTeleport) {
				grabbedRigidbody.transform.position = grabbablePosition;
				grabbedRigidbody.transform.rotation = grabbableRotation;
			}
			else {
				grabbedRigidbody.MovePosition(grabbablePosition);
				grabbedRigidbody.MoveRotation(grabbableRotation);
			}
		}
	}

	//TODO brauche ich eine version von GrabVolumeEnable?


	public override void OffhandGrabbed(OVRGrabbable grabbable) {
		if (grabbable == m_grabbedObj) {
			GrabbableRelease(Vector3.zero, Vector3.zero);
			foreach (var k in IndirektGegriffene) {
				k.Key.GrabEnd(Vector3.zero, Vector3.zero);
				if (m_parentHeldObject) k.Key.transform.parent = null;
			}

			IndirektGegriffene = new Dictionary<GreifbaresEinzelteil, Tuple<Quaternion, Vector3>>();
		}
		else if (grabbable is GreifbaresEinzelteil && grabbable.isGrabbed) {
			if (IndirektGegriffene.ContainsKey((GreifbaresEinzelteil) grabbable)) {
				grabbable.GrabEnd(Vector3.zero, Vector3.zero);
				if (m_parentHeldObject) grabbable.transform.parent = null;
				IndirektGegriffene.Remove((GreifbaresEinzelteil) grabbable);
			}
		}
	}

	//TODO closestGrabbableCollider muss für zusätzliche geändert werden vielleicht


	protected override void GrabBegin() {
		base.GrabBegin();
		if (m_grabbedObj is GreifbaresEinzelteil) {
			GreifbaresEinzelteil gegriffenes = (GreifbaresEinzelteil) m_grabbedObj;
			HashSet<GreifbaresEinzelteil> verbundene =
				gegriffenes.alleVerbundenen(new HashSet<GreifbaresEinzelteil>());
			verbundene.Remove(gegriffenes);

			//TODO hier entscheiden, welches gegriffen wird, falls schon gegriffen?
			
			foreach (var k in verbundene) {
				Vector3 kPosOffset;
				if (k.snapPosition) {
					kPosOffset = m_gripTransform.localPosition;
					if (k.snapOffset) {
						Vector3 snapOffset = k.snapOffset.position;
						if (m_controller == OVRInput.Controller.LTouch) snapOffset.x = -snapOffset.x;
						kPosOffset += snapOffset;
					}
				}
				else {
					Vector3 relPos = k.transform.position - transform.position;
					relPos = Quaternion.Inverse(transform.rotation) * relPos;
					kPosOffset = relPos;
				}
				// Hier ist offhandgrab für indirekte
				if (k.isGrabbed && k.GreifDistanz <= kPosOffset.magnitude) break;
				else if (k.isGrabbed) {
					//TODO drop it with the other hand
					k.GreifDistanz = kPosOffset.magnitude;
				}

				Quaternion kRotOffset;
				if (k.snapOrientation) {
					kRotOffset = m_gripTransform.localRotation;
					if (k.snapOffset) {
						kRotOffset = k.snapOffset.rotation * kRotOffset;
					}
				}
				else {
					Quaternion relOri = Quaternion.Inverse(transform.rotation) * k.transform.rotation;
					kRotOffset = relOri;
				}

				IndirektGegriffene.Add(k, new Tuple<Quaternion, Vector3>(kRotOffset, kPosOffset));
				k.GrabBegin(this, k.GetComponentInChildren<Collider>());

				SetPlayerIgnoreCollision(k.gameObject, true);
				if (m_parentHeldObject) m_grabbedObj.transform.parent = transform;
			}

			MoveGrabbedObject(m_lastPos, m_lastRot, true);
		}
	}

	// Ich brauche linearVelocity und angularVelocity aus der base function, weswegen ich sie hier
	// explizit hinkopiert habe statt base.GrabEnd(); aufzurufen
	// ich könnte statt das hier so groß zu machen auch Grabbable Release überschreiben, was die
	// OVRGrabber Klasse aber nicht vorsieht. Das würde dann auch OffhandGrabbed beeinflussen
	protected override void GrabEnd() {
		if (m_grabbedObj != null) {
			OVRPose localPose = new OVRPose {
				position = OVRInput.GetLocalControllerPosition(m_controller),
				orientation = OVRInput.GetLocalControllerRotation(m_controller)
			};
			OVRPose offsetPose = new OVRPose {
				position = m_anchorOffsetPosition,
				orientation = m_anchorOffsetRotation
			};
			localPose = localPose * offsetPose;

			OVRPose trackingSpace = transform.ToOVRPose() * localPose.Inverse();
			Vector3 linearVelocity =
				trackingSpace.orientation * OVRInput.GetLocalControllerVelocity(m_controller);
			Vector3 angularVelocity =
				trackingSpace.orientation * OVRInput.GetLocalControllerAngularVelocity(m_controller);

			GrabbableRelease(linearVelocity, angularVelocity);

			foreach (var k in IndirektGegriffene) {
				k.Key.GrabEnd(linearVelocity, angularVelocity);
				if (m_parentHeldObject) k.Key.transform.parent = null;
			}

			IndirektGegriffene = new Dictionary<GreifbaresEinzelteil, Tuple<Quaternion, Vector3>>();
		}
	}
}
