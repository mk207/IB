using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessDeadZone : DeadZone, IObjectPool<ChessDeadZone>
{
    //[SerializeField]
    //private ParticleSystem m_Spikes;
    [SerializeField]
    private ParticleSystemRenderer m_Renderer;

    private static Vector2 mPlayerPos;

    private static MaterialPropertyBlock mBlackMPB;
    private static MaterialPropertyBlock mWhiteMPB;

    private bool mbCanAttack;

    //private ParticleSystem Spikes { get { return m_Spikes; } }
    public ChessDeadZone Next { get; set; }
    public Action<ChessDeadZone> ReturnObjectDelegate { get; set; }
    public Vector2 PlayerPos { get { return mPlayerPos; } set { mPlayerPos = value; } }

    private MaterialPropertyBlock BlackMPB { get { return mBlackMPB; } }
    private MaterialPropertyBlock WhiteMPB { get { return mWhiteMPB; } }
    private ParticleSystemRenderer Renderer { get { return m_Renderer; } }

    private bool CanAttack { get { return mbCanAttack; } set { mbCanAttack = value; } }

    private void Awake()
    {
        if (mBlackMPB == null)
        {
            mBlackMPB = new MaterialPropertyBlock();
            mBlackMPB.SetColor("_FrontFacesColor", new Color(0.5764706f, 0.5764706f, 0.5764706f));
        }
        if (mWhiteMPB == null)
        {
            mWhiteMPB = new MaterialPropertyBlock();
            mWhiteMPB.SetColor("_FrontFacesColor", new Color(0.0f, 0.0f, 0.0f));
        }

        //mRenderer = Spikes.GetComponent<ParticleSystemRenderer>();
    }

    public static void CheckPlayerPosotion(Dictionary<PlayerInformation, Vector3> playerPos, List<QuizDeadZone> deadZone)
    {
        foreach (var player in playerPos)
        {
            int row = Mathf.FloorToInt((player.Value.z + 2.0f) * 0.25f);
            int col = Mathf.FloorToInt((player.Value.x + 2.0f) * 0.25f);
            if (deadZone[row * 8 + col].IsCurrDeadZone && player.Key.IsDead == false)
            {
                player.Key.InstantDeath(true);
            }
        }
    }

    private void OnEnable()
    {
        CanAttack = false;
        Invoke("ReadyToAttack", 2.0f);
    }

    private void ReadyToAttack()
    {
        CanAttack = true;
    }

    public void SetColor()
    {
        Vector3 pos = transform.position;
        int row = (int)((pos.z + 2.0f) / 4.0f);
        int col = (int)((pos.x + 2.0f) / 4.0f);

        //ParticleSystem.MainModule main = Spikes.main;

        // È¦È¦ ¶Ç´Â Â¦Â¦ ÀÌ¸é Èò¹Ù´Ú
        if ((row % 2 == 0 && col % 2 == 0) || (row % 2 == 1 && col % 2 == 1))
        {
            Renderer.SetPropertyBlock(WhiteMPB);
        }
        else
        {
            Renderer.SetPropertyBlock(BlackMPB);
        }

    }
}
