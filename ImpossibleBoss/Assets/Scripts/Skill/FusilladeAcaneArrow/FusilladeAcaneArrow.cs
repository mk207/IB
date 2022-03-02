using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FuslladeAcaneArrow", menuName = "Skills/FuslladeAcaneArrow")]
public class FusilladeAcaneArrow : Skill, IChanneling, IRange
{
    private Collider[] mCollider = new Collider[10];
    private GameObject mArcaneArrowPrefab;
    [SerializeField]
    private float m_SkillRange;
    [SerializeField]
    private float m_ChannelingTime;
    [SerializeField]
    private bool m_b_CanBeInterruptedByDash;
    //[SerializeField, Tooltip("채널링중 무빙가능?")]
    //private bool m_b_CanMove;
    [SerializeField]
    STakeDamageParams m_DamageInfo;
    public float ChannelingTime { get { return m_ChannelingTime; } set { m_ChannelingTime = value; } }
    //public bool CanMove { get { return m_b_CanMove; } set { m_b_CanMove = value; } }
    public float ElapsedChannelingTime { get; set; }
    private GameObject ArcaneArrowPrefab { get { return mArcaneArrowPrefab; } set { mArcaneArrowPrefab = value; } }

    public float SkillRange => m_SkillRange;

    public bool CanBeInterruptedByDash { get => m_b_CanBeInterruptedByDash; set => m_b_CanBeInterruptedByDash = value; }

    public void Channeling(LivingEntity castedBy)
    {
        ElapsedChannelingTime += Time.deltaTime;

        if (ElapsedChannelingTime >= 0.5f)
        {
            //           Boss      Enemy
            int mask = (1 << 8) | (1 << 9);
            float distance = float.MaxValue;
            LivingEntity target = null;
            float skillRange = SkillRange;

            ElapsedChannelingTime -= 0.5f;

            STakeDamageParams damageParams = m_DamageInfo;
            damageParams.causedBy = castedBy.gameObject;
            damageParams.DamageAmplificationRate = castedBy.GetDamageAmplification();
            damageParams.damageAmount = castedBy.GetDamageAmount(damageParams.damageAmount);
            

            int numFound = Physics.OverlapSphereNonAlloc(castedBy.transform.position, skillRange, mCollider, mask);
            for (int index = 0; index < numFound; index++)
            {
                if (mCollider[index].CompareTag("Boss"))
                {
                    //target = mCollider[index].gameObject.GetComponent<LivingEntity>();
                    target = mCollider[index].GetComponent<LivingEntity>();
                    break;
                }
                else
                {
                    if ((mCollider[index].transform.position - castedBy.transform.position).sqrMagnitude < distance)
                    {
                        target = mCollider[index].GetComponent<LivingEntity>();
                        distance = (mCollider[index].transform.position - castedBy.transform.position).sqrMagnitude;
                    }
                }
            }

            if (target != null)
            {
                ArcaneArrowProjectile projectile = UnityEngine.Object.Instantiate(ArcaneArrowPrefab).GetComponent<ArcaneArrowProjectile>();
                projectile.DeparturePos = castedBy.transform.position;
                projectile.DestinationPos = target.transform;
                projectile.DamageParams = damageParams;
            }

        }
    }

    public void ChannelingStart(LivingEntity castedBy)
    {
        castedBy.GetComponent<PlayerSkillManager>().SetCustomIndicator(true, new Color(0.7961f, 0.0f, 1.0f), SkillRange);
        //castedBy.GetComponent<PlayerSkillManager>().IsChanneling = true;
        ElapsedChannelingTime = 0.0f;
    }

    public void ChannelingEnd(LivingEntity castedBy)
    {
        castedBy.GetComponent<PlayerSkillManager>().SetCustomIndicator(false);
    }

    public Sprite GetIcon()
    {
        return Icon;
    }

    public override void Init()
    {
        Name = "name_FusilladeAcaneArrow";
        Description = "desc_FusilladeAcaneArrow";

        m_DamageInfo = new STakeDamageParams();
        m_DamageInfo.damageAmount = 40;
        m_DamageInfo.tag = Tag;

        ArcaneArrowPrefab = UnityEngine.Resources.Load<GameObject>("Skill/SkillEffect/ArcaneArrow/ArcaneArrow");
    }    
}
