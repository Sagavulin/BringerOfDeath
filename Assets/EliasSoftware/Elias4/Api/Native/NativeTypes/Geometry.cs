using System;
using System.Runtime.InteropServices;

namespace EliasSoftware.Elias4.Native {

    using uint32_t = UInt32;
    using elias_scale_t = elias_location_t;
    using uint8_t = System.Byte;

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_location_t
    {
        public double x_forward;
        public double y_right;
        public double z_up;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_location2d_t
    {
        public double x;
        public double y;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_quaternion_t
    {
        public double x;
        public double y;
        public double z;
        public double w;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_euler_t
    {
        public double pitch_around_y_right;
        public double yaw_around_z_up;
        public double roll_around_x_forward;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_bounding_box_t
    {
        public elias_location_t center;
        public elias_location_t size;
        public elias_quaternion_t orientation;
    }

    public enum elias_geometry_type_t : uint8_t
    {
        ELIAS_GEOMETRY_EXTRUDED_POLY,
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public unsafe struct elias_extruded_polygon_t
    {
        public const int ELIAS_POLYGON_MAX_POINTS = 64;
        public const int SIZEOF_ELIAS_LOCATION2D_IN_DOUBLES = 2;
        public fixed double points[SIZEOF_ELIAS_LOCATION2D_IN_DOUBLES * ELIAS_POLYGON_MAX_POINTS];
        public uint32_t num_points;
        public double height;
    }
    
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_geometry_t
    {
        public elias_geometry_type_t type;

        // Note: This is declared as a union with a single value
        // in the library.
        public elias_extruded_polygon_t extruded_polygon;
    }
    
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_geometry_with_transform_t
    {
        public elias_geometry_t geom_element;
        public elias_location_t location;
        public elias_quaternion_t rotation;
        public elias_scale_t scale;
    }

}