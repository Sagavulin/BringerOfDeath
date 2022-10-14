namespace EliasSoftware.Elias4.Common {

	using System;
	using System.IO;
	using System.Linq;
	#if UNITY_EDITOR
	using UnityEditor;
	#endif
	using UnityEngine;

    [Serializable]
	/// <summary>
	/// Elias audio playback settings.
	/// </summary>
	public struct AudioRenderingSettings
	{
		[Range(1, 8), Tooltip("The number of output channels.")]
		/// <summary>
		/// The number of output channels.
		/// </summary>
		public int NumChannels;

		[Range(256, 8192), Tooltip("The number of frames contained in a buffer.")]
		/// <summary>
		/// The number of frames per buffer. A (power of two) value between 256 and 8192.
		/// </summary>
		public int FramesPerBuffer;

		[Range(16000, 96000)]
		/// <summary>
		/// The output sample rate.
		/// </summary>
		public int SampleRate;

		public AudioRenderingSettings(int numChannels, int framesPerBuffer, int sampleRate)
		{
			NumChannels     = numChannels;
			FramesPerBuffer = framesPerBuffer;
			SampleRate      = sampleRate;
		}
	};
}