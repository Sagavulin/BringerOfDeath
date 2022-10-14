using UnityEngine;
using System.Runtime.InteropServices;

namespace EliasSoftware.Elias4.Common
{
	using System;
	using Native;

    /// <summary>
    /// Elias parameter value types. <br>
    /// ParamType.Empty means "no type".
    /// </summary>
    public enum ParamType {
        Empty,
        Pulse,
        Int,
        Double,
        Bool,
        EnumValue
    }

    /// <summary>
    /// EliasIDs are objects that can provide a byte array representation of a UUID. <br>
    /// Typically used for assets of different kinds.
    /// </summary>
    public interface IEliasID
    {
        public elias_uuid_t UUID { get; }
        public bool IsValid { get; }
    }

    /// <summary>
    /// EliasInstaceIDs are objects that can provide an (engine internal) model instance id.
    /// </summary>
    public interface IEliasInstanceID :
        IEquatable<IEliasInstanceID>
    {
        public elias_model_instance_id_t ModelInstanceID { get; }
        public bool IsValid { get; }
    }

    /// <summary>
    /// Represents the ID of an Impulse Response asset. 
    /// </summary>
    public interface IIRID : IEliasID {}

    /// <summary>
    /// Represents the ID of a Patch asset. 
    /// </summary>
    public interface IPatchID : IEliasID {}

    /// <summary>
    /// Represents the ID of a Patch parameter. 
    /// </summary>
    public interface IParameterID : IEliasID {}

    /// <summary>
    /// Represents the ID of an Enum (Elias-Enum) asset. 
    /// </summary>
    public interface IEnumID : IEliasID {}

    /// <summary>
    /// Represents the ID of an Enum value (Elias-Enum value). 
    /// </summary>
    public interface IEnumValueID : IEliasID {}


    /// <summary>
    /// Currently the only implementation for all IEliasID derived interfaces.
    /// </summary>
    [Serializable]
    public struct EliasID :
	    IIRID,
        IPatchID,
        IParameterID,
        IEnumID,
        IEnumValueID,
        IEquatable<EliasID>
    {
        [SerializeField]
        private elias_uuid_t nativeId;

        /// <summary>
        /// The native UUID representation.
        /// </summary>
        public elias_uuid_t UUID => nativeId;

        /// <summary>
        /// Creates an EliasID instance from a string.
        /// </summary>
        public static EliasID FromString(string id)
            => new EliasID(elias_uuid_t.FromString(id));

        public EliasID(elias_uuid_t id) {
            nativeId = id;
        }

        public EliasID(byte[] id) {
            nativeId = new elias_uuid_t(id);
        }

        public override int GetHashCode()
            => nativeId.GetHashCode();

        public bool Equals(EliasID other)
		    => nativeId.Equals(other.nativeId);

        public override bool Equals(object other) {
            if(other is EliasID)
                return Equals((EliasID)other);
            return false;
        }
        public bool IsValid => nativeId.Equals(elias_uuid_t.AllZero) == false;
        public static EliasID Invalid => new EliasID(elias_uuid_t.AllZero);

        public override string ToString()
        {
            return nativeId.ToString();
        }

#if UNITY_EDITOR
        public static bool Serialize(EliasID eliasId, UnityEditor.SerializedProperty parent)
        {
            var dataField = parent.FindPropertyRelative(nameof(EliasID.nativeId));
            UnityEngine.Debug.Assert(dataField != null);
            if(dataField == null)
                return false;
            var eliasIdStruct = eliasId;
            return eliasIdStruct.nativeId.Serialize(dataField);
        }
        public static EliasID FromProperty(UnityEditor.SerializedProperty parent)
        {
            var dataField = parent.FindPropertyRelative(nameof(EliasID.nativeId));
            return new EliasID(elias_uuid_t.FromProperty(dataField));
        }
#endif
    }

    /// <summary>
    /// The ID of an Elias-Session.
    /// </summary>
    public struct SessionID
    {
        private uint id;
        public uint ID => id;
        public SessionID(uint id) {
            this.id = id;
        }
        public bool IsValid => id != 0;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct RuntimeHandle {
        public elias_handle_t Handle { get; private set; }
        public RuntimeHandle(elias_handle_t hndl) {
            Handle = hndl;
        }
        public RuntimeHandle(IntPtr hndl) {
            Handle = new elias_handle_t() {
                intPtr = hndl
            };
        }
        public bool IsValid {
            get => Handle.intPtr != IntPtr.Zero;
        }
        public static RuntimeHandle InvalidHandle => new RuntimeHandle() {
            Handle = new elias_handle_t() {
                intPtr = IntPtr.Zero
            }
        };
    }

    /// <summary>
    /// The ID of a Sound Instance.
    /// </summary>
    public struct InstanceID :
        IEliasInstanceID
    {
        private elias_model_instance_id_t instanceId;
        public elias_model_instance_id_t ModelInstanceID => instanceId;
        public InstanceID(elias_model_instance_id_t id) {
            instanceId = id;
        }
        public override int GetHashCode()
            => instanceId.GetHashCode();

		public bool Equals(IEliasInstanceID other)
		    => instanceId.Equals(other.ModelInstanceID);

        public override bool Equals(object other) {
            if(other is IEliasInstanceID)
                return Equals((IEliasInstanceID)other);
            return false;
        }
        public static InstanceID Zero
            => new InstanceID(elias_model_instance_id_t.Zero);
		public override string ToString()
            => "InstanceID: " + instanceId.ToString();
        public bool IsValid => instanceId.IsZero == false;
    }

    /// <summary>
    /// The ID of an Elias-Listener.
    /// </summary>
    public struct ListenerID :
        IEliasInstanceID
    {
        private elias_model_instance_id_t instanceId;
        public elias_model_instance_id_t ModelInstanceID => instanceId;
        public ListenerID(elias_model_instance_id_t id) {
            instanceId = id;
        }
        public override int GetHashCode()
            => instanceId.GetHashCode();
		public bool Equals(IEliasInstanceID other)
		    => instanceId.Equals(other.ModelInstanceID);
        public override bool Equals(object other) {
            if(other is IEliasInstanceID)
                return Equals((IEliasInstanceID)other);
            return false;
        }
        public static ListenerID Zero
            => new ListenerID(elias_model_instance_id_t.Zero);
		public override string ToString()
            => "ListenerID: " + instanceId.ToString();
        public bool IsValid => instanceId.IsZero == false;
    }

    /// <summary>
    /// The ID of an Elias-Zone.
    /// </summary>
    public struct ZoneID :
        IEliasInstanceID
    {
        private elias_model_instance_id_t instanceId;
        public elias_model_instance_id_t ModelInstanceID => instanceId;
        public ZoneID(elias_model_instance_id_t id) {
            instanceId = id;
        }
        public override int GetHashCode()
            => instanceId.GetHashCode();

		public bool Equals(IEliasInstanceID other)
		    => instanceId.Equals(other.ModelInstanceID);

        public override bool Equals(object other) {
            if(other is IEliasInstanceID)
                return Equals((IEliasInstanceID)other);
            return false;
        }

        public static ZoneID Zero
            => new ZoneID(elias_model_instance_id_t.Zero);

		public override string ToString()
            => "ZoneID: " + instanceId.ToString();
        public bool IsValid => instanceId.IsZero == false;
    }

    /// <summary>
    /// Options for EliasZone sound bleed behaviour.
    /// </summary>
    public enum ZoneSoundBleedBehaviour : byte {
        // Sound attenuates at zone boundary as defined by zone settings.
		Default = elias_zone_bleed_behaviour_t.ELIAS_ZONE_BLEED_DEFAULT,
		// Sounds unaffected by zone boundary.
		IgnoreZones = elias_zone_bleed_behaviour_t.ELIAS_ZONE_BLEED_IGNORE,
		// Sounds cannot pass zone boundary.
		NoBleed = elias_zone_bleed_behaviour_t.ELIAS_ZONE_BLEED_NONE,
    }

    /// <summary>
    /// A wrapper struct for Elias-Patch parameter values.
    /// </summary>
    public struct EliasValue
    {
        /// <summary>
        /// The value type contained within the EliasValue. 
        /// </summary>
        public ParamType Type { get; private set; }

        /// <summary>
        /// The actual value. 
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        /// Converts the contained value to its native representation.
        /// </summary>
        public elias_typed_value_t AsNative() {
            switch(Type) {
                case ParamType.Pulse:
                    return new elias_typed_value_t() {
                        type = elias_fundamental_types_t.ELIAS_FUNDAMENTAL_TYPE_INT,
                        value = new elias_generic_value_t() {
                            int_value = (int)Data
                        }
                    };
                case ParamType.Int:
                    return new elias_typed_value_t() {
                        type = elias_fundamental_types_t.ELIAS_FUNDAMENTAL_TYPE_INT,
                        value = new elias_generic_value_t() {
                            int_value = (int)Data
                        }
                    };
                case ParamType.Double:
                    return new elias_typed_value_t() {
                        type = elias_fundamental_types_t.ELIAS_FUNDAMENTAL_TYPE_DOUBLE,
                        value = new elias_generic_value_t() {
                            double_value = (double)Data
                        }
                    };
                case ParamType.Bool:
                    return new elias_typed_value_t() {
                        type = elias_fundamental_types_t.ELIAS_FUNDAMENTAL_TYPE_BOOL,
                        value = new elias_generic_value_t() {
                            bool_value = (bool)Data
                        }
                    };
                case ParamType.EnumValue:
                    return new elias_typed_value_t() {
                        type = elias_fundamental_types_t.ELIAS_FUNDAMENTAL_TYPE_ID,
                        value = new elias_generic_value_t() {
                            id_value = (Data as IEliasID).UUID
                        }
                    };
                default: {
                    Debug.LogErrorFormat("No conversion from param type {0} exists",
                        Type.ToString());
                } break;
            }

            return new elias_typed_value_t() {
                type = elias_fundamental_types_t.ELIAS_FUNDAMENTAL_TYPE_UNSPECIFIED,
                value = new elias_generic_value_t() { bool_value = false }
            };
        }

        /// <summary>
        /// Creates an EliasValue from a value of type integer.
        /// </summary>
        public static EliasValue CreateInteger(int value)
            => new EliasValue() {
                Data = value,
                Type = ParamType.Int
            };

        /// <summary>
        /// Creates an EliasValue representing a Pulse.
        /// </summary>
        public static EliasValue CreatePulse()
            => new EliasValue() {
                Data = 1,
                Type = ParamType.Pulse
            };

        /// <summary>
        /// Creates an EliasValue from a value of type double.
        /// </summary>
        public static EliasValue CreateDouble(double value)
            => new EliasValue() {
                Data = value,
                Type = ParamType.Double
            };

        /// <summary>
        /// Creates an EliasValue from a value of type bool.
        /// </summary>
        public static EliasValue CreateBool(bool value)
            => new EliasValue() {
                Data = value,
                Type = ParamType.Bool
            };

        /// <summary>
        /// Creates an EliasValue from the provided Elias-Enum value ID.
        /// </summary>
        public static EliasValue CreateEnum(IEnumValueID value)
            => new EliasValue() {
                Data = value,
                Type = ParamType.EnumValue
            };

        /// <summary>
        /// Attempts to create an EliasValue from a string.
        /// </summary>
        /// <param name="type">The resulting value type.</param>
        /// <param name="value">The value as a string.</param>
        /// <param name="result">A reference to where to put the result.</param>
        /// <returns>true on successful parsing, false otherwise.</returns>
        public static bool TryCreateFromString(ParamType type,
            string value,
            out EliasValue result)
        {
            switch(type) {
                case ParamType.Int:
                {
                    int val;
                    if(int.TryParse(value, out val)) {
                        result = EliasValue.CreateInteger(val);
                        return true;
                    }
                } break;
                case ParamType.Pulse:
                {
                    result = EliasValue.CreatePulse();
                    return true;
                };
                case ParamType.Double:
                {
                    double val;
                    if(double.TryParse(value, out val)) {
                        result = EliasValue.CreateDouble(val);
                        return true;
                    }
                } break;
                case ParamType.Bool:
                {
                    bool val;
                    if(bool.TryParse(value, out val)) {
                        result = EliasValue.CreateBool(val);
                        return true;
                    }
                } break;
                case ParamType.EnumValue:
                {
                    IEnumValueID id;
                    try {
                        id = EliasID.FromString(value);
                    } catch {
                        id = EliasID.Invalid;
                    }
                    if(id.IsValid) {
                        result = EliasValue.CreateEnum(id);
                        return true;
                    }
                } break;
                default: {
                    Debug.LogErrorFormat("Unhandled parameter type {0}",
                        type.ToString());
                } break;
            }

            result = new EliasValue() {
                Type = ParamType.Empty,
                Data = null
            };
            return false;
        }

        /// <summary>
        /// Overrides ToString to return a string with both the type and the value.
        /// </summary>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(Type).Append(" : ").Append(Data.ToString());
            return sb.ToString();
        }
    }
}
