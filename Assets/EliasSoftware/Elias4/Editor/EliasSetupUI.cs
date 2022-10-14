namespace EliasSoftware.Elias4.Editor
{
	using EliasSoftware.Elias4;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UIElements;
	using EliasSoftware.Elias4.Common;

	public class EliasSetupUI :
		SettingsProvider
	{
		public const string Path = "Project/Audio/Elias";
		private const float LeftMargin = 5f;
		private const float ButtonWidth = 150f;


		[SettingsProvider]
		private static SettingsProvider EliasSettingsProvider() => new EliasSetupUI();

		public EliasSetupUI() : base(Path, SettingsScope.Project)
		{
		}

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
		}

		public override void OnGUI(string searchContext)
		{
			var info = (EliasSettingsSO)EliasSettings.Instance;

			var serializedObject = new SerializedObject(info);
			serializedObject.Update();

			var projectFilePath           = serializedObject.FindProperty(nameof(EliasSettingsSO.projectFilePath));
			var musicProjectFilePath      = serializedObject.FindProperty(nameof(EliasSettingsSO.musicProjectFilePath));
			var autoLoadMusic      		  = serializedObject.FindProperty(nameof(EliasSettingsSO.autoLoadMusicProject));
			var audioRenderingSettings    = serializedObject.FindProperty(nameof(EliasSettingsSO.audioRendering));

			//var logDirPath                = serializedObject.FindProperty(nameof(EliasSettingsSO.logDirPath));
			var zoneVisualizationSettings = serializedObject.FindProperty(nameof(EliasSettingsSO.zoneVisualization));

			string mainProject = projectFilePath.stringValue;
			string musicProject = musicProjectFilePath.stringValue;

			EditorGUILayout.Space();
			using (var outerH = new EditorGUILayout.HorizontalScope())
			{
				GUILayout.Space(LeftMargin);
				using (var outerV = new EditorGUILayout.VerticalScope())
				{
					EditorGUILayout.LabelField("Elias Project", EditorStyles.boldLabel);
					using (var h = new GUILayout.HorizontalScope())
					{
						mainProject = EditorGUILayout.TextField("Project Path", mainProject);
						if(EditorApplication.isUpdating == false) {
							if (GUILayout.Button("Browse", GUILayout.Width(ButtonWidth)))
							{
								var path = EditorUtility.OpenFilePanel("Elias", "", "elias");
								if (path.Length != 0) {
									mainProject = path;
									projectFilePath.stringValue = mainProject;
								}
							}
						}
					}

					EditorGUILayout.LabelField("Elias Music Project", EditorStyles.boldLabel);
					EditorGUILayout.PropertyField(autoLoadMusic);
					using (var h = new GUILayout.HorizontalScope())
					{
						musicProject = EditorGUILayout.TextField("Music Project Path", musicProject);
						if( EditorApplication.isUpdating == false ) {
							if (GUILayout.Button("Browse", GUILayout.Width(ButtonWidth)))
							{
								var path = EditorUtility.OpenFilePanel("Elias Music Project", "", "mepro");
								if (path.Length != 0) {
									musicProject = path;
									musicProjectFilePath.stringValue = musicProject;
								}
							}
						}
					}

					/*using (var h = new GUILayout.HorizontalScope())
					{
						logDirPath.stringValue = EditorGUILayout.TextField("Log", logDirPath.stringValue);
						if (GUILayout.Button("Browse", GUILayout.Width(ButtonWidth)))
						{
							var path = EditorUtility.OpenFolderPanel("Log folder", "", "");
							if (path.Length != 0)
								logDirPath.stringValue = path;
						}
					}*/

					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Elias Audio Rendering", EditorStyles.boldLabel);
					{
						RenderAudioRenderingSettings(audioRenderingSettings);
						//SampleRateWarning(info);
					}

					EditorGUILayout.Space();

					//Remoting.GetState(out var remotingState);
					//EditorGUILayout.LabelField("Harmony Studio Connection", EditorStyles.boldLabel);
					//{
					//	RenderRemoteStatus(remotingState);
					//}

					//EditorGUILayout.Space();

					EditorGUILayout.LabelField("Zone Visualization", EditorStyles.boldLabel);
					{
						RenderZoneVisualizationSettings(zoneVisualizationSettings);
					}

					serializedObject.ApplyModifiedProperties();

					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
					{
						if (!Application.isPlaying)
						{
							if (GUILayout.Button("Setup", GUILayout.Width(ButtonWidth)))
								EditorApplication.delayCall += () => {
									Elias.Shutdown();
									Elias.Setup(info, new EliasEditor());
								};
						}
					}
				}
			}





			/*GUILayout.FlexibleSpace();

			RenderFusionVerion();*/

		}

		private void SampleRateWarning(EliasSettingsSO info)
		{
			if (AudioSettings.outputSampleRate != info.audioRendering.SampleRate)
			{
				EditorGUILayout.Space();
				using (var h = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
				{
					GUILayout.Label(EditorGUIUtility.IconContent("Warning@2x"));
					using (var v = new EditorGUILayout.VerticalScope())
					{
						GUILayout.Label($"Harmony sample rate ({info.audioRendering.SampleRate} hz) does not match Unity output sample rate ({AudioSettings.outputSampleRate} hz).\nAudio output will not sound correct.");
						GUILayout.Label("Do you want to update the Unity output sample rate to match Harmony?");
						if (GUILayout.Button("Update output sample rate", GUILayout.ExpandWidth(false)))
						{
							var audioConfig = AudioSettings.GetConfiguration();
							audioConfig.sampleRate = info.audioRendering.SampleRate;
							AudioSettings.Reset(audioConfig);
						}
					}
				}
				EditorGUILayout.Space();
			}
		}

		private void RenderAudioRenderingSettings(SerializedProperty renderingSettings)
		{
			var numChannels     = renderingSettings.FindPropertyRelative(nameof(AudioRenderingSettings.NumChannels));
			var framesPerBuffer = renderingSettings.FindPropertyRelative(nameof(AudioRenderingSettings.FramesPerBuffer));
			var sampleRate      = renderingSettings.FindPropertyRelative(nameof(AudioRenderingSettings.SampleRate));

			EditorGUILayout.PropertyField(numChannels);
			EditorGUILayout.PropertyField(framesPerBuffer);
			EditorGUILayout.PropertyField(sampleRate);
		}

		private void RenderZoneVisualizationSettings(SerializedProperty zoneSettings)
		{
			var drawSoundBleedRange      = zoneSettings.FindPropertyRelative(nameof(ZoneVisualizationSettings.drawSoundBleedRange));
			var drawOcclusionFilterRange = zoneSettings.FindPropertyRelative(nameof(ZoneVisualizationSettings.drawOcclusionFilterRange));
			var drawOnlyWhenSelected     = zoneSettings.FindPropertyRelative(nameof(ZoneVisualizationSettings.drawOnlyWhenSelected));
			var boundaryPointSize        = zoneSettings.FindPropertyRelative(nameof(ZoneVisualizationSettings.boundaryPointSize));
			var boundaryPointSpacing     = zoneSettings.FindPropertyRelative(nameof(ZoneVisualizationSettings.boundaryPointSpacing));

			var defaultZoneColor         = zoneSettings.FindPropertyRelative(nameof(ZoneVisualizationSettings.defaultZoneColor));
			var invalidZoneColor         = zoneSettings.FindPropertyRelative(nameof(ZoneVisualizationSettings.invalidZoneColor));
			var soundBleedInColor        = zoneSettings.FindPropertyRelative(nameof(ZoneVisualizationSettings.soundBleedInColor));
			var soundBleedOutColor       = zoneSettings.FindPropertyRelative(nameof(ZoneVisualizationSettings.soundBleedOutColor));
			var occlusionFilteringColor  = zoneSettings.FindPropertyRelative(nameof(ZoneVisualizationSettings.occlusionFilteringColor));

			EditorGUILayout.PropertyField(drawSoundBleedRange);
			EditorGUILayout.PropertyField(drawOcclusionFilterRange);
			EditorGUILayout.PropertyField(drawOnlyWhenSelected);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(boundaryPointSize);
			EditorGUILayout.PropertyField(boundaryPointSpacing);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(defaultZoneColor);
			EditorGUILayout.PropertyField(invalidZoneColor);
			EditorGUILayout.PropertyField(soundBleedInColor);
			EditorGUILayout.PropertyField(soundBleedOutColor);
			EditorGUILayout.PropertyField(occlusionFilteringColor);
		}

		/*private void RenderVerion()
		{
			using (var h = new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label($"Fusion version {FusionPluginInfo.FusionVersion}");
			}
		}*/

		/*private void RenderRemoteStatus(Remoting.FusionRemotingState state)
		{
			if (state == Remoting.FusionRemotingState.fusion_remoting_state_none)
			{
				if (GUILayout.Button("Start Remoting Server", GUILayout.Width(ButtonWidth)))
					Remoting.StartServer();
			}
			else
			{
				if (GUILayout.Button("Stop Remoting Server", GUILayout.Width(ButtonWidth)))
					Remoting.StopServer();
			}
		}*/

	}
}
