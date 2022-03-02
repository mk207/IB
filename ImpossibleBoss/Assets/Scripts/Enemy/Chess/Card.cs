using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour, IObjectPool<Card>
{
    private static float mSpeed = 0.05f;
    private static float mDamage;
    private static float mKnockBackSecond;
    private static float mRange = 10.0f;
    private static GameObject mCausedBy;
    private float mElapsedTime;
    private Vector3 mOriginPos;
    private Vector3 mDir;
    
    static private STakeDamageParams mDamageParams;

    public GameObject CausedBy { get { return mCausedBy; } set { mCausedBy = value; } }    
    public float ElapsedTime { get { return mElapsedTime; } set { mElapsedTime = value; } }
    public float Range { private get { return mRange; } set { mRange = value; } }
    public Vector3 OriginPos { get { return mOriginPos; } set { mOriginPos = value; } }
    public Vector3 Dir { get { return mDir; } set { mDir = value; } }
    private STakeDamageParams DamageParams { get { return mDamageParams; } }
    public float Damage { private get { return mDamage; } set { mDamage = value; } }
    public float Speed { private get { return mSpeed; } set { mSpeed = value; } }
    public float KnockBackSecond { private get { return mKnockBackSecond; } set { mKnockBackSecond = value; } }

    public Card Next { get; set; }
    public Action<Card> ReturnObjectDelegate { get; set; }

    private void OnEnable()
    {
        ElapsedTime = 0.0f;
    }

    public void ReturnObject()
    {
        gameObject.SetActive(false);
        ReturnObjectDelegate(this);
    }

    private void Awake()
    {
        mDamageParams.causedBy = CausedBy;
        mDamageParams.damageAmount = Damage;
        //mDamageParams.cc = ECC.KnockBack;
        //mDamageParams.ccAmount = KnockBackSecond;
    }
    //private void Start()
    //{
    //    Destroy(gameObject, 20.0f);
    //}


    private void Update()
    {
        //transform.position = (transform.position + Dir * Speed);
        transform.position = Vector3.Lerp(OriginPos, OriginPos + Dir * Range, ElapsedTime );
        if (ElapsedTime <= 1.0f)
        {
            ElapsedTime = ElapsedTime + Time.deltaTime * Speed;
        }
        else
        {
            ReturnObject();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<PlayerInformation>().TakeDamage(DamageParams);
        }
    }
}
