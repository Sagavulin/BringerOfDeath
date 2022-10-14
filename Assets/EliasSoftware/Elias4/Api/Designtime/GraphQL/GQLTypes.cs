using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace EliasSoftware.Elias4.Designtime {

    [Serializable]
    public class GQLProject {

        [SerializeField]
        public string name;

        [SerializeField]
        public string uri;

        [SerializeField]
        public string id;

        [SerializeField]
        public string uuid;

        public GQLProject(string name, string uri, string id, string uuid) {
            this.name = name;
            this.uri = uri;
            this.id = id;
            this.uuid = uuid;
        }
        public GQLProject() {
            name = "";
            uri = "";
            id = "";
            uuid = "";
        }

        public bool IsValid => uri != null
            && uri.Length > 0 
            && id != null 
            && id.Length > 0;
    }

    [System.Serializable]
    public class GQLWorkspaceItem {
        public string path;
        public string id;
    }

    [System.Serializable]
    public class GQLWorkspaceAssetItem :
        GQLWorkspaceItem
    {
        public string uri;
        public string assetTypeName;
    }

    [System.Serializable]
    public class GQLValue {
        public string name;
        public string id;

        public override string ToString()
            => string.Format("( name: \"{0}\", id: \"{1}\" )", name, id);
    }

    [System.Serializable]
    public class GQLEnum {
        public string name;
        public string id;
        public List<GQLValue> values;

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendFormat("Enum: ( name: \"{0}\", id: \"{1}\", values: ", name, id);
            foreach(var v in values)
                sb.Append("\n\t").Append(v);
            sb.Append(")");
            return sb.ToString();
        }
    }

    [System.Serializable]
    public class GQLParameterValue {
        public string boolValue;
        public string intValue;
        public string floatValue;
        public string enumValue;
    }

    [System.Serializable]
    public class GQLParameterType {
        public string typeName;
        public string reference;

        public override string ToString()
            => string.Format("( typeName: \"{0}\", reference: {1} )", typeName, "<ref>");
    }

    [System.Serializable]
    public class GQLParameter {
        public string name;
        public string id;
        public GQLParameterValue defaultValue;
        public GQLParameterType type;

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendFormat("( name: \"{0}\", id: \"{1}\", type: {2} )", name, id, type);
            return sb.ToString();
        }
    }

    [System.Serializable]
    public class GQLPatch {
        public string name;
        public string id;
        public List<GQLParameter> parameters;

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendFormat("Patch: ( name: \"{0}\", id: \"{1}\", parameters: ", name, id);
            foreach(var p in parameters)
                sb.Append("\n\t").Append(p);
            sb.Append(")");
            return sb.ToString();
        }
    }

    [System.Serializable]
    public class GQLAsset
    {
        public string path;
        public string uri;
        public string name;
        public string id;

        public GQLAsset(string path, string uri, string name, string id)
        {
            this.path = path == null ? "" : path;
            this.uri = uri == null ? "" : uri;
            this.name = name == null ? "" : name;
            this.id = id == null ? "" : id;
        }

        public GQLAsset() {
            this.path = "";
            this.uri  = "";
            this.name = "";
            this.id  = "";
        }

        public static GQLAsset FromURI(string uri)
            => new GQLAsset("", uri, "", "");
        public static GQLAsset FromPath(string path)
            => new GQLAsset(path, "", "", "");

        public override string ToString()
        {
            return string.Format("(name={0}, path={1}, id={2}, uri={3})",
                name == null ? "null" : name,
                path == null ? "null" : path,
                id == null ? "null" : id,
                uri == null  ? "null" :
                    uri.Length < 10 ? uri :
                        uri.Substring( 0, Mathf.Min( uri.Length-1, 10 ) ) + " ...");
        }
    }
}