namespace EliasSoftware.Elias4.Common
{
	using System;
    using System.Collections.Generic;
	using UnityEngine;

#if UNITY_EDITOR
    using System.Linq;
#endif

    /// <summary>
    /// A type descriptor for serializable ID object. 
    /// </summary>
    public enum ObjectType {
        None,
        Patch,
        PatchParam,
        GlobalParam,
        Enum,
        EnumValue,
        IR
    }

    [Serializable]
    /// <summary>
    /// The base class of all serializable ID objects.<br><br>
    /// Serializable IDs are used in the following way (by the Editor scripts):<br>
    /// IDs are selected by the user via the Unity Editor in a value-by-name fashion.<br>
    /// Once the user has selected a name, a lookup is performed to get the corresponding ID
    /// then both the name and the ID is serialized in order to store it in Unitys meta data.<br>
    /// The basic structure is that the Serializable ID object represents a single pair (ID, Name).<br>
    /// Some specific IDs (EliasPatch for example) also encodes / decodes additional meta data.<br>
    /// </summary>
	public class EliasSerializableID
    {
#pragma warning disable 0649

        [HideInInspector, SerializeField]
        private ObjectType m_objectType;

        [HideInInspector, SerializeField]
        private EliasID m_id_fst = EliasID.Invalid;

        [HideInInspector, SerializeField]
        private string m_name_fst;

#pragma warning restore 0649
        /// <summary>
        /// A type label for this Serializable ID describing the type of the asset it represents.
        /// </summary>
        public ObjectType ObjectType { get => m_objectType; }

        /// <summary>
        /// The EliasSerializableID essentially corresponds to a pair: (ID, Name).<br>
        /// In this class the ID component is called 'FirstID' and the Name component 'FirstName'.<br>
        /// The reason for the First* prefix is that some ID objects (EnumValueID for example) are of the form ((ID,Name), (ID,Name)).<br>
        /// The prefix exists to distinguish between the first and the second id-name pair.
        /// </summary>
        /// <value>An EliasID value</value>
        public EliasID FirstID { get => m_id_fst; }

        /// <summary>
        /// The EliasSerializableID essentially corresponds to a pair: (ID, Name).<br>
        /// In this class the ID component is called 'FirstID' and the Name component 'FirstName'.<br>
        /// The reason for the First* prefix is that some ID objects (EnumValueID for example) are of the form ((ID,Name), (ID,Name)).<br>
        /// The prefix exists to distinguish between the first and the second id-name pair.
        /// </summary>
        /// <value>A human readable name tag</value>
        public string FirstName { get => m_name_fst; }

#if UNITY_EDITOR
        public static ObjectType ObjectTypeFromProperty(UnityEditor.SerializedProperty property) {
            var field = property.FindPropertyRelative(nameof(EliasSerializableID.m_objectType));
			var index = field.enumValueIndex;
			if(Enum.IsDefined(typeof(ObjectType), index) == false)
				return ObjectType.None;
			return (ObjectType)index;
        }
        public static EliasID FirstIDFromProperty(UnityEditor.SerializedProperty property) {
            var field = property.FindPropertyRelative(nameof(EliasSerializableID.m_id_fst));
            return EliasID.FromProperty(field);
        }

        public static UnityEditor.SerializedProperty FirstIDProperty(UnityEditor.SerializedProperty property) {
            return property.FindPropertyRelative(nameof(EliasSerializableID.m_id_fst));
        }

        public static string FirstNameFromProperty(UnityEditor.SerializedProperty property) {
            var field = property.FindPropertyRelative(nameof(EliasSerializableID.m_name_fst));
            return field.stringValue;
        }
        public static UnityEditor.SerializedProperty FirstNameProperty(UnityEditor.SerializedProperty property) {
            return property.FindPropertyRelative(nameof(EliasSerializableID.m_name_fst));
        }


        public static bool Serialize(ObjectType type, EliasID id, string name, UnityEditor.SerializedProperty property)
        {
            var typeField = property.FindPropertyRelative(nameof(EliasSerializableID.m_objectType));
            var idField = property.FindPropertyRelative(nameof(EliasSerializableID.m_id_fst));
            var nameField = property.FindPropertyRelative(nameof(EliasSerializableID.m_name_fst));

            Debug.Assert(nameField != null);
            Debug.Assert(idField != null);
            Debug.Assert(typeField != null);

            if(nameField == null || idField == null || typeField == null)
                return false;

            typeField.enumValueIndex = (int)type;
            nameField.stringValue = name;
            return EliasID.Serialize(id, idField);
        }
#endif

    }

    [Serializable]
    /// <summary>
    /// Serializable ID class that makes it possible to specify a Patch ID in the Unity Editor
    /// by selecting the name of the patch (instead of the decoded UUID data).
    /// </summary>
	public class EliasPatch : EliasSerializableID
    {
#pragma warning disable 0649
        [HideInInspector, SerializeField]
        private EliasPatchParameter[] m_parameters = new EliasPatchParameter[] { };
#pragma warning restore 0649

        /// <summary>
        /// The stored data in this object as a patch ID.
        /// </summary>
        /// <value>An Elias Patch ID</value>
        public IPatchID PatchID { get => FirstID; }

        /// <summary>
        /// An array of all the parameters of this Patch.
        /// </summary>
        public EliasPatchParameter[] Parameters { get => m_parameters; }

#if UNITY_EDITOR
        public static bool Serialize(EliasID id,
            string name,
            IEnumerable<(string, IParameterID, ParamType, string)> parameters,
            UnityEditor.SerializedProperty property)
        {
            if(EliasSerializableID.Serialize(ObjectType.Patch, id, name, property) == false)
                return false;

            var paramsField = property.FindPropertyRelative(nameof(EliasPatch.m_parameters));
            Debug.Assert(paramsField != null);
            if(paramsField == null)
                return false;

            Debug.Assert(paramsField.isArray);

            if(parameters == null)
                return true;

            var paramList = parameters.ToList();

            int len = paramList.Count();
            paramsField.ClearArray();
            paramsField.arraySize = len;
            for(int i = 0; i < len; i++) {
                paramsField.InsertArrayElementAtIndex(0);
            }

            paramsField.serializedObject.ApplyModifiedProperties();
            paramsField.serializedObject.Update();

            int cntr = 0;
            foreach(var param in paramList)
            {
                var pField = paramsField.GetArrayElementAtIndex(cntr);
                var res = EliasPatchParameter.Serialize(id,
                    (EliasID)param.Item2,
                    name,
                    param.Item1,
                    param.Item3,
                    param.Item4,
                    pField);
                Debug.Assert(res);
                cntr++;
            }

            paramsField.serializedObject.ApplyModifiedProperties();
            paramsField.serializedObject.Update();

            return true;
        }
#endif
    }

    [Serializable]
    /// <summary>
    /// The base class for all ID objects that has both a First and a Second component.<br>
    /// Eg. ID objects that are of the form ((ID,Name), (ID,Name)).
    /// </summary>
	public class EliasSerializableIDPair :
        EliasSerializableID
    {
#pragma warning disable 0649
        [SerializeField]
        private EliasID m_id_snd = EliasID.Invalid;

        [SerializeField]
        private string m_name_snd;
#pragma warning restore 0649

        /// <summary>
        /// The ID part of the second (ID, Name) pair.
        /// See FirstID for a more in-depth explanation.
        /// </summary>
        /// <value>An EliasID value</value>
        public EliasID SecondID { get => m_id_snd; }

        /// <summary>
        /// The Name part of the second (ID, Name) pair.
        /// See FirstName for a more in-depth explanation.
        /// </summary>
        /// <value>An EliasID value</value>
        public string SecondName { get => m_name_snd; }

#if UNITY_EDITOR
        public static EliasID SecondIDFromProperty(UnityEditor.SerializedProperty property) {
            var field = property.FindPropertyRelative(nameof(EliasSerializableIDPair.m_id_snd));
            return EliasID.FromProperty(field);
        }

        public static UnityEditor.SerializedProperty SecondIDProperty(UnityEditor.SerializedProperty property) {
            return property.FindPropertyRelative(nameof(EliasSerializableIDPair.m_id_snd));
        }

        public static string SecondNameFromProperty(UnityEditor.SerializedProperty property) {
            var field = property.FindPropertyRelative(nameof(EliasSerializableIDPair.m_name_snd));
            return field.stringValue;
        }

        public static bool Serialize(ObjectType type,
            EliasID id1,
            EliasID id2,
            string name1,
            string name2,
            UnityEditor.SerializedProperty property)
        {
            var idField = property.FindPropertyRelative(nameof(EliasSerializableIDPair.m_id_snd));
            var nameField = property.FindPropertyRelative(nameof(EliasSerializableIDPair.m_name_snd));
            if(EliasSerializableID.Serialize(type, id1, name1, property) == false)
                return false;

            Debug.Assert(idField != null);
            Debug.Assert(nameField != null);

            if( idField == null || nameField == null )
                return false;

            nameField.stringValue = name2;
            return EliasID.Serialize(id2, idField);
        }
#endif

    }


    [Serializable]
    /// <summary>
    /// Serializable ID class that makes it possible to specify an Impulse Response ID in the Unity Editor
    /// by selecting the name of the IR (instead of the decoded UUID data).
    /// </summary>
	public class EliasIRID :
        EliasSerializableID
    {
        /// <summary>
        /// The Impulse Response ID stored in this object.
        /// </summary>
        public IIRID IRID { get => FirstID; }
    }

    [Serializable]
    /// <summary>
    /// Serializable ID class that makes it possible to specify an Elias Enum ID in the Unity Editor
    /// by selecting the name of the Enum (instead of the decoded UUID data).
    /// </summary>
	public class EliasEnumID :
        EliasSerializableID
    {
#pragma warning disable 0649
        [HideInInspector, SerializeField]
        private EliasEnumValueID[] m_domain  = new EliasEnumValueID[] { };
#pragma warning restore 0649

        /// <summary>
        /// The Enum ID stored in this object.
        /// </summary>
        public IEnumID EnumID { get => FirstID; }

#if UNITY_EDITOR
        public static bool Serialize(EliasID id,
            string name,
            IEnumerable<(string, IEnumValueID)> domain,
            UnityEditor.SerializedProperty property)
        {
            if(EliasSerializableID.Serialize(ObjectType.Enum, id, name, property) == false)
                return false;

            var domField = property.FindPropertyRelative(nameof(EliasEnumID.m_domain));
            Debug.Assert(domField != null);
            if(domField == null)
                return false;

            Debug.Assert(domField.isArray);

            if(domain == null)
                return true;

            var paramList = domain.ToList();

            int len = paramList.Count();
            domField.arraySize = len;
            domField.serializedObject.ApplyModifiedProperties();

            int cntr = 0;
            foreach(var val in domain)
            {
                var pField = domField.GetArrayElementAtIndex(cntr++);
                EliasEnumValueID.Serialize(id,
                    (EliasID)val.Item2,
                    name,
                    val.Item1,
                    pField);
            }

            domField.serializedObject.ApplyModifiedProperties();

            return true;
        }
#endif
    }

    [Serializable]
    /// <summary>
    /// Serializable ID class that makes it possible to specify an Elias Enum Value in the Unity Editor
    /// by selecting the name of the Enum and the Enum Value (instead of their decoded UUID data).
    /// </summary>
	public class EliasEnumValueID :
        EliasSerializableIDPair
    {
        /// <summary>
        /// The Enum ID stored in this object, this is the Enum to which the Enum Value belongs.
        /// </summary>
        public IEnumID EnumID { get => FirstID; }

        /// <summary>
        /// The Enum Value ID stored in this object.
        /// </summary>
        public IEnumValueID ValueID { get => SecondID; }

#if UNITY_EDITOR
        public static bool Serialize(EliasID enumID,
            EliasID valueID,
            string enumName,
            string enumValueName,
            UnityEditor.SerializedProperty property)
        {
            if(EliasPatchParameter.Serialize(ObjectType.EnumValue, enumID, valueID, enumName, enumValueName, property) == false)
                return false;
            return true;
        }
#endif

    }

    [Serializable]
    /// <summary>
    /// Serializable ID class that makes it possible to specify an Elias Patch Parameter in the Unity Editor
    /// by selecting the name of the Patch and the Parameter Name (instead of their decoded UUID data).
    /// </summary>
	public class EliasPatchParameter :
        EliasSerializableIDPair
    {
#pragma warning disable 0649
        [SerializeField]
        private ParamType m_paramType;

        [SerializeField]
        private string m_data;

#pragma warning restore 0649
        /// <summary>
        /// A string data buffer.
        /// </summary>
        public string Data { get => m_data; }

        /// <summary>
        /// The the argument value type for this parameter.
        /// </summary>
        public ParamType ParamType { get => m_paramType; }

        /// <summary>
        /// The Elias Parameter ID.
        /// </summary>
        public IParameterID ParameterID { get => SecondID; }

        /// <summary>
        /// The ID of the Patch this parameter belongs to.
        /// </summary>
        public IPatchID PatchID { get => FirstID; }

#if UNITY_EDITOR
        public static ParamType ParamTypeFromProperty(UnityEditor.SerializedProperty property) {
            var field = property.FindPropertyRelative(nameof(EliasPatchParameter.m_paramType));
			var index = field.enumValueIndex;
			if(Enum.IsDefined(typeof(ObjectType), index) == false)
				return ParamType.Empty;
			return (ParamType)index;
        }

        public static string ParamValueFromProperty(UnityEditor.SerializedProperty property) {
            var field = property.FindPropertyRelative(nameof(EliasPatchParameter.m_data));
			return field.stringValue;
        }

        public static bool Serialize(EliasID patchID,
            EliasID paramID,
            string patchName,
            string paramName,
            ParamType argType,
            string value,
            UnityEditor.SerializedProperty property)
        {
            if(EliasSerializableIDPair.Serialize(ObjectType.PatchParam, patchID, paramID, patchName, paramName, property) == false)
                return false;
            var paramTypeField = property.FindPropertyRelative(nameof(EliasPatchParameter.m_paramType));
            var paramDef = property.FindPropertyRelative(nameof(EliasPatchParameter.m_data));
            Debug.Assert(paramTypeField != null);
            if(paramTypeField == null || paramDef == null)
                return false;
            paramTypeField.enumValueIndex = (int)argType;
            paramDef.stringValue = value;
            return true;
        }
#endif
    }
}
