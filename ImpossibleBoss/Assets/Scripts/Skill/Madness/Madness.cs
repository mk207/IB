using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Madness", menuName = "Skills/Madness")]
public class Madness : Skill, IPassive
{
    [SerializeField]
    private byte m_MaxStack;
    [SerializeField]
    private float m_StackScale;
    [SerializeField]
    private float m_PassiveCoolTime;
    [SerializeField, Tooltip("다른 시전자와 중복가능하다 (흑마 도트스킬 같은것들)  불가능하다(법사 지능버프 같은것들)")]
    private bool m_b_CanDuplicate;
    [SerializeField, Tooltip("받는 피해 (%)")]
    private float m_TakeDamage;
    [SerializeField, Tooltip("주는 피해 (%)")]
    private float m_DamageAmplification;

    private StatBuffInfo m_StatBuffInfo;

    public byte MaxStack { get { return m_MaxStack; } set { m_MaxStack = value; } }

    public float StackScale { get { return m_StackScale; } set { m_StackScale = value; } }
    public bool CanDuplicate { get { return m_b_CanDuplicate; } set { m_b_CanDuplicate = value; } }
    public float PassiveCoolTime { get { return m_PassiveCoolTime; } set { m_PassiveCoolTime = value; } }
    public StatBuffInfo StatBuffInfo { get { return m_StatBuffInfo; } set { m_StatBuffInfo = value; } }
    private float TakeDamage { get { return m_TakeDamage; } }
    private float DamageAmplification { get { return m_DamageAmplification; } }


    //public override void Cast(LivingEntity castedBy)
    //{
    //    //BuffManager.Instance.RegisterPassive(castedBy, castedBy, this);
    //}

    //public override void Casting()
    //{

    //}

    public Sprite GetIcon()
    {
        return Icon;
    }

    public void ApplyPassive(BuffInfo buff)
    {
        StatBuffInfo.BuffInfo = buff;
        StatBuffInfo.IsRegister = true;
        StatBuffInfo.BuffInfo.Rewrite.Invoke();
        buff.SetStatBuffInfo(StatBuffInfo);
        buff.Target.ApplyStatChanged(StatBuffInfo);
    }

    public void RemovePassive(BuffInfo buff)
    {
        StatBuffInfo.BuffInfo = buff;
        StatBuffInfo.IsRegister = false;
        buff.Target.ApplyStatChanged(StatBuffInfo);
    }

    public void RegisterPassive(LivingEntity castedBy, LivingEntity target)
    {
        BuffManager.Instance.RegisterPassive(castedBy, target, this);
    }

    public override void Init()
    {
        Name = "name_Madness";
        Description = "desc_Madness";

        StatBuffInfo = new StatBuffInfo(this);
        StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(TakeDamage, EAmountType.Percent, EBuffFactorType.DamageResistance));
        StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(DamageAmplification, EAmountType.Percent, EBuffFactorType.DamageAmplification));
    }

    //private Madness() : base(0.0f, 0, "Madness", "입히는 데미지 <color=#00FFFD>20</color>% 만큼 증가, 받는데미지 <color=#00FFFD>20</color>%만큼 증가",
    //    Resources.Load<Sprite>("Skill/Image/SKL_Madness"), true, EType.Buff, 0, 0, false)
    //{
    //    StatBuffInfo = new StatBuffInfo(this);
    //    StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Percent, EBuffFactorType.DamageResistance));
    //    StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Percent, EBuffFactorType.DamageAmplification));

    //    MaxStack = 1;
    //    StackScale = 1f;
    //    DurationTime = -1.0f;
    //    PassiveCoolTime = -1.0f;
    //    CanDuplicate = false;
    //    mTag = new List<string>(3);
    //    Tag.Add("buff");
    //    Tag.Add("debuff");
    //    Tag.Add("passive");
    //}
}
