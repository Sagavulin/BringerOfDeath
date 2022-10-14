namespace EliasSoftware.Elias4.Common
{
	using System;
	using UnityEngine;

	[Serializable]
	/// <summary>
	/// Defines how the inbound and outbound sounds should be heard when the listener is inside or outside the zone.
	/// </summary>
	public class SoundBleed
	{
		[Serializable]
		/// <summary>
		/// Defines how the inbound and outbound sounds should be heard when the listener is inside or outside the zone.
		/// </summary>
		public class ZoneOcclusion
		{
			[Tooltip("Toggle On/Off")]
			/// <summary>
			/// Enable or disable Occlusion.
			/// </summary>
			public bool enabled;

			[Range(0f, 20000f), Tooltip("The frequency of the occlusion filter (a low pass filter)")]
			/// <summary>
			/// The frequency of the occlusion filter (a low pass filter).
			/// </summary>
			public float endFrequency = 1000f;

			[Range(0f, 1f), Tooltip("Occlusion filter send proportion")]
			/// <summary>
			/// Occlusion filter send proportion.
			/// </summary>
			public float endDryWet = 1f;

			[Min(0f), Tooltip("The distance from the zone wall from where the effect should be disabled.")]
			/// <summary>
			/// The distance from the zone wall from where the effect should be disabled.
			/// </summary>
			public float endDistance = 0f;

			[Range(0f, 1f), Tooltip("The amplitude occluded sounds should drop to.")]
			/// <summary>
			/// The amplitude occluded sounds should drop to.
			/// </summary>
			public float endAmplitude = 0f;

			/// <summary>
			/// Helper for checking if attenuation is active.
			/// </summary>
			public bool HasAnyAttenuation => endAmplitude < 1f;
		}

		/// <summary>
		/// Inbound occlusion settings (When listener is inside the zone and the source is outside).
		/// </summary>
		public ZoneOcclusion inbound = new ZoneOcclusion();
		
		/// <summary>
		/// Outbound occlusion settings (When listener is outside the zone and the source is inside).
		/// </summary>
		public ZoneOcclusion outbound = new ZoneOcclusion();
	}
}
