using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Common
{
    public interface IEliasAssets {
        IPatchID GetPatchID(string patchName);
        (IParameterID, ParamType, string) GetParameterInfo(IPatchID patchId, string parameterName);
        IEnumerable<(string, IParameterID, ParamType, string)> EnumerateGlobalParameters();
        IEnumID GetEnumID(string enumName);
        IEnumValueID GetEnumValueID(IEnumID enumId, string enumValue);
        IEnumValueID GetEnumValueID(string enumValue);
        IParameterID GetGlobalParameter(string name);
        IIRID GetIRID(string irName);
        string GetName(IPatchID patchId);
        string GetName(IPatchID patchId, IParameterID paramId);
        string GetName(IEnumID enumId);
        string GetName(IEnumID enumId, IEnumValueID valueId);
        string GetName(IEnumValueID valueId);
        string GetName(IIRID irId);
        string GetGlobalParameterName(IParameterID paramId);
    }
}