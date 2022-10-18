using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EliasSoftware.Elias4;
using EliasSoftware.Elias4.Common;

public class PlayerActionSfx : MonoBehaviour
{
    // Elias Sound instance to set parameter values
    public EliasSoundInstance instance;

    // player action parameter
    public EliasPatchParameter playerActionParam;

    // Elias Enum values for jump, dash and fall
    public EliasEnumValue jumpValue;

    // parameter to specify the current jump force
    public EliasPatchParameter jumpForceParam;

    public void TriggerJumpSound()  // if input paremeter is needed add  "float force"
    {
        //instance.SetParameter(jumpForceParam, EliasValue.CreateDouble(force));
        instance.SetParameter(playerActionParam, jumpValue);
    }
}
