using System;

public enum ECC
{
    None,
    KnockBack,
    Stun,
    ElectricShock
}

public class CCEventArgs : EventArgs
{
    public ECC CC { get; set; }

    public CCEventArgs(ECC cc)
    {
        CC = cc;
    }
}
