using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class SampleUnit : Unit
{
	public Color LeadingColor;
	public Color FinishedColor;
	public Color FriendlyColor;
	public Color SelectedColor;
	public Color ReachableEnColor;
	public bool selected = false;
	public GameObject VisualObj;
	public int UnitType;
	public Transform CellsParent;

    public override void Initialize()
    {
        base.Initialize();
        transform.position += new Vector3(0, 0, -1);
		VisualObj.GetComponent<Renderer>().material.color = LeadingColor;

		//**
		if (PlayerNumber == 1 && overwatchPattern == null) {
			List<Transform> cells = new List<Transform> (); //populate cell list
			foreach (Transform cell in CellsParent) {
				cells.Add (cell);
			}
			
			overwatchPattern = new bool [,] {
				{ true, true, true, true, true, true },
				{ false, true, true, true, true, true },
				{ true, true, true, true, true, true },
			};
			if (UnitType == 1) {
				overwatchPattern = new bool [,] {
					{ false, false, false, false, false, false },
					{ false, true, true, true, true, true },
					{ false, false, false, false, false, false },
				};
			}
			if (UnitType == 2) {
				overwatchPattern = new bool [,] {
					{ false, false, false, true, false, false },
					{ false, false, true, true, true, false },
					{ false, false, false, true, false, false },
				};
			}
			if (UnitType == 3) {
				overwatchPattern = new bool [,] {
					{ true, true, false, false, false, false },
					{ false, true, false, false, false, false },
					{ true, true, false, false, false, false },
				};
			}
			for (int i = 0; i < overwatchPattern.GetLength (0); i++) {
				for (int j = 0; j < overwatchPattern.GetLength (1); j++) {
					if (overwatchPattern [i, j]) {
						//List<Transform> oCells = cells.FindAll (c => c.transform.position.x == transform.position.x + (i-1) && c.transform.position.y == transform.position.y + j);
						List<Transform> oCells = cells;
						if (GetComponent<Unit> ().Facing == 0f) { //UP
							oCells = cells.FindAll (c => c.transform.position.x == GetComponent<Unit> ().transform.position.x + (i - 1) && c.transform.position.y == GetComponent<Unit> ().transform.position.y + j);
						} else if (GetComponent<Unit> ().Facing == 180f) { //DOWN
							oCells = cells.FindAll (c => c.transform.position.x == GetComponent<Unit> ().transform.position.x + (i - 1) && c.transform.position.y == GetComponent<Unit> ().transform.position.y - j);
						} else if (GetComponent<Unit> ().Facing == 90f) { //LEFT
							oCells = cells.FindAll (c => c.transform.position.x == GetComponent<Unit> ().transform.position.x + j && c.transform.position.y == GetComponent<Unit> ().transform.position.y + (i - 1));
						} else if (GetComponent<Unit> ().Facing == 270f) { //RIGHT
							oCells = cells.FindAll (c => c.transform.position.x == GetComponent<Unit> ().transform.position.x - j && c.transform.position.y == GetComponent<Unit> ().transform.position.y + (i - 1));
						} else {
							Debug.Break ();
						}
						foreach (Transform oCell in oCells) {
							if (!oCell.GetComponent<Cell> ().WatchedBy.Contains (this)) {
								oCell.GetComponent<Cell> ().WatchedBy.Add (this);
							}
						}
					}
				}
			}
		}
		//**
    }
		
    public override void MarkAsAttacking(Unit other)
    {      
    }

    public override void MarkAsDefending(Unit other)
    {       
    }

    public override void MarkAsDestroyed()
    {      
		transform.position += new Vector3(0, 0, 1000);
    }

    public override void MarkAsFinished()
    {
		VisualObj.GetComponent<Renderer>().material.color = FinishedColor;
		selected = false;
    }

    public override void MarkAsFriendly()
    {
		VisualObj.GetComponent<Renderer>().material.color = FriendlyColor;
    }

    public override void MarkAsReachableEnemy()
    {
		VisualObj.GetComponent<Renderer>().material.color = ReachableEnColor;
    }

    public override void MarkAsSelected()
    {
		VisualObj.GetComponent<Renderer>().material.color = SelectedColor;
		selected = true;
    }

    public override void UnMark()
    {
		VisualObj.GetComponent<Renderer>().material.color = LeadingColor;
		selected = false;
    }

	public override void OnUnitDeselected()
	{
		base.OnUnitDeselected ();
		selected = false;
	}

	public override void ShowOverwatchPattern()
	{
		List<SampleSquare> SSs = new List<SampleSquare>();
		foreach (Transform c in CellsParent) {
			if (c.GetComponent<SampleSquare> ().WatchedBy.Contains(this)) {
				c.GetComponent<SampleSquare> ().MarkAsOverwatch();
				SSs.Add (c.GetComponent<SampleSquare> ());
			}
		}
		StartCoroutine(ExecuteAfterTime(1));
	}

	public IEnumerator ExecuteAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		foreach (Transform c in CellsParent) {
			//if (c.GetComponent<SampleSquare> ().WatchedBy.Contains(this)) {
				c.GetComponent<SampleSquare> ().UnMark();
		//	}
		}
	}
}
