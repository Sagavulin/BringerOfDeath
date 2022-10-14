using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace EliasSoftware.Elias4.Editor {

    using EliasSoftware.Elias4;
    using EliasSoftware.Elias4.Common;
    using EliasSoftware.Elias4.Designtime;

	public class EliasDesigntimeAssets :
        IEliasDesigntimeAssets
	{
        private GQLProject project;
        private IAssetTableWriter tableAccess;
        public GQLProject Project => project;

        private EliasDesigntimeAssets(GQLProject projectData, IAssetTableWriter tableIO)
        {
            project = projectData;
            tableAccess = tableIO;
        }

        public static EliasDesigntimeAssets Create(GQLProject gqlProject,
            string assetPath)
        {
            var tableWriter = new DesigntimeAssetTable(assetPath);
            tableWriter.Init();

            var assetDb = new EliasDesigntimeAssets(gqlProject, tableWriter);
            return assetDb;
        }

        private bool IsValidEnum(GQLEnum _enum) {
            return _enum != null && _enum.id != null
                && _enum.name != null
                && _enum.values != null
                && _enum.id.Length > 0
                && _enum.name.Length > 0
                && _enum.values.Count > 0;
        }

        private bool IsValidPatch(GQLPatch patch) {
            return patch != null && patch.id != null
                && patch.name != null
                && patch.parameters != null
                && patch.id.Length > 0
                && patch.name.Length > 0;
        }

        private bool IsValidAsset(GQLAsset asset) {
            return asset != null && asset.id != null
                && asset.name != null
                && asset.id.Length > 0
                && asset.name.Length > 0;
        }

        private IEnumerable<GQLEnum> RequestLoadAllEnums() {
            var matches = DesigntimeAPI.ListAssetsByTypeName(project, "enum");
            foreach(var match in matches) {
                var asset = DesigntimeAPI.LoadAsset(project, match.path);
                var _enum = DesigntimeAPI.GetEnum(project, match.uri);
                if(IsValidEnum(_enum))
                    yield return _enum;
            }
        }

        private IEnumerable<GQLPatch> RequestLoadAllPatches() {
            var matches = DesigntimeAPI.ListAssetsByTypeName(project, "patch");
            foreach(var match in matches) {
                var asset = DesigntimeAPI.LoadAsset(project, match.path);
                var patch = DesigntimeAPI.GetPatch(project, match.uri);
                if(IsValidPatch(patch))
                    yield return patch;
            }
        }

        private IEnumerable<GQLPatch> RequestLoadAllPatchConfigs() {
            var matches = DesigntimeAPI.ListAssetsByTypeName(project, "config");
            foreach(var match in matches) {
                var asset = DesigntimeAPI.LoadAsset(project, match.path);
                var patch = DesigntimeAPI.GetConfig(project, match.uri);
                if(IsValidPatch(patch))
                    yield return patch;
            }
        }

        private IEnumerable<GQLAsset> RequestLoadAllIRs() {
            var matches = DesigntimeAPI.ListAssetsByTypeName(project, "ir");
            foreach(var match in matches) {
                var asset = DesigntimeAPI.LoadAsset(project, match.path);
                var ir = DesigntimeAPI.GetIR(project, match.uri);
                if(IsValidAsset(ir))
                    yield return ir;
            }
        }

        private IEnumerable<GQLParameter> RequestLoadAllGlobalParams() {
            var matches = DesigntimeAPI.ListGlobalParameters(project);
            foreach(var m in matches)
                yield return m;
        }

        public void ClearAllData() {
            tableAccess.Clear();
        }

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

        public enum LoaderResult {
            Done,
            Error,
            WaitingForProjectScanner,
            PatchModified,
            EnumModified,
            IRModified,
            GlobalParamModified,
            NoChange
        }

        private ParamType GetValueType(GQLParameter param) {

            var typeName = "null-param";
            if(param != null) {
                var paramType = param.type;
                if(paramType != null) {
                    typeName = paramType.typeName != null
                        ? paramType.typeName
                        : "null-type-name";
                } else {
                    typeName = "null-type";
                }
            }

            switch(typeName) {
                case "Int":
                    return ParamType.Int;
                case "Pulse":
                    return ParamType.Pulse;
                case "Float":
                    return ParamType.Double;
                case "Boolean":
                    return ParamType.Bool;
                case "Enum":
                    return ParamType.EnumValue;
                default:
                    Debug.LogErrorFormat("Now known value type for {0}", typeName);
                    return ParamType.Empty;
            }
        }

        private string UUIDFromURI(string uri) {
            var rparen = uri.LastIndexOf("(");
            if(rparen < 0)
                return string.Empty;
            var start = rparen + 1;
            var end = uri.LastIndexOf(".Enum");
            if(end < 0 || start >= end)
                return string.Empty;
            return uri.Substring(start, (end-start));
        }

        private string GetParamData(GQLParameter param) {

            var paramType = GetValueType(param);

            var defaultValue = param != null ? param.defaultValue : null;
            if(defaultValue == null)
                return null;

            switch(paramType) {
                case ParamType.Int:
                    return param.defaultValue.intValue;
                case ParamType.Pulse:
                    return param.defaultValue.intValue;
                case ParamType.Bool:
                    return param.defaultValue.boolValue;
                case ParamType.Double:
                    return param.defaultValue.floatValue;
                case ParamType.EnumValue: // Special case (get Enum UUID)
                    var uuid = param.type.reference == null ? string.Empty : UUIDFromURI(param.type.reference);
                    return uuid;
                default:
                    Debug.LogErrorFormat("unhandled parameter type {0}",
                        paramType.ToString());
                    return null;
            }
        }

        private IEnumerable<LoaderResult> Worker() {

            var removedNames = new HashSet<string>();
            var keepIds = new HashSet<(string, string)>();

            int maxWaitScanningIterations = 100;
            bool keepWaiting = DesigntimeAPI.IsScanningProject(project);
            while( keepWaiting ) {
                Debug.Log("Elias: Scanning");
                yield return LoaderResult.WaitingForProjectScanner;
                keepWaiting = DesigntimeAPI.IsScanningProject(project)
                    && maxWaitScanningIterations > 0;
                maxWaitScanningIterations --;
            }

            foreach(var _enum in RequestLoadAllEnums()) {

                LoaderResult result = LoaderResult.NoChange;
                try {
                    IEnumID enumId = EliasID.FromString(_enum.id);
                    string enumIdStr = enumId.ToString();

                    removedNames.Clear();
                    tableAccess.RemoveAll(enumId, removedNames);

                    tableAccess.Add(EntryType.Enum, _enum.name, enumIdStr, enumIdStr, ParamType.Empty, null);
                    keepIds.Add((enumIdStr, enumIdStr));
                    if(removedNames.Contains(_enum.name) == false)
                        result = LoaderResult.EnumModified;

                    foreach(var value in _enum.values) {
                        var valId = EliasID.FromString(value.id);
                        string valIDStr = valId.ToString();
                        tableAccess.Add(EntryType.EnumValue, value.name, enumIdStr, valIDStr, ParamType.EnumValue, null);
                        keepIds.Add((enumIdStr,valIDStr));
                        if(removedNames.Contains(value.name) == false)
                            result = LoaderResult.EnumModified;
                    }
                }
                catch (Exception e) {
                    Debug.LogException(e);
                    result = LoaderResult.Error;
                }
                yield return result;
            }

            foreach(var patch in RequestLoadAllPatches()) {
                LoaderResult result = LoaderResult.NoChange;
                try {
                    var patchId = EliasID.FromString(patch.id);
                    string baseStrId = patchId.ToString();

                    removedNames.Clear();
                    tableAccess.RemoveAll(patchId, removedNames);
                    tableAccess.Add(EntryType.Patch, patch.name, baseStrId, baseStrId, ParamType.Empty, null);
                    keepIds.Add((baseStrId, baseStrId));

                    if(removedNames.Contains(patch.name) == false)
                        result = LoaderResult.PatchModified;

                    foreach(var param in patch.parameters) {
                        string paramIdStr = EliasID.FromString(param.id).ToString();
                        tableAccess.Add(EntryType.PatchParam, param.name, baseStrId, paramIdStr, GetValueType(param), GetParamData(param));
                        keepIds.Add((baseStrId, paramIdStr));
                        if(removedNames.Contains(param.name) == false)
                            result = LoaderResult.PatchModified;
                    }

                } catch (Exception e) {
                    Debug.LogException(e);
                    result = LoaderResult.Error;
                }
                yield return result;
            }

            foreach(var patchConfig in RequestLoadAllPatchConfigs()) {
                LoaderResult result = LoaderResult.NoChange;
                try {
                    var patchId = EliasID.FromString(patchConfig.id);
                    string baseStrId = patchId.ToString();

                    removedNames.Clear();
                    tableAccess.RemoveAll(patchId, removedNames);

                    tableAccess.Add(EntryType.Patch, patchConfig.name, baseStrId, baseStrId, ParamType.Empty, null);
                    keepIds.Add((baseStrId, baseStrId));

                    if(removedNames.Contains(patchConfig.name) == false)
                        result = LoaderResult.PatchModified;

                    foreach(var param in patchConfig.parameters) {
                        string paramIdStr = EliasID.FromString(param.id).ToString();
                        tableAccess.Add(EntryType.PatchParam, param.name, baseStrId, paramIdStr, GetValueType(param), GetParamData(param));
                        keepIds.Add((baseStrId, paramIdStr));
                        if(removedNames.Contains(param.name) == false)
                            result = LoaderResult.PatchModified;
                    }

                } catch (Exception e) {
                    Debug.LogException(e);
                    result = LoaderResult.Error;
                }
                yield return result;
            }

            foreach(var ir in RequestLoadAllIRs()) {
                LoaderResult result = LoaderResult.NoChange;
                try {
                    IIRID irId = EliasID.FromString(ir.id);
                    string irIdStr = irId.ToString();

                    removedNames.Clear();
                    tableAccess.RemoveAll(irId, removedNames);
                    if(removedNames.Contains(ir.name) == false)
                        result = LoaderResult.IRModified;

                    tableAccess.Add(EntryType.IR, ir.name, irIdStr, irIdStr, ParamType.Empty, null);
                    keepIds.Add((irIdStr, irIdStr));
                }
                catch (Exception e) {
                    Debug.LogException(e);
                    result = LoaderResult.Error;
                }
                yield return result;
            }

            foreach(var param in RequestLoadAllGlobalParams()) {
                LoaderResult result = LoaderResult.NoChange;
                try {
                    IParameterID paramId = EliasID.FromString(param.id);
                    string idStr = paramId.ToString();

                    removedNames.Clear();
                    tableAccess.RemoveAll(paramId, removedNames);

                    if(removedNames.Contains(param.name) == false)
                        result = LoaderResult.GlobalParamModified;

                    tableAccess.Add(EntryType.GlobalParam,
                        param.name,
                        idStr,
                        idStr,
                        GetValueType(param),
                        GetParamData(param));
                    keepIds.Add((idStr, idStr));
                }
                catch (Exception e) {
                    Debug.LogException(e);
                    result = LoaderResult.Error;
                }

                yield return result;
            }

            tableAccess.RemoveAllExcept(keepIds);

	        // we keep the iterator in state=Done
	        // until it gets reset
            while(true) {
                yield return LoaderResult.Done;
            }
        }

        private IEnumerator<LoaderResult> loader = null;
        private int currentRefreshBulkSize = 1;
        private int refreshIntervalSeconds = 30;
        private DateTime lastRefreshStartTime = DateTime.Now;

        private void EditorUpdate() {

            if(Application.isPlaying || Elias.IsInitialized == false)
                return;

            if(loader == null) {
                loader = Worker().GetEnumerator();
                lastRefreshStartTime = DateTime.Now;
                //Debug.Log("Starting new refresh cycle");
            }

            int cntr = currentRefreshBulkSize;
            while( cntr > 0 ) {
                loader.MoveNext();
                cntr = cntr - 1;
            }

            if( loader.Current == LoaderResult.Done ) {
                var timeSinceLastRefresh = DateTime.Now - lastRefreshStartTime;
                if(timeSinceLastRefresh.Seconds >= refreshIntervalSeconds)
                    loader = null;
            }
        }

		public void SetRefreshOptions(int size, int interval)
		{
            refreshIntervalSeconds = interval;
            currentRefreshBulkSize = UnityEngine.Mathf.Max(0, size);
		}

        public void ForceRefreshNow() {
            tableAccess.Clear();
            loader = Worker().GetEnumerator();
            bool hasNext = true;
            while(hasNext && loader.Current != LoaderResult.Done) {
                hasNext = loader.MoveNext();
            }
            AssetDatabase.SaveAssets();
            loader = null;
        }

		public void Start()
		{
			EditorApplication.update += EditorUpdate;
		}

		public void Stop()
		{
			EditorApplication.update -= EditorUpdate;
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

		public string ExportToJson()
			=> tableAccess.ExportToJson();
	}
}
