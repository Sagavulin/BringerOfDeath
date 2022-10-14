using System;
using System.Runtime.InteropServices;

namespace EliasSoftware.Elias4.Native {


    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_occlusion_boundary_config_t
    {
        /**
        * The distance beyond the boundary at which maximum attenuation is reached for sounds on the other side.
        */
        public double attenuationEndDistance;

        /**
        * The maximum attenuation (as an amplitude multiplier) applied by the boundary.
        */
        public double maxAttenuation;

        /**
        * The dry/wet mix of the lowpass filter applied for sounds crossing the boundary.
        * If 0, no filtering is applied. 1 is entirely filtered.
        */
        public double filterDryWet;

        /**
        * The frequency of the lowpass filter applied for sounds crossing the boundary.
        */
        public double filterFrequency;

        /**
        * Distance from the boundary at which point filtering effect is fully applied.
        * 0 will result in instant filtering when crossing the boundary.
        */
        public double filterEndDistance;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_zone_reverb_config_t
    {
        /**
        * The reverb asset used for reverb processing on sounds in the zone.
        */
        public elias_uuid_t reverb_asset_id;

        /**
        * The proportion of a sound in the zone which gets sent through reverb processing.
        * 0.0 will disable reverb on the zone.
        */
        public double reverb_send_proportion;
    }

    public enum elias_zone_op_t : uint
    {
        ELIAS_ZONE_SET_GEOMETRY,
        ELIAS_ZONE_SET_OUTBOUND_OCCLUSION,
        ELIAS_ZONE_SET_INBOUND_OCCLUSION,
        ELIAS_ZONE_SET_REVERB,
        ELIAS_ZONE_SET_ENABLE_VERTICAL_BLENDING,
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_zone_set_geometry_t
    {
        public elias_geometry_with_transform_t geometry;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_zone_set_occlusion_t
    {
        public elias_occlusion_boundary_config_t boundary;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_zone_set_reverb_t
    {
        public elias_zone_reverb_config_t reverb;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_zone_set_enable_vertical_blending_t
    {
        public bool enabled;
    }
}