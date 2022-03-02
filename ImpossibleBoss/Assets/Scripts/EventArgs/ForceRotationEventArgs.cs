using System;
using UnityEngine;

public class ForceRotationEventArgs : EventArgs
{
    public Vector3 Dir { get; set; }
    public bool IsForceRot { get; set; }
    public ForceRotationEventArgs(Vector3 dir, bool isForceRot)
    {
        Dir = dir;
        IsForceRot = isForceRot;
    }
}
