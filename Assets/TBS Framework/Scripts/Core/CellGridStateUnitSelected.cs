using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

class CellGridStateUnitSelected : CellGridState
{
	public Transform ObstaclesParent = GameObject.Find("Obstacles").transform;
	public Transform FogParent = GameObject.Find("FogGrid").transform;
	private Unit _unit;
    private List<Cell> _pathsInRange;
    private List<Unit> _unitsInRange;

    private Cell _unitCell;
	private MonoBehaviour _mb;

    public CellGridStateUnitSelected(CellGrid cellGrid, Unit unit) : base(cellGrid)
    {
        _unit = unit;
        _pathsInRange = new List<Cell>();
        _unitsInRange = new List<Unit>();
    }
//	public bool survivedMove;
//	public bool[,] overwatchPattern;
//	public Cell destinationCell;
    public override void OnCellClicked(Cell cell)
    {
        if (_unit.isMoving)
            return;
        if(cell.IsTaken)
        {
            _cellGrid.CellGridState = new CellGridStateWaitingForInput(_cellGrid);
            return;
        }
            
        if(!_pathsInRange.Contains(cell))
        {
            _cellGrid.CellGridState = new CellGridStateWaitingForInput(_cellGrid);
        }
        else
        {
            var path = _unit.FindPath(_cellGrid.Cells, cell);
			_unit.Move(cell,path,_cellGrid); //*%*%*%*%*
        }
    }
    public override void OnUnitClicked(Unit unit)
    {
        if (unit.Equals(_unit) || unit.isMoving)
            return;

        if (_unitsInRange.Contains(unit) && _unit.ActionPoints > 0)
        {
			_unit.DealDamage(unit,_cellGrid);
            _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, _unit);
        }

        if (unit.PlayerNumber.Equals(_unit.PlayerNumber))
        {
            _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, unit);
        }
            
    }
    public override void OnCellDeselected(Cell cell)
    {
        base.OnCellDeselected(cell);

        foreach (var _cell in _pathsInRange)
        {
//            _cell.MarkAsReachable();
			_cell.UnMark();
        }
//        foreach (var _cell in _cellGrid.Cells.Except(_pathsInRange))
//        {
//            _cell.UnMark();
//        }
    }
    public override void OnCellSelected(Cell cell)
    {
        base.OnCellSelected(cell);
//		if (!_pathsInRange.Contains (cell)) {
//			return;
//		}
		//&*&*&
//		bool containsObstacle = false;
//		for (int i = 0; i < ObstaclesParent.childCount; i++) {
//			var ob = ObstaclesParent.GetChild(i);
//			var ce = _cellGrid.Cells.OrderBy(h => Math.Abs((h.transform.position - ob.transform.position).magnitude)).First();
//			Debug.Log ("---");
//			Debug.Log (cell.transform.position);
//			Debug.Log (ce.transform.position);
//			if (cell.transform.position == ce.transform.position) {
//				containsObstacle = true;
//			}
//		}
		bool closestFogIsOn = false;
		Transform closest = FogParent.GetChild(0);
		for (int i = 0; i < FogParent.childCount; i++) {
			
			var fo = FogParent.GetChild(i);
			var dist_fo = Math.Abs ((cell.transform.position - fo.transform.position).magnitude);
			var dist_closest = Math.Abs ((cell.transform.position - closest.position).magnitude);
			if (dist_fo < dist_closest) {
				closest = fo;
			}
		}
		if (closest.GetComponent<FogScript> ().fogOn) {
			closestFogIsOn = true;
		}
		if (closestFogIsOn) {
			foreach (var _cell in _cellGrid.Cells) {
				_cell.UnMark();
			}
			return;
		}
		var cel = _unit.Cell;
		if (cell.transform.position == cel.transform.position) {
			return;
		}
		var path = _unit.FindPath (_cellGrid.Cells, cell);
		foreach (var _cell in path) {
			_cell.MarkAsPath ();
		}
		foreach (var _cell in _cellGrid.Cells.Except(path)) {
			_cell.UnMark ();
		}
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        _unit.OnUnitSelected();
        _unitCell = _unit.Cell;

        _pathsInRange = _unit.GetAvailableDestinations(_cellGrid.Cells);
        var cellsNotInRange = _cellGrid.Cells.Except(_pathsInRange);

        foreach (var cell in cellsNotInRange)
        {
            cell.UnMark();
        }
        foreach (var cell in _pathsInRange)
        {
            cell.MarkAsReachable();
        }

        //if (_unit.ActionPoints <= 0) return;

        foreach (var currentUnit in _cellGrid.Units)
        {
            if (currentUnit.PlayerNumber.Equals(_unit.PlayerNumber))
                continue;
        
			if (_unit.IsUnitAttackable(currentUnit,_unit.Cell,_cellGrid))
            {
                currentUnit.SetState(new UnitStateMarkedAsReachableEnemy(currentUnit));
                _unitsInRange.Add(currentUnit);
            }
			else {
				currentUnit.SetState (new UnitStateNormal (currentUnit));
				if (_unitsInRange.Contains (currentUnit)) {
					_unitsInRange.Remove (currentUnit);
				}
//				Debug.Break ();
			}
        }
        
		if (_unitCell.GetNeighbours(_cellGrid.Cells).FindAll(c => c.MovementCost <= _unit.MovementPoints).Count == 0
			&& _unitsInRange.Count == 0) {
//            _unit.SetState(new UnitStateMarkedAsFinished(_unit));
		}
    }
    public override void OnStateExit()
    {
        _unit.OnUnitDeselected();
        foreach (var unit in _unitsInRange)
        {
            if (unit == null) continue;
            unit.SetState(new UnitStateNormal(unit));
        }
        foreach (var cell in _cellGrid.Cells)
        {
            cell.UnMark();
        }   
    }
}
