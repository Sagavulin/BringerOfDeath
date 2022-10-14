namespace EliasSoftware.Elias4.Editor
{
    using EliasSoftware.Elias4;
	using UnityEditor;
	using UnityEditor.EditorTools;
	using UnityEngine;

	[CustomEditor(typeof(EliasZone))]
	public class EliasZoneEditor : Editor
	{
		SerializedProperty geometry;
		SerializedProperty enableVerticalBlending;
		SerializedProperty soundBleed;
		SerializedProperty reverb;

		private void OnEnable()
		{
			geometry               = serializedObject.FindProperty(nameof(EliasZone.geometry));
			enableVerticalBlending = serializedObject.FindProperty(nameof(EliasZone.enableVerticalBlending));
			soundBleed             = serializedObject.FindProperty(nameof(EliasZone.soundBleed));
			reverb                 = serializedObject.FindProperty(nameof(EliasZone.reverb));
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(enableVerticalBlending);
			EditorGUILayout.PropertyField(soundBleed, includeChildren: true);
			EditorGUILayout.PropertyField(reverb, includeChildren: true);

			using (var change = new EditorGUI.ChangeCheckScope())
			{
				bool enableTool = GUILayout.Toggle(ToolManager.activeToolType == typeof(EliasZoneTool), "Edit vertices", EditorStyles.miniButton);
				if (change.changed)
				{
					if (enableTool)
						ToolManager.SetActiveTool<EliasZoneTool>();
					else
						ToolManager.RestorePreviousTool();
				}
			}

			EditorGUILayout.PropertyField(geometry, includeChildren: true);

			var vertexCount = geometry.FindPropertyRelative("vertices").arraySize;
			if (vertexCount > Elias.API.Runtime.PolygonMaxPoints)
				EditorGUILayout.HelpBox($"Harmony only supports up to {Elias.API.Runtime.PolygonMaxPoints} vertices.\nYour geometry currently has {vertexCount} vertices.", MessageType.Error);

			serializedObject.ApplyModifiedProperties();
		}
	}

}
