namespace EliasSoftware.Elias4.Native
{
	using System.Runtime.InteropServices;
	using System.Text;

	public static class PluginInfo
	{

	//TODO: Other Platforms
	//   UNITY_IOS && !UNITY_EDITOR 	: "__Internal"
	//   UNITY_ANDROID && !UNITY_EDITOR : "elias_designtime_shared_library"
	//   etc...

#if UNITY_EDITOR
		public const string DesigntimeDllName 	= "elias_designtime_shared_library";
		public const string RuntimeDllName 		= "elias_designtime_shared_library";
#elif UNITY_STANDALONE_WIN
		//#error Not implemented yet
		public const string RuntimeDllName 		= "elias_runtime_shared_library";
#else
		#error This version of the Unity Plugin only supports Windows builds.
		#error Please contact Elias Software for more information.
#endif
	}
}