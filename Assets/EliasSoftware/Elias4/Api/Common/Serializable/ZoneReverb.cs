namespace EliasSoftware.Elias4.Common
{
	using System;
	using UnityEngine;

	[Serializable]
	/// <summary>
	/// EliasZone IR filter settings.
	/// </summary>
	public class ZoneReverb
	{
		/// <summary>
		/// The id of the filter.
		/// </summary>
		public EliasIRID impulseResponse;

		[Range(0f, 1f), Tooltip("Specify to what degree the sound should be filtered.")]
		/// <summary>
		/// Defines to what degree the sound should be filtered.
		/// </summary>
		public float sendProportion = 0.5f;
	}
}
