using System;

public class DashEventArgs : EventArgs 
{
    public float DashSecond { get; set; }
    public bool IsDashing { get; set; }

    public DashEventArgs(float second, bool isDashing)
    {
        DashSecond = second;
        IsDashing = isDashing;
    }
}
    

