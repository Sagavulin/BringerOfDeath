using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Common
{

    public interface IAssetTableReader
    {
        void Init();
        IEnumerable<IEntry> GetAll();
        IEnumerable<IEntry> GetAll(IEliasID baseId);
        IEnumerable<IEntry> GetAll(EntryType entryType);
        IEntry Get(EntryType entryType, string entryName);
        IEntry Get(EntryType entryType, IEliasID baseId, string entryName);
        IEntry Get(EntryType entryType, IEliasID baseId);
        IEntry Get(EntryType entryType, IEliasID baseId, IEliasID entryId);
    }

    public interface IAssetTableWriter : 
        IAssetTableReader
    {
        bool Add(EntryType entryType, string name, string baseId, string entryId, ParamType valueType = ParamType.Empty, string data = null);
        int RemoveAll(IEliasID baseId);
        void RemoveAll(IEliasID baseId, HashSet<string> removedNames);
        void RemoveAllExcept(HashSet<(string, string)> entryIds);
        void Clear();
        string ExportToJson();
    }

    [Serializable]
    public class EliasAssetTable :
        ScriptableObject
    {
        [SerializeField]
        public Entry[] patchEntries = new Entry[]{};
        [SerializeField]
        public Entry[] enumEntries = new Entry[]{};
        [SerializeField]
        public Entry[] irEntries = new Entry[]{};
        [SerializeField]
        public Entry[] globalParamEntries = new Entry[]{};
	}
}