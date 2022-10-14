using System;
using System.Runtime.InteropServices;

namespace EliasSoftware.Elias4.Native {

    using uint32_t = UInt32;

    public enum elias_listener_op_t : uint32_t
    {
        ELIAS_LIST_SET_POSITION,
        ELIAS_LIST_SET_VELOCITY,
        ELIAS_LIST_SET_SPATIALIZATION,
        ELIAS_LIST_SET_REVERB_MODE,
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_listener_set_position_t
    {
        public elias_location_t location;
        public elias_quaternion_t rotation;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_listener_set_velocity_t
    {
        public elias_location_t linear;
        public elias_location_t angular;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_listener_set_spatialization_t
    {
        public bool enabled;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_listener_set_reverb_mode_t
    {
        public bool enabled;
    }
}