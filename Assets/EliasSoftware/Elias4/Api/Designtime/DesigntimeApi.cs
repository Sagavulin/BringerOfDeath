using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Designtime {

    using Common;
    using EliasSoftware.Elias4.Native.Designtime;

    public static class DesigntimeAPI
    {
        private static ApiConsumer currentApiConsumer = new ApiConsumer();
        public static ApiConsumer CurrentApiConsumer { get => currentApiConsumer; }
        public static bool IsEnabled() => currentApiConsumer != null
            && currentApiConsumer.ApiKey != null
            && currentApiConsumer.ApiKey.Length > 0;

        private static Process process;
        private static Assets assets;
        private static Export export;

        // MetaData/Operations/Mutation
        private static SelectA1 registerAPIConsumer =
            GQLTool.BindSelect("apiKey", "registerAPIConsumer", "name");
        private static SelectA1 unregisterAPIConsumer =
            GQLTool.BindSelect("result", "unregisterAPIConsumer", "apiKey");

        public static void Reset() {
            Debug.Log("elias_designtime_reset");
            // Reset the designtime lib
            EliasDesigntimeLib.elias_designtime_reset();
        }

        public static void SetActiveApiConsumer(ApiConsumer consumer) {
            Debug.Assert(consumer.IsValid);
            if(process == null)
                process = new Process();
            if(assets == null)
                assets = new Assets();
            if(export == null)
                export = new Export();
            process.ApiConsumer = consumer;
            assets.ApiConsumer = consumer;
            export.ApiConsumer = consumer;
            currentApiConsumer = consumer;
        }

        public static ApiConsumer CreateApiConsumer(string apiConsumer)
        {
            var mutation = GQL.Mutation(
                                registerAPIConsumer(
                                    Utils.QuotedString(apiConsumer)));

            var respObj = Utils.SendAndExpect(mutation,
                new { apiKey = "" }, null);

            if( respObj != null ) {
                return new ApiConsumer(respObj.apiKey, apiConsumer);
            }

            return new ApiConsumer();
        }

        public static void Disable(ApiConsumer context)
        {
            if(context == null)
                return;

            var mutation = GQL.Mutation(
                            unregisterAPIConsumer(
                                Utils.QuotedString(context.ApiKey)));

            var respObj = Utils.SendAndExpect(mutation,
                new { },
                context.ApiKey);
        }

        public static GQLProject OpenProject(string systemPath) {
            if(process == null)
                return new GQLProject();
            return process.OpenProject(systemPath, true);
        }

        public static bool CloseProject(GQLProject project) {
            if(process == null)
                return false;
            return process.CloseProject(project);
        }

        public static bool IsScanningProject(GQLProject project) {
            if(process == null)
                return false;
            return process.IsScanningProject(project);
        }

        public static GQLAsset LoadAsset(GQLProject project, string eliasProjectPath) {
            if(assets == null)
                return null;
            return assets.LoadAsset(project, eliasProjectPath);
        }

        public static List<GQLParameter> ListGlobalParameters(GQLProject project) {
            if(assets == null)
                return null;
            return assets.ListGlobalParameters(project);
        }

        public static List<GQLWorkspaceAssetItem> ListAssetsByName(GQLProject project, string assetName) {
            if(assets == null)
                return null;
            return assets.ListAssetsByName(project, assetName);
        }

        public static List<GQLWorkspaceAssetItem> ListAssetsByNameAndType(GQLProject project, string assetName, string typeName) {
            if(assets == null)
                return null;
            return assets.ListAssetsByNameAndType(project, assetName, typeName);
        }

        public static List<GQLWorkspaceAssetItem> ListAssetsByTypeName(GQLProject project, string typeName) {
            if(assets == null)
                return null;
            return assets.ListAssetsByTypeName(project, typeName);
        }

        public static GQLEnum GetEnum(GQLProject project, string uri) {
            if(assets == null)
                return null;
            return assets.GetEnum(project, uri);
        }

        public static GQLPatch GetPatch(GQLProject project, string uri) {
            if(assets == null)
                return null;
            return assets.GetPatch(project, uri);
        }

        public static GQLPatch GetConfig(GQLProject project, string uri) {
            if(assets == null)
                return null;
            return assets.GetConfig(project, uri);
        }

        public static GQLAsset GetIR(GQLProject project, string uri) {
            if(assets == null)
                return null;
            return assets.GetIR(project, uri);
        }

        public static RuntimeHandle GetRuntimeHandle(GQLProject project) {
            if(process == null)
                return RuntimeHandle.InvalidHandle;
            return process.GetRuntimeHandle(project);
        }

        public static bool ExportProject(GQLProject project, string targetDir) {
            if(export == null)
                return false;
            return export.ExportProject(project, targetDir);
        }

        public static IEnumerable<string> CreateExportedAssetIndex(string targetDir) {
            if(export == null)
                yield break;
            foreach(var id in export.EnumerateExportedProjectAssets(targetDir)) {
                yield return id.ToString();
            }
        }

        public static void Shutdown() {
            EliasDesigntimeLib.elias_designtime_shutdown();
        }
    }
}
