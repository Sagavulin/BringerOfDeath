namespace EliasSoftware.Elias4.Common
{
	using System;
	using UnityEngine;

	[Serializable]
	/// <summary>
	/// Defines the shape of a sound instance.
	/// Enables changing from a sphere source to a cone source (direct your sound in a specific direction).
	/// </summary>
	public class ConeSourceConfig
	{
		[Range(0.0f, 360.0f), Tooltip("The angle (the volume/size) of the inner cone.")]
		/// <summary>
		/// The angle (the volume/size) of the inner cone.
		/// </summary>
		public float innerConeSizeAngle = 360.0f;

		[Range(0.0f, 360.0f), Tooltip("The angle (the volume/size) of the outer cone.")]
		/// <summary>
		/// The angle (the volume/size) of the outer cone.
		/// </summary>
		public float outerConeSizeAngle = 360.0f;

		[Range(0.0f, 1.0f), Tooltip("The source's amplitude inside the outer cone.")]
		/// <summary>
		/// The source's amplitude when the listener is inside the outer cone.
		/// </summary>
		public float outerConeAmplitude = 0.0f;

		[Tooltip("No angular attenuation within Falloff Start Distance (Attenuation)")]
		public bool disableAngularAttenuationWithStartRadius = false;


		/// <summary>
		/// Helper method for checking if the source is spherical or not.
		/// </summary>
		public bool IsFullSphere() => innerConeSizeAngle >= 360.0f;

		/// <summary>
		/// Helper method for checking if the source is cone shaped or not.
		/// </summary>
		public bool IsCone() => !IsFullSphere();

		/// <summary>
		/// Getter that ensures consistent (valid) inner angle in radians.
		/// </summary>
		public float GetEffectiveInnerAngleRadians() => Mathf.Clamp(Mathf.Deg2Rad * innerConeSizeAngle, 0f, 2f * Mathf.PI);

		/// <summary>
		/// Getter that ensures consistent (valid) outer angle in radians.
		/// </summary>
		public float GetEffectiveOuterAngleRadians() => Mathf.Clamp(Mathf.Deg2Rad * Mathf.Max(outerConeSizeAngle, innerConeSizeAngle), 0f, 2f * Mathf.PI);

		/// <summary>
		/// Getter that ensures consistent (valid) outer cone amplitude gain.
		/// </summary>
		public float GetEffectiveOuterAmplitudeGain() => Mathf.Clamp01(outerConeAmplitude);
	}
}
