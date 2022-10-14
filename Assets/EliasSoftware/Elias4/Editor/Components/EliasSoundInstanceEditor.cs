namespace EliasSoftware.Elias4.Editor
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	using UnityEditor;
	using UnityEngine;
	using EliasSoftware.Elias4.Common;

	[CustomEditor(typeof(EliasSoundInstance))]
	public class EliasSoundInstanceEditor : Editor
	{
		bool showAvailableParameters = false;
		SerializedProperty patchIdProp;
		//SerializedProperty autoPlay;
		SerializedProperty amplitude;
		SerializedProperty zoneBehaviour;
		SerializedProperty attenuation;
		SerializedProperty spatialization;
		SerializedProperty spatializationEnabled;
        IEnumerable<(string, IParameterID, ParamType, string)> parameters;
		string patchName = "";
		IPatchID patchId = EliasID.Invalid;
		PreviewSession previewSession = null;

		private static EliasEditor EditorAPI => Elias.API as EliasEditor;
		private bool PreviewDisabled => Elias.IsInitialized == false || EditorAPI.IsPreviewEnabled() == false;

		private void OnEnable()
		{
			//Debug.Log("Enable");
			patchIdProp     		= serializedObject.FindProperty(nameof(EliasSoundInstance.patch));
			spatializationEnabled 	= serializedObject.FindProperty(nameof(EliasSoundInstance.spatializationEnabled));
			//autoPlay       		= serializedObject.FindProperty(nameof(EliasSoundInstance.autoPlay));
			attenuation    			= serializedObject.FindProperty(nameof(EliasSoundInstance.attenuation));
			zoneBehaviour  			= serializedObject.FindProperty(nameof(EliasSoundInstance.zoneBehaviour));
			amplitude      			= serializedObject.FindProperty(nameof(EliasSoundInstance.amplitude));
			spatialization 			= serializedObject.FindProperty(nameof(EliasSoundInstance.spatialization));

			patchId  = EliasSerializableID.FirstIDFromProperty(patchIdProp);

			if( Application.isPlaying == false && Elias.IsInitialized ) {
				patchName = EditorAPI.Assets.GetName(patchId);
				Debug.Assert(previewSession == null);
				if(EditorAPI.IsPreviewEnabled()) {
					parameters = EditorAPI.EditorAssets.EnumeratePatchParameters(patchId);
					previewSession = EditorAPI.CreatePreviewSession();
					EditorApplication.update += Update;
				}
			}
		}

		private void OnDisable() {
			if(previewSession != null){
				previewSession.Cancel();
				EditorApplication.update -= Update;
				previewSession = null;
			}
		}

		private void OnDestroy() {
			if(previewSession != null){
				previewSession.Cancel();
				EditorApplication.update -= Update;
			}
			previewSession = null;
		}

		private void Update() {
			if(previewSession != null)
				previewSession.Update();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var lastPatchId = patchId;
			patchIdProp = serializedObject.FindProperty(nameof(EliasSoundInstance.patch));
			patchId  = EliasSerializableID.FirstIDFromProperty(patchIdProp);

			EditorGUILayout.Space();
			Fields();
			EditorGUILayout.Space();

			if(Elias.IsInitialized) {
				if ( lastPatchId.Equals(patchId) == false ) {
					patchName = EditorAPI.Assets.GetName(patchId);
				}

				if( parameters == null || parameters.Count() == 0 )
					parameters = EditorAPI.EditorAssets.EnumeratePatchParameters(patchId);

				if ( previewSession == null ) {
					previewSession = EditorAPI.CreatePreviewSession();
				}
			}

			AvailableParameters();

			serializedObject.ApplyModifiedProperties();
		}

		private void AvailableParameters()
		{
			if( Elias.IsInitialized == false || EditorAPI.IsPreviewEnabled() == false ) {
				return;
			}

            showAvailableParameters = EditorGUILayout.BeginFoldoutHeaderGroup(showAvailableParameters, "Available Pulses");
            if ( showAvailableParameters && parameters != null )
            {
				EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
                foreach (var param in parameters)
                {
					if(param.Item3 != ParamType.Pulse)
						continue;

					if(param.Item3 == ParamType.Pulse) {
						if (GUILayout.Button(param.Item1)) {
							previewSession.Play(patchId, param.Item2, param.Item3, "1");
						}
					}

                    EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private void Fields()
		{
            EditorGUILayout.PropertyField(patchIdProp);
			EditorGUILayout.PropertyField(attenuation);
			EditorGUILayout.PropertyField(zoneBehaviour);
			EditorGUILayout.PropertyField(amplitude);
			EditorGUILayout.PropertyField(spatializationEnabled);
			if(spatializationEnabled.boolValue)
				EditorGUILayout.PropertyField(spatialization);
		}

		static Mesh innerCone = null;
		static Mesh outerCone = null;
		[DrawGizmo(GizmoType.Selected)]
		static void DrawGizmosSelected(EliasSoundInstance soundInstance, GizmoType gizmoType)
		{
			var position = soundInstance.transform.position;
			var rotation = soundInstance.transform.rotation;
			var innerConeAngleDegrees = soundInstance.spatialization.sourceShape.innerConeSizeAngle;
			var outerConeAngleDegrees = soundInstance.spatialization.sourceShape.outerConeSizeAngle;

			var innerColor = new Color(0.3f, 0.1f, 0.8f);
			var outerColor = new Color(0.9f, 0.2f, 0.2f);
			var attenStartColor = Color.yellow;
			var attenEndColor = new Color(1.0f, 0.5f, 0.0f);
			var disabledColor = new Color(0.9f, 0.9f, 0.9f);

			//const float innerOpacity = 0.3f;
			//const float outerOpacity = 0.3f;
			if ( soundInstance.spatializationEnabled == false
				|| soundInstance.attenuation.falloffEndDistance <= 0.0f )
			{
				Gizmos.color = disabledColor;
				Gizmos.DrawWireSphere(position, 0.5f);
				return;
			}

			if (!Mathf.Approximately(innerConeAngleDegrees, 0.0f) && !Mathf.Approximately(innerConeAngleDegrees, 360.0f))
			{
				Gizmos.color = innerColor;
				if (innerCone == null)
					innerCone = new Mesh();
				CreateViewCone(innerCone, innerConeAngleDegrees, soundInstance.attenuation.falloffEndDistance);
				Gizmos.DrawWireMesh(innerCone, position, rotation);
			}

			if (!Mathf.Approximately(outerConeAngleDegrees, 0.0f) && !Mathf.Approximately(outerConeAngleDegrees, 360.0f))
			{
				if (outerCone == null)
					outerCone = new Mesh();
				Gizmos.color = outerColor;
				CreateViewCone(outerCone, outerConeAngleDegrees, soundInstance.attenuation.falloffEndDistance);
				Gizmos.DrawWireMesh(outerCone, position, rotation);
				//Graphics.DrawMeshNow(outerCone, position, rotation);
			}
			if (outerConeAngleDegrees > 0.0f)
			{
				if (outerConeAngleDegrees < 360.0f)
				{ }
				else
				{

				}
			}
			Gizmos.color = attenStartColor;
			Gizmos.DrawWireSphere(position, soundInstance.attenuation.falloffStartDistance);
			Gizmos.color = attenEndColor;
			Gizmos.DrawWireSphere(position, soundInstance.attenuation.falloffEndDistance);
		}

		public static void CreateViewCone(Mesh mesh, float aAngle, float aDistance, int numFaces = 30)
		{
			aAngle = aAngle * 0.5f;
			if (mesh == null)
			{
				mesh = new Mesh();
			}
			else
			{
				mesh.Clear();
			}
			Vector3[] verts = new Vector3[numFaces + 1];
			Vector3[] normals = new Vector3[verts.Length];
			int[] tris = new int[numFaces * 3];
			Vector3 a = Quaternion.Euler(-aAngle, 0, 0) * Vector3.forward * aDistance;
			Vector3 n = Quaternion.Euler(-aAngle, 0, 0) * Vector3.up;
			Quaternion step = Quaternion.Euler(0, 0, 360f / numFaces);
			verts[0] = Vector3.zero;
			normals[0] = Vector3.back;
			for (int i = 0; i < numFaces; i++)
			{
				normals[i + 1] = n;
				verts[i + 1] = a;
				a = step * a;
				n = step * n;
				tris[i * 3] = 0;
				tris[i * 3 + 1] = (i + 1) % numFaces + 1;
				tris[i * 3 + 2] = i + 1;
			}
			mesh.vertices = verts;
			mesh.normals = normals;
			mesh.triangles = tris;
			mesh.RecalculateBounds();
		}
	}
}
