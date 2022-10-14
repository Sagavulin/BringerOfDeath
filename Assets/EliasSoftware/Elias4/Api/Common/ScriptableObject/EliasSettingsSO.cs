namespace EliasSoftware.Elias4.Common
{
	using System;
	using System.IO;
	using System.Linq;
	#if UNITY_EDITOR
	using UnityEditor;
	#endif
	using UnityEngine;


	public class EliasSettingsSO : ScriptableObject, IEliasSettings
	{
		public string ProjectFilePath => projectFilePath;
		public string MusicProjectFilePath => musicProjectFilePath;
		public bool AutoLoadMusicProject => autoLoadMusicProject;
		public string AssetMetadataFilePath => AssetsFile;

		public AudioRenderingSettings AudioRendering => audioRendering;
		public string projectFilePath = string.Empty;
		public string musicProjectFilePath = string.Empty;
		public bool autoLoadMusicProject = false;

		public AudioRenderingSettings audioRendering = new AudioRenderingSettings(2, 512, 48000);
		public ZoneVisualizationSettings zoneVisualization = ZoneVisualizationSettings.CreateDefault();

		[NonSerialized]
		internal const string SettingsPath = "Assets/EliasSoftware/Elias4/";
		[NonSerialized]
		internal const string SettingsFile = SettingsPath + "EliasSettings.asset";
		[NonSerialized]
		internal const string AssetsPath = "Assets/EliasSoftware/Elias4/";
		[NonSerialized]
		internal const string AssetsFile = AssetsPath + "EliasAssets.asset";
	}

	public class RuntimeEliasSettings : ScriptableObject, IEliasSettings
	{
		public string ProjectFilePath => Path.Combine(Application.streamingAssetsPath, "Elias4", eliasProjectName);
		public string MusicProjectFilePath => Path.Combine(Application.streamingAssetsPath, "Elias4Music", eliasMusicProjectName);
		public string AssetMetadataFilePath => Path.Combine(Application.streamingAssetsPath, "Elias4", "EliasAssets.asset");

		//public string LogDirPath => string.Empty;
		public bool AutoLoadMusicProject => autoLoadMusicProject;

		public AudioRenderingSettings AudioRendering => audioRendering;

		public bool autoLoadMusicProject;
		public string eliasProjectName;
		public string eliasMusicProjectName;
		public AudioRenderingSettings audioRendering = new AudioRenderingSettings(2, 512, 48000);

		public static RuntimeEliasSettings FromEliasSettings(IEliasSettings settings)
		{
			var runtime = CreateInstance<RuntimeEliasSettings>();
			runtime.eliasProjectName = Path.GetFileName(settings.ProjectFilePath);
			runtime.audioRendering = settings.AudioRendering;
			runtime.autoLoadMusicProject = settings.AutoLoadMusicProject;
			runtime.eliasMusicProjectName = Path.GetFileName(settings.MusicProjectFilePath);
			return runtime;
		}
	}

	public static class EliasSettings
	{
		public static IEliasSettings Instance
		{
			get
			{
				if (instance == null)
					instance = Load();
				return instance;
			}
		}

		private static IEliasSettings instance = null;

		private static IEliasSettings Load()
		{
#if UNITY_EDITOR
			EliasSettingsSO settings = null;

			//Debug.Log("Loading settings (creating instance)");

			if (!settings)
				settings = Resources.FindObjectsOfTypeAll<EliasSettingsSO>().FirstOrDefault();
			if (!settings)
				settings = AssetDatabase.LoadAssetAtPath<EliasSettingsSO>(EliasSettingsSO.SettingsFile);
			if (!settings)
			{
				Debug.Log("Could not find EliasSettingsSO, creating new ...");
				settings = ScriptableObject.CreateInstance<EliasSettingsSO>();
				AssetDatabase.CreateAsset(settings, EliasSettingsSO.SettingsFile);
				AssetDatabase.SaveAssets();
			}
			if (!settings)
				Debug.LogError("singleton asset missing: " + EliasSettingsSO.SettingsPath);

			return settings;
#else
			RuntimeEliasSettings settings = ScriptableObject.CreateInstance<RuntimeEliasSettings>();
			JsonUtility.FromJsonOverwrite(
				File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Elias4", "EliasSettings.json")),
				settings);
			return settings;
#endif
		}
	}
}
