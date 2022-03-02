using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;

public class PlayerInputManager : MonoBehaviour
{
    public event System.EventHandler<ForceCancelChangeArgs> IsForceCancelEvent;

    [SerializeField]
    private PlayerInputActions m_InputActions;
    private PlayerInput mPlayerInput;
    private InputActionMap mPlayerMap;
    private PlayerSkillManager mSkillManager;
    //LivingEctity of PlayerCharacter
    private LivingEntity mOwner;
    private Vector2 mDir;
    private Vector2 mMousePosition;

    private float mEndTime; 

    #region Skills Handler
    private Action<InputAction.CallbackContext> mSkillStartHandler;
    private Action<InputAction.CallbackContext> mSkill1PerformHandler;
    private Action<InputAction.CallbackContext> mSkill2PerformHandler;
    private Action<InputAction.CallbackContext> mSkill3PerformHandler;
    private Action<InputAction.CallbackContext> mSkill4PerformHandler;
    private Action<InputAction.CallbackContext> mSkill5PerformHandler;
    private Action<InputAction.CallbackContext> mSkill6PerformHandler;
    #endregion

    #region FixHead Handler
    private Action<InputAction.CallbackContext> mFixHeadStartHandler;
    private Action<InputAction.CallbackContext> mFixHeadPerformHandler;
    #endregion

    #region Cancel Handler
    private Action<InputAction.CallbackContext> mCancelStartHandler;
    private Action<InputAction.CallbackContext> mCancelPerformHandler;
    #endregion

    #region Menu Handler
    private Action<InputAction.CallbackContext> mMenuStartHandler;
    #endregion

    private int mReadiedSkill;
    private float mPressedTime;
    private bool mbIsPressed = false;
    private bool mbIsFixedHead;
    private bool mbIsPaused;

    private OptionManager OM;
    public Vector2 Dir { get { return mDir; } set { mDir = value; } }
    public PlayerInputActions InputActions { get { return m_InputActions; } }
    public LivingEntity Owner { get { return mOwner; } }
    public Vector2 MousePos { get { return mMousePosition; } set { mMousePosition = value; } }

    private bool IsPressed { get { return mbIsPressed; } set { mbIsPressed = value; } }
    public bool IsFixedHead { get { return mbIsFixedHead; } set { mbIsFixedHead = value; } }
    public bool IsPaused { get { return mbIsPaused; } set { mbIsPaused = value; } }
    private float PressedTime { get { return mPressedTime; } set { mPressedTime = value; } }
    private int ReadiedSkill { get { return mReadiedSkill; } set { mReadiedSkill = value; } }
    private InputActionMap PlayerMap { get { return mPlayerMap; } }

    private PlayerSkillManager SkillManager { get { return mSkillManager; } }
    
    public void SelectSkill(int index)
    {
        Logger.Log("Player Selected Skill :" + index);
        //SkillManager.Cast(Owner, index);
        SkillManager.TryCast(index);
        //SkillManager.Casting(index);
    }
    
    public void TestLanguageToggle()
    {
        if(OM.Language == ELanguage.Kor)
        {
            OM.Language = ELanguage.Eng;           
        }
        else
        {
            OM.Language = ELanguage.Kor;
        }
        OM.ChangeLanguage();
    }
    void Awake()
    {
        GameManager.Instance.GamePauseEvent += OnGamePause;
        mSkillManager = GetComponent<PlayerSkillManager>();
        mOwner = gameObject.GetComponent<LivingEntity>();
        mPlayerInput = GetComponent<PlayerInput>();
        mPlayerMap = mPlayerInput.actions.FindActionMap("Player");
        m_InputActions = RebindInputManager.InputActions;
        //m_InputActions = new PlayerInputActions();
        //if (InputActions == null)
        //{
        //    m_InputActions = new PlayerInputActions();
        //    InputActions.Player.Skill1.started += skillNum => { ReadiedSkill = (int)skillNum.ReadValue<float>(); IsPressed = true; Logger.Log("started" + skillNum.ReadValue<float>().ToString()); };
        //    InputActions.Player.Skill1.performed += skillNum => { if (ReadiedSkill == 0) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };
        //    InputActions.Player.Skill2.started += skillNum => { ReadiedSkill = (int)skillNum.ReadValue<float>(); IsPressed = true; Logger.Log("started" + skillNum.ReadValue<float>().ToString()); };
        //    InputActions.Player.Skill2.performed += skillNum => { if (ReadiedSkill == 1) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };
        //    InputActions.Player.Skill3.started += skillNum => { ReadiedSkill = (int)skillNum.ReadValue<float>(); IsPressed = true; Logger.Log("started" + skillNum.ReadValue<float>().ToString()); };
        //    InputActions.Player.Skill3.performed += skillNum => { if (ReadiedSkill == 2) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };
        //    InputActions.Player.Skill4.started += skillNum => { ReadiedSkill = (int)skillNum.ReadValue<float>(); IsPressed = true; Logger.Log("started" + skillNum.ReadValue<float>().ToString()); };
        //    InputActions.Player.Skill4.performed += skillNum => { if (ReadiedSkill == 3) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };
        //    InputActions.Player.Skill5.started += skillNum => { ReadiedSkill = (int)skillNum.ReadValue<float>(); IsPressed = true; Logger.Log("started" + skillNum.ReadValue<float>().ToString()); };
        //    InputActions.Player.Skill5.performed += skillNum => { if (ReadiedSkill == 4) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };
        //    InputActions.Player.Skill6.started += skillNum => { ReadiedSkill = (int)skillNum.ReadValue<float>(); IsPressed = true; Logger.Log("started" + skillNum.ReadValue<float>().ToString()); };
        //    InputActions.Player.Skill6.performed += skillNum => { if (ReadiedSkill == 5) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };

        //    InputActions.Player.FixHead.started += _ => { IsFixedHead = true; Logger.Log("Fix Head true"); };
        //    InputActions.Player.FixHead.performed += _ => { IsFixedHead = false; Logger.Log("Fix Head false"); };

        //    InputActions.Player.Cancel.started += skillNum => { OnIsForceCanelEvent(new ForceCancelChangeArgs(true)); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); };
        //    InputActions.Player.Cancel.performed += skillNum => { OnIsForceCanelEvent(new ForceCancelChangeArgs(false)); };
        //    //InputActions.Player.Skill.canceled += skillNum => { Logger.Log("canceled" + skillNum.ReadValue<float>().ToString());/*SelectSkill((int)skillNum.ReadValue<float>()); ReadiedSkill = 0; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false);*/ };


        //    InputActions.Player.Skill1.Enable();
        //    InputActions.Player.Skill2.Enable();
        //    InputActions.Player.Skill3.Enable();
        //    InputActions.Player.Skill4.Enable();
        //    InputActions.Player.Skill5.Enable();
        //    InputActions.Player.Skill6.Enable();
        //    InputActions.Player.MousePosition.Enable();
        //    InputActions.Player.FixHead.Enable();
        //    InputActions.Player.Cancel.Enable();
        //}


        /*
         * 리스타트시 이 시발 오브젝트들이 전부 제대로 해제되고 다시 할당되는게 아닌건지 완벽한 최초의 상태로 되돌아 가지를 않음
         * 예를들면 밑의 InputActions.Player.Skill1.started += skillNum 이 리스타트 할때마다 쌓여서 에러를 쳐뱉음.
         * 그냥 null초기화를 해야하나 싶지만 이런식의 관리는 끝이없을거같음
         * 클린업용 씬으로 이동해서 해결해야할듯.
         */

        //처음에는 Skill에 버튼 1~6을 해놨더니
        //1을 꾹누르는 도중에 2를 누르면 반응안함 -> 이거는 의도와 완전 반대였지만 로직의 문제지 버그 문제는 아니었음
        //심각한 문제는 2를 계속 누르는 와중에 1을 때버리면 즉시 2의 started가 발동되고 2를 때도 performed가 실행이 안됨 -> 해결불가

        mSkillStartHandler = skillNum => { ReadiedSkill = (int)skillNum.ReadValue<float>(); IsPressed = true; Logger.Log("started" + skillNum.ReadValue<float>().ToString()); };
        mSkill1PerformHandler = skillNum => { if (ReadiedSkill == 0) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };
        mSkill2PerformHandler = skillNum => { if (ReadiedSkill == 1) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };
        mSkill3PerformHandler = skillNum => { if (ReadiedSkill == 2) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };
        mSkill4PerformHandler = skillNum => { if (ReadiedSkill == 3) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };
        mSkill5PerformHandler = skillNum => { if (ReadiedSkill == 4) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };
        mSkill6PerformHandler = skillNum => { if (ReadiedSkill == 5) { SelectSkill(ReadiedSkill); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); } Logger.Log("performed" + skillNum.ReadValue<float>().ToString()); };

        //RemoveInputEvent();

        InputActions.Player.Skill1.started += mSkillStartHandler;
        InputActions.Player.Skill1.performed += mSkill1PerformHandler;
        InputActions.Player.Skill2.started += mSkillStartHandler;
        InputActions.Player.Skill2.performed += mSkill2PerformHandler;
        InputActions.Player.Skill3.started += mSkillStartHandler;
        InputActions.Player.Skill3.performed +=  mSkill3PerformHandler;
        InputActions.Player.Skill4.started += mSkillStartHandler;
        InputActions.Player.Skill4.performed +=  mSkill4PerformHandler;
        InputActions.Player.Skill5.started += mSkillStartHandler;
        InputActions.Player.Skill5.performed +=  mSkill5PerformHandler;
        InputActions.Player.Skill6.started += mSkillStartHandler;
        InputActions.Player.Skill6.performed +=  mSkill6PerformHandler;

        mFixHeadStartHandler = _ => { IsFixedHead = true; Logger.Log("Fix Head"); };
        mFixHeadPerformHandler = _ => { IsFixedHead = false; Logger.Log("Free Head"); };

        InputActions.Player.FixHead.started += mFixHeadStartHandler;
        InputActions.Player.FixHead.performed += mFixHeadPerformHandler;

        mCancelStartHandler = skillNum => { OnIsForceCanelEvent(new ForceCancelChangeArgs(true)); ReadiedSkill = -1; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false); };
        mCancelPerformHandler = skillNum => { OnIsForceCanelEvent(new ForceCancelChangeArgs(false)); };

        InputActions.Player.Cancel.started += mCancelStartHandler;
        InputActions.Player.Cancel.performed += mCancelPerformHandler;
        //InputActions.Player.Skill.canceled += skillNum => { Logger.Log("canceled" + skillNum.ReadValue<float>().ToString());/*SelectSkill((int)skillNum.ReadValue<float>()); ReadiedSkill = 0; IsPressed = false; mSkillManager.SetIndicator(ReadiedSkill, false);*/ };

        InputActions.Player.Menu.performed += _ => { };

        InputActions.Player.Skill1.Enable();
        InputActions.Player.Skill2.Enable();
        InputActions.Player.Skill3.Enable();
        InputActions.Player.Skill4.Enable();
        InputActions.Player.Skill5.Enable();
        InputActions.Player.Skill6.Enable();
        InputActions.Player.MousePosition.Enable();
        InputActions.Player.FixHead.Enable();
        InputActions.Player.Cancel.Enable();

        //InputActions.UI.Menu.Enable();

        OM = FindObjectOfType<OptionManager>();
        FindObjectOfType<GameManager>().GameOverEvent += GameOver;
    }

    private void OnGamePause(object sender, GamePauseEventArgs e)
    {
        IsPaused = e.IsPaused;

        if (IsPaused)
        {
            InputActions.Player.Skill1.Disable();
            InputActions.Player.Skill2.Disable();
            InputActions.Player.Skill3.Disable();
            InputActions.Player.Skill4.Disable();
            InputActions.Player.Skill5.Disable();
            InputActions.Player.Skill6.Disable();
            InputActions.Player.MousePosition.Disable();
            InputActions.Player.FixHead.Disable();
            InputActions.Player.Cancel.Disable();
            //PlayerMap.Disable();
        }
        else
        {
            InputActions.Player.Skill1.Enable();
            InputActions.Player.Skill2.Enable();
            InputActions.Player.Skill3.Enable();
            InputActions.Player.Skill4.Enable();
            InputActions.Player.Skill5.Enable();
            InputActions.Player.Skill6.Enable();
            InputActions.Player.MousePosition.Enable();
            InputActions.Player.FixHead.Enable();
            InputActions.Player.Cancel.Enable();
        }
    }

    private void GameOver(object sender, GameOverEventArgs e)
    {
        Dir = Vector2.zero;
        //InputActions.Player.Disable();
        PlayerMap.Disable();
        InputActions.UI.Enable();
        RemoveInputEvent();
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        if (IsPaused == false)
        {
            Dir = ctx.ReadValue<Vector2>();
        }
        
        //Logger.Log("Dir : " + ctx.ReadValue<Vector2>());
    }

    public void MousePosition(InputAction.CallbackContext ctx)
    {
        MousePos = ctx.ReadValue<Vector2>();
        //Logger.Log("Mouse Pos : " + ctx.ReadValue<Vector2>());
    }

    // Update is called once per frame
    void Update()
    {
        //if (GameManager.Instance != null && GameManager.Instance.IsGameover)
        //{
        //    Dir = Vector2.zero;
        //    return;
        //}

        if (IsPressed)
        {
            PressedTime += Time.deltaTime;
            //Logger.Log(PressedTime);
            if (PressedTime >= 0.5f)
            {
                mSkillManager.SetIndicator(ReadiedSkill, true);
            }
        }
        else
        {
            PressedTime = 0.0f;
        }
    }

    private void OnIsForceCanelEvent(ForceCancelChangeArgs e)
    {
        System.EventHandler<ForceCancelChangeArgs> OnForceCancelEvent = IsForceCancelEvent;
        if (OnForceCancelEvent != null)
        {
            OnForceCancelEvent(this, e);
        }
    }

    public void DisableSkill(int index)
    {
        switch (index)
        {
            case 0: InputActions.Player.Skill1.Disable(); break;
            case 1: InputActions.Player.Skill2.Disable(); break;
            case 2: InputActions.Player.Skill3.Disable(); break;
            case 3: InputActions.Player.Skill4.Disable(); break;
            case 4: InputActions.Player.Skill5.Disable(); break;
            case 5: InputActions.Player.Skill6.Disable(); break;
            default: throw new System.ArgumentOutOfRangeException(nameof(index));
        }
    }

    public void RemoveInputEvent()
    {
        //보스 스테이지 리스타트때 호출해야함
        InputActions.Player.Skill1.started -= mSkillStartHandler;
        InputActions.Player.Skill1.performed -= mSkill1PerformHandler;
        InputActions.Player.Skill2.started -= mSkillStartHandler;
        InputActions.Player.Skill2.performed -= mSkill2PerformHandler;
        InputActions.Player.Skill3.started -= mSkillStartHandler;
        InputActions.Player.Skill3.performed -= mSkill3PerformHandler;
        InputActions.Player.Skill4.started -= mSkillStartHandler;
        InputActions.Player.Skill4.performed -= mSkill4PerformHandler;
        InputActions.Player.Skill5.started -= mSkillStartHandler;
        InputActions.Player.Skill5.performed -= mSkill5PerformHandler;
        InputActions.Player.Skill6.started -= mSkillStartHandler;
        InputActions.Player.Skill6.performed -= mSkill6PerformHandler;


        InputActions.Player.FixHead.started -= mFixHeadStartHandler;
        InputActions.Player.FixHead.performed -= mFixHeadPerformHandler;


        InputActions.Player.Cancel.started -= mCancelStartHandler;
        InputActions.Player.Cancel.performed -= mCancelPerformHandler;
    }
}
