using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using EliasSoftware.Elias4.Native;
using EliasSoftware.Elias4.Native.Runtime;
using EliasSoftware.Elias4.Common;
using Unity.Collections.LowLevel.Unsafe;
using System.Text.RegularExpressions;

namespace EliasSoftware.Elias4.Runtime {

    using void_ptr = IntPtr;
    using elias_runtime_reader_result_t = elias_result_t;
    using elias_reader_instance_t = IntPtr;
    using uint64_t = Int64;
    using uint8_t = Byte;
    using elias_memory_functions_ptr = IntPtr;

	public class EliasStandaloneRuntime :
		IEliasStandaloneRuntime
	{
		public static string EliasAssetsPath { get; set; }
		public static string EliasMusicPath { get; set; }
		private static HashSet<EliasID> existingAssetIds;

		private class ReaderContext {
			public FileStream stream;
			public ulong ObjRef { get; private set; }
			public IntPtr ThisPtr { get; private set; }
			public void Free() {
				UnsafeUtility.ReleaseGCObject(ObjRef);
			}

			public static unsafe ReaderContext CreateNew() {
				var ctx = new ReaderContext();
				ulong h;
				ctx.ThisPtr = (IntPtr)UnsafeUtility.PinGCObjectAndGetAddress(ctx, out h);
				ctx.ObjRef = h;
				Debug.Assert(ctx.ThisPtr != IntPtr.Zero);
				return ctx;
			}

			public static ReaderContext FromPtr(IntPtr ptr) {
				ReaderContext context = UnsafeUtility.As<IntPtr, ReaderContext>(ref ptr);
				return context;
			}
		}

        public string HexStringFromEliasID(EliasID id) {
			char_buf str = new char_buf();
			elias_uuid_t nativeId = id.UUID;
			EliasRuntimeLib.elias_id_as_string(nativeId, out str);
			return str;
		}

		public EliasID EliasIDFromHexString(string idStr) {
			char_buf str = idStr;
			elias_uuid_t id;
			EliasRuntimeLib.elias_id_from_string(str, out id);
			return new EliasID(id);
		}

		[AOT.MonoPInvokeCallback(typeof(Native.Runtime.create_instance))]
		private static elias_runtime_reader_result_t CreateInstance(
			elias_asset_export_mode_t export_mode,
			elias_memory_functions_ptr memory_functions,
			void_ptr user,
			out elias_reader_instance_t instance )
		{
			try {
				var importer = ReaderContext.CreateNew();
				instance = importer.ThisPtr;
			} catch (Exception e) {
				Debug.LogException(e);
				instance = IntPtr.Zero;
				return elias_runtime_reader_result_t.elias_error_io_failure;
			}
			return elias_runtime_reader_result_t.elias_success;
		}

		[AOT.MonoPInvokeCallback(typeof(Native.Runtime.free_instance))]
        private static void FreeInstance( elias_reader_instance_t instance,
                elias_memory_functions_ptr memory_functions )
		{
			try {
				var readerHandle = ReaderContext.FromPtr(instance);
				readerHandle.Free();
			} catch (Exception e) {
				Debug.LogException(e);
			}
		}

		private static string GetAsssetPath(string ctx_id) {
			if(Regex.IsMatch(ctx_id, @"\A\b[0-9a-f]+\b\Z"))
				return Path.Combine(EliasAssetsPath, ctx_id);
			return Path.Combine(EliasMusicPath, ctx_id);
		}

		[AOT.MonoPInvokeCallback(typeof(Native.Runtime.open_context))]
        private static elias_runtime_reader_result_t OpenContext(
                elias_reader_instance_t instance,
                char_ptr context_id )
		{
			try {
				var path = GetAsssetPath(context_id);
				Debug.Log("OpenContext: " + path);
				var importer = ReaderContext.FromPtr(instance);
				var stream = new FileStream(path,
					FileMode.Open,
					FileAccess.Read,
					FileShare.None);
				importer.stream = stream;
			} catch (Exception e) {
				Debug.LogException(e);
				return elias_runtime_reader_result_t.elias_error_io_failure;
			}
			return elias_runtime_reader_result_t.elias_success;
		}

		[AOT.MonoPInvokeCallback(typeof(Native.Runtime.close_context))]
        private static void CloseContext( elias_reader_instance_t instance ) {
			try {
				var importer = ReaderContext.FromPtr(instance);
				importer.stream.Dispose();
				importer.stream = null;
			} catch (Exception e) {
				Debug.LogException(e);
			}
		}

		[AOT.MonoPInvokeCallback(typeof(read))]
        private static elias_runtime_reader_result_t Read(
                elias_reader_instance_t instance,
                uint64_t bytes_to_read,
                uint8_t urgent,
                IntPtr buffer,
                out uint64_t bytes_read )
		{
			try {
				Debug.Assert(bytes_to_read < int.MaxValue,
					"The requested chunk is too big");
				var importer = ReaderContext.FromPtr(instance);
				Debug.Assert(importer.stream != null);
				byte[] bytes = new byte[(int)bytes_to_read];
				bytes_read = importer.stream.Read(bytes, 0, (int)bytes_to_read);
				Marshal.Copy(bytes, 0, buffer, (int)bytes_read);
			} catch (Exception e) {
				Debug.LogException(e);
				bytes_read = 0;
				return elias_runtime_reader_result_t.elias_error_io_failure;
			}
			return elias_runtime_reader_result_t.elias_success;
		}

		[AOT.MonoPInvokeCallback(typeof(seek))]
        private static elias_runtime_reader_result_t Seek(
            elias_reader_instance_t instance,
            uint64_t offset_in_bytes )
		{
			try {
				var importer = ReaderContext.FromPtr(instance);
				if(importer.stream.CanSeek) {
					importer.stream.Seek(offset_in_bytes, SeekOrigin.Begin);
					return elias_runtime_reader_result_t.elias_success;
				}
				return elias_runtime_reader_result_t.elias_error_io_failure;
			} catch (Exception e) {
				Debug.LogException(e);
				return elias_runtime_reader_result_t.elias_error_io_failure;
			}
		}

		[AOT.MonoPInvokeCallback(typeof(get_size))]
        private static elias_runtime_reader_result_t GetSize(
            elias_reader_instance_t instance,
            out uint64_t size )
		{
			try {
				var importer = ReaderContext.FromPtr(instance);
				size = importer.stream.Length;
				return elias_runtime_reader_result_t.elias_success;
			} catch (Exception e) {
				Debug.LogException(e);
			}
			size = 0;
			return elias_runtime_reader_result_t.elias_error_io_failure;
		}

		[AOT.MonoPInvokeCallback(typeof(get_position))]
        private static elias_runtime_reader_result_t GetPosition(
            elias_reader_instance_t instance,
            out uint64_t position )
		{
			try {
				var importer = ReaderContext.FromPtr(instance);
				position = importer.stream.Position;
				return elias_runtime_reader_result_t.elias_success;
			} catch (Exception e) {
				Debug.LogException(e);
			}
			position = 0;
			return elias_runtime_reader_result_t.elias_error_io_failure;
		}

		public void Free(RuntimeHandle runHandle)
		{
			var res = EliasRuntimeLib.elias_runtime_free(runHandle.Handle);
			Debug.Assert(res == elias_runtime_reader_result_t.elias_success,
				"Failed to free elias runtime");
		}

		private void ReadUpIndexData(string indexFileName="index") {
			var path = Path.Combine(EliasAssetsPath, indexFileName);
			var content = File.ReadAllLines(path);
			existingAssetIds = new HashSet<EliasID>();
			foreach(var id in content) {
				existingAssetIds.Add(EliasID.FromString(id));
			}
		}

		private static elias_runtime_reader_functions_t readerFun = new elias_runtime_reader_functions_t() {
				open_context = OpenContext,
				close_context = CloseContext,
				create_instance = CreateInstance,
				free_instance = FreeInstance,
				get_position = GetPosition,
				get_size = GetSize,
				read = Read,
				seek = Seek,
				user = IntPtr.Zero,
			};

		public bool Initialize(out RuntimeHandle runHandle)
		{
			runHandle = RuntimeHandle.InvalidHandle;

			ReadUpIndexData();

			var result = EliasRuntimeLib.elias_initialize_global_allocation(IntPtr.Zero);
			Debug.Assert(result == elias_runtime_reader_result_t.elias_success,
				"Failed to initialize global memory functions");

			elias_handle_t handle = elias_handle_t.NullHandle;
			if(result == elias_runtime_reader_result_t.elias_success) {

				result = EliasRuntimeLib.elias_runtime_initialize(
					IntPtr.Zero,
					readerFun,
					readerFun,
					out handle);
				Debug.Assert(result == elias_runtime_reader_result_t.elias_success,
					"Failed to initialize the elias runtime");
			}

			runHandle = new RuntimeHandle(handle);
			return result == elias_runtime_reader_result_t.elias_success;
		}

		public bool AssetExists(EliasID id) {
			return existingAssetIds.Contains(id);
		}
	}

}
