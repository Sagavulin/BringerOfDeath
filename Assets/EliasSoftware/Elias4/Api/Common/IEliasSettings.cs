using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Common
{

	public interface IEliasPaths {
		public string ProjectFilePath { get; }
		public string MusicProjectFilePath { get; }
		public string AssetMetadataFilePath { get; }
		public bool AutoLoadMusicProject { get; }
	}

	public interface IEliasSettings :
		IEliasPaths
	{
		public AudioRenderingSettings AudioRendering { get; }
	}
    
}