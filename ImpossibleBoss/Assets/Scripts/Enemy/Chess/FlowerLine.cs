using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerLine : MonoBehaviour, IObjectPool<FlowerLine>
{
    //public string testName;

    private ChessBoss mBoss;
    private static float mDamage;
    private static float mDamageInterval;
    private static STakeDamageParams mDamageParams;
    private static GameObject mCausedBy;
    private static float mEffectTiming;
    private float mStartTime;
    //private LineRenderer mLineRender;
    private Dictionary<GameObject, float> mIgnorePlayer;
    private int mLayerMask;
    private Vector3[] mPos;
    private bool mbCanAttack = default;

    [SerializeField]
    private ParticleSystem m_LaserCore;
    [SerializeField]
    private ParticleSystem m_LaserTrails;
    [SerializeField]
    private ParticleSystem m_GuideTrails;
    private const float LASER_DISTANCE = 22.0f;
    private Vector3 mDistance;
    public ChessBoss Boss { get { return mBoss; } set { mBoss = value; } }
    public GameObject CausedBy { get { return mCausedBy; } set { mCausedBy = value; } }
    public float EffectTiming { get { return mEffectTiming; } set { mEffectTiming = value; } }
    public float DamageInterval { private get { return mDamageInterval; } set { mDamageInterval = value; } }    
    private int LayerMask { get { return mLayerMask; } set { mLayerMask = value; } }
    private float StartTime { get { return mStartTime; } set { mStartTime = value; } }
    public float Damage { private get { return mDamage; } set { mDamage = value; } }
    //private LineRenderer LineRender { get { return mLineRender; } }
    private STakeDamageParams DamageParams { get { return mDamageParams; } }
    private Dictionary<GameObject, float> IgnorePlayer { get { return mIgnorePlayer; } }
    private ParticleSystem LaserCore { get { return m_LaserCore; } }
    private ParticleSystem LaserTrails { get { return m_LaserTrails; } }
    private ParticleSystem GuideTrails { get { return m_GuideTrails; } }
    private Vector3 Distance { get { return mDistance; } }
    public FlowerLine Next { get; set; }
    public Action<FlowerLine> ReturnObjectDelegate { get; set; }
    public bool CanAttack { get { return mbCanAttack; } set { mbCanAttack = value; } }
   
    public void SetPos(Vector3[] newPos)
    {
        var laserCore = LaserCore.main;
        var laserTrails = LaserTrails.main;
        var guideTrails = GuideTrails.main;
        mPos[0] = newPos[0];
        mPos[1] = newPos[1];
        transform.position = mPos[0];
        transform.LookAt(mPos[1]);

        laserCore.startSizeZMultiplier = (mPos[0] - mPos[1]).magnitude / LASER_DISTANCE;
        laserTrails.startSizeMultiplier = (mPos[0] - mPos[1]).magnitude / LASER_DISTANCE;
        guideTrails.startSizeZMultiplier = (mPos[0] - mPos[1]).magnitude / LASER_DISTANCE;
    }

    private bool IsIgnoringPlayer(GameObject info)
    {
        for (int index = 0; index < 4; index++)
        {
            //if (mIgnorePlayer[index] == info)
            //{
            //    return true;
            //}
        }
        return false;
    }

    private void Awake()
    {
        mDamageParams.causedBy = CausedBy;
        mDamageParams.damageAmount = Damage;
        mDamageParams.cc = ECC.None;

        mIgnorePlayer = new Dictionary<GameObject, float>(4);
        //mLineRender = GetComponent<LineRenderer>();
        LayerMask = 1 << 7;

        mPos = new Vector3[2];        
    }

    private void OnEnable()
    {
        StartTime = Time.time;
    }

    private void OnDisable()
    {
        CanAttack = false;
    }

    private void FixedUpdate()
    {
        RaycastHit[] raycastHits = new RaycastHit[4];
        //Vector3[] pos = new Vector3[2];
        float time;
        //LineRender.GetPositions(pos);
        //raycastHits = Physics.RaycastAll(mPos[0], (mPos[1] - mPos[0]).normalized, (mPos[1] - mPos[0]).magnitude, LayerMask, QueryTriggerInteraction.Collide);
        Vector3 half = new Vector3(0.4f, 0.5f, (mPos[1] - mPos[0]).magnitude);
        Physics.BoxCastNonAlloc(mPos[0], half, (mPos[1] - mPos[0]).normalized, raycastHits, Quaternion.LookRotation((mPos[1] - mPos[0]).normalized));
        //ExtDebug.DrawBoxCastBox(mPos[0], half, Quaternion.LookRotation((mPos[1] - mPos[0]).normalized), (mPos[1] - mPos[0]).normalized, (mPos[1] - mPos[0]).magnitude, Color.red);
        //Logger.Log($"{testName} : {mPos[0]} // {mPos[1]}");

        if (raycastHits.Length > 0 /*&& (Time.time - StartTime) > EffectTiming*/ && CanAttack)
        {
            for (int index = 0; index < raycastHits.Length; index++)
            {
                if (raycastHits[index].transform != null && raycastHits[index].transform.name.Equals("Player"))
                {
                    if (IgnorePlayer.TryGetValue(raycastHits[index].collider.gameObject, out time))
                    {
                        if (time < Time.time)
                        {
                            raycastHits[index].collider.gameObject.GetComponent<PlayerInformation>().TakeDamage(DamageParams);
                            IgnorePlayer[raycastHits[index].collider.gameObject] = Time.time + DamageInterval;
                        }
                    }
                    else
                    {
                        raycastHits[index].collider.gameObject.GetComponent<PlayerInformation>().TakeDamage(DamageParams);
                        IgnorePlayer.Add(raycastHits[index].collider.gameObject, Time.time + DamageInterval);
                    }
                }
                
            }
        }

    }

}
