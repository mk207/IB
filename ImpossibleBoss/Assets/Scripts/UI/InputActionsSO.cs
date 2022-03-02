using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputActionsSO", menuName = "InputActions/InputActionsSO")]
public class InputActionsSO : ScriptableObject
{
    [SerializeField]
    private InputActionReference m_Skill1;
    [SerializeField]
    private InputActionReference m_Skill2;
    [SerializeField]
    private InputActionReference m_Skill3;
    [SerializeField]
    private InputActionReference m_Skill4;
    [SerializeField]
    private InputActionReference m_Skill5;
    [SerializeField]
    private InputActionReference m_Skill6;

    public InputActionReference GetRefernce(string actionName)
    {
        switch (actionName)
        {
            case "Skill1": return m_Skill1;
            case "Skill2": return m_Skill2;
            case "Skill3": return m_Skill3;
            case "Skill4": return m_Skill4;
            case "Skill5": return m_Skill5;
            case "Skill6": return m_Skill6;
            default: throw new System.ArgumentOutOfRangeException(nameof(actionName));
        }
    }

    //public InputActionReference Skill1 { get { return m_Skill1; } }
    //public InputActionReference Skill2 { get { return m_Skill2; } }
    //public InputActionReference Skill3 { get { return m_Skill3; } }
    //public InputActionReference Skill4 { get { return m_Skill4; } }
    //public InputActionReference Skill5 { get { return m_Skill5; } }
    //public InputActionReference Skill6 { get { return m_Skill6; } }
}
