using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private Color mColor = Color.white;
    private Material mMaterial;
    [Range(0,1)]
    public float Progress = 0.0f;
    [Range(10, 100)]
    public float BlinkSpeed = 50.0f;

    private void Start()
    {
        mMaterial = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        float emission = Mathf.PingPong(Time.time * BlinkSpeed * Mathf.Pow(Progress, 2.0f), 10) + 1;

        //GetComponent<MeshRenderer>().material.SetColor("_Color", color * Mathf.LinearToGammaSpace(emission));
        mMaterial.SetColor("_Color", mColor * emission);
        Progress += Time.deltaTime * 0.1f;
        Progress = Mathf.Clamp01(Progress);
    }
}
