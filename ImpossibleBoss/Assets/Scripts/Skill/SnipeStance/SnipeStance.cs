using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SnipeStance", menuName = "Skills/SnipeStance")]
public class SnipeStance : Skill, IPassive
{
    [SerializeField]
    private byte m_MaxStack;
    [SerializeField]
    private float m_StackScale;
    [SerializeField]
    private float m_PassiveCoolTime;
    [SerializeField, Tooltip("�ٸ� �����ڿ� �ߺ������ϴ� (�渶 ��Ʈ��ų �����͵�)  �Ұ����ϴ�(���� ���ɹ��� �����͵�)")]
    private bool m_b_CanDuplicate;
    [SerializeField]
    private StatBuffInfo m_StatBuffInfo;

    public byte MaxStack { get { return m_MaxStack; } set { m_MaxStack = value; } }
    public float StackScale { get { return m_StackScale; } set { m_StackScale = value; } }
    public bool CanDuplicate { get { return m_b_CanDuplicate; } set { m_b_CanDuplicate = value; } }
    public StatBuffInfo StatBuffInfo { get { return m_StatBuffInfo; } set { m_StatBuffInfo = value; } }
    public float PassiveCoolTime { get { return m_PassiveCoolTime; } set { m_PassiveCoolTime = value; } }

    //public static ChargeStance Instance()
    //{
    //    if (mInstance != null)
    //    {
    //        return mInstance;
    //    }
    //    else
    //    {
    //        return mInstance = new ChargeStance();
    //    }
    //}

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
        Name = "name_SnipeStance";
        Description = "desc_SnipeStance";

        StatBuffInfo = new StatBuffInfo(this);
        StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(10, EAmountType.Percent, EBuffFactorType.DamageAmplification));
        StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(-10, EAmountType.Percent, EBuffFactorType.DamageResistance));
        StatBuffInfo.AddBanTag("melee");
    }
}
