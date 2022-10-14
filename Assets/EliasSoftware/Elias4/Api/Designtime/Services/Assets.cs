using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Designtime {

    using EliasSoftware.Elias4;

    public class Assets {
        public ApiConsumer ApiConsumer { get; set; }
        public static SelectA1WithContinuation getEnum =
            GQLTool.BindSelectWithContinuation("_enum", "getEnum", "_uri");

        public static SelectA1WithContinuation getPatch =
            GQLTool.BindSelectWithContinuation("patch", "getPatch", "_uri");

        public static SelectA1WithContinuation getConfig =
            GQLTool.BindSelectWithContinuation("config", "getConfig", "_uri");

        public static SelectA1WithContinuation getIR =
            GQLTool.BindSelectWithContinuation("ir", "getIR", "_uri");

        // Project
        public static SelectA1WithContinuation listContents =
            GQLTool.BindSelectWithContinuation("results", "listContents", "folder");

        public static SelectA1WithContinuation listAssetsByTypeAlias =
            GQLTool.BindSelectWithContinuation("assets", "listAssets", "assetTypeAlias");

        public static SelectA1WithContinuation listAssetsByNameFilter =
            GQLTool.BindSelectWithContinuation("assets", "listAssets", "assetNameFilter");

        public static SelectA2WithContinuation listAssetsByNameAndType =
            GQLTool.BindSelectWithContinuation("assets", "listAssets", "assetNameFilter", "assetTypeAlias");

        // Test
        public static SelectA3 addAsset =
            GQLTool.BindSelect("uri", "addAsset", "parentFolder", "assetName", "assetTypeAlias");

        // loadAsset(path | uri) : Asset
        public static SelectA1WithContinuation loadAssetFromPath =
            GQLTool.BindSelectWithContinuation("asset", "loadAsset", "path");

        // unloadAsset (path | uri): Boolean
        public static SelectA1WithContinuation unloadAssetWithUri =
            GQLTool.BindSelectWithContinuation("asset", "unloadAsset", "uri");

        public List<GQLWorkspaceItem> ListContents(GQLProject project, string projectPath)
        {
            var query = GQL.Mutation(
                Process.getProject(Utils.QuotedString(project.uri),
                    listContents(Utils.QuotedString(projectPath),
                            GQL.Selection(GQL.Field("path"), GQL.Field("id")))));

            var result = Utils.SendAndExpect(query,
                new { project = new { results = new List<GQLWorkspaceItem>() } }, ApiConsumer.ApiKey);

            return result == null ? null : result.project.results;
        }

        public List<GQLParameter> ListGlobalParameters(GQLProject project)
        {
            var query = GQL.Mutation(
                Process.getProject(Utils.QuotedString(project.uri),
                            GQL.Selection( GQL.Field("globalParameters",
                                    GQL.Selection( GQL.Field("name"),
                                                       GQL.Field("id"),
                                                       GQL.Field("defaultValue",
                                                            GQL.Selection(
                                                                GQL.Field("boolValue"),
                                                                GQL.Field("intValue"),
                                                                GQL.Field("floatValue"),
                                                                GQL.Field("enumValue"))),
                                                       GQL.Field("type",
                                                            GQL.Selection(
                                                                GQL.Field("typeName"),
                                                                GQL.Field("reference"))))))));

            var result = Utils.SendAndExpect(query,
                new { project = new { globalParameters = new List<GQLParameter>() } }, ApiConsumer.ApiKey);

            if(result!=null)
                return result.project.globalParameters;

	        return null;
        }

        public List<GQLWorkspaceAssetItem> ListAssetsByTypeName(GQLProject project, string typeName)
        {
            var query = GQL.Mutation(
                Process.getProject(Utils.QuotedString(project.uri),
                    listAssetsByTypeAlias(Utils.QuotedString(typeName),
                        GQL.Selection(  GQL.Field("uri"),
                                            GQL.Field("assetTypeName"),
                                            GQL.Field("path"),
                                            GQL.Field("id")))));

            var result = Utils.SendAndExpect(query,
                new { project = new { assets = new List<GQLWorkspaceAssetItem>() } },
                ApiConsumer.ApiKey, null);

            if(result!=null)
                return result.project.assets;
            else
                return null;
        }

        public List<GQLWorkspaceAssetItem> ListAssetsByName(GQLProject project, string assetName)
        {
            var query = GQL.Mutation(
                Process.getProject(Utils.QuotedString(project.uri),
                    listAssetsByNameFilter(Utils.QuotedString(assetName),
                        GQL.Selection(  GQL.Field("uri"),
                                        GQL.Field("assetTypeName"),
                                        GQL.Field("path"),
                                        GQL.Field("id")))));

            var result = Utils.SendAndExpect(query,
                new { project = new { assets = new List<GQLWorkspaceAssetItem>() } },
                ApiConsumer.ApiKey, null);

            if(result!=null)
                return result.project.assets;
            else
                return null;
        }

        public List<GQLWorkspaceAssetItem> ListAssetsByNameAndType(GQLProject project, string assetName, string typeName)
        {
            var query = GQL.Mutation(
                Process.getProject(Utils.QuotedString(project.uri),
                    listAssetsByNameAndType(Utils.QuotedString(assetName),
                                                    Utils.QuotedString(typeName),
                        GQL.Selection(  GQL.Field("uri"),
                                        GQL.Field("assetTypeName"),
                                        GQL.Field("path"),
                                        GQL.Field("id")))));

            var result = Utils.SendAndExpect(query,
                new { project = new { assets = new List<GQLWorkspaceAssetItem>() } },
                ApiConsumer.ApiKey, null);

            if(result!=null)
                return result.project.assets;
            else
                return null;
        }

        public string AddAsset(GQLProject project,
            string parentFolder,
            string assetName,
            string assetTypeAlias)
        {
            var mutation = GQL.Mutation(
                Process.getProject(Utils.QuotedString(project.uri),
                    addAsset(Utils.QuotedString(parentFolder),
                        Utils.QuotedString(assetName),
                        Utils.QuotedString(assetTypeAlias))));

            var result = Utils.SendAndExpect(mutation,
                new { project = new { uri = "" } }, ApiConsumer.ApiKey);

            if(result!=null) {
                return result.project.uri;
            } else {
                return null;
            }
        }

        public GQLEnum GetEnum(GQLProject project, string enumUri)
        {
            var query = GQL.Query(
                    getEnum(Utils.QuotedString(enumUri),
                            GQL.Selection(
                                GQL.Field("name"),
                                GQL.Field("id"),
                                    GQL.Field("values",
                                        GQL.Selection( GQL.Field("name"),
                                                       GQL.Field("id"))))));
            var result = Utils.SendAndExpect(query,
                new { _enum = new GQLEnum() }, ApiConsumer.ApiKey, null, true);
            if(result == null) {
                return null;
            }
            return result._enum;
        }

        public string GetEnumName(GQLProject project, string enumUri)
        {
            var query = GQL.Query(
                    getEnum( Utils.QuotedString(enumUri),
                              GQL.Selection( GQL.Field("name") )));

            var result = Utils.SendAndExpect(query,
                new { _enum = new { name = "" } }, ApiConsumer.ApiKey, null, true);

            if(result == null) {
                return null;
            }

            return result._enum.name;
        }

        public GQLPatch GetPatch(GQLProject project, string patchUri)
        {
            var query = GQL.Query(
                    getPatch(Utils.QuotedString(patchUri),
                            GQL.Selection(
                                GQL.Field("name"),
                                GQL.Field("id"),
                                    GQL.Field("parameters",
                                        GQL.Selection( GQL.Field("name"),
                                                       GQL.Field("id"),
                                                       GQL.Field("defaultValue",
                                                            GQL.Selection(
                                                                GQL.Field("boolValue"),
                                                                GQL.Field("intValue"),
                                                                GQL.Field("floatValue"),
                                                                GQL.Field("enumValue"))),
                                                       GQL.Field("type",
                                                            GQL.Selection(
                                                                GQL.Field("typeName"),
                                                                GQL.Field("reference"))))))));

            var result = Utils.SendAndExpect(query,
                new { patch = new GQLPatch() }, ApiConsumer.ApiKey, null, true);
            if(result == null) {
                return null;
            }
            return result.patch;
        }

        public GQLPatch GetConfig(GQLProject project, string patchUri)
        {
            var query = GQL.Query(
                    getConfig(Utils.QuotedString(patchUri),
                            GQL.Selection(
                                GQL.Field("name"),
                                GQL.Field("id"),
                                    GQL.Field("parameters",
                                        GQL.Selection( GQL.Field("name"),
                                                       GQL.Field("id"),
                                                       GQL.Field("defaultValue",
                                                            GQL.Selection(
                                                                GQL.Field("boolValue"),
                                                                GQL.Field("intValue"),
                                                                GQL.Field("floatValue"),
                                                                GQL.Field("enumValue"))),
                                                       GQL.Field("type",
                                                            GQL.Selection(
                                                                GQL.Field("typeName"),
                                                                GQL.Field("reference"))))))));

            var result = Utils.SendAndExpect(query,
                new { config = new GQLPatch() }, ApiConsumer.ApiKey, null, true);
            if(result == null) {
                return null;
            }
            return result.config;
        }

        public string GetPatchName(GQLProject project, string patchUri)
        {
            var query = GQL.Query(
                    getPatch( Utils.QuotedString(patchUri),
                              GQL.Selection( GQL.Field("name") )));

            var result = Utils.SendAndExpect(query,
                new { patch = new { name = "" } }, ApiConsumer.ApiKey, null, true);

            if(result == null) {
                return null;
            }

            return result.patch.name;
        }

        public GQLAsset GetIR(GQLProject project, string patchUri)
        {
            var query = GQL.Query(
                    getIR(Utils.QuotedString(patchUri),
                            GQL.Selection(
                                GQL.Field("path"),
                                GQL.Field("uri"),
                                GQL.Field("name"),
                                GQL.Field("id"))));
            var result = Utils.SendAndExpect(query,
                new { ir = new GQLAsset() }, ApiConsumer.ApiKey, null, true);
            if(result == null) {
                return null;
            }
            return result.ir;
        }

        public GQLAsset LoadAsset(GQLProject project, string assetPath)
        {
            var mutation = GQL.Mutation(
                Process.getProject(Utils.QuotedString(project.uri),
                    loadAssetFromPath(Utils.QuotedString(assetPath),
                            GQL.Selection(
                                GQL.Field("path"),
                                GQL.Field("uri"),
                                GQL.Field("name"),
                                GQL.Field("id")))));
            var result = Utils.SendAndExpect(mutation,
                new { project = new {
                        asset = new GQLAsset("","","","")
                    }
                }, ApiConsumer.ApiKey);
            if(result == null)
                return null;
            return result.project.asset;
        }
    }
}
