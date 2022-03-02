using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum EEventType
{
    Health,
    TakeDamage,
    ElapsedTime
}

public class BuffCondition
{
    private EventHandlerList mEventList = new EventHandlerList();
    private BuffInfo mOwnerBuff;
    /*
     * 버프 내부쿨
     * 버프 지속시간은 BuffInfo에서 관리하고
     * 버프 내부쿨은 조건의 한 종류로서 BuffCondition에서 관리함
     * ex)패시브 내부쿨
     */
    private float mCoolTime;
    private float mEndCoolTime;
    private Dictionary<EEventType, bool> mConditions;
    private bool mbCanTrigger;
    private bool mbIsPassive;

    #region HealthChange (버프타겟의 체력이 변했을 때 버프 발동 시도)
    private Func<object, HealthChangeEventArgs, bool> mHealthChangeDelegate;

    private void OnHealthChange(object sender, HealthChangeEventArgs e)
    {
        //mConditions[EEventType.Health] = mHealthChangeDelegate(sender, e);
        SetCondition(EEventType.Health, mHealthChangeDelegate(sender, e));
    }

    public Func<object, HealthChangeEventArgs, bool> HealthChangeDelegate
    {
        set
        {
            mHealthChangeDelegate = value;
            OwnerBuff.Target.HealthChangeEvent += OnHealthChange;
        }
    }
    #endregion
    #region TakeDamage (버프타겟이 데미지를 받았을 때 버프 발동 시도)
    private Func<object, TakeDamageEventArgs, bool> mTakeDamageDelegate;

    private void OnTakeDamage(object sender, TakeDamageEventArgs e)
    {        
        SetCondition(EEventType.TakeDamage, mTakeDamageDelegate(sender, e));
    }

    public Func<object, TakeDamageEventArgs, bool> TakeDamageDelegate
    {
        set
        {
            mTakeDamageDelegate = value;
            OwnerBuff.Target.TakeDamageEvent += OnTakeDamage;
        }
    }
    #endregion
    #region ElapsedTime
    private void OnElapsedTime(object sender, ElapsedTimeEventArgs e)
    {
        if (EndCoolTime > Time.time)
        {
            SetCondition(EEventType.ElapsedTime, false);
        }
        else
        {
            SetCondition(EEventType.ElapsedTime, true);
        }
    }
    
    public void SetCoolTime(float newCoolTime, bool willCoolDown)
    {

        CoolTime = newCoolTime;
        if (willCoolDown)
        {
            EndCoolTime = Time.time + CoolTime;
            OwnerBuff.Slider.value = CoolTime;
        }
        else
        {
            EndCoolTime = Time.time;
            OwnerBuff.Slider.value = 0.0f;
        }
        OwnerBuff.ElapsedTimeEvent -= OnElapsedTime;
        OwnerBuff.ElapsedTimeEvent += OnElapsedTime;

    }
    #endregion


    public void SetCondition(EEventType type, bool state)
    {
        mConditions[type] = state;
        TryToTriggerBuff();
    }

    public BuffInfo OwnerBuff { get { return mOwnerBuff; } private set { mOwnerBuff = value; } }
    public float CoolTime { get { return mCoolTime; } private set { mCoolTime = value; } }
    private bool CanTrigger { get { return mbCanTrigger; } set { mbCanTrigger = value; /*OwnerBuff.TriggerBuff();*/ } }
    public bool IsPassive { get { return mbIsPassive; } set { mbIsPassive = value; } }
    private float EndCoolTime { get { return mEndCoolTime; } set { mEndCoolTime = value; } }

    public BuffCondition(BuffInfo owner)
    {
        OwnerBuff = owner;
        mbCanTrigger = true;        
        mConditions = new Dictionary<EEventType, bool>(System.Enum.GetValues(typeof(EEventType)).Length);

    }

    public void UpdateEndCoolTime()
    {
        EndCoolTime = Time.time + CoolTime;
    }

    public void TryToTriggerBuff()
    {
        CanTrigger = true;

        //if (EndCoolTime > CoolTime)
        //{
        //    CanTrigger &= true;
        //}
        //else
        //{
        //    CanTrigger &= false;
        //}

        foreach (var condition in mConditions)
        {
            CanTrigger &= condition.Value;
        }

        if (CanTrigger)
        {
            OwnerBuff.TriggerBuff();
        }
    }

    public void Clear()
    {
        CoolTime = 0.0f;
        EndCoolTime = 0.0f;
        mConditions.Clear();
        CanTrigger = true;
        IsPassive = false;
        if (mTakeDamageDelegate != null)
        {
            OwnerBuff.Target.TakeDamageEvent -= OnTakeDamage;
            mTakeDamageDelegate = null;
        }

        if (mHealthChangeDelegate != null)
        {
            OwnerBuff.Target.HealthChangeEvent -= OnHealthChange;
            mHealthChangeDelegate = null;
        }
    }
}

public class BuffInfo
{
    private BuffCondition mCondition;
    private Action<LivingEntity, LivingEntity, BuffInfo> mTriggerBuffDelegate;

    private GameObject mBuffIcon;
    private UnityEngine.UI.Slider mSlider;
    private LivingEntity mTarget;
    private LivingEntity mCastedBy;
    private float mDurationTime;
    private float mTimer;
    private float mStackScale;
    private float mDOTInterval;
    private float mDOTElapsedTime;
    private byte mStack;
    private bool mbIsCanceled = false;
    private bool mbIsRenew = false;
    private bool mbIsPendingKill = false;
    private bool mbHasCondition = false;
    private bool mbIsPassive = false;

    public Action<BuffInfo> remove;
    public Action Rewrite;
    public Action<LivingEntity, LivingEntity> DOT;
    public Action<BuffInfo> LazyBuff;

    public List<EventArgs> eventList;
    public event EventHandler<ElapsedTimeEventArgs> ElapsedTimeEvent;


    //public List<Action<object, EventArgs>> EventDelegateList;
    //public List<Action<object, HealthChangeEventArgs>> EventDelegateList;

    public void IncreaseStack()
    {
        Stack++;
        Rewrite.Invoke();
    }

    public Action<LivingEntity, LivingEntity, BuffInfo> TriggerBuffDelegate { get { return mTriggerBuffDelegate; } set { mTriggerBuffDelegate = value; } }
    public BuffCondition Condition { get { return mCondition; } set { mCondition = value; } }
    public GameObject BuffIcon { get { return mBuffIcon; } set { mBuffIcon = value; } }
    public UnityEngine.UI.Slider Slider { get { return mSlider; } set { mSlider = value; } }
    public float DOTInterval { get { return mDOTInterval; } set { mDOTInterval = value; } }
    public bool IsCanceled { get { return mbIsCanceled; } private set { mbIsCanceled = value; } }
    public LivingEntity Target { get { return mTarget; } set { mTarget = value; } }
    public float DurationTime { private get { return mDurationTime; } set { mDurationTime = value; } }
    public LivingEntity CastedBy { get { return mCastedBy; } set { mCastedBy = value; } }
    public float StackScale { get { return mStackScale; } set { mStackScale = value; } }
    public byte Stack { get { return mStack; } set { mStack = value; } }
    public bool IsRenew { get { return mbIsRenew; } private set { mbIsRenew = value; } }
    public bool IsPendingKill { get { return mbIsPendingKill; } set { mbIsPendingKill = value; } }
    public bool HasCondition { get { return mbHasCondition; } set { mbHasCondition = value; } }
    public bool IsPassive { get { return mbIsPassive; } set { mbIsPassive = value; } }
    private float DOTElapsedTime { get { return mDOTElapsedTime; } set { mDOTElapsedTime = value; } }    
    private float Timer { get { return mTimer; } set { mTimer = value; } }
    

    public void OnElapsedTime(float elapsedTime)
    {
        EventHandler<ElapsedTimeEventArgs> temp = ElapsedTimeEvent;
        if (temp != null)
        {
            temp(this, new ElapsedTimeEventArgs(elapsedTime));
        }
    }
    public void SetStatBuffInfo(StatBuffInfo info)
    {
        BuffIcon.GetComponent<BuffIcon>().StatBuffInfo = info;
    }
    public void ResetTimer()
    {
        Timer = DurationTime;
        Slider.value = 0.0f;
    }

    
     // 조건을 만족한다는 전제에 호출하는 함수임, 반드시 조건 검사후 true일 때만 호출해야함
    public void TriggerBuff()
    {
        Condition.UpdateEndCoolTime();
        Slider.value = DurationTime;

        // TriggerBuffDelegate = RegisterBuff
        TriggerBuffDelegate(CastedBy, Target, this);
        //내부쿨 스타트
        BuffManager.Instance.StartCoroutine(Countdown());
    }

    public void Renew(LivingEntity newCastedBy = null)
    {
        IsRenew = true;
        if (newCastedBy != null)
        {
            CastedBy = newCastedBy;
        }
    }

    public IEnumerator Countdown()
    {
        OnElapsedTime(Time.deltaTime);
        //Logger.Log("Start Countdown");
        if (IsPassive)
        {
            //패시브, 내부쿨 검사
            for (Timer = Condition.CoolTime; Timer >= 0; Timer -= Time.deltaTime)
            {
                Slider.value = Timer;

                //Logger.Log(string.Format("Countdown {0}", Timer));
                yield return null;
            }
            OnElapsedTime(Time.deltaTime);
            Condition.TryToTriggerBuff();
        }
        else
        {
            //일시적인 버프
            for (Timer = DurationTime; Timer >= 0; Timer -= Time.deltaTime)
            {
                //Logger.Log(string.Format("Start Countdown for : {0}", Timer));
                if (IsCanceled)
                {
                    //Logger.Log("Start Countdown Cancel");
                    remove.Invoke(this);
                    BuffManager.Instance.RemoveBuffInfo(this);
                    yield break;
                }

                if (IsRenew)
                {
                    ResetTimer();
                    IsRenew = false;
                }

                if (DOT != null && DOTElapsedTime >= DOTInterval)
                {
                    DOT.Invoke(CastedBy, Target);
                    DOTElapsedTime -= DOTInterval;
                }
                else
                {
                    DOTElapsedTime += Time.deltaTime;
                }

                Slider.value += Time.deltaTime;

                //Logger.Log(string.Format("Countdown {0}", Timer));
                yield return null;
            }

            remove.Invoke(this);
            BuffManager.Instance.RemoveBuffInfo(this);
        }

        //Logger.Log("Start Countdown Invoke");

    }

    public void Clear()
    {
        Condition.Clear();
        Target = null;
        CastedBy = null;
        DurationTime = 0.0f;
        Timer = 0.0f;
        DOTElapsedTime = 0.0f;
        DOTInterval = -1.0f;
        StackScale = 0.0f;
        Stack = 0;
        remove = null;
        Rewrite = null;
        DOT = null;
        IsCanceled = false;
        IsPendingKill = false;
        IsRenew = false;        
        BuffIcon.SetActive(false);
    }

    public void ForceRemove()
    {
        IsCanceled = true;
    }

    BuffInfo mNext;
    public BuffInfo Next { get { return mNext; } set { mNext = value; } }
}

public class BuffManager : MonoSingleton<BuffManager>
{
    [SerializeField]
    private GameObject m_BuffIconPrefab;
    //public GameObject mDebuffIcon;
    public GameObject mPlayerBuffList;
    public GameObject mBossBuffList;
    public GameObject InfoPanel;
    AsyncOperationHandle mBuffIconHandle;
    private List<BuffInfo> mBuffPool;
    // Head of BuffPool
    private BuffInfo mAvailableBuff;
    private List<BuffInfo> mActivatedBuff;
    private uint mActivatedBuffCount = 0;
    private uint mBuffID;


    public uint ActivatedBuffCount { get { return mActivatedBuffCount; } set { mActivatedBuffCount = value; } }
    private GameObject BuffIconPrefab { get { return m_BuffIconPrefab; } set { m_BuffIconPrefab = value; } }
    private AsyncOperationHandle BuffIconHandle { get { return mBuffIconHandle; } set { mBuffIconHandle = value; } }
    //private BuffInfo ActivatedBuff { set { mActivatedBuff.Add(value); } }
    private void AddActivatedBuff(BuffInfo buffInfo)
    {
        mActivatedBuff.Add(buffInfo);
        //Logger.Log(mActivatedBuff.Count);
    }
    private void RemoveActivatedBuff(BuffInfo buffInfo)
    {
        mActivatedBuff.Remove(buffInfo);
        //Logger.Log(mActivatedBuff.Count);
    }

    private BuffInfo AvailableBuff { set { mAvailableBuff = value; } }
    private BuffInfo GetBuffInfo()
    {
        //var newBuff = mAvailableBuff;
        //mAvailableBuff = newBuff.Next;
        //AddActivatedBuff(newBuff);
        //return newBuff;
        return mAvailableBuff;
    }

    public void RemoveBuffInfo(BuffInfo buff)
    {
        //Logger.Log("BM : RemoveBuff");
        buff.Clear();
        buff.Next = GetBuffInfo();
        AvailableBuff = buff;
        RemoveActivatedBuff(buff);
        buff.BuffIcon.transform.SetParent(null);
        mActivatedBuffCount--;
        RelocationBuffIcon();
    }

    private void RelocationBuffIcon()
    {
        for (int index = 0; index < mBossBuffList.transform.childCount; index++)
        {
            Logger.Log($"Buff count : {mBossBuffList.transform.childCount}");
            mBossBuffList.transform.GetChild(index).transform.localPosition = new Vector3(-500.0f + 60.0f * index, 0.0f);
        }
        for (int index = 0; index < mPlayerBuffList.transform.childCount; index++)
        {
            mPlayerBuffList.transform.GetChild(index).transform.localPosition = new Vector3(-500.0f + 60.0f * index, 0.0f);
        }
    }

    // 실사용 코드
    public void RegisterBuff(LivingEntity castedBy, List<LivingEntity> targets, IBuff buff)
    {
        //테스트코드
        //BuffInfo newBuff = GetBuffInfo();
        //newBuff.CastedBy = castedBy;
        //newBuff.Target = FindObjectOfType<BossInformation>();
        //newBuff.DurationTime = 3.0f;
        //newBuff.Slider.maxValue = 3.0f;
        //newBuff.remove = buff.RemoveBuff;
        //newBuff.StackScale = buff.StackScale;
        //newBuff.Stack = 1;
        //if (newBuff.Target is BossInformation)
        //{
        //    newBuff.BuffIcon.transform.SetParent(mBossBuffList.transform);
        //}

        //if(buff as IDOT != null)
        //{
        //    newBuff.DOTInterval = (buff as IDOT).DOTInterval;
        //    newBuff.DOT = (buff as IDOT).DOT;
        //}

        ////else if(newBuff.Target is )
        //newBuff.BuffIcon.transform.Find("CoolDown/Icon").GetComponent<UnityEngine.UI.Image>().sprite = buff.GetIcon();
        //newBuff.BuffIcon.transform.localPosition = new Vector3(-500.0f + 60.0f * ActivatedBuffCount, 0.0f);
        //newBuff.BuffIcon.SetActive(true);

        //mAvailableBuff = newBuff.Next;
        //AddActivatedBuff(newBuff);

        //buff.ApplyBuff(newBuff);
        //ActivatedBuffCount++;
        //if (newBuff.IsPendingKill)
        //{
        //    RemoveBuffInfo(newBuff);
        //}
        //else
        //{
        //    StartCoroutine(newBuff.Countdown());

        //}

        foreach (var target in targets)
        {

            BuffInfo newBuff = GetBuffInfo();
            newBuff.IsPassive = false;
            newBuff.CastedBy = castedBy;
            newBuff.Target = target;
            newBuff.DurationTime = buff.BuffDurationTime;
            newBuff.Slider.maxValue = buff.BuffDurationTime;
            newBuff.remove = buff.RemoveBuff;
            newBuff.StackScale = buff.StackScale;
            newBuff.Stack = 1;
            if (newBuff.Target is BossInformation)
            {
                newBuff.BuffIcon.transform.SetParent(mBossBuffList.transform);
            }
            if (newBuff.Target is PlayerInformation)
            {
                newBuff.Target.GetComponent<PlayerInformation>().SetBuffIcon(newBuff.BuffIcon.transform);
            }

            if (buff as IDOT != null)
            {
                newBuff.DOTInterval = (buff as IDOT).DOTInterval;
                newBuff.DOT = (buff as IDOT).DOT;
            }

            newBuff.BuffIcon.transform.Find("CoolDown/Icon").GetComponent<UnityEngine.UI.Image>().sprite = (buff as Skill).Icon;
            newBuff.BuffIcon.transform.localPosition = new Vector3(-500.0f + 60.0f * ActivatedBuffCount, 0.0f);
            newBuff.BuffIcon.SetActive(true);
            mAvailableBuff = newBuff.Next;
            AddActivatedBuff(newBuff);

            buff.ApplyBuff(newBuff);
            ActivatedBuffCount++;
            if (newBuff.IsPendingKill)
            {
                RemoveBuffInfo(newBuff);
            }
            else
            {
                StartCoroutine(newBuff.Countdown());
            }
        }
    }

    public void RegisterPassive(LivingEntity castedBy, LivingEntity target, IPassive buff)
    {
        BuffInfo newBuff = GetBuffInfo();
        newBuff.IsPassive = true;
        newBuff.CastedBy = castedBy;
        newBuff.Target = target;
        //newBuff.DurationTime = buff.PassiveCoolTime;
        newBuff.Slider.maxValue = buff.PassiveCoolTime;
        newBuff.remove = buff.RemovePassive;
        newBuff.StackScale = 1.0f;
        newBuff.Stack = 1;
        if (newBuff.Target is BossInformation)
        {
            newBuff.BuffIcon.transform.SetParent(mBossBuffList.transform);
        }
        if (newBuff.Target is PlayerInformation)
        {
            newBuff.Target.GetComponent<PlayerInformation>().SetBuffIcon(newBuff.BuffIcon.transform);
        }

        if (buff as IDOT != null)
        {
            newBuff.DOTInterval = (buff as IDOT).DOTInterval;
            newBuff.DOT = (buff as IDOT).DOT;
        }

        if (buff as ILazyBuff != null)
        {
            newBuff.LazyBuff = (buff as ILazyBuff).ApplyLazyBuff;
        }

        newBuff.BuffIcon.transform.Find("CoolDown/Icon").GetComponent<UnityEngine.UI.Image>().sprite = buff.GetIcon();
        newBuff.BuffIcon.transform.localPosition = new Vector3(-500.0f + 60.0f * ActivatedBuffCount, 0.0f);
        newBuff.BuffIcon.SetActive(true);
        mAvailableBuff = newBuff.Next;
        AddActivatedBuff(newBuff);

        buff.ApplyPassive(newBuff);
        ActivatedBuffCount++;
        if (newBuff.IsPendingKill)
        {
            RemoveBuffInfo(newBuff);
        }
        //게임 시작후 내부쿨이 돌게되므로 주석        
        //else
        //{
        //    StartCoroutine(newBuff.Countdown());
        //}
    }

    public void RegisterBuff(LivingEntity castedBy, LivingEntity target, IBuff buff)
    {
        BuffInfo newBuff = GetBuffInfo();
        newBuff.IsPassive = false;
        newBuff.CastedBy = castedBy;
        newBuff.Target = target;
        newBuff.DurationTime = buff.BuffDurationTime;
        newBuff.Slider.maxValue = buff.BuffDurationTime;
        newBuff.Slider.value = 0.0f;
        newBuff.remove = buff.RemoveBuff;
        newBuff.StackScale = buff.StackScale;
        newBuff.Stack = 1;
        if (newBuff.Target is BossInformation)
        {
            newBuff.BuffIcon.transform.SetParent(mBossBuffList.transform);
        }
        if (newBuff.Target is PlayerInformation)
        {
            newBuff.Target.GetComponent<PlayerInformation>().SetBuffIcon(newBuff.BuffIcon.transform);
        }

        if (buff as IDOT != null)
        {
            newBuff.DOTInterval = (buff as IDOT).DOTInterval;
            newBuff.DOT = (buff as IDOT).DOT;
        }

        if (buff as ILazyBuff != null)
        {
            newBuff.LazyBuff = (buff as ILazyBuff).ApplyLazyBuff;
        }

        newBuff.BuffIcon.transform.Find("CoolDown/Icon").GetComponent<UnityEngine.UI.Image>().sprite = (buff as Skill).Icon;
        newBuff.BuffIcon.transform.localPosition = new Vector3(-500.0f + 60.0f * ActivatedBuffCount, 0.0f);
        newBuff.BuffIcon.SetActive(true);
        mAvailableBuff = newBuff.Next;
        AddActivatedBuff(newBuff);

        buff.ApplyBuff(newBuff);
        ActivatedBuffCount++;
        if (newBuff.IsPendingKill)
        {
            RemoveBuffInfo(newBuff);
        }
        else
        {
            StartCoroutine(newBuff.Countdown());
        }
    }

    protected override void Awake()
    {
        base.Awake();
        //BuffIconPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/BuffIcon/BuffIcon.prefab");

        Addressables.LoadAssetAsync<GameObject>("btnBuffIcon").Completed += (obj) => 
        { 
            BuffIconHandle = obj; 
            BuffIconPrefab = obj.Result;

            mActivatedBuff = new List<BuffInfo>(512);
            mBuffPool = new List<BuffInfo>(512);

            GameObject buffContainer = new GameObject("Buff Container");

            for (int index = 0; index < 512; index++)
            {
                mBuffPool.Add(new BuffInfo());
                mBuffPool[index].BuffIcon = Instantiate<GameObject>(BuffIconPrefab, buffContainer.transform);
                mBuffPool[index].BuffIcon.SetActive(false);
                mBuffPool[index].Slider = mBuffPool[index].BuffIcon.GetComponentInChildren<UnityEngine.UI.Slider>();
                mBuffPool[index].Condition = new BuffCondition(mBuffPool[index]);
                mBuffPool[index].Next = GetBuffInfo();

                AvailableBuff = mBuffPool[index];
            }
        };        
    }

    //void DuplicateTest()
    //{
    //    GlobalSkillList list = FindObjectOfType<GlobalSkillList>();
    //    Skill skill = list[0];
    //    RegisterBuff("testEntity2", skill as IBuff, null);

    //}

    //void DuplicateTest2()
    //{
    //    GlobalSkillList list = FindObjectOfType<GlobalSkillList>();
    //    Skill skill = list[0];
    //    RegisterBuff("testEntity3", skill as IBuff, null);
    //}

    private void Start()
    {

        //GlobalSkillList list = FindObjectOfType<GlobalSkillList>();
        //Skill skill = list[1];

        //중복 허용 안하는 스택 테스트
        //RegisterBuff("testEntity1", skill as IBuff, null);
        //RegisterBuff("testEntity1", skill as IBuff, null);
        //Invoke("DuplicateTest", 2.0f);
        //Invoke("DuplicateTest2", 4.0f);



        //중복 가능 & 사망 테스트 -> 통과
        //중복 불가능 & 사망 테스트 -> 통과
        //RegisterBuff("testEntity1", skill as IBuff, null);


        //PlayerInformation[] t = FindObjectsOfType<PlayerInformation>();

        //RegisterBuff(t[0], null, skill as IBuff);
        //RegisterBuff(t[0], null, skill as IBuff);
        //RegisterBuff(t[0], null, skill as IBuff);
        //RegisterBuff(t[1], null, skill as IBuff);
        //RegisterBuff(t[0], null, skill as IBuff);
        //RegisterBuff(t[0], null, skill as IBuff);
        ////Invoke("DuplicateTest2", 2.0f);
        //RegisterBuff(t[0], null, skill as IBuff);
        //RegisterBuff(t[1], null, skill as IBuff);
        //RegisterBuff(t[2], null, skill as IBuff);



        //부하 테스트
        //for (int index = 0; index < 100; index++)
        //{
        //    RegisterBuff(FindObjectOfType<PlayerInformation>(), skill as IBuff, null);
        //}

    }
}
