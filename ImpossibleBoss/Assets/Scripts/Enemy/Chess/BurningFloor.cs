using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningFloor : MonoBehaviour, IObjectPool<BurningFloor>
{
    private static float mDamage;
    private GameObject mCausedBy;
    private static float mInterval;
    private static float mDurationTime;
    private float mEndTime;
    [SerializeField]
    private GameObject m_Effect;
    private static Dictionary<PlayerInformation, float> mNextDamageTime;
    private bool mbCanSummonChess;

    public static EDifficulty m_Difficulty;
    public static Action<byte> mSummonChessDelegate;
    private static byte mSummonChessCount;

    public float Damage { get { return mDamage; } set { mDamage = value; } }
    public GameObject CausedBy { get { return mCausedBy; } set { mCausedBy = value; } }
    public float Interval { get { return mInterval; } set { mInterval = value; } }
    public float DurationTime { get { return mDurationTime; } set { mDurationTime = value; } }    
    public Dictionary<PlayerInformation, float> NextDamageTime { get { return mNextDamageTime; } set { mNextDamageTime = value; } }
    public BurningFloor Next { get; set; }
    public Action<BurningFloor> ReturnObjectDelegate { get; set; }
    public byte SummonChessCount { get { return mSummonChessCount; } set { mSummonChessCount = value; } }
    private bool CanSummonChess { get { return mbCanSummonChess; } set { mbCanSummonChess = value; } }
    private float EndTime { get { return mEndTime; } set { mEndTime = value; } }

    public GameObject Effect { private get { return m_Effect; } set { m_Effect = value; } }

    private void OnEnable()
    {
        CanSummonChess = true;
        //Effect.SetActive(true);
    }

    private void Awake()
    {
        mNextDamageTime = new Dictionary<PlayerInformation, float>(4);
        
        //Effect.SetActive(false);
    }
    
    public void SetTimer()
    {
        EndTime = Time.time + DurationTime;
    }

    private void Update()
    {
        if (EndTime < Time.time)
        {
            ReturnObject();
        }
    }

    public void ReturnObject()
    {
        gameObject.SetActive(false);
        ReturnObjectDelegate(this);
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    STakeDamageParams damageParams = new STakeDamageParams();
    //    damageParams.causedBy = gameObject;
    //    damageParams.damageAmount = Damage;

    //    PlayerInformation playerInformation = other.GetComponent<PlayerInformation>();
    //    float time = 0.0f;
    //    if (playerInformation != null && !NextDamageTime.TryGetValue(playerInformation, out time) || time < Time.time)
    //    {
    //        playerInformation.TakeDamage(damageParams);
    //        NextDamageTime[playerInformation] = Time.time + Interval;
    //    }
    //}
    private void OnTriggerStay(Collider other)
    {
        STakeDamageParams damageParams = new STakeDamageParams();
        damageParams.causedBy = CausedBy;
        damageParams.damageAmount = Damage;

        PlayerInformation playerInformation = other.GetComponent<PlayerInformation>();
        float time = 0.0f;
        bool isPlayer = playerInformation != null;

        if (isPlayer && (NextDamageTime.TryGetValue(playerInformation, out time) == false || time < Time.time) )
        {
            playerInformation.TakeDamage(damageParams);
            NextDamageTime[playerInformation] = Time.time + Interval;
            if (CanSummonChess)
            {
                CanSummonChess = false;
                if (m_Difficulty == EDifficulty.Hard)
                {
                    mSummonChessDelegate(SummonChessCount);
                }
            }
        }
    }
}
