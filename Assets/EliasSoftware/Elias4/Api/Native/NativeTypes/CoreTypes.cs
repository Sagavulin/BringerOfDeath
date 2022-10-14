using System;
using System.Runtime.InteropServices;

namespace EliasSoftware.Elias4.Native {

    using elias_base_id_t = UInt32;
    using int32_t = Int32;
    using void_ptr = IntPtr;
    using elias_export_context_id_t = char_ptr;

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_handle_t :
        IEquatable<elias_handle_t>
    {
        public IntPtr intPtr;

        public override int GetHashCode() => intPtr.GetHashCode();

        public override bool Equals(object other) {
            if(ReferenceEquals(null, other))
                return false;
            if(other is elias_handle_t)
                return Equals((elias_handle_t)other);
            return false;
        }

        public bool Equals(elias_handle_t other) {
            return intPtr.Equals(other.intPtr);
        }

        public override string ToString() {
            return string.Format("0x{0:X}", intPtr.ToInt64());
        }

        public static elias_handle_t NullHandle
            => new elias_handle_t() { intPtr = IntPtr.Zero };
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct char_ptr {

        [MarshalAs(UnmanagedType.LPStr)]
        public String data;
        public static implicit operator char_ptr(string val)
            => new char_ptr() { data = val };
        public static implicit operator string(char_ptr val)
            => val.data;

        public override string ToString() => data;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public unsafe struct char_buf {
        public const int BUF_SIZE = 56;
        public fixed char data[BUF_SIZE];
        public static implicit operator char_buf(string val) {
            var buf = new char_buf();
            UnityEngine.Debug.AssertFormat(val.Length <= BUF_SIZE,
                "The provided string was longer than BUF_SIZE");
            int len = UnityEngine.Mathf.Min(BUF_SIZE, val.Length);
            for(int i = 0; i < BUF_SIZE; i++) {
                if(i < len)
                    buf.data[i] = val[i];
                else
                    buf.data[i] = '\0';
            }
            return buf;
        }

        public static implicit operator string(char_buf val) {
            var sb = new System.Text.StringBuilder();
            for(int i = 0; i < BUF_SIZE; i++) {
                if(val.data[i] == '\0')
                    return sb.ToString();
                sb.Append(val.data[i]);
            }
            return sb.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct uint64_t {
        public UInt64 value;
        public static implicit operator uint64_t(UInt64 val)
            => new uint64_t() { value = val };
        public static implicit operator UInt64(uint64_t val)
            => val.value;

        public override string ToString() => value.ToString();
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_size_t {
        public int value;
        public static implicit operator elias_size_t(int val)
            => new elias_size_t() { value = val };
        public static implicit operator int(elias_size_t val)
            => val.value;
        public override string ToString() => value.ToString();
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_buffer_length_t {
        public int value;
        public static implicit operator elias_buffer_length_t(int val)
            => new elias_buffer_length_t() { value = val };
        public static implicit operator int(elias_buffer_length_t val)
            => val.value;
        public override string ToString() => value.ToString();
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_sample_rate_t {
        public int value;
        public static implicit operator elias_sample_rate_t(int val)
            => new elias_sample_rate_t() { value = val };
        public static implicit operator int(elias_sample_rate_t val)
            => val.value;
        public override string ToString() => value.ToString();
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_channel_count_t {
        public int value;
        public static implicit operator elias_channel_count_t(int val)
            => new elias_channel_count_t() { value = val };
        public static implicit operator int(elias_channel_count_t val)
            => val.value;
        public override string ToString() => value.ToString();
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_session_id_t {
        public uint value;
        public static implicit operator elias_session_id_t(uint val)
            => new elias_session_id_t() { value = val };
        public static implicit operator uint(elias_session_id_t val)
            => val.value;
        public override string ToString() => value.ToString();
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public unsafe struct elias_model_instance_id_t :
        IEquatable<elias_model_instance_id_t>
    {
        public fixed elias_base_id_t subcomponents[ 3 ];
        public elias_size_t length;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is elias_model_instance_id_t
                && Equals((elias_model_instance_id_t)obj);
        }

		public bool Equals(elias_model_instance_id_t other)
		{
			if(length != other.length)
                return false;
            if( subcomponents[0] == other.subcomponents[0]
                && subcomponents[1] == other.subcomponents[1]
                && subcomponents[2] == other.subcomponents[2] )
                return true;
            return false;
		}

		public override int GetHashCode() {
            // fun fact: 104729 is the 10,000th prime number
            int hash = 104729;
            // you guessed it, 541 is the 100th prime number
            hash = hash * 541 ^ subcomponents[0].GetHashCode();
            hash = hash * 541 ^ subcomponents[1].GetHashCode();
            hash = hash * 541 ^ subcomponents[2].GetHashCode();
            hash = hash * 541 ^ length.GetHashCode();
			return hash;
		}

        private elias_model_instance_id_t(
            int size,
            elias_base_id_t comp1,
            elias_base_id_t comp2,
            elias_base_id_t comp3)
        {
            length = size;
            subcomponents[0] = comp1;
            subcomponents[1] = comp2;
            subcomponents[2] = comp3;
        }

        public static elias_model_instance_id_t Zero
            => new elias_model_instance_id_t(0,0,0,0);

        public bool IsZero => Equals(elias_model_instance_id_t.Zero);

		public override string ToString()
		{
			return string.Format("len={0} [{1:X}, {2:X}, {3:X}]",
                length, subcomponents[0], subcomponents[1], subcomponents[2]);
		}
	}


    [StructLayout(LayoutKind.Sequential), Serializable]
    public unsafe struct elias_uuid_t
        : IEquatable<elias_uuid_t>
    {
        [UnityEngine.SerializeField]
        private fixed byte data[16];
        public elias_uuid_t(byte[] id_bytes) {
            var len = id_bytes.Length;
            UnityEngine.Debug.Assert(len == 16);
            for(int i = 0; i < 16; i++)
                data[i] = id_bytes[i];
        }

        private elias_uuid_t(byte init) {
            for(int i = 0; i < 16; i++)
                data[i] = init;
        }

        public static elias_uuid_t FromString(string s) {
            var res = Convert.FromBase64String(s);
            var len = res.Length < 16 ? res.Length : 16;
            var t = new elias_uuid_t();
            for(int i = 0; i < 16; i++) {
                if(i < len)
                    t.data[i] = res[i];
                else
                    t.data[i] = 0;
            }
            return t;
        }

        public static string ToString(elias_uuid_t uuid) {
            int len = 16;
            byte[] d = new byte[len];
            for(int i = 0; i < len; i++) {
                d[i] = uuid.data[i];
            }
            return Convert.ToBase64String(d);
        }

        public override string ToString(){
            return ToString(this);
        }

        public override int GetHashCode() {
            int hash = 104729;
            for(int i = 0; i < 16; i++)
                hash = hash * 541 ^ data[0].GetHashCode();
			return hash;
		}

        public override bool Equals(object other) {
            if (ReferenceEquals(null, other))
                return false;
            return other is elias_uuid_t
                && Equals((elias_uuid_t)other);
        }

		public bool Equals(elias_uuid_t other)
		{
			for(int i = 0; i < 16; i++) {
                if(data[i] != other.data[i])
                    return false;
            }
            return true;
		}

#if UNITY_EDITOR
        public bool Serialize(UnityEditor.SerializedProperty parent)
        {
            var dataField = parent.FindPropertyRelative(nameof(elias_uuid_t.data));
            UnityEngine.Debug.Assert(dataField.isFixedBuffer);
            if(dataField.isFixedBuffer == false)
                return false;
            UnityEngine.Debug.Assert(dataField != null);
            if(dataField == null)
                return false;
            var size = UnityEngine.Mathf.Min(dataField.fixedBufferSize, 16);
            for(int i=0; i < size; i++)
                dataField.GetFixedBufferElementAtIndex(i).intValue = data[i];
            return size > 0;
        }

        public static elias_uuid_t FromProperty(UnityEditor.SerializedProperty parent)
        {
            var newId = elias_uuid_t.AllZero;
            if(parent == null)
                return newId;
            var dataField = parent.FindPropertyRelative(nameof(elias_uuid_t.data));
            for(int i=0; i < 16; i++)
                newId.data[i] = (byte)dataField.GetFixedBufferElementAtIndex(i).intValue;
            return newId;
        }
#endif

		public static elias_uuid_t AllZero => new elias_uuid_t(0);
    }


    /**
    * The different data types supported as signals in the runtime.
    */
    public enum elias_signal_types_t
    {
        ELIAS_SIGNAL_TYPE_BOOL = 0,
        ELIAS_SIGNAL_TYPE_INT = 1,
        ELIAS_SIGNAL_TYPE_DOUBLE = 2,
        ELIAS_SIGNAL_TYPE_ENUM = 3,
        ELIAS_SIGNAL_TYPE_PULSE = 4,
        ELIAS_SIGNAL_TYPE_AUDIO = 5,

        ELIAS_SIGNAL_TYPE_UNSPECIFIED = -1,
    }

    /**
    * Variant type to hold a value of any one of the types supported by
    * Elias's node graph processing system.
    * See elias_fundamental_types_t above.
    */
    [StructLayout(LayoutKind.Explicit), Serializable]
    public struct elias_generic_value_t
    {
        [FieldOffset(0)]
        public bool bool_value;

        [FieldOffset(0)]
        public int32_t int_value;

        [FieldOffset(0)]
        public double double_value;

        [FieldOffset(0)]
        public elias_uuid_t id_value;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_typed_value_t
    {
        public elias_fundamental_types_t type;
        public elias_generic_value_t value;
    }

    /**
    * An enumeration of the fundamental types that are supported by the node graph processing system.
    * These define low level implementation/storage types, and so do not distinguish between, for example,
    * enum and object reference, since both are represented by an id.
    */
    public enum elias_fundamental_types_t
    {
        ELIAS_FUNDAMENTAL_TYPE_BOOL = 0,
        ELIAS_FUNDAMENTAL_TYPE_INT = 1,
        ELIAS_FUNDAMENTAL_TYPE_DOUBLE = 2,
        ELIAS_FUNDAMENTAL_TYPE_ID = 3,
        ELIAS_FUNDAMENTAL_TYPE_AUDIO = 4,

        // experimental - adding non-runtime types
        ELIAS_FUNDAMENTAL_TYPE_STRING = 5,
        //

        ELIAS_FUNDAMENTAL_TYPE_UNSPECIFIED = -1,
    }

    public enum elias_result_t {
        elias_success = 0,

        elias_error_invalid_parameter,
        elias_error_invalid_context,
        elias_error_invalid_id,
        elias_error_buffer_too_small,
        elias_error_memory_functions_not_initialized,
        elias_error_already_initialized,
        elias_error_resolution_failure,
        elias_error_io_failure,
        elias_error_out_of_memory,
        elias_error_not_open,
        elias_error_already_open,
        elias_error_not_supported
    }

    public enum elias_asset_export_mode_t
    {
        ELIAS_ASSET_EXPORT_NONE,
        ELIAS_ASSET_EXPORT_STANDARD,
        ELIAS_ASSET_EXPORT_STREAMED,
    }

    public enum elias_export_result_t
    {
        elias_export_success = 0,
        elias_export_invalid_operation = 1,
        elias_export_io_failure = 2
    }

    /** Opens a new export context with a given id. In practice, this will generally map to creating a new file. */
    public delegate elias_export_result_t open_context( void_ptr user, elias_export_context_id_t context_id );

    /** Closes the currently open context. */
    public delegate elias_export_result_t close_context( void_ptr user );

    /** Writes bytes/characters out to the currently open context. */
    public delegate elias_export_result_t write( void_ptr user, IntPtr data, uint64_t count );

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_export_functions_t
    {
        // @NOTE: Below comments were written by Cameron for the old fusion_export_functions_t.

        // @TODO: probably better to provide a different result enum for user callbacks. maybe a dedicated enum per function is best.

        // @TODO: m0. will likely want a way to specify some additional data about the context being opened, to allow the implementation
        // to optimize/chunk things together, etc. worth adding an additional struct ptr arg for such a case.
        // we'll probably want to include a tag for the format too (json/binary/etc) to allow for detecting this when reading in.

        // @TODO: maybe worth adding instances (returned from open, passed to write/close) to allow for multithreaded export?
        // or perhaps we can just multithread the intensive tasks like audio encoding, but constrain the final serialization to a single thread.

        /** Opens a new export context with a given id. In practice, this will generally map to creating a new file. */
        public open_context open_context;

        /** Closes the currently open context. */
        public close_context close_context;

        /** Writes bytes/characters out to the currently open context. */
        public write write;

        /** User data, passed to the callbacks to provide state. */
        public void_ptr user;

    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_export_filter_t
    {
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_exported_item_group_info_t
    {
        public char_ptr item_type;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_exported_item_info_t
    {
        public char_ptr item_name;
    }

    public delegate void elias_exported_item_group_callback_t(elias_exported_item_group_info_t item_group_info, void_ptr user);
    public delegate void elias_exported_item_callback_t(elias_exported_item_info_t item_info, void_ptr user);

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct elias_export_options_t
    {
        /** Filter controlling what assets to include in the export */
        public elias_export_filter_t filter;

        // @TODO: export format
        // debug options

        /** An optional callback, invoked for every item type for which items are being exported */
        public elias_exported_item_group_callback_t item_group_callback;

        /** An optional callback, invoked for every item that is exported */
        public elias_exported_item_callback_t item_callback;

        /** Optional user data, passed to any callbacks */
        public void_ptr user;

    }
}
