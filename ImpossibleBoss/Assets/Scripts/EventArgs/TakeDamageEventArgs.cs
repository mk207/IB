using System;

public class TakeDamageEventArgs : EventArgs
{
    public STakeDamageParams TakeDamageParams { get; set; }
    public TakeDamageEventArgs(STakeDamageParams param)
    {
        TakeDamageParams = param;
    }
}
