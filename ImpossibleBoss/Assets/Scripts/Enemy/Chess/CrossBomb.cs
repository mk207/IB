using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossBomb : MonoBehaviour, IObjectPool<CrossBomb>
{
    //[SerializeField]
    //private GameObject mBurningFloorPrefab;    
    private GameObject mCausedBy;
    //private ObjectPool<BurningFloor> mBurningFloorPool;
    private float mBoomTime;
    private float mBoomTimeFactor;
    private int mRow;
    private int mCol;
    private static float mBombTimer;    
    private static float mBombDamage;
    private GameObject mEffect;

    private static float BOXSIZE = 8.0f;
    private static float HEIGHT = 16.0f;

    [SerializeField]
    private Slider m_NorthGuides;
    [SerializeField]
    private Slider m_SouthGuides;
    [SerializeField]
    private Slider m_EastGuides;
    [SerializeField]
    private Slider m_WestGuides;

    private RectTransform mNorthTransform;
    private RectTransform mSouthTransform;
    private RectTransform mEastTransform;
    private RectTransform mWestTransform;

    public static bool ShouldGuide { get; set; }

    public static Action<int, int> Boom;
    //private static float mFloorDamage;
    //private static float mInterval;
    //private static float mDurationTime;
    //private static Transform mBoardTransform;

    //public Transform BoardTransform { get { return mBoardTransform; } set { mBoardTransform = value; } }
    public GameObject CausedBy { get { return mCausedBy; } set { mCausedBy = value; } }    
    public float BombTimer { get { return mBombTimer; } set { mBombTimer = value; } }
    public int Row { get { return mRow; } set { mRow = value; } }
    public int Col { get { return mCol; } set { mCol = value; } }
    public float BombDamage { get { return mBombDamage; } set { mBombDamage = value; } }
    //public float FloorDamage { private get { return mFloorDamage; } set { mFloorDamage = value; } }
    //public float Interval { private get { return mInterval; } set{ mInterval = value; } }
    //public float DurationTime { get { return mDurationTime; } set { mDurationTime = value; } }
    //private GameObject BurningFloorPrefab { get { return mBurningFloorPrefab; } }
    //private ObjectPool<BurningFloor> BurningFloorPool { get { return mBurningFloorPool; } }
    public GameObject Effect { get { return mEffect; } set { mEffect = value; mEffect.SetActive(false); } }
    public CrossBomb Next { get; set; }
    public Action<CrossBomb> ReturnObjectDelegate { get; set; }
    private float BoomTime { get { return mBoomTime; } set { mBoomTime = value; } }    

    public void ReturnObject()
    {
        gameObject.SetActive(false);        
        ReturnObjectDelegate(this);
        Invoke("TurnoffEffect", Effect.GetComponent<ParticleSystem>().main.duration);
    }

    private void TurnoffEffect()
    {
        Effect.SetActive(false);
    }

    private void OnEnable()
    {
        mNorthTransform.sizeDelta = new Vector2((8 - Row) * BOXSIZE - 4.0f, HEIGHT);
        mSouthTransform.sizeDelta = new Vector2(Row * BOXSIZE + 4.0f, HEIGHT);
        mEastTransform.sizeDelta = new Vector2((8 - Col) * BOXSIZE - 4.0f, HEIGHT);
        mWestTransform.sizeDelta = new Vector2(Col * BOXSIZE + 4.0f, HEIGHT);
    }

    // Start is called before the first frame update
    void Awake()
    {
        mNorthTransform = m_NorthGuides.GetComponent<RectTransform>();
        mSouthTransform = m_SouthGuides.GetComponent<RectTransform>();
        mEastTransform = m_EastGuides.GetComponent<RectTransform>();
        mWestTransform = m_WestGuides.GetComponent<RectTransform>();
    }

    //private void Boom()
    //{
        
    //}

    public void SetTimer()
    {
        BoomTime = Time.time + BombTimer;
        mBoomTimeFactor = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (BoomTime < Time.time)
        {            
            Boom(Row, Col);
            Effect.transform.position = transform.position;
            Effect.SetActive(true);
            ReturnObject();
        }

        if (ShouldGuide)
        {
            float value = (Time.time - mBoomTimeFactor) / (BoomTime - mBoomTimeFactor);
            m_NorthGuides.value = value;
            m_SouthGuides.value = value;
            m_EastGuides.value = value;
            m_WestGuides.value = value;
        }
        
    }
}
