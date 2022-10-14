using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EliasSoftware.Elias4;

public class MaterialType : MonoBehaviour
{
    public EliasEnumValue footstepMaterial;

    private void OnTriggerEnter(Collider other)
    {
        var selector = other.GetComponent<FootstepMaterialSelector>();
        if(selector)
            selector.currentMaterialType = footstepMaterial;
    }
}
