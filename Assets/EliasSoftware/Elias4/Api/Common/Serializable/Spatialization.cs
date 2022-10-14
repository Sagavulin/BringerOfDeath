namespace EliasSoftware.Elias4.Common
{
	using System;
	using UnityEngine;

	[Serializable]
	/// <summary>
	/// Defines how the panning of a sound to be in relation to the listener.
	/// </summary>
	public class Spatialization
	{
		[SerializeField, Range(0.0f, 1.0f), Tooltip("Adjust the stereo width on a stereo sound. Set it to 0 to make it a mono sound.")]
		private float m_StereoWidth = 1.0f;
		
		/// <summary>
		/// Adjust the stereo width on a stereo sound.
		/// </summary>
		/// <value>A float value between 0.0 and 1.0</value>
		public float stereoWidth
		{
			get => m_StereoWidth;
			set => Mathf.Max(0.0f, value);
		}

		[SerializeField, Min(0.0f), Tooltip("At what distance should the sound start to fold, at the point where the direction of the sound starts to be apparent.")]
		private float m_SpreadStartDistance = 200.0f;
		
		/// <summary>
		/// The distance the sound should start to fold, at the point where the direction of the sound starts to be apparent.
		/// </summary>
		/// <value>A positive float value (or 0.0)</value>
		public float spreadStartDistance
		{
			get => m_SpreadStartDistance;
			set => Mathf.Max(0.0f, value);
		}

		[SerializeField, Min(0.0f), Tooltip("At what distance is the sound completely panned to one side (if the angle is right).")]
		private float m_SpreadEndDistance = 3000.0f;

		/// <summary>
		/// The distance where the sound should be completely panned to one side (if the angle is right).
		/// </summary>
		/// <value>A positive float value (or 0.0)</value>
		public float spreadEndDistance
		{
			get => m_SpreadEndDistance;
			set => Mathf.Max(0.0f, value);
		}

		[SerializeField, Range(0.0f, 360.0f), Tooltip("At what angle from the listener should the panning start to react.")]
		private float m_SpreadStartAngle = 45.0f;
		/// <summary>
		/// The angle at which the listener panning should start to react.
		/// </summary>
		/// <value></value>
		public float spreadStartAngle
		{
			get => m_SpreadStartAngle;
			set => Mathf.Clamp(value, 0.0f, 360.0f);
		}

		[SerializeField, Range(0.0f, 360.0f), Tooltip("At what angle from the listener should the panning effect be fully applied.")]
		private float m_SpreadEndAngle = 10.0f;

		/// <summary>
		/// The angle between the listener's forward vector and the vector to the source where the panning effect should be fully applied.
		/// </summary>
		/// <value></value>
		public float spreadEndAngle
		{
			get => m_SpreadEndAngle;
			set => Mathf.Clamp(value, 0.0f, 360.0f);
		}

		[SerializeField, Range(0.0f, 360.0f), Tooltip("The minimum channel spread angle in degrees.")]
		private float m_MinimumChannelSpreadDegrees = 0.0f;
		
		/// <summary>
		/// The minimum channel spread angle in degrees.
		/// </summary>
		public float minimumChannelSpreadDegrees
		{
			get => m_MinimumChannelSpreadDegrees;
			set => Mathf.Clamp(value, 0.0f, 360.0f);
		}

		[SerializeField]
		/// <summary>
		/// Defines the shape of the sound source.
		/// </summary>
		public ConeSourceConfig sourceShape = new ConeSourceConfig();

		[SerializeField]
		/// <summary>
		/// Defines how the emitted sound should react to the reverb.
		/// </summary>
		public SoundReverbSettings reverb = new SoundReverbSettings();
	}
}
