using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EmptySkill", menuName = "Skills/EmptySkill")]
public class EmptySkill : Skill
{
    //private static EmptySkill mInstance;
    //public static EmptySkill Instance()
    //{
    //    if (mInstance != null)
    //    {
    //        return mInstance;
    //    }
    //    else
    //    {
    //        return mInstance = new EmptySkill();
    //    }
    //}
    //private EmptySkill() : base(0.0f, ushort.MaxValue, "Empty", "",
    //    Resources.Load<Sprite>("Skill/Image/SKL_Empty"), true, EType.Buff, 0, 0, false)
    //{ }
    //public override void Cast(LivingEntity castedBy)
    //{
    //    //throw new NotImplementedException();
    //}

    //public override void Casting()
    //{
    //    //throw new NotImplementedException();
    //}

    public override void Init()
    {
        Name = "name_Empty";
        Description = "desc_Empty";
    }
}
