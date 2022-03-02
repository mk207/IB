using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



[DefaultExecutionOrder(0)]
public class BossInformation : LivingEntity
{    
    public EventHandler<BossDieEventArgs> BossDieEvent;
    public EventHandler<EnergyChangeEventArgs> EnergyChangeEvent;
    [SerializeField]
    private Slider m_sliHealth;
    [SerializeField]
    private Text m_txtHealthPercent;
    [SerializeField]
    private Slider m_sliEnergy;
    private float mEnergy;
    [SerializeField]
    private float m_InitEnergy;
    private string mHealthFormat;

    private ushort mUnlockID;

    private int mDifficulty;
    
    protected int Difficult { get { return mDifficulty; } }

    private List<PlayerInformation> mPlayers;

    private List<PlayerInformation> Players { get { return mPlayers; } }

    protected float Energy { get { return mEnergy; } }
    protected void SetEnergy(float newEnergy)
    {
        mEnergy = ClampEnergy(newEnergy);
        ChangeEnergy();
        //if (NotifyChangeHealth != null)
        //{
        //    NotifyChangeHealth.Invoke();
        //}
        OnEnergyChange(Energy, InitEnergy);
    }
    protected float InitEnergy { get { return m_InitEnergy; } }
    private ushort UnlockID { get { return mUnlockID; } set { mUnlockID = value; } }
    private Slider sliHealth { get { return m_sliHealth; } }
    private Slider sliEnergy { get { return m_sliEnergy; } }
    private Text txtHealthPercent { get { return m_txtHealthPercent; } }
    private string HealthFormat { get { return mHealthFormat; } }
    protected override void SetInvincible(bool isRegister)
    {
        base.SetInvincible(isRegister);
        //보스 체력바 변경
    }
    protected void SetInitEnergy(float init)
    {
        m_InitEnergy = init;
        sliEnergy.maxValue = InitEnergy;
    }
    private float ClampEnergy(float newEnergy)
    {
        if (newEnergy <= 0.0f)
        {
            return 0.0f;
        }
        if (newEnergy >= InitEnergy)
        {
            return InitEnergy;
        }
        return newEnergy;
    }

    protected virtual void ExterminatePlayers(bool ignoreInvincible)
    {
        for (int index = 0; index < Players.Count; index++)
        {
            Players[index].InstantDeath(ignoreInvincible);
        }
    }

    public override void TakeDamage(STakeDamageParams takeDamageParams)
    {
        //테스트용 무적
        //if (Health <= 500) return;
        //Health = Health - (uint)amount;
        base.TakeDamage(takeDamageParams);

    }
    private void OnEnergyChange(float energy, float initEnergy)
    {
        EventHandler<EnergyChangeEventArgs> temp = EnergyChangeEvent;
        if (temp != null)
        {
            temp(this, new EnergyChangeEventArgs(energy, initEnergy));
        }
    }

    void ChangeHealth()
    {
        sliHealth.value = Health;
        txtHealthPercent.text = string.Format(HealthFormat, Health / InitHealth * 100.0f);
    }

    void ChangeEnergy()
    {
        sliEnergy.value = Energy;        
    }

    protected override void Die()
    {
        
        
        //gameObject.SetActive(false);
        Logger.Log("Boss die");

        if (IsDead == false)
        {
            base.Die();
            EventHandler<BossDieEventArgs> temp = BossDieEvent;
            if (temp != null)
            {
                temp(this, new BossDieEventArgs(UnlockID));
            }
        }       
    }

    protected override void Awake()
    {
        mHealthFormat = "{0:0}%";
        //mHealthMultiFormat = " X {0:0}";
        m_sliHealth = GameObject.Find("sliBossHealth").GetComponent<Slider>();
        m_txtHealthPercent = GameObject.Find("txtHealthPercent").GetComponent<Text>();
        m_sliEnergy = GameObject.Find("sliBossEnergy").GetComponent<Slider>();
        base.Awake();
        mPlayers = new List<PlayerInformation>(4);
        foreach (var player in FindObjectsOfType<PlayerInformation>())
        {
            Players.Add(player);
        }
        NotifyChangeHealth = ChangeHealth;
        SetHealth(InitHealth);
        Armor = InitArmor;
        //OnDeath = Die;
        

        sliHealth.maxValue = InitHealth;
        sliHealth.value = InitHealth;
        sliEnergy.maxValue = InitEnergy;
        sliEnergy.value = 0.0f;



        PlayerData PD = FindObjectOfType<PlayerData>();
        mDifficulty = (int)PD.Difficulty;

        //보스는 몸체에 보여주는게 아니라 보스체력바 밑에 따로 보여줄 것임
        //그러므로 "보스만" 따로 Owner할당
        //GameObject.Find("Boss Health Panel").GetComponent<BuffIcon>().SetOwner(this);
        //m_sliHealth.maxValue = HEALTH_BAR_MAX_VALUE;
        //m_sliHealth.value = HEALTH_BAR_MAX_VALUE;
        //ChangeHealthMultiText();

        //testDie = false;

    }

    //protected override void Update()
    //{
    //    //if (testDie)
    //    //{
    //    //    testOutlineWidth += (Time.deltaTime * 0.25f);
    //    //    GetComponent<MeshRenderer>().material.SetFloat("_Dp", testOutlineWidth);
    //    //}
    //    base.Update();
    //}
}
