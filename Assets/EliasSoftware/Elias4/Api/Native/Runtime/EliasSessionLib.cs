using System;
using System.Runtime.InteropServices;

namespace EliasSoftware.Elias4.Native.Runtime {

    using elias_sample_t_ptr = IntPtr;
    using void_ptr = IntPtr;
    using elias_realtime_update_handle_t = IntPtr;
    using elias_base_id_t = UInt32;
    using elias_model_id_t = elias_uuid_t;
    using elias_float_t = Double;
    using elias_parameter_id_t = elias_uuid_t;
    using elias_event_handler_id_t = uint64_t;
    using elias_callback_id_t = UInt32; // should be elias_base_id_t

    public delegate void elias_rendered_buffer_callback_t(
        [MarshalAs(UnmanagedType.LPStruct)] elias_rendered_buffer_t buffer,
        void_ptr user );
    public delegate void elias_realtime_process_buffer_t(
        elias_realtime_update_handle_t handle,
        elias_rendered_buffer_callback_t callback,
        void_ptr user);

    public class EliasSessionLib {

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_create_session")]
        public static extern elias_result_t elias_create_session(
            elias_handle_t context,
            in elias_session_config_t config,
            out elias_session_id_t session_id,
            elias_realtime_process_buffer_t rt_update_fn,
            elias_realtime_update_handle_t rt_update_handle );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_create_session")]
        public static extern elias_result_t elias_create_session_with_manual_rendering(
            elias_handle_t context,
            in elias_session_config_t config,
            out elias_session_id_t session_id,
            out elias_realtime_process_buffer_t rt_update_fn,
            out elias_realtime_update_handle_t rt_update_handle );
        
        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_destroy_session")]
        public static extern elias_result_t elias_destroy_session(
            elias_handle_t context,
            elias_session_id_t session_id );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_get_session_count")]
        public static extern elias_result_t elias_get_session_count(
            elias_handle_t context,
            out elias_size_t count );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_enumerate_sessions")]
        public static extern elias_result_t elias_enumerate_sessions(
            elias_handle_t context,
            [In, Out] elias_session_id_t[] session_ids,
            elias_size_t capacity, 
            out elias_size_t resulting_count );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_get_config")]
        public static extern elias_result_t elias_get_session_config( 
            elias_handle_t context,
            elias_session_id_t session_id,
            out elias_session_config_t config );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_create_listener")]
        public static extern elias_result_t elias_session_create_listener(
            elias_handle_t context,
            elias_session_id_t session_id,
            elias_model_id_t listener_model_id,
            out elias_model_instance_id_t instance_id );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_destroy_listener")]
        public static extern elias_result_t elias_session_destroy_listener(
            elias_handle_t context,
            elias_session_id_t session_id,
            elias_model_instance_id_t instance_id );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_create_sound_instance")]
        public static extern elias_result_t elias_session_create_sound_instance(
            elias_handle_t context,
            elias_session_id_t session_id,
            elias_model_id_t patch_id,
            out elias_model_instance_id_t instance_id );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_destroy_sound_instance")]
        public static extern elias_result_t elias_session_destroy_sound_instance(
            elias_handle_t context,
            elias_session_id_t session_id,
            elias_model_instance_id_t instance_id );


        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_create_zone")]
        public static extern elias_result_t elias_session_create_zone(
            elias_handle_t context,
            elias_session_id_t session_id,
            out elias_model_instance_id_t instance_id );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_destroy_zone")]
        public static extern elias_result_t elias_session_destroy_zone(
            elias_handle_t context,
            elias_session_id_t session_id,
            elias_model_instance_id_t instance_id );


        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_force_preload_audio")]
        public static extern elias_result_t elias_session_force_preload_audio(
            elias_handle_t context,
            elias_session_id_t session_id,
            [In, Out] elias_uuid_t[] audio_assets,
            elias_size_t audio_asset_count );


        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_load_reverb")]
        public static extern elias_result_t elias_session_load_reverb(
            elias_handle_t context,
            elias_session_id_t session_id,
            in elias_uuid_t reverb_asset_id ); // ptr ref


        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_unload_reverb")]
        public static extern elias_result_t elias_session_unload_reverb(
            elias_handle_t context,
            elias_session_id_t session_id,
            in elias_uuid_t reverb_asset_id ); // ptr ref


        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_update")]
        public static extern elias_result_t elias_session_update(
            elias_handle_t context,
            elias_session_id_t session_id,
            elias_float_t elapsed_time );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_apply_operation")]
        public static extern elias_result_t elias_session_apply_operation(
            elias_handle_t context,
            elias_session_id_t session_id,
            elias_model_instance_id_t model_instance_id,
            elias_model_instance_operation_t operation );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_set_global_param_value")]
        public static extern elias_result_t elias_session_set_global_param_value(
            elias_handle_t context,
            elias_session_id_t session_id,
            elias_parameter_id_t param_id,
            [In, Out] elias_typed_value_t value );

        public delegate void elias_session_lifecycle_callback_t(
                elias_session_id_t session_id,
                elias_session_lifecycle_event_t event_type,
                void_ptr user);

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_register_session_lifecycle_callback")]
        public static extern elias_result_t elias_register_session_lifecycle_callback(
            elias_handle_t context,
            uint64_t filter_session_flags,
            elias_session_lifecycle_callback_t callback,
            void_ptr user,
            out elias_callback_id_t callback_id );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_unregister_session_lifecycle_callback")]
        public static extern elias_result_t elias_unregister_session_lifecycle_callback(
            elias_handle_t context,
            elias_callback_id_t callback_id );

        public delegate void elias_simple_callback_t(void_ptr user);

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_register_instance_destroyed_callback")]
        public static extern elias_result_t elias_session_register_instance_destroyed_callback(
            elias_handle_t context,
            elias_session_id_t session_id,
            elias_model_instance_id_t model_instance_id,
            elias_simple_callback_t callback,
            void_ptr user,
            out elias_event_handler_id_t callback_id );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_unregister_instance_destroyed_callback")]
        public static extern elias_result_t elias_session_unregister_instance_destroyed_callback(
            elias_handle_t context,
            elias_session_id_t session_id,
            elias_event_handler_id_t callback_id );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_get_instance_count")]
        public static extern elias_result_t elias_session_get_instance_count(
            elias_handle_t context,
            elias_session_id_t session_id,
            [In, Out] elias_model_instance_filter_t filter,
            out elias_size_t count );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_session_enumerate_instances")]
        public static extern elias_result_t elias_session_enumerate_instances(
            elias_handle_t context,
            elias_session_id_t session_id,
            [In, Out] elias_model_instance_filter_t filter,
            [In, Out] elias_model_instance_id_t[] instance_ids,
            elias_size_t capacity,
            out elias_size_t resulting_count );

    }
}
