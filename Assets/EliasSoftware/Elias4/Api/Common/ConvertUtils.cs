using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace EliasSoftware.Elias4.Common {

	using EliasSoftware.Elias4.Native;

    public static class ConvertUtils {
        public static elias_location_t ToEliasLocation(double x, double y, double z)
            => new elias_location_t() {
                x_forward=z,
                y_right=x,
                z_up=y
            };

        public static elias_location_t UnityToElias(Vector3 vec)
            => new elias_location_t() {
                x_forward=vec.z,
                y_right=vec.x,
                z_up=vec.y
            };

        public static elias_location2d_t ToEliasLocation2D(double x, double y)
            => new elias_location2d_t() {
                x = y,
                y = x
            };

        public static elias_location2d_t UnityToElias(Vector2 vec)
            => new elias_location2d_t() {
                x = vec.y,
                y = vec.x
            };

        public static elias_quaternion_t ToEliasQuaternion(double x, double y, double z, double w)
            => new elias_quaternion_t() {
                x=z,
                y=x,
                z=y,
                w=w
            };

        public static elias_quaternion_t UnityToElias(Quaternion orientation)
            => new elias_quaternion_t() {
                x=orientation.z,
                y=orientation.x,
                z=orientation.y,
                w=orientation.w
            };

        public static elias_geometry_with_transform_t UnityToElias(Geometry geometry, Transform transform)
		{
			var geometry_with_transform = new elias_geometry_with_transform_t
			{
				geom_element = new elias_geometry_t
				{
					type = elias_geometry_type_t.ELIAS_GEOMETRY_EXTRUDED_POLY,
					extruded_polygon = new elias_extruded_polygon_t()
					{
						height = geometry.height,
						num_points = (uint)geometry.vertices.Length,
					},
				},
				location = UnityToElias(transform.position),
				rotation = UnityToElias(transform.rotation),
				scale = UnityToElias(transform.lossyScale),
			};

			var pointCount = Mathf.Min(geometry.vertices.Length, elias_extruded_polygon_t.ELIAS_POLYGON_MAX_POINTS);
			for (int pointIndex = 0; pointIndex < pointCount; ++pointIndex)
			{
				var point = UnityToElias(geometry.vertices[pointIndex]);
				var pointOutIndex = pointIndex * 2;
				unsafe
				{
					geometry_with_transform.geom_element.extruded_polygon.points[pointOutIndex + 0] = point.x;
					geometry_with_transform.geom_element.extruded_polygon.points[pointOutIndex + 1] = point.y;
				}
			}

			return geometry_with_transform;
		}

        public static elias_geometry_with_transform_t UnityToElias(Geometry geometry, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			var geometry_with_transform = new elias_geometry_with_transform_t
			{
				geom_element = new elias_geometry_t
				{
					type = elias_geometry_type_t.ELIAS_GEOMETRY_EXTRUDED_POLY,
					extruded_polygon = new elias_extruded_polygon_t()
					{
						height = geometry.height,
						num_points = (uint)geometry.vertices.Length,
					},
				},
				location = UnityToElias(position),
				rotation = UnityToElias(rotation),
				scale = UnityToElias(scale)
			};

			var pointCount = Mathf.Min(geometry.vertices.Length, elias_extruded_polygon_t.ELIAS_POLYGON_MAX_POINTS);
			for (int pointIndex = 0; pointIndex < pointCount; ++pointIndex)
			{
				var point = UnityToElias(geometry.vertices[pointIndex]);
				var pointOutIndex = pointIndex * 2;
				unsafe
				{
					geometry_with_transform.geom_element.extruded_polygon.points[pointOutIndex + 0] = point.x;
					geometry_with_transform.geom_element.extruded_polygon.points[pointOutIndex + 1] = point.y;
				}
			}

			return geometry_with_transform;
		}
    }
}