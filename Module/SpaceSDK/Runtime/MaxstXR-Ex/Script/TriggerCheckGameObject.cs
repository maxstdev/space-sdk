using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCheckGameObject : MonoBehaviour
{
	public LayerMask mask;

	private void OnTriggerEnter(Collider other)
	{
		//Debug.Log("OnTriggerEnter");
		if (mask.IsValid(other.gameObject.layer))
		{
			var r = other.gameObject.GetComponent<MeshRenderer>();
			if (r != null) r.enabled = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		//Debug.Log("OnTriggerExit");
		if (mask.IsValid(other.gameObject.layer))
		{
			var r = other.gameObject.GetComponent<MeshRenderer>();
			if (r != null) r.enabled = false;
		}
	}
}
