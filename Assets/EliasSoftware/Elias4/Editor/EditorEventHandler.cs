namespace EliasSoftware.Elias4.Editor {

    using UnityEngine;
    using UnityEditor;
    using Elias4;
    using EliasSoftware.Elias4.Common;
    using EliasSoftware.Elias4.Designtime;

    [InitializeOnLoad]
    public static class EditorEventHandler
    {
	    private static bool WantsToQuit() {
            return true;
        }
        private static void OnQuit() {
            DesigntimeAPI.Shutdown();
        }

        private static void OnPlayModeStateChange(PlayModeStateChange stateChange) {
            if(stateChange == PlayModeStateChange.EnteredEditMode) {
                OnEnterEditMode();
            }
        }

        private static void OnEnterEditMode() {
            if(Elias.IsInitialized)
                Elias.Shutdown();
            Elias.Setup(EliasSettings.Instance, new EliasEditor());
        }

        private static void OnProjectChanged() {

        }

	    static EditorEventHandler()
        {
            EditorApplication.quitting += OnQuit;
            EditorApplication.wantsToQuit += WantsToQuit;
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
            EditorApplication.projectChanged += OnProjectChanged;

            EditorApplication.delayCall += () => {
                if(Application.isPlaying)
                    return;
                OnEnterEditMode();
            };
        }
    }
}
