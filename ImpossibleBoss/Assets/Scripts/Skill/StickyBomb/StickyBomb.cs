using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StickyBomb", menuName = "Skills/StickyBomb")]
public class StickyBomb : Skill, ICast, IBuff, IBox
{
    private Collider[] mColliders;

    [SerializeField, Tooltip("Æø")]
    private float m_Width;
    [SerializeField, Tooltip("¼¼·Î")]
    private float m_Height;
    [SerializeField]
    private float m_Damage;
    [SerializeField]
    private float m_BombDamage;

    STakeDamageParams mDamageParams;
    STakeDamageParams mBombDamageParams;

    public byte MaxStack { get; set; }

    public float StackScale{ get; set; }

    public float BuffDurationTime{ get; set; }

    public bool CanDuplicate{ get; set; }

    public StatBuffInfo StatBuffInfo { get; set; }

    public float Width { get { return m_Width; } }
    public float Height { get { return m_Height; } }

    

    public void Cast(LivingEntity castedBy)
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

        if (Physics.BoxCast(currPos, new Vector3(half, half, half), (targetPos - currPos).normalized, out hit, Quaternion.LookRotation((targetPos - currPos).normalized), Height, Mask))
        {
            STakeDamageParams damageParams = mDamageParams;
            damageParams.causedBy = castedBy.gameObject;
            damageParams.damageAmount = castedBy.GetDamageAmount(damageParams.damageAmount);
            hit.collider.GetComponent<LivingEntity>().TakeDamage(damageParams);
            RegisterBuff(castedBy, hit.transform.GetComponent<LivingEntity>());
        }

        //PlayEffect(EAnimState.Cast, castedBy.transform.position, castedBy.transform.rotation);
    }

    public Sprite GetIcon()
    {
        return Icon;
    }

    public override void Init()
    {
        Name = "name_StickyBomb";
        Description = "desc_StickyBomb";

        mColliders = new Collider[10];

        mDamageParams.damageAmount = m_Damage;
        mDamageParams.tag = Tag;
        mBombDamageParams.damageAmount = m_BombDamage;
        mBombDamageParams.tag = new List<string>(Tag.Count);
        for (int count = 0; count < Tag.Count; count++) 
        {
            mBombDamageParams.tag.Add(Tag[count]);
        }
        mBombDamageParams.tag.Add("DOT");

        BuffDurationTime = float.MaxValue;
    }
    public void ApplyBuff(BuffInfo buff)
    {
        buff.Condition.TakeDamageDelegate = OnTakeDamage;
        buff.TriggerBuffDelegate = TrrigerBomb;
        //buff.SetStatBuffInfo(StatBuffInfo);
    }

    private bool OnTakeDamage(object sender, TakeDamageEventArgs e)
    {
        int maxCount = e.TakeDamageParams.tag.Count;
        for (int count = 0; count < maxCount; count++)
        {
            if (e.TakeDamageParams.tag.Contains("DOT"))
            {
                return false;
            }
        }        
        return true;
    }

    private void TrrigerBomb(LivingEntity castedBy, LivingEntity target, BuffInfo info)
    {
        info.ForceRemove();
    }

    public void RegisterBuff(LivingEntity castedBy, LivingEntity target)
    {
        BuffManager.Instance.RegisterBuff(castedBy, target, this);
    }

    public void RemoveBuff(BuffInfo buff)
    {
        STakeDamageParams damageParams = mBombDamageParams;
        damageParams.causedBy = buff.CastedBy.gameObject;
        damageParams.damageAmount = buff.CastedBy.GetDamageAmount(mBombDamageParams.damageAmount);
        buff.Target.TakeDamage(damageParams);
        //Play effect
        PlayEffect(EAnimState.Cast, buff.Target.transform);
    }
}
