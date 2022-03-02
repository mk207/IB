using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject InfoPanel;
    public Text InfoText;
    private StatBuffInfo mStatBuffInfo;
    public StatBuffInfo StatBuffInfo { get { return mStatBuffInfo; } set { mStatBuffInfo = value; } }

    public void ResizeInfoPanel()
    {
        InfoPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(InfoText.preferredWidth, InfoText.preferredHeight);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InfoPanel.SetActive(true);
        InfoText.text = StatBuffInfo.BuffText;
        ResizeInfoPanel();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InfoPanel.SetActive(false);
    }

    private void Start()
    {
        InfoPanel = GameObject.Find("HUD Canvas").transform.Find("InformationPanel").gameObject;
        InfoText = InfoPanel.GetComponentInChildren<Text>();
    }
}
