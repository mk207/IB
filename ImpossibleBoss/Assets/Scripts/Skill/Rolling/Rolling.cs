using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Rolling", menuName = "Skills/Rolling")]
public class Rolling : Skill, ISkillChain, IDash
{
    //[SerializeField, Tooltip("첫번째 스킬 사용후 다음 연계스킬에 쿨타임 주고 싶을때 사용. (구르기 같은 연속 스킬에서는 CoolTime에 숫자 넣지 말것)")]
    private float m_FirstCoolTime;
    [SerializeField, Tooltip("두번째 스킬 사용후 또는 사용안했을시 스킬 쿨타임 (구르기 같은 연속 스킬에서는 CoolTime에 숫자 넣지 말것)")]
    private float m_SecondCoolTime;

    [SerializeField, Tooltip("이동거리")]
    private float m_DashDistance;
    //[SerializeField, Tooltip("1 = 1초")]
    private float m_DashSecond;
    [SerializeField, Tooltip("두번째 구르기 시간제한")]
    private float m_TimeLimit;

    private bool mbShouldSecondRoll;

    public float DashDistance { get { return m_DashDistance; } }
    public float DashSecond { get { return m_DashSecond; } }

    private bool ShouldSecondRoll { get { return mbShouldSecondRoll; } set { mbShouldSecondRoll = value; } }

    public float FirstCoolTime { get => m_FirstCoolTime; set => m_FirstCoolTime = value; }
    public float SecondCoolTime { get => m_SecondCoolTime; set => m_SecondCoolTime = value; }
    public float TimeLimit { get => m_TimeLimit; set => m_TimeLimit = value; }
    public bool IsChaining { get; set; }

    public override void Init()
    {
        Name = "name_Rolling";
        Description = "desc_Rolling";

        m_DashSecond = GetAnimInfo(EAnimState.Dash).AnimTime;

        FirstCoolTime = 0.0f;
        CoolDown = FirstCoolTime;
        ShouldSecondRoll = false;
    }

    public void TimeOut(LivingEntity castedBy)
    {
        ShouldSecondRoll = false;
        CoolDown = SecondCoolTime;
        IsChaining = false;
    }

    public void FinishDash()
    {
        
    }

    public void Dash(LivingEntity castedBy)
    {
        RaycastHit hit;
        Ray ray = castedBy.GetComponent<PlayerMovement>().m_MainCamera.ScreenPointToRay(castedBy.GetComponent<PlayerInputManager>().MousePos);
        Physics.Raycast(ray, out hit);
        Vector3 currPos = castedBy.transform.position;
        Vector3 mousePos = new Vector3(hit.point.x, castedBy.transform.position.y, hit.point.z);
        Vector3 targetVec = (mousePos - currPos).normalized * DashDistance;

        castedBy.GetComponent<PlayerMovement>().StartDash(targetVec, DashSecond);

        if (ShouldSecondRoll)
        {
            ShouldSecondRoll = false;
            CoolDown = SecondCoolTime;
            IsChaining = false;
        }
        else
        {
            ShouldSecondRoll = true;
            CoolDown = FirstCoolTime;
            IsChaining = true;
        }
    }
}
