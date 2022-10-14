namespace EliasSoftware.Elias4.Editor
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using EliasSoftware.Elias4.Designtime;
	using UnityEditor;
	using UnityEditor.Build;
	using UnityEditor.Build.Reporting;
	using UnityEngine;
	using EliasSoftware.Elias4.Common;

	public class EliasBuild : IPostBuildPlayerScriptDLLs, IPostprocessBuildWithReport, IPreprocessBuildWithReport
	{
		public int callbackOrder => 0;
		private string EliasAssetFolder => Path.Combine(Application.streamingAssetsPath, "Elias4");
		
		public void OnPostBuildPlayerScriptDLLs(BuildReport report)
		{
			var baseFolder = EliasAssetFolder;
			var settings = EliasSettings.Instance;

			var res = UnityEditorSession.instance.SetActiveProject(settings.ProjectFilePath,
				settings.AssetMetadataFilePath);

			if ( Directory.Exists(Application.streamingAssetsPath) == false )
				Directory.CreateDirectory(Application.streamingAssetsPath);

			if ( Directory.Exists(baseFolder) == false )
				Directory.CreateDirectory(baseFolder);

			res = UnityEditorSession.instance.ExportAssets(baseFolder);
			if(res == false) {
				Debug.LogError("Failed to export elias project assets");
			}

			// write an 'index' file to the asset folder
			// currently used for checking if an ID has
			// a corresponding exported asset
			string exported = "";
			foreach(var f in UnityEditorSession.instance.CollectExportedAssetList(baseFolder)) {
				exported += f + "\n";
			}
			File.WriteAllText(Path.Combine(baseFolder, "index"), exported);


			File.WriteAllText(Path.Combine(baseFolder, "EliasSettings.json"),
				JsonUtility.ToJson(RuntimeEliasSettings.FromEliasSettings(settings))
			);

			var designtimeAssets = UnityEditorSession.instance.UnsafeDesigntimeAssets;
			if(designtimeAssets == null)
				designtimeAssets = UnityEditorSession.instance.CreateDesigntimeAssets();
	
			var jsonBlob = designtimeAssets.ExportToJson();
			if(jsonBlob != null) {
				File.WriteAllText(Path.Combine(baseFolder, "EliasAssets.json"), jsonBlob);
			} else {
				Debug.LogError("Failed to export elias id mapping object");
			}
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			var baseFolder = EliasAssetFolder;

			AssetDatabase.DisallowAutoRefresh();

			if ( Directory.EnumerateFileSystemEntries(baseFolder).Any() )
			{
				FileUtil.DeleteFileOrDirectory(baseFolder);
				var metaFile = Path.ChangeExtension(baseFolder, ".meta");
				if (File.Exists(metaFile))
					FileUtil.DeleteFileOrDirectory(metaFile);
			}

			AssetDatabase.AllowAutoRefresh();
		}

		public void OnPreprocessBuild(BuildReport report)
		{
			// ensure that the current editor session 
			// has actually loaded the project stored in
			// settings
			var settings = EliasSettings.Instance;
			var res = UnityEditorSession.instance.SetActiveProject(settings.ProjectFilePath,
				settings.AssetMetadataFilePath);
			Debug.Assert(res, "Failed to activate project from settings");
		}
	}
}
