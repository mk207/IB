using System;
using UnityEngine;

public class AlarmEventArgs : EventArgs
{
    public AudioClip Clip { get; private set; }
    public AlarmEventArgs(AudioClip clip)
    {
        Clip = clip;
    }
}
