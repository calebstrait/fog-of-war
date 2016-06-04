using UnityEngine;
using System.Collections;

public class FogScript : MonoBehaviour {

	public GameObject prefab;
	public CellGrid CellGrid;
	public Transform UnitsParent;
	public float fogdist = 3.2f;
	public bool fogOn;
	public bool wasSeen;
	private GameObject gridTopper;

	void Start () {
		wasSeen = false;
		fogOn = true;
		Vector3 shift = new Vector3 (-.18f, .088f, -.45f);
		gridTopper = Instantiate(prefab, transform.position + shift, Quaternion.identity) as GameObject;
	}

	void LateUpdate () {
		fogOn = true;
		foreach (Transform unit in UnitsParent) {
			Vector3 diff = transform.position - unit.transform.position;
			if (diff.magnitude < fogdist && unit.gameObject.GetComponent<Unit> ().PlayerNumber == 0) {//CellGrid.CurrentPlayerNumber) {
				fogOn = false;
				wasSeen = true;
			}
		}
		if (fogOn && CellGrid.CurrentPlayerNumber == 0) {
			GetComponent<Renderer> ().enabled = true;
			gridTopper.SetActive(true);
			if (wasSeen) {
				GetComponent<Renderer> ().material.color = new Color (0.0f, 0.0f, 0.0f, 0.5f);
				gridTopper.SetActive(false);
			} else {
				GetComponent<Renderer> ().material.color = new Color (0.0f, 0.0f, 0.0f, 1.0f);
			}
		} else {
			GetComponent<Renderer> ().enabled = false;
			gridTopper.SetActive(false);
			if (CellGrid.CurrentPlayerNumber == 0) {
				wasSeen = true;
			}
		}
	}
}
