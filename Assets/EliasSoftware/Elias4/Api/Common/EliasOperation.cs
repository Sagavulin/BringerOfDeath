using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Common
{
	using System;
	using Native;
    using System.Runtime.InteropServices;


    public class EliasOperation {

        private uint opType;
        private byte[] argData;
        private EliasOperation(uint operationType, int dataSize) {
            opType = operationType;
            argData = new byte[dataSize];
        }

        public elias_model_instance_operation_t GetModelInstanceOperation() {
            return new elias_model_instance_operation_t() {
                op = opType,
                args = argData,
                args_length = (uint)argData.Length
            };
        }
        private unsafe void SetArgData<T_Struct>(ref T_Struct data) {
            UnityEngine.Debug.Assert(Marshal.SizeOf(data) == argData.Length);
            fixed (byte* arrPtr = argData) {
                Marshal.StructureToPtr(data, (IntPtr)arrPtr, true);
            }
        }

        private static elias_sound_instance_set_param_value_t SIParamVal(
            IEliasID paramId, int val)
        {
            return new elias_sound_instance_set_param_value_t() {
                param_id = paramId.UUID,
                value = new elias_typed_value_t() { 
                    type = elias_fundamental_types_t.ELIAS_FUNDAMENTAL_TYPE_INT,
                    value = new elias_generic_value_t() {
                        int_value = val
                    } 
                }
            };
        }

        private static elias_sound_instance_set_param_value_t SIParamVal(
            IEliasID paramId, bool val)
        {
            return new elias_sound_instance_set_param_value_t() {
                param_id = paramId.UUID,
                value = new elias_typed_value_t() { 
                    type = elias_fundamental_types_t.ELIAS_FUNDAMENTAL_TYPE_BOOL,
                    value = new elias_generic_value_t() {
                        bool_value = val
                    } 
                }
            };
        }

        private static elias_sound_instance_set_param_value_t SIParamVal(
            IEliasID paramId, double val)
        {
            return new elias_sound_instance_set_param_value_t() {
                param_id = paramId.UUID,
                value = new elias_typed_value_t() { 
                    type = elias_fundamental_types_t.ELIAS_FUNDAMENTAL_TYPE_DOUBLE,
                    value = new elias_generic_value_t() {
                        double_value = val
                    } 
                }
            };
        }

        private static elias_sound_instance_set_param_value_t SIParamVal(
            IEliasID paramId, elias_uuid_t val)
        {
            return new elias_sound_instance_set_param_value_t() {
                param_id = paramId.UUID,
                value = new elias_typed_value_t() { 
                    type = elias_fundamental_types_t.ELIAS_FUNDAMENTAL_TYPE_ID,
                    value = new elias_generic_value_t() {
                        id_value = val
                    }
                }
            };
        }

        private static elias_sound_instance_set_param_value_t SIParamVal(IEliasID paramId,
            EliasValue val)
            => new elias_sound_instance_set_param_value_t() {
                param_id = paramId.UUID,
                value = val.AsNative()
            };

        private static EliasOperation CreateSetParameterValueOperation(
            elias_sound_instance_set_param_value_t setParamValOp )
        {
            var size = Marshal.SizeOf(setParamValOp);
            var operation = new EliasOperation(
                (uint)elias_sound_instance_op_t.ELIAS_SI_SET_PARAM_VALUE,
                size);
            operation.SetArgData(ref setParamValOp);
            return operation;
        }

        private static EliasOperation CreateSetValueOperation<T>(
            uint opType,
            T setParamValOp )
        {
            var size = Marshal.SizeOf(setParamValOp);
            var operation = new EliasOperation(
                opType,
                size);
            operation.SetArgData(ref setParamValOp);
            return operation;
        }

        private static EliasOperation CreateSetSoundInstanceValueOperation<T>(
            elias_sound_instance_op_t opType,
            T setParamValOp )
                => CreateSetValueOperation((uint)opType, setParamValOp);
        private static EliasOperation CreateSetListenerValueOperation<T>(
            elias_listener_op_t opType,
            T setParamValOp )
                => CreateSetValueOperation((uint)opType, setParamValOp);
        private static EliasOperation CreateSetZoneValueOperation<T>(
            elias_zone_op_t opType,
            T setParamValOp )
            => CreateSetValueOperation((uint)opType, setParamValOp);

        public static EliasOperation SetSoundParam(IParameterID paramId, EliasValue val)
            => CreateSetParameterValueOperation(SIParamVal(paramId, val));

        public static EliasOperation SetSoundIntParam(IParameterID paramId, int val)
            => CreateSetParameterValueOperation(SIParamVal(paramId, val));

        public static EliasOperation SetSoundBoolParam(IParameterID paramId, bool val)
            => CreateSetParameterValueOperation(SIParamVal(paramId, val));

        public static EliasOperation SetSoundDoubleParam(IParameterID paramId, double val)
            => CreateSetParameterValueOperation(SIParamVal(paramId, val));

        public static EliasOperation SetSoundIDParam(IParameterID paramId, IEliasID val)
            => CreateSetParameterValueOperation(SIParamVal(paramId, val.UUID));

        public static EliasOperation SetSoundDestroyOnSilence(bool val)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_DESTROY_ON_SILENCE,
                val);

        public static EliasOperation SetSoundMasterPitch(double val)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_MASTER_PITCH,
                val);

        public static EliasOperation SetSoundMasterAmplitude(double val)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_MASTER_AMPLITUDE,
                val);

        public static EliasOperation SetSoundInterpolationTime(double val)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_INTERPOLATION_TIME,
                val);

        public static EliasOperation SetSoundMinChannelSpread(double val)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_MIN_CHANNEL_SPREAD,
                val);

        public static EliasOperation SetSoundStereoWidth(double val)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_STEREO_WIDTH,
                val);
        public static EliasOperation SetSoundSpatialization(bool val)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_SPATIALIZATION,
                val);

        private static elias_location_t ToVec(double x, double y, double z)
            => new elias_location_t() { x_forward=z, y_right=x, z_up=y };
        private static elias_quaternion_t ToQuat(double x, double y, double z, double w)
            => new elias_quaternion_t() { x=z, y=x, z=y, w=w };

        private static elias_distance_attenuation_t DistAtten(double maxAttenuation, bool linearFalloff)
            => new elias_distance_attenuation_t() {
                    falloff_type = linearFalloff
                        ? elias_falloff_curve_type_t.ELIAS_FALLOFF_TYPE_LINEAR
                        : elias_falloff_curve_type_t.ELIAS_FALLOFF_TYPE_NONE,
                    max_attenuation = maxAttenuation
                };

        public static EliasOperation SetSoundPosition(
            double x, double y, double z)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_POSITION,
                new elias_sound_instance_set_position_t() {
                    location = ToVec(x, y, z),
                    enable_orientation = false
                });

        public static EliasOperation SetSoundPosition(Vector3 position)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_POSITION,
                new elias_sound_instance_set_position_t() {
                    location = ToVec(position.x, position.y, position.z),
                    enable_orientation = false
                });

        public static EliasOperation SetSoundTransform(Vector3 position, Quaternion orientation)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_POSITION,
                new elias_sound_instance_set_position_t() {
                    location = ToVec(position.x, position.y, position.z),
                    enable_orientation = true,
                    rotation = ToQuat(orientation.x, orientation.y, orientation.z, orientation.w)
                });

        public static EliasOperation SetSoundTransform(Transform transform)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_POSITION,
                new elias_sound_instance_set_position_t() {
                    location = ToVec(
                        transform.position.x,
                        transform.position.y,
                        transform.position.z ),
                    enable_orientation = true,
                    rotation = ToQuat(
                        transform.rotation.x,
                        transform.rotation.y,
                        transform.rotation.z,
                        transform.rotation.w )
                });

        public static EliasOperation SetSoundVelocity(Vector3 linear, Vector3 angular)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_VELOCITY,
                new elias_sound_instance_set_velocity_t() {
                    linear = new elias_location_t() { x_forward=linear.z, y_right=linear.x, z_up=linear.y },
                    angular = new elias_location_t() { x_forward=angular.z, y_right=angular.x, z_up=angular.y },
                });

        public static EliasOperation SetSoundDistanceAttenuation( double maxAttenuation,
            double start,
            double end, 
            bool linearFalloff = true)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_DISTANCE_ATTENUATION,
                new elias_sound_instance_set_distance_attenuation_t() {
                    attenuation = DistAtten(maxAttenuation, linearFalloff),
                    range = new elias_range_t() {
                        start = start,
                        end = end
                    }
                });

        // TODO:: This does not seem to work
        public static EliasOperation SetSoundAttenuation(
            double innerConeAngle,
            double outerConeAngle,
            double outerConeAmplitudeGain,
            double interpolationSpeed)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_LISTENER_ATTENUATION,
                new elias_listener_spatial_attenuation_defn_t() {
                    inner_cone_angle = innerConeAngle,
                    outer_cone_angle = outerConeAngle,
                    outer_cone_amplitude_gain = outerConeAmplitudeGain,
                    interpolation_speed = interpolationSpeed
                });

        public static EliasOperation SetSoundConeSource(
            double innerConeAngle,
            double outerConeAngle,
            double outerConeAmplitudeGain,
            bool angularAttenuationStartRadius)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_SOURCE_DEFN,
                new elias_sound_source_t() {
                    cone = new elias_cone_sound_source_defn_t() {
                        inner_cone_angle = innerConeAngle,
                        outer_cone_angle = outerConeAngle,
                        outer_cone_amplitude_gain = outerConeAmplitudeGain,
                        disable_angular_attenuation_within_start_radius = angularAttenuationStartRadius
                    },
                    type = elias_sound_source_type_t.ELIAS_SOUND_SOURCE_CONE
                });

        public static EliasOperation SetSoundSpotSource()
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_SOURCE_DEFN,
                new elias_sound_source_t() {
                    cone = new elias_cone_sound_source_defn_t() { },
                    type = elias_sound_source_type_t.ELIAS_SOUND_SOURCE_SPOT
                });

        public static EliasOperation SetSoundZoneBleed(ZoneSoundBleedBehaviour beh)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_ZONE_BEHAVIOUR,
                new elias_sound_instance_set_zone_behaviour_t() {
                    behaviour = (elias_zone_bleed_behaviour_t)beh
                });
        
        public static EliasOperation SetSoundReverb(float distance_min,
            float distance_max,
            float reverb_min,
            float reverb_max)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_REVERB,
                new elias_sound_instance_reverb_settings_t() {
                    distance_min = distance_min,
                    distance_max = distance_max,
                    reverb_min = reverb_min,
                    reverb_max = reverb_max
                });

        public static EliasOperation SetSoundSpread(double start, double end, double start_spread, double end_spread, bool useLinearFalloff = true)
            => CreateSetSoundInstanceValueOperation(
                elias_sound_instance_op_t.ELIAS_SI_SET_SPREAD,
                new elias_sound_instance_set_spread_t() {
                    range = new elias_range_t() {
                        start = start,
                        end = end
                    },
                    spread = new elias_spread_t() {
                        end_spread = end_spread,
                        start_spread = start_spread,
                        falloff_type = useLinearFalloff
                            ? elias_falloff_curve_type_t.ELIAS_FALLOFF_TYPE_LINEAR 
                            : elias_falloff_curve_type_t.ELIAS_FALLOFF_TYPE_NONE
                    }
                });

        public static EliasOperation SetListenerSpatialization(bool enabled)
            => CreateSetListenerValueOperation(
                elias_listener_op_t.ELIAS_LIST_SET_SPATIALIZATION,
                enabled);

        public static EliasOperation SetListenerReverbMode(bool enabled)
            => CreateSetListenerValueOperation(
                elias_listener_op_t.ELIAS_LIST_SET_REVERB_MODE,
                enabled);

        public static EliasOperation SetListenerTransform(Vector3 position, Quaternion orientation)
            => CreateSetListenerValueOperation(
                elias_listener_op_t.ELIAS_LIST_SET_POSITION,
                new elias_listener_set_position_t() {
                    location = ToVec(position.x, position.y, position.z),
                    rotation = ToQuat(orientation.x, orientation.y, orientation.z, orientation.w)
                });

        public static EliasOperation SetListenerTransform(Transform transform)
            => CreateSetListenerValueOperation(
                elias_listener_op_t.ELIAS_LIST_SET_POSITION,
                new elias_listener_set_position_t() {
                    location = ToVec(
                        transform.position.x,
                        transform.position.y,
                        transform.position.z ),
                    rotation = ToQuat(
                        transform.rotation.x,
                        transform.rotation.y,
                        transform.rotation.z,
                        transform.rotation.w )
                });

        public static EliasOperation SetZoneGeometry(Geometry geometry, Transform transform)
            => CreateSetZoneValueOperation(elias_zone_op_t.ELIAS_ZONE_SET_GEOMETRY,
                new elias_zone_set_geometry_t() {
                    geometry = ConvertUtils.UnityToElias(geometry, transform)
                });

        public static EliasOperation SetZoneGeometry(Geometry geometry, Vector3 position, Quaternion orientation, Vector3 scale)
            => CreateSetZoneValueOperation(elias_zone_op_t.ELIAS_ZONE_SET_GEOMETRY,
                new elias_zone_set_geometry_t() {
                    geometry = ConvertUtils.UnityToElias(geometry, position, orientation, scale)
                });


        public static EliasOperation SetZoneOutboundOcclusion( double attenuationEndDistance,
            double filterEndDistance,
            double filterDryWet,
            double filterFrequency,
            double maxAttenuation)
            => CreateSetZoneValueOperation(elias_zone_op_t.ELIAS_ZONE_SET_OUTBOUND_OCCLUSION,
                new elias_zone_set_occlusion_t() {
                    boundary = new elias_occlusion_boundary_config_t() {
                        attenuationEndDistance = attenuationEndDistance,
                        filterDryWet = filterDryWet,
                        filterEndDistance = filterEndDistance,
                        filterFrequency = filterFrequency,
                        maxAttenuation = maxAttenuation
                    }
                });

        public static EliasOperation SetZoneInboundOcclusion( double attenuationEndDistance,
            double filterEndDistance,
            double filterDryWet,
            double filterFrequency,
            double maxAttenuation)
            => CreateSetZoneValueOperation(elias_zone_op_t.ELIAS_ZONE_SET_INBOUND_OCCLUSION,
                new elias_zone_set_occlusion_t() {
                    boundary = new elias_occlusion_boundary_config_t() {
                        attenuationEndDistance = attenuationEndDistance,
                        filterDryWet = filterDryWet,
                        filterEndDistance = filterEndDistance,
                        filterFrequency = filterFrequency,
                        maxAttenuation = maxAttenuation
                    }
                });

        public static EliasOperation SetZoneReverb(IIRID reverb, double reverb_send_proportion)
            => CreateSetZoneValueOperation(elias_zone_op_t.ELIAS_ZONE_SET_REVERB,
                new elias_zone_reverb_config_t() {
                    reverb_asset_id = reverb.UUID,
                    reverb_send_proportion = reverb_send_proportion
                });

        public static EliasOperation SetZoneSetEnableVerticalBlending(bool enabled)
            => CreateSetZoneValueOperation(elias_zone_op_t.ELIAS_ZONE_SET_ENABLE_VERTICAL_BLENDING,
                new elias_zone_set_enable_vertical_blending_t() {
                    enabled = enabled
                });
    }

}