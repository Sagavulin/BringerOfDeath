namespace EliasSoftware.Elias4
{
	using UnityEngine;
	using System.Collections.Generic;
	using EliasSoftware.Elias4.Common;

	/// <summary>
	/// A MonoBehaviour which when added to a GameObject provides a way to select a Patch to be instantiated into a Sound Instance.<br>
	/// This class also provides a way to specify sound source related settings.<br>
	/// When spatialization is enabled the source location and orientation is updated automatically.
	/// </summary>
	public class EliasSoundInstance
        : MonoBehaviour
	{
		[Tooltip("The patch to create the sound instance from.")]
		/// <summary>
		/// The patch to create the sound instance from.
		/// </summary>
		public EliasPatch patch;

		[Tooltip("Attenuation settings for the sound source.")]
		/// <summary>
		/// Attenuation settings for the sound source.
		/// </summary>
		public Attenuation attenuation;

		[Tooltip("Amplitude (Sound Volume) and Pitch settings on the sound source.")]
		/// <summary>
		/// Amplitude (Sound Volume) and Pitch settings on the sound source.
		/// </summary>
		public Amplitude amplitude;

		[Tooltip("Enable or disable spatialization.")]
		/// <summary>
		/// Enable or disable spatialization.
		/// </summary>
		public bool spatializationEnabled = true;

		[Tooltip("Defines how the panning of a sound to be in relation to the listener.")]
		/// <summary>
		/// Defines how the panning of a sound to be in relation to the listener.
		/// </summary>
		public Spatialization spatialization;

		[Tooltip("Zone behavior will decide the behavior of the patch if it is placed inside a zone.")]
		/// <summary>
		/// Zone behavior will decide the behavior of the patch if it is placed inside a zone.
		/// </summary>
		public ZoneSoundBleedBehaviour zoneBehaviour;

		private InstanceID sound;
		private Rigidbody body;
		private Dictionary<string, EliasPatchParameter> parameters = new Dictionary<string, EliasPatchParameter>();
		private Dictionary<string, IEnumID> enums = new Dictionary<string, IEnumID>();

		private EliasPatchParameter GetParameter(string paramName, ParamType expectedType)
		{
			EliasPatchParameter param = null;
			if(parameters.TryGetValue(paramName, out param) == false)
			{
				Debug.LogWarningFormat("No parameter named {0} found", paramName);
				return null;
			}

			Debug.AssertFormat(param.ParamType == expectedType,
				"Incorrect value type, expected {0} but got type {1}",
				expectedType,
				param.ParamType);

			return param;
		}

		/// <summary>
		/// Sends a pulse signal corresponding to the provided parameter name.
		/// </summary>
		/// <param name="pulseName">The name of the pulse parameter (as defined on the Patch in Elias Studio).</param>
		public void SendPulse(string pulseName)
		{
			var eliasParam = GetParameter(pulseName, ParamType.Pulse);
			if(eliasParam != null)
				Elias.API.SetSoundParameter(sound, eliasParam.ParameterID, 1);
		}

		/// <summary>
		/// Takes an EliasPatchParameter and sends the corresponding signal (pulse) to the sound instance.
		/// If the provided parameter ID is not the id of a pulse parameter (for example the id of a boolean parameter) no signal gets sent.
		/// </summary>
		/// <param name="parameter">A parameter of type Pulse (ParamType.Pulse)</param>
		public void SendPulse(EliasPatchParameter parameter)
		{
			if(parameter.ParamType != ParamType.Pulse) {
				Debug.LogWarningFormat("The {0} is not a Pulse parameter", parameter.SecondName);
				return;
			}
			var eliasParam = GetParameter(name, ParamType.Pulse);
			if(eliasParam != null)
				Elias.API.SetSoundParameter(sound, eliasParam.ParameterID, 1);
		}

		/// <summary>
		/// Sets a parameter value to the sound instance.
		/// In order to apply the value the value type has to match the type specified in the parameter.
		/// </summary>
		/// <param name="paramName">The name of the parameter to set.</param>
		/// <param name="value">
		/// Any of the following:
		/// <br>  EliasIntValue
		/// <br>  EliasDoubleValue
		/// <br>  EliasEnumValue
		/// <br>  EliasBoolValue
		/// </param>
		public void SetParameter(string paramName, IEliasValueBuilder value) {
			var eliasValue = value.GetEliasValue();
			var eliasParam = GetParameter(paramName, eliasValue.Type);
			if(eliasParam == null)
				return;
			Elias.API.SetSoundParameter(sound, eliasParam.ParameterID, eliasValue);
		}

		/// <summary>
		/// Sets a parameter value to the sound instance.
		/// In order to apply the value the value type has to match the type specified in the parameter.
		/// </summary>
		/// <param name="parameter">An object of type EliasPatchParameter</param>
		/// <param name="value">
		/// Any of the following:
		/// <br>  EliasIntValue
		/// <br>  EliasDoubleValue
		/// <br>  EliasEnumValue
		/// <br>  EliasBoolValue
		/// </param>
		public void SetParameter(EliasPatchParameter parameter, IEliasValueBuilder value)
			=> SetParameter(parameter, value.GetEliasValue());


		/// <summary>
		/// Sets a parameter value to the sound instance.
		/// In order to apply the value the value type has to match the type specified in the parameter.
		/// This might be preferred if the value is created from the code.
		/// </summary>
		/// <param name="parameter">An object of type EliasPatchParameter</param>
		/// <param name="value">
		/// An EliasValue object <br>
		/// Example:<br>
		/// <code>
		/// var amount = EliasValue.CreateInteger(101);
		/// soundInstance.SetParameter(dalmatians, amount);
		/// </code>
		/// </param>
		public void SetParameter(EliasPatchParameter parameter, EliasValue value) {
			if(parameter.ParamType == ParamType.Empty) {
				Debug.LogWarningFormat("The parameter {0} has undefined type", parameter.SecondName);
				return;
			}

			if(value.Type == ParamType.Empty) {
				Debug.LogWarningFormat("The value has undefined type",
					parameter.SecondName);
				return;
			}

			if(parameter.ParamType != value.Type) {
				Debug.LogWarningFormat("The parameter {0} of type {1} that does not match the value type {2}",
					parameter.SecondName, parameter.ParamType, value.Type);
				return;
			}

			Elias.API.SetSoundParameter(sound, parameter.ParameterID, value);
		}

		/// <summary>
		/// Does a lookup to find the Enum ID matching the provided name.
		/// </summary>
		/// <param name="enumName">The name of the Enum</param>
		/// <returns>An object of type IEnumID or null if no match is found.</returns>
		public IEnumID GetEnumID(string enumName) {
			IEnumID id;
			if(enums.TryGetValue(enumName, out id) == false)
				return id;
			Debug.LogWarningFormat("No enum named {0} was found", enumName);
			return null;
		}

		/// <summary>
		/// Performs a lookup of an IEnumValueID based on the name of the Enum and the name of the EnumValue.
		/// </summary>
		/// <param name="enumName">The name of the Enum</param>
		/// <param name="valueName">The name of the Enum Value</param>
		/// <returns>The ID of the enum value. If the name of the Enum or its value is not found then null is returned.</returns>
		public IEnumValueID LookupEnumValueID(string enumName, string valueName) {
			var enumId = GetEnumID(enumName);
			if(enumId == null)
				return null;
			var value = Elias.API.Assets.GetEnumValueID(enumId, valueName);
			Debug.AssertFormat(value != null, "value {0} not found");
			return value;
		}

		private void Awake()
		{
			body = GetComponent<Rigidbody>();
		}

		private void OnEnable()
		{
			Debug.Assert(patch.PatchID.IsValid);
			sound = Elias.API.CreateSoundInstance(patch.PatchID);
			Debug.Assert(sound.IsValid);
			Elias.API.SetSoundVelocity(sound, Vector3.zero, Vector3.zero);
			Elias.API.SetSoundLocation(sound, transform.position, transform.rotation);
			Elias.API.SetSoundSpread(sound, 0.0f, 100.0f, 0.01f, 0.25f);
			Elias.API.SetSoundSpatialization(sound, spatializationEnabled);

			var paramArray = patch.Parameters;
			int len = paramArray.Length;

			for(int i = 0; i < len; i++) {
				var paramName = paramArray[i].SecondName;
				parameters[paramName] = paramArray[i];
				if(paramArray[i].ParamType == ParamType.EnumValue) {
					var enumName = paramArray[i].Data;
					var id = Elias.API.Assets.GetEnumID(paramArray[i].Data);
					if(id.IsValid == false)
						continue;
					enums.Add(enumName, id);
				}
			}
		}

		private void OnDisable()
		{
			Elias.API.DestroySoundInstance(sound);
			parameters.Clear();
			enums.Clear();
		}

		private void Start()
		{
			UpdateEliasProperties();
		}

		private void Update()
		{
			if ( gameObject.isStatic )
				return;

			if( spatializationEnabled == false )
				return;

			Elias.API.SetSoundTransform(sound, transform);

			if (body) {
				Elias.API.SetSoundVelocity(sound, body.velocity, body.angularVelocity);
			}
		}

		/// <summary>
		/// This method sends all the settings (Amplitude, Spatialization etc) to the elias audio engine and is currently called automatically when the game object is enabled.
		/// If any of the settings on a sound instance are modified after the sound instance has been enabled the audio engine will not know about those changes unless this method is called.
		/// Note: This requirement is very likely to change in the near future.
		/// </summary>
		public void UpdateEliasProperties() {

			Elias.API.SetSoundSpatialization(sound, spatializationEnabled);

			Elias.API.SetSoundTransform(sound, transform);

			Elias.API.SetSoundSpread(sound,
				spatialization.spreadStartDistance,
				spatialization.spreadEndDistance,
				Mathf.Clamp( Mathf.Deg2Rad * spatialization.spreadStartAngle, 0f, 2f * Mathf.PI),
				Mathf.Clamp( Mathf.Deg2Rad * spatialization.spreadEndAngle, 0f, 2f * Mathf.PI));

			Elias.API.SetSoundConeSource(sound,
				spatialization.sourceShape.GetEffectiveInnerAngleRadians(),
				spatialization.sourceShape.GetEffectiveOuterAngleRadians(),
				spatialization.sourceShape.GetEffectiveOuterAmplitudeGain(),
				spatialization.sourceShape.disableAngularAttenuationWithStartRadius == false);

			Elias.API.SetSoundMinChannelSpread(sound,
				Mathf.Clamp( Mathf.Deg2Rad * spatialization.minimumChannelSpreadDegrees, 0f, 2f * Mathf.PI));

			Elias.API.SetSoundStereoWidth(sound, spatialization.stereoWidth);

			Elias.API.SetSoundMasterAmplitude(sound, amplitude.amplitude);

			Elias.API.SetSoundMasterPitch(sound, amplitude.pitch);

			Elias.API.SetSoundDistanceAttenuation(sound,
				attenuation.falloffEndAmplitude,
				attenuation.falloffStartDistance,
				attenuation.falloffEndDistance);

			Elias.API.SetSoundZoneBleed(sound, zoneBehaviour);

			Elias.API.SetSoundReverb(sound,
				spatialization.reverb.distanceMin,
				spatialization.reverb.distanceMax,
				spatialization.reverb.reverbMin,
				spatialization.reverb.reverbMax);
		}

		private void DelayUpdate()
		{
			if(spatialization == null)
				return;
			spatialization.spreadStartDistance = Mathf.Clamp(spatialization.spreadStartDistance, 0.0f, spatialization.spreadEndDistance);
			spatialization.spreadEndDistance = Mathf.Max(spatialization.spreadEndDistance, spatialization.spreadStartDistance);
			UpdateEliasProperties();
		}

		#if UNITY_EDITOR
		[ExecuteInEditMode]
		private void OnValidate() {
			UnityEditor.EditorApplication.delayCall +=
				() => {
					if(this == null || Elias.IsInitialized == false)
						return;
					DelayUpdate();
				};
		}
		#endif

	}
}
