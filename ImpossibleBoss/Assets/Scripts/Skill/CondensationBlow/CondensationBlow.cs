using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CondensationBlow", menuName = "Skills/CondensationBlow")]
public class CondensationBlow : Skill, IDash, IBuff, ICast, ILazyCast, IBox
{
    [SerializeField, Tooltip("∆¯")]
    private float m_Width;
    [SerializeField, Tooltip("ºº∑Œ")]
    private float m_Height;

    Collider[] mColliders;

    [SerializeField]
    private float m_BuffDurationTime;

    private StatBuffInfo mStatBuffInfo;


    public float Width { get => m_Width; set => m_Width = value; }
    public float Height { get => m_Height; set => m_Height = value; }
    public byte MaxStack { get; set; }

    public float StackScale { get; set; }

    public float BuffDurationTime { get => m_BuffDurationTime; set => m_BuffDurationTime = value; }

    public bool CanDuplicate { get; set; }

    public StatBuffInfo StatBuffInfo { get => mStatBuffInfo; set => mStatBuffInfo = value; }

    [SerializeField]
    private float m_DashDistance;
    //[SerializeField, Tooltip("1 = 1√ ")]
    private float m_DashSecond;
    [SerializeField]
    private float m_Damage;
    [SerializeField]
    private float m_BuffAmount;

    STakeDamageParams mDamageParams;

    [SerializeField]
    private float m_HammerEffectTime;

    private Vector3 mAttackDir;

    public float DashDistance { get { return m_DashDistance; } }
    public float DashSecond { get { return m_DashSecond; } }
    private float Damage { get { return m_Damage; } }
    private float BuffAmount { get { return m_BuffAmount; } }
    private Vector3 AttackDir { get { return mAttackDir; } set { mAttackDir = value; } }
    public void Cast(LivingEntity castedBy)
    {
        //PlayEffect(EAnimState.Cast, castedBy.transform.position, castedBy.transform.rotation);
    }

    public void LazyCast(LivingEntity castedBy)
    {
        for (int index = 0; index < 10; index++)
        {
            mColliders[index] = null;
        }

        Vector3 half = new Vector3(Width / 2.0f, 1.0f, Height / 2.0f);
        Vector3 center = AttackDir * 0.5f + AttackDir * Height * 0.5f + castedBy.transform.position;
        Physics.OverlapBoxNonAlloc(center, half, mColliders, Quaternion.LookRotation(AttackDir, castedBy.transform.up), Mask);
        Logger.Log($"c : {center}");
        for (int index = 0; index < mColliders.Length; index++)
        {
            if (mColliders[index] != null)
            {
                STakeDamageParams damageParams = mDamageParams;
                damageParams.causedBy = castedBy.gameObject;
                damageParams.damageAmount = castedBy.GetDamageAmount(damageParams.damageAmount);
                mColliders[index].GetComponent<LivingEntity>().TakeDamage(damageParams);
                RegisterBuff(castedBy, castedBy);
            }
            else
            {
                break;
            }            
        }

        Vector3 pos = new Vector3(castedBy.transform.position.x, castedBy.transform.position.y - 0.8f, castedBy.transform.position.z);
        pos += castedBy.transform.forward * 5.0f;

        PlayEffect(EAnimState.Cast, pos, castedBy.transform.rotation, m_HammerEffectTime);
    }

    //public override void Casting()
    //{
        
    //}

    public Sprite GetIcon()
    {
        return Icon;
    }

    public override void Init()
    {
        Name = "name_CondensationBlow";
        Description = "desc_CondensationBlow";

        m_DashSecond = GetAnimInfo(EAnimState.Dash).AnimTime;

        MaxStack = 1;
        StackScale = 1;
        CanDuplicate = false;

        mDamageParams.damageAmount = Damage;
        mDamageParams.tag = Tag;

        StatBuffInfo = new StatBuffInfo(this);
        StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(BuffAmount, EAmountType.Percent, EBuffFactorType.Damage));

        mColliders = new Collider[10];
    }

    public void ApplyBuff(BuffInfo buff)
    {
        StatBuffInfo.BuffInfo = buff;
        StatBuffInfo.IsRegister = true;
        StatBuffInfo.BuffInfo.Rewrite.Invoke();
        buff.SetStatBuffInfo(StatBuffInfo);
        buff.Target.ApplyStatChanged(StatBuffInfo);
    }

    public void RegisterBuff(LivingEntity castedBy, LivingEntity target)
    {
        BuffManager.Instance.RegisterBuff(castedBy, target, this);
    }

    public void RemoveBuff(BuffInfo buff)
    {
        StatBuffInfo.BuffInfo = buff;
        StatBuffInfo.IsRegister = false;
        buff.Target.ApplyStatChanged(StatBuffInfo);
    }

    public void Dash(LivingEntity castedBy)
    {
        RaycastHit hit;
        Ray ray = castedBy.GetComponent<PlayerMovement>().m_MainCamera.ScreenPointToRay(castedBy.GetComponent<PlayerInputManager>().MousePos);
        Physics.Raycast(ray, out hit);
        Vector3 currPos = castedBy.transform.position;
        Vector3 mousePos = new Vector3(hit.point.x, castedBy.transform.position.y, hit.point.z);
        AttackDir = (mousePos - currPos).normalized;
        Vector3 targetVec = AttackDir * DashDistance;
        
        castedBy.GetComponent<PlayerMovement>().StartDash(targetVec, DashSecond);
        PlayEffect(EAnimState.Dash, castedBy.transform, m_HammerEffectTime, new Vector3(1.0f, 0.0f, 0.0f), 0.7f);
    }

    public void FinishDash()
    {
        throw new System.NotImplementedException();
    }
}
