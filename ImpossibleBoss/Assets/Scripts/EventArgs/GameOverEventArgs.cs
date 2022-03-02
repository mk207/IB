using System;

public class GameOverEventArgs : EventArgs
{
    public float Time { get; private set; }
    public GameOverEventArgs(float time)
    {
        Time = time;
    }
}

