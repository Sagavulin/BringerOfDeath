using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EliasSoftware.Elias4;

public class FootstepMaterialSelector : MonoBehaviour
{
    // Elias Enum value defined in the Unity Editor
    public EliasEnumValue currentMaterialType;

    void Update()
    {
        // Call the Elias API with the name of the global parameter to set and the EliasEnumValue.
        Elias.API.SetGlobalParameter("FootStepMaterials", currentMaterialType);
    }
}
