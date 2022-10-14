namespace EliasSoftware.Elias4.Common
{
	using System;
	using System.IO;
	using System.Linq;
	#if UNITY_EDITOR
	using UnityEditor;
	#endif
	using UnityEngine;

	[Serializable]
	public struct ZoneVisualizationSettings
	{
		public bool drawSoundBleedRange;
		public bool drawOcclusionFilterRange;
		public bool drawOnlyWhenSelected;

		[Min(0.01f)]
		public float boundaryPointSize;
		[Min(0.01f)]
		public float boundaryPointSpacing;

		public Color defaultZoneColor;
		public Color invalidZoneColor;
		public Color soundBleedInColor;
		public Color soundBleedOutColor;
		public Color occlusionFilteringColor;

		public static ZoneVisualizationSettings CreateDefault()
		{
			return new ZoneVisualizationSettings
			{
				drawSoundBleedRange      = true,
				drawOcclusionFilterRange = false,
				drawOnlyWhenSelected     = true,

				boundaryPointSize        = 0.1f,
				boundaryPointSpacing     = 0.3f,

				defaultZoneColor         = new Color(0.8f, 0.6f, 0f),
				invalidZoneColor         = Color.red,
				soundBleedInColor        = Color.blue,
				soundBleedOutColor       = Color.cyan,
				occlusionFilteringColor  = new Color(0.6f, 0.3f, 0f),
			};
		}
	}

}