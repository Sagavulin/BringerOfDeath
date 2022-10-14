using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Designtime {

    using System;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using Elias4.Native;
    using Elias4.Native.Designtime;
	using Elias4.Common;
	using System.Text.RegularExpressions;

	using UnityEditor;
	using UnityEngine;

    public class Export {
        public ApiConsumer ApiConsumer { get; set; }
		private string TargetDir { get; set; }

		private class ExportContext
		{
			public FileStream stream;
		}

		private elias_export_result_t ExportOpenContext(IntPtr user, char_ptr context_id)
		{
			var contextHandle = GCHandle.FromIntPtr(user);
			var context = (ExportContext)contextHandle.Target;

			try {
				EliasID id = EliasID.FromString(context_id.data);
				var name = EliasIDToHexString(id);
				if(name == null)
					throw new Exception(string.Format("could not convert id {0} to hex-string", context_id.data));
				var path = Path.Combine(TargetDir, name);
				context.stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
			} catch (Exception e) {
				Debug.LogException(e);
				return elias_export_result_t.elias_export_io_failure;
			}

			return elias_export_result_t.elias_export_success;
		}

		private elias_export_result_t ExportCloseContext(IntPtr user)
		{
			var contextHandle = GCHandle.FromIntPtr(user);
			var context = (ExportContext)contextHandle.Target;

			context.stream.Dispose();
			context.stream = null;

			return elias_export_result_t.elias_export_success;
		}

		private elias_export_result_t ExportWrite(IntPtr user, IntPtr data, uint64_t count)
		{
			var contextHandle = GCHandle.FromIntPtr(user);
			var context = (ExportContext)contextHandle.Target;

			var dataArray = new byte[count];
			Marshal.Copy(data, dataArray, 0, (int)count.value);

			context.stream.Write(dataArray, 0, (int)count.value);

			return elias_export_result_t.elias_export_success;
		}

		private string EliasIDToHexString(EliasID id) {
			char_buf str = new char_buf();
			elias_uuid_t nativeId = id.UUID;
			EliasDesigntimeLib.elias_id_as_string(nativeId, out str);
			return str;
		}

		private EliasID HexStringToEliasID(string idStr) {
			char_buf str = idStr;
			elias_uuid_t id;
			EliasDesigntimeLib.elias_id_from_string(str, out id);
			return new EliasID(id);
		}

		private bool IsHexStrID(string data) {
			return Regex.IsMatch(data, @"\A\b[0-9a-f]+\b\Z");
		}

		public IEnumerable<EliasID> EnumerateExportedProjectAssets( string targetDir ) {
			IEnumerable<string> lst = null;
			try {
				lst = Directory.EnumerateFiles(targetDir);
			} catch {
				yield break;
			}

			foreach(var f in lst) {
				var str = Path.GetFileName(f);
				if( IsHexStrID(str) ) {
					var id = EliasID.Invalid;
					try { id = HexStringToEliasID(str); } catch { }
					if ( id.IsValid )
						yield return id;
				}
			}
		}

		public bool ExportProject(GQLProject project, string targetDir)
		{
            if ( project == null || ApiConsumer == null ) {
				Debug.LogError("Project was null / Api Consumer was null");
				return false;
			}

			if ( project.uuid == null || project.uuid.Length == 0 || ApiConsumer.IsValid == false) {
				Debug.LogError("Invalid project UUID");
				return false;
			}

			TargetDir = targetDir;

			var exportContext = new ExportContext();
			var exportContextHandle = GCHandle.Alloc(exportContext, GCHandleType.Pinned);

			var exportFunctions = new elias_export_functions_t() {
				open_context  = ExportOpenContext,
				close_context = ExportCloseContext,
				write         = ExportWrite,
				user          = GCHandle.ToIntPtr(exportContextHandle),
			};

			var exportOptions = new elias_export_options_t();

			var result = EliasDesigntimeLib.elias_designtime_export_project(project.uuid,
                exportFunctions,
                exportOptions,
                ApiConsumer.ApiKey);

            Debug.AssertFormat(result > 0, "Project export failed {0}", project.uuid);

			exportContextHandle.Free();

			if(result == 0) {
				EliasDesigntimeLib.elias_designtime_get_latest_invocation_result_message(
					(msg, usr) => Debug.LogError(msg), IntPtr.Zero);
			}

            return result > 0;
		}
    }
}
