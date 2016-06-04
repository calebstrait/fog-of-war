using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
//using UnityEditor;
using System.Linq;
using System;

public class GUIController : MonoBehaviour
{
    public CellGrid CellGrid;
	public Sprite ATK;
	public Sprite DEF;
	public Button _button;
	public Button buttonR;
	public Button buttonL;
	public Button buttonU;
	public Button buttonD;
	public Transform UnitsParent;
	public Transform UnitsVisParent;
	public Transform CellsParent;
	public Transform FogParent;
	public Transform prefabATKer;
	public Transform prefabDEFer;
	public Transform prefabATKvis;
	public Transform prefabDEFvis;
	public List<Vector3> DEFpositions = new List<Vector3> ();
	public List<Quaternion> DEFrotations = new List<Quaternion> ();
	public List<Vector3> ATKpositions = new List<Vector3> ();
	public List<Quaternion> ATKrotations = new List<Quaternion> ();
	
    void Start()
	{
		foreach (Transform unit in UnitsParent) {
			if (unit.gameObject.GetComponent<Unit> ().PlayerNumber == 1) { 
				DEFpositions.Add (unit.transform.position);
				DEFrotations.Add (unit.transform.localRotation);
			} else {
				ATKpositions.Add (unit.transform.position);
				ATKrotations.Add (unit.transform.localRotation);
			}
		}

    }

	void Update ()
    {

	}

	public void ModeSwitch ()
	{
		foreach (Transform unit in UnitsParent) {
			if (unit.gameObject.GetComponent<Unit> ().isMoving)
				return;
		}
		if (CellGrid.CurrentPlayerNumber == 0) {
			_button.image.overrideSprite = ATK;
			buttonR.gameObject.SetActive (true);
			buttonL.gameObject.SetActive (true);
			buttonU.gameObject.SetActive (true);
			buttonD.gameObject.SetActive (true);
		} else {
			_button.image.overrideSprite = DEF;
			buttonR.gameObject.SetActive (false);
			buttonL.gameObject.SetActive (false);
			buttonU.gameObject.SetActive (false);
			buttonD.gameObject.SetActive (false);
		}

		int indA = 0;
		int indD = 0;
		foreach (Transform unit in UnitsParent) {
			if (unit.gameObject.GetComponent<Unit> ().PlayerNumber == 0) {
				unit.gameObject.GetComponent<Unit> ().transform.position = ATKpositions [indA];
				unit.gameObject.GetComponent<Unit> ().transform.localRotation = ATKrotations [indA];
				indA++;
			} else {
				if (!unit.gameObject.GetComponent<Unit> ().isAlive) { 
					bool allset = false;
					while (allset == false) {
						if (indD >= DEFpositions.Count)
							indD = 0;
						var tryThisPosition = DEFpositions [indD];
						Transform closestToP = CellsParent.GetChild (0); //closest to tryThisPosition
						for (int i = 0; i < CellsParent.childCount; i++) {
							var ce = CellsParent.GetChild (i);
							var dist_ce = Math.Abs ((tryThisPosition - ce.gameObject.GetComponent<Cell> ().transform.position).magnitude);
							var dist_closest = Math.Abs ((tryThisPosition - closestToP.gameObject.GetComponent<Cell> ().transform.position).magnitude);
							if (dist_ce < dist_closest) {
								closestToP = ce;
							}
						}
						if (closestToP.gameObject.GetComponent<Cell> ().IsTaken == false) {
							unit.gameObject.GetComponent<Unit> ().transform.position = DEFpositions [indD];
							unit.gameObject.GetComponent<Unit> ().transform.localRotation = DEFrotations [indD];
							allset = true;
						}
						indD++;
					}
				}
			} 
			unit.gameObject.GetComponent<Unit> ().HitPoints = unit.gameObject.GetComponent<Unit> ().TotalHitPoints;
			unit.gameObject.GetComponent<Unit> ().Initialize ();
			unit.gameObject.GetComponent<Unit> ().transform.position += new Vector3(0, 0, 1);

			Transform closest = CellsParent.GetChild (0);
			for (int i = 0; i < CellsParent.childCount; i++) {
				var ce = CellsParent.GetChild (i);
				var dist_ce = Math.Abs ((unit.gameObject.GetComponent<Unit> ().transform.position - ce.gameObject.GetComponent<Cell> ().transform.position).magnitude);
				var dist_closest = Math.Abs ((unit.gameObject.GetComponent<Unit> ().transform.position - closest.gameObject.GetComponent<Cell> ().transform.position).magnitude);
				if (dist_ce < dist_closest) {
					closest = ce;
				}
			}
			unit.gameObject.GetComponent<Unit> ().Cell.IsTaken = false;
			unit.gameObject.GetComponent<Unit> ().Cell = closest.gameObject.GetComponent<Cell> ();
			unit.gameObject.GetComponent<Unit> ().Cell.IsTaken = true;

			//Refresh overwatch
//			Debug.Log (" ");
//			Debug.Log (unit.gameObject.GetComponent<Unit> ().name);
			//Debug.Log ("1");
			if (unit.gameObject.GetComponent<Unit> ().PlayerNumber == 1) {
				List<Transform> cells = new List<Transform> ();
				foreach (Transform cell in CellGrid.transform) {
					cells.Add (cell);
					cell.GetComponent<Cell> ().WatchedBy.Remove (unit.gameObject.GetComponent<SampleUnit> ());
				}
				//Debug.Log ("2");
				for (int i = 0; i < unit.gameObject.GetComponent<Unit> ().overwatchPattern.GetLength (0); i++) {
					//Debug.Log ("3");
					for (int j = 0; j < unit.gameObject.GetComponent<Unit> ().overwatchPattern.GetLength (1); j++) {
						//Debug.Log ("4");
						if (unit.gameObject.GetComponent<Unit> ().overwatchPattern [i, j]) {
							//Debug.Log ("5");
							List<Transform> oCells = cells;
							if (unit.gameObject.GetComponent<Unit> ().Facing == 0f) { //UP
								oCells = cells.FindAll (c => c.transform.position.x == unit.gameObject.GetComponent<Unit> ().transform.position.x + (i - 1) && c.transform.position.y == unit.gameObject.GetComponent<Unit> ().transform.position.y + j);
							} else if (unit.gameObject.GetComponent<Unit> ().Facing == 180f) { //DOWN
								oCells = cells.FindAll (c => c.transform.position.x == unit.gameObject.GetComponent<Unit> ().transform.position.x + (i - 1) && c.transform.position.y == unit.gameObject.GetComponent<Unit> ().transform.position.y - j);
							} else if (unit.gameObject.GetComponent<Unit> ().Facing == 90f) { //LEFT
								oCells = cells.FindAll (c => c.transform.position.x == unit.gameObject.GetComponent<Unit> ().transform.position.x + j && c.transform.position.y == unit.gameObject.GetComponent<Unit> ().transform.position.y + (i - 1));
							} else if (unit.gameObject.GetComponent<Unit> ().Facing == 270f) { //RIGHT
								oCells = cells.FindAll (c => c.transform.position.x == unit.gameObject.GetComponent<Unit> ().transform.position.x - j && c.transform.position.y == unit.gameObject.GetComponent<Unit> ().transform.position.y + (i - 1));
							} else {
								Debug.Log ("!!");
							}
							//Debug.Log ("oCells.Count:" + oCells.Count + " / cells.Count:" + cells.Count);
							foreach (Transform oCell in oCells) {
								//Debug.Log ("6");
								if (!oCell.GetComponent<Cell> ().WatchedBy.Contains (unit.gameObject.GetComponent<SampleUnit> ())) {
									//Debug.Log ("7");
									oCell.GetComponent<Cell> ().WatchedBy.Add (unit.gameObject.GetComponent<SampleUnit> ());
								}
							}
						}
					}
				}
			}
			//

			unit.gameObject.GetComponent<SampleUnit> ().OnUnitDeselected ();
		}
		foreach (Transform fs in FogParent) {
			fs.GetComponent<FogScript>().wasSeen = false;
		}
			
		//Debug.Log ("CellGrid.EndTurn()");
		CellGrid.EndTurn();
		CellGrid.CellGridState = new CellGridStateWaitingForInput(CellGrid);

	}

	public void ArrowButton (int facing) {
		foreach (Transform unit in UnitsParent) {
			if (unit.GetComponent<Unit> ().isAlive && CellGrid.CurrentPlayerNumber == 1 && unit.GetComponent<SampleUnit> ().selected) {
				unit.GetComponent<Unit> ().Facing = facing;
				unit.transform.eulerAngles = new Vector3 (unit.transform.eulerAngles.x, unit.transform.eulerAngles.y, unit.GetComponent<Unit> ().Facing);		
				List<Transform> cells = new List<Transform> ();
				foreach (Transform cell in CellGrid.transform) {
					cells.Add (cell);
					cell.GetComponent<Cell> ().WatchedBy.Remove (unit.GetComponent<SampleUnit> ());
				}
				for (int i = 0; i < unit.GetComponent<Unit> ().overwatchPattern.GetLength (0); i++) {
					for (int j = 0; j < unit.GetComponent<Unit> ().overwatchPattern.GetLength (1); j++) {
						if (unit.GetComponent<Unit> ().overwatchPattern [i, j]) {
							List<Transform> oCells = cells;
							if (unit.GetComponent<Unit> ().Facing == 0f) { //UP
								oCells = cells.FindAll (c => c.transform.position.x == unit.GetComponent<Unit> ().transform.position.x + (i - 1) && c.transform.position.y == unit.GetComponent<Unit> ().transform.position.y + j);
							} else if (unit.GetComponent<Unit> ().Facing == 180f) { //DOWN
								oCells = cells.FindAll (c => c.transform.position.x == unit.GetComponent<Unit> ().transform.position.x + (i - 1) && c.transform.position.y == unit.GetComponent<Unit> ().transform.position.y - j);
							} else if (unit.GetComponent<Unit> ().Facing == 90f) { //LEFT
								oCells = cells.FindAll (c => c.transform.position.x == unit.GetComponent<Unit> ().transform.position.x + j && c.transform.position.y == unit.GetComponent<Unit> ().transform.position.y + (i - 1));
							} else if (unit.GetComponent<Unit> ().Facing == 270f) { //RIGHT
								oCells = cells.FindAll (c => c.transform.position.x == unit.GetComponent<Unit> ().transform.position.x - j && c.transform.position.y == unit.GetComponent<Unit> ().transform.position.y + (i - 1));
							} else {
								Debug.Break ();
							}
							foreach (Transform oCell in oCells) {
								if (!oCell.GetComponent<Cell> ().WatchedBy.Contains (unit.GetComponent<SampleUnit> ())) {
									oCell.GetComponent<Cell> ().WatchedBy.Add (unit.GetComponent<SampleUnit> ());
								}
							}
						}
					}
				}
				unit.GetComponent<SampleUnit> ().ShowOverwatchPattern ();
			}
		}
	}

}
