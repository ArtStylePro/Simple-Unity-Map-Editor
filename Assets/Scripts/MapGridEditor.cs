using UnityEngine;
using System.Collections;
using UnityEditor;

namespace MapEditor
{
	[CustomEditor(typeof(MapGrid))]
	public class MapGridEditor : Editor 
	{
		public override void OnInspectorGUI()
		{
			MapGrid grid = (MapGrid)target;

			grid.Columns = EditorGUILayout.IntField ("Columns", grid.Columns);
			grid.Rows = EditorGUILayout.IntField ("Rows", grid.Rows);

			if (GUILayout.Button ("Update Grid Size")) 
			{
				UpdateGridSize (grid);
			}
		}

		private void UpdateGridSize(MapGrid grid)
		{
			if (grid.PlaneTransform && grid.GridLineTransform)
			{
				grid.PlaneTransform.localScale = new Vector3 (grid.Columns / 10f, 1, grid.Rows / 10f);
				grid.GridLineTransform.localScale = grid.PlaneTransform.localScale;
			}
		}
	}
}