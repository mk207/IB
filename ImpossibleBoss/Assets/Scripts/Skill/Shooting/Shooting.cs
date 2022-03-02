using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

[CreateAssetMenu(fileName = "Shooting", menuName = "Skills/Shooting")]
public class Shooting : Skill, ICast, IBox
{
    [SerializeField]
    private float m_Damage;
    [SerializeField, Tooltip("¼¼·Î")]
    private float m_Height;
    [SerializeField, Tooltip("Æø")]
    private float m_Width;

    STakeDamageParams mDamageParams;

    private Collider[] mColliders;

    public float Height => m_Height;

    private float Damage { get { return m_Damage; } }
    public float Width => m_Width;


    public /*override*/ void Cast(LivingEntity castedBy)
    {
        for (int index = 0; index < 10; index++)
        {
            mColliders[index] = null;
        }

        RaycastHit hit;
        Ray ray = castedBy.GetComponent<PlayerMovement>().m_MainCamera.ScreenPointToRay(castedBy.GetComponent<PlayerInputManager>().MousePos);
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
           
    //public override void Casting()
    //{
        
    //}

    public override void Init()
    {
        Name = "name_Shooting";
        Description = "desc_Shooting";

        mDamageParams.damageAmount = m_Damage;
        mDamageParams.tag = Tag;

        mColliders = new Collider[10];
    }
}
