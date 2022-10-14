using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Editor
{
    using EliasSoftware.Elias4.Common;

    public class PreviewSession {
        IEnumerator<bool> iterator = null;
        bool cancelled = false;
        float maxDuration = 10.0f;
        SessionID previewSession;

        public PreviewSession(SessionID session, float maxPreviewDuration = 5.0f) {
            maxDuration = maxPreviewDuration;
            previewSession = session;
        }

        public void Play(string patchName, string parameterName, string value)
        {
            Debug.Assert(Elias.IsInitialized);
            if( Elias.IsInitialized == false || previewSession.IsValid == false  )
                return;

            var previewPatch = Elias.API.Assets.GetPatchID(patchName);
            Debug.Assert(previewPatch.IsValid);
            if( previewPatch.IsValid == false )
                return;

            var previewParam = Elias.API.Assets.GetParameterInfo(previewPatch, parameterName);
            Debug.Assert(previewParam.Item1.IsValid);
            if( previewParam.Item1.IsValid == false )
                return;

            Play(previewPatch, previewParam.Item1, previewParam.Item2, value);
        }

        public void Play(IPatchID patchId, IParameterID paramId, ParamType type, string value) {

            Debug.Assert(Elias.IsInitialized);
            if( Elias.IsInitialized == false || previewSession.IsValid == false )
                return;

            Cancel();

            if( iterator != null )
                return;

            EliasOperation previewOp;
            if( PreviewParam(paramId, type, value, out previewOp) == false )
            {
                Debug.LogAssertionFormat("Failed to create preview operation. Value={0}", value);
                return;
            }

            cancelled = false;
            iterator = PreviewSessionSequence(patchId, previewOp).GetEnumerator();
        }

        public bool IsPlaying() {
            return iterator != null && iterator.Current;
        }

        public void Cancel() {
            if(iterator != null) {
                cancelled = true;
                int maxIter = 10;
                while(maxIter > 0 && Update()) {
                    maxIter --;
                }
                Debug.Assert(iterator.Current == false);
                if(iterator.Current == false)
                    iterator = null;
            }
        }

        public bool Update() {
            if(iterator == null)
                return false;
            iterator.MoveNext();
            return iterator.Current;
        }

        private bool PreviewParam(IParameterID paramId, ParamType type, string value, out EliasOperation op)
        {
            op = EliasOperation.SetSoundIntParam(paramId, -1);
            switch(type) {
                case ParamType.Int:
                {
                    int val;
                    if(int.TryParse(value, out val)) {
                        op = EliasOperation.SetSoundIntParam(paramId, val);
                        return true;
                    }
                } break;
                case ParamType.Pulse:
                {
                    int val;
                    if(int.TryParse(value, out val)) {
                        op = EliasOperation.SetSoundIntParam(paramId, val);
                        return true;
                    }
                } break;
                case ParamType.Double:
                {
                    double val;
                    if(double.TryParse(value, out val)) {
                        op = EliasOperation.SetSoundDoubleParam(paramId, val);
                        return true;
                    }
                } break;
                case ParamType.Bool:
                {
                    bool val;
                    if(bool.TryParse(value, out val)) {
                        op = EliasOperation.SetSoundBoolParam(paramId, val);
                        return true;
                    }
                } break;
                case ParamType.EnumValue:
                {
                    IEnumValueID id;
                    try {
                        id = EliasID.FromString(value);
                    } catch {
                        id = Elias.API.Assets.GetEnumValueID(value);
                    }

                    if(id.IsValid) {
                        op = EliasOperation.SetSoundIDParam(paramId, id);
                        return true;
                    }
                } break;
                default: {
                    Debug.LogErrorFormat("Unhandled parameter type {0}",
                        type.ToString());
                } break;
            }
            return false;
        }

        private IEnumerable<bool> PreviewSessionSequence(IPatchID patchId, EliasOperation previewOp)
        {
            var previewListener = Elias.API.Runtime.CreateListener(previewSession);
            float timer = maxDuration;
            float lastTime = Time.realtimeSinceStartup;
            Debug.Assert(previewListener.IsValid);

            var previewInstance = Elias.API.Runtime.CreateSoundInstance(previewSession, patchId);
            Debug.Assert(previewInstance.IsValid);

            // Send the patch param
            Elias.API.Runtime.ApplyOperation(previewSession, previewInstance, previewOp);

            while(cancelled == false && timer > 0.0f)
            {
                yield return true;
                var now = Time.realtimeSinceStartup;
                var delta = now - lastTime;
                lastTime = now;
                Elias.API.Runtime.UpdateSession(previewSession, delta);
                timer = timer - delta;
            }

            Elias.API.Runtime.DestroySoundInstance(previewSession, previewInstance);
            Elias.API.Runtime.DestroyListener(previewSession, previewListener);
            // the iterator is kept in a state where
            // it yields false waiting for an external reset
            while(true) {
                yield return false;
            }
        }
    }
}
