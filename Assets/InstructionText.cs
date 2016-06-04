using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InstructionText : MonoBehaviour {

	public CellGrid CellGrid;
	public Text instruction;
	void Start () {
		instruction = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		if (CellGrid.result == -1) {
			instruction.text = "ATTACKERS WIN!";
			instruction.color = Color.red;
		} else if (CellGrid.result == 1) {
			instruction.text = "DEFENDERS WIN!";
			instruction.color = Color.blue;
		} else if (CellGrid.CurrentPlayerNumber == 0) {
			instruction.text = "INFILTRATE AND LOOT!";
			instruction.color = Color.green;
		} else {
			instruction.text = "POSITION YOUR TROOPS!";
			instruction.color = Color.green;
		}
	}
}
