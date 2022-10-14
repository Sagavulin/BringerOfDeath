namespace EliasSoftware.Elias4.Common
{
	using System;
	using UnityEngine;

	[Serializable]
	/// <summary>
	/// General sound source settings.
 	/// </summary>
	public class Amplitude
	{
		[SerializeField, Range(0.0f, 1.0f), Tooltip("The amplitude as a value between 0.0 and 1.0 (where 1.0 is the default amplitude of the source and 0.0 is silence).")]
		private float m_Amplitude = 1.0f;

		/// <summary>
		/// Sets or gets the current amplitude (where 1.0 is the default amplitude of the source and 0.0 is silence).
		/// </summary>
		/// <value>A float value between 0.0 and 1.0</value>
		public float amplitude
		{
			get => m_Amplitude;
			set => Mathf.Max(0.0f, value);
		}

        [SerializeField, Range(0.125f, 8.0f), Tooltip("The pitch modifier on any emitted sounds. A value of 1.0 means that the sound is not modified.")]
        private float m_Pitch = 1.0f;
		
		/// <summary>
		/// Sets or gets the current pitch modifier for the sound source.
		/// </summary>
		/// <value>A float value between 0.125 and 8.0</value>
		public float pitch
		{
			get => m_Pitch;
			set => Mathf.Clamp(value, 0.125f, 8.0f);
		}
	}
}
