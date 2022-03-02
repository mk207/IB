using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HundredFists", menuName = "Skills/HundredFists")]
public class HundredFists : Skill, IChanneling, IBox
{
    private Collider[] mColliders;

    //[SerializeField]
    private float mChannelingTime;
    //[SerializeField]
    private float mCastingTime;
    [SerializeField, Tooltip("막타 딜")]
    private float m_LastDamage;
    [SerializeField, Tooltip("걍 딜")]
    private float m_Damage;
    [SerializeField, Tooltip("폭")]
    private float m_Width;
    [SerializeField, Tooltip("세로")]
    private float m_Height;
    [SerializeField]
    private bool m_b_CanBeInterruptedByDash;

    private bool mbIsChanneling;
    private float mFirstCoolTime;
    private float mSecondCoolTime;
    private byte mPunchedCount;

    STakeDamageParams m_DamageInfo;

    private const float ATTACK_INTERVAL = 1.7f / 10.0f;

    public bool IsChanneling { get { return mbIsChanneling; } set { mbIsChanneling = value; } }
    public float ChannelingTime { get { return mChannelingTime; } set { mChannelingTime = value; } }
    public float ElapsedChannelingTime { get; set; }
    public bool CanMove { get; set; }
    public float Width { get { return m_Width; } }
    public float Height { get { return m_Height; } }
    private float ElapsedAttackTime { get; set; }

    public float LastDamage { get { return m_LastDamage; } }
    public float Damage { get { return m_Damage; } }
    private float FirstCoolTime { get { return mFirstCoolTime; } }
    private float SecondCoolTime { get { return mSecondCoolTime; } }
    private byte PunchedCount { get { return mPunchedCount; } set { mPunchedCount = value; } }
    public bool CanBeInterruptedByDash { get => m_b_CanBeInterruptedByDash; set => m_b_CanBeInterruptedByDash = value; }

    public /*override*/ void Casting()
    {
        
    }

    public void ChannelingStart(LivingEntity castedBy)
    {
        if (IsChanneling)
        {
            //채널링 상태에서 cast가 호출됐다는것은 채널링이 끝나기 전에 다시 한번 눌렀다는 뜻            
            ChannelingEnd(castedBy);
        }
        else
        {
            castedBy.GetComponent<PlayerSkillManager>().IsChanneling = true;
            IsChanneling = true;
            ElapsedChannelingTime = 0.0f;
            ElapsedAttackTime = ATTACK_INTERVAL;
            PunchedCount = 0;
        }
    }

    public void ChannelingEnd(LivingEntity castedBy)
    {
        //castedBy.GetComponent<PlayerSkillManager>().FinishChanneling();
        IsChanneling = false;
        float time = ElapsedChannelingTime / ChannelingTime;
    }

    public void Channeling(LivingEntity castedBy)
    {        
        //castedBy.GetAttackSpeed();
        if (ElapsedChannelingTime >= 0.3f)
        {
            if (ElapsedAttackTime >= ATTACK_INTERVAL)
            {
                //Logger.Log("attack Time : " + ElapsedAttackTime + " // " + ATTACK_INTERVAL);
                ElapsedAttackTime -= ATTACK_INTERVAL;

                Vector3 half = new Vector3(Width / 2.0f, 1.0f, Height / 2.0f);
                Vector3 center = castedBy.transform.forward * 0.5f + castedBy.transform.forward * Height * 0.5f + castedBy.transform.position;
                Physics.OverlapBoxNonAlloc(center, half, mColliders, Quaternion.LookRotation(castedBy.transform.forward, castedBy.transform.up), Mask);

                m_DamageInfo.causedBy = castedBy.gameObject;

                if (PunchedCount == 9)
                {
                    m_DamageInfo.damageAmount = castedBy.GetDamageAmount(LastDamage);
                }
                else
                {
                    m_DamageInfo.damageAmount = castedBy.GetDamageAmount(Damage);
                }

                for (int index = 0; index < mColliders.Length; index++)
                {
                    if (mColliders[index] != null)
                    {
                        mColliders[index].GetComponent<LivingEntity>().TakeDamage(m_DamageInfo);
                    }
                    else
                    {
                        break;
                    }
                }

                float dx = Random.Range(-1.0f, 1.0f);
                float dy = Random.Range(-1.0f, 1.0f);

                Vector3 effectPos = castedBy.transform.position;
                Quaternion effectRot = castedBy.transform.rotation;
                effectPos.x += dx;
                effectPos.y += dy;

                //Logger.Log($"{effectPos.position} {castedBy.transform.position}");

                PlayEffect(EAnimState.ChannelingStart, effectPos, effectRot);
                PunchedCount++;
                Logger.Log("Punch Count : " + PunchedCount);
                ElapsedAttackTime += Time.deltaTime;
            }
            else
            {
                ElapsedAttackTime += Time.deltaTime;
            }
        }        
        ElapsedChannelingTime += Time.deltaTime;
    }

    public override void Init()
    {
        Name = "name_HundredFists";
        Description = "desc_HundredFists";

        mColliders = new Collider[10];

        IsChanneling = false;

        ElapsedAttackTime = 0.0f;
        PunchedCount = 0;

        var info = GetAnimInfo(EAnimState.Casting);
        mCastingTime = info.AnimTime;

        float chanTime = 0.0f;
        info = GetAnimInfo(EAnimState.ChannelingStart);
        chanTime += info.AnimTime;
        info = GetAnimInfo(EAnimState.Channeling);
        chanTime += info.AnimTime;
        info = GetAnimInfo(EAnimState.ChannelingEnd);
        chanTime += info.AnimTime;

        mChannelingTime = chanTime;

        m_DamageInfo.tag = Tag;
    }


}
