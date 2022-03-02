using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindButton : MonoBehaviour
{
    //[SerializeField]
    //private InputActionReference mInputAction; //this is on the SO
    //[SerializeField]
    //private InputActionsSO m_InputActionSO;

    private InputAction mInputAction;

    [SerializeField]
    private bool m_ExcludeMouse = true;
    [Range(0, 10)]
    [SerializeField]
    private int selectedBinding;
    [SerializeField]
    private InputBinding.DisplayStringOptions displayStringOptions;
    [Header("Binding Info - DO NOT EDIT")]
    [SerializeField]
    private InputBinding inputBinding;
    private int bindingIndex;

    private string mActionName;

    [Header("UI Fields")]
    [SerializeField]
    private Text m_ActionText;
    [SerializeField]
    private Button m_RebindButton;
    [SerializeField]
    private Text m_RebindText;
    [SerializeField]
    private Button resetButton;

    //private InputActionsSO InputActionSO { get { return m_InputActionSO; } }

    private void Awake()
    {
        mActionName = m_ActionText.text;
        mInputAction = RebindInputManager.GetAction(mActionName);
    }

    private void OnEnable()
    {
        m_RebindButton.onClick.AddListener(() => DoRebind());
        //resetButton.onClick.AddListener(() => ResetBinding());
        //if (mInputAction == null)
        //{
        //    mInputAction = InputActionSO.GetRefernce(mActionName);            
        //}
        if (mInputAction != null)
        {
            RebindInputManager.LoadBindingOverride(mInputAction);
            GetBindingInfo();            
            UpdateUI();
        }



        RebindInputManager.rebindComplete += UpdateUI;
        RebindInputManager.rebindCanceled += UpdateUI;
    }

    private void OnDisable()
    {
        RebindInputManager.rebindComplete -= UpdateUI;
        RebindInputManager.rebindCanceled -= UpdateUI;
    }

    private void OnValidate()
    {
        if (mInputAction == null)
            return;

        GetBindingInfo();
        UpdateUI();
    }

    private void GetBindingInfo()
    {
        //if (mInputAction.action != null)
        //    mActionName = mInputAction.action.name;

        mActionName = m_ActionText.text;

        if (mInputAction.bindings.Count > selectedBinding)
        {
            inputBinding = mInputAction.bindings[selectedBinding];
            bindingIndex = selectedBinding;
        }
    }

    private void UpdateUI()
    {
        if (m_ActionText != null)
            m_ActionText.text = mActionName;

        if (m_RebindText != null)
        {
            string text;
            if (Application.isPlaying)
            {
                text = RebindInputManager.GetBindingName(mInputAction, bindingIndex);
            }
            else
            {
                text = mInputAction.GetBindingDisplayString(bindingIndex);
            }
            m_RebindText.text = text.Split(' ')[1];
        }
    }

    private void DoRebind()
    {
        //RebindInputManager.StartRebind(mActionName, bindingIndex, m_RebindText, m_ExcludeMouse);
        RebindInputManager.StartRebind(mInputAction, bindingIndex, m_RebindText, m_ExcludeMouse);
    }

    private void ResetBinding()
    {
        RebindInputManager.ResetBinding(mActionName, bindingIndex);
        UpdateUI();
    }
}
