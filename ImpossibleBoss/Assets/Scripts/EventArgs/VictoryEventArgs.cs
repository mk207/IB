using System;
using System.Collections.Generic;

public class VictoryEventArgs : EventArgs
{
    public float Time { get; set; }

    public VictoryEventArgs(float time)
    {
        Time = time;
    }
}
