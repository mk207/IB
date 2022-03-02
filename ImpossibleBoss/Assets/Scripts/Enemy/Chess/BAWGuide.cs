using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BAWGuide : MonoBehaviour, IObjectPool<BAWGuide>
{
    private static float SIZE = 6.25f;
    private Transform mTransform;
    private RectTransform mRectTransform;

    public BAWGuide Next { get; set; }
    public Action<BAWGuide> ReturnObjectDelegate { get; set; }

    private void Awake()
    {
        mTransform = gameObject.transform;
        mRectTransform = GetComponent<RectTransform>();
    }

    public void SetPosition(Vector3 newPos)
    {
        mTransform.position = newPos;
    }

    public void SetSize(float rate)
    {
        mRectTransform.sizeDelta = new Vector2(SIZE * rate, SIZE * rate);
    }

    private void OnDisable()
    {
        mRectTransform.sizeDelta = Vector2.zero;
    }
}
