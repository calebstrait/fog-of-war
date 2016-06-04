using UnityEngine;

class SampleSquare : Square
{

    public override Vector3 GetCellDimensions()
    {
        return GetComponent<Renderer>().bounds.size;
    }

    public override void MarkAsHighlighted()
    {
		GetComponent<Renderer> ().material.color = new Color (0.75f, 0.75f, 0.75f);
    }

    public override void MarkAsPath()
    {
		//Debug.Log (transform.parent.GetComponent<CellGrid> ().CurrentPlayerNumber);
		if (transform.parent.GetComponent<CellGrid>().CurrentPlayerNumber == 0) {
			GetComponent<Renderer> ().material.color = new Color(0.6f, 0.6f, 1.0f);
		}
    }

    public override void MarkAsReachable()
    {
//		if (transform.parent.GetComponent<CellGrid> ().CurrentPlayerNumber == 0) {
//			GetComponent<Renderer> ().material.color = Color.green;
//		}
    }

    public override void UnMark()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }

	public override void MarkAsOverwatch()
	{
		//Debug.Log("MarkAsOverwatch() " + transform.position);
		GetComponent<Renderer> ().material.color = new Color (1f, .3f, .3f);
	}
}

