namespace EliasSoftware.Elias4.Editor
{
    using EliasSoftware.Elias4;
	using UnityEditor;
	using UnityEditor.EditorTools;
	using UnityEngine;

	[EditorTool("Edit EliasZone", typeof(EliasZone))]
	public class EliasZoneTool : EditorTool
	{
		//[SerializeField]
		//Texture2D toolIcon;

		private GUIContent iconContent;

		private const float HandleSize = 0.1f;

		public override GUIContent toolbarIcon => iconContent;

		private void OnEnable()
		{
			/*iconContent = new GUIContent("Edit HarmonyZone", toolIcon,
@"Edit HarmonyZone

- Hold Ctrl to add vertices.");*/
			iconContent = EditorGUIUtility.IconContent("d_CustomTool",
@"|Edit EliasZone

- Hold Ctrl to add vertices."
			);
		}

		public override void OnToolGUI(EditorWindow window)
		{
			var serializedObject = new SerializedObject(target);
			var geometry = serializedObject.FindProperty("geometry");
			var vertices = geometry.FindPropertyRelative("vertices");

			serializedObject.Update();

			Vector3 cameraLocalPos;

			{
				var component = (Component)target;
				Handles.matrix = component.transform.localToWorldMatrix;

				var camera = SceneView.lastActiveSceneView.camera;
				cameraLocalPos = component.transform.InverseTransformPoint(camera.transform.position);
			}

			var plane = new Plane(Vector3.up, Vector3.zero);

			var vertexCount = vertices.arraySize;
			if (vertexCount >= 2)
			{
				var points = new Vector3[vertexCount];
				for (int vertexIndex = 0; vertexIndex < vertexCount; ++vertexIndex)
				{
					var vertex = vertices.GetArrayElementAtIndex(vertexIndex);
					var position2D = vertex.vector2Value;
					var position3D = new Vector3(position2D.x, 0f, position2D.y);
					var handleSize = HandleUtility.GetHandleSize(position3D) * HandleSize;

					using (var change = new EditorGUI.ChangeCheckScope())
					{
						position3D = Handles.FreeMoveHandle(position3D, Quaternion.identity, handleSize, Vector3.one, Handles.DotHandleCap);

						if (change.changed)
						{
							var planeRay = new Ray(cameraLocalPos, (position3D - cameraLocalPos).normalized);
							if (plane.Raycast(planeRay, out var enterDistance))
							{
								var point = planeRay.GetPoint(enterDistance);
								vertex.vector2Value = new Vector2(point.x, point.z);
							}
							else
							{
								vertex.vector2Value = new Vector2(position3D.x, position3D.z);
							}
						}
					}

					points[vertexIndex] = position3D;
				}

				if (EditorGUI.actionKey)
				{
					//NOTE(johannes): Always repaint scene view while holding the action key,
					// otherwise the new vertex button stutters.
					SceneView.lastActiveSceneView.Repaint();

					var point = ClosestPointToPolyLine(points, out var afterIndex);
					var handleSize = HandleUtility.GetHandleSize(point) * HandleSize;
					if (Handles.Button(point, Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap))
					{
						vertices.InsertArrayElementAtIndex(afterIndex + 1);
						var vertex = vertices.GetArrayElementAtIndex(afterIndex + 1);
						vertex.vector2Value = new Vector2(point.x, point.z);
					}
				}
			}

			//NOTE(johannes): Set Handles.matrix back to an identity matrix before returning,
			// otherwise the top-right axis gizmo also takes on the matrix.
			Handles.matrix = Matrix4x4.identity;

			serializedObject.ApplyModifiedProperties();
		}

		static Vector3 ClosestPointToPolyLine(Vector3[] vertices, out int afterIndex)
		{
			int vertCount = vertices.Length;
			Debug.Assert(vertCount >= 2, "Must have at least two vertices");

			var smallestDistance = HandleUtility.DistanceToLine(vertices[0], vertices[1]);
			int smallestIndex = 0;
			for (int i = 2; i < vertCount + 1 /* +1 to check line between first and last vertex */; ++i)
			{
				var distance = HandleUtility.DistanceToLine(vertices[i - 1], vertices[i % vertCount]);
				if (distance < smallestDistance)
				{
					smallestDistance = distance;
					smallestIndex = i - 1;
				}
			}

			var vertex1 = vertices[smallestIndex];
			var vertex2 = vertices[(smallestIndex + 1) % vertCount];
			var screenOffsetFromMouse = Event.current.mousePosition - HandleUtility.WorldToGUIPoint(vertex1);
			var screenLineDelta = HandleUtility.WorldToGUIPoint(vertex2) - HandleUtility.WorldToGUIPoint(vertex1);
			var magnitude = screenLineDelta.magnitude;
			var alpha = Vector3.Dot(screenLineDelta, screenOffsetFromMouse);
			if (magnitude > Mathf.Epsilon)
				alpha /= magnitude * magnitude;
			alpha = Mathf.Clamp01(alpha);
			afterIndex = smallestIndex;
			return Vector3.Lerp(vertex1, vertex2, alpha);
		}
	}
}
