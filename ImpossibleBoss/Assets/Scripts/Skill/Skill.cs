
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public struct SBuffFactorInfo
{
    private readonly float mAmount;
    private readonly EAmountType meAmountType;
    private readonly EBuffFactorType meBuffFactorType;

    public float Amount { get { return mAmount; } }
    public EAmountType AmountType { get { return meAmountType; } }
    public EBuffFactorType FactorType { get { return meBuffFactorType; } }

    public SBuffFactorInfo(float amount, EAmountType eAmountType, EBuffFactorType eBuffFactorType)
    {
        mAmount = amount;
        meAmountType = eAmountType;
        meBuffFactorType = eBuffFactorType;
    }
}
[System.Serializable]
public class StatBuffInfo
{
    private readonly Skill mSkill;
    private BuffInfo mBuffInfo;
    private readonly List<SBuffFactorInfo> mBuffFactorInfo;
    private readonly List<string> mBanTagList;
    private string mBuffText;
    private bool mbIsRegister;

    public Skill Skill { get { return mSkill; } }
    public BuffInfo BuffInfo { get { return mBuffInfo; } set { mBuffInfo = value; BuffInfo.Rewrite = RewriteBuffText; } }
    public bool IsRegister { get { return mbIsRegister; } set { mbIsRegister = value; } }
    public List<SBuffFactorInfo> BuffFactorInfo { get { return mBuffFactorInfo; } }
    public List<string> BanTagList { get { return mBanTagList; } }
    public string BuffText { get { /*RewriteBuffText();*/ return mBuffText; } set { mBuffText = value; } }
    public void AddBuffFactorInfo(SBuffFactorInfo buffFactorInfo) { mBuffFactorInfo.Add(buffFactorInfo); if (BuffInfo != null) BuffInfo.Rewrite = RewriteBuffText; }
    //public void RemoveBuffFactorInfo(SBuffFactorInfo buffFactorInfo) { mBuffFactorInfo.Remove(buffFactorInfo); }
    public void AddCondition() { }
    public void AddBanTag(string tag) { mBanTagList.Add(tag); }
    private void GetColoredText(out string coloredAmount, float amount)
    {
        if (amount < 0)
        {
            coloredAmount = "<color=#ff0000>" + amount + "</color>";
        }
        else
        {
            coloredAmount = "<color=#00ff00>" + amount + "</color>";
        }
    }

    private void RewriteBuffText()
    {
        //BuffText = string.Format("{0}\n", BuffInfo.Target.DamagePercent);
        BuffText = "";
        string amount;
        foreach (var factorInfo in BuffFactorInfo)
        {
            GetColoredText(out amount, factorInfo.Amount * BuffInfo.StackScale * BuffInfo.Stack);
            string factor;
            switch (factorInfo.FactorType)
            {
                case EBuffFactorType.Damage: factor = "Damage"; break;
                case EBuffFactorType.Armor: factor = "Armor"; break;
                case EBuffFactorType.Health: factor = "Health"; break;
                case EBuffFactorType.MaxHealth: factor = "MaxHealth"; break;
                case EBuffFactorType.CoolDown: factor = "Cool Down"; break;
                case EBuffFactorType.AttackSpeed: factor = "Attack Speed"; break;
                case EBuffFactorType.Regen: factor = "Regen"; break;
                case EBuffFactorType.RegenTime: factor = "Regen Time"; break;
                case EBuffFactorType.MoveSpeed: factor = "Move Speed"; break;
                case EBuffFactorType.DamageResistance: factor = "DMG Resistance"; break;
                case EBuffFactorType.DamageAmplification: factor = "DMG Amp"; break;
                case EBuffFactorType.Invincible: factor = "Invincible"; break;
                default: throw new ArgumentOutOfRangeException(nameof(factorInfo.FactorType));
            }

            if (factorInfo.AmountType == EAmountType.Percent)
            {
                BuffText += string.Format("{0}     {4} : {1:0}%     {2}       stack : {3}\n", Skill.Name, amount, BuffInfo.CastedBy.Name, BuffInfo.Stack, factor);
            }
            else
            {
                BuffText += string.Format("{0}     {4} : +{1:0}     {2}       stack : {3}\n", Skill.Name, amount, BuffInfo.CastedBy.Name, BuffInfo.Stack, factor);
            }
        }

        if (BanTagList.Count > 0)
        {
            foreach (var tag in BanTagList)
            {
                BuffText += string.Format("Skill Ban : {0}     \n", tag);
            }
        }
    }

    public StatBuffInfo(Skill skill, BuffInfo buffInfo, bool isRegister)
    {
        mSkill = skill;
        mBuffInfo = buffInfo;
        mbIsRegister = isRegister;
        mBuffFactorInfo = new List<SBuffFactorInfo>(1);
        mBanTagList = new List<string>(1);
    }

    public StatBuffInfo(Skill skill)
    {
        mSkill = skill;
        mBuffFactorInfo = new List<SBuffFactorInfo>(1);
        mBanTagList = new List<string>(1);
    }

    private StatBuffInfo(StatBuffInfo info)
    {
        mSkill = info.mSkill;
        mBuffInfo = info.mBuffInfo;
        mBuffInfo.Rewrite = RewriteBuffText;
        mBuffFactorInfo = info.mBuffFactorInfo;
        mBanTagList = info.mBanTagList;
        mBuffText = info.mBuffText;
        mbIsRegister = info.mbIsRegister;
    }

    public StatBuffInfo Clone()
    {
        StatBuffInfo newStatBuffInfo = new StatBuffInfo(this);
        return newStatBuffInfo;
    }

}

public enum EAmountType
{
    Percent,
    Plus
}

public interface IBuff
{
    public byte MaxStack { get; }
    public float StackScale { get; }
    public float BuffDurationTime { get; }
    public bool CanDuplicate { get; }
    //public Sprite GetIcon();
    public void RegisterBuff(LivingEntity castedBy, LivingEntity target);
    public void ApplyBuff(BuffInfo buff);
    public void RemoveBuff(BuffInfo buff);
    public StatBuffInfo StatBuffInfo { get; set; }
}

public interface ILazyBuff
{
    public void ApplyLazyBuff(BuffInfo buff);
}

public interface IPassive
{
    //패시브 내부쿨타임.
    public float PassiveCoolTime { get; set; }
    public Sprite GetIcon();
    public void RegisterPassive(LivingEntity castedBy, LivingEntity target);
    public void ApplyPassive(BuffInfo buff);
    public void RemovePassive(BuffInfo buff);
    public StatBuffInfo StatBuffInfo { get; set; }

}

public interface IChanneling
{
    public float ChannelingTime { get; set; }
    public float ElapsedChannelingTime { get; set; }
    public bool CanBeInterruptedByDash { get; set; }
    public void ChannelingStart(LivingEntity castedBy);
    public void Channeling(LivingEntity castedBy);
    public void ChannelingEnd(LivingEntity castedBy);
}

public interface IDOT
{
    public float DOTInterval { get; set; }
    public void DOT(LivingEntity causedBy, LivingEntity target);
}

public interface ISkillChain
{
    public float FirstCoolTime { get; set; }
    public float SecondCoolTime { get; set; }
    public float TimeLimit { get; set; }
    //체인중인가?
    public bool IsChaining { get; set; }
    //스킬 사용 않고 TimeLimit이 지나버렸을때 이 함수로 초기화
    public void TimeOut(LivingEntity castedBy);

}

public interface ILazyCast
{
    public void LazyCast(LivingEntity castedBy);
}

public interface ICast
{
    public void Cast(LivingEntity castedBy);
}

public interface ICasting
{
    public bool CanBeInterruptedByDash { get; set; }
    public float CastingTime { get; set; }
    public void Casting(LivingEntity castedBy);
}

public interface IBox
{
    public float Width { get; }
    public float Height { get; }
}
public interface IRound
{
    public float Radius { get; }
}
public interface ISector
{
    public float Radius { get; }
    public float Angle { get; }
}

public interface IRange
{
    public float SkillRange { get; }
}

public interface IDash
{
    public void Dash(LivingEntity castedBy);
    public void FinishDash();
    public float DashDistance { get; }
    public float DashSecond { get; }
}

public enum EType
{
    Move,
    Range,
    Melee,
    Buff
}

[System.Serializable]
public struct SSkillRange
{
    public float x;
    public float y;
    public float Magnitude()
    {
        return Mathf.Sqrt(x * x + y * y);
    }
    public void SetRange(float range)
    {
        x = range;
        y = range;
    }
}

[System.Serializable]
public struct SAnimStateInfo
{
    [SerializeField]
    private EAnimState m_AnimState;
    [SerializeField]
    private float m_AnimTime;
    [SerializeField/*, Tooltip("이펙트 시간은 애니메이션 타임 따라감")*/]
    private GameObject m_Effect;
    [SerializeField, Tooltip("캐스팅처럼 속도는 일정하돼 시간만 다른것들은 true")]
    private bool m_IsConstantSpeed;

    public EAnimState AnimState { get { return m_AnimState; } }
    public float AnimTime { get { return m_AnimTime; } }
    public GameObject Effect { get { return m_Effect; } }
    public bool IsConstantSpeed { get { return m_IsConstantSpeed; } }

    public static SAnimStateInfo GetDefault()
    {
        SAnimStateInfo defaultInfo = new SAnimStateInfo();

        defaultInfo.m_AnimState = EAnimState.Idle;
        defaultInfo.m_AnimTime = 1.0f;
        defaultInfo.m_IsConstantSpeed = true;

        return defaultInfo;
    }
}

public abstract class Skill : ScriptableObject
{
    [SerializeField]
    private /*readonly*/ ushort m_ID;
    [SerializeField, Tooltip("패시브 내부쿨과 독립적인 관계임")]
    private /*readonly*/ float m_CoolDown;
    //[SerializeField]
    //private /*readonly*/ float m_CastingTime;
    //[SerializeField]
    //private float m_SkillRange;    
    private /*readonly*/ string m_SkillName;
    private /*readonly*/ string m_Description;
    [SerializeField]
    private List<SAnimStateInfo> m_AnimQueue;
    //[SerializeField]
    private /*readonly*/ Sprite mIcon;
    [SerializeField]
    private /*readonly*/ EType m_Type;
    [SerializeField]
    protected List<string> m_Tag;
    //[SerializeField]
    //private AnimationClip m_AnimClip;
    //[SerializeField]
    //현재 플레이중인 이펙트 캐싱용
    private GameObject m_PlayingEffect;
    //[SerializeField]
    //private ECastMotion m_CastMotion;
    [SerializeField]
    private AnimatorOverrideController m_AOC;
    //[SerializeField]
    private float m_AnimTime;
    [SerializeField, Tooltip("스킬 사용이 가능한가?")]
    private bool m_b_CanUse;
    [SerializeField, Tooltip("무빙 캐스팅이 가능한가?")]
    private bool m_b_CanMove;
    [SerializeField, Tooltip("시전 방향으로 강제 고정")]
    private bool m_b_IsForceRotation;
    [SerializeField]
    private bool m_b_IsKnockBackImmun;
    [SerializeField]
    private bool m_b_HasChain;
    [SerializeField, Tooltip("애니메이션 동안 이동입력을 차단할 것인가?")]
    private bool m_b_ShouldObstructInput;

    //           Boss      Enemy
    int mMask = (1 << 8) | (1 << 9);


    public ushort ID { get { return m_ID; } }
    public float CoolDown { get { return m_CoolDown; } protected set { m_CoolDown = value; } }
    public string Name { get { return m_SkillName; } protected set { m_SkillName = value; } }
    public string Description { get { return m_Description; } protected set { m_Description = value; } }
    public Sprite Icon { get { return mIcon; } private set { mIcon = value; } }
    public EType Type { get { return m_Type; } }
    //public AnimationClip AnimClip { get { return m_AnimClip; } }
    public GameObject Effect { get { return m_PlayingEffect; } set { m_PlayingEffect = value; } }
    //public ECastMotion CastMotion { get { return m_CastMotion; } }
    public AnimatorOverrideController AOC { get { return m_AOC; } }
    public float AnimTime { get { Assert.AreNotEqual(0.0f, m_AnimTime, "Anim Time is 0"); return m_AnimTime; } }
    public bool CanUse { get { return m_b_CanUse; } set { m_b_CanUse = value; } }
    public bool CanMove { get { return m_b_CanMove; } protected set { m_b_CanMove = value; } }
    public bool IsForceRotation { get { return m_b_IsForceRotation; } /*protected set { m_b_IsForceRotation = value; }*/ }
    public bool IsKnockBackImmun { get { return m_b_IsKnockBackImmun; } }
    public bool HasChain { get { return m_b_HasChain; } }
    public bool ShouldObstructInput { get { return m_b_ShouldObstructInput; } }
    //public float SkillRange { get { return m_SkillRange; } set { m_SkillRange = value; } }
    public List<string> Tag { get { return m_Tag; } }

    protected int Mask { get { return mMask; } }

    private List<SAnimStateInfo> AnimQueue { get { return m_AnimQueue; } }
    protected SAnimStateInfo GetAnimInfo(EAnimState state)
    {
        return AnimQueue.Find(x => x.AnimState.Equals(state));
        //SAnimStateInfo info = AnimQueue.Find(x => x.AnimState.Equals(state));

        //return (info.Effect, info.AnimTime);
    }

    protected void SetEffectTransform(Transform transform)
    {
        if (Effect != null)
        {
            Effect.transform.position = transform.position;
            Effect.transform.rotation = transform.rotation;
        }
        
    }


    protected void PlayEffect(EAnimState state, Vector3 pos, Quaternion rot, float effectTime, Vector3? correction = null, float speedRate = 0.0f)
    {
        var info = GetAnimInfo(state);
        if (info.Effect != null)
        {
            if (correction != null)
            {
                Effect = Instantiate(info.Effect, pos + correction.Value, rot);
            }
            else
            {
                Effect = Instantiate(info.Effect, pos, rot);
            }

            if (speedRate != 0.0f)
            {
                var main = Effect.GetComponent<ParticleSystem>().main;
                main.simulationSpeed = speedRate;
            }
            Destroy(Effect, effectTime);
        }
    }
    protected void PlayEffect(EAnimState state, Vector3 pos, Quaternion rot, Vector3? correction = null, float speedRate = 0.0f)
    {
        var info = GetAnimInfo(state);
        if (info.Effect != null)
        {
            if (correction != null)
            {
                Effect = Instantiate(info.Effect, pos + correction.Value, rot);
            }
            else
            {
                Effect = Instantiate(info.Effect, pos, rot);
            }

            if (speedRate != 0.0f)
            {
                var main = Effect.GetComponent<ParticleSystem>().main;
                main.simulationSpeed = speedRate;
            }
            Destroy(Effect, info.AnimTime);
        }
    }
    protected void PlayEffect(EAnimState state, Transform transform, float effectTime, Vector3? correction = null, float speedRate = 0.0f)
    {
        var info = GetAnimInfo(state);
        if (info.Effect != null)
        {
            Effect = Instantiate(info.Effect, transform);
            if (correction != null)
            {
                Effect.transform.position += correction.Value;
            }

            if (speedRate != 0.0f)
            {
                var main = Effect.GetComponent<ParticleSystem>().main;
                main.simulationSpeed = speedRate;
            }

            ParticleSystem.MainModule particleSystems = Effect.GetComponent<ParticleSystem>().main;

            Destroy(Effect, effectTime);
        }
    }
    protected void PlayEffect(EAnimState state, Transform transform, Vector3? correction = null, float speedRate = 0.0f)
    {
        var info = GetAnimInfo(state);
        if (info.Effect != null)
        {
            Effect = Instantiate(info.Effect, transform);
            if (correction != null)
            {
                Effect.transform.position += correction.Value;
            }

            if (speedRate != 0.0f)
            {
                var main = Effect.GetComponent<ParticleSystem>().main;
                main.simulationSpeed = speedRate;
            }

            ParticleSystem.MainModule particleSystems = Effect.GetComponent<ParticleSystem>().main;

            Destroy(Effect, particleSystems.duration);
        }
    }
    //protected void PlayEffect(EAnimState state, Vector3 pos, Quaternion rot, Vector3? correction = null, bool shouldChangeSpeed = false)
    //{
    //    var info = GetAnimInfo(state);
    //    if (info.Effect != null)
    //    {
    //        if (correction != null)
    //        {
    //            Effect = Instantiate(info.Effect, pos + correction.Value, rot);
    //        }
    //        else
    //        {
    //            Effect = Instantiate(info.Effect, pos, rot);
    //        }
                                    
    //        if (shouldChangeSpeed)
    //        {
    //            ParticleSystem[] particleSystems = Effect.GetComponents<ParticleSystem>();
    //            foreach (var ps in particleSystems)
    //            {
    //                var main = ps.main;
    //                main.simulationSpeed = 1.0f / info.AnimTime;
    //            }
                
    //        }
    //        Destroy(Effect, info.AnimTime);
    //    }
    //}
    //protected void PlayEffect(EAnimState state, Transform transform, Vector3? correction = null)
    //{
    //    var info = GetAnimInfo(state);
    //    if (info.Effect != null)
    //    {
    //        Effect = Instantiate(info.Effect, transform);
    //        if (correction != null)
    //        {
    //            Effect.transform.position += correction.Value;
    //        }
            
    //        ParticleSystem.MainModule particleSystems = Effect.GetComponent<ParticleSystem>().main;

    //        Destroy(Effect, particleSystems.duration);
    //    }
    //}
    public Queue<SAnimStateInfo> GetAnimQueue()
    {
        //Queue<SAnimStateInfo> queue = new Queue<SAnimStateInfo>(m_AnimQueue);
        //for (int index = 0; index < m_AnimQueue.Count; index++)
        //{
        //    queue.Enqueue(m_AnimQueue[index]);
        //}
        return new Queue<SAnimStateInfo>(AnimQueue);
    }

    //스킬 시전모션 호출
    //public abstract void Casting();
    // 스킬 시전
    //public abstract void Cast(LivingEntity castedBy);
    public abstract void Init();

    internal void InitIcon()
    {
        string[] name = Name.Split('_');
        Icon = Resources.Load<Sprite>("Skill/Image/SKL_" + name[1]);
    }
}

//public class EmptySkill : Skill
//{
//    private static EmptySkill mInstance;
//    public static EmptySkill Instance()
//    {
//        if (mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new EmptySkill();
//        }
//    }
//    private EmptySkill() : base(0.0f, ushort.MaxValue, "Empty", "",
//        Resources.Load<Sprite>("Skill/Image/SKL_Empty"), true, EType.Buff, 0, 0, false)
//    { }
//    public override void Cast(LivingEntity castedBy)
//    {
//        //throw new NotImplementedException();
//    }

//    public override void Casting()
//    {
//        //throw new NotImplementedException();
//    }
//}
//public class Madness : Skill, IPassive
//{
//    private static Madness mInstance;    
//    public byte MaxStack { get; set; }

//    public float StackScale { get; set; }

//    public float DurationTime { get; set; }

//    public bool CanDuplicate { get; set; }
//    public StatBuffInfo statBuffInfo { get; set; }
//    public float PassiveCoolTime { get; set; }

//    public static Madness Instance()
//    {
//        if (mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new Madness();
//        }
//    }

//    //public void ApplyBuff(BuffInfo buff)
//    //{        
//    //    statBuffInfo.BuffInfo = buff;
//    //    statBuffInfo.IsRegister = true;
//    //    statBuffInfo.BuffInfo.Rewrite.Invoke();
//    //    buff.SetStatBuffInfo(statBuffInfo);
//    //    buff.Target.ApplyStatChanged(statBuffInfo);
//    //}

//    public override void Cast(LivingEntity castedBy)
//    {
//        //BuffManager.Instance.RegisterPassive(castedBy, castedBy, this);
//    }

//    public override void Casting()
//    {

//    }

//    public Sprite GetIcon()
//    {
//        return Icon;
//    }

//    //public void RemoveBuff(BuffInfo buff)
//    //{
//    //    statBuffInfo.BuffInfo = buff;
//    //    statBuffInfo.IsRegister = false;
//    //    buff.Target.ApplyStatChanged(statBuffInfo);
//    //}

//    public void ApplyPassive(BuffInfo buff)
//    {
//        statBuffInfo.BuffInfo = buff;
//        statBuffInfo.IsRegister = true;
//        statBuffInfo.BuffInfo.Rewrite.Invoke();
//        buff.SetStatBuffInfo(statBuffInfo);
//        buff.Target.ApplyStatChanged(statBuffInfo);
//    }

//    public void RemovePassive(BuffInfo buff)
//    {
//        statBuffInfo.BuffInfo = buff;
//        statBuffInfo.IsRegister = false;
//        buff.Target.ApplyStatChanged(statBuffInfo);
//    }

//    public void RegisterPassive(LivingEntity castedBy, LivingEntity target)
//    {
//        BuffManager.Instance.RegisterPassive(castedBy, target, this);
//    }

//    private Madness() : base(0.0f, 0, "Madness", "입히는 데미지 <color=#00FFFD>20</color>% 만큼 증가, 받는데미지 <color=#00FFFD>20</color>%만큼 증가",
//        Resources.Load<Sprite>("Skill/Image/SKL_Madness"), true, EType.Buff, 0, 0, false)
//    {
//        statBuffInfo = new StatBuffInfo(this);
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Percent, EBuffFactorType.DamageResistance));
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Percent, EBuffFactorType.DamageAmplification));

//        MaxStack = 1;
//        StackScale = 1f;
//        DurationTime = -1.0f;
//        PassiveCoolTime = -1.0f;
//        CanDuplicate = false;
//        mTag = new List<string>(3);
//        Tag.Add("buff");
//        Tag.Add("debuff");
//        Tag.Add("passive");
//    }
//}

//public class FixedArtillery : Skill, IPassive
//{
//    private static FixedArtillery mInstance;
//    public byte MaxStack { get; set; }

//    public float StackScale { get; set; }

//    public float DurationTime { get; set; }

//    public bool CanDuplicate { get; set; }
//    public StatBuffInfo statBuffInfo { get; set; }
//    public float PassiveCoolTime { get; set; }

//    public static FixedArtillery Instance()
//    {
//        if (mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new FixedArtillery();
//        }
//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        //BuffManager.Instance.RegisterPassive(castedBy, castedBy, this);
//    }

//    public override void Casting()
//    {

//    }

//    public Sprite GetIcon()
//    {
//        return Icon;
//    }

//    public void ApplyPassive(BuffInfo buff)
//    {
//        statBuffInfo.BuffInfo = buff;
//        statBuffInfo.IsRegister = true;
//        statBuffInfo.BuffInfo.Rewrite.Invoke();
//        buff.SetStatBuffInfo(statBuffInfo);
//        buff.Target.ApplyStatChanged(statBuffInfo);
//    }

//    public void RemovePassive(BuffInfo buff)
//    {
//        statBuffInfo.BuffInfo = buff;
//        statBuffInfo.IsRegister = false;
//        buff.Target.ApplyStatChanged(statBuffInfo);
//    }

//    public void RegisterPassive(LivingEntity castedBy, LivingEntity target)
//    {
//        BuffManager.Instance.RegisterPassive(castedBy, target, this);
//    }
//    private FixedArtillery() : base(0.0f, 1, "FixedArtillery", "move태그 붙은 스킬 사용불가, 입히는데미지 <color=#00FFFD>15</color>%만큼 증가",
//        Resources.Load<Sprite>("Skill/Image/SKL_FixedArtillery"), true, EType.Buff, 0, 0, false)
//    {
//        statBuffInfo = new StatBuffInfo(this);
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(15, EAmountType.Percent, EBuffFactorType.DamageAmplification));
//        statBuffInfo.AddBanTag("move");

//        MaxStack = 1;
//        StackScale = 1f;
//        DurationTime = -1.0f;
//        PassiveCoolTime = -1.0f;
//        CanDuplicate = false;

//        mTag = new List<string>(3);
//        Tag.Add("buff");
//        Tag.Add("ban");
//        Tag.Add("passive");
//    }
//}

//public class ChargeStance : Skill, IPassive
//{
//    private static ChargeStance mInstance;
//    public byte MaxStack { get; set; }

//    public float StackScale { get; set; }

//    public float DurationTime { get; set; }

//    public bool CanDuplicate { get; set; }
//    public StatBuffInfo statBuffInfo { get; set; }
//    public float PassiveCoolTime { get; set; }

//    public static ChargeStance Instance()
//    {
//        if (mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new ChargeStance();
//        }
//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        //BuffManager.Instance.RegisterPassive(castedBy, castedBy, this);
//    }

//    public override void Casting()
//    {

//    }

//    public Sprite GetIcon()
//    {
//        return Icon;
//    }

//    public void ApplyPassive(BuffInfo buff)
//    {
//        statBuffInfo.BuffInfo = buff;
//        statBuffInfo.IsRegister = true;
//        statBuffInfo.BuffInfo.Rewrite.Invoke();
//        buff.SetStatBuffInfo(statBuffInfo);
//        buff.Target.ApplyStatChanged(statBuffInfo);
//    }

//    public void RemovePassive(BuffInfo buff)
//    {
//        statBuffInfo.BuffInfo = buff;
//        statBuffInfo.IsRegister = false;
//        buff.Target.ApplyStatChanged(statBuffInfo);
//    }

//    public void RegisterPassive(LivingEntity castedBy, LivingEntity target)
//    {
//        BuffManager.Instance.RegisterPassive(castedBy, target, this);
//    }

//    private ChargeStance() : base(0.0f, 2, "ChargeStance", "range태그 스킬 사용불가, 입히는 데미지 10% 만큼 증가, 받는데미지 10% 만큼 감소",
//        Resources.Load<Sprite>("Skill/Image/SKL_ChargeStance"), true, EType.Buff, 0, 0, false)
//    {
//        statBuffInfo = new StatBuffInfo(this);
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(10, EAmountType.Percent, EBuffFactorType.DamageAmplification));
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(-10, EAmountType.Percent, EBuffFactorType.DamageResistance));
//        statBuffInfo.AddBanTag("range");

//        MaxStack = 1;
//        StackScale = 1f;
//        DurationTime = -1.0f;
//        PassiveCoolTime = -1.0f;
//        CanDuplicate = false;

//        mTag = new List<string>(3);
//        Tag.Add("buff");
//        Tag.Add("ban");
//        Tag.Add("passive");
//    }
//}

//public class SnipeStance : Skill, IPassive
//{
//    private static SnipeStance mInstance;
//    public byte MaxStack { get; set; }

//    public float StackScale { get; set; }

//    public float DurationTime { get; set; }

//    public bool CanDuplicate { get; set; }
//    public StatBuffInfo statBuffInfo { get; set; }
//    public float PassiveCoolTime { get; set; }

//    public static SnipeStance Instance()
//    {
//        if (mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new SnipeStance();
//        }
//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        //BuffManager.Instance.RegisterPassive(castedBy, castedBy, this);
//    }

//    public override void Casting()
//    {

//    }

//    public Sprite GetIcon()
//    {
//        return Icon;
//    }

//    public void ApplyPassive(BuffInfo buff)
//    {
//        statBuffInfo.BuffInfo = buff;
//        statBuffInfo.IsRegister = true;
//        statBuffInfo.BuffInfo.Rewrite.Invoke();
//        buff.SetStatBuffInfo(statBuffInfo);
//        buff.Target.ApplyStatChanged(statBuffInfo);
//    }

//    public void RemovePassive(BuffInfo buff)
//    {
//        statBuffInfo.BuffInfo = buff;
//        statBuffInfo.IsRegister = false;
//        buff.Target.ApplyStatChanged(statBuffInfo);
//    }

//    public void RegisterPassive(LivingEntity castedBy, LivingEntity target)
//    {
//        BuffManager.Instance.RegisterPassive(castedBy, target, this);
//    }

//    private SnipeStance() : base(0.0f, 3, "SnipeStance", "melee태그 스킬 사용불가, 행동속도 20% 만큼 증가, 행동속도는 공격 속도 , 이동 속도",
//        Resources.Load<Sprite>("Skill/Image/SKL_SnipeStance"), true, EType.Buff, 0, 0, false)
//    {
//        statBuffInfo = new StatBuffInfo(this);
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Percent, EBuffFactorType.AttackSpeed));
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Percent, EBuffFactorType.MoveSpeed));
//        statBuffInfo.AddBanTag("melee");

//        MaxStack = 1;
//        StackScale = 1f;
//        DurationTime = -1.0f;
//        PassiveCoolTime = -1.0f;
//        CanDuplicate = false;

//        mTag = new List<string>(3);
//        Tag.Add("buff");
//        Tag.Add("ban");
//        Tag.Add("passive");
//    }
//}

//public class CloseCall : Skill, IBuff, IPassive
//{
//    private static CloseCall mInstance;
//    public byte MaxStack { get; set; }

//    public float StackScale { get; set; }

//    public float DurationTime { get; set; }

//    public bool CanDuplicate { get; set; }
//    public StatBuffInfo statBuffInfo { get; set; }
//    public float PassiveCoolTime { get; set; }

//    public static CloseCall Instance()
//    {
//        if (mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new CloseCall();
//        }
//    }

//    public void ApplyBuff(BuffInfo buff)
//    {
//        buff.Target.AddHealth(buff.Target.InitHealth * 0.2f);
//        statBuffInfo.BuffInfo = buff;
//        statBuffInfo.IsRegister = true;
//        statBuffInfo.BuffInfo.Rewrite.Invoke();
//        buff.SetStatBuffInfo(statBuffInfo);
//        buff.Target.ApplyStatChanged(statBuffInfo);
//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        //BuffManager.Instance.RegisterBuff(castedBy, castedBy, this);
//        //Logger.Log("Cast Close Call");
//        //BuffManager.Instance.RegisterPassive(castedBy, castedBy, this);
//    }

//    public override void Casting()
//    {

//    }

//    public Sprite GetIcon()
//    {
//        return Icon;
//    }

//    public void RemoveBuff(BuffInfo buff)
//    {
//        statBuffInfo.BuffInfo = buff;
//        //statBuffInfo.RemoveBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Percent, EBuffFactorType.Health));
//        statBuffInfo.IsRegister = false;
//        buff.Target.ApplyStatChanged(statBuffInfo);
//    }

//    public void ApplyPassive(BuffInfo buff)
//    {
//        statBuffInfo.BuffInfo = buff;
//        statBuffInfo.IsRegister = true;
//        statBuffInfo.BuffInfo.Rewrite.Invoke();
//        buff.SetStatBuffInfo(statBuffInfo);

//        buff.Condition.HealthChangeDelegate = OnHealthChange;
//        buff.Condition.SetCoolTime(PassiveCoolTime, false);
//        buff.TriggerBuffDelegate = RegisterBuff;
//        //buff.Condition.RegisterEvent<HealthChangeEventArgs>(EEventType.Health, OnHealthChangeEvent);
//        //buff.Target.HealthChangeEvent += OnHealthChangeEvent;
//        //buff.EventDelegateList.Add(OnHealthChangeEvent);
//    }

//    public void RemovePassive(BuffInfo buff)
//    {
//        //buff.Target.HealthChangeEvent -= OnHealthChangeEvent;
//    }

//    public void RegisterPassive(LivingEntity castedBy, LivingEntity target)
//    {
//        BuffManager.Instance.RegisterPassive(castedBy, target, this);
//    }

//    public void RegisterBuff(LivingEntity castedBy, LivingEntity target)
//    {
//        BuffManager.Instance.RegisterBuff(castedBy, target, this);
//    }

//    public bool OnHealthChange(object sender, HealthChangeEventArgs e)
//    {
//        if (e.Health <= 0.0f)
//        {
//            return true;
//        }
//        else
//        {
//            return false;
//        }
//    }



//    private CloseCall() : base(0.0f, 4, "CloseCall", "죽음에 이르는 데미지 받을시 20% 만큼 피 회복 후 생존, 발동시점은 체력이 0 이하일때 아무리 큰데미지에서도 발동, 발동후 1초동안 무적",
//        Resources.Load<Sprite>("Skill/Image/SKL_CloseCall"), true, EType.Move, 0, 0, false)
//    {
//        statBuffInfo = new StatBuffInfo(this);
//        //statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Percent, EBuffFactorType.Health));
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(1, EAmountType.Plus, EBuffFactorType.Invincible));

//        MaxStack = 1;
//        StackScale = 1f;
//        DurationTime = 1.0f;
//        CanDuplicate = false;

//        //패시브 내부 쿨
//        PassiveCoolTime = 300.0f;

//        mTag = new List<string>(2);
//        Tag.Add("buff");
//        Tag.Add("passive");
//    }
//}

//public class Blink : Skill
//{
//    private static Blink mInstance;

//    STakeDamageParams mTakeDamageParams;

//    public static Blink Instance()
//    {
//        if (mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new Blink();
//        }
//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        RaycastHit hit;
//        Ray ray = castedBy.GetComponent<PlayerMovement>().m_MainCamera.ScreenPointToRay(castedBy.GetComponent<PlayerInputManager>().MousePos);
//        Physics.Raycast(ray, out hit);

//        Vector3 currPos = castedBy.transform.position;
//        Vector3 targetPos = new Vector3(hit.point.x, castedBy.transform.position.y, hit.point.z);

//        //           Boss      Enemy
//        int mask = (1 << 8) | (1 << 9);

//        if ((targetPos - currPos).sqrMagnitude <= 7.0f * 7.0f)
//        {
//            castedBy.transform.position = targetPos;
//        }
//        else
//        {
//            targetPos = currPos + (targetPos - currPos).normalized * 7.0f;
//            castedBy.transform.position = targetPos;
//        }

//        mTakeDamageParams.causedBy = castedBy.gameObject;
//        Collider[] colliders = Physics.OverlapSphere(targetPos, 3.0f, mask);
//        foreach (var collider in colliders)
//        {                        
//            collider.GetComponent<LivingEntity>().TakeDamage(mTakeDamageParams);
//        }
//    }

//    public override void Casting()
//    {

//    }

//    public Sprite GetIcon()
//    {
//        return Icon;
//    }


//    private Blink() : base(1.0f, 5, "Blink", "마우스 위치로 순간이동, 케릭터 좌표가 이동하는 방식 보스몬스터를 제외한 몬스터와 겹칠시 밀어냄",
//        Resources.Load<Sprite>("Skill/Image/SKL_Blink"), true, EType.Buff, 0, 0, true)
//    {
//        mTakeDamageParams = new STakeDamageParams();
//        mTakeDamageParams.damageAmount = 0;
//        mTakeDamageParams.cc = ECC.KnockBack;
//        mTakeDamageParams.ccAmount = 5.0f;

//        mTag = new List<string>(1);
//        Tag.Add("move");
//    }
//}

//public class Lightning : Skill, IBuff, IDOT
//{
//    private Collider[] mCollider = new Collider[10];
//    private static Lightning mInstance;

//    STakeDamageParams mTakeDamageParams;
//    STakeDamageParams mTakeDOTDamageParams;

//    public byte MaxStack { get; set; }

//    public float StackScale { get; set; }

//    public float DurationTime { get; set; }

//    public bool CanDuplicate { get; set; }

//    public StatBuffInfo statBuffInfo { get; set; }
//    public float DOTInterval { get; set; }

//    public static Lightning Instance()
//    {
//        if (mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new Lightning();
//        }
//    }    

//    public override void Cast(LivingEntity castedBy)
//    {
//        mTakeDamageParams.causedBy = castedBy.gameObject;
//        mTakeDOTDamageParams.causedBy = castedBy.gameObject;

//        RaycastHit hit;
//        Ray ray = castedBy.GetComponent<PlayerMovement>().m_MainCamera.ScreenPointToRay(castedBy.GetComponent<PlayerInputManager>().MousePos);
//        Physics.Raycast(ray, out hit);

//        Vector3 currPos = castedBy.transform.position;
//        Vector3 targetPos = new Vector3(hit.point.x, castedBy.transform.position.y, hit.point.z);

//        //           Boss      Enemy
//        int mask = (1 << 8) | (1 << 9);

//        if ((currPos - targetPos).sqrMagnitude > 25.0f)
//        {
//            targetPos = (targetPos - currPos).normalized * 5.0f;
//        }
//        else
//        {
//            targetPos = new Vector3(hit.point.x, 0.0f, hit.point.z);
//        }

//        Physics.OverlapSphereNonAlloc(targetPos, 1.0f, mCollider, mask);

//        for (int index = 0; index < mCollider.Length; index++)
//        {
//            if (mCollider[index] != null)
//            {
//                int shock = UnityEngine.Random.Range(0,2);

//                if (shock == 1)
//                {
//                    mTakeDamageParams.cc = ECC.ElectricShock;
//                    mTakeDamageParams.ccAmount = 3.0f;
//                }
//                else
//                {
//                    mTakeDamageParams.cc = ECC.None;
//                    mTakeDamageParams.ccAmount = 0.0f;
//                }

//                LivingEntity target = mCollider[index].GetComponent<LivingEntity>();
//                target.TakeDamage(mTakeDamageParams);
//                BuffManager.Instance.RegisterBuff(castedBy, target, this);
//            }
//            else
//            {
//                break;
//            }
//        }
//    }

//    public override void Casting()
//    {

//    }

//    public Sprite GetIcon()
//    {
//        return Icon;
//    }

//    public void DOT(LivingEntity causedBy, LivingEntity target)
//    {
//        Logger.Log("DOT");
//        target.TakeDamage(mTakeDOTDamageParams);
//    }

//    public void RegisterBuff(LivingEntity castedBy, LivingEntity target)
//    {

//    }

//    public void ApplyBuff(BuffInfo buff)
//    {
//        buff.SetStatBuffInfo(statBuffInfo);
//    }

//    public void RemoveBuff(BuffInfo buff)
//    {

//    }

//    private Lightning() : base(1.0f, 15, "Lightning", "범위안의 타겟 마법진에 120 데미지 적중시 50%확률로 대상 감전, 3초에걸처 0.5초당 10의 데미지",
//        Resources.Load<Sprite>("Skill/Image/SKL_Lightning"), true, EType.Range, 5, 0, false)
//    {
//        mTakeDamageParams = new STakeDamageParams();
//        mTakeDamageParams.damageAmount = 120;
//        mTakeDamageParams.cc = ECC.ElectricShock;
//        mTakeDamageParams.ccAmount = 3.0f;

//        mTakeDOTDamageParams = new STakeDamageParams();
//        mTakeDOTDamageParams.damageAmount = 10000;

//        //statBuffInfo = new StatBuffInfo(this);        
//        //statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(-10, EAmountType.Plus, EBuffFactorType.Health));

//        DurationTime = 3.0f;
//        DOTInterval = 0.5f;

//        mTag = new List<string>(2);
//        Tag.Add("range");
//        Tag.Add("instant");
//    }
//}

//public class FusilladeAcaneArrow : Skill, IChanneling
//{
//    private Collider[] mCollider = new Collider[10];
//    private static FusilladeAcaneArrow mInstance;
//    private GameObject mArcaneArrowPrefab;

//    STakeDamageParams mTakeDamageParams;
//    public float ChannelingTime { get; set; }
//    public bool CanMove { get; set; }
//    public float ElapsedChannelingTime { get; set; }
//    private GameObject ArcaneArrowPrefab { get { return mArcaneArrowPrefab; } set { mArcaneArrowPrefab = value; } }

//    public static FusilladeAcaneArrow Instance()
//    {
//        if (mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {            
//            return mInstance = new FusilladeAcaneArrow();
//        }
//    }

//    public void Channeling(LivingEntity castedBy)
//    {
//        ElapsedChannelingTime += Time.deltaTime;

//        if (ElapsedChannelingTime >= 0.5f)
//        {
//            //           Boss      Enemy
//            int mask = (1 << 8) | (1 << 9);
//            float distance = float.MaxValue;
//            LivingEntity target = null;

//            ElapsedChannelingTime -= 0.5f;
//            mTakeDamageParams.causedBy = castedBy.gameObject;
//            mTakeDamageParams.DamageAmplificationRate = castedBy.GetDamageAmplification();

//            int numFound = Physics.OverlapSphereNonAlloc(castedBy.transform.position, 3.0f, mCollider, mask);
//            for (int index = 0; index < numFound; index++)
//            {
//                if (mCollider[index].CompareTag("Boss"))
//                {
//                    //target = mCollider[index].gameObject.GetComponent<LivingEntity>();
//                    target = mCollider[index].GetComponent<LivingEntity>();
//                    break;
//                }
//                else
//                {
//                    if ((mCollider[index].transform.position - castedBy.transform.position).sqrMagnitude < distance)
//                    {
//                        target = mCollider[index].GetComponent<LivingEntity>();
//                        distance = (mCollider[index].transform.position - castedBy.transform.position).sqrMagnitude;
//                    }
//                }
//            }

//            if (target != null)
//            {
//                ArcaneArrowProjectile projectile = UnityEngine.Object.Instantiate(ArcaneArrowPrefab).GetComponent<ArcaneArrowProjectile>();
//                projectile.DeparturePos = castedBy.transform.position;
//                projectile.DestinationPos = target.transform;
//                projectile.DamageParams = mTakeDamageParams;
//            }

//        }
//        //else
//        //{
//        //    ElapsedChannelingTime += Time.deltaTime;
//        //    Logger.Log("ElapsedChan" + ElapsedChannelingTime);
//        //}  
//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        castedBy.GetComponent<PlayerSkillManager>().IsChanneling = true;                
//    }

//    public override void Casting()
//    {
//        ElapsedChannelingTime = 0.0f;
//    }

//    public Sprite GetIcon()
//    {
//        return Icon;
//    }    

//    private FusilladeAcaneArrow() : base(10.0f, 16, "Fusillade Acane Arrow", "2.5초동안 이동가능 한 캐스팅을 하며 0.5초주기로 40의 데미지를 주는 비전미사일 소환, 타겟팅은 범위내에 보스 우선하는 오토타겟팅",
//        Resources.Load<Sprite>("Skill/Image/SKL_FusilladeAcaneArrow"), true, EType.Range, 3, 0, true)
//    {
//        mTakeDamageParams = new STakeDamageParams();
//        mTakeDamageParams.damageAmount = 40;

//        mTag = new List<string>(2);
//        Tag.Add("range");
//        Tag.Add("channeling");

//        ArcaneArrowPrefab = UnityEngine.Resources.Load<GameObject>("Skill/SkillEffect/ArcaneArrow/ArcaneArrow");
//        ChannelingTime = 2.5f;
//        CanMove = true;
//    }
//}

//public class test1 : Skill 
//{
//    private static List<Sprite> mIndicatorImage;

//    private static test1 mInstance;
//    private readonly List<float> mDatas;



//    public List<float> SkillDatas { get { return mDatas; } }

//    public static test1 Instance()
//    {
//        if(mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new test1();
//        }
//    }

//    private test1() : base(1f, 0, "test1", "<color=#00FFFD>1</color> 범위에 <color=#00FFFD>2</color> 만큼의 피해를 입히고, <color=#00FFFD>3</color> 초동안 <color=#00FFFD>4</color> 의 간격으로 <color=#00FFFD>5</color> 의 딜을 가하는 DOT를 남김", 
//        Resources.Load<Sprite>("Skill/Image/test1"), true, EType.Move, 10, 2, false)
//    {
//        mDatas = new List<float>();
//        mDatas.Add((float)1);
//		mDatas.Add((float)2);
//		mDatas.Add((float)3);
//		mDatas.Add((float)4);
//		mDatas.Add((float)5);

//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        Logger.Log("Cast 0");
//    }

//    public override void Casting()
//    {
//        throw new NotImplementedException();
//    }
//}

//public class test2 : Skill , IBuff, IDOT
//{
//    private static List<Sprite> mIndicatorImage;

//    private static test2 mInstance;
//    private readonly List<float> mDatas;


//    private byte mMaxStack;
//    private float mStackScale;
//    private float mDurationTime;
//    private float mDOTInterval;
//    private readonly bool mbCanDuplicate;

//    public byte MaxStack { get => mMaxStack; private set => mMaxStack = value; }
//    public float StackScale { get => mStackScale; private set => mStackScale = value; }
//    public bool CanDuplicate { get => mbCanDuplicate; }
//    public float DurationTime { get => mDurationTime; private set => mDurationTime = value; }


//    public List<float> SkillDatas { get { return mDatas; } }

//    public float DOTInterval { get { return mDOTInterval; } set { mDOTInterval = value; } }

//    public static test2 Instance()
//    {
//        if(mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new test2();
//        }
//    }

//    private test2() : base(2.5f, 1, "test2", "<color=#00FFFD>20</color> 초동안 공격력 <color=#00FFFD>10</color> 상승 방어력 param2 % 상승",
//        Resources.Load<Sprite>("Skill/Image/test2"), true, EType.Move, 0, 2, true)
//    {
//        mDatas = new List<float>();
//        mDatas.Add((float)20);
//		mDatas.Add((float)10);

//        MaxStack = 5;
//        StackScale = 1f;
//        DurationTime = 3f;
//        DOTInterval = 0.5f;
//        mbCanDuplicate = false;
//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        Logger.Log("Cast 1");
//        BuffManager.Instance.RegisterBuff(player, player, this);
//    }   

//    public void ApplyBuff(BuffInfo buff)
//    {
//        StatBuffInfo statBuffInfo;

//        statBuffInfo = new StatBuffInfo(this, buff, true);
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Plus, EBuffFactorType.Damage));
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(10, EAmountType.Percent, EBuffFactorType.Armor));
//        statBuffInfo.BuffInfo.Rewrite.Invoke();
//        buff.BuffIcon.GetComponent<BuffIcon>().BuffInfo = statBuffInfo;
//        buff.Target.AddBuffInfo(statBuffInfo);


//    }

//    public void RemoveBuff(BuffInfo buff)
//    {
//        StatBuffInfo statBuffInfo;

//        statBuffInfo = new StatBuffInfo(this, buff, false);
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(20, EAmountType.Plus, EBuffFactorType.Damage));
//        statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(10, EAmountType.Percent, EBuffFactorType.Armor));
//        buff.Target.AddBuffInfo(statBuffInfo);
//    }

//    public Sprite GetIcon()
//    {
//        return Icon;
//    }

//    public void DOT(LivingEntity causedBy, LivingEntity target)
//    {
//        STakeDamageParams takeDamageParams = new STakeDamageParams();
//        takeDamageParams.causedBy = causedBy.gameObject;
//        takeDamageParams.damageAmount = causedBy.GetDamageAmount(100000.0f);
//        takeDamageParams.DamageAmplificationRate = causedBy.GetDamageAmplification();
//        target.TakeDamage(takeDamageParams);
//        Logger.Log(causedBy.GetDamageAmount(100000.0f));
//    }
//    public override void Casting()
//    {
//        throw new NotImplementedException();
//    }
//}

//public class test3 : Skill 
//{
//    private static List<Sprite> mIndicatorImage;

//    private static test3 mInstance;
//    private readonly List<float> mDatas;



//    public List<float> SkillDatas { get { return mDatas; } }

//    public static test3 Instance()
//    {
//        if(mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new test3();
//        }
//    }

//    private test3() : base(3f, 2, "test3", "<color=#00FFFD>15</color> 초동안 공격력 <color=#00FFFD>10</color> % 상승", Resources.Load<Sprite>("Skill/Image/test3"), false, EType.Move, 0, 2, false)
//    {
//        mDatas = new List<float>();
//        mDatas.Add((float)10);
//		mDatas.Add((float)15);

//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        Logger.Log("Cast 2");
//    }
//    public override void Casting()
//    {
//        throw new NotImplementedException();
//    }
//}

//public class test4 : Skill 
//{
//    private static List<Sprite> mIndicatorImage;

//    private static test4 mInstance;
//    private readonly List<float> mDatas;



//    public List<float> SkillDatas { get { return mDatas; } }

//    public static test4 Instance()
//    {
//        if(mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new test4();
//        }
//    }

//    private test4() : base(4f, 3, "test4", "<color=#00FFFD>10</color> 초동안 방어력 <color=#00FFFD>50</color> 감소", Resources.Load<Sprite>("Skill/Image/test4"), false, EType.Move, 0, 2, false)
//    {
//        mDatas = new List<float>();
//        mDatas.Add((float)50);
//		mDatas.Add((float)10);

//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        Logger.Log("Cast 3");
//    }
//    public override void Casting()
//    {
//        throw new NotImplementedException();
//    }
//}

//public class test5 : Skill 
//{
//    private static List<Sprite> mIndicatorImage;

//    private static test5 mInstance;
//    private readonly List<float> mDatas;



//    public List<float> SkillDatas { get { return mDatas; } }

//    public static test5 Instance()
//    {
//        if(mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new test5();
//        }
//    }

//    private test5() : base(5.5f, 4, "test5", "<color=#00FFFD>15</color> 초동안 공격속도 <color=#00FFFD>10</color>% 상승", Resources.Load<Sprite>("Skill/Image/test5"), true, EType.Move, 0, 2, false)
//    {
//        mDatas = new List<float>();
//        mDatas.Add((float)10);
//		mDatas.Add((float)15);

//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        Logger.Log("Cast 4");
//    }
//    public override void Casting()
//    {
//        throw new NotImplementedException();
//    }
//}

//public class test6 : Skill 
//{
//    private static List<Sprite> mIndicatorImage;

//    private static test6 mInstance;
//    private readonly List<float> mDatas;



//    public List<float> SkillDatas { get { return mDatas; } }

//    public static test6 Instance()
//    {
//        if(mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new test6();
//        }
//    }

//    private test6() : base(6f, 5, "test6", "<color=#00FFFD>15</color> 거리만큼 순간이동", Resources.Load<Sprite>("Skill/Image/test6"), false, EType.Move, 0, 2, false)
//    {
//        mDatas = new List<float>();
//        mDatas.Add((float)15);

//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        Logger.Log("Cast 5");
//    }
//    public override void Casting()
//    {
//        throw new NotImplementedException();
//    }
//}

//public class test7 : Skill 
//{
//    private static List<Sprite> mIndicatorImage;

//    private static test7 mInstance;
//    private readonly List<float> mDatas;



//    public List<float> SkillDatas { get { return mDatas; } }

//    public static test7 Instance()
//    {
//        if(mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new test7();
//        }
//    }

//    private test7() : base(7f, 6, "test7", "parma 0 범위에 <color=#00FFFD>10</color> 만큼 피해를 입히고, 다음 공격시 터져서 param3만큼의 데미지를 주는 폭탄을 설치한다", 
//        Resources.Load<Sprite>("Skill/Image/test7"), false, EType.Move, 0, 2)
//    {
//        mDatas = new List<float>();
//        mDatas.Add((float)10);
//		mDatas.Add((float)10);
//		mDatas.Add((float)50);

//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        Logger.Log("Cast 6");
//    }
//    public override void Casting()
//    {
//        throw new NotImplementedException();
//    }
//}

//public class test8 : Skill 
//{
//    private static List<Sprite> mIndicatorImage;

//    private static test8 mInstance;
//    private readonly List<float> mDatas;



//    public List<float> SkillDatas { get { return mDatas; } }

//    public static test8 Instance()
//    {
//        if(mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new test8();
//        }
//    }

//    private test8() : base(8f, 7, "test8", "parma0 초동안 시전위치에서 기를 모으며 이동하고 이동한 거리에 비례하여 최소 <color=#00FFFD>10</color>, 최대 <color=#00FFFD>70</color> 만큼 피해를 준다", 
//        Resources.Load<Sprite>("Skill/Image/test8"), false, EType.Move, 0, 2)
//    {
//        mDatas = new List<float>();
//        mDatas.Add((float)10);
//		mDatas.Add((float)10);
//		mDatas.Add((float)70);

//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        Logger.Log("Cast 7");
//    }
//    public override void Casting()
//    {
//        throw new NotImplementedException();
//    }
//}

//public class test9 : Skill 
//{
//    private static List<Sprite> mIndicatorImage;

//    private static test9 mInstance;
//    private readonly List<float> mDatas;



//    public List<float> SkillDatas { get { return mDatas; } }

//    public static test9 Instance()
//    {
//        if(mInstance != null)
//        {
//            return mInstance;
//        }
//        else
//        {
//            return mInstance = new test9();
//        }
//    }

//    private test9() : base(9f, 8, "test9", "<color=#00FFFD>10</color> 거리만큼 이동(구르기)  <color=#00FFFD>5</color> 초동안 다시 parma0 거리만큼 이동가능", Resources.Load<Sprite>("Skill/Image/test9"), 
//        false, EType.Move, 0, 2)
//    {
//        mDatas = new List<float>();
//        mDatas.Add((float)10);
//		mDatas.Add((float)5);
//		mDatas.Add((float)5);

//    }

//    public override void Cast(LivingEntity castedBy)
//    {
//        Logger.Log("Cast 8");
//    }
//    public override void Casting()
//    {
//        throw new NotImplementedException();
//    }
//}

