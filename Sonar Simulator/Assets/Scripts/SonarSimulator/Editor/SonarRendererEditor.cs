using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SonarRenderer))]
public class SonarRendererEditor : Editor
{
	SonarRenderer sonarRenderer;

	void OnEnable()
	{
		sonarRenderer = (SonarRenderer) target;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		GUILayout.Space(10);

		if (GUILayout.Button("Take Image"))
		{
			sonarRenderer.TakeImage();
		}
	}
}
