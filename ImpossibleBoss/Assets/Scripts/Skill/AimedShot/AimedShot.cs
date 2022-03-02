using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AimedShot", menuName = "Skills/AimedShot")]
public class AimedShot : Skill, ICast, ICasting, IBox
{
    [SerializeField]
    private float m_Damage;
    [SerializeField, Tooltip("¼¼·Î")]
    private float m_Height;
    [SerializeField, Tooltip("Æø")]
    private float m_Width;
    [SerializeField]
    private bool m_b_CanBeInterruptedByDash;
    //[SerializeField]
    private float mCastingTime;

    STakeDamageParams mDamageParams;

    private Collider[] mColliders;

    public float CastingTime { get => mCastingTime; set => mCastingTime = value; }

    public float Height => m_Height;
    public float Width => m_Width;

    public bool CanBeInterruptedByDash { get => m_b_CanBeInterruptedByDash; set => m_b_CanBeInterruptedByDash = value; }

    private float Damage { get { return m_Damage; } }
    
    private Vector2 Dir { get; set; }

    public /*override*/ void Cast(LivingEntity castedBy)
    {
        for (int index = 0; index < 10; index++)
        {
            mColliders[index] = null;
        }

        RaycastHit hit;
        Ray ray = castedBy.GetComponent<PlayerMovement>().m_MainCamera.ScreenPointToRay(Dir);
        Physics.Raycast(ray, out hit);

        Vector3 currPos = castedBy.transform.position;
        Vector3 targetPos = new Vector3(hit.point.x, castedBy.transform.position.y, hit.point.z);

        float half = Width / 2.0f;
        int mask = (1 << 8) | (1 << 9);

        if (Physics.BoxCast(currPos, new Vector3(half, half, half), (targetPos - currPos).normalized, out hit, Quaternion.LookRotation((targetPos - currPos).normalized), Height, mask))
        {
            STakeDamageParams damageParams = mDamageParams;
            damageParams.causedBy = castedBy.gameObject;
            damageParams.damageAmount = castedBy.GetDamageAmount(damageParams.damageAmount);
            hit.collider.GetComponent<LivingEntity>().TakeDamage(damageParams);
        }

        PlayEffect(EAnimState.Cast, castedBy.transform.position, castedBy.transform.rotation);
    }

    public /*override*/ void Casting(LivingEntity castedBy)
    {
        Dir = castedBy.GetComponent<PlayerInputManager>().MousePos;
    }

    public override void Init()
    {
        Name = "name_AimedShot";
        Description = "desc_AimedShot";

        mDamageParams.damageAmount = m_Damage;
        mDamageParams.tag = Tag;

        var info = GetAnimInfo(EAnimState.Casting);
        mCastingTime = info.AnimTime;

        mColliders = new Collider[10];
    }
}
