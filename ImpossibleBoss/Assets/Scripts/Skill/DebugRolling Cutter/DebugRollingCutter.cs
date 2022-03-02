using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugRollingCutter", menuName = "Skills/DebugRollingCutter")]
public class DebugRollingCutter : Skill, ICast, ISector
{
    [SerializeField]
    private float m_Radius;
    [SerializeField]
    private float m_Angle;
    [SerializeField, Tooltip("끝사거리 범위 (사거리3에 끝사거리 0.1이면 2.9~3이 끝사거리 범위)")]
    private float m_CriticalZoneSize;
    [SerializeField]
    private float m_Damage;
    [SerializeField, Tooltip("끝사거리 데미지 배율")]
    private float m_CritDamage;
    private Collider[] mColliders;

    private float EFFECT_RADIUS = 2.5f;

    private STakeDamageParams mTakeDamage;
    private STakeDamageParams mTakeCritDamage;

    public float Radius => m_Radius;

    public float Angle { get { return m_Angle; } }

    private float CriticalZoneSize { get { return m_CriticalZoneSize; } }
    private float Damage { get { return m_Damage; } }
    private float CritDamage { get { return m_CritDamage; } }
    private STakeDamageParams TakeDamage { get { return mTakeDamage; } }
    private STakeDamageParams TakeCritDamage { get { return mTakeCritDamage; } }
    

    public /*override*/ void Cast(LivingEntity castedBy)
    {
        for (int index = 0; index < 10; index++)
        {
            mColliders[index] = null;
        }

        float cosAngle = Mathf.Cos(Mathf.Deg2Rad * (Angle / 2.0f) );
        //           Boss      Enemy
        int mask = (1 << 8) | (1 << 9);
        Physics.OverlapSphereNonAlloc(castedBy.transform.position, Radius, mColliders, mask);                

        for (int index = 0; index < mColliders.Length; index++)
        {
            if(mColliders[index] != null && Vector3.Dot(castedBy.transform.forward, (mColliders[index].transform.position - castedBy.transform.position).normalized) >= cosAngle)
            {
                STakeDamageParams damageParams = mTakeDamage;
                damageParams.causedBy = castedBy.gameObject;
                damageParams.damageAmount = castedBy.GetDamageAmount(damageParams.damageAmount);
                mColliders[index].GetComponent<LivingEntity>().TakeDamage(damageParams);
            }
        }
        
        PlayEffect(EAnimState.Cast, castedBy.transform.position, castedBy.transform.rotation * Quaternion.Euler(0.0f, -60.0f, 0.0f));
    }

    //public override void Casting()
    //{
        
    //}

    public override void Init()
    {
        Name = "name_DebugRollingCutter";
        Description = "desc_DebugRollingCutter";
        
        mTakeDamage.damageAmount = Damage;
        mTakeCritDamage.damageAmount = Damage * CritDamage;
        mColliders = new Collider[10];

        var info = GetAnimInfo(EAnimState.Cast);
        var main = info.Effect.GetComponent<ParticleSystem>().main;
        main.startSize = Radius / EFFECT_RADIUS;

        var transforms = info.Effect.transform.GetComponentsInChildren<Transform>();

        foreach (var transform in transforms)
        {
            if(transform.gameObject.name == "Hit")
            {
                transform.localPosition = new Vector3(0.84770038f, 0.0f, 0.53047532f) * Radius;
                break;
            }
        }       
    }
}
