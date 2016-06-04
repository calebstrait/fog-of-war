using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SampleUnit))]
public class SampleUnitHelper : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		SampleUnit su = (SampleUnit)target;

		if(GUILayout.Button("ShowOverwatchPattern"))
		{
			su.ShowOverwatchPattern();
		}
	}
}
