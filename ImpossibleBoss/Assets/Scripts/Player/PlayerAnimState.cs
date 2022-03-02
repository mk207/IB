using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EAnimState
{
    Idle,
    Casting,
    Cast,
    ChannelingStart,
    Channeling,
    ChannelingEnd,
    Dash,
    KnockBack,
    Cancel
}
public class PlayerAnimState : MonoBehaviour
{
    private PlayerSkillManager mSkillManager;
    private EAnimState mCurrAnimState;
    private Animator mPlayerAnimator;
    private float mAnimEndTime;
    private Queue<SAnimStateInfo> mAnimQueue;
    private bool mbIsConstantSpeed;
    private bool mbIsAnimating;
    private bool mbIsWalking;
    private bool mbIsEndQueue;
    private bool mbCanMove;

    private int ANIM_SPEED = Animator.StringToHash("AnimSpeed");

    private int CHANNELING_START = Animator.StringToHash("ChannelingStart");
    private int CHANNELING_END = Animator.StringToHash("ChannelingEnd");
    private int KNOCK_BACK = Animator.StringToHash("KnockBack");
    private int IS_ANIMATING = Animator.StringToHash("IsAnimating");

    public event EventHandler<AnimationEventArgs> AnimationEvent;

    public EAnimState CurrAnimState { get { return mCurrAnimState; } }
    public Queue<SAnimStateInfo> AnimQueue { get { return mAnimQueue; } }
    private Animator PlayerAnimator { get { return mPlayerAnimator; } }

    private PlayerSkillManager SkillManager { get { return mSkillManager; } }
    private float AnimEndTime { get { return mAnimEndTime; } }
    private bool IsConstantSpeed { get { return mbIsConstantSpeed; } set { mbIsConstantSpeed = value; } }
    private bool IsAnimating { get { return mbIsAnimating; } set { mbIsAnimating = value; OnAnimation();  } }
    private bool IsWalking { get { return mbIsWalking; } set { mbIsWalking = value; } }
    private bool IsEndQueue { get { return mbIsEndQueue; } set { mbIsEndQueue = value; } }
    private bool CanMove { get { return mbCanMove; } set { mbCanMove = value; } }

    private void OnAnimation()
    {
        PlayerAnimator.SetBool(IS_ANIMATING, IsAnimating);
        EventHandler<AnimationEventArgs> temp = AnimationEvent;
        if (temp != null)
        {
            temp(this, new AnimationEventArgs(IsAnimating));
        }
    }

    private void SetAnimEndTime(EAnimState state, float animTime)
    {
        mAnimEndTime = Time.time + animTime;

        switch (state)
        {
            case EAnimState.Casting: SkillManager.SetCastingTime(animTime); break;
            case EAnimState.ChannelingStart:  break;
            case EAnimState.Cancel:              
            case EAnimState.Idle:                                      
            case EAnimState.Cast:                                        
            case EAnimState.Channeling:                
            case EAnimState.ChannelingEnd:                
            case EAnimState.Dash:                
            case EAnimState.KnockBack:               
            default: break;               
        }
    }

    private SAnimStateInfo AnimDequeue()
    {
        //Logger.Log(string.Format($"Dequeue : EndQueue {0} Count {1}, State {2}", IsEndQueue ? "true" : "false", AnimQueue.Count, AnimQueue.Peek().AnimState.ToString()));
        if (AnimQueue.Count == 0)
        {
            IsAnimating = false;
            IsEndQueue = true;
            FinishAnim();
            return SAnimStateInfo.GetDefault();
        }
        else
        {
            SAnimStateInfo temp = AnimQueue.Dequeue();
            //if (AnimQueue.Count == 0)
            //{
            //    IsEndQueue = true;
            //    IsAnimating = false;
            //}
            return temp;
        }
    }

    public void SetCanMove(bool canMove)
    {
        CanMove = canMove;
    }

    public void SetAnimQueue(Queue<SAnimStateInfo> newAnimQueue)
    {
        mAnimQueue = newAnimQueue;
        IsEndQueue = false;
        PlayNextAnim(AnimDequeue());
    }

    public void SetController(AnimatorOverrideController controller)
    {
        PlayerAnimator.runtimeAnimatorController = controller;
    }

    public void SetAnimState(EAnimState newState)
    {
        switch (newState)
        {
            case EAnimState.Cancel:
                {
                    IsAnimating = false;
                    PlayerAnimator.SetTrigger("Cancel");
                    mCurrAnimState = EAnimState.Idle;
                    FinishAnim();
                    break;
                }
            case EAnimState.Idle:
                {
                    IsAnimating = false;
                    //PlayerAnimator.SetTrigger("Idle");
                    mCurrAnimState = EAnimState.Idle;
                    //Logger.Log("SetAnimState Idle");
                    break;
                }                  
            case EAnimState.Casting:
                {
                    IsAnimating = true;
                    PlayerAnimator.SetTrigger("Casting");
                    mCurrAnimState = EAnimState.Casting;
                    break;
                }
            case EAnimState.Cast:
                {
                    IsAnimating = true;
                    PlayerAnimator.ResetTrigger("Cast");
                    PlayerAnimator.SetTrigger("Cast");
                    mCurrAnimState = EAnimState.Cast;
                    break;
                }
            case EAnimState.ChannelingStart:
                {
                    IsAnimating = true;
                    PlayerAnimator.ResetTrigger(CHANNELING_END);
                    PlayerAnimator.SetTrigger(CHANNELING_START);
                    mCurrAnimState = EAnimState.ChannelingStart;
                    break;
                }                
            case EAnimState.Channeling:
                {
                    IsAnimating = true;
                    PlayerAnimator.SetTrigger("Channeling");
                    mCurrAnimState = EAnimState.Channeling;
                    break;
                }
            case EAnimState.ChannelingEnd:
                {
                    IsAnimating = false;
                    PlayerAnimator.SetTrigger(CHANNELING_END);
                    mCurrAnimState = EAnimState.Idle;
                    FinishAnim();
                    break;
                }
            case EAnimState.Dash:
                {
                    IsAnimating = true;
                    PlayerAnimator.SetTrigger("Dash");
                    mCurrAnimState = EAnimState.Dash;
                    break;
                }
            case EAnimState.KnockBack:
                {
                    IsAnimating = true;
                    //PlayerAnimator.SetTrigger("KnockBack");
                    PlayerAnimator.SetBool(KNOCK_BACK, true);
                    mCurrAnimState = EAnimState.KnockBack;
                    break;
                }
            default:
                {
                    IsAnimating = false;
                    //PlayerAnimator.SetTrigger("Idle");
                    mCurrAnimState = EAnimState.Idle;
                    break;
                }
        }
    }

    private void FinishAnim()
    {
        IsAnimating = false;
        AnimQueue.Clear();
        SkillManager.FinishAnim();
    }

    private void OnWalkingHandler(object sender, MoveStateChangeArgs e)
    {
        //응축의 시발일격의 경우때문에 Move로 인한 취소는 따로 계산해야 할듯함
        //Cancel은 esc에 의한 강제 취소만하고 무빙은 무빙대로 처리해야할듯함.
        if (CurrAnimState != EAnimState.Dash)
        {
            IsWalking = e.IsMoving;
            PlayerAnimator.SetBool("IsWalking", e.IsMoving);
        }
        

        //굳이 따질 필요가 있을까
        //if (CurrAnimState != EAnimState.Dash && CanMove == false && IsAnimating && IsWalking)
        //{
        //    SetAnimState(EAnimState.Cancel);
        //}
    }
    private void OnForceCancelHandler(object sender, ForceCancelChangeArgs e)
    {
        SetAnimState(EAnimState.Cancel);        
    }
    private void OnKnockBackHandler(object sender, KnockBackEventArgs e)
    {
        //넉백은 LivingEntity에서 처리하므로 애니는 받아서 사용
        if (e.IsKnockBack)
        {
            AnimQueue.Clear();
            PlayerAnimator.SetBool(KNOCK_BACK, true);
        }
        else
        {
            PlayerAnimator.SetBool(KNOCK_BACK, false);
        }
        

        //SetAnimState(EAnimState.KnockBack);
        //Logger.Log($"Knock Back : {e.IsKnockBack}");
    }

    private void OnDashHandler(object sender, DashEventArgs e)
    {
        if (e.IsDashing == false)
        {
            IsAnimating = false;
        }
        //PlayerAnimator.SetFloat(ANIM_SPEED, 1.0f / e.DashSecond);
        //if (e.IsDashing)
        //{
        //    SetAnimState(EAnimState.Dash);
        //}
        //else
        //{
        //    IsAnimating = false;
        //}

        //PlayerAnimator.SetBool("IsDashing", e.IsDashing);        
    }

    // Start is called before the first frame update
    void Awake()
    {
        mPlayerAnimator = GetComponentInChildren<Animator>();
        mSkillManager = GetComponentInChildren<PlayerSkillManager>();
        GetComponentInChildren<PlayerMovement>().DashEvent += OnDashHandler;
        GetComponentInChildren<PlayerMovement>().PlayerMovingEvent += OnWalkingHandler;
        GetComponentInChildren<PlayerInformation>().KnockBackEvent += OnKnockBackHandler;
        GetComponentInChildren<PlayerInputManager>().IsForceCancelEvent += OnForceCancelHandler;
    }

    private void Start()
    {
        SetAnimState(EAnimState.Idle);
        mAnimQueue = new Queue<SAnimStateInfo>();        
    }

    private void PlayNextAnim(SAnimStateInfo animStateInfo)
    {
        Logger.Log($"Play Next Anim : {animStateInfo.AnimState.ToString()}");
        if (animStateInfo.IsConstantSpeed)
        {
            IsConstantSpeed = true;
            SetAnimState(animStateInfo.AnimState);
            PlayerAnimator.SetFloat(ANIM_SPEED, 1.0f);            
            //AnimEndTime = Time.time + animStateInfo.AnimTime;
        }
        else
        {
            IsConstantSpeed = false;
            SetAnimState(animStateInfo.AnimState);
            PlayerAnimator.SetFloat(ANIM_SPEED, 1.0f / animStateInfo.AnimTime);
            //AnimEndTime = Time.time + 1.0f / animStateInfo.AnimTime;
            //AnimEndTime = Time.time + animStateInfo.AnimTime;
        }
        //PlayerEffect(animStateInfo.Effect, animStateInfo.AnimTime);
        SetAnimEndTime(animStateInfo.AnimState, animStateInfo.AnimTime);
        SkillManager.NextAction();
    }

    //private void PlayerEffect(GameObject effect, float animTime)
    //{
    //    GameObject spawnEffect = Instantiate(effect);
    //}

    // Update is called once per frame
    void Update()
    {
        //if (CurrAnimState != EAnimState.Idle && IsEndQueue)
        //{
        //    SetAnimState(EAnimState.Idle);
        //    IsEndQueue = false;
        //}
        //Logger.Log($"{CurrAnimState} : {CurrAnimState != EAnimState.Idle && IsEndQueue == false && AnimQueue.Count > 0 && AnimEndTime <= Time.time}");
        if (CurrAnimState != EAnimState.Idle && IsEndQueue == false /*&& AnimQueue.Count > 0*/ && AnimEndTime <= Time.time)
        {            
            PlayNextAnim(AnimDequeue());            
        }
    }
}
