namespace EliasSoftware.Elias4
{
	using System.Collections.Generic;
	using UnityEngine;
	using EliasSoftware.Elias4.Common;

	/// <summary>
	/// This MonoBehaviour corresponds to a zone (a volume in 3D space).
	/// The zones are used to create the sound of a room and set how sounds inside and outside of the room should be affected by occlusion.
	/// </summary>
	public class EliasZone : MonoBehaviour
	{
		/// <summary>
		/// The ID of this zone.
		/// </summary>
		public ZoneID Zone { get; private set; }

		[Tooltip("Enables or disables the abillity of this zone to be stacked on top of or under another zone.")]
		/// <summary>
		/// Enables or disables the abillity of this zone to be stacked on top of or under another zone.
		/// </summary>
		public bool enableVerticalBlending;

		[Tooltip("Defines how the inbound and outbound sounds should be heard when the listener is inside or outside the zone.")]
		/// <summary>
		/// Defines how the inbound and outbound sounds should be heard when the listener is inside or outside the zone.
		/// </summary>
		public SoundBleed soundBleed;

		[Tooltip("The geometry of the room.")]
		/// <summary>
		/// The geometry of the room.
		/// </summary>
		public Geometry geometry;

		[Tooltip("Select what IR asset you want this zone to use and set the dry/wet mix.")]
		/// <summary>
		/// Select what IR asset you want this zone to use and set the dry/wet mix.
		/// </summary>
		public ZoneReverb reverb;

		private void OnEnable()
		{
            Zone = Elias.API.CreateZone();
            Elias.API.LoadIR(reverb.impulseResponse.IRID);
			UpdateEliasProperties();
		}

		private void OnDisable()
		{
            Elias.API.UnloadIR(reverb.impulseResponse.IRID);
            Elias.API.DestroyZone(Zone);
		}

		private void Update()
		{
			if (gameObject.isStatic)
				return;

            Elias.API.SetZoneGeometry(Zone, geometry, transform);
		}

		/// <summary>
		/// This method sends all the settings (SoundBleed, Geometry etc) to the elias audio engine and is currently called automatically when the game object is enabled.
		/// If any of the settings on a zone are modified after the zone's GameObject has been enabled the audio engine will not know about those changes unless this method is called.
		/// Note: This requirement is very likely to change in the near future.
		/// </summary>
		public void UpdateEliasProperties()
		{
            Elias.API.SetZoneGeometry(Zone, geometry, transform);
            Elias.API.SetZoneSetEnableVerticalBlending(Zone, enableVerticalBlending);

			if(soundBleed != null && soundBleed.inbound.enabled)
			{
				Elias.API.SetZoneInboundOcclusion(Zone,
					soundBleed.inbound.endDistance,
					soundBleed.inbound.endDistance,
					soundBleed.inbound.endDryWet,
					soundBleed.inbound.endFrequency,
					soundBleed.inbound.endAmplitude);
			}

			if(soundBleed != null && soundBleed.outbound.enabled)
			{
				Elias.API.SetZoneOutboundOcclusion(Zone,
					soundBleed.outbound.endDistance,
					soundBleed.outbound.endDistance,
					soundBleed.outbound.endDryWet,
					soundBleed.outbound.endFrequency,
					soundBleed.outbound.endAmplitude);
			}

			if(reverb != null)
            	Elias.API.SetZoneIR(Zone, reverb.impulseResponse.IRID, reverb.sendProportion);
		}

		#if UNITY_EDITOR
		[HideInInspector]
		private Mesh previewMesh;

		[HideInInspector]
		private bool isValidGeometry = true;

		private void Reset()
		{
			geometry = new Geometry();
			UpdatePreviewMesh();
		}

		private void OnValidate()
		{
			UnityEditor.EditorApplication.delayCall +=
				() => {
					if( this == null || Elias.IsInitialized == false )
						return;
					UpdatePreviewMesh();
					UpdateEliasProperties();
				};
		}

		private void UpdatePreviewMesh()
		{
			if (!previewMesh)
			{
				previewMesh = new Mesh();
				previewMesh.name = "EliasZonePreviewMesh";
				previewMesh.MarkDynamic();
			}
			previewMesh.Clear(keepVertexLayout: true);

			if (geometry.vertices.Length < 3)
			{
				previewMesh.SetVertices(new[] { Vector3.zero, Vector3.zero, Vector3.zero });
				previewMesh.SetIndices(new[] { 0, 1, 2 }, MeshTopology.Triangles, 0);
			}
			else
			{
				var vertexCount = geometry.vertices.Length;
				var vtxCount = vertexCount * 2;

				var meshVertices = new List<Vector3>(vtxCount);
				var meshIndices = new List<int>(vertexCount * 3 * 4);

				foreach (var vertex in geometry.vertices)
				{
					meshVertices.Add(new Vector3(vertex.x, 0f, vertex.y));
					meshVertices.Add(new Vector3(vertex.x, geometry.height, vertex.y));
				}

				var indexCount = (vertexCount - 1) * 2;
				for (int index = 0; index <= indexCount; index += 2)
				{
					// First triangle
					meshIndices.Add((index + 0) % vtxCount);
					meshIndices.Add((index + 1) % vtxCount);
					meshIndices.Add((index + 3) % vtxCount);

					// First triangle reverse
					meshIndices.Add((index + 0) % vtxCount);
					meshIndices.Add((index + 3) % vtxCount);
					meshIndices.Add((index + 1) % vtxCount);

					// Second triangle
					meshIndices.Add((index + 0) % vtxCount);
					meshIndices.Add((index + 3) % vtxCount);
					meshIndices.Add((index + 2) % vtxCount);

					// Second triangle reverse
					meshIndices.Add((index + 0) % vtxCount);
					meshIndices.Add((index + 2) % vtxCount);
					meshIndices.Add((index + 3) % vtxCount);
				}

				previewMesh.SetVertices(meshVertices);
				previewMesh.SetIndices(meshIndices, MeshTopology.Triangles, 0);
			}
			previewMesh.RecalculateNormals();
		}

		private struct BoundaryVisualizationPoint
		{
			public Vector2 location;

			public struct SegmentInfo
			{
				public float length;
				public bool isArc;
				public Vector2 focus;
			}
			public SegmentInfo subsequentSegment;
		}

		private struct BoundaryVisualization
		{
			public List<BoundaryVisualizationPoint> points;
			public float length;
		}

		private static float V2CrossProduct(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
		private static float AngleFromArcRadians(Vector2 a, Vector2 b, Vector2 focus)
		{
			var fa = a - focus;
			var fb = b - focus;
			var angleRadians = Mathf.Asin(V2CrossProduct(fa.normalized, fb.normalized));
			if (Vector2.Dot(fa, fb) < 0f)
				angleRadians = Mathf.Sign(angleRadians) * (Mathf.PI - Mathf.Abs(angleRadians));
			return angleRadians;
		}

		private static BoundaryVisualization ConstructBoundaryFromPolygon(Geometry geometry, Vector2 scale, bool inner, float width)
		{
			var vertexCount = geometry.vertices.Length;
			var result = new BoundaryVisualization
			{
				points = new List<BoundaryVisualizationPoint>(vertexCount * 2 /* Reserve space for at most 2 points per vertex */)
			};

			for (int i = 0; i < vertexCount; ++i)
			{
				var point = geometry.vertices[i] * scale;
				var prevPoint = geometry.vertices[(i - 1 + vertexCount) % vertexCount] * scale;
				var nextPoint = geometry.vertices[(i + 1) % vertexCount] * scale;
				var inSeg = point - prevPoint;
				var outSeg = nextPoint - point;
				var inSegDir = inSeg.normalized;
				var outSegDir = outSeg.normalized;

				var crossProduct = V2CrossProduct(inSegDir, outSegDir);
				bool pveWinding = crossProduct < 0f;
				bool obtuseAngle = (pveWinding == inner);

				Vector2 GeneratePerpendicularRadial(Vector2 segDir) => Vector2.Perpendicular(segDir) * (inner ? 1f : -1f);

				if (obtuseAngle)
				{
					// Need 2 points/segments. First will be an arc.
					result.points.Add(new BoundaryVisualizationPoint
					{
						location = point + GeneratePerpendicularRadial(inSegDir) * width,
						subsequentSegment = new BoundaryVisualizationPoint.SegmentInfo
						{
							isArc = true,
							focus = point,
						},
					});
				}

				Vector2 radial;
				if (obtuseAngle || Mathf.Approximately(crossProduct, 0f))
				{
					// Parallel segments
					radial = GeneratePerpendicularRadial(outSegDir) * width;
				}
				else
				{
					var radialDir = -(inSegDir - outSegDir).normalized;
					var halfAngleSize = Mathf.Asin(Mathf.Abs(V2CrossProduct(inSegDir, radialDir)));
					var radialLength = width / Mathf.Cos(Mathf.PI / 2f - halfAngleSize);
					radial = radialDir * radialLength;
				}

				result.points.Add(new BoundaryVisualizationPoint
				{
					location = point + radial,
					subsequentSegment = new BoundaryVisualizationPoint.SegmentInfo
					{
						isArc = false,
					},
				});
			}

			for (int i = 0; i < result.points.Count; ++i)
			{
				var point = result.points[i];
				var next = result.points[(i + 1) % result.points.Count];

				if (point.subsequentSegment.isArc)
				{
					var angleRadians = AngleFromArcRadians(point.location, next.location, point.subsequentSegment.focus);
					point.subsequentSegment.length = Mathf.Abs(angleRadians) * (point.location - point.subsequentSegment.focus).magnitude;
				}
				else
				{
					point.subsequentSegment.length = (next.location - point.location).magnitude;
				}

				result.points[i] = point;
				result.length += point.subsequentSegment.length;
			}

			return result;
		}

		private void GizmoDrawSoundBleed(Geometry geometry, SoundBleed.ZoneOcclusion bleedOut, SoundBleed.ZoneOcclusion bleedIn, float boundarySpacing, float boundaryPointSize, Color bleedOutColor, Color bleedInColor)
		{
			var scale3D = transform.lossyScale;
			var scale2D = new Vector2(scale3D.x, scale3D.z);

			var trs = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

			// Bleed Out boundary
			if (bleedOut.HasAnyAttenuation)
			{
				var boundary = ConstructBoundaryFromPolygon(geometry, scale2D, inner: false, bleedOut.endDistance);
				GizmoDrawBoundary(boundary, trs, bleedOutColor, boundarySpacing, boundaryPointSize);
			}

			// Bleed In boundary
			if (bleedIn.HasAnyAttenuation)
			{
				var boundary = ConstructBoundaryFromPolygon(geometry, scale2D, inner: true, bleedIn.endDistance);
				GizmoDrawBoundary(boundary, trs, bleedInColor, boundarySpacing, boundaryPointSize, 0.5f);
			}
		}

		private void GizmoDrawOcclusionFiltering(Geometry geometry, SoundBleed filtering, float boundarySpacing, float boundaryPointSize, Color color)
		{
			var scale3D = transform.lossyScale;
			var scale2D = new Vector2(scale3D.x, scale3D.z);

			var trs = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

			if(filtering.inbound.enabled) {
				var innerBoundary = ConstructBoundaryFromPolygon(geometry, scale2D, inner: true, filtering.inbound.endDistance);
				GizmoDrawBoundary(innerBoundary, trs, color, boundarySpacing, boundaryPointSize);
			}

			if(filtering.outbound.enabled) {
				var outerBoundary = ConstructBoundaryFromPolygon(geometry, scale2D, inner: false, filtering.outbound.endDistance);
				GizmoDrawBoundary(outerBoundary, trs, color, boundarySpacing, boundaryPointSize);
			}
		}

		private static void GizmoDrawBoundary(BoundaryVisualization boundary, Matrix4x4 transform, Color color, float pointSpacing, float pointSize, float staggerProportion = 0f)
		{
			if (boundary.length <= 0f)
				return;

			var oldColor = Gizmos.color;
			var oldMatrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.identity;

			var approxDrawPointSeparation = pointSpacing;
			const int MaxDrawPoints = 1000;
			var numDrawPoints = Mathf.Clamp(Mathf.RoundToInt(boundary.length / approxDrawPointSeparation), 8, MaxDrawPoints);
			var actualDrawPointSeparation = boundary.length / numDrawPoints;

			var remainder = staggerProportion * actualDrawPointSeparation;
			int pointIndex = 0;
			for (int i = 0; i < numDrawPoints; ++i, remainder += actualDrawPointSeparation)
			{
				while (remainder >= boundary.points[pointIndex].subsequentSegment.length)
				{
					remainder -= boundary.points[pointIndex].subsequentSegment.length;
					pointIndex = (pointIndex + 1) % boundary.points.Count;
				}

				var pointA = boundary.points[pointIndex];
				var pointB = boundary.points[(pointIndex + 1) % boundary.points.Count];

				if (pointA.subsequentSegment.isArc)
				{
					var angleRadians = AngleFromArcRadians(pointA.location, pointB.location, pointA.subsequentSegment.focus);
					var rotationRad = angleRadians * remainder / pointA.subsequentSegment.length;
					var quat = new Vector2(Mathf.Cos(rotationRad), Mathf.Sin(rotationRad));
					var fa = pointA.location - pointA.subsequentSegment.focus;
					var faRot = new Vector2(fa.x * quat.x - fa.y * quat.y, fa.x * quat.y + fa.y * quat.x);
					var loc = pointA.subsequentSegment.focus + faRot;
					Gizmos.color = color;
					Gizmos.DrawWireSphere(transform.MultiplyPoint3x4(new Vector3(loc.x, 0f, loc.y)), pointSize);
				}
				else
				{
					var loc = Vector2.Lerp(pointA.location, pointB.location, remainder / pointA.subsequentSegment.length);
					Gizmos.color = color;
					Gizmos.DrawWireSphere(transform.MultiplyPoint3x4(new Vector3(loc.x, 0f, loc.y)), pointSize);
				}
			}

			Gizmos.matrix = oldMatrix;
			Gizmos.color = oldColor;
		}

		private void OnDrawGizmos()
		{
			if (!previewMesh)
				UpdatePreviewMesh();
			Gizmos.matrix = transform.localToWorldMatrix;

			var zoneSettings = ((EliasSettingsSO)EliasSettings.Instance).zoneVisualization;

			var color = isValidGeometry ? zoneSettings.defaultZoneColor : zoneSettings.invalidZoneColor;
			var selected = UnityEditor.Selection.activeGameObject == gameObject;

			if (selected)
			{
				var fadedColor = color;
				fadedColor.a *= 0.5f;
				Gizmos.color = fadedColor;
				Gizmos.DrawMesh(previewMesh);
			}

			Gizmos.color = color;

			int vertCount = geometry.vertices.Length;
			for (int i = 1; i < vertCount + 1; ++i)
			{
				var vertex1 = geometry.vertices[i - 1];
				var vertex2 = geometry.vertices[i % vertCount];

				// Horizontal lines
				Gizmos.DrawLine(new Vector3(vertex1.x, 0f, vertex1.y), new Vector3(vertex2.x, 0f, vertex2.y));
				Gizmos.DrawLine(new Vector3(vertex1.x, geometry.height, vertex1.y), new Vector3(vertex2.x, geometry.height, vertex2.y));

				// Vertical lines
				Gizmos.DrawLine(new Vector3(vertex1.x, 0f, vertex1.y), new Vector3(vertex1.x, geometry.height, vertex1.y));
			}

			/*if (!zoneSettings.drawOnlyWhenSelected || selected)
			{
				if (zoneSettings.drawSoundBleedRange)
				{
					GizmoDrawSoundBleed(geometry,
						soundBleed.GetEffectiveBleedOutAttenuation(),
						soundBleed.GetEffectiveBleedInAttenuation(),
						zoneSettings.boundaryPointSpacing,
						zoneSettings.boundaryPointSize,
						zoneSettings.soundBleedOutColor,
						zoneSettings.soundBleedInColor
					);
				}

				if (zoneSettings.drawOcclusionFilterRange)
				{
					GizmoDrawOcclusionFiltering(geometry,
						soundBleed.GetEffectiveOcclusionFiltering(),
						zoneSettings.boundaryPointSpacing,
						zoneSettings.boundaryPointSize,
						zoneSettings.occlusionFilteringColor
					);
				}
			}*/
		}
		#endif
	}

}
