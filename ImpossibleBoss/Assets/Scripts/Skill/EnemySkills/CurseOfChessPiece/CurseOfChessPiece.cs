using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurseOfChessPiece", menuName = "Skills/Enemy/CurseOfChessPiece")]
public class CurseOfChessPiece : Skill, ICast, IBuff
{
    public byte MaxStack => 200;

    public float StackScale => 1.0f;

    public float BuffDurationTime => float.MaxValue;

    public bool CanDuplicate => true;

    public StatBuffInfo StatBuffInfo { get; set; }

    private static ChessBoss Boss { get; set; }

    public void ApplyBuff(BuffInfo buff)
    {
        StatBuffInfo.BuffInfo = buff;
        StatBuffInfo.IsRegister = true;
        StatBuffInfo.BuffInfo.Rewrite.Invoke();
        StatBuffInfo clone = StatBuffInfo.Clone();
        buff.SetStatBuffInfo(clone);
        buff.Target.ApplyStatChanged(clone);
    }

    public void Cast(LivingEntity castedBy)
    {
        if (Boss == null)
        {
            Boss = FindObjectOfType<ChessBoss>();
        }
        RegisterBuff(castedBy, Boss);
    }

    public Sprite GetIcon()
    {
        return Icon;
    }

    public override void Init()
    {
        Name = "name_CurseOfChessPiece";
        Description = "desc_CurseOfChessPiece";

        StatBuffInfo = new StatBuffInfo(this);        
        StatBuffInfo.AddBuffFactorInfo(new SBuffFactorInfo(-0.75f, EAmountType.Percent, EBuffFactorType.DamageResistance));
    }

    public void RegisterBuff(LivingEntity castedBy, LivingEntity target)
    {
        BuffManager.Instance.RegisterBuff(castedBy, target, this);
    }

    public void RemoveBuff(BuffInfo buff)
    {
        
    }
}
