using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EliasSoftware.Elias4;
using EliasSoftware.Elias4.Common;

public class PlayerActionSfx : MonoBehaviour
{
    // Elias Sound instance to set parameter values
    public EliasSoundInstance instance;

    // hero action parameter
    public EliasPatchParameter playerActionParam;

    // Elias Enum values for jump, dash and fall
    public EliasEnumValue jumpValue;
    public EliasEnumValue dashValue;
    public EliasEnumValue fallValue;

    // parameter to specify the current jump force
    public EliasPatchParameter jumpForceParam;

    public void TriggerJumpSound(float force)
    {
        instance.SetParameter(jumpForceParam, EliasValue.CreateDouble(force));
        instance.SetParameter(playerActionParam, jumpValue);
    }

    public void TriggerDashSound()
    {
        instance.SetParameter(playerActionParam, dashValue);
    }

    public void TriggerFallSound()
    {
        instance.SetParameter(playerActionParam, fallValue);
    }
}
