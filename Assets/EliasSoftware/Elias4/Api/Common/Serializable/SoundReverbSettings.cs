namespace EliasSoftware.Elias4.Common
{
	using System;
	using UnityEngine;

	[Serializable]
	/// <summary>
	/// Defines how the emitted sound should react to the reverb.
	/// Depending on the distance it will crossfade the Dry/Wet signals between the Reverb minimum and maximum values.
	/// </summary>
	public class SoundReverbSettings
	{
		[Min(0.0f), Tooltip("The distance at which the sound instance should be heard playing through the Reverb with the Dry/Wet value set in Reverb Min.")]
		/// <summary>
		/// The distance at which the sound instance should be heard playing through the Reverb with the Dry/Wet value set in Reverb Min.
		/// </summary>
		public float distanceMin = 0.0f;

		[Min(0.0f), Tooltip("The distance at which the sound instance should be heard playing through the Reverb with the Dry/Wet value set in Reverb Max.")]
		/// <summary>
		/// The distance at which the sound instance should be heard playing through the Reverb with the Dry/Wet value set in Reverb Max.
		/// </summary>
		public float distanceMax = 0.0f;

		[Range(0.0f, 1.0f), Tooltip("The minimum Dry/Wet value of the reverb.")]
		/// <summary>
		/// The minimum Dry/Wet value of the reverb.
		/// </summary>
		public float reverbMin = 1.0f;

		[Range(0.0f, 1.0f), Tooltip("The maximum Dry/Wet value of the reverb.")]
		/// <summary>
		/// The maximum Dry/Wet value of the reverb.
		/// </summary>
		public float reverbMax = 1.0f;
	}
}
