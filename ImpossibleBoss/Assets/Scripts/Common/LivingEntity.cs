using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public enum EBuffFactorType
{
    Damage,
    Armor,
    MoveSpeed,
    CoolDown,
    Regen,
    RegenTime,
    AttackSpeed,
    Health,
    MaxHealth,
    DamageAmplification,
    DamageResistance,
    Invincible    
}

//30글자 * 32 * 8 = 7.5KByte
//나머지 필드까지 합치면 7.56KByte
public abstract partial class LivingEntity : MonoBehaviour, IDamageable
{
    [SerializeField]
    private string mName;

    private float mHealth;
    private float mHealthRegen = 1.0f;
    private float mHealthRegenTime = 5.0f;
    private float mHealthRegenElapse = 0.0f;
    [Header("Health")]
    public float InitHealth;
    public float InitHealthRegen;
    public float InitHealthRegenTime;

    [Header("Shield"), SerializeField, VisibleOnly, Tooltip("보호막")]
    private float m_Shield;

    private float mArmor;
    [Header("Armor")]
    public float InitArmor;
    [SerializeField, Tooltip("영구적용 뎀감")]
    private float m_InitDamageResistance;

    private float mMoveSpeed;
    [Header("MoveSpeed")]
    public float InitMoveSpeed;

    private float mAttackSpeed = 1.0f;
    [Header("AttackSpeed")]
    public float InitAttackSpeed = 1.0f;

    private float mCoolDown = 1.0f;

    private float mDamagePercent = 100.0f;
    private float mDamagePlus = 0.0f;

    private float mDamageResistance = 0.0f;
    private float mDamageAmplificationPercent = 100.0f;

    private float mArmorPercent = 100.0f;
    private float mArmorPlus = 0.0f;

    private float mMoveSpeedPercent = 100.0f;
    private float mMoveSpeedPlus = 0.0f;

    private float mAttackSpeedPercent = 100.0f;
    private float mAttackSpeedPlus = 0.0f;

    private float mHealthRegenPercent = 100.0f;
    private float mHealthRegenPlus = 0.0f;

    private float mHealthRegenTimePercent = 100.0f;
    private float mHealthRegenTimePlus = 0.0f;

    private float mCoolDownPercent = 100.0f;
    private float mCoolDownPlus = 0.0f;

    [SerializeField]
    private bool mbIsInvincible = false;

    private bool mbHasCCImmunity;
    private bool mbHasKnockBackImmun;

    //private float mEndTimeInterruptsController;
    private float mRemainingTimeInterruptsController;
    private bool mbCanControl = true;

    private bool mbIsPaused;

    private bool mbIsDead;
    #region KnockBack    
    private static float mKnockBackSpeed = 1.0f;
    //private float mElapsedKnockBackTime;
    //private float mKnockBackEndTime;
    private float mRemainingTimeKnockBack;
    private Vector3 mKnockBackDir;
    private bool mbIsKnockBack;
    #endregion

    #region Events
    public event EventHandler<TakeDamageEventArgs> TakeDamageEvent;
    public event EventHandler<HealthChangeEventArgs> HealthChangeEvent;
    public event EventHandler<KnockBackEventArgs> KnockBackEvent;
    public event EventHandler<CCEventArgs> CCEvent;
    #endregion
    protected Action NotifyChangeHealth;
    protected Action OnDeath;
    
    [Tooltip("0.5 = 2배 오래 (1s to 2s) // 2 = 2배 빠르게 (1s to 0.5s)")]
    public float m_KnockBackTimeFactor; 
    [Tooltip("2 = 2배 멀리 // 짧은시간 + 더 멀리 = 짧고 빠르게")]
    public float m_KnockBackSpeed;


    public string Name { get { return mName; } private set { mName = value; } }

    //public float EndTimeInterruptsController { get { return mEndTimeInterruptsController; } set { mEndTimeInterruptsController = value; } }
    public float RemainingTimeInterruptsController { get { return mRemainingTimeInterruptsController; } set { mRemainingTimeInterruptsController = value; } }
    public bool CanControl { get { return mbCanControl; } set { mbCanControl = value; } }

    protected float CoolDownPercent { get { return mCoolDownPercent; } set { mCoolDownPercent = value; } }
    protected float CoolDownPlus { get { return mCoolDownPlus; } set { mCoolDownPlus = value; } }

    protected float MoveSpeedPercent { get { return mMoveSpeedPercent; }  set { mMoveSpeedPercent = value; } }
    protected float MoveSpeedPlus { get { return mMoveSpeedPlus; }  set { mMoveSpeedPlus = value; } }
    protected float DamagePercent { get { return mDamagePercent; }  set { mDamagePercent = value; } }
    protected float DamagePlus { get { return mDamagePlus; }  set { mDamagePlus = value; } }
    
    protected float InitDamageResistance { get { return m_InitDamageResistance * 0.01f; } set { m_InitDamageResistance = value; } }
    protected float DamageResistance { get { return mDamageResistance; } set { mDamageResistance = value; } }
    protected float DamageAmplificationPercent { get { return mDamageAmplificationPercent; } set { mDamageAmplificationPercent = value; } }
    
    protected float ArmorPercent { get { return mArmorPercent; } set { mArmorPercent = value; } }
    protected float ArmorPlus { get { return mArmorPlus; }  set { mArmorPlus = value; } }
    
    protected float HealthRegenPercent { get { return mHealthRegenPercent; } set { mHealthRegenPercent = value; } }
    protected float HealthRegenPlus { get { return mHealthRegenPlus; }  set { mHealthRegenPercent = value; } }
    
    protected float HealthRegenTimePercent { get { return mHealthRegenTimePercent; } set { mHealthRegenTimePercent = value; } }
    protected float HealthRegenTimePlus { get { return mHealthRegenTimePlus; } set { mHealthRegenTimePercent = value; } }
    
    protected float AttackSpeedPercent { get { return mAttackSpeedPercent; }  set{ mAttackSpeedPercent = value; } }
    protected float AttackSpeedPlus { get { return mAttackSpeedPlus; }  set { mAttackSpeedPercent = value; } }

    protected float Shield { get { return m_Shield; } set { m_Shield = value; } }

    // 공격속도 : 캐스팅, 채널링 모두 포함함
    protected float AttackSpeed { get { return mAttackSpeed; } set { mAttackSpeed = value; } }
    protected float Armor { get { return mArmor; } set { mArmor = value; } }
    protected float CoolDown { get { return mCoolDown; } set { mCoolDown = value; } }
    protected float MoveSpeed { get { return mMoveSpeed; } set { mMoveSpeed = value; } }
    protected float Health { get { return mHealth; } /*set { mHealth = value; }*/ }
    protected float HealthRegen { get { return mHealthRegen; } set { mHealthRegen = value; } }
    protected float HealthRegenTime { get { return mHealthRegenTime; } set { mHealthRegenTime = value; } }

    protected bool IsInvincible { get { return mbIsInvincible; } set { mbIsInvincible = value; } }
    protected bool HasCCImmunity { get { return mbHasCCImmunity; } set { mbHasCCImmunity = value; } }

    protected bool IsPaused { get { return mbIsPaused; } set { mbIsPaused = value; } }
    public bool IsDead { get { return mbIsDead; } protected set { mbIsDead = value; } }
    public bool HasKnockBackImmun { get { return mbHasKnockBackImmun; } set { mbHasKnockBackImmun = value; } }
    #region Taken KnockBack Property
    protected float KnockBackSpeed { get { return mKnockBackSpeed; } }
    public float RemainingTimeKnockBack { get { return mRemainingTimeKnockBack; } set { mRemainingTimeKnockBack = value; } }
    //protected float KnockBackEndTime { get { return mKnockBackEndTime; } set { mKnockBackEndTime = value; } }
    //protected float ElapsedKnockBackTime { get { return mElapsedKnockBackTime; } set { mElapsedKnockBackTime = value; } }
    public Vector3 KnockBackDir { get { return mKnockBackDir; } set { mKnockBackDir = value; } }
    public bool IsKnockBack { get { return mbIsKnockBack; } protected set { mbIsKnockBack = value; } }
    #endregion

    public void AddHealth(float addAmount)
    {
        SetHealth(Health + addAmount);
    }
    protected void SetHealth(float newHealth)
    {
        mHealth = ClampHealth(newHealth); 
        if (NotifyChangeHealth != null)
        {
            NotifyChangeHealth.Invoke();
        }            
        OnHealthChange(Health, InitHealth);
    }

    private void OnTakeDamage(STakeDamageParams damageParams)
    {
        EventHandler<TakeDamageEventArgs> temp = TakeDamageEvent;
        if (temp != null)
        {
            temp(this, new TakeDamageEventArgs(damageParams));
        }
    }

    private void OnHealthChange(float health, float initHealth)
    {
        EventHandler<HealthChangeEventArgs> temp = HealthChangeEvent;
        if (temp != null)
        {
            temp(this, new HealthChangeEventArgs(health, initHealth));
        }
    }

    private void OnKnockBack(Vector3 dir, float power, bool knockBackState)
    {
        Logger.Log($"Knock back : {knockBackState}");
        EventHandler<KnockBackEventArgs> temp = KnockBackEvent;
        if (temp != null)
        {
            temp(this, new KnockBackEventArgs(dir, power, knockBackState));
        }
    }    

    private void OnCC(ECC cc)
    {
        EventHandler<CCEventArgs> temp = CCEvent;
        if (temp != null)
        {
            temp(this, new CCEventArgs(cc));
        }
    }

    private float ClampHealth(float newHealth)
    {
        if (newHealth <= 0.0f)
        {
            return 0.0f;
        }
        if (newHealth >= InitHealth)
        {
            return InitHealth;
        }
        return newHealth;
    }
    
    public void AddShield(float amountShield)
    {
        Shield += amountShield;        
    }

    public float GetDamageAmount(float skillDamage) { return (skillDamage * (DamagePercent * 0.01f)) * GetDamageAmplification();/*return (skillDamage * (DamagePercent * 0.01f)) + DamagePlus;*/ }
    public float GetArmorAmount() { return (Armor * (ArmorPercent * 0.01f));/*return (Armor * (ArmorPercent * 0.01f)) + ArmorPlus;*/ }
    public float GetMoveSpeed() { return (MoveSpeed * (MoveSpeedPercent * 0.01f)) + MoveSpeedPlus; }
    public float GetCoolDown() { return (CoolDown * (CoolDownPercent * 0.01f)) + CoolDownPlus; }
    public float GetAttackSpeed() { return (AttackSpeed * (AttackSpeedPercent * 0.01f)) + AttackSpeedPlus; }
    public float GetHealthRegen() { return (HealthRegen * (HealthRegenPercent * 0.01f)) + HealthRegenPlus; }
    public float GetHealthRegenTime() { return (HealthRegenTime * (HealthRegenTimePercent * 0.01f)) + HealthRegenTimePlus; }
    public float GetDamageResistance() { return DamageResistance * 0.01f; }
    public float GetDamageAmplification() { return DamageAmplificationPercent * 0.01f; }

    protected virtual void Die()
    {
        IsDead = true;
        RemoveAllBuff();
    }

    private void Stun(ECC cc, float second)
    {
        //만약 통제불가 디버프별로 이펙트를 다르게 줄거면 여기서
        //switch ()
        //{

        //}
        InterruptsController(cc, second);
    }

    private void KnockBack(Vector3 dir, float second)
    {
        if (HasKnockBackImmun == false)
        {
            IsKnockBack = true;            
            Stun(ECC.KnockBack, second);
            RemainingTimeKnockBack = second;
            //KnockBackEndTime = Time.time + (second * 0.1f);
            //ElapsedKnockBackTime = 0.0f;
            //KnockBackDir = dir;
            //gameObject.GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;
            //gameObject.GetComponentInChildren<Rigidbody>().AddForce(dir * second * 2.0f, ForceMode.VelocityChange);        
            OnKnockBack(dir, second, true);
        }        
    }

    public void RecoveryController()
    {
        //CanControl = true;
        RemainingTimeInterruptsController = 0.0f;
    }

    public void InterruptsController(ECC cc, float second)
    {
        CanControl = false;
        //EndTimeInterruptsController = Time.time + second;
        if (RemainingTimeInterruptsController < second)
        {
            RemainingTimeInterruptsController = second;
        }
        
        //OnCC(cc);
    }

    public virtual void TakeDamage(STakeDamageParams takeDamageParams)
    {
        if (!IsInvincible)
        {
            if (Shield > takeDamageParams.damageAmount)
            {
                Shield -= takeDamageParams.damageAmount;
            }
            else
            {
                takeDamageParams.damageAmount -= Shield;
                if (GetArmorAmount() < takeDamageParams.damageAmount)
                {
                    float damage = takeDamageParams.damageAmount - GetArmorAmount();

                    SetHealth(Health - (damage *= Mathf.Clamp(1.0f - (GetDamageResistance() + InitDamageResistance), 0.0f, 100.0f)) );
                    takeDamageParams.damageAmount = damage;
                }
            }

            //대쉬중 맞거나, 매혹같이 맞으면 풀리는 스킬들을 위해 true
            //이후, cc판정해서 필요하면 다시 CanControl = false
            //if (CanControl == false)
            //{
            //    switch ()
            //    {

            //    }
            //}
            

            //CC기 적용
            if (HasCCImmunity)
            {
                //CC면역 글자 띄워주기?
            }
            else
            {
                switch (takeDamageParams.cc)
                {
                    case ECC.None: break;
                    case ECC.Stun: break;
                    case ECC.ElectricShock:break;
                    case ECC.KnockBack: KnockBack((transform.position - takeDamageParams.causedBy.transform.position).normalized, takeDamageParams.ccAmount); break;
                    default: throw new System.ArgumentOutOfRangeException(nameof(takeDamageParams.cc));
                }
            }
            //Logger.Log("LivingEntity TakeDamage");
            //피해 입으면 알림
            OnTakeDamage(takeDamageParams);

            //죽음 처리
            if (Health <= 0.0f)
            {
                Debug.Assert(OnDeath != null, string.Format("{0} OnDeath is null", name));
                OnDeath.Invoke();
            }
        }        
    }

    protected virtual void Start()
    {
        MoveSpeed = InitMoveSpeed;
        IsInvincible = false;
        CanControl = true;
    }

    protected virtual void Update()
    {
        if(mHealthRegenElapse >= HealthRegenTime)
        {
            SetHealth(Health + HealthRegen);
            mHealthRegenElapse = 0.0f;
        }
        else
        {
            mHealthRegenElapse += Time.deltaTime;
        }

        if (IsKnockBack)
        {
            if ( RemainingTimeKnockBack <= 0.0f)
            {
                IsKnockBack = false;
                OnKnockBack(Vector3.zero, 0.0f, false);
                //CanControl = true;
            }
            else
            {
                RemainingTimeKnockBack -= Time.deltaTime * m_KnockBackTimeFactor;
            }
        }

        if (RemainingTimeInterruptsController <= 0.0f)
        {
            CanControl = true;
        }
        else
        {
            RemainingTimeInterruptsController -= Time.deltaTime;
        }
    }
}
