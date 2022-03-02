using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpearCharge", menuName = "Skills/SpearCharge")]
public class SpearCharge : Skill, IChanneling, IBox, ISkillChain
{
    private PlayerMovement mMovement;
    private Collider[] mColliders;
    [SerializeField]
    private float m_ChannelingTime;
    //[SerializeField, Tooltip("채널링중 무빙가능?")]
    //private bool m_b_CanMove;
    [SerializeField]
    private float m_MaxDamage;
    [SerializeField]
    private float m_MinDamage;
    [SerializeField, Tooltip(@"최대 이속 ""비율"" ")]
    private float m_MaxMovingSpeed;
    [SerializeField, Tooltip("최소 이속 비율")]
    private float m_MinMovingSpeed;
    [SerializeField, Tooltip("1초당 최대 회전 각도")]
    private float m_MaxRotationAngle;
    [SerializeField, Tooltip("최대 폭")]
    private float m_MaxWidth;
    [SerializeField, Tooltip("최소 폭")]
    private float m_MinWidth;
    [SerializeField, Tooltip("최대 길이")]
    private float m_MaxHeight;
    [SerializeField, Tooltip("최소 길이")]
    private float m_MinHeight;
    [SerializeField]
    private AnimationCurve m_Curve;
    [SerializeField]
    private bool m_b_CanBeInterruptedByDash;

    private float mFirstCoolTime;
    private float mSecondCoolTime;
    
    private bool mbIsChanneling;

    STakeDamageParams m_DamageInfo;

    private float mElapsedChannelingTime;


    public float Width { get; set; }

    public float Height { get; set; }
    //private STakeDamageParams DamageInfo { get => m_DamageInfo; set => m_DamageInfo = value; }
    private PlayerMovement Movement { get => mMovement; set => mMovement = value; }
    public float ChannelingTime { get => m_ChannelingTime; set => m_ChannelingTime = value; }
    public float ElapsedChannelingTime { get => mElapsedChannelingTime; set => mElapsedChannelingTime = value; }
    //public bool CanMove { get => m_b_CanMove; set => m_b_CanMove = value; }
    public float MaxMovingSpeed { get => m_MaxMovingSpeed; }
    public float MinMovingSpeed { get => m_MinMovingSpeed; }
    public float MaxDamage { get => m_MaxDamage;}
    public float MinDamage { get => m_MinDamage;}
    public float MaxWidth { get => m_MaxWidth; }
    public float MinWidth { get => m_MinWidth; }
    public float MaxHeight { get => m_MaxHeight;}
    public float MinHeight { get => m_MinHeight;}
    public AnimationCurve Curve { get => m_Curve; }
    public bool IsChanneling { get => mbIsChanneling; set => mbIsChanneling = value; }
    public float FirstCoolTime { get => mFirstCoolTime; set => mFirstCoolTime = value; }
    public float SecondCoolTime { get => mSecondCoolTime; set => mSecondCoolTime = value; }
    private float MaxRotationAngle { get => m_MaxRotationAngle; }
    public float TimeLimit { get; set; }
    public bool IsChaining { get; set; }
    public bool CanBeInterruptedByDash { get => m_b_CanBeInterruptedByDash; set => m_b_CanBeInterruptedByDash = value; }


    //public override void Casting()
    //{

    //}

    public void Channeling(LivingEntity castedBy)
    {
        if(IsChanneling)
        {
            ElapsedChannelingTime += Time.deltaTime;
            Movement.ChargeSpeedRate = Mathf.Lerp(MinMovingSpeed, MaxMovingSpeed, Curve.Evaluate(ElapsedChannelingTime / ChannelingTime));
            SetEffectTransform(castedBy.transform);
            //if (ElapsedChannelingTime >= ChannelingTime)
            //{
            //    ChannelingEnd(castedBy);
            //}
        }
    }

    public override void Init()
    {
        Name = "name_SpearCharge";
        Description = "desc_SpearCharge";

        mFirstCoolTime = 0.3f;
        mSecondCoolTime = CoolDown;

        mColliders = new Collider[10];
        IsChanneling = false;
        IsChaining = false;

        TimeLimit = GetAnimInfo(EAnimState.ChannelingStart).AnimTime;
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
            CoolDown = FirstCoolTime;
            Movement = castedBy.GetComponent<PlayerMovement>();
            castedBy.GetComponent<PlayerSkillManager>().IsChanneling = true;
            IsChanneling = true;
            IsChaining = true;
            Movement.StartCharge(Curve.Evaluate(0.0f), MaxRotationAngle);
            ElapsedChannelingTime = 0.0f;
            PlayEffect(EAnimState.ChannelingStart, castedBy.transform.position, castedBy.transform.rotation);

            for (int index = 0; index < 10; index++)
            {
                mColliders[index] = null;
            }
            
        }
    }

    public void ChannelingEnd(LivingEntity castedBy)
    {
        CoolDown = SecondCoolTime;
        IsChanneling = false;
        IsChaining = false;
        Movement.EndCharge();
        float time = ElapsedChannelingTime / ChannelingTime;

        float value = Curve.Evaluate(time);
        Width = Mathf.Lerp(MinWidth, MaxWidth, value);
        Height = Mathf.Lerp(MinHeight, MaxHeight, value);
        Vector3 forward = castedBy.transform.forward;
        Vector3 up = castedBy.transform.up;

        Vector3 center = forward * Height * 0.5f + castedBy.transform.position;
        Vector3 halfExtents = new Vector3(Width * 0.5f, 5.0f, Height * 0.5f);
        //center = castedBy.transform.TransformPoint(forward * height * 0.5f + castedBy.transform.position);
        Physics.OverlapBoxNonAlloc(center, halfExtents, mColliders, Quaternion.LookRotation(forward, up), Mask);
        Debug.DrawLine(castedBy.transform.position, center, Color.red, 5.0f);

        m_DamageInfo.causedBy = castedBy.gameObject;
        m_DamageInfo.damageAmount = Mathf.Lerp(MinDamage, MaxDamage, value);
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
        Effect.gameObject.SetActive(false);
    }

    public void TimeOut(LivingEntity castedBy)
    {
        ChannelingEnd(castedBy);
    }
}
