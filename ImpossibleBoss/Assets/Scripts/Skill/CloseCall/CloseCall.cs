using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CloseCall", menuName = "Skills/CloseCall")]
public class CloseCall : Skill, IBuff, IPassive
{
    [SerializeField]
    private byte m_MaxStack;
    [SerializeField]
    private float m_StackScale;
    [SerializeField]
    private float m_BuffDurationTime;
    [SerializeField, Tooltip("영구적인 패시브는 -1")]
    private float m_PassiveCoolTime;
    [SerializeField, Tooltip("다른 시전자와 중복가능하다 (흑마 도트스킬 같은것들)  불가능하다(법사 지능버프 같은것들)")]
    private bool m_b_CanDuplicate;
    [SerializeField]
    private StatBuffInfo m_StatBuffInfo;
    public byte MaxStack { get { return m_MaxStack; } set { m_MaxStack = value; } }

    public float StackScale { get { return m_StackScale; } set { m_StackScale = value; } }

    public float BuffDurationTime { get { return m_BuffDurationTime; } set { m_BuffDurationTime = value; } }

    public bool CanDuplicate { get { return m_b_CanDuplicate; } set { m_b_CanDuplicate = value; } }

    public StatBuffInfo StatBuffInfo { get { return m_StatBuffInfo; } set { m_StatBuffInfo = value; } }
    public float PassiveCoolTime { get { return m_PassiveCoolTime; } set { m_PassiveCoolTime = value; } }

    public void ApplyBuff(BuffInfo buff)
    {
        buff.Target.AddHealth(buff.Target.InitHealth * 0.2f);
        StatBuffInfo.BuffInfo = buff;
        StatBuffInfo.IsRegister = true;
        StatBuffInfo.BuffInfo.Rewrite.Invoke();
        buff.SetStatBuffInfo(StatBuffInfo);
        buff.Target.ApplyStatChanged(StatBuffInfo);
    }

    //public override void Cast(LivingEntity castedBy)
    //{
    //    //BuffManager.Instance.RegisterBuff(castedBy, castedBy, this);
    //    //Logger.Log("Cast Close Call");
    //    //BuffManager.Instance.RegisterPassive(castedBy, castedBy, this);
    //}

    //public override void Casting()
    //{

    //}

    public Sprite GetIcon()
    {
        return Icon;
    }

    public void RemoveBuff(BuffInfo buff)
    {
        StatBuffInfo.BuffInfo = buff;
        //StatBuffInfo.RemoveBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Percent, EBuffFactorType.Health));
        StatBuffInfo.IsRegister = false;
        buff.Target.ApplyStatChanged(StatBuffInfo);
    }

    public void ApplyPassive(BuffInfo buff)
    {
        StatBuffInfo.BuffInfo = buff;
        StatBuffInfo.IsRegister = true;
        StatBuffInfo.BuffInfo.Rewrite.Invoke();
        buff.SetStatBuffInfo(StatBuffInfo);

        buff.Condition.HealthChangeDelegate = OnHealthChange;
        buff.Condition.SetCoolTime(PassiveCoolTime, false);
        buff.TriggerBuffDelegate = TriggerBuff;
        //buff.Condition.RegisterEvent<HealthChangeEventArgs>(EEventType.Health, OnHealthChangeEvent);
        //buff.Target.HealthChangeEvent += OnHealthChangeEvent;
        //buff.EventDelegateList.Add(OnHealthChangeEvent);
    }

    public void RemovePassive(BuffInfo buff)
    {
        //buff.Target.HealthChangeEvent -= OnHealthChangeEvent;
    }

    public void RegisterPassive(LivingEntity castedBy, LivingEntity target)
    {
        BuffManager.Instance.RegisterPassive(castedBy, target, this);
    }

    private void TriggerBuff(LivingEntity castedBy, LivingEntity target, BuffInfo info)
    {
        RegisterBuff(castedBy, target);
    }

    public void RegisterBuff(LivingEntity castedBy, LivingEntity target)
    {
        BuffManager.Instance.RegisterBuff(castedBy, target, this);
    }

    public bool OnHealthChange(object sender, HealthChangeEventArgs e)
    {
        if (e.Health <= 0.0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    public override void Init()
    {
        Name = "name_CloseCall";
        Description = "desc_CloseCall";

        StatBuffInfo = new StatBuffInfo(this);
        //StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Percent, EBuffFactorType.Health));
        StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(1, EAmountType.Plus, EBuffFactorType.Invincible));

        //MaxStack = 1;
        //StackScale = 1f;
        //DurationTime = 1.0f;
        //CanDuplicate = false;

        //패시브 내부 쿨
        //PassiveCoolTime = 300.0f;

        //mTag = new List<string>(2);
        //Tag.Add("buff");
        //Tag.Add("passive");
    }
}
