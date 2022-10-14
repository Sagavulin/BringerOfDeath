namespace EliasSoftware.Elias4.Common
{
	using System;
	using UnityEngine;

	[Serializable]
	/// <summary>
	/// Sound source falloff configuration.
	/// </summary>
	public class Attenuation
	{
		[SerializeField, Min(0.0f), Tooltip("At what distance the patch should start to drop off in volume")]
		private float m_FalloffStartDistance = 2.0f;

		/// <summary>
		/// At what distance the sound should start to drop off in volume.
		/// Should be less than falloffEndDistance.
		/// </summary>
		/// <value>A distance value</value>
		public float falloffStartDistance
		{
			get => m_FalloffStartDistance;
			set => Mathf.Clamp(value, 0.0f, m_FalloffEndDistance);
		}

		[SerializeField, Min(0.0f), Tooltip("How far the sound of the patch will reach. If you pass this point you will no longer be able to hear it if Falloff End Amplitude is set to 0.")]
		private float m_FalloffEndDistance = 30.0f;

		/// <summary>
		/// The maximum distance the sound will reach.
		/// If you pass this point you will no longer be able to hear it if falloffEndAmplitude is set to 0.
		/// </summary>
		/// <value>A distance value</value>
		public float falloffEndDistance
		{
			get => m_FalloffEndDistance;
			set => Mathf.Clamp(value, 0.0f, m_FalloffEndDistance);
		}

		[SerializeField, Range(0.0f, 1.0f), Tooltip("A value between 0.0 and 1.0 where 0.0 is no gain and 1.0 is 100% gain. Use this if you want the sound to extend endlessly beyond the Falloff End Distance.")]
		private float m_FalloffEndAmplitude = 0.0f;
		/// <summary>
		/// Use this if you want the sound to extend endlessly beyond the falloffEndDistance.
		/// </summary>
		/// <value>A value between 0.0 and 1.0 where 0.0 is no gain and 1.0 is 100% gain.</value>
		public float falloffEndAmplitude
		{
			get => m_FalloffEndAmplitude;
			set => Mathf.Clamp(value, 0.0f, 1.0f);
		}
	}
}
