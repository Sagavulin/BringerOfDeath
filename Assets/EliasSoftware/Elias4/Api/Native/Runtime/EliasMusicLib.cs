using System;
using System.Runtime.InteropServices;

namespace EliasSoftware.Elias4.Native.Runtime {

    using elias_const_string_ptr_t = char_ptr;

    public class EliasMusicLib {

        [DllImport(PluginInfo.RuntimeDllName,
            CallingConvention   = CallingConvention.Cdecl,
            CharSet             = CharSet.Ansi, 
            EntryPoint          = "elias_set_music_project")]
        public static extern elias_result_t elias_set_music_project(
            elias_handle_t context,
            [In, Out] elias_const_string_ptr_t project_data,
            [In, Out] elias_const_string_ptr_t base_path );
    }
}