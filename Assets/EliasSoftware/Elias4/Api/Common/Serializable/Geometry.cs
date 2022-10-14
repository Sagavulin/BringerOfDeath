namespace EliasSoftware.Elias4.Common
{
	using System;
	using UnityEngine;

	[Serializable]
	/// <summary>
	/// Defines the zone geometry for an EliasZone.
	/// </summary>
	public class Geometry
	{
		[Min(0f), Tooltip("The height of the zone volume.")]
		/// <summary>
		/// The height of the zone volume.
		/// </summary>
		public float height = 1f;

		[Tooltip("An array of vertex positions defining the zone geometry.")]
		/// <summary>
		/// An array of vertex positions defining the zone geometry.
		/// </summary>
		public Vector2[] vertices = new[] {
			new Vector2(-1f, -1f),
			new Vector2( 1f, -1f),
			new Vector2( 1f,  1f),
			new Vector2(-1f,  1f),
		};
	}
}
