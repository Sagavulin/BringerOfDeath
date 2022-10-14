using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Runtime {

    using EliasSoftware.Elias4;
    using EliasSoftware.Elias4.Common;

#if (UNITY_STANDALONE && !UNITY_EDITOR)

	public class EliasStandaloneAssetTableReader :
		IAssetTableReader
	{
        private EliasAssetTable table;
        public string AssetMetadataPath { get; private set; }
        public static EliasStandaloneAssetTableReader Create(string assetMetadataPath) {
            var tableReader = new EliasStandaloneAssetTableReader
            {
	            AssetMetadataPath = System.IO.Path.Combine(assetMetadataPath, "EliasAssets.json")
            };
            tableReader.Init();
            return tableReader;
        }

        public void Init()
		{
			table = ScriptableObject.CreateInstance<EliasAssetTable>();
			JsonUtility.FromJsonOverwrite(
				File.ReadAllText(AssetMetadataPath),
				table);
		}

        private Entry[] GetEntries(EntryType entryType) {

            if(table == null) {
                Debug.LogError("Table was null");
                return null;
            }

            if(entryType == EntryType.Enum || entryType == EntryType.EnumValue)
                return table.enumEntries;
            else if(entryType == EntryType.Patch || entryType == EntryType.PatchParam)
                return table.patchEntries;
            else if(entryType == EntryType.IR)
                return table.irEntries;
            else if(entryType == EntryType.GlobalParam)
                return table.globalParamEntries;
            else {
                Debug.LogErrorFormat("No table found for entry type {0}",
                    entryType.ToString());
                return null;
            }
        }

        private IEnumerable<Entry> EnumerateAllArrays() {
            if(table.globalParamEntries != null)
                foreach(var e in table.globalParamEntries)
                    yield return e;
            if(table.enumEntries != null)
                foreach(var e in table.enumEntries)
                    yield return e;
            if(table.enumEntries != null)
                foreach(var e in table.enumEntries)
                    yield return e;
            if(table.irEntries != null)
                foreach(var e in table.irEntries)
                    yield return e;
        }

        public IEntry Get(EntryType entryType, string name)
		{
            var entries = GetEntries(entryType);
            if(entries == null)
                return null;

			foreach(var e in entries)
            {
                if(e.entryType == entryType && e.entryName == name)
                    return e;
            }

            return null;
		}

        public IEnumerable<IEntry> GetAll() {
            foreach(var e in EnumerateAllArrays())
                yield return e;
        }

        public IEnumerable<IEntry> GetAll(string baseId)
		{
			foreach(var e in EnumerateAllArrays())
            {
                if(e.baseId == baseId)
                    yield return e;
            }
		}

        public IEnumerable<IEntry> GetAll(EntryType entryType)
		{
            var entries = GetEntries(entryType);
            if(entries == null)
                yield break;
			foreach(var e in entries)
            {
                if(e.entryType == entryType)
                    yield return e;
            }
		}

        private Entry FindFirst(IEliasID baseId) {
            foreach(var e in EnumerateAllArrays())
            {
                if(e.BaseID.Equals(baseId))
                    return e;
            }
            return null;
        }

		public IEnumerable<IEntry> GetAll(IEliasID baseId)
            => GetAll(baseId.ToString());

		public IEntry Get(EntryType entryType, IEliasID baseId, string entryName)
		{
			foreach(var e in GetAll(baseId)) {
                if(e.Name == entryName)
                    return e;
            }
            return null;
		}

        private string IdToString(IEliasID id) => id.ToString();
        private IEliasID StringToId(string id) => EliasID.FromString(id);

		public IEntry Get(EntryType entryType, IEliasID baseId)
		{
            var strId = IdToString(baseId);
            var entries = GetEntries(entryType);
            if( entries == null )
                return null;
			foreach(var e in entries)
            {
                if( e.entryType == entryType
                    && e.baseId == strId)
                    return e;
            }
            return null;
		}

		public IEntry Get(EntryType entryType, IEliasID baseId, IEliasID entryId)
		{
            var strBaseId = IdToString(baseId);
            var entries = GetEntries(entryType);
            if( entries == null )
                return null;
			foreach(var e in entries)
            {
                if( e.entryType == entryType
                    && e.baseId == strBaseId
                    && EliasID.FromString(e.entryId).Equals(entryId))
                    return e;
            }
            return null;
		}

	}

	public class EliasStanaloneAssets :
		IEliasAssets
	{
        public static EliasStanaloneAssets Create(string assetMetadataPath) {
            var assets = new EliasStanaloneAssets
            {
	            tableAccess = EliasStandaloneAssetTableReader.Create(assetMetadataPath)
            };
            return assets;
        }

        private IAssetTableReader tableAccess;


		public IPatchID GetPatchID(string patchName)
		{
			var entry = tableAccess.Get(EntryType.Patch, patchName);
            if( entry == null )
                return EliasID.Invalid;
            return entry.EntryID;
		}

		public (IParameterID, ParamType, string) GetParameterInfo(IPatchID patchId, string parameterName)
		{
			var entry = tableAccess.Get(EntryType.PatchParam, patchId, parameterName);
            if( entry == null )
                return (EliasID.Invalid, ParamType.Empty, string.Empty);
            return (entry.EntryID, entry.ValueType, entry.Data);
		}

		public IEnumID GetEnumID(string enumName)
		{
			var entry = tableAccess.Get(EntryType.Enum, enumName);
            if( entry == null )
                return EliasID.Invalid;
            return entry.EntryID;
		}

		public IEnumValueID GetEnumValueID(IEnumID enumId, string enumValue)
		{
			var entry = tableAccess.Get(EntryType.EnumValue, enumId, enumValue);
            if( entry == null )
                return EliasID.Invalid;
            return entry.EntryID;
		}

        public IEnumValueID GetEnumValueID(string enumValue)
		{
			var entry = tableAccess.Get(EntryType.EnumValue, enumValue);
            if( entry == null )
                return EliasID.Invalid;
            return entry.EntryID;
		}

        public IIRID GetIRID(string irName)
		{
			var entry = tableAccess.Get(EntryType.IR, irName);
            if( entry == null )
                return EliasID.Invalid;
            return entry.EntryID;
		}

		public string GetName(IPatchID patchId)
		{
			var entry = tableAccess.Get(EntryType.Patch, patchId);
            if( entry == null )
                return null;
            return entry.Name;
		}

		public string GetName(IPatchID patchId, IParameterID paramId)
		{
			var entry = tableAccess.Get(EntryType.PatchParam, patchId, paramId);
            if( entry == null )
                return null;
            return entry.Name;
		}

		public string GetName(IEnumID enumId)
		{
			var entry = tableAccess.Get(EntryType.Enum, enumId);
            if( entry == null )
                return null;
            return entry.Name;
		}

		public string GetName(IEnumID enumId, IEnumValueID valueId)
		{
			var entry = tableAccess.Get(EntryType.EnumValue, enumId, valueId);
            if( entry == null )
                return null;
            return entry.Name;
		}

        public string GetName(IEnumValueID valueId)
		{
			var entry = tableAccess.Get(EntryType.EnumValue, valueId);
            if( entry == null )
                return null;
            return entry.Name;
		}

		public string GetName(IIRID irId)
		{
			var entry = tableAccess.Get(EntryType.IR, irId);
            if( entry == null )
                return null;
            return entry.Name;
		}

        public IEnumerable<(string, IEliasID)> Enumerate(EntryType type) {
            foreach(var e in tableAccess.GetAll(type))
                yield return (e.Name, e.EntryID);
        }

		public IEnumerable<(string, IPatchID)> EnumeratePatches()
		{
            foreach(var e in tableAccess.GetAll(EntryType.Patch))
                yield return (e.Name, e.EntryID);
		}

        public IEnumerable<(string, IEnumID)> EnumerateEnums()
		{
            foreach(var e in tableAccess.GetAll(EntryType.Enum))
                yield return (e.Name, e.EntryID);
		}

        public IEnumerable<(string, IIRID)> EnumerateIRs()
		{
            foreach(var e in tableAccess.GetAll(EntryType.IR))
                yield return (e.Name, e.EntryID);
		}

        public IEnumerable<(string, IParameterID, ParamType, string)> EnumeratePatchParameters(IPatchID patchId)
		{
            foreach(var e in tableAccess.GetAll(patchId)) {
                if(e.Type == EntryType.PatchParam)
                    yield return (e.Name, e.EntryID, e.ValueType, e.Data);
            }
		}

        public IEnumerable<(string, IParameterID, ParamType, string)> EnumerateGlobalParameters()
		{
            foreach(var e in tableAccess.GetAll(EntryType.GlobalParam)) {
                yield return (e.Name, e.EntryID, e.ValueType, e.Data);
            }
		}

        public IEnumerable<(string, IEnumValueID)> EnumerateEnumDomain(IEnumID enumId)
		{
            foreach(var e in tableAccess.GetAll(enumId)) {
                if(e.Type == EntryType.EnumValue)
                    yield return (e.Name, e.EntryID);
            }
		}

        public IParameterID GetGlobalParameter(string name)
		{
			var entry = tableAccess.Get(EntryType.GlobalParam, name);
            if(entry == null)
                return null;
            return entry.EntryID;
		}

		public string GetGlobalParameterName(IParameterID paramId)
		{
			var entry = tableAccess.Get(EntryType.GlobalParam, paramId);
            if(entry == null)
                return null;
            return entry.Name;
		}
	}

	public class EliasStandalone :
		Elias
	{

        private IEliasAssets assets;
        private IEliasRuntime runtime;

        private static EliasStandaloneRuntime runtimeLifecycle;
        private static AudioReader audioReader;

        private static RuntimeHandle eliasLibHandle = RuntimeHandle.InvalidHandle;

        private static EliasStandalone thisInstance;
        private Dictionary<string, (IParameterID, ParamType, string)> parameters = null;

        private static string musicBasePath = Path.Combine(Application.streamingAssetsPath, "Elias4Music");
        private static string projectBasePath = Path.Combine(Application.streamingAssetsPath, "Elias4");

        public override IEliasAssets Assets => assets;
		public override IEliasRuntime Runtime => runtime;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() {
            thisInstance = new EliasStandalone();
            Setup(EliasSettings.Instance, thisInstance);
            EliasStandaloneRuntime.EliasAssetsPath = projectBasePath;
            EliasStandaloneRuntime.EliasMusicPath = musicBasePath;
            runtimeLifecycle = new EliasStandaloneRuntime();
            var res = runtimeLifecycle.Initialize(out eliasLibHandle);
            Debug.Assert(res, "Failed to initialize runtime");
            if(res) {
                Application.quitting += Destroy;
            }
        }

        private static void Destroy() {
            if(runtimeLifecycle == null)
                return;
            if(thisInstance != null)
                thisInstance.Stop();
            runtimeLifecycle.Free(eliasLibHandle);
            runtimeLifecycle = null;
        }

        protected override bool IsValidAssetID(IEliasID assetId) {
            Debug.Assert(assetId is EliasID);
            if(Runtime == null && assetId is EliasID)
                return false;
            var id = (EliasID)assetId;
            return runtimeLifecycle.AssetExists(id);
        }

        private void ReadUpGlobalParameters() {
            parameters = new Dictionary<string, (IParameterID, ParamType, string)>();
            foreach(var param in Assets.EnumerateGlobalParameters()) {
                if(parameters.ContainsKey(param.Item1) == false)
                    parameters[param.Item1] = (param.Item2, param.Item3, param.Item4);
            }
        }

        public override void SetGlobalParameter(string name, EliasValue value)
        {
            if( parameters == null ) {
                Debug.LogError("Global parameter table was not created");
                return;
            }

            if( parameters.ContainsKey(name) == false ) {
                Debug.LogWarningFormat("Global parameter name {0} was not found", name);
                return;
            }

            SetGlobalParameterValue(parameters[name].Item1, value);
        }

        public override void SetGlobalParameter(string name, IEliasValueBuilder value)
        {
            if( parameters == null ) {
                Debug.LogError("Global parameter table was not created");
                return;
            }

            if( parameters.ContainsKey(name) == false ) {
                Debug.LogWarningFormat("Global parameter name {0} was not found", name);
                return;
            }

            SetGlobalParameterValue(parameters[name].Item1, value);
        }

        public void LoadMusicProject(IEliasRuntime runtime, string musicProjectPath)
        {
            if(System.IO.File.Exists(musicProjectPath) == false) {
                Debug.LogWarningFormat("Could not locate the music project at the provided path:\n\t{0}\n",
                    musicBasePath);
                return;
            }
            var content = System.IO.File.ReadAllText(musicProjectPath,
                System.Text.Encoding.UTF8);
            var res = runtime.SetMusicProject(content, null);
            Debug.AssertFormat(res, "Failed to load music project {0}", musicProjectPath);
        }

		protected override void Start(IEliasPaths settings)
		{
            if(runtimeLifecycle == null) {
                Debug.LogWarning("Elias has been shutdown");
                return;
            }

            assets = EliasStanaloneAssets.Create(projectBasePath);
            if(eliasLibHandle.IsValid) {
                runtime = new EliasRuntime(eliasLibHandle);
                ReadUpGlobalParameters();
            }

            var musicProject = settings.MusicProjectFilePath;
            if( settings.AutoLoadMusicProject
                && musicProject != null
                && musicProject.Length > 0 )
            {
                var file = Path.GetFileName(musicProject);
                Debug.Log(Path.Combine(musicBasePath, file));
                LoadMusicProject(runtime, Path.Combine(musicBasePath, file));
            }

            SessionId = runtime.CreateSessionWithManualRendering();
            Debug.Assert(SessionId.IsValid);
            audioReader = AudioReader.MakeInstance();
            audioReader.Runtime = runtime;
		}

		protected override void Stop()
		{
            if (audioReader != null) {
                audioReader.Destroy();
            }

            if( runtime != null ) {
                if(SessionId.IsValid)
                    runtime.DestroySession(SessionId);
                SessionId = new SessionID(0);
                runtime = null;
            }

            assets = null;
		}
	}
#endif
}
