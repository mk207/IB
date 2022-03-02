using System;

public class GamePauseEventArgs : EventArgs
{
    public bool IsPaused { get; set; }
    public GamePauseEventArgs(bool isPaused)
    {
        IsPaused = isPaused;
    }
}
