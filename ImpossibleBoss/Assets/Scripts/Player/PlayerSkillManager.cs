using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PlayerSkillManager : MonoBehaviour
{
    [SerializeField]
    private float m_GlovalCoolTime;
    private float mRemainingGlovalCoolTime;
    [SerializeField]
    private List<Skill> mSkills;
    private List<float> mDurationCoolTime;
    private List<float> mDurationChaining;
    private float mDurationCastingTime;
    private float mElapsedChannelingTime;
    private ICasting mCastingSkill;
    private int mCastingIndex;
    private int mChannelingIndex;
    private float mChannelingTime;
    //private bool mbCanMoveChanneling;
    private (LivingEntity player, int index) mLazyCastInfo;
    private bool mbIsCasting;
    private bool mbIsChanneling;
    private bool mbIsMoving;
    private bool mbIsForceCancel;
    private bool mbIsAnimStopped;
    private bool mbIsShowingCustomIndicator;
    private bool mbCanBeInterruptedByDash;
    private bool mbIsKnockback;
    public List<Slider> m_Slider;
    public List<Image> m_ChainSlider;
    public SkillButton[] m_btnSkill;

    [SerializeField]
    private GameObject m_Indicator;
    [SerializeField]
    private Slider m_CastingBar;
    [SerializeField]
    private Image m_CastingBarFill;
    [SerializeField]
    private Image m_CastingIcon;
    [SerializeField]
    private Localize m_CastingSkillName;
    [SerializeField]
    private Text m_CastingText;

    private Queue<SAnimStateInfo> mAnimQueue;

    private Dictionary<string, bool> mSkillTagBanList;

    private const float INDICATOR_SIZE = 10.0f;

    private string mCastingTextFormat;

    //public event EventHandler<ForceRotationEventArgs> CastingEvent;

    private Action<LivingEntity> mFinishChan;

    private PlayerMovement mPlayerMovement;

    //애니메이션
    private SwapCastMotion mSwapMotion;
    //[SerializeField]
    //private AnimationClip m_CastClip;
    private Animator mPlayerAnimator;
    private PlayerAnimState mPlayerAnimState;
    private int mAnimPlayingIndex;

    //넉백 면역 테스트
    private float immunTest = 0.0f;
    PlayerInformation mPlayerInfo;
    
    //private bool[] mbIsSmartCasting = new bool[6];


    //private bool IsSmartCasting(int index) 
    //{
    //    return mbIsSmartCasting[index];
    //}

    //private bool[] SetSmartCasting { set { mbIsSmartCasting = value; } }

    public bool IsCasting { get { return mbIsCasting; } set { mbIsCasting = value; } }
    public bool IsChanneling { get { return mbIsChanneling; } set { mbIsChanneling = value; } }
    public bool IsMoving { get { return mbIsMoving; } set { mbIsMoving = value; } }
    public bool IsForceCancel { get { return mbIsForceCancel; } set { mbIsForceCancel = value; } }
    private Action<LivingEntity> FinishChan { get { return mFinishChan; } set { mFinishChan = value; } }
    private PlayerInformation PlayerInfo { get { return mPlayerInfo; } set { mPlayerInfo = value; } }
    private Queue<SAnimStateInfo> AnimQueue { get { return mAnimQueue; } set { mAnimQueue = value; } }
    private bool IsAnimStopped { get { return mbIsAnimStopped; } set { mbIsAnimStopped = value; } }
    //public Dictionary<string, bool> SkillTagBanList { get { return mSkillTagBanList; } }
    private float GlovalCoolTime { get { return m_GlovalCoolTime; } }
    private float RemainingGlovalCoolTime { get { return mRemainingGlovalCoolTime; } set { mRemainingGlovalCoolTime = value; } }
    private ICasting CastingSkill { get { return mCastingSkill; } set { mCastingSkill = value; } }
    private int CastingIndex { get { return mCastingIndex; } set { mCastingIndex = value; } }
    private int ChannelingIndex { get { return mChannelingIndex; } set { mChannelingIndex = value; } }
    private float ChannelingTime { get { return mChannelingTime; } set { mChannelingTime = value; } }
    //private bool CanMoveChanneling { get { return mbCanMoveChanneling; } set { mbCanMoveChanneling = value; } }
    private (LivingEntity, int) LazyCastInfo { get { return mLazyCastInfo; } set { mLazyCastInfo = value; } }
    private float DurationCastingTime { get { return mDurationCastingTime; } set { mDurationCastingTime = value; } }
    private float ElapsedChannelingTime { get { return mElapsedChannelingTime; } set { mElapsedChannelingTime = value; } }
    private string CastingTextFormat { get { return mCastingTextFormat; } }
    private List<Skill> Skills { get { return mSkills; } set { mSkills = value; } }
    private List<Slider> Slider { get { return m_Slider; } }
    private List<Image> ChainSlider { get { return m_ChainSlider; } }
    private List<float> DurationCoolTime { get { return mDurationCoolTime; } set { mDurationCoolTime = value; } }
    private List<float> DurationChaining { get { return mDurationChaining; } set { mDurationChaining = value; } }
    private Animator PlayerAnimator { get { return mPlayerAnimator; } /*set { mPlayerAnimator = value; }*/ }
    private PlayerAnimState PlayerAnim { get { return mPlayerAnimState; } }
    private SwapCastMotion SwapMotion { get { return mSwapMotion; } }
    private int AnimPlayingIndex { get { return mAnimPlayingIndex; } set { mAnimPlayingIndex = value; } }
    private bool IsShowingCustomIndicator { get { return mbIsShowingCustomIndicator; } set { mbIsShowingCustomIndicator = value; } }
    private bool CanBeInterruptedByDash { get { return mbCanBeInterruptedByDash; } set { mbCanBeInterruptedByDash = value; } }
    private bool IsKnockback { get { return mbIsKnockback; } set { mbIsKnockback = value; } }
    private PlayerMovement Movement { get { return mPlayerMovement; } }

    //private bool IsSkillCanceled()
    //{
    //    if (IsCasting || IsChanneling || )
    //    {

    //    }
    //}

    //public AnimationClip CastClip { get { return m_CastClip; } set { m_CastClip = value; } }

    public void TryCast(int index)
    {
        
        if (CanUseSkill(index))
        {
            OnForceRotation(Skills[index].IsForceRotation);
            AnimPlayingIndex = index;
            IsAnimStopped = false;
            AnimQueue = Skills[index].GetAnimQueue();            
            PlayerAnim.SetController(Skills[index].AOC);
            PlayerAnim.SetCanMove(Skills[index].CanMove);
            PlayerAnim.SetAnimQueue(Skills[index].GetAnimQueue());
            Movement.ShouldObstructInput = Skills[index].ShouldObstructInput;
            if (Skills[index].GetAnimQueue().Peek().AnimState != EAnimState.Casting || (Skills[index] as ISkillChain) != null)
            {
                //SetCoolTime(AnimPlayingIndex);
                SetCoolTime(index);
            }                        
        }
    }

    public void NextAction()
    {
        CastNextAction(AnimPlayingIndex);
    }

    private void Dash(int index)
    {
        (Skills[index] as IDash).Dash(PlayerInfo);
    }

    private bool CanUseSkill(int index)
    {
        bool isCooledDown = IsCooledDown(index);
        bool shouldBan = false;
        ISkillChain chain = Skills[index] as ISkillChain;
        bool isChaining = false;
        if (chain != null)
        {
            isChaining = true;
        }
        bool isDash = false;
        IDash dash = Skills[index] as IDash;
        if (dash != null)
        {
            isDash = true;
        }

        bool isSelf = isChaining && IsChanneling && ChannelingIndex == index;
        bool canMove = Skills[index].CanMove;

        if (IsKnockback)
        {
            return false;
        }
        
        if(isCooledDown && ( (IsCasting == false && IsChanneling == false) || (isDash == false && isSelf) || (isDash && CanBeInterruptedByDash) ) /*&& (canMove || IsMoving == false)*/ )
        {            
            //skill ban 검사
            List<string> tag = GetSkill(index).Tag;
            for (int tagIndex = 0; tagIndex < tag.Count; tagIndex++)
            {
                shouldBan ^= ShouldSkillBan(tag[tagIndex]);
                if (shouldBan == true)
                {
                    return false;
                }
            }

            return true;
        }
        else
        {
            return false;
        }        
    }
    private bool IsCooledDown(int index)
    {
        if (WithinRange(index))
        {
            return DurationCoolTime[index] <= 0.0f ? true : false;
        }
        else
        {
            return false;
        }        
    }

    private bool IsGlovalCooledDown()
    {
        return RemainingGlovalCoolTime <= 0.0f ? true : false;
    }

    public void AnimStop()
    {        
        SetCustomIndicator(false);
        IsAnimStopped = true;
        Movement.ShouldObstructInput = false;
    }

    private bool CanCast()
    {
        return IsAnimStopped;
    }

    private void SetCoolTime(int index)
    {
        if (WithinRange(index))
        {
            //연쇄 스킬이면서 현재 연쇄중인가?
            ISkillChain skillChain = GetSkill(index) as ISkillChain;
            if (skillChain != null && skillChain.IsChaining)
            {
                ChainSlider[index].fillAmount = 1.0f;
                DurationChaining[index] = skillChain.TimeLimit;
                ChainSlider[index].gameObject.SetActive(true);
            }
            else
            {
                ChainSlider[index].gameObject.SetActive(false);
                DurationCoolTime[index] = GetSkill(index).CoolDown;
                if (DurationCoolTime[index] == 0.0f)
                {
                    Slider[index].value = 0.0f;
                }
                else
                {
                    Slider[index].value = 1.0f;
                }
            }
        }
    }
    public Skill GetSkill(int index)
    {
        if (WithinRange(index) && Skills.Count > 0)
        {
            return Skills[index];
        }
        return null;
    }

    public void SetSkillBan(string tag, bool state)
    {
        bool isBan;

        if (mSkillTagBanList.TryGetValue(tag, out isBan))
        {
            mSkillTagBanList[tag] = state;
        }
        else
        {
            mSkillTagBanList.Add(tag, state);
        }
    }

    public bool ShouldSkillBan(string tag)
    {
        bool isBan = false;
        if (mSkillTagBanList.TryGetValue(tag, out isBan) && isBan)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool SearchTag(string tag, int index)
    {
        if (GetSkill(index).Tag != null)
        {
            foreach (string skillTag in GetSkill(index).Tag)
            {
                if (skillTag == tag)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void SetSkillBtn()
    {
        GlobalSkillList list = FindObjectOfType<GlobalSkillList>();
        DurationCoolTime = new List<float>(6);
        DurationChaining = new List<float>(6);

        

        #region 테스트용 하드코딩 (선택한 스킬)
        Skills = new List<Skill>(6);
        //Skills.Add(list[0]);
        //Skills.Add(list[1]);
        //Skills.Add(list[2]);
        //Skills.Add(list[3]);
        //Skills.Add(list[4]);
        //Skills.Add(list[5]);

        //Skills.Add(list[0]);
        //Skills.Add(list[1]);
        //Skills.Add(list[2]);
        //Skills.Add(list[3]);
        //Skills.Add(list[15]);
        //Skills.Add(list[5]);
        //Skills.Add(list[13]);
        //Skills.Add(list[16]);
        //Skills.Add(list[17]);
        //Skills.Add(list[ushort.MaxValue]);
        #endregion
        //실제 사용할 코드
        Skills = list.SelectedSkills;

        m_btnSkill[0].Desc = Skills[0].Description;
        m_btnSkill[1].Desc = Skills[1].Description;
        m_btnSkill[2].Desc = Skills[2].Description;
        m_btnSkill[3].Desc = Skills[3].Description;
        m_btnSkill[4].Desc = Skills[4].Description;
        m_btnSkill[5].Desc = Skills[5].Description;

        m_btnSkill[0].SetIcon(Skills[0].Icon);
        m_btnSkill[1].SetIcon(Skills[1].Icon);
        m_btnSkill[2].SetIcon(Skills[2].Icon);
        m_btnSkill[3].SetIcon(Skills[3].Icon);
        m_btnSkill[4].SetIcon(Skills[4].Icon);
        m_btnSkill[5].SetIcon(Skills[5].Icon);

        DurationCoolTime.Add(0.0f);
        DurationCoolTime.Add(0.0f);
        DurationCoolTime.Add(0.0f);
        DurationCoolTime.Add(0.0f);
        DurationCoolTime.Add(0.0f);
        DurationCoolTime.Add(0.0f);

        DurationChaining.Add(0.0f);
        DurationChaining.Add(0.0f);
        DurationChaining.Add(0.0f);
        DurationChaining.Add(0.0f);
        DurationChaining.Add(0.0f);
        DurationChaining.Add(0.0f);
    }

    private void Awake()
    {
        //m_btnSkill = new SkillButton[6];
        SetSkillBtn();
        mSkillTagBanList = new Dictionary<string, bool>();
        mPlayerInfo = GetComponent<PlayerInformation>();
        GameManager.Instance.GameStartEvent += OnGameStartEvent;
        mPlayerAnimator = GetComponent<Animator>();
        mPlayerAnimState = GetComponent<PlayerAnimState>();
        mSwapMotion = GetComponent<SwapCastMotion>();
        IsAnimStopped = true;
        ChannelingIndex = -1;
    }

    public void Start()
    {
        IsCasting = false;
        IsShowingCustomIndicator = false;
        mPlayerMovement = gameObject.GetComponentInChildren<PlayerMovement>();
        Movement.PlayerMovingEvent += IsMovingEvent;
        gameObject.GetComponentInChildren<PlayerInformation>().KnockBackEvent += KnockBackEvent;
        gameObject.GetComponentInChildren<PlayerInputManager>().IsForceCancelEvent += IsForceCancelEvent;
        mCastingTextFormat = "{0:0.00} / {1}";
    }
    private void Update()
    {
        //RemainingGlovalCoolTime -= Time.deltaTime;

        for (int index = 0; index < 6; index++)
        {
            if (IsCooledDown(index) == false)
            {
                DurationCoolTime[index] -= Time.deltaTime;
                Slider[index].value = Mathf.Clamp(DurationCoolTime[index] / Skills[index].CoolDown, 0.0f, 1.0f);
            }
            ISkillChain skillChain = (Skills[index] as ISkillChain);
            if (skillChain != null && skillChain.IsChaining)
            {
                DurationChaining[index] -= Time.deltaTime;
                ChainSlider[index].fillAmount = Mathf.Clamp(DurationChaining[index] / skillChain.TimeLimit, 0.0f, 1.0f);

                //만약 스킬을 사용하지 않고 시간이 지나버리면 쿨다운 돌림
                if (DurationChaining[index] <= 0.0f)
                {
                    skillChain.TimeOut(PlayerInfo);
                    SetCoolTime(index);
                }
            }
        }

        //캐스팅중 무빙 체크
        //Logger.Log($"IsMoving : {IsMoving}");
        if (IsCasting)
        {
            if (!IsForceCancel && (Skills[CastingIndex].CanMove || !IsMoving))
            {
                m_CastingBar.value = CastingSkill.CastingTime - DurationCastingTime;
                m_CastingText.text = string.Format(CastingTextFormat, DurationCastingTime, CastingSkill.CastingTime);
                DurationCastingTime -= Time.deltaTime;

                //Logger.Log(DurationCastingTime);

                if (DurationCastingTime <= 0)
                {
                    //CastNextAction(CastingIndex);
                    SetCoolTime(CastingIndex);
                    FinishCasting();
                }
            }
            else
            {                
                FinishCasting();
                FinishAnim();
            }

        }
        
        //Logger.Log($"IsChanneling : {IsChanneling}");
        if (IsChanneling)
        {
            //채널링중 무빙 체크
            if (!IsForceCancel && (Skills[ChannelingIndex].CanMove || !IsMoving) && ElapsedChannelingTime <= ChannelingTime)
            {                
                m_CastingBar.value = ElapsedChannelingTime;
                m_CastingText.text = string.Format(CastingTextFormat, ElapsedChannelingTime, ChannelingTime);
                ElapsedChannelingTime += Time.deltaTime;
                Channeling(ChannelingIndex);
            }
            else
            {                
                FinishChanneling();
                FinishAnim();
            }
        }

        //#region Immun Test
        //if (mPlayerInfo.HasKnockBackImmun && immunTest >= 0.3f)
        //{
        //    mPlayerInfo.HasKnockBackImmun = false;
        //}
        //else
        //{
        //    immunTest += Time.deltaTime;
        //}
        //#endregion
    }

    private void FinishCasting()
    {        
        IsCasting = false;
        CanBeInterruptedByDash = false;
        m_CastingBar.gameObject.SetActive(false);
        CastingIndex = -1;
    }

    private void CastNextAction(int index)
    {
        if(AnimQueue.Count == 0)
        {
            AnimPlayingIndex = -1;
            return;
        }
        else
        {
            if(Skills[index] as ILazyCast != null)
            {
                LazyCastInfo = (PlayerInfo, index);
            }

            switch (AnimQueue.Dequeue().AnimState)
            {
                case EAnimState.Cast: Cast(index); break;
                case EAnimState.Casting: Casting(index); break;
                case EAnimState.Dash: Dash(index); break;
                case EAnimState.ChannelingStart: ChannelingStart(index); break;
                default: return;
            }
        }
    }

    public void LazyCast()
    {        
        (Skills[LazyCastInfo.Item2] as ILazyCast).LazyCast(LazyCastInfo.Item1);
    }

    private void OnForceRotation(bool isForceRot)
    {
        RaycastHit hit;
        Ray ray = Movement.m_MainCamera.ScreenPointToRay(GetComponent<PlayerInputManager>().MousePos);
        Physics.Raycast(ray, out hit);

        Vector3 currPos = transform.position;
        Vector3 targetPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);

        Movement.ForceRot((targetPos - currPos).normalized, isForceRot);
    }

    private void Casting(int index)
    {
        CastingSkill = Skills[index] as ICasting;

        m_CastingBar.gameObject.SetActive(true);
        m_CastingBarFill.color = new Color(1.0f, 0.3882353f, 0.0f);
        m_CastingIcon.sprite = Skills[index].Icon;
        m_CastingSkillName.SetID(Skills[index].Name);
        CastingIndex = index;
        IsCasting = true;
        CanBeInterruptedByDash = CastingSkill.CanBeInterruptedByDash;
        CastingSkill.Casting(PlayerInfo);

        //OnForceRotation(Skills[index].IsForceRotation);

        //if (CanUseSkill(index))
        //{
        //    CastingSkill = Skills[index] as ICasting;

        //    //RemainingGlovalCoolTime = GlovalCoolTime;

        //    if (CastingSkill != null)
        //    {

        //        //SwapMotion.SwapController(Skills[index].AOC);

        //        m_CastingBar.gameObject.SetActive(true);
        //        m_CastingBarFill.color = new Color(1.0f, 0.3882353f, 0.0f);
        //        //m_CastingBar.maxValue = CastingSkill.CastingTime;
        //        m_CastingIcon.sprite = Skills[index].Icon;
        //        m_CastingSkillName.SetID(Skills[index].Name);
        //        //DurationCastingTime = CastingSkill.CastingTime;
        //        CastingIndex = index;
        //        IsCasting = true;
        //        CastingSkill.Casting();
        //    }
        //    else
        //    {
        //        Cast(PlayerInfo, index);
        //    }
        //    OnForceRotation(Skills[index].IsForceRotation);
        //}
    }

    public void SetCastingTime(float castingTime)
    {
        m_CastingBar.maxValue = castingTime;
        DurationCastingTime = castingTime;
    }

    //public void StartChanneling()
    //{
    //    m_CastingBar.gameObject.SetActive(false);
    //    ChannelingIndex = -1;
    //    IsChanneling = false;
    //    ElapsedChannelingTime = 0.0f;
    //    OnForceRotation(false);
    //}

    //public void FinishDash()
    //{
    //    Movement.FinishDash();
    //    Logger.Log("Finish Dash By Animation End");
    //}

    private void ChannelingStart(int index)
    {
        IChanneling chan = (Skills[index] as IChanneling);

        //IsChanneling = true;
        //ChannelingIndex = index;
        //FinishChan = chan.ChannelingEnd;
        //ElapsedChannelingTime = 0.0f;
        //m_CastingBarFill.color = Color.red;
        //ChannelingTime = chan.ChannelingTime;
        //chan.ChannelingStart(PlayerInfo);
        //CanBeInterruptedByDash = chan.CanBeInterruptedByDash;

        //자기 스스로 호출됐다 = 두번 호출됐다.
        //index == ChannelingIndex이 검사되는 경우는 채널링시간 > 쿨타임인 경우뿐임
        //그리고 이 경우는 Chain밖에 없음
        if (index == ChannelingIndex)
        {
            FinishChanneling();
        }
        else
        {
            IsChanneling = true;
            ChannelingIndex = index;
            FinishChan = chan.ChannelingEnd;
            ElapsedChannelingTime = 0.0f;
            m_CastingBarFill.color = Color.red;
            ChannelingTime = chan.ChannelingTime;
            chan.ChannelingStart(PlayerInfo);
            CanBeInterruptedByDash = chan.CanBeInterruptedByDash;
            //CanMoveChanneling = (Skills[ChannelingIndex] as IChanneling).CanMove;
        }
    }

    public void Channeling(int index)
    {
        IChanneling skill = Skills[index] as IChanneling;
        m_CastingBar.gameObject.SetActive(true);
        m_CastingBar.maxValue = skill.ChannelingTime;
        m_CastingSkillName.SetID(Skills[index].Name);
        m_CastingIcon.sprite = Skills[index].Icon;
        skill.Channeling(PlayerInfo);
    }
    public void FinishChanneling()
    {
        if ( (Skills[ChannelingIndex] as ISkillChain) != null)
        {

        }
        m_CastingBar.gameObject.SetActive(false);
        ChannelingIndex = -1;
        IsChanneling = false;
        CanBeInterruptedByDash = false;
        ElapsedChannelingTime = 0.0f;
        //중간에 짤릴경우를 대비해서
        PlayerAnim.SetAnimState(EAnimState.ChannelingEnd);
        //PlayerAnimator.SetTrigger("ChannelingEnd");
        FinishChan(PlayerInfo);
        PlayerInfo.HasKnockBackImmun = false;
        //OnForceRotation(false);        
    }

    public void Cast(int index)
    {
        ICast castSkill = Skills[index] as ICast; 
        //ILazyCast lazyCastSkill = Skills[index] as ILazyCast;

        if (castSkill != null)
        {
            castSkill.Cast(PlayerInfo);
        }

        //if (lazyCastSkill != null)
        //{
        //    LazyCastInfo = (PlayerInfo, index);
        //}
        //SetCoolTime(index);

        //넉백 저항 테스트
        if (Skills[index].IsKnockBackImmun)
        {
            PlayerInfo.HasKnockBackImmun = true;
            immunTest = 0.0f;
        }
    }

    //private void SetKnockBackImmun(int index, bool immun)
    //{
    //    if (Skills[index].IsKnockBackImmun)
    //    {
    //        mPlayerInfo.HasKnockBackImmun = immun;
    //        immunTest = 0.0f;
    //    }

    //}

    public void SetSkillByIndex(Skill skill, int index)
    {
        Skills[index] = skill;
    }

    public void SetIndicator(int skillIndex, bool shouldShow)
    {
        if (shouldShow && Skills[skillIndex] as IRange != null)
        {
            m_Indicator.SetActive(true);            
            float range = (Skills[skillIndex] as IRange).SkillRange * INDICATOR_SIZE;
            m_Indicator.GetComponent<RectTransform>().sizeDelta = new Vector2(range, range);
        }
        else
        {
            if (IsShowingCustomIndicator == false)
            {
                m_Indicator.SetActive(false);
                //Logger.Log("SetIndicator false");
            }            
        }
    }

    public void SetCustomIndicator(bool shouldShow, Color? color = null, float range = 0.0f)
    {
        if (shouldShow)
        {
            Logger.Log("SetCustomIndicator true");
            IsShowingCustomIndicator = true;
            m_Indicator.SetActive(true);
            m_Indicator.GetComponent<Image>().color = color.Value;
            m_Indicator.GetComponent<RectTransform>().sizeDelta = new Vector2(range * INDICATOR_SIZE, range * INDICATOR_SIZE);
        }
        else
        {
            IsShowingCustomIndicator = false;
            m_Indicator.SetActive(false);
            m_Indicator.GetComponent<Image>().color = new Color(0.0f, 1.0f, 0.0f);
        }
        
    }

    private bool WithinRange(int index)
    {
        if (index >= 0 && index <= 5)
        {
            return true;
        }
        else
        {
            Assert.IsFalse(true, "Skill index out of range");
            //Logger.LogWarning("Skill index out of range");
            return false;
        }
    }

    private void IsMovingEvent(object sender, MoveStateChangeArgs e)
    {

        IsMoving = e.IsMoving;
        //Logger.Log(string.Format("IsMoving : {0}", IsMoving));
    }

    public void FinishAnim()
    {
        //PlayerAnim.SetAnimState(EAnimState.Cancel);
        AnimPlayingIndex = -1;
        if (AnimQueue != null)
        {
            AnimQueue.Clear();
        }
        
        AnimStop();        
        OnForceRotation(false);
    }
    private void KnockBackEvent(object sender, KnockBackEventArgs e)
    {
        if (IsChanneling && Skills[ChannelingIndex].IsKnockBackImmun == true)
        {
            IsKnockback = false;
        }
        else
        {
            IsKnockback = e.IsKnockBack;
            IsForceCancel = e.IsKnockBack;
            OnForceRotation(false);
            //PlayerAnim.SetAnimState(EAnimState.KnockBack);
            AnimStop();
            //FinishAnim();
        }
    }
    private void IsForceCancelEvent(object sender, ForceCancelChangeArgs e)
    {
        IsForceCancel = e.IsForceCancel;
        OnForceRotation(false);
        //PlayerAnim.SetAnimState(EAnimState.Cancel);
        FinishAnim();
        //Logger.Log(string.Format("IsCaneled : {0}", IsForceCancel));
    }

    private void OnGameStartEvent(object sender, GameStartEventArgs e)
    {
        //패시브 있으면 적용
        for (int index = 0; index < 6; index++)
        {
            IPassive passive = GetSkill(index) as IPassive;
            if (passive != null/* && SearchTag("passive", index)*/)
            {
                passive.RegisterPassive(PlayerInfo, PlayerInfo);
                gameObject.GetComponentInChildren<PlayerInputManager>().DisableSkill(index);
            }
        }
    }
}
