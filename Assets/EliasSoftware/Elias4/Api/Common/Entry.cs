using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Common
{
    public enum EntryType {
        Invalid,
        Patch,
        PatchParam,
        Enum,
        EnumValue,
        IR,
        GlobalParam
    }
    
    public interface IEntry {
        public EntryType Type { get; }
        public string Name { get; }
        public EliasID BaseID { get; }
        public EliasID EntryID { get; }
        public ParamType ValueType { get; }
        public string Data { get; }
    }

    [Serializable]
    public class Entry :
        IEntry
    {
        public EntryType entryType;
        public string entryName;
        public string baseId;
        public string entryId;
        public ParamType valueType;
        public string data;

		public EntryType Type => entryType;
		public string Name => entryName;
		public EliasID BaseID => EliasID.FromString(baseId);
        public ParamType ValueType => valueType;
        public string Data => data;
		public EliasID EntryID => entryId != null
            ? EliasID.FromString(entryId)
            : EliasID.Invalid;

#if UNITY_EDITOR

        public static void Serialize(EntryType entryType,
            string entryName,
            string baseId,
            string entryId,
            ParamType valueType,
            string data,
            UnityEditor.SerializedProperty property)
        {
            property.FindPropertyRelative(nameof(Entry.baseId)).stringValue = baseId;
            property.FindPropertyRelative(nameof(Entry.entryName)).stringValue = entryName;
            property.FindPropertyRelative(nameof(Entry.entryType)).enumValueIndex = (int)entryType;
            property.FindPropertyRelative(nameof(Entry.entryId)).stringValue = entryId;
            property.FindPropertyRelative(nameof(Entry.valueType)).enumValueIndex = (int)valueType;
            property.FindPropertyRelative(nameof(Entry.data)).stringValue = data;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public static Entry FromProperty(UnityEditor.SerializedProperty property)
        {
            property.serializedObject.Update();
            
            var baseId = property.FindPropertyRelative(nameof(Entry.baseId));
            var name = property.FindPropertyRelative(nameof(Entry.entryName));
            var entryType = property.FindPropertyRelative(nameof(Entry.entryType));
            var entryId = property.FindPropertyRelative(nameof(Entry.entryId));
            var valueType = property.FindPropertyRelative(nameof(Entry.valueType));
            var data = property.FindPropertyRelative(nameof(Entry.data));

            return new Entry() {
                baseId = baseId != null ? baseId.stringValue : string.Empty,
                entryName = name != null ? name.stringValue : string.Empty,
                entryType = entryType != null ? (EntryType)entryType.enumValueIndex : EntryType.Invalid,
                valueType = valueType != null ? (ParamType)valueType.enumValueIndex : ParamType.Empty,
                entryId = entryId != null ? entryId.stringValue : string.Empty,
                data = data != null ? data.stringValue : string.Empty,
            };
        }

#endif
	}
}