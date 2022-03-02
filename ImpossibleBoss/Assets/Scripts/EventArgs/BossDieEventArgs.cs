using System;

public class BossDieEventArgs : EventArgs
{
    public ushort UnlockID { get; set; }
    public BossDieEventArgs(ushort unlockID)
    {
        UnlockID = unlockID;
    }
}
