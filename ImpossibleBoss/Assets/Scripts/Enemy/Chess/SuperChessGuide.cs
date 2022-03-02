using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuperChessGuide : MonoBehaviour
{
    public static bool ShouldShowGuide { get; set; }
    [SerializeField]
    private Image m_ArrowImage;
    private Transform CamTransform { get; set; }
    private Transform Transform { get; set; }
    private Image ArrowImage { get { return m_ArrowImage; } }
    private float LerpAlpha { get; set; }

    private bool IsTurnOn { get; set; } = default;

    public float Row { get; set; }
    public float Col { get; set; }

    private static float Height()
    {
        return 2.5f + Mathf.Abs(Mathf.Sin(Time.time));
    }

    void Awake()
    {
        if (ShouldShowGuide == false)
        {
            enabled = false;
        }

        if (CamTransform == null)
        {
            CamTransform = FindObjectOfType<Camera>().transform;
        }
        Transform = transform;
    }

    public void TurnOn()
    {
        if (ShouldShowGuide)
        {
            gameObject.SetActive(true);
            IsTurnOn = true;
            LerpAlpha = 0.0f;
        }        
    }

    public void TurnOff()
    {
        if (ShouldShowGuide)
        {
            IsTurnOn = false;
        }        
    }

    private void Update()
    {        
        if (IsTurnOn && LerpAlpha < 1.0f)
        {
            LerpAlpha = Mathf.Clamp01(LerpAlpha + Time.deltaTime);
            ArrowImage.color = new Color(1.0f, 0.0f, 0.0f, Mathf.Lerp(0.0f, 0.41176471f, LerpAlpha));
        }

        if (IsTurnOn == false)
        {
            LerpAlpha = Mathf.Clamp01(LerpAlpha - Time.deltaTime);
            ArrowImage.color = new Color(1.0f, 0.0f, 0.0f, Mathf.Lerp(0.0f, 0.41176471f, LerpAlpha));
            if (LerpAlpha <= 0.0f)
            {
                gameObject.SetActive(false);
            }            
        }

        //Transform.LookAt(CamTransform);
        Transform.position = new Vector3(Col, Height(), Row);
    }
}
