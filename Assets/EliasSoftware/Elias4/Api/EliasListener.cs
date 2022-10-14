using UnityEngine;

namespace EliasSoftware.Elias4
{
	using EliasSoftware.Elias4.Common;

	/// <summary>
	/// This MonoBehaviour corresponds to the ears in the game world.
	/// In order for any sound to be heard there must be an active EliasListener in the scene.
	/// </summary>
	public class EliasListener
        : MonoBehaviour
	{
		private ListenerID listener;

		[SerializeField] bool spatilizationEnabled = true;

		/// <summary>
		/// Enable / Disable 3D Spatialization for this listener.
		/// </summary>
		/// <value>Spatialization Enabled</value>
		public bool Spatialization {
			get => spatilizationEnabled;
			set {
				spatilizationEnabled = value;
				if( Elias.IsInitialized )
					Elias.API.SetListenerSpatialization(listener, spatilizationEnabled);
			}
		}

		/// <summary>
		/// Enable / Disable IR filter processing for this listener.
		/// </summary>
		[SerializeField] private bool receiveReverbEnabled = true;

		/// <summary>
		/// Enable / Disable IR filter processing for this listener.
		/// </summary>
		/// <value>Enable (true) / Disable (false) IR filter processing.</value>
		public bool RevciveReverbEnabled {
			get => receiveReverbEnabled;
			set {
				receiveReverbEnabled = value;
				if( Elias.IsInitialized )
					Elias.API.SetListenerReverbMode(listener, receiveReverbEnabled);
			}
		}

		private void Awake()
		{
			listener = Elias.API.CreateListener();
			Elias.API.SetListenerSpatialization(listener, true);
		}


		private void OnDestroy()
		{
			Elias.API.DestroyListener(listener);
		}

		private void Update()
		{
			if (gameObject.isStatic)
				return;
			Elias.API.SetListenerTransform(listener, transform);
			Elias.API.SetDeferredCommandsSpecified();
		}

		private void LateUpdate()
			=> Elias.API.CommitDeferredCommands();

		private void UpdateEliasProperties()
		{
			Elias.API.SetListenerReverbMode(listener, receiveReverbEnabled);
			Elias.API.SetListenerSpatialization(listener, spatilizationEnabled);
		}

		#if UNITY_EDITOR
		[ExecuteInEditMode]
		private void OnValidate()
		{
			UnityEditor.EditorApplication.delayCall += () => {
				if(this == null || Elias.IsInitialized == false)
					return;
				UpdateEliasProperties();
			};
		}
		#endif
	}
}
