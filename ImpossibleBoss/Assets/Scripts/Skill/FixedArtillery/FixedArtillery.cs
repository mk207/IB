using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FixedArtillery", menuName = "Skills/FixedArtillery")]
public class FixedArtillery : Skill, IPassive
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

    //public static FixedArtillery Instance()
    //{
    //    if (mInstance != null)
    //    {
    //        return mInstance;
    //    }
    //    else
    //    {
    //        return mInstance = new FixedArtillery();
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
        Name = "name_FixedArtillery";
        Description = "desc_FixedArtillery";

        StatBuffInfo = new StatBuffInfo(this);
        StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(15, EAmountType.Percent, EBuffFactorType.DamageAmplification));
        StatBuffInfo.AddBanTag("move");
    }
    //private FixedArtillery() : base(0.0f, 1, "FixedArtillery", "move�±� ���� ��ų ���Ұ�, �����µ����� <color=#00FFFD>15</color>%��ŭ ����",
    //    Resources.Load<Sprite>("Skill/Image/SKL_FixedArtillery"), true, EType.Buff, 0, 0, false)
    //{
    //    StatBuffInfo = new StatBuffInfo(this);
    //    StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(15, EAmountType.Percent, EBuffFactorType.DamageAmplification));
    //    StatBuffInfo.AddBanTag("move");

    //    MaxStack = 1;
    //    StackScale = 1f;
    //    DurationTime = -1.0f;
    //    PassiveCoolTime = -1.0f;
    //    CanDuplicate = false;

    //    mTag = new List<string>(3);
    //    Tag.Add("buff");
    //    Tag.Add("ban");
    //    Tag.Add("passive");
    //}
}
