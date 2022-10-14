using System;
using System.Runtime.InteropServices;
using EliasSoftware.Elias4.Native;

namespace EliasSoftware.Elias4.Native.Runtime {

    using void_ptr = IntPtr;
    using elias_const_string_ptr_t = char_buf;
    using elias_read_context_id_t = char_buf;
    using elias_runtime_reader_result_t = elias_result_t;
    using elias_reader_instance_t = IntPtr;
    using uint64_t = Int64;
    using elias_string_ptr_t = char_buf;
    using uint8_t = Byte;
    using elias_memory_functions_ptr = IntPtr;

    // don't worry, these are not used :)
    public delegate void_ptr malloc( ulong bytes, void_ptr user );
    public delegate void free( void_ptr pointer, void_ptr user );
    public delegate void realloc( void_ptr pointer, ulong bytes, void_ptr user );

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_memory_functions
    {
        public malloc malloc;
        public free free;
        public realloc realloc;
        public void_ptr user;
    }

    public delegate elias_runtime_reader_result_t create_instance(
            elias_asset_export_mode_t export_mode,
            elias_memory_functions_ptr memory_functions,
            void_ptr user,
            out elias_reader_instance_t instance );

    public delegate void free_instance(
            elias_reader_instance_t instance,
            elias_memory_functions_ptr memory_functions );

    public delegate elias_runtime_reader_result_t open_context(
            elias_reader_instance_t instance,
            char_ptr context_id );

    public delegate void close_context(
            elias_reader_instance_t instance );

    public delegate elias_runtime_reader_result_t read(
            elias_reader_instance_t instance,
            uint64_t bytes_to_read,
            uint8_t urgent,
            IntPtr buffer,
            out uint64_t bytes_read );

    public delegate elias_runtime_reader_result_t seek(
            elias_reader_instance_t instance,
            uint64_t offset_in_bytes );

    public delegate elias_runtime_reader_result_t get_size(
            elias_reader_instance_t instance,
            out uint64_t size );

    public delegate elias_runtime_reader_result_t get_position(
            elias_reader_instance_t instance,
            out uint64_t position );


    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_runtime_reader_functions_t
    {
        public create_instance create_instance;
        public free_instance free_instance;
        public open_context open_context;
        public close_context close_context;
        public read read;
        public seek seek;
        public get_size get_size;
        public get_position get_position;
        public void_ptr user;
    }

    public class EliasRuntimeLib {


        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi,
            EntryPoint          = "elias_initialize_global_allocation")]
        public static extern elias_result_t elias_initialize_global_allocation(
            elias_memory_functions_ptr memory_functions );


        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi,
            EntryPoint          = "elias_runtime_initialize")]
        public static extern elias_result_t elias_runtime_initialize(
            elias_memory_functions_ptr memory_functions,
            in elias_runtime_reader_functions_t standard_reader_functions,
            in elias_runtime_reader_functions_t streaming_reader_functions,
            out elias_handle_t context );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi,
            EntryPoint          = "elias_runtime_free")]
        public static extern elias_result_t elias_runtime_free(
            elias_handle_t context );

        // these entrypoints may go away
        // or be moved in future updates

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi,
            EntryPoint          = "elias_id_as_string")]
        public static extern  void elias_id_as_string(
            [In, Out] elias_uuid_t id,
            out elias_string_ptr_t buffer );

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi,
            EntryPoint          = "elias_id_from_string")]
        public static extern void elias_id_from_string(
            [In, Out] elias_const_string_ptr_t id_str,
            out elias_uuid_t id );
    }
}
