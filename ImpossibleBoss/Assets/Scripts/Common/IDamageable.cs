using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct STakeDamageParams
{
    [HideInInspector]
    public GameObject causedBy;
    public float damageAmount;
    [HideInInspector]
    public float DamageAmplificationRate;
    public ECC cc;
    public float ccAmount;
    [HideInInspector]
    public List<string> tag;
}

public interface IDamageable
{
    void TakeDamage(STakeDamageParams takeDamageParams);
}
