using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Rolling", menuName = "Skills/Rolling")]
public class Rolling : Skill, ISkillChain, IDash
{
    //[SerializeField, Tooltip("ù��° ��ų ����� ���� ���轺ų�� ��Ÿ�� �ְ� ������ ���. (������ ���� ���� ��ų������ CoolTime�� ���� ���� ����)")]
    private float m_FirstCoolTime;
    [SerializeField, Tooltip("�ι�° ��ų ����� �Ǵ� ���������� ��ų ��Ÿ�� (������ ���� ���� ��ų������ CoolTime�� ���� ���� ����)")]
    private float m_SecondCoolTime;

    [SerializeField, Tooltip("�̵��Ÿ�")]
    private float m_DashDistance;
    //[SerializeField, Tooltip("1 = 1��")]
    private float m_DashSecond;
    [SerializeField, Tooltip("�ι�° ������ �ð�����")]
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
