using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class VisualScript : MonoBehaviour {

	public GameObject trackedUnit;
	public CellGrid CellGrid;
	public Transform FogParent;
	public float Facing;

	private Vector3 posOffset;
	private Vector3 rotOffset;

	// Use this for initialization
	void Start () {
		posOffset = transform.position - trackedUnit.transform.position;
		rotOffset = (transform.localEulerAngles - trackedUnit.transform.localEulerAngles);// + new Vector3 (0f, -180f, 0f);
		GetComponent<Renderer> ().enabled = true;
	}

	// Update is called once per frame
	void LateUpdate () {
		if (trackedUnit.GetComponent<Unit> ().isAlive && CellGrid.CurrentPlayerNumber == 1 && trackedUnit.GetComponent<Unit> ().hover) {
			bool rotating = false;
			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				trackedUnit.GetComponent<Unit> ().Facing = 270f;
				rotating = true;
			}
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				trackedUnit.GetComponent<Unit> ().Facing = 0f;
				rotating = true;
			}
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				trackedUnit.GetComponent<Unit> ().Facing = 90f;
				rotating = true;
			}
			if (Input.GetKeyDown (KeyCode.DownArrow)) {
				trackedUnit.GetComponent<Unit> ().Facing = 180f;	
				rotating = true;
			}
			if (rotating) {
				trackedUnit.transform.eulerAngles = new Vector3 (trackedUnit.transform.eulerAngles.x, trackedUnit.transform.eulerAngles.y, trackedUnit.GetComponent<Unit> ().Facing);		
				List<Transform> cells = new List<Transform> ();
				foreach (Transform cell in CellGrid.transform) {
					cells.Add (cell);
					cell.GetComponent<Cell> ().WatchedBy.Remove (trackedUnit.GetComponent<SampleUnit> ());
				}
				for (int i = 0; i < trackedUnit.GetComponent<Unit> ().overwatchPattern.GetLength (0); i++) {
					for (int j = 0; j < trackedUnit.GetComponent<Unit> ().overwatchPattern.GetLength (1); j++) {
						if (trackedUnit.GetComponent<Unit> ().overwatchPattern [i, j]) {
							List<Transform> oCells = cells;
							if (trackedUnit.GetComponent<Unit> ().Facing == 0f) { //UP
								oCells = cells.FindAll (c => c.transform.position.x == trackedUnit.GetComponent<Unit> ().transform.position.x + (i - 1) && c.transform.position.y == trackedUnit.GetComponent<Unit> ().transform.position.y + j);
							} else if (trackedUnit.GetComponent<Unit> ().Facing == 180f) { //DOWN
								oCells = cells.FindAll (c => c.transform.position.x == trackedUnit.GetComponent<Unit> ().transform.position.x + (i - 1) && c.transform.position.y == trackedUnit.GetComponent<Unit> ().transform.position.y - j);
							} else if (trackedUnit.GetComponent<Unit> ().Facing == 90f) { //LEFT
								oCells = cells.FindAll (c => c.transform.position.x == trackedUnit.GetComponent<Unit> ().transform.position.x + j && c.transform.position.y == trackedUnit.GetComponent<Unit> ().transform.position.y + (i - 1));
							} else if (trackedUnit.GetComponent<Unit> ().Facing == 270f) { //RIGHT
								oCells = cells.FindAll (c => c.transform.position.x == trackedUnit.GetComponent<Unit> ().transform.position.x - j && c.transform.position.y == trackedUnit.GetComponent<Unit> ().transform.position.y + (i - 1));
							} else {
								Debug.Log (Facing);
								Debug.Break ();
							}
							foreach (Transform oCell in oCells) {
								if (!oCell.GetComponent<Cell> ().WatchedBy.Contains (trackedUnit.GetComponent<SampleUnit> ())) {
									oCell.GetComponent<Cell> ().WatchedBy.Add (trackedUnit.GetComponent<SampleUnit> ());
								}
							}
						}
					}
				}
				trackedUnit.GetComponent<SampleUnit> ().ShowOverwatchPattern ();
			}
		}
			
		GetComponent<Renderer> ().enabled = true;
		transform.position = trackedUnit.transform.position + posOffset;
		transform.localEulerAngles = trackedUnit.transform.localEulerAngles + rotOffset;
		if (!trackedUnit.GetComponent<Unit> ().isAlive) {
			GetComponent<Renderer> ().enabled = false;
		} else if (CellGrid.CurrentPlayerNumber == 1 && trackedUnit.GetComponent<Unit> ().PlayerNumber == 0) {
			GetComponent<Renderer> ().material.color = Color.black;
		}

		//%*%*%
		if (CellGrid.CurrentPlayerNumber == 0 && trackedUnit.GetComponent<Unit> ().PlayerNumber == 1) {
			Transform closest = FogParent.GetChild (0);
			for (int i = 0; i < FogParent.childCount; i++) {
				var fo = FogParent.GetChild (i);
				var dist_fo = Math.Abs ((trackedUnit.GetComponent<Unit> ().Cell.transform.position - fo.transform.position).magnitude);
				var dist_closest = Math.Abs ((trackedUnit.GetComponent<Unit> ().Cell.transform.position - closest.position).magnitude);
				if (dist_fo < dist_closest) {
					closest = fo;
				}
			}
			if (closest.GetComponent<FogScript> ().fogOn) {
				GetComponent<Renderer> ().enabled = false;
			} else if (trackedUnit.GetComponent<Unit> ().isAlive) {
				GetComponent<Renderer> ().enabled = true;
			}
		}
		if (CellGrid.CurrentPlayerNumber == 1 && trackedUnit.GetComponent<Unit> ().PlayerNumber == 1) {
			GetComponent<Renderer> ().enabled = true;
		}
		//%&%&
	}

//	public void RotateAsMoving ()
//	{
//		transform.localEulerAngles = Vector3.RotateTowards (transform.localEulerAngles, (cell.transform.position - transform.position), Time.deltaTime, 0.0F);
//
//	}
		
}
