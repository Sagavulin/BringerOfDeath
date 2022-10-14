using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Common {
    public class EliasSettingsObject :
		IEliasPaths
	{
		public EliasSettingsObject(string projectPath,
			string assetMetadataPath,
			string musicProjectFilePath=null)
		{
			this.autoLoadMusicProject = false;
			this.musicProjectFilePath = string.Empty;

			this.assetMetadataFilePath = assetMetadataPath;
			this.projectFilePath = projectPath;

			if(musicProjectFilePath != null) {
				this.autoLoadMusicProject = true;
				this.musicProjectFilePath = musicProjectFilePath;
			}
		}

		public EliasSettingsObject(IEliasPaths other)
		{
			this.autoLoadMusicProject = other.AutoLoadMusicProject;
			this.musicProjectFilePath = other.ProjectFilePath;

			this.assetMetadataFilePath = other.AssetMetadataFilePath;
			this.projectFilePath = other.ProjectFilePath;
		}

		public string ProjectFilePath => projectFilePath;
		public string MusicProjectFilePath => musicProjectFilePath;
		public bool AutoLoadMusicProject => autoLoadMusicProject;
		public string AssetMetadataFilePath => assetMetadataFilePath;

		public string assetMetadataFilePath = string.Empty;
		public string projectFilePath = string.Empty;
		public string musicProjectFilePath = string.Empty;
		public bool autoLoadMusicProject = false;
	}
}