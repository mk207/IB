using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Blink", menuName = "Skills/Blink")]
public class Blink : Skill, IDash, ILazyCast
{
    [SerializeField, Tooltip("순간이동 거리")]
    private float m_SkillRange;
    [SerializeField]
    private STakeDamageParams m_TakeDamageParams;
    private Vector3 mTargetPos;
    private Vector3 mCurrPos;
    private Vector3 TargetPos { get { return mTargetPos; } set { mTargetPos = value; } }
    private Vector3 CurrPos { get { return mCurrPos; } set { mCurrPos = value; } }

    public float DashDistance => m_SkillRange;

    public float DashSecond => 0.7f;

    public Sprite GetIcon()
    {
        return Icon;
    }

    public override void Init()
    {
        Name = "name_Blink";
        Description = "desc_Blink";

        m_TakeDamageParams = new STakeDamageParams();
        m_TakeDamageParams.damageAmount = 0;
        m_TakeDamageParams.cc = ECC.KnockBack;
        m_TakeDamageParams.ccAmount = 5.0f;
    }

    public void LazyCast(LivingEntity castedBy)
    {       
        
        //           Boss      Enemy
        int mask = (1 << 8) | (1 << 9);

        if ((TargetPos - CurrPos).sqrMagnitude <= DashDistance * DashDistance)
        {
            castedBy.transform.position = TargetPos;
        }
        else
        {
            TargetPos = CurrPos + (TargetPos - CurrPos).normalized * DashDistance;
            castedBy.transform.position = TargetPos;
        }

        m_TakeDamageParams.causedBy = castedBy.gameObject;
        Collider[] colliders = Physics.OverlapSphere(TargetPos, 3.0f, mask);
        foreach (var collider in colliders)
        {
            collider.GetComponent<LivingEntity>().TakeDamage(m_TakeDamageParams);
        }
    }

    public void Dash(LivingEntity castedBy)
    {
        RaycastHit hit;
        Ray ray = castedBy.GetComponent<PlayerMovement>().m_MainCamera.ScreenPointToRay(castedBy.GetComponent<PlayerInputManager>().MousePos);
        Physics.Raycast(ray, out hit);

        CurrPos = castedBy.transform.position;
        Vector3 dir = (hit.point - CurrPos).normalized;
        float dist = (hit.point - CurrPos).magnitude * 2.0f;

        //Logger.Log("before blink : " + hit.point);

        //               ground      wall
        int blinkMask = (1 << 6) | (1 << 10);
        Physics.Raycast(CurrPos, dir, out hit, dist, blinkMask);

        if (hit.collider != null && hit.collider.gameObject.layer == 10)
        {
            TargetPos = new Vector3(hit.point.x, hit.point.y + CurrPos.y, hit.point.z) * 0.99f;
        }
        else
        {
            TargetPos = new Vector3(hit.point.x, hit.point.y + CurrPos.y, hit.point.z);
        }
    }

    public void FinishDash()
    {
        throw new System.NotImplementedException();
    }
}
