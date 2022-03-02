using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "Lightning", menuName = "Skills/Lightning")]
public class Lightning : Skill, IBuff, IDOT, ICast, IRange, IRound
{
    private Collider[] mCollider;


    //[SerializeField]
    private STakeDamageParams m_DamageInfo;
    //[SerializeField]
    private STakeDamageParams m_DOTInfo;

    [SerializeField]
    private float m_Damage;
    [SerializeField]
    private float m_DOTDamage;
    [SerializeField]
    private float m_DOTInterval;
    [SerializeField, Tooltip("시전 가능 범위")]
    private float m_SkillRange;

    [SerializeField, Tooltip("타격 마법진 반지름")]
    private float m_Radius;

    //[SerializeField]
    private byte m_MaxStack;
    //[SerializeField]
    private float m_StackScale;
    [SerializeField]
    private float m_BuffDurationTime;
    [SerializeField, Tooltip("다른 시전자와 중복가능하다 (흑마 도트스킬 같은것들)  불가능하다(법사 지능버프 같은것들)")]
    private bool m_b_CanDuplicate;
    //[SerializeField]
    private StatBuffInfo mStatBuffInfo;
    

    public byte MaxStack { get { return m_MaxStack; } set { m_MaxStack = value; } }

    public float StackScale { get { return m_StackScale; } set { m_StackScale = value; } }

    public float BuffDurationTime { get { return m_BuffDurationTime; } set { m_BuffDurationTime = value; } }

    public bool CanDuplicate { get { return m_b_CanDuplicate; } set { m_b_CanDuplicate = value; } }

    public StatBuffInfo StatBuffInfo { get { return mStatBuffInfo; } set { mStatBuffInfo = value; } }
    public float DOTInterval { get { return m_DOTInterval; } set { m_DOTInterval = value; } }

    public float SkillRange { get => m_SkillRange; }

    public float Radius => m_Radius;

    public/* override */void Cast(LivingEntity castedBy)
    {
        STakeDamageParams damageParams = m_DamageInfo;
        damageParams.causedBy = castedBy.gameObject;
        damageParams.damageAmount = castedBy.GetDamageAmount(damageParams.damageAmount);
        m_DOTInfo.causedBy = castedBy.gameObject;

        RaycastHit hit;
        Ray ray = castedBy.GetComponent<PlayerMovement>().m_MainCamera.ScreenPointToRay(castedBy.GetComponent<PlayerInputManager>().MousePos);
        Physics.Raycast(ray, out hit);

        Vector3 currPos = castedBy.transform.position;
        Vector3 targetPos = new Vector3(hit.point.x, castedBy.transform.position.y, hit.point.z);
        
        //           Boss      Enemy
        //int mask = (1 << 8) | (1 << 9);

        if ((currPos - targetPos).sqrMagnitude > SkillRange * SkillRange)
        {
            targetPos = (targetPos - currPos).normalized * SkillRange + currPos;            
        }
        else
        {
            targetPos = new Vector3(hit.point.x, 0.0f, hit.point.z);
        }

        //GameObject gameObject = Instantiate(Effect, new Vector3(targetPos.x, targetPos.y + 6, targetPos.z), Quaternion.Euler(-90.0f, 0.0f, 0.0f));
        //Destroy(gameObject, /*Effect.GetComponent<VisualEffect>()*/ 0.5f);

        PlayEffect(EAnimState.Cast, new Vector3(targetPos.x, targetPos.y + 6, targetPos.z), Quaternion.Euler(-90.0f, 0.0f, 0.0f));

        Physics.OverlapSphereNonAlloc(targetPos, Radius, mCollider, Mask);

        for (int index = 0; index < mCollider.Length; index++)
        {
            if (mCollider[index] != null)
            {
                int shock = UnityEngine.Random.Range(0, 2);

                LivingEntity target = mCollider[index].GetComponent<LivingEntity>();
                target.TakeDamage(damageParams);

                if (shock == 1)
                {                    
                    RegisterBuff(castedBy, target);
                }
            }
            else
            {
                break;
            }
        }
    }

    //public override void Casting()
    //{

    //}

    public Sprite GetIcon()
    {
        return Icon;
    }

    public void DOT(LivingEntity causedBy, LivingEntity target)
    {
        Logger.Log("DOT");
        target.TakeDamage(m_DOTInfo);
    }

    public void RegisterBuff(LivingEntity castedBy, LivingEntity target)
    {
        BuffManager.Instance.RegisterBuff(castedBy, target, this);
    }

    public void ApplyBuff(BuffInfo buff)
    {

        //buff.SetStatBuffInfo(StatBuffInfo.Clone());
        //buff.Target.ApplyStatChanged();
    }

    public void RemoveBuff(BuffInfo buff)
    {

    }

    public override void Init()
    {
        Name = "name_Lightning";
        Description = "desc_Lightning";

        m_DamageInfo.damageAmount = m_Damage;
        m_DamageInfo.tag = Tag;
        m_DOTInfo.damageAmount = m_DOTDamage;
        m_DOTInfo.tag = new List<string>(1);
        m_DOTInfo.tag.Add("DOT");

        MaxStack = 1;
        StackScale = 1.0f;

        mCollider = new Collider[10];

    }


    //private Lightning() : base(1.0f, 15, "Lightning", "범위안의 타겟 마법진에 120 데미지 적중시 50%확률로 대상 감전, 3초에걸처 0.5초당 10의 데미지",
    //    Resources.Load<Sprite>("Skill/Image/SKL_Lightning"), true, EType.Range, 5, 0, false)
    //{
    //    m_DamageInfo = new STakeDamageParams();
    //    m_DamageInfo.damageAmount = 120;
    //    m_DamageInfo.cc = ECC.ElectricShock;
    //    m_DamageInfo.ccAmount = 3.0f;

    //    m_DOTInfo = new STakeDamageParams();
    //    m_DOTInfo.damageAmount = 10000;

    //    //statBuffInfo = new StatBuffInfo(this);        
    //    //statBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(-10, EAmountType.Plus, EBuffFactorType.Health));

    //    DurationTime = 3.0f;
    //    DOTInterval = 0.5f;

    //    mTag = new List<string>(2);
    //    Tag.Add("range");
    //    Tag.Add("instant");
    //}
}
