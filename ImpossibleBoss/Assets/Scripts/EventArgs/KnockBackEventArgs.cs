using System;
using UnityEngine;

public class KnockBackEventArgs : EventArgs
{
    public KnockBackEventArgs(Vector3 dir, float power, bool isKnockBack)
    {
        KnockBackDir = dir;
        KnockBackPower = power;
        IsKnockBack = isKnockBack;
    }
    
    public Vector3 KnockBackDir { get; set; } 
    public float KnockBackPower { get; set; }
    public bool IsKnockBack { get; set; }
}
