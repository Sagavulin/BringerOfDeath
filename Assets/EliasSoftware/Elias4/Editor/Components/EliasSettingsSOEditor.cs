namespace EliasSoftware.Elias4.Editor
{
    using EliasSoftware.Elias4;
	using UnityEditor;
	using UnityEditor.EditorTools;
	using UnityEngine;
	using EliasSoftware.Elias4.Common;

	[CustomEditor(typeof(EliasSettingsSO))]
	public class EliasSettingsSOEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Open settings"))
				SettingsService.OpenProjectSettings(EliasSetupUI.Path);
		}
	}
}
