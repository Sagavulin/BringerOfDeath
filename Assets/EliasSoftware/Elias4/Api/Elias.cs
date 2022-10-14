//#define ENABLE_PREVIEW_SESSION

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is root namespace for the API.
/// All definitions for standard use cases are contained within this namespace.
/// </summary>
namespace EliasSoftware.Elias4
{
    using EliasSoftware.Elias4.Common;
    using EliasSoftware.Elias4.Runtime;

    /// <summary>
    /// The high-level API definition.
    /// </summary>
	public abstract class Elias
    {
        private static IEliasPaths sessionSettings;
        private static Elias eliasInstance = null;

        /// <summary>
        /// This static (read only) property will return an existing instance of the API if it exists and is configured.
        /// </summary>
        /// <value>Elias API</value>
        public static Elias API {
            get {
                if(IsInitialized)
                    return eliasInstance;
                eliasInstance.Start(sessionSettings);
                return eliasInstance;
            }
        }

        /// <summary>
        /// This static (read only) property is true if the API is initialized and false if not.
        /// </summary>
        public static bool IsInitialized => eliasInstance != null
            && eliasInstance.Assets != null
            && eliasInstance.Runtime != null;


        /// <summary>
        /// Stops the API instance.
        /// This should not be called by the game code (under normal circumstances) since the Elias 4 API lifecycle is handled by the plugin.
        /// </summary>
        public static void Shutdown() {
            if(eliasInstance != null)
                eliasInstance.Stop();
            eliasInstance = null;
        }

        /// <summary>
        /// Initializes the provided Elias instance with settings and sets the current API instance to the one provided.
        /// This should not be called by the game code (under normal circumstances) since the Elias 4 API lifecycle is handled by the plugin.
        /// </summary>
        public static void Setup(IEliasPaths settings, Elias apiInstance) {
            Debug.Assert(apiInstance != null);
            Debug.Assert(settings != null);
            eliasInstance = apiInstance;
            sessionSettings = settings;
#if UNITY_EDITOR
            eliasInstance.Start(sessionSettings);
#endif
        }

        /// <summary>
        /// Starts the API instance with the provided settings.
        /// </summary>
        protected abstract void Start( IEliasPaths settings );

        /// <summary>
        /// Shuts down the API instance.
        /// </summary>
        protected abstract void Stop( );

        /// <summary>
        /// Sets a global parameter with the provided name.
        /// </summary>
        /// <param name="name">The name of the global parameter as defined in the Elias Studio project.</param>
        /// <param name="value">The value to set to the global parameter.</param>
        public abstract void SetGlobalParameter(string name, EliasValue value);

        /// <summary>
        /// Sets a global parameter with the provided name.
        /// </summary>
        /// <param name="name">The name of the global parameter as defined in the Elias Studio project.</param>
        /// <param name="value">The value to set to the global parameter.</param>
        public abstract void SetGlobalParameter(string name, IEliasValueBuilder value);
        /// <summary>
        /// The current Elias-Session ID.
        /// </summary>
        protected SessionID SessionId;

        /// <summary>
        /// Access to the local ID store.<br>
        /// Used for looking up IDs from names among other things.<br>
        /// Currently, accessing IDs from here directly is a very slow operation.<br>
        /// Note: Subject to change in future releases.
        /// </summary>
        public abstract IEliasAssets Assets { get; }

        /// <summary>
        /// Provides access to the lower-level native libraries.
        /// </summary>
        public abstract IEliasRuntime Runtime { get; }

        /// <summary>
        /// Retrieves the Current Session ID.
        /// </summary>
        /// <returns>Current Session ID</returns>
        public SessionID GetSession( ) {
            if( Runtime == null )
                return new SessionID(0);
            return SessionId;
        }

        /// <summary>
        /// This method is used together with CommitDeferredCommands to tell the API when it is time to update the session.
        /// </summary>
        public void SetDeferredCommandsSpecified()
        {
            newFrame = true;
        }

        private double lastUpdate = Time.unscaledTimeAsDouble;
        private bool newFrame = false;
        
        protected virtual bool IsValidAssetID( IEliasID assetId ) {
            return assetId.IsValid;
        }

        /// <summary>
        /// This method is used together with SetDeferredCommandsSpecified to tell the API when it is time to update the session.
        /// </summary>
        public void CommitDeferredCommands() {
            if(newFrame == false)
                return;
            if(Runtime == null)
                return;
            double now = Time.unscaledTimeAsDouble;
            double delta = now - lastUpdate;
            Runtime.UpdateSession(SessionId, delta);
            lastUpdate = now;
            newFrame = false;
        }

        /// <summary>
        /// Triggers an update on the current session.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last call to this method in seconds.</param>
        public void Update(float deltaTime)
        {
            if(Runtime == null)
                return;
            Runtime.UpdateSession(SessionId, deltaTime);
        }

        /// <summary>
        /// Creates a listener in the current session and returns it's ID.<br>
        /// On failure an invalid ID is returned.
        /// </summary>
        public ListenerID CreateListener() {
            if( IsInitialized == false )
                return ListenerID.Zero;
            return Runtime.CreateListener(SessionId);
        }

        /// <summary>
        /// Tells the audio engine to destroy the listener bound to this ID in the current session.
        /// </summary>
        /// <param name="listener">The Listener ID of the listener to be destroyed.</param>
        public void DestroyListener( ListenerID listener ) {
            if( IsInitialized == false || listener.IsValid == false )
                return;
            Runtime.DestroyListener(SessionId, listener);
        }

        /// <summary>
        /// Creates a sound instance from a given patch in the current session.
        /// </summary>
        /// <param name="patch">The Patch ID from which to create a sound instance.</param>
        /// <returns>A sound instance ID.<br>On failure an invalid ID is returned.</returns>
        public InstanceID CreateSoundInstance( IPatchID patch ) {
            if( IsInitialized == false || IsValidAssetID( patch ) == false )
                return InstanceID.Zero;
            return Runtime.CreateSoundInstance(SessionId, patch);
        }

        /// <summary>
        /// Tells the audio engine to destroy the sound instance bound to this ID in the current session.
        /// </summary>
        /// <param name="instance">The ID of the sound instance to destroy.</param>
        public void DestroySoundInstance(InstanceID instance) {
            if(IsInitialized == false || instance.IsValid == false )
                return;
            Runtime.DestroySoundInstance(SessionId, instance);
        }

        /// <summary>
        /// Creates a zone in the current session.
        /// </summary>
        /// <returns>The ID of the new zone.<br>On failure an invalid ID is returned.</returns>
        public ZoneID CreateZone() {
            if(IsInitialized == false )
                return ZoneID.Zero;
            return Runtime.CreateZone(SessionId);
        }

        /// <summary>
        /// Destroys a zone in the current session.
        /// </summary>
        /// <param name="zone">The ID of the zone to destroy.</param>
        public void DestroyZone(ZoneID zone) {
            if( IsInitialized == false || zone.IsValid == false )
                return;
            Runtime.DestroyZone(SessionId, zone);
        }

        /// <summary>
        /// Sets the location of a sound instance in the current session.
        /// </summary>
        /// <param name="instance">The ID of the sound instance.</param>
        /// <param name="pos">The position of the sound instance.</param>
        public void SetSoundLocation(InstanceID instance, Vector3 pos) {
            if( IsInitialized == false || instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundPosition(pos);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets the position and orientation of a sound instance in the current session.
        /// </summary>
        /// <param name="instance">The ID of the sound instance.</param>
        /// <param name="pos">The position of the sound instance.</param>
        /// <param name="orientation">The orientation of the sound instance.</param>
        public void SetSoundLocation(InstanceID instance, Vector3 pos, Quaternion orientation) {
            if(IsInitialized == false || instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundTransform(pos, orientation);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets the position and orientation of a listener in the current session.
        /// </summary>
        /// <param name="listener">The ID of the listener.</param>
        /// <param name="pos">The position of the listener.</param>
        /// <param name="orientation">The listener's orientation.</param>
        public void SetListenerLocation(ListenerID listener, Vector3 pos, Quaternion orientation) {
            if(IsInitialized == false || listener.IsValid == false )
                return;
            var operation = EliasOperation.SetListenerTransform(pos, orientation);
            Runtime.ApplyOperation(SessionId, listener, operation);
        }

        /// <summary>
        /// Sets the reverb mode on a listener in the current session.
        /// Having this disabled means that the sound that reaches the listener will be free from any IR effects.
        /// </summary>
        public void SetListenerReverbMode(ListenerID listener, bool enabled) {
            if(IsInitialized == false || listener.IsValid == false )
                return;
            var operation = EliasOperation.SetListenerReverbMode(enabled);
            Runtime.ApplyOperation(SessionId, listener, operation);
        }

        /// <summary>
        /// Sets the position and orientation of a sound instance in the current session.
        /// </summary>
        public void SetSoundTransform(InstanceID instance, Transform transform) {
            if(IsInitialized == false || instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundTransform(transform);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets the position and orientation of a listener in the current session.
        /// </summary>
        public void SetListenerTransform(ListenerID listener, Transform transform) {
            if( IsInitialized == false || listener.IsValid == false )
                return;
            var operation = EliasOperation.SetListenerTransform(transform);
            Runtime.ApplyOperation(SessionId, listener, operation);
        }

        /// <summary>
        /// Enables or disabled spatialization for a sound instance in the current session.
        /// </summary>
        public void SetSoundSpatialization(InstanceID instance, bool value) {
            if(IsInitialized == false || instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundSpatialization(value);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets parameters for panning of a sound in relation to the listener in the current session.
        /// </summary>
        /// <param name="instance">The sound instance ID.</param>
        /// <param name="start">The distance the sound should start to fold, at the point where the direction of the sound starts to be apparent.</param>
        /// <param name="end">The distance where the sound, on a right angle to the listener, should be completely panned to one side.</param>
        /// <param name="startSpread">The angle at which the listener panning should start to react.</param>
        /// <param name="endSpread">The angle between the listener's forward vector and the vector to the source where the panning effect should be fully applied.</param>
        /// <param name="useLinearFalloff">Specifies if the falloff should be linear (true) or none (false).</param>
        public void SetSoundSpread(InstanceID instance, double start, double end, double startSpread, double endSpread, bool useLinearFalloff = true) {
            if(IsInitialized == false || instance.IsValid == false)
                return;
            var operation = EliasOperation.SetSoundSpread(start, end, startSpread, endSpread, useLinearFalloff);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Enables or disables spatialization for a listener in the current session.
        /// </summary>
        public void SetListenerSpatialization(ListenerID listener, bool enabled)
        {
            if(IsInitialized == false || listener.IsValid == false )
                return;
            var operation = EliasOperation.SetListenerSpatialization(enabled);
            Runtime.ApplyOperation(SessionId, listener, operation);
        }

        /// <summary>
        /// Sets the stereo width of the sound from a sound instance in the current session.
        /// </summary>
        public void SetSoundStereoWidth(InstanceID instance, double value)
        {
            if( IsInitialized == false || instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundStereoWidth(value);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets the master channel amplitude of a sound instance in the current session.
        /// </summary>
        public void SetSoundMasterAmplitude(InstanceID instance, double value)
        {
            if( IsInitialized == false || instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundMasterAmplitude(value);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets the master channel pitch of a sound instance in the current session.
        /// </summary>
        public void SetSoundMasterPitch(InstanceID instance, double value)
        {
            if(IsInitialized == false || instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundMasterPitch(value);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets a sound instance to automatically be destroyed when it is done playing.
        /// </summary>
        /// <param name="instance">The ID of a sound instance.</param>
        /// <param name="value">
        /// true - the instance should be destroyed.<br>
        /// false - the default, do not destroy when silent.
        /// </param>
        public void SetDestroyOnSilence(InstanceID instance, bool value)
        {
            if(IsInitialized == false || instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundDestroyOnSilence(value);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets attenuation configuration on a sound instance in the current session.
        /// </summary>
        /// <param name="instance">The ID of a sound instance.</param>
        /// <param name="maxAttenuation">The value at which attenuation should be clamped when the distance is beyond the end-value</param>
        /// <param name="start">The distance at which the sound should start to drop off in volume.</param>
        /// <param name="end">The maximum distance the sound will reach (unless see maxAttenuation).</param>
        /// <param name="useLinearFalloff">Use linear falloff (true) or none (false).</param>
        public void SetSoundDistanceAttenuation(InstanceID instance,
            double maxAttenuation,
            double start,
            double end,
            bool useLinearFalloff = true)
        {
            if(IsInitialized == false|| instance.IsValid == false)
                return;
            var operation = EliasOperation.SetSoundDistanceAttenuation(maxAttenuation, start, end, useLinearFalloff);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets the velocity of a sound source in the current session.
        /// </summary>
        /// <param name="instance">The ID of a sound instance.</param>
        /// <param name="linear">The linear velocity.</param>
        /// <param name="angular">The angular velocity.</param>
        public void SetSoundVelocity(InstanceID instance, Vector3 linear, Vector3 angular)
        {
            if(IsInitialized == false || instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundVelocity(linear, angular);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Instructs the audio engine to treat a sound instance as a spot sound source.
        /// </summary>
        public void SetSoundSpotSource(InstanceID instance)
        {
            if(IsInitialized == false|| instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundSpotSource();
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets the zone bleed behaviour of a sound instance in the current session.
        /// </summary>
        public void SetSoundZoneBleed(InstanceID instance, ZoneSoundBleedBehaviour behaviour)
        {
            if(IsInitialized == false|| instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundZoneBleed(behaviour);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Instructs the audio engine to treat this sound instance as a cone shaped source.
        /// </summary>
        /// <param name="instance">The ID of a sound instance.</param>
        /// <param name="innerConeAngle">The angle in radians describing the inner cone.</param>
        /// <param name="outerConeAngle">The angle in radians describing the outer cone.</param>
        /// <param name="outerConeAmplitudeGain">The amplitude level of sound in the outer cone.</param>
        /// <param name="angularAttenuationStartRadius">Angular attenuation within falloff start distance (attenuation)</param>
        public void SetSoundConeSource(InstanceID instance,
            double innerConeAngle,
            double outerConeAngle,
            double outerConeAmplitudeGain,
            bool angularAttenuationStartRadius = false)
        {
            if(IsInitialized == false|| instance.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundConeSource(innerConeAngle, outerConeAngle, outerConeAmplitudeGain, angularAttenuationStartRadius);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets the minimum channel spread for the sound emitted from a sound instance. 
        /// </summary>
        /// <param name="instance">The ID of a sound instance.</param>
        /// <param name="value">A value between 0.0 an 1.0.</param>
        public void SetSoundMinChannelSpread(InstanceID instance, double value)
        {
            if(IsInitialized == false || instance.IsValid == false )
                return;
            var val = value > 0.999f ? 0.999f : value;
            val = val < 0.0001f ? 0.0001f : val;
            var operation = EliasOperation.SetSoundMinChannelSpread(val);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets reverb (IR effect) related configuration on a sound instance in the current session.
        /// </summary>
        /// <param name="instance">The ID of a sound instance.</param>
        /// <param name="distanceMin">The minimum distance for which the sound should be affected.</param>
        /// <param name="distanceMax">The maximum distance for which the sound should be affected.</param>
        /// <param name="reverbMin">The send proportion (Dry/Wet value) at the minimum distance.</param>
        /// <param name="reverbMax">The send proportion (Dry/Wet value) at the maximum distance.</param>
        public void SetSoundReverb(InstanceID instance,
            float distanceMin,
            float distanceMax,
            float reverbMin,
            float reverbMax)
        {
            if(IsInitialized == false || instance.IsValid == false )
                return;
            distanceMax = Mathf.Max(distanceMin, distanceMax);
            reverbMax = Mathf.Max(reverbMax, reverbMin);
            distanceMin = Mathf.Min(distanceMin, distanceMax);
            reverbMin = Mathf.Min(distanceMin, reverbMin);
            var operation = EliasOperation.SetSoundReverb(distanceMin, distanceMax, reverbMin, reverbMax);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets the geometry of a zone in the current session.
        /// </summary>
        /// <param name="zone">The ID of a zone in the current session.</param>
        /// <param name="geometry">The geometry of the zone.</param>
        /// <param name="transform">A transform to apply on the provided geometry.</param>
        public void SetZoneGeometry(ZoneID zone,
            Geometry geometry,
            Transform transform)
        {
            if(IsInitialized == false || zone.IsValid == false )
                return;
            var operation = EliasOperation.SetZoneGeometry(geometry, transform);
            Runtime.ApplyOperation(SessionId, zone, operation);
        }

        /// <summary>
        /// Sets the geometry of a zone in the current session.
        /// </summary>
        /// <param name="zone">The ID of a zone in the current session.</param>
        /// <param name="geometry">The geometry of the zone.</param>
        /// <param name="position">The position of the geometry object.</param>
        /// <param name="orientation">The orientation of the geometry object.</param>
        /// <param name="scale">The scale of the geometry object.</param>
        public void SetZoneGeometry(ZoneID zone,
            Geometry geometry,
            Vector3 position,
            Quaternion orientation,
            Vector3 scale)
        {
            if(IsInitialized == false || zone.IsValid == false )
                return;
            var operation = EliasOperation.SetZoneGeometry(geometry, position, orientation, scale);
            Runtime.ApplyOperation(SessionId, zone, operation);
        }

        /// <summary>
        /// Sets the inbound occlusion for a zone.<br>
        /// Or in other words, sets how the sound should be affected when the source is on the outside of a zone while the listener is on the inside.
        /// </summary>
        /// <param name="zone">The ID of a zone in the current session.</param>
        /// <param name="attenuationEndDistance">The end distance for attenuation.</param>
        /// <param name="filterEndDistance">The end distance for the filter effect.</param>
        /// <param name="filterDryWet">The occlusion filter send proportion (Dry/Wet balance).</param>
        /// <param name="filterFrequency">The frequency of the occlusion filter (a low pass filter)</param>
        /// <param name="maxAttenuation">The amplitude occluded sounds should drop to.</param>
        public void SetZoneInboundOcclusion(ZoneID zone,
            double attenuationEndDistance,
            double filterEndDistance,
            double filterDryWet,
            double filterFrequency,
            double maxAttenuation)
        {
            if(IsInitialized == false || zone.IsValid == false )
                return;
            var operation = EliasOperation.SetZoneInboundOcclusion(attenuationEndDistance,
                filterEndDistance, filterDryWet, filterFrequency, maxAttenuation);
            Runtime.ApplyOperation(SessionId, zone, operation);
        }

        /// <summary>
        /// Sets the outbound occlusion for a zone.<br>
        /// Or in other words, sets how the sound should be affected when the source is on the inside of a zone while the listener is on the outside.
        /// </summary>
        /// <param name="zone">The ID of a zone in the current session.</param>
        /// <param name="attenuationEndDistance">The end distance for attenuation.</param>
        /// <param name="filterEndDistance">The end distance for the filter effect.</param>
        /// <param name="filterDryWet">The occlusion filter send proportion (Dry/Wet balance).</param>
        /// <param name="filterFrequency">The frequency of the occlusion filter (a low pass filter)</param>
        /// <param name="maxAttenuation">The amplitude occluded sounds should drop to.</param>
        public void SetZoneOutboundOcclusion(ZoneID zone,
            double attenuationEndDistance,
            double filterEndDistance,
            double filterDryWet,
            double filterFrequency,
            double maxAttenuation)
        {
            if(IsInitialized == false || zone.IsValid == false )
                return;
            var operation = EliasOperation.SetZoneOutboundOcclusion(attenuationEndDistance,
                filterEndDistance, filterDryWet, filterFrequency, maxAttenuation);
            Runtime.ApplyOperation(SessionId, zone, operation);
        }

        /// <summary>
        /// Loads an IR effect into memory.
        /// </summary>
        /// <param name="ir">The ID of an IR effect asset.</param>
        public void LoadIR(IIRID ir)
        {
            if( IsInitialized == false || IsValidAssetID(ir) == false )
                return;
            Runtime.LoadReverb(SessionId, ir);
        }

        /// <summary>
        /// Unloads an IR effect from memory.
        /// </summary>
        /// <param name="ir">The ID of an IR effect asset.</param>
        public void UnloadIR(IIRID ir)
        {
            if(IsInitialized == false || IsValidAssetID(ir) == false )
                return;
            Runtime.UnloadReverb(SessionId, ir);
        }

        /// <summary>
        /// Sets the active effect on a zone in the current session.
        /// </summary>
        /// <param name="zone">The ID of a zone in the current session.</param>
        /// <param name="ir">The ID of an IR filter asset.</param>
        /// <param name="sendProportion">The send proportion (Dry/Wet balance).</param>
        public void SetZoneIR( ZoneID zone,
            IIRID ir,
            double sendProportion)
        {
            if(IsInitialized == false || zone.IsValid == false || IsValidAssetID(ir) == false )
                return;
            var operation = EliasOperation.SetZoneReverb(ir, sendProportion);
            Runtime.ApplyOperation(SessionId, zone, operation);
        }

        /// <summary>
        /// Enables or disables vertical blending for a zone in the current session. 
        /// </summary>
        /// <param name="zone">The ID of a zone in the current session.</param>
        /// <param name="enabled">Vertical blending on (true) / off (false)</param>
        public void SetZoneSetEnableVerticalBlending( ZoneID zone,
            bool enabled)
        {
            if(IsInitialized == false || zone.IsValid == false )
                return;
            var operation = EliasOperation.SetZoneSetEnableVerticalBlending(enabled);
            Runtime.ApplyOperation(SessionId, zone, operation);
        }

        ////////// PARAMETERS ///////////////

        /// <summary>
        /// Sets a user defined parameter value to a sound instance in the current session.
        /// </summary>
        /// <param name="instance">The ID of a sound instance.</param>
        /// <param name="parameter">The ID of the parameter the value should be set to.</param>
        /// <param name="value">The value to set as an argument to the parameter.</param>
        public void SetSoundParameter(InstanceID instance, IParameterID parameter, int value) {
            if( IsInitialized == false || instance.IsValid == false || parameter.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundIntParam(parameter, value);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets a user defined parameter value to a sound instance in the current session.
        /// </summary>
        /// <param name="instance">The ID of a sound instance.</param>
        /// <param name="parameter">The ID of the parameter the value should be set to.</param>
        /// <param name="value">The value to set as an argument to the parameter.</param>
        public void SetSoundParameter(InstanceID instance, IParameterID parameter, double value) {
            if(IsInitialized == false || instance.IsValid == false || parameter.IsValid == false)
                return;
            var operation = EliasOperation.SetSoundDoubleParam(parameter, value);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets a user defined parameter value to a sound instance in the current session.
        /// </summary>
        /// <param name="instance">The ID of a sound instance.</param>
        /// <param name="parameter">The ID of the parameter the value should be set to.</param>
        /// <param name="value">The value to set as an argument to the parameter.</param>
        public void SetSoundParameter(InstanceID instance, IParameterID parameter, bool value) {
            if(IsInitialized == false || instance.IsValid == false || parameter.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundBoolParam(parameter, value);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets a user defined parameter value to a sound instance in the current session.
        /// </summary>
        /// <param name="instance">The ID of a sound instance.</param>
        /// <param name="parameter">The ID of the parameter the value should be set to.</param>
        /// <param name="value">The ID of a value to set as an argument to the parameter.</param>
        public void SetSoundParameter(InstanceID instance, IParameterID parameter, IEliasID value) {
            if( IsInitialized == false || value.IsValid == false || parameter.IsValid == false )
                return;
            var operation = EliasOperation.SetSoundIDParam(parameter, value);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets a user defined parameter value to a sound instance in the current session.
        /// </summary>
        /// <param name="instance">The ID of a sound instance.</param>
        /// <param name="parameter">The ID of the parameter the value should be set to.</param>
        /// <param name="value">The value to set as an argument to the parameter.</param>
        public void SetSoundParameter(InstanceID instance, IParameterID parameter, EliasValue value) {

            if( IsInitialized == false
                || parameter.IsValid == false
                || value.Type == ParamType.Empty )
                return;

            if( value.Type == ParamType.EnumValue ) {
                var id = value.Data as IEliasID; 
                if( id == null || id.IsValid == false )
                    return;
            }

            var operation = EliasOperation.SetSoundParam(parameter, value);
            Runtime.ApplyOperation(SessionId, instance, operation);
        }

        /// <summary>
        /// Sets a global parameter for the current session.
        /// </summary>
        /// <param name="parameter">The ID of the parameter,</param>
        /// <param name="value">The value to set as an argument to the parameter.</param>
        public void SetGlobalParameterValue(IParameterID parameter, IEliasValueBuilder value) {
            if(IsInitialized == false || parameter.IsValid == false )
                return;

            var val = value.GetEliasValue();
            if(val.Type == ParamType.EnumValue) {
                var id = val.Data as IEliasID; 
                if(id == null || id.IsValid == false)
                    return;
            }

            Runtime.SetGlobalParameter(SessionId, parameter, val);
        }

        /// <summary>
        /// Sets a global parameter for the current session.
        /// </summary>
        /// <param name="parameter">The ID of the parameter.</param>
        /// <param name="value">The value to set as an argument to the parameter.</param>
        public void SetGlobalParameterValue(IParameterID parameter, EliasValue value) {
            if(IsInitialized == false || parameter.IsValid == false || value.Type == ParamType.Empty)
                return;
                
            if(value.Type == ParamType.EnumValue) {
                var id = value.Data as IEliasID; 
                if(id == null || id.IsValid == false)
                    return;
            }
            
            Runtime.SetGlobalParameter(SessionId, parameter, value);
        }
    }
}
