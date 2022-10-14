using System;
using UnityEditor;
using UnityEngine;

using EliasSoftware.Elias4.Designtime;
using EliasSoftware.Elias4.Runtime;
using EliasSoftware.Elias4.Common;
using System.Collections.Generic;
using System.Linq;

namespace EliasSoftware.Elias4.Editor
{
    public class UnityEditorSession :
        UnityEditor.ScriptableSingleton<UnityEditorSession>
    {
        [SerializeField]
        private ApiConsumer apiConsumer = new ApiConsumer();

        [SerializeField]
        private GQLProject project = new GQLProject();

        [SerializeField]
        private string eliasProjectPath = string.Empty;

        [SerializeField]
        private string assetPath = string.Empty;

        [SerializeField]
        private bool triggerForceRefreshAssets = true;

        [NonSerialized]
        private EliasDesigntimeAssets cachedAssets = null;

        public bool HasProjectPath => CheckProjectPath(eliasProjectPath);

        public bool HasValidProjectReference => CheckProject(project);

        public bool HasValidApiConsumer => apiConsumer.IsValid;
        
        public bool HasValidConfig => HasProjectPath
            && HasValidProjectReference
            && HasValidApiConsumer;

        private bool CheckProjectPath(string path) {
            return path != null && path.Length > 0 && path.EndsWith(".elias");
        }

        private bool CheckProject(GQLProject proj) {
            if(proj == null)
                return false;
            return CheckProjectURI(proj.uri);
        }

        private bool CheckProjectURI(string uri) {
            return uri != null && uri.Length > 0 && uri.TrimStart().StartsWith("elias://");
        }

        public bool SetActiveProject(string projectPath, string customAssetPath)
        {
            if( CheckProjectPath(projectPath) == false ) {
                if(projectPath == null || projectPath.Length > 0) {
                    Debug.LogErrorFormat("Incorrect project file path specified in Settings.\nReceived \"{0}\"",
                        projectPath);
                } else {
                    Debug.LogErrorFormat("Please specify the Elias project file path in Project Settings");
                }
                return false;
            }

            if( customAssetPath == null || customAssetPath.Length == 0 ) {
                Debug.LogError("Meta asset file path unspecified");
                return false;
            }

            if( apiConsumer == null || apiConsumer.IsValid == false ) {
                apiConsumer = DesigntimeAPI.CreateApiConsumer("EliasUnityPlugin");
                Debug.Assert(apiConsumer.IsValid);
                if(apiConsumer.IsValid == false) {
                    Debug.LogError("Invalid context");
                    return false;
                }
            }

            DesigntimeAPI.SetActiveApiConsumer(apiConsumer);

            if(projectPath != eliasProjectPath) {
                if( CheckProjectPath(eliasProjectPath) && CheckProject(project) ) {
                    DesigntimeAPI.CloseProject(project);
                }
                triggerForceRefreshAssets = true;
            }

            eliasProjectPath = projectPath;
            assetPath = customAssetPath;
            project = DesigntimeAPI.OpenProject(projectPath);

            return CheckProject(project);
        }

        public EliasDesigntimeAssets CreateDesigntimeAssets() {
            if( apiConsumer == null || apiConsumer.IsValid == false ) {
                Debug.LogError("The Designtime API is not initialized properly");
                return null;
            }
            if( project == null || project.IsValid == false ) {
                Debug.LogError("Invalid project URI or ID");
                return null;
            }

            EliasDesigntimeAssets assets = null;

            if( cachedAssets != null
                && cachedAssets.Project.id == project.id
                && cachedAssets.Project.IsValid )
            {
                assets = cachedAssets;
            }
            else
            {
                assets = EliasDesigntimeAssets.Create(project, assetPath);
                if( assets == null ) {
                    Debug.LogError("Failed to create designtime assets");
                    return null;
                }
            }

            assets.Start();
            assets.SetRefreshOptions(1, 30);

            if(triggerForceRefreshAssets) {
                assets.ForceRefreshNow();
                triggerForceRefreshAssets = false;
            }

            cachedAssets = assets;

            return assets;
        }

        public IEliasRuntime CreateRuntime() {
            if( project == null || project.IsValid == false ) {
                Debug.LogError("Invalid project URI or ID");
                return null;
            }
            var handle = DesigntimeAPI.GetRuntimeHandle(project);
            Debug.Assert(handle.IsValid);
            return new EliasRuntime(handle);
        }

        public IEliasDesigntimeAssets UnsafeDesigntimeAssets => cachedAssets;

        public List<string> CollectExportedAssetList( string targetDir ) {
            return DesigntimeAPI.CreateExportedAssetIndex( targetDir ).ToList();
        }

        public bool ExportAssets( string targetDir ) {

            if( apiConsumer == null || apiConsumer.IsValid == false ) {
                Debug.LogError("The Designtime API is not initialized properly");
                return false;
            }

            if( CheckProject(project) == false ) {
                Debug.LogError("Invalid project");
                return false;
            }

            return DesigntimeAPI.ExportProject(project, targetDir);
        }
    }
}
