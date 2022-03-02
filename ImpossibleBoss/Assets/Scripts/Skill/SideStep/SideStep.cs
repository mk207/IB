using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SideStep", menuName = "Skills/SideStep")]
public class SideStep : Skill, IDash
{
    [SerializeField, Tooltip("이동거리")]
    private float m_DashDistance;
    //[SerializeField, Tooltip("1 = 1초")]
    private float m_DashSecond;

    public float DashSecond => m_DashSecond;

    public float DashDistance => m_DashDistance;

    public void Dash(LivingEntity castedBy)
    {
        Vector3 pos = castedBy.transform.position;
        Quaternion rot = Quaternion.Euler(0.0f, -90.0f, 0.0f);
        RaycastHit hit;
        Ray ray = castedBy.GetComponent<PlayerMovement>().m_MainCamera.ScreenPointToRay(castedBy.GetComponent<PlayerInputManager>().MousePos);
        Physics.Raycast(ray, out hit);
        castedBy.GetComponent<PlayerMovement>().StartDash(rot * (pos - hit.point).normalized * DashDistance, DashSecond);
    }

    public void FinishDash()
    {
        throw new System.NotImplementedException();
    }

    //public override void Casting()
    //{

    //}

    public override void Init()
    {
        Name = "name_SideStep";
        Description = "desc_SideStep";
        m_DashSecond = GetAnimInfo(EAnimState.Dash).AnimTime;
    }
}
