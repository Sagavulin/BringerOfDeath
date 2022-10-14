using System;
using System.Collections.Generic;
using UnityEngine;


namespace EliasSoftware.Elias4.Runtime {
    using Elias4.Native;
    using Elias4.Native.Runtime;
    using Elias4.Common;

	public class EliasRuntime :
		IEliasRuntime
	{
        private struct SessionEventHandlerData {
            private bool initialized;
            private uint sessionEventHandlerCallbackId;
            public event SessionEvent onSessionEvent;
            public bool IsInitialized { get => initialized; }
            public uint CallbackId { get => sessionEventHandlerCallbackId; }
            public bool NoListeners => onSessionEvent == null;
            public void Dispatch(SessionEventType stype, SessionID session) {
                if(onSessionEvent != null)
                    onSessionEvent.Invoke(stype, session);
            }
            public void Setup(uint callbackId) {
                sessionEventHandlerCallbackId = callbackId;
                initialized = true;
            }
            public void Reset() {
                onSessionEvent = null;
                initialized = false;
                sessionEventHandlerCallbackId = uint.MaxValue;
            }
        }

        private SessionEventHandlerData sessionEventData;
        private elias_handle_t runtimeHandle;
        private static elias_realtime_process_buffer_t ProcessBufferFn;
        private static IntPtr ProcessBufferHandle;

        public EliasRuntime(RuntimeHandle handle) {
            runtimeHandle = handle.Handle;
            sessionEventData.Reset();
        }

        public int PolygonMaxPoints => elias_extruded_polygon_t.ELIAS_POLYGON_MAX_POINTS;
        public bool ProcessBuffer(object renderBuffercallback)
            => ProcessBuffer((elias_rendered_buffer_callback_t)renderBuffercallback);
        private static bool ProcessBuffer(elias_rendered_buffer_callback_t callback)
        {
            if (ProcessBufferFn == null || ProcessBufferHandle == System.IntPtr.Zero)
                return false;
            ProcessBufferFn(ProcessBufferHandle, callback, System.IntPtr.Zero);
            return true;
        }

        public SessionID CreateSession( int numChannels=2,
            int framesPerBuffer=512, 
            int sampleRate=48000)
        {
            var config = new elias_session_config_t() {
                session_flags       = 0,
                num_channels        = numChannels,
                frames_per_buffer   = framesPerBuffer,
                sample_rate         = sampleRate,
                gain_interpolation_coefficient = 0.0f,
                panner_interpolation_frames = 256
            };

            elias_session_id_t sessionId;
            var res = EliasSessionLib.elias_create_session(
                runtimeHandle, 
                config, 
                out sessionId,
                null, IntPtr.Zero);

            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to create session\n\t{0}",
                res.ToString());

            if( res != elias_result_t.elias_success ) 
                return new SessionID(0);

            Debug.AssertFormat( sessionId.value != 0, "Successfully created session returned id=0," + 
                " the value 0 is treated as invalid but the unity plugin" );
            return new SessionID(sessionId.value);
		}
        public SessionID CreateSessionWithManualRendering(
            int numChannels = 2,
            int framesPerBuffer = 512,
            int sampleRate = 48000)
        {
            var config = new elias_session_config_t() {
                session_flags       = elias_session_flags_t.ELIAS_SESSION_MANUAL_RENDERING,
                num_channels        = numChannels,
                frames_per_buffer   = framesPerBuffer,
                sample_rate         = sampleRate,
                gain_interpolation_coefficient = 0.0f,
                panner_interpolation_frames = 256
            };

            elias_session_id_t sessionId;
            var res = EliasSessionLib.elias_create_session_with_manual_rendering(
                runtimeHandle,
                config,
                out sessionId,
                out ProcessBufferFn,
                out ProcessBufferHandle);

            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to create session\n\t{0}",
                res.ToString());

            if( res != elias_result_t.elias_success ) 
                return new SessionID(0);

            Debug.AssertFormat( sessionId.value != 0, "Successfully created session returned id=0," + 
                " the value 0 is treated as invalid but the unity plugin" );

            return new SessionID(sessionId.value);
        }

        public void DestroySession(SessionID session)
		{
			var res = EliasSessionLib.elias_destroy_session(
                runtimeHandle, session.ID);

            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to destroy session {0}\n\t{1}",
                session.ID.ToString(),
                res.ToString());
		}

        public int GetSessionCount()
		{
            elias_size_t size;
			var res = EliasSessionLib.elias_get_session_count(
                runtimeHandle, out size);
            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to get session count\n\t{0}",
                res.ToString());
            if(res != elias_result_t.elias_success) 
                return -1;
            return size;
		}

        public IEnumerable<SessionID> EnumerateSessions()
		{
            int nSessions = GetSessionCount();
            elias_size_t retrieved_size = 0;
            var session_ids = new elias_session_id_t[nSessions];
            var res = EliasSessionLib.elias_enumerate_sessions(
                runtimeHandle,
                session_ids,
                nSessions,
                out retrieved_size);
            
            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to enumerate sessions\n\t{0}",
                res.ToString());

            if( res != elias_result_t.elias_success )
                yield break;

            for(int i = 0; i < retrieved_size; i++)
                yield return new SessionID(session_ids[i].value);        
		}

        public void UpdateSession(SessionID session, double deltaTime) {
            deltaTime = deltaTime < double.Epsilon ? double.Epsilon : deltaTime;
            var res = EliasSessionLib.elias_session_update(runtimeHandle,
                session.ID, deltaTime);
            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to update session {0}\n\t{1}",
                session.ID.ToString(),
                res.ToString());
        }

        public void UpdateAllSessions(double deltaTime) {
            var sessions = EnumerateSessions();
            foreach(var session in sessions) {
                var res = EliasSessionLib.elias_session_update(runtimeHandle,
                    session.ID, deltaTime);
                Debug.AssertFormat(res == elias_result_t.elias_success,
                    "failed to update session {0}\n\t{1}",
                    session.ID.ToString(),
                    res.ToString());
            }
        }

		public InstanceID CreateSoundInstance(SessionID session,
            IPatchID patch)
		{
            elias_model_instance_id_t modelInstance;
			var res = EliasSessionLib.elias_session_create_sound_instance(
                runtimeHandle, 
                session.ID, 
                patch.UUID,
                out modelInstance);

            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to create component\n\t{0}",
                res.ToString());

            if( res != elias_result_t.elias_success ) 
                return InstanceID.Zero;

            return new InstanceID(modelInstance);
		}

        public void DestroySoundInstance(SessionID session, InstanceID instance)
		{
			var res = EliasSessionLib.elias_session_destroy_sound_instance(
                    runtimeHandle,
                    session.ID,
                    instance.ModelInstanceID);

            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to destroy component\n\t{0}",
                res.ToString());
		}

		public ListenerID CreateListener(SessionID session)
		{
            elias_model_instance_id_t modelInstance;
            elias_uuid_t listener_id; // note: currently not "used"
			var res = EliasSessionLib.elias_session_create_listener(
                runtimeHandle,
                session.ID,
                listener_id,
                out modelInstance);

            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to create listener\n\t{0}",
                res.ToString());

            if( res != elias_result_t.elias_success )
                return ListenerID.Zero;

            return new ListenerID(modelInstance);
		}

		public void DestroyListener(SessionID session, ListenerID listener)
		{
            var res = EliasSessionLib.elias_session_destroy_listener(
                runtimeHandle,
                session.ID,
                listener.ModelInstanceID);

            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to destroy listener\n\t{0}",
                res.ToString());
		}

        public int GetInstanceCount(SessionID session,
            IEliasID modelType=null,
            IEliasID modelId=null)
		{
            var filter = new elias_model_instance_filter_t() {
                model_id = modelId != null ? modelId.UUID : elias_uuid_t.AllZero,
                model_type_id = modelType != null ? modelType.UUID : elias_uuid_t.AllZero
            };

            elias_size_t size;
			var res = EliasSessionLib.elias_session_get_instance_count(
                runtimeHandle,
                session.ID,
                filter,
                out size);

            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to get instance count\n\t{0}",
                res.ToString());

            if(res != elias_result_t.elias_success)
                return -1;

            return size;
		}

		public IEnumerable<IEliasInstanceID> EnumerateInstances(
            SessionID session,
            IEliasID modelType,
            IEliasID modelId=null)
		{
            var filter = new elias_model_instance_filter_t() {
                model_id = modelId != null ? modelId.UUID : elias_uuid_t.AllZero,
                model_type_id = modelType != null ? modelType.UUID : elias_uuid_t.AllZero
            };

            elias_size_t count;
			var count_res = EliasSessionLib.elias_session_get_instance_count(
                runtimeHandle,
                session.ID,
                filter,
                out count);

            Debug.AssertFormat(count_res == elias_result_t.elias_success,
                "failed to get instance count (enumerating instances)\n\t{0}",
                count_res.ToString());

            if(count_res != elias_result_t.elias_success)
                yield break;

            var instances = new elias_model_instance_id_t[count];
            elias_size_t retrieved_count;
            var res = EliasSessionLib.elias_session_enumerate_instances(
                runtimeHandle,
                session.ID,
                filter,
                instances,
                count,
                out retrieved_count);

            Debug.AssertFormat(count_res == elias_result_t.elias_success,
                "failed to enumerate instances\n\t{0}",
                res.ToString());

            if(res != elias_result_t.elias_success)
                yield break;

            for(int i = 0; i < retrieved_count; i++)
                yield return new InstanceID(instances[i]);
        }

        private void SessionEventCallback(elias_session_id_t session_id,
                elias_session_lifecycle_event_t event_type,
                IntPtr user)
        {
            Debug.Assert(sessionEventData.IsInitialized);

            SessionEventType sessionEventType = SessionEventType.UNDEFINED;
            switch(event_type) {
                case elias_session_lifecycle_event_t.ELIAS_SESSION_CREATED: {
                    sessionEventType = SessionEventType.CREATED;
                } break;
                case elias_session_lifecycle_event_t.ELIAS_SESSION_DESTROYED: {
                    sessionEventType = SessionEventType.DESTROYED;
                } break;
                default: {
                    sessionEventType = SessionEventType.UNDEFINED;
                } break;
            }

            sessionEventData.Dispatch(sessionEventType,
                new SessionID(session_id));
        }

        private void RegisterSessionEventCallback()
        {
            if(sessionEventData.IsInitialized)
                return;

            uint callbackId;
			var res = EliasSessionLib
                        .elias_register_session_lifecycle_callback(
                            runtimeHandle,
                            0, // session filter flags
                            SessionEventCallback,
                            IntPtr.Zero,
                            out callbackId);

            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to register lifecycle callback\n\t{0}",
                res.ToString());

            if( res == elias_result_t.elias_success ) {
                sessionEventData.Setup(callbackId);
            }
        }

        private void UnregisterSessionEventCallback()
        {
            if(sessionEventData.IsInitialized == false)
                return;
            
            uint callbackId = sessionEventData.CallbackId;
			var res = EliasSessionLib
                        .elias_unregister_session_lifecycle_callback(
                            runtimeHandle,
                            callbackId);

            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to unregister lifecycle callback\n\t{0}",
                res.ToString());

            if( res == elias_result_t.elias_success ) {
                sessionEventData.Reset();
            }
        }

		public void AddSessionEventHandler(SessionEvent eventHandler)
		{
            // if this is the first listener
            //   - set up session event data
            //   - register the callback
            RegisterSessionEventCallback();
            sessionEventData.onSessionEvent += eventHandler;
		}

		public void RemoveSessionEventHandler(SessionEvent eventHandler)
		{
			if(sessionEventData.IsInitialized)
                sessionEventData.onSessionEvent -= eventHandler;
            // if there are no listeners un-register the callback
            if(sessionEventData.NoListeners)
                UnregisterSessionEventCallback();
		}

		public void ApplyOperation(SessionID session,
                IEliasInstanceID instance,
                EliasOperation operation)
		{
			var res = EliasSessionLib
                .elias_session_apply_operation(
                    runtimeHandle,
                    session.ID,
                    instance.ModelInstanceID,
                    operation.GetModelInstanceOperation());
            Debug.AssertFormat(res == elias_result_t.elias_success,
                "apply operation failed {0}",
                res.ToString());
		}

		public ZoneID CreateZone(SessionID session)
		{
            elias_model_instance_id_t id;
			var res = EliasSessionLib.elias_session_create_zone(runtimeHandle,
                session.ID, out id);
            Debug.AssertFormat(res == elias_result_t.elias_success,
                "create zone failed {0}",
                res.ToString());
            if(res == elias_result_t.elias_success)
                return new ZoneID(id);
            return ZoneID.Zero;
		}

		public void DestroyZone(SessionID session, ZoneID zone)
		{
			var res = EliasSessionLib.elias_session_destroy_zone(runtimeHandle,
                session.ID, zone.ModelInstanceID);
            Debug.AssertFormat(res == elias_result_t.elias_success,
                "destroy zone failed {0}",
                res.ToString());
		}

        public void LoadReverb(SessionID session, IIRID reverb)
		{
			var res = EliasSessionLib.elias_session_load_reverb(runtimeHandle,
                session.ID, reverb.UUID);
            Debug.AssertFormat(res == elias_result_t.elias_success,
                "load reverb failed {0}",
                res.ToString());
		}

        public void UnloadReverb(SessionID session, IIRID reverb)
		{
			var res = EliasSessionLib.elias_session_unload_reverb(runtimeHandle,
                session.ID, reverb.UUID);
            Debug.AssertFormat(res == elias_result_t.elias_success,
                "unload reverb failed {0}",
                res.ToString());
		}

        public void ForcePreloadAudio(SessionID session, List<IEliasID> audioAssets)
		{
            int len = audioAssets.Count;
            elias_uuid_t[] audioFiles = new elias_uuid_t[len];
            for(int i = 0; i < len; i++)
                audioFiles[i] = audioAssets[i].UUID; 

			var res = EliasSessionLib.elias_session_force_preload_audio(runtimeHandle,
                session.ID, audioFiles, len);
            Debug.AssertFormat(res == elias_result_t.elias_success,
                "force preload audio failed {0}",
                res.ToString());
		}

		public void SetGlobalParameter(SessionID session,
            IParameterID parameter,
            EliasValue paramValue)
		{
            if(paramValue.Type == ParamType.Empty) {
                Debug.LogError("Invalid type specified for global parameter value");
                return;
            }

            var val = paramValue.AsNative();
            if(val.type == elias_fundamental_types_t.ELIAS_FUNDAMENTAL_TYPE_UNSPECIFIED) {
                Debug.LogError("Invalid type specified for param value");
                return;
            }
            
			var res = EliasSessionLib.elias_session_set_global_param_value(runtimeHandle,
                session.ID, parameter.UUID, val);
            Debug.AssertFormat(res == elias_result_t.elias_success,
                "failed to set global parameter value {0}",
                res.ToString());
		}

        private bool PathEndsWithSlash(string basePath)
            => basePath.EndsWith("/") || basePath.EndsWith("\\");
		public bool SetMusicProject(string musicProjectContentUTF8, string basePath)
		{
            if( musicProjectContentUTF8 == null || musicProjectContentUTF8.Length == 0 ) {
                Debug.LogError("Music project content was empty or null");
                return false;
            }

            // calling with null param on when runtime
            if( basePath != null ) {
                
                // remove any extra whitespace
                basePath = basePath.Trim();

                if( PathEndsWithSlash(basePath) == false ) {
                    // need a slash at the end of the basePath
                    basePath = basePath + "/";
                }
            }

			var res = EliasMusicLib.elias_set_music_project(
                runtimeHandle, musicProjectContentUTF8, basePath);
            Debug.AssertFormat(res == elias_result_t.elias_success,
                "Failed to set music project {0}", res.ToString());
            return res == elias_result_t.elias_success;
		}
	}
}
