using System;
using System.Runtime.InteropServices;

namespace EliasSoftware.Elias4.Native {

    using uint32_t = UInt32;
    using uint8_t = System.Byte;

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_sound_instance_set_param_value_t
    {
        public elias_uuid_t param_id;
        public elias_typed_value_t value;
    }

    public enum elias_sound_instance_op_t 
        : uint32_t
    {
        ELIAS_SI_SET_SPATIALIZATION,
        ELIAS_SI_SET_POSITION,
        ELIAS_SI_SET_VELOCITY,
        ELIAS_SI_SET_SOURCE_DEFN,
        ELIAS_SI_SET_MASTER_AMPLITUDE,
        ELIAS_SI_SET_MASTER_PITCH,
        ELIAS_SI_SET_DISTANCE_ATTENUATION,
        ELIAS_SI_SET_LISTENER_ATTENUATION,
        ELIAS_SI_SET_STEREO_WIDTH,
        ELIAS_SI_SET_SPREAD,
        ELIAS_SI_SET_MIN_CHANNEL_SPREAD,
        ELIAS_SI_SET_ZONE_BEHAVIOUR,
        ELIAS_SI_SET_REVERB,
        ELIAS_SI_SET_INTERPOLATION_TIME,    // @TODO: m0 still wanted? does the engine actually use this value still?
        ELIAS_SI_SET_DESTROY_ON_SILENCE,
        ELIAS_SI_SET_PARAM_VALUE,
    }

    public enum elias_zone_bleed_behaviour_t : uint8_t
    {
        /** Sound attenuates at zone boundary as defined by zone settings. */
        ELIAS_ZONE_BLEED_DEFAULT,
        /** Sounds unaffected by zone boundary. */
        ELIAS_ZONE_BLEED_IGNORE,
        /** Sounds cannot pass zone boundary. */
        ELIAS_ZONE_BLEED_NONE,
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_sound_instance_set_zone_behaviour_t
    {
        public elias_zone_bleed_behaviour_t behaviour;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_sound_instance_reverb_settings_t
    {
        public float distance_min;
        public float distance_max;
        public float reverb_min;
        public float reverb_max;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_listener_spatial_attenuation_defn_t
    {
        public double inner_cone_angle;
        public double outer_cone_angle;
        public double outer_cone_amplitude_gain;
        public double interpolation_speed;
    }

    
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_sound_instance_set_position_t
    {
        public elias_location_t location;
        public bool enable_orientation;
        public elias_quaternion_t rotation;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_sound_instance_set_velocity_t
    {
        public elias_location_t linear;
        public elias_location_t angular;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_range_t
    {
        public double start;
        public double end;
    }

    public enum elias_falloff_curve_type_t
    {
        ELIAS_FALLOFF_TYPE_NONE,
        ELIAS_FALLOFF_TYPE_LINEAR,
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_distance_attenuation_t
    {
        // The attenuation multiplier to use at the end of the falloff range. Non-zero values result in sounds
        // that do not fully attenuate with distance.
        public double max_attenuation;
        // Falloff curve.
        public elias_falloff_curve_type_t falloff_type;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_sound_instance_set_distance_attenuation_t
    {
        public elias_range_t range;
        public elias_distance_attenuation_t attenuation;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_spread_t
    {
        // Spread value within the spread starting range, in [0, 1].
        public double start_spread;
        // Spread value at the end radius, in [0, 1].
        public double end_spread;
        // Spread falloff curve.
        public elias_falloff_curve_type_t falloff_type;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_spot_sound_source_defn_t
    {
        public byte _; // empty struct are different in C/C++/C#
    };


    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_cone_sound_source_defn_t
    {
        /** Angles are in radians. */
        public double inner_cone_angle;
        public double outer_cone_angle;
        public double outer_cone_amplitude_gain;
        public bool disable_angular_attenuation_within_start_radius;
    }

    public enum elias_sound_source_type_t : uint
    {
        ELIAS_SOUND_SOURCE_SPOT,
        ELIAS_SOUND_SOURCE_CONE,
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_sound_instance_set_spread_t
    {
        public elias_range_t range;
        public elias_spread_t spread;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_sound_source_t
    {
        public elias_sound_source_type_t type;
        public elias_cone_sound_source_defn_t cone;
    }
}