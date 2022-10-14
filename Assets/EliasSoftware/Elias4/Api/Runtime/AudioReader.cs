using System;
using System.Runtime.InteropServices;
using EliasSoftware.Elias4.Native;
using EliasSoftware.Elias4.Native.Runtime;
using UnityEngine;

namespace EliasSoftware.Elias4.Runtime {
	
	using EliasSoftware.Elias4.Common;

	[RequireComponent(typeof(AudioSource))]
	public class AudioReader : MonoBehaviour
	{
		private static float[] mainBuffer;
		private static long mainBufferReadIndex;
		private static long mainBufferWriteIndex;
		private static int mainBufferChannels;
		private static long mainBufferDataAvailable => mainBufferWriteIndex - mainBufferReadIndex;

		private AudioSource audioSource;
		public IEliasRuntime Runtime { get; set; }

		private const string GameObjectName = "~EliasOutput";

		public static AudioReader MakeInstance()
		{
			var instance = FindObjectOfType<AudioReader>();
			if (instance)
				return instance;

			var go =
				#if UNITY_EDITOR
					UnityEditor.EditorUtility.CreateGameObjectWithHideFlags(GameObjectName, HideFlags.HideAndDontSave);
				#else
					new GameObject(GameObjectName);
				#endif
			go.AddComponent<AudioSource>();
			return go.AddComponent<AudioReader>();
		}

		public void Destroy()
		{
			if (Application.isPlaying)
				Destroy(gameObject);
			else
				DestroyImmediate(gameObject);
		}

		private void Awake()
		{
			audioSource = GetComponent<AudioSource>();
			DontDestroyOnLoad(gameObject);
		}

		private void OnEnable()
		{
			AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
			OnAudioConfigurationChanged(deviceWasChanged: false);
		}

		void Update() {
			Runtime.UpdateAllSessions(Time.deltaTime);
		}

		private void OnDisable()
		{
			AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
			audioSource.Stop();
		}

		private void OnAudioConfigurationChanged(bool deviceWasChanged)
		{
			var audioConfig = AudioSettings.GetConfiguration();
			var settings = EliasSettings.Instance;

			var channelCount = settings.AudioRendering.NumChannels;
			var framesPerBuffer = Mathf.Max(settings.AudioRendering.FramesPerBuffer, audioConfig.dspBufferSize);

			mainBuffer = new float[framesPerBuffer * channelCount * 10];
			mainBufferReadIndex = 0;
			mainBufferWriteIndex = 0;
			mainBufferChannels = channelCount;

			audioSource.bypassEffects = true;
			audioSource.bypassListenerEffects = true;
			audioSource.bypassReverbZones = true;
			audioSource.spatialize = false;
			audioSource.loop = true;
			audioSource.Play();
		}

		private static void MarshalCopyToRingBuffer(IntPtr source, float[] destination, long destinationStartIndex, int length)
		{
			Debug.Assert(destinationStartIndex >= 0);
			var destLength = destination.Length;
			var beginIndex = (int)(destinationStartIndex % destLength);
			if (beginIndex + length > destLength)
			{
				var splitLength = length - (destLength - beginIndex);
				if (splitLength > 0)
				{
					var sourceOffset = source + sizeof(float) * splitLength;
					Marshal.Copy(sourceOffset, destination, 0, splitLength);
					length = destLength - beginIndex;
				}
			}
			Marshal.Copy(source, destination, beginIndex, length);
		}

		private static void CopyRingBufferToStraightBuffer(float[] source, float[] destination, long sourceStartIndex, int length)
		{
			Debug.Assert(sourceStartIndex >= 0);
			var sourceLength = source.Length;
			var beginIndex = (int)(sourceStartIndex % sourceLength);
			var destinationOffset = 0;
			if (beginIndex + length > sourceLength)
			{
				var splitLength = sourceLength - beginIndex;
				if (splitLength > 0)
				{
					Array.Copy(source, beginIndex, destination, 0, splitLength);
					length = length - splitLength;
					beginIndex = 0;
					destinationOffset = splitLength;
				}
			}
			Array.Copy(source, beginIndex, destination, destinationOffset, length);
		}

		[AOT.MonoPInvokeCallback(typeof(elias_rendered_buffer_callback_t))]
		private static void RenderCallback(elias_rendered_buffer_t buffer, IntPtr user)
		{
			Debug.AssertFormat(buffer.num_channels == mainBufferChannels,
				"Channel mismatch in rendering callback (requested: {0}, buffer: {1}",
				mainBufferChannels, buffer.num_channels);

			var dataLength = buffer.num_frames * buffer.num_channels;
			lock (mainBuffer)
			{
				MarshalCopyToRingBuffer(buffer.samples, mainBuffer, mainBufferWriteIndex, dataLength);
			}

			mainBufferWriteIndex += dataLength;
			if (mainBufferWriteIndex < 0)
			{
				//NOTE(johannes): Quietly ignore overflow.
				mainBufferReadIndex = 0;
				mainBufferWriteIndex = 0;
			}
		}

		private static elias_rendered_buffer_callback_t cb = RenderCallback;

		private void OnAudioFilterRead(float[] data, int channels)
		{
			Debug.AssertFormat(channels == mainBufferChannels, "Channel mismatch in audio filter (requested: {0}, buffer: {1})", channels, mainBufferChannels);

			bool error = false;
			while (mainBufferDataAvailable < data.Length)
			{
				if (Runtime.ProcessBuffer(cb) == false)
				{
					error = true;
					break;
				}
			}
			if (error)
				return;

			lock (mainBuffer)
			{
				CopyRingBufferToStraightBuffer(mainBuffer, data, mainBufferReadIndex, data.Length);
			}

			mainBufferReadIndex += data.Length;
			if (mainBufferReadIndex < 0)
			{
				//NOTE(johannes): Quietly ignore overflow.
				mainBufferReadIndex = 0;
				mainBufferWriteIndex = 0;
			}
		}
	}
}
