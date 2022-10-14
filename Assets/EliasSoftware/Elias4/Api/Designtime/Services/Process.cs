using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Designtime {

    using EliasSoftware.Elias4.Common;
    using Elias4.Native.Designtime;
    using Elias4.Native;

    public class Process {

        public ApiConsumer ApiConsumer { get; set; }

        public static SelectA1WithContinuation getProcess =
            GQLTool.BindSelectWithContinuation("process", "getProcess", "_uri");

        public static SelectA1WithContinuation findOrOpenProject =
            GQLTool.BindSelectWithContinuation("project", "projects_findOrOpenProject", "projectFile");

        public static SelectA1 closeProject =
            GQLTool.BindSelect("result", "projects_closeProject", "uri");

        public static SelectA1WithContinuation getProject =
            GQLTool.BindSelectWithContinuation("project", "getProject", "_uri");

        public string GetProcessURI()
        {
            var query = GQL.Query(
                    GQLTool.SelectPath( "process",
                                            "uri"));
            var respObj = Utils.SendAndExpect( query,
                new { process = new {  uri = "" } }, ApiConsumer.ApiKey);

            if(respObj != null)                     
                return respObj.process.uri;

            return "";
        }

        public GQLProject OpenProject(string path, bool supressErrorMsg=false)
        {
            var process_uri = GetProcessURI();
            var mutation = GQL.Mutation(
                getProcess(Utils.QuotedString(process_uri), 
                    findOrOpenProject(Utils.BlockQuotedString(path),
                            GQL.Selection(  GQL.Field("uri"), 
                                            GQL.Field("name"),
                                            GQL.Field("id"),
                                            GQL.Field("uuid")))));

            var result = Utils.SendAndExpect(mutation,
                new { process = new { project = new GQLProject() } },
                ApiConsumer.ApiKey, null, supressErrorMsg);

            if( result != null )
                return result.process.project;
            else
                return new GQLProject() { name= "", uri="", id="" };
        }

        public RuntimeHandle GetRuntimeHandle(GQLProject project)
        {
            elias_handle_t eliasHandle;
            int success = EliasDesigntimeLib
                .elias_designtime_get_project_runtime_context(
                    project.id, out eliasHandle);

            if(success != 0) {
                return new RuntimeHandle(eliasHandle);
            }

            Debug.LogError("Failed to get project runtime handle");
            return RuntimeHandle.InvalidHandle;
        }

        public bool IsScanningProject(GQLProject project)
        {
            var mutation = GQL.Mutation(
                    getProject(Utils.QuotedString(project.uri),
                            GQL.Selection( GQL.Field("isScanning") )));

            var result = Utils.SendAndExpect(mutation,
                new { project = new { isScanning = false } }, ApiConsumer.ApiKey);

            if( result != null )
                return result.project.isScanning;
            else
                return false;
        }

        public bool CloseProject(GQLProject project)
        {
            var process_uri = GetProcessURI();
            var mutation = GQL.Mutation(
                getProcess(Utils.QuotedString(process_uri), 
                    closeProject(Utils.QuotedString(project.uri))));

            var result = Utils.SendAndExpect(mutation,
                new { process = new { result = false } }, ApiConsumer.ApiKey);

            if(result!=null)
                return result.process.result;
            else
                return false;
        }
    }
}