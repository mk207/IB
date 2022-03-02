using System;

public class ElapsedTimeEventArgs : EventArgs
{
    public float ElapsedTime { get; set; }

    public ElapsedTimeEventArgs(float elapsedTime)
    {
        ElapsedTime = elapsedTime;
    }
}
