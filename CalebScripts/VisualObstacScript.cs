using UnityEngine;
using System.Collections;

public class VisualObstacScript : MonoBehaviour {

	public GameObject trackedObstac;

	private Vector3 offset;

	// Use this for initialization
	void Start () {
		offset = transform.position - trackedObstac.transform.position;
	}

	// Update is called once per frame
	void LateUpdate () {
		transform.position = trackedObstac.transform.position + offset;
	}
}