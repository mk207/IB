using System;

public class ForceCancelChangeArgs : EventArgs
{
    public ForceCancelChangeArgs(bool isForceCancel)
    {
        IsForceCancel = isForceCancel;
    }
    public bool IsForceCancel { get; set; }
}
