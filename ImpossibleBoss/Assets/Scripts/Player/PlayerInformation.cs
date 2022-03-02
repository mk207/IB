using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInformation : LivingEntity
{
    

    public Text HealthText;
    public Slider HealthSlider;
    [SerializeField]
    private GameObject m_PlayerBuffList;

    [SerializeField]
    private AudioClip testHitAudioClip;
    private AudioSource mAudioSource;    

    private GameObject PlayerBuffList { get { return m_PlayerBuffList; } }
    private AudioSource AudioSource { get { return mAudioSource; } set { mAudioSource = value; } }

    private byte mPlayerID;
    private float mTotalDamage;

    public byte PlayerID { get { return mPlayerID; } set { mPlayerID = value; } }
    private float TotalDamage { get { return mTotalDamage; } set { mTotalDamage = value; } }

    protected override void SetInvincible(bool isRegister)
    {
        base.SetInvincible(isRegister);
        //플레이어 체력바 변경
    }

    //즉사
    public void InstantDeath(bool ignoreInvincible)
    {
        if (ignoreInvincible || IsInvincible == false)
        {
            Die();
        }
    }

    public override void TakeDamage(STakeDamageParams takeDamageParams)
    {
        //Health = Health - (uint)amount;
        base.TakeDamage(takeDamageParams);
        Logger.Log($"takeDamage caused by : {takeDamageParams.causedBy} //  amount : {takeDamageParams.damageAmount}");
        AudioSource.clip = testHitAudioClip;
        AudioSource.Play();
        //Logger.Log(string.Format("Player TakeDamege {0} // {1}", amount, Health));
    }

    public void SetBuffIcon(Transform buffTransform)
    {
        buffTransform.SetParent(PlayerBuffList.transform);
    }

    private void ChangeHealthUI()
    {
        HealthText.text = string.Format("{0} / {1} ", Health, InitHealth);
        HealthSlider.value = Health;
    }

    protected override void Awake()
    {
        base.Awake();
        NotifyChangeHealth = ChangeHealthUI;
        HealthSlider.maxValue = InitHealth;
        SetHealth(InitHealth);
        Armor = InitArmor;        
        //OnDeath = Die;
        AudioSource = GetComponent<AudioSource>();                
    }

    protected override void Start()
    {
        base.Start();
        FindObjectOfType<GameManager>().RegisterPlayer(0);
        //FindObjectOfType<GameManager>().RegisterPlayer(PlayerID);
    }

    protected override void Die()
    {
        base.Die();

        FindObjectOfType<GameManager>().PlayerDie(PlayerID);
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().enabled = false;
        IsDead = true;
        //GetComponent<PlayerInputManager>().RemoveInputEvent();
    }
}
