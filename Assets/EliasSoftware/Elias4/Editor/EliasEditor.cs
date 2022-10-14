//#define ENABLE_PREVIEW_SESSION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Editor {

    using EliasSoftware.Elias4;
    using EliasSoftware.Elias4.Common;
    using EliasSoftware.Elias4.Designtime;
    using EliasSoftware.Elias4.Runtime;

    public class EliasEditor :
        Elias
    {
        private EliasDesigntimeAssets assets = null;
        private IEliasRuntime eliasRuntime;
        private AudioReader audioReader;
        private SessionID PreviewSessionId;
        public IEliasDesigntimeAssets EditorAssets => assets;
        public override IEliasAssets Assets => assets;
        public override IEliasRuntime Runtime => eliasRuntime;

        private Dictionary<string, (IParameterID, ParamType, string)> parameters = null;

#if UNITY_EDITOR

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() {
            Setup(EliasSettings.Instance, new EliasEditor());
        }
#endif

        private void ReadUpGlobalParameters() {
            parameters = new Dictionary<string, (IParameterID, ParamType, string)>();
            foreach(var param in EditorAssets.EnumerateGlobalParameters()) {
                if(parameters.ContainsKey(param.Item1) == false)
                    parameters[param.Item1] = (param.Item2, param.Item3, param.Item4);
            }
        }

        protected override void Start( IEliasPaths settings )
        {
            var projectPath = settings.ProjectFilePath;
            var customAssetPath = settings.AssetMetadataFilePath;
            var autoLoad = settings.AutoLoadMusicProject;
            var musicProject = settings.MusicProjectFilePath;

            var res = UnityEditorSession.instance.SetActiveProject(projectPath, customAssetPath);
            if( res == false ) {
                if ( projectPath != null && projectPath.Length > 0 ) {
                    var name = System.IO.Path.GetFileNameWithoutExtension(projectPath);
                    Debug.LogErrorFormat("Failed to activate project {0}. " + 
                    "If the the project is open in Elias Studio, please close Elias Studio and then return to Unity." +
                    "\nOnce project is activated, please use 'Remoting Session' in Elias Studio to access the project.", name);
                } else {
                    Debug.LogWarning("Elias could not be started.");
                }
                return;
            }

            assets = UnityEditorSession.instance.CreateDesigntimeAssets();
            Debug.Assert(assets != null);
            if( assets == null )
                return;
                
            ReadUpGlobalParameters();

            eliasRuntime = UnityEditorSession.instance.CreateRuntime();
            Debug.Assert(eliasRuntime != null);
            if( eliasRuntime == null ) {
                assets.Stop();
                assets = null;
                eliasRuntime = null;
                return;
            }

            SessionId = eliasRuntime.CreateSessionWithManualRendering();

#if ENABLE_PREVIEW_SESSION
            PreviewSessionId = runtime.CreateSession();
#else
            PreviewSessionId = new SessionID(0);
#endif

            audioReader = AudioReader.MakeInstance();
            audioReader.Runtime = eliasRuntime;
            
            if(autoLoad && musicProject != null && musicProject.Length > 0) {
                LoadMusicProject(eliasRuntime, musicProject);
            }
        }

        protected override void Stop( ) 
        {
            if (audioReader != null) {
                audioReader.Destroy();
            }

            if( eliasRuntime != null ) {
                if(PreviewSessionId.IsValid)
                    eliasRuntime.DestroySession(PreviewSessionId);
                if(SessionId.IsValid)
                    eliasRuntime.DestroySession(SessionId);
                SessionId = new SessionID(0);
                PreviewSessionId = new SessionID(0);
                eliasRuntime = null;
            }

            if( parameters != null ) {
                parameters.Clear();
                parameters = null;
            }

            if( assets == null )
                return;

            var designAssets = assets as EliasDesigntimeAssets;
            Debug.Assert(designAssets != null,
                "unexpected implementation of IEliasAssets");
            if(designAssets != null) {
                designAssets.Stop();
            }
            assets = null;
        }

        public override void SetGlobalParameter(string name, EliasValue value)
        {
            if( parameters.ContainsKey(name) == false ) {
                Debug.LogWarningFormat("Global parameter name {0} was not found", name);
                return;
            }
            
            SetGlobalParameterValue(parameters[name].Item1, value);
        }

        public override void SetGlobalParameter(string name, IEliasValueBuilder value)
        {
            if( parameters.ContainsKey(name) == false ) {
                Debug.LogWarningFormat("Global parameter name {0} was not found", name);
                return;
            }
            
            SetGlobalParameterValue(parameters[name].Item1, value);
        }

        public bool IsPreviewEnabled() {
            return PreviewSessionId.IsValid;
        }

        public PreviewSession CreatePreviewSession() {
            return new PreviewSession(PreviewSessionId);
        }

        public void LoadMusicProject(IEliasRuntime runtime, string musicProjectPath)
        {
            if(System.IO.File.Exists(musicProjectPath) == false) {
                Debug.LogWarningFormat("Could not locate the music project at the provided path:\n\t{0}\n",
                    musicProjectPath);
                return;
            }
            var content = System.IO.File.ReadAllText(musicProjectPath,
                System.Text.Encoding.UTF8);
            var basePath = System.IO.Path.GetDirectoryName(musicProjectPath);
            var res = runtime.SetMusicProject(content, basePath);
            Debug.AssertFormat(res, "Failed to load music project {0}", musicProjectPath);
        }
    }
}