using System;
using System.Collections.Generic;
using EliasSoftware.Elias4.Native.Runtime;
using UnityEngine;

namespace EliasSoftware.Elias4.Runtime {

    using Elias4.Common;

    public enum InstanceEventType {
        CREATED,
        DESTROYED
    }

    public enum SessionEventType {
        UNDEFINED,
        CREATED,
        DESTROYED
    }

    public delegate void InstanceEvent(InstanceEventType eventType, IEliasInstanceID instanceId);
    public delegate void SessionEvent(SessionEventType eventType, SessionID sessionId);

    public interface IEliasRuntime {
        SessionID CreateSession( int numChannels=2,
                    int framesPerBuffer=512,
                    int sampleRate=48000 );
        SessionID CreateSessionWithManualRendering(
                    int numChannels = 2,
                    int framesPerBuffer = 512,
                    int sampleRate = 48000 );
        bool    ProcessBuffer(object renderBuffercallback);
        void    AddSessionEventHandler(SessionEvent eventHandler);
        void    RemoveSessionEventHandler(SessionEvent eventHandler);
        void    DestroySession(SessionID session);
        IEnumerable<SessionID> EnumerateSessions();
        int     GetSessionCount();
        void    UpdateAllSessions(double deltaTime);
        void    UpdateSession( SessionID session,
                    double deltaTime );
        ListenerID CreateListener( SessionID session );
        void    DestroyListener( SessionID session, 
                    ListenerID listener );
        InstanceID CreateSoundInstance( SessionID session,
                    IPatchID patch);
        void    DestroySoundInstance( SessionID session,
                    InstanceID instance );
        ZoneID CreateZone( SessionID session );
        void    DestroyZone( SessionID session,
                    ZoneID zone );

        void LoadReverb(SessionID session, IIRID reverb);
        void UnloadReverb(SessionID session, IIRID reverb);

        IEnumerable<IEliasInstanceID> EnumerateInstances( SessionID session,
                                            IEliasID modelType,
                                            IEliasID modelId=null);
        int     GetInstanceCount( SessionID session,
                    IEliasID modelType=null,
                    IEliasID modelId=null);

        void    ApplyOperation( SessionID session,
                    IEliasInstanceID instance,
                    EliasOperation operation);

        void    SetGlobalParameter( SessionID session,
            IParameterID parameter,
            EliasValue paramValue );

        int PolygonMaxPoints { get; }
        bool SetMusicProject(string musicProjectContentUTF8, string basePath);
    }

    public interface IEliasStandaloneRuntime {
        bool Initialize(out RuntimeHandle runHandle);
        void Free(RuntimeHandle handle);
        EliasID EliasIDFromHexString(string idStr);
        string HexStringFromEliasID(EliasID id);
        bool AssetExists(EliasID id);
    }
}