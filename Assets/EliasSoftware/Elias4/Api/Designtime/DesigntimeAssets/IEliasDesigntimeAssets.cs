using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Designtime
{
    using EliasSoftware.Elias4.Common;
    
    public interface IEliasDesigntimeAssets :
        IEliasAssets 
    {
        void Start();
        void ClearAllData();
        void SetRefreshOptions(int size, int interval);
        void ForceRefreshNow();
        IEnumerable<(string, IEliasID)> Enumerate(EntryType entryType);
        IEnumerable<(string, IPatchID)> EnumeratePatches();
        IEnumerable<(string, IEnumID)> EnumerateEnums();
        IEnumerable<(string, IEnumValueID)> EnumerateEnumDomain(IEnumID enumId);
        IEnumerable<(string, IIRID)> EnumerateIRs();
        IEnumerable<(string, IParameterID, ParamType, string)> EnumeratePatchParameters(IPatchID patchId);
        void Stop();
        string ExportToJson();
    }
}