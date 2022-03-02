using System.Collections;
using UnityEngine;
using System;

public class MoveStateChangeArgs : EventArgs
{
    public MoveStateChangeArgs(PlayerInformation player, Vector3 playerpos, bool isMoving)
    {
        Player = player;
        PlayerPos = playerpos;
        IsMoving = isMoving;
    }
    public PlayerInformation Player { get; set; }
    public Vector3 PlayerPos { get; set; }
    public bool IsMoving { get; set; }
}
