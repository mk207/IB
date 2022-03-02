using System;

public class AnimationEventArgs : EventArgs
{
    public bool IsAnimating { get; set; }
    public AnimationEventArgs(bool isAnimating)
    {
        IsAnimating = isAnimating;
    }
}
