using System;
using UnityEngine;

namespace EliasSoftware.Elias4
{
    using EliasSoftware.Elias4.Common;

    /// <summary>
    /// All Elias parameter value objects implement this interface.
    /// </summary>
    public interface IEliasValueBuilder {
        /// <summary>
        /// returns the value wrapped in an EliasValue instance.
        /// </summary>
        EliasValue GetEliasValue();
    }

    /// <summary>
    /// This is a serializable struct representing an integer parameter value.
    /// Use this as a public (or [SerializeField]) in your class to enable the value to be set from the Unity Editor.
    /// </summary>
    [Serializable]
    public struct EliasIntValue :
        IEliasValueBuilder
    {
#pragma warning disable 0649
        [SerializeField]
        private int value;
#pragma warning restore 0649
        /// <summary>
        /// Wraps the value in an EliasValue instance.
        /// </summary>
        public EliasValue GetEliasValue()
            => EliasValue.CreateInteger(value);
    }

    /// <summary>
    /// This is a serializable struct representing a double precision floating point parameter value.
    /// Use this as a public (or [SerializeField]) in your class to enable the value to be set from the Unity Editor.
    /// </summary>
    [Serializable]
    public struct EliasDoubleValue :
        IEliasValueBuilder
    {
#pragma warning disable 0649
        [SerializeField]
        private double value;
#pragma warning restore 0649
        /// <summary>
        /// Wraps the value in an EliasValue instance.
        /// </summary>
        public EliasValue GetEliasValue()
            => EliasValue.CreateDouble(value);
    }

    /// <summary>
    /// This is a serializable struct representing a boolean parameter value.
    /// Use this as a public (or [SerializeField]) in your class to enable the value to be set from the Unity Editor.
    /// </summary>
    [Serializable]
    public struct EliasBoolValue :
        IEliasValueBuilder
    {
#pragma warning disable 0649
        [SerializeField]
        private bool value;
#pragma warning restore 0649

        /// <summary>
        /// Wraps the value in an EliasValue instance.
        /// </summary>
        public EliasValue GetEliasValue()
            => EliasValue.CreateBool(value);
    }

    /// <summary>
    /// This is a serializable struct representing an Elias-Enum parameter value.
    /// Use this as a public (or [SerializeField]) in your class to enable the value to be set from the Unity Editor.
    /// </summary>
    [Serializable]
    public struct EliasEnumValue :
        IEliasValueBuilder
    {
#pragma warning disable 0649
        [SerializeField]
        private EliasEnumValueID value;
#pragma warning restore 0649
        /// <summary>
        /// Wraps the value in an EliasValue instance.
        /// </summary>
        public EliasValue GetEliasValue()
            => EliasValue.CreateEnum(value.ValueID);
    }
}
