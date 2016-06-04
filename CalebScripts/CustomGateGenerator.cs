using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomGateGenerator : MonoBehaviour
{
    public Transform GateParent;
    public CellGrid CellGrid;

    public void Start()
    {
        StartCoroutine(SpawnGates());
    }
	public IEnumerator SpawnGates()
    {
        while (CellGrid.Cells == null)
        {
            yield return 0;
        }

        var cells = CellGrid.Cells;

		for (int i = 0; i < GateParent.childCount; i++)
        {
			var obstacle = GateParent.GetChild(i);

            var cell = cells.OrderBy(h => Math.Abs((h.transform.position - obstacle.transform.position).magnitude)).First();
			var neighbor1 = cells.Find(c => c.OffsetCoord == cell.OffsetCoord + new Vector2 (5000, 0));
			var neighbor2 = cells.Find(c => c.OffsetCoord == cell.OffsetCoord + new Vector2 (5000, 0));
			if (obstacle.transform.localScale.x > 1) {
				neighbor1 = cells.Find(c => c.OffsetCoord == cell.OffsetCoord + new Vector2 (1, 0));
				neighbor2 = cells.Find(c => c.OffsetCoord == cell.OffsetCoord + new Vector2 (-1, 0));
			} else {
				neighbor1 = cells.Find(c => c.OffsetCoord == cell.OffsetCoord + new Vector2(0, 1));
				neighbor2 = cells.Find(c => c.OffsetCoord == cell.OffsetCoord + new Vector2(0, -1));
			}
			if (!cell.IsTaken && !neighbor1.IsTaken && !neighbor2.IsTaken && neighbor1 != null && neighbor2 != null)
            {
                cell.IsTaken = true;
                Vector3 offset = new Vector3(0, 0, cell.GetCellDimensions().z);
                obstacle.position = cell.transform.position - offset;
				neighbor1.IsTaken = true;
				neighbor2.IsTaken = true;
            }
            else
            {
                Destroy(obstacle.gameObject);
            }
        }
    }

    public void SnapToGrid()
    {
        List<Transform> cells = new List<Transform>();

        foreach (Transform cell in CellGrid.transform)
        {
            cells.Add(cell);
        }

		foreach (Transform obstacle in GateParent)
        {
            var closestCell = cells.OrderBy(h => Math.Abs((h.transform.position - obstacle.transform.position).magnitude)).First();
            if (!closestCell.GetComponent<Cell>().IsTaken)
            {
                Vector3 offset = new Vector3(0, 0, closestCell.GetComponent<Cell>().GetCellDimensions().z);
                obstacle.position = closestCell.transform.position - offset;
            }
        }
    }
}

