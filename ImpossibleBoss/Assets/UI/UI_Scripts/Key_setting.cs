using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Key_setting : MonoBehaviour
{
    public InputActionReference triggerAction;
    public void ChangeBinding()
    {       
        Logger.Log(triggerAction.action.bindings[0]);
        var rebindOperation = triggerAction.action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .Start();
    }
}
