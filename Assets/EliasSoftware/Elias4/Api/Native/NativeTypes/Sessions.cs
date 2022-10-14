using System;
using System.Runtime.InteropServices;

namespace EliasSoftware.Elias4.Native {

    using elias_sample_t_ptr = IntPtr;
    using uint32_t = UInt32;
    using elias_model_id_t = elias_uuid_t;
    using elias_model_type_id_t = elias_uuid_t;

    public enum elias_session_flags_t : UInt64
    {
        /** If set, session output rendering triggers a client-provided callback. Otherwise, rendering is to an internally managed device specified by device_name in the session config. */
        ELIAS_SESSION_MANUAL_RENDERING          = 1 << 0,
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public unsafe struct elias_session_config_t
    {
        public elias_session_flags_t session_flags;

        /** Zero for these members means 'default' */
        public elias_channel_count_t num_channels;
        public elias_buffer_length_t frames_per_buffer;
        public elias_sample_rate_t sample_rate;
        public float gain_interpolation_coefficient;
        public elias_buffer_length_t panner_interpolation_frames;

        /** @NOTE: Currently we just use a string to identify different device outputs (so two sessions with the same device name will output to the same device).
        * If a device with matching name already exists, it will be used and the session will inherit the sample rate and channel count of the device, with the values
        * specified in the config ignored.
        * Frames per buffer is independent and will always use the value specified, regardless of the device.
        *
        * If session_flags includes ELIAS_SESSION_MANUAL_RENDERING, device_name is ignored.
        */
        public fixed byte device_name[64];

        // @TODO: designtime-conditional
        public fixed byte session_name[64];

        public override string ToString() {
            var sb = new System.Text.StringBuilder();

            int max_len = 64;
            for(int i = 0; i < max_len; i++) {
                var c = device_name[i];
                if(c == 0)
                    break;
                sb.Append(c);
            }
            string device_name_str = sb.ToString();

            sb.Clear();
            for(int i = 0; i < max_len; i++) {
                var c = session_name[i];
                if(c == 0)
                    break;
                sb.Append(c);
            }
            string session_name_str = sb.ToString();

            return string.Format("elias_session_config_t :\n" +
                "\t session_flags = {0}\n" +
                "\t num_channels = {1}\n" +
                "\t frames_per_buffer = {2}\n" +
                "\t sample_rate = {3}\n" +
                "\t gain_interpolation_coefficient = {4}\n" +
                "\t panner_interpolation_frames = {5}\n" +
                "\t device_name = {6}\n" +
                "\t session_name = {7}\n",
                    session_flags,
                    num_channels,
                    frames_per_buffer,
                    sample_rate,
                    gain_interpolation_coefficient,
                    panner_interpolation_frames,
                    device_name_str,
                    session_name_str);
        }
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_model_instance_operation_t
    {
        //elias_model_instance_id_t inst;
        public uint op;
        public byte[] args;
        public uint32_t args_length;
    }

    public enum elias_session_lifecycle_event_t
    {
        ELIAS_SESSION_CREATED,
        ELIAS_SESSION_DESTROYED,
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_rendered_buffer_t
    {
        public elias_sample_t_ptr samples;
        public elias_channel_count_t num_channels;
        public elias_buffer_length_t num_frames;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_model_instance_filter_t
    {
        public elias_model_type_id_t model_type_id;
        public elias_model_id_t model_id;
    }

}
