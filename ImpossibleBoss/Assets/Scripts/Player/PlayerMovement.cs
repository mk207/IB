using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class PlayerMovement : MonoBehaviour
{
    public Camera m_MainCamera;
    private PlayerInputManager mPlayerInput;
    private float mRotSpeed;
    private Rigidbody mPlayerRigidbody;
    private float mTargetYaw;
    private PlayerInformation mPlayerDamageInformation;
    private Vector3 mPrePos;
    private Vector3 mDashTargetPos;
    private Vector3 mDashOriginPos;
    private Vector3 mForceDir;
    private bool mbIsForceRot;
       
    private Vector3 mKnockBackDir;
    private float mKnockBackSpeed;
    private bool mbIsKnockBack;

    private float mDashLerpAlpha;
    private float mDashSecond;
    private Vector3 mDashSpeed;
    private bool mbIsDashing = false;

    private float mChargeSpeedRate = 1.0f;
    private float mChargeMaxRotationAngle;
    private bool mbIsCharging = false;
    private bool mbIsAnimating = false;
    private bool mbShouldObstructInput = false;

    public event EventHandler<MoveStateChangeArgs> PlayerMovingEvent;
    public event EventHandler<DashEventArgs> DashEvent;
    //public float InitMoveSpeed;
    public float InitRotSpeed;

    public float ChargeSpeedRate { get { return mChargeSpeedRate; } set { mChargeSpeedRate = value; } }
    public float ChargeMaxRotationAngle { get { return mChargeMaxRotationAngle; } set { mChargeMaxRotationAngle = value; } }
    public Vector3 ForceDir { get { return mForceDir; } set { mForceDir = value; } }
    public Vector3 DashTargetPos { get { return mDashTargetPos; } set { mDashTargetPos = value; } }
    public bool IsForceRot { get { return mbIsForceRot; } set { mbIsForceRot = value; } }    
    public bool ShouldObstructInput { get { return mbShouldObstructInput; } set { mbShouldObstructInput = value; } }
    private Rigidbody PlayerRigidbody { get { return mPlayerRigidbody; } }
    private PlayerInformation PlayerInformation { get { return mPlayerDamageInformation; } }
    private float TargetYaw { get { return mTargetYaw; } set { mTargetYaw = value; } }
    private float RotSpeed { get { return mRotSpeed; } set { mRotSpeed = value; } }
    private PlayerInputManager InputManager { get { return mPlayerInput; } }
    private Vector3 PrePos { get { return mPrePos; } set { mPrePos = value; } }
    private Vector3 DashOriginPos { get { return mDashOriginPos; } set { mDashOriginPos = value; } }
    private float DashLerpAlpha { get { return mDashLerpAlpha; } set { mDashLerpAlpha = value; } }
    private float DashSecond { get { return mDashSecond; } set { mDashSecond = value; } }
    private Vector3 DashSpeed { get { return mDashSpeed; } set { mDashSpeed = value; } }
    private bool IsDashing { get { return mbIsDashing; } set { mbIsDashing = value; } }

    private float KnockBackSpeed { get { return mKnockBackSpeed; } set { mKnockBackSpeed = value; } }
    private Vector3 KnockBackDir { get { return mKnockBackDir; } set { mKnockBackDir = value; } }
    private bool IsKnockBack { get { return mbIsKnockBack; } set { mbIsKnockBack = value; } }
    
    private bool IsCharhing { get { return mbIsCharging; } set { mbIsCharging = value; } }

    private bool IsAnimating { get { return mbIsAnimating; } set { mbIsAnimating = value; } }
    //private Camera MainCamera { get { return mMainCamera; } }
    private void Start()
    {
        // 사용할 컴포넌트들의 참조를 가져오기
        mPlayerInput = GetComponent<PlayerInputManager>();
        mPlayerRigidbody = GetComponent<Rigidbody>();
        //playerAnimator = GetComponent<Animator>();
        mPlayerDamageInformation = GetComponent<PlayerInformation>();
        //GetComponent<PlayerSkillManager>().CastingEvent += OnForceRot;
        mPlayerDamageInformation.KnockBackEvent += KnockBack;
        //MoveSpeed = /* InitMoveSpeed;*/
        RotSpeed = InitRotSpeed;
        //ChargeSpeedRate = 1.0f;
        //GetComponent<LivingEntity>().CCEvent += CC;
    }

    public void ForceRot(Vector3 dir, bool isForceRot)
    {
        IsForceRot = isForceRot;
        ForceDir = dir;
        gameObject.transform.LookAt(ForceDir + transform.position);
    }

    private void KnockBack(object sender, KnockBackEventArgs e)
    {
        KnockBackDir = e.KnockBackDir;
        KnockBackSpeed = e.KnockBackPower;
        IsKnockBack = e.IsKnockBack;
    }

    //private void OnForceRot(object sender, ForceRotationEventArgs e)
    //{
    //    IsForceRot = e.IsForceRot;
    //    ForceDir = e.Dir;
    //}

    private void OnDashEvent(DashEventArgs e)
    {
        EventHandler<DashEventArgs> temp = DashEvent;
        if (temp != null)
        {
            temp(this, e);
        }
    }

    public void StartDash(Vector3 targetVec, float dashSecond)
    {
        IsDashing = true;
        DashSecond = dashSecond;
        //DashTargetPos = transform.position + targetPos;
        //DashOriginPos = transform.position;
        DashLerpAlpha = 0.0f;

        DashSpeed = targetVec / dashSecond;
        OnDashEvent(new DashEventArgs(DashSecond, IsDashing));
        //오류 때문에 굳이 dash할때 false해줌
        OnMoveEvent(new MoveStateChangeArgs(PlayerInformation, transform.position, false));

        //PlayerInformation.CanControl = false;
        PlayerInformation.InterruptsController(ECC.None, 9999.9f);
    }

    public void FinishDash()
    {
        IsDashing = false;
        OnDashEvent(new DashEventArgs(1.0f, IsDashing));
        //Logger.Log("FinishDash");
        PlayerRigidbody.velocity = Vector3.zero;
        PlayerInformation.RecoveryController();
    }

    public void StartCharge(float speed, float angle)
    {
        IsCharhing = true;
        ChargeMaxRotationAngle = angle;
        ChargeSpeedRate = speed;
    }
    public void EndCharge()
    {
        IsCharhing = false;
    }

    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨
    private void FixedUpdate()
    {
        if (IsCharhing)
        {
            ChargeMove();
            ChargeRotate();
        }
        else
        {
            Move();
            Rotate();
        }
        

        if (transform.position != PrePos /*&& IsDashing == false*/)
        {
            //Logger.Log("transform.hasChanged true");
            //transform.hasChanged = false;
            //Logger.Log($"{PlayerRigidbody.velocity}");
            PrePos = transform.position;
            OnMoveEvent(new MoveStateChangeArgs(PlayerInformation, transform.position, true));
        }
        else
        {
            OnMoveEvent(new MoveStateChangeArgs(PlayerInformation, transform.position, false));
            //Logger.Log("transform.hasChanged false");
        }
        //playerAnimator.SetFloat("Move", Mathf.Clamp(mPlayerInput.move.magnitude, -1.0f, 1.0f));
    }

    private void MousePos(out RaycastHit hit)
    {
        
        Ray ray = m_MainCamera.ScreenPointToRay(InputManager.MousePos);
        Physics.Raycast(ray, out hit);
    }

    private void ChargeMove()
    {
        //PlayerRigidbody.MovePosition( transform.position + (hit.point - transform.position).normalized * Time.deltaTime * ChargeSpeedRate );
        //PlayerRigidbody.MovePosition(transform.position + transform.forward * PlayerInformation.GetMoveSpeed() * Time.fixedDeltaTime * ChargeSpeedRate);
        PlayerRigidbody.velocity = transform.forward * PlayerInformation.GetMoveSpeed() * ChargeSpeedRate;
    }    

    private void ChargeRotate()
    {
        RaycastHit hit;
        MousePos(out hit);

        Vector3 mouseNormal = new Vector3 (hit.point.x - transform.position.x, 0.0f, hit.point.z - transform.position.z).normalized;
        float dot = Vector3.Dot(transform.right, mouseNormal);
        float angle = ChargeMaxRotationAngle * Time.fixedDeltaTime;

        //양수면 마우스가 오른쪽에 있다는 뜻임
        if (dot >= 0)
        {
            gameObject.transform.Rotate(new Vector3(0.0f, angle, 0.0f));
        }
        else
        {
            gameObject.transform.Rotate(new Vector3(0.0f, -angle, 0.0f));
        }        
    }

    //입력값에 따라 캐릭터를 앞뒤로 움직임
    private void Move()
    {
        if (PlayerInformation.CanControl && ShouldObstructInput == false)
        {
            Vector3 move = InputManager.Dir;
            Vector3 Dir = new Vector3(move.x, 0.0f, move.y);
            Vector3 moveDistance = Dir * PlayerInformation.GetMoveSpeed()/*MoveSpeed*/ * Time.fixedDeltaTime;
            PlayerRigidbody.MovePosition(PlayerRigidbody.position + moveDistance);

            if (Dir != Vector3.zero)
            {
                if (Dir.x < 0.0f)
                {
                    TargetYaw = -Mathf.Acos(Vector3.Dot(new Vector3(0.0f, 0.0f, 1.0f), Dir.normalized)) * Mathf.Rad2Deg;
                }
                else
                {
                    TargetYaw = Mathf.Acos(Vector3.Dot(new Vector3(0.0f, 0.0f, 1.0f), Dir.normalized)) * Mathf.Rad2Deg;
                }
                IsForceRot = false;
            }                    
        }
        else
        {
            //if (PlayerInformation.IsKnockBack)
            //{
            //    PlayerRigidbody.MovePosition(transform.position + PlayerInformation.KnockBackDir * Time.fixedDeltaTime * PlayerInformation.m_KnockBackSpeed);
            //}
            if (IsKnockBack)
            {
                PlayerRigidbody.MovePosition(transform.position + KnockBackDir * Time.fixedDeltaTime * KnockBackSpeed);
            }

            if (IsDashing)
            {
                //PlayerRigidbody.MovePosition(Vector3.Lerp(DashOriginPos, DashTargetPos, DashLerpAlpha));
                PlayerRigidbody.velocity = DashSpeed;
                DashLerpAlpha += (Time.fixedDeltaTime / DashSecond);
                //Logger.Log("Lerp : " + DashLerpAlpha + " // " + DashTargetPos);

                //LivingEntity에서 InterruptsController때문에 DashLerpAlpha이 0.99999999999에서 멈춘것이었음.
                //CanControl을 따로 계산해서 발생한일 
                //그렇다고 발생주체에서 시작끝을 전부 컨트롤 하는것도 힘들고...
                //CanControl만으로 하는것도 힘들고...

                if (DashLerpAlpha >= 1.0f)
                {
                    FinishDash();
                }
            }
        }
    }

    

    private void OnMoveEvent(MoveStateChangeArgs e)
    {
        EventHandler<MoveStateChangeArgs> temp = PlayerMovingEvent;
        if (temp != null)
        {
            temp(this, e);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerRigidbody.velocity = Vector3.zero;
        PlayerRigidbody.angularVelocity = Vector3.zero;

        //Logger.Log(collision);
    }

    private void Rotate()
    {        
        if (IsForceRot)
        {
            //Logger.Log("ForceRot : " + IsForceRot);
            gameObject.transform.LookAt(ForceDir + transform.position);
            return;
        }

        if (ShouldObstructInput == false)
        {
            if (InputManager.IsFixedHead)
            {
                RaycastHit hit;
                Ray ray = m_MainCamera.ScreenPointToRay(InputManager.MousePos);
                Physics.Raycast(ray, out hit);
                gameObject.transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
            }
            else
            {
                PlayerRigidbody.rotation = Quaternion.Euler(0.0f, Mathf.LerpAngle(PlayerRigidbody.rotation.eulerAngles.y, TargetYaw, RotSpeed * Time.fixedDeltaTime), 0.0f);
                //Logger.Log("Rot : " + TargetYaw);
            }
        }
        
    }
}
