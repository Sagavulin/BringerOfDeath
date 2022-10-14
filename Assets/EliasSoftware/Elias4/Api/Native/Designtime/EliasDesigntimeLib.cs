using System;
using System.Runtime.InteropServices;

namespace EliasSoftware.Elias4.Native.Designtime {
    using Native;

    using elias_designtime_bool_t = Int32;
    using void_ptr = IntPtr;

    using elias_string_ptr_t = char_buf;
    using elias_const_string_ptr_t = char_buf;

    /////////////// GRAPHQL /////////////////

    public delegate void elias_designtime_response_callback_t (
        char_ptr response,
        void_ptr userData);

    public static class EliasDesigntimeLib
    {

        [DllImport(PluginInfo.DesigntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_designtime_call")]
        public static extern  elias_designtime_bool_t elias_designtime_call(
            char_ptr request,
            char_ptr variables,
            char_ptr api_key,
            elias_designtime_response_callback_t response_callback,
            void_ptr user_data );

        [DllImport(PluginInfo.DesigntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_designtime_get_project_runtime_context")]
        public static extern int elias_designtime_get_project_runtime_context(
            char_ptr project_id,
            out elias_handle_t runtime_context);


        [DllImport(PluginInfo.DesigntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_designtime_reset")]
        public static extern int elias_designtime_reset();

        [DllImport(PluginInfo.DesigntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_designtime_shutdown")]
        public static extern int elias_designtime_shutdown();

        [DllImport(PluginInfo.DesigntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_designtime_export_single_model")]
        public static extern elias_designtime_bool_t elias_designtime_export_single_model(
            char_ptr project_uuid,
            elias_uuid_t model_id,
            elias_export_functions_t export_functions,
            char_ptr api_key );

        [DllImport(PluginInfo.DesigntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_designtime_export_project")]
        public static extern elias_designtime_bool_t elias_designtime_export_project(
            char_ptr project_uuid,
            elias_export_functions_t export_functions,
            elias_export_options_t options,
            char_ptr api_key );

        public delegate void elias_designtime_result_message_callback_t( char_ptr message, void_ptr user_data );

        [DllImport(PluginInfo.DesigntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_designtime_get_latest_invocation_result_message")]
        public static extern void elias_designtime_get_latest_invocation_result_message(
            elias_designtime_result_message_callback_t message_callback,
            void_ptr user_data );


        // Note: this is a bit of a hack. And is likely 
        // going to be changed in the future.
        // elias_id_as_string and elias_id_from_string
        // are acually defined as part of the runtime. 
        // But right now the runtime lib if is 
        // part of the the designtime lib too. 
        // This "hack" was made to avoid creating an 
        // dependency on the runtime assembly
        // in unity. These entry points are defined
        // in two places (here and in EliasRuntimeLib).

        [DllImport(PluginInfo.DesigntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_id_as_string")]
        public static extern  void elias_id_as_string(
            [In, Out] elias_uuid_t id,
            out elias_string_ptr_t buffer );

        [DllImport(PluginInfo.DesigntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_id_from_string")]
        public static extern void elias_id_from_string(
            [In, Out] elias_const_string_ptr_t id_str,
            out elias_uuid_t id );
    }
}
