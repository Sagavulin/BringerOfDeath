using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace EliasSoftware.Elias4.Designtime
{
    using EliasSoftware.Elias4.Common;
    
    public class DesigntimeAssetTable :
		IAssetTableWriter
	{
        private EliasAssetTable _table = null;
        private SerializedObject _tableObject = null;
        private SerializedObject TableObject {
            get {
                if(_tableObject == null || _table == null)
                    Init();
                return _tableObject;
            }
        }

        public DesigntimeAssetTable(string path) {
            AssetFilePath = path;
        }
        public string AssetFilePath { get; private set; }

        public void Init()
		{
            Debug.Assert(AssetFilePath != null && AssetFilePath.Length > 0);
			_table = Resources.FindObjectsOfTypeAll<EliasAssetTable>().FirstOrDefault();
			if (!_table)
				_table = AssetDatabase.LoadAssetAtPath<EliasAssetTable>(AssetFilePath);
			if (!_table)
			{
				Debug.Log("Could not find EliasAssetTable, creating new ... \n" + AssetFilePath);
				_table = ScriptableObject.CreateInstance<EliasAssetTable>();
				AssetDatabase.CreateAsset(_table, AssetFilePath);
				AssetDatabase.SaveAssets();
			}
			if (!_table)
				Debug.LogError("singleton asset missing: " + AssetFilePath);
            
            _tableObject = new SerializedObject(_table);
            _tableObject.Update();
		}

        private SerializedProperty GetEntries(EntryType entryType) {
            TableObject.UpdateIfRequiredOrScript();
            if(entryType == EntryType.Enum || entryType == EntryType.EnumValue)
                return TableObject.FindProperty(nameof(EliasAssetTable.enumEntries));
            else if(entryType == EntryType.Patch || entryType == EntryType.PatchParam)
                return TableObject.FindProperty(nameof(EliasAssetTable.patchEntries));
            else if(entryType == EntryType.IR)
                return TableObject.FindProperty(nameof(EliasAssetTable.irEntries));
            else if(entryType == EntryType.GlobalParam)
                return TableObject.FindProperty(nameof(EliasAssetTable.globalParamEntries));
            else {
                Debug.LogErrorFormat("No table found for entry type {0}",
                    entryType.ToString());
                return null;
            }
        }

		public bool Add(EntryType entryType, string name, string baseId, string entryId, ParamType valueType = ParamType.Empty, string data = null)
		{
            TableObject.Update();

            var existing = Get(entryType, entryId);
            if(existing != null) {
                return false;
            }

            var entries = GetEntries(entryType);
            if(entries == null) {
                return false;
            }

            Debug.Assert(entries.isArray);
            TableObject.Update();
            entries.InsertArrayElementAtIndex(0);
            entries.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            entries.serializedObject.UpdateIfRequiredOrScript();
            var newEntry = entries.GetArrayElementAtIndex(0);
            newEntry.serializedObject.UpdateIfRequiredOrScript();
            Entry.Serialize( entryType, name, baseId, entryId, valueType, data, newEntry);
            TableObject.ApplyModifiedPropertiesWithoutUndo();
            return true;
		}

        private IEnumerable<Entry> EnumerateSerializedArray(SerializedProperty arrayProp) {
            if(arrayProp.serializedObject == null)
                yield break;
            arrayProp.serializedObject.UpdateIfRequiredOrScript();
            Debug.Assert(arrayProp.isArray);
            var size = arrayProp.arraySize;
            for(int i = 0; i < size; i++)
                yield return Entry.FromProperty(arrayProp.GetArrayElementAtIndex(i));
        }

        private IEnumerable<Entry> EnumerateAllArrays() {
            TableObject.Update();
            var enums = TableObject.FindProperty(nameof(EliasAssetTable.enumEntries));
            var globals = TableObject.FindProperty(nameof(EliasAssetTable.globalParamEntries));
            var irs = TableObject.FindProperty(nameof(EliasAssetTable.irEntries));
            var patches = TableObject.FindProperty(nameof(EliasAssetTable.patchEntries));
			foreach(var e in EnumerateSerializedArray(globals))
                yield return e;
			foreach(var e in EnumerateSerializedArray(enums))
                yield return e;
            foreach(var e in EnumerateSerializedArray(irs))
                yield return e;
            foreach(var e in EnumerateSerializedArray(patches))
                yield return e;
        }

        public IEntry Get(EntryType entryType, string name)
		{
            var entries = GetEntries(entryType);
            if(entries == null)
                return null;

			foreach(var e in EnumerateSerializedArray(entries))
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
			foreach(var e in EnumerateSerializedArray(entries))
            {
                if(e.entryType == entryType)
                    yield return e;
            }
		}

        private Entry FindFirst(IEliasID baseId) {
            //var idStr = IdToString(baseId);
            foreach(var e in EnumerateAllArrays())
            {
                if(e.BaseID.Equals(baseId))
                    return e;
            }
            return null;
        }

		public int RemoveAll(IEliasID baseId)
		{
            int count = 0;
            var target = FindFirst(baseId);
            if(target == null)
                return 0;
            var entries = GetEntries(target.entryType);

            entries.serializedObject.Update();

            int len = entries.arraySize;
            for(int i = len -1; i >= 0; i--) {
                var entry = Entry.FromProperty(entries);
                if(entry.baseId == target.baseId) {
                    entries.DeleteArrayElementAtIndex(i);
                    entries.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    entries.serializedObject.Update();
                    count ++;
                }
            }
            entries.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return count;
		}

        public void RemoveAll(IEliasID baseId, HashSet<string> removedNames)
		{
            var target = FindFirst(baseId);
            //Debug.Assert(target != null);
            if(target == null)
                return;

            var entries = GetEntries(target.entryType);
            entries.serializedObject.Update();

            int len = entries.arraySize;
            for(int i = len -1; i >= 0; i--) {
                var entry = Entry.FromProperty(entries.GetArrayElementAtIndex(i));
                if(entry.BaseID.Equals(baseId)) {
                    removedNames.Add(entry.Name);
                    entries.DeleteArrayElementAtIndex(i);
                    entries.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    entries.serializedObject.Update();
                }
            }
            entries.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            entries.serializedObject.Update();
		}

        private void RemoveAllExcept(SerializedProperty entries, HashSet<(string, string)> entryIdPairs) {
            entries.serializedObject.Update();
            int len = entries.arraySize;
            for(int i = len -1; i >= 0; i--) {
                var entry = Entry.FromProperty(entries.GetArrayElementAtIndex(i));
                if(entryIdPairs.Contains((entry.BaseID.ToString(), entry.EntryID.ToString())) == false) {
                    entries.DeleteArrayElementAtIndex(i);
                    entries.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    entries.serializedObject.Update();
                }
            }
            entries.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        
        public void RemoveAllExcept(HashSet<(string, string)> entryIds)
		{
            TableObject.Update();
            var enums = TableObject.FindProperty(nameof(EliasAssetTable.enumEntries));
            var globals = TableObject.FindProperty(nameof(EliasAssetTable.globalParamEntries));
            var irs = TableObject.FindProperty(nameof(EliasAssetTable.irEntries));
            var patches = TableObject.FindProperty(nameof(EliasAssetTable.patchEntries));
            RemoveAllExcept(enums, entryIds);
            RemoveAllExcept(globals, entryIds);
            RemoveAllExcept(irs, entryIds);
            RemoveAllExcept(patches, entryIds);
            TableObject.ApplyModifiedPropertiesWithoutUndo();
		}

		public IEnumerable<IEntry> GetAll(IEliasID baseId)
            => GetAll(baseId.UUID.ToString());

		public IEntry Get(EntryType entryType, IEliasID baseId, string entryName)
		{
            TableObject.Update();
			foreach(var e in GetAll(baseId)) {
                if(e.Name == entryName)
                    return e;
            }
            return null;
		}

        private string IdToString(IEliasID id) => id.UUID.ToString();
        private IEliasID StringToId(string id) => EliasID.FromString(id);

        public void Clear() {
            TableObject.Update();
            var enums = TableObject.FindProperty(nameof(EliasAssetTable.enumEntries));
            var globals = TableObject.FindProperty(nameof(EliasAssetTable.globalParamEntries));
            var irs = TableObject.FindProperty(nameof(EliasAssetTable.irEntries));
            var patches = TableObject.FindProperty(nameof(EliasAssetTable.patchEntries));
            enums.ClearArray();
            patches.ClearArray();
            globals.ClearArray();
            irs.ClearArray();
            TableObject.ApplyModifiedPropertiesWithoutUndo();
        }

		public IEntry Get(EntryType entryType, IEliasID baseId)
		{
            var strId = IdToString(baseId);
            TableObject.Update();
            var entries = GetEntries(entryType);
            if( entries == null )
                return null;
			foreach(var e in EnumerateSerializedArray(entries))
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
            TableObject.Update();
            var entries = GetEntries(entryType);
            if( entries == null )
                return null;
			foreach(var e in EnumerateSerializedArray(entries))
            {
                if( e.entryType == entryType
                    && e.baseId == strBaseId
                    && EliasID.FromString(e.entryId).Equals(entryId))   // todo: do this in a better way
                    return e;                                           // str fmt id may be shorter 
            }
            return null;
		}

		public string ExportToJson()
		{
			if(_table == null) {
                Debug.LogError("table ref was null");
                return null;
            }
            return JsonUtility.ToJson(_table);
		}
	}
}