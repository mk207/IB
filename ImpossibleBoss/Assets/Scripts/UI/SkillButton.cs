using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject InfoPanel;
    [SerializeField]
    private Text InfoText;
    private string mDesc;

    public string Desc { get { return mDesc; } set { mDesc = value; } }
    public void OnPointerEnter(PointerEventData eventData)
    {
        InfoPanel.SetActive(true);
        WriteSkillInfo();
        ResizeInfoPanel();
        SetPanelPos();
        
        //Logger.Log("Enter");
    }

    public void SetIcon(Sprite sprite)
    {
        GetComponent<Image>().sprite = sprite;
    }

    private void WriteSkillInfo()
    {
        //InfoText.text = Desc;
        GetComponent<Localize>().SetID(Desc);
        InfoText.fontSize = 30;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InfoPanel.SetActive(false);
        
        //Logger.Log("Exit");
    }
    private void ResizeInfoPanel()
    {
        InfoPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(InfoText.preferredWidth, InfoText.preferredHeight);        
    }
    private void SetPanelPos()
    {
        InfoPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.0f);
        InfoPanel.transform.localPosition = new Vector3(-170.0f, -350.0f);
    }
}
