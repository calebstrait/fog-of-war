using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

/// <summary>
/// Base class for all units in the game.
/// </summary>
public abstract class Unit : MonoBehaviour
{
	public bool survivedMove;
	public bool updateMove;
	private Vector3 updateV3;
	private float rate;
	public bool[,] overwatchPattern;
	public Cell destinationCell;

	/// <summary>
    /// UnitClicked event is invoked when user clicks the unit. It requires a collider on the unit game object to work.
    /// </summary>
    public event EventHandler UnitClicked;
    /// <summary>
    /// UnitSelected event is invoked when user clicks on unit that belongs to him. It requires a collider on the unit game object to work.
    /// </summary>
    public event EventHandler UnitSelected;
    public event EventHandler UnitDeselected;
    /// <summary>
    /// UnitHighlighted event is invoked when user moves cursor over the unit. It requires a collider on the unit game object to work.
    /// </summary>
    public event EventHandler UnitHighlighted;
    public event EventHandler UnitDehighlighted;
    public event EventHandler<AttackEventArgs> UnitAttacked;
    public event EventHandler<AttackEventArgs> UnitDestroyed;
    public event EventHandler<MovementEventArgs> UnitMoved;

    public UnitState UnitState { get; set; }
    public void SetState(UnitState state)
    {
        UnitState.MakeTransition(state);
		//Debug.Log(name + " into state: " + state);
    }

    public List<Buff> Buffs { get; private set; }

    public int TotalHitPoints { get; private set; }
    protected int TotalMovementPoints;
    protected int TotalActionPoints;

    /// <summary>
    /// Cell that the unit is currently occupying.
    /// </summary>
    public Cell Cell { get; set; }

    public int HitPoints;
    public int AttackRange;
    public int AttackFactor;
    public int DefenceFactor;
    /// <summary>
    /// Determines how far on the grid the unit can move.
    /// </summary>
    public int MovementPoints;
    /// <summary>
    /// Determines speed of movement animation.
    /// </summary>
    public float MovementSpeed;
    /// <summary>
    /// Determines how many attacks unit can perform in one turn.
    /// </summary>
    public int ActionPoints;

    /// <summary>
    /// Indicates the player that the unit belongs to. Should correspoond with PlayerNumber variable on Player script.
    /// </summary>
    public int PlayerNumber;

	public float Facing = 180f;

    /// <summary>
    /// Indicates if movement animation is playing.
    /// </summary>
    public bool isMoving { get; set; }
	public bool hover;
	public bool isAlive;

    private static IPathfinding _pathfinder = new AStarPathfinding();

    /// <summary>
    /// Method called after object instantiation to initialize fields etc. 
    /// </summary>
    public virtual void Initialize()
    {
		hover = false;
		isAlive = true;
		updateMove = false;
		Buffs = new List<Buff>();

        UnitState = new UnitStateNormal(this);

        TotalHitPoints = HitPoints;
        TotalMovementPoints = MovementPoints;
        TotalActionPoints = ActionPoints;

    }

    protected virtual void OnMouseDown()
    {
        if (UnitClicked != null)
            UnitClicked.Invoke(this, new EventArgs());
    }
    protected virtual void OnMouseEnter()
    {
		hover = true;
		//Debug.Log ("hover!");
		if (UnitHighlighted != null)
            UnitHighlighted.Invoke(this, new EventArgs());
    }
    protected virtual void OnMouseExit()
    {
		hover = false;
		if (UnitDehighlighted != null)
            UnitDehighlighted.Invoke(this, new EventArgs());
    }

    /// <summary>
    /// Method is called at the start of each turn.
    /// </summary>
    public virtual void OnTurnStart()
    {
        MovementPoints = TotalMovementPoints;
        ActionPoints = TotalActionPoints;

        SetState(new UnitStateMarkedAsFriendly(this));
    }
    /// <summary>
    /// Method is called at the end of each turn.
    /// </summary>
    public virtual void OnTurnEnd()
    {
        Buffs.FindAll(b => b.Duration == 0).ForEach(b => { b.Undo(this); });
        Buffs.RemoveAll(b => b.Duration == 0);
        Buffs.ForEach(b => { b.Duration--; });

        SetState(new UnitStateNormal(this));
    }
    /// <summary>
    /// Method is called when units HP drops below 1.
    /// </summary>
    protected virtual void OnDestroyed(CellGrid CellGrid)
    {

		Cell.IsTaken = false;
        MarkAsDestroyed();
        //Destroy(gameObject);
		isAlive = false;

		int ATKersAlive = 0;  //Checking if the game is over
		int DEFersAlive = 0;
		foreach (Transform unit in this.transform.parent) {
			if (unit.gameObject.GetComponent<Unit> ().isAlive) { 
				if (unit.gameObject.GetComponent<Unit> ().PlayerNumber == 0) {
					ATKersAlive++;
				} else {
					DEFersAlive++;
				}
			}
		}
		if (ATKersAlive == 0) {
			CellGrid.result = 1;
			StartCoroutine(TurnOffResultText (CellGrid));
		} else if (DEFersAlive == 0) {
			CellGrid.result = -1;
			StartCoroutine(TurnOffResultText (CellGrid));
			GameObject.Find ("GUIController").gameObject.GetComponent<GUIController> ().ModeSwitch ();
		}

    }

	protected virtual IEnumerator TurnOffResultText(CellGrid CellGrid) {
		yield return new WaitForSeconds (2);
		CellGrid.result = 0;
	}

    /// <summary>
    /// Method is called when unit is selected.
    /// </summary>
    public virtual void OnUnitSelected()
    {
        SetState(new UnitStateMarkedAsSelected(this));
        if (UnitSelected != null)
            UnitSelected.Invoke(this, new EventArgs());
    }
    /// <summary>
    /// Method is called when unit is deselected.
    /// </summary>
    public virtual void OnUnitDeselected()
    {
        SetState(new UnitStateMarkedAsFriendly(this));
        if (UnitDeselected != null)
            UnitDeselected.Invoke(this, new EventArgs());
    }

    /// <summary>
    /// Method indicates if it is possible to attack unit given as parameter, from cell given as second parameter.
    /// </summary>
	public virtual bool IsUnitAttackable(Unit other, Cell sourceCell, CellGrid cg)
    {
//		var myself=other.Cell.transform;
//		var parent=other.Cell.transform.parent;
//		var childCount=parent.childCount;
//		int indexOfOther;
//		for (int i=0;i<childCount;i++) { // skip the last, as it doesn't have a successor
//			if (parent.GetChild(i)==myself)
//				indexOfOther = i;
//		}
//		FogScript FS = FogGrid.transform.GetChild (indexOfOther);

		Vector3 diff = transform.position - other.transform.position;
		//Debug.Log (diff.magnitude);
		if (diff.magnitude < 2.3f && cg.CurrentPlayerNumber == 0) //sourceCell.GetDistance(other.Cell) <= AttackRange && 
            return true;

        return false;
    }
    /// <summary>
    /// Method deals damage to unit given as parameter.
    /// </summary>
    public virtual void DealDamage(Unit other, CellGrid cg)
    {
        if (isMoving)
            return;
//        if (ActionPoints == 0)
//            return;
        if (!IsUnitAttackable(other, Cell, cg))
            return;

		//Rotate if needed ***
		float xdist = transform.position.x - other.transform.position.x;
		float ydist = transform.position.y - other.transform.position.y;
		if (xdist > 0f && Math.Abs(xdist) > Math.Abs(ydist)) {transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 270f);}
		if (xdist < 0f && Math.Abs(xdist) > Math.Abs(ydist)) {transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 90f);}
		if (ydist > 0f && Math.Abs(xdist) < Math.Abs(ydist)) {transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 180f);}
		if (ydist < 0f && Math.Abs(xdist) < Math.Abs(ydist)) {transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f);}
		//***

		MarkAsAttacking(other);
        //ActionPoints--;
        other.Defend(this, AttackFactor, cg);

//        if (ActionPoints == 0)
//        {
//            SetState(new UnitStateMarkedAsFinished(this));
//            MovementPoints = 0;
//        }  
    }
    /// <summary>
    /// Attacking unit calls Defend method on defending unit. 
    /// </summary>
	public virtual void Defend(Unit other, int damage, CellGrid cg)
    {
        MarkAsDefending(other);
        HitPoints -= Mathf.Clamp(damage - DefenceFactor, 1, damage);  //Damage is calculated by subtracting attack factor of attacker and defence factor of defender. If result is below 1, it is set to 1.
                                                                      //This behaviour can be overridden in derived classes.
        if (UnitAttacked != null)
            UnitAttacked.Invoke(this, new AttackEventArgs(other, this, damage));

        if (HitPoints <= 0)
        {
			foreach (Transform cell in cg.transform) {
				while (cell.GetComponent<Cell> ().WatchedBy.Contains(GetComponent<SampleUnit> ())) {
					cell.GetComponent<Cell> ().WatchedBy.Remove (GetComponent<SampleUnit> ());
				}
			}
//			if (UnitDestroyed != null)
//                UnitDestroyed.Invoke(this, new AttackEventArgs(other, this, damage));
            OnDestroyed(cg);
        }
    }

	//public CellGridState CellGridState;

	public virtual void Move(Cell dc, List<Cell> path, CellGrid cg)
    {
		destinationCell = dc;
		if (isMoving)
            return;

		//Check for overwatch ***
		List<Cell> newPath = new List<Cell>();
		Cell dest = destinationCell;
		path.Reverse();
		foreach (var cell in path) {
			newPath.Add(cell);
			if (cg.CurrentPlayerNumber == 0 && cell.WatchedBy.Count > 0) {
				dest = cell;
				break;
			}
		}
		newPath.Reverse();
		destinationCell = dest;
		//**
		if (UnitMoved != null) 
			UnitMoved.Invoke(this, new MovementEventArgs(Cell, destinationCell, newPath));

		var totalMovementCost = newPath.Sum(h => h.MovementCost);
		if (MovementPoints < totalMovementCost)
			return;

		MovementPoints -= totalMovementCost;

		StartCoroutine(MovementAnimation(newPath, cg));
    }
	protected virtual IEnumerator MovementAnimation(List<Cell> path, CellGrid cg)
	{
		isMoving = true;

		path.Reverse ();
		foreach (var cell in path) {

			//Rotate during move ***
			if (transform.position.x > cell.transform.position.x) {
				Facing = 270f;
			}
			if (transform.position.x < cell.transform.position.x) {
				Facing = 90f;
			}
			if (transform.position.y > cell.transform.position.y) {
				Facing = 180f;
			}
			if (transform.position.y < cell.transform.position.y) {
				Facing = 0f;
			}
			transform.eulerAngles = new Vector3 (transform.eulerAngles.x, transform.eulerAngles.y, Facing);
			//***

			while (new Vector2 (transform.position.x, transform.position.y) != new Vector2 (cell.transform.position.x, cell.transform.position.y)) {		
				updateV3 = new Vector3 (cell.transform.position.x, cell.transform.position.y, transform.position.z);
				rate = Time.deltaTime * MovementSpeed;
				updateMove = true;
				yield return 0;
			}
			updateMove = false;
//			while (new Vector2 (transform.position.x, transform.position.y) != new Vector2 (cell.transform.position.x, cell.transform.position.y)) {
//				updateMove 
//				transform.position = Vector3.MoveTowards (transform.position, new Vector3 (cell.transform.position.x, cell.transform.position.y, transform.position.z), Time.deltaTime * MovementSpeed);
//				yield return 0;
//			}
		}

		//**** OVERWATCH
		survivedMove = true;
		if (cg.CurrentPlayerNumber == 0 && destinationCell.WatchedBy.Count > 0) {
			foreach (SampleUnit watcher in destinationCell.WatchedBy) {
				watcher.ShowOverwatchPattern ();
				if (HitPoints <= watcher.AttackFactor)
					survivedMove = false;
				//GetComponent<Unit>().Defend (watcher, watcher.AttackFactor); 
			}
		}

		if (cg.CurrentPlayerNumber == 1) {
			//** Refresh overwatch post-move
			List<Transform> cells = new List<Transform> ();
			foreach (Transform cell in cg.transform) {
				cells.Add (cell);
				//if (cell.GetComponent<Cell> ().WatchedBy.Contains (GetComponent<SampleUnit>())) {//remove all
				cell.GetComponent<Cell> ().WatchedBy.Remove (GetComponent<SampleUnit> ());
				//}
			}
			for (int i = 0; i < overwatchPattern.GetLength (0); i++) {
				for (int j = 0; j < overwatchPattern.GetLength (1); j++) {
					if (overwatchPattern [i, j]) {
						List<Transform> oCells = cells;
						if (Facing == 0f) { //UP
							oCells = cells.FindAll (c => c.transform.position.x == transform.position.x + (i - 1) && c.transform.position.y == transform.position.y + j);
						} else if (Facing == 180f) { //DOWN
							oCells = cells.FindAll (c => c.transform.position.x == transform.position.x + (i - 1) && c.transform.position.y == transform.position.y - j);
						} else if (Facing == 90f) { //LEFT
							oCells = cells.FindAll (c => c.transform.position.x == transform.position.x + j && c.transform.position.y == transform.position.y + (i - 1));
						} else if (Facing == 270f) { //RIGHT
							oCells = cells.FindAll (c => c.transform.position.x == transform.position.x - j && c.transform.position.y == transform.position.y + (i - 1));
						} else {
							Debug.Log (Facing);
							Debug.Break ();
						}
						foreach (Transform oCell in oCells) {
							if (!oCell.GetComponent<Cell> ().WatchedBy.Contains (GetComponent<SampleUnit> ())) {
								oCell.GetComponent<Cell> ().WatchedBy.Add (GetComponent<SampleUnit> ());
							}
						}
					}
				}
			}
			//**
			transform.GetComponent<SampleUnit> ().ShowOverwatchPattern ();
		}
			
		Cell.IsTaken = false;
		if (survivedMove) {
			Cell = destinationCell;
			destinationCell.IsTaken = true;
		} else {
			//ActionPoints = 0;
		}
		//****

		if (cg.CurrentPlayerNumber == 0 && destinationCell.WatchedBy.Count > 0) {
			transform.GetComponent<SampleUnit> ().MarkAsReachableEnemy ();
			yield return new WaitForSeconds (1);
		} else if (cg.CurrentPlayerNumber == 1) {
			yield return new WaitForSeconds (1);
		}

		//DO ACTUAL DMG
		if (cg.CurrentPlayerNumber == 0 && destinationCell.WatchedBy.Count > 0) {
			foreach (SampleUnit watcher in destinationCell.WatchedBy) {
				watcher.ShowOverwatchPattern ();
				GetComponent<Unit>().Defend (watcher, watcher.AttackFactor, cg); 
			}
		}

		if (survivedMove) {
			cg.CellGridState = new CellGridStateUnitSelected (cg, this);
		} else {
			cg.CellGridState = new CellGridStateWaitingForInput (cg);
			//Debug.Log ("Overwatch Kill!");
		}
		isMoving = false; 

		if (cg.result != 0)
			GameObject.Find ("GUIController").gameObject.GetComponent<GUIController> ().ModeSwitch ();
    }

	void Update ()
	{
		if (updateMove) {
			transform.position = Vector3.MoveTowards (transform.position, updateV3, rate);
		}
	}

    ///<summary>
    /// Method indicates if unit is capable of moving to cell given as parameter.
    /// </summary>
    public virtual bool IsCellMovableTo(Cell cell)
    {
        return !cell.IsTaken;
    }
    /// <summary>
    /// Method indicates if unit is capable of moving through cell given as parameter.
    /// </summary>
    public virtual bool IsCellTraversable(Cell cell)
    {
        return !cell.IsTaken;
    }
    /// <summary>
    /// Method returns all cells that the unit is capable of moving to.
    /// </summary>
    public List<Cell> GetAvailableDestinations(List<Cell> cells)
    {
        var ret = new List<Cell>();
        var cellsInMovementRange = cells.FindAll(c => IsCellMovableTo(c) && c.GetDistance(Cell) <= MovementPoints);

        var traversableCells = cells.FindAll(c => IsCellTraversable(c) && c.GetDistance(Cell) <= MovementPoints);
        traversableCells.Add(Cell);

        foreach (var cellInRange in cellsInMovementRange)
        {
            if (cellInRange.Equals(Cell)) continue;

            var path = FindPath(traversableCells, cellInRange);
            var pathCost = path.Sum(c => c.MovementCost);
            if (pathCost > 0 && pathCost <= MovementPoints)
                ret.AddRange(path);
        }
        return ret.FindAll(IsCellMovableTo).Distinct().ToList();
    }

    public List<Cell> FindPath(List<Cell> cells, Cell destination)
    {
        return _pathfinder.FindPath(GetGraphEdges(cells), Cell, destination);
    }
    /// <summary>
    /// Method returns graph representation of cell grid for pathfinding.
    /// </summary>
    protected virtual Dictionary<Cell, Dictionary<Cell, int>> GetGraphEdges(List<Cell> cells)
    {
        Dictionary<Cell, Dictionary<Cell, int>> ret = new Dictionary<Cell, Dictionary<Cell, int>>();
        foreach (var cell in cells)
        {
            if (IsCellTraversable(cell) || cell.Equals(Cell))
            {
                ret[cell] = new Dictionary<Cell, int>();
                foreach (var neighbour in cell.GetNeighbours(cells).FindAll(IsCellTraversable))
                {
                    ret[cell][neighbour] = neighbour.MovementCost;
                }
            }
        }
        return ret;
    }

    /// <summary>
    /// Gives visual indication that the unit is under attack.
    /// </summary>
    /// <param name="other"></param>
    public abstract void MarkAsDefending(Unit other);
    /// <summary>
    /// Gives visual indication that the unit is attacking.
    /// </summary>
    /// <param name="other"></param>
    public abstract void MarkAsAttacking(Unit other);
    /// <summary>
    /// Gives visual indication that the unit is destroyed. It gets called right before the unit game object is
    /// destroyed, so either instantiate some new object to indicate destruction or redesign Defend method. 
    /// </summary>
    public abstract void MarkAsDestroyed();

    /// <summary>
    /// Method marks unit as current players unit.
    /// </summary>
    public abstract void MarkAsFriendly();
    /// <summary>
    /// Method mark units to indicate user that the unit is in range and can be attacked.
    /// </summary>
    public abstract void MarkAsReachableEnemy();
    /// <summary>
    /// Method marks unit as currently selected, to distinguish it from other units.
    /// </summary>
    public abstract void MarkAsSelected();
    /// <summary>
    /// Method marks unit to indicate user that he can't do anything more with it this turn.
    /// </summary>
    public abstract void MarkAsFinished();
    /// <summary>
    /// Method returns the unit to its base appearance
    /// </summary>
    public abstract void UnMark();
	public abstract void ShowOverwatchPattern();
}

public class MovementEventArgs : EventArgs
{
    public Cell OriginCell;
    public Cell DestinationCell;
    public List<Cell> Path;

    public MovementEventArgs(Cell sourceCell, Cell destinationCell, List<Cell> path)
    {
        OriginCell = sourceCell;
        DestinationCell = destinationCell;
        Path = path;
    }
}
public class AttackEventArgs : EventArgs
{
    public Unit Attacker;
    public Unit Defender;

    public int Damage;

    public AttackEventArgs(Unit attacker, Unit defender, int damage)
    {
        Attacker = attacker;
        Defender = defender;

        Damage = damage;
    }
}
