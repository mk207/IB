using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DefaultExecutionOrder(-1)]
public partial class LivingEntity : MonoBehaviour
{
    //               SkillID           CastedBy
    private Dictionary<uint, Dictionary<string, StatBuffInfo>> mAppliedBuff;

    private void RemoveAllBuff()
    {
        foreach (var buffs in mAppliedBuff)
        {
            foreach(var buff in buffs.Value)
            {
                buff.Value.BuffInfo.ForceRemove();
            }
        }
    }

    //refactor
    private void InitSkillList(StatBuffInfo newBuffInfo)
    {
        mAppliedBuff[newBuffInfo.Skill.ID] = new Dictionary<string, StatBuffInfo>(16);
        mAppliedBuff[newBuffInfo.Skill.ID].Add(newBuffInfo.BuffInfo.CastedBy.Name, newBuffInfo);
    }

    public bool DuplicateCheck(StatBuffInfo newInfo)
    {
        if (mAppliedBuff == null) mAppliedBuff = new Dictionary<uint, Dictionary<string, StatBuffInfo>>(32);


        if (mAppliedBuff.TryGetValue(newInfo.Skill.ID, out var skillList))
        {
            if ((newInfo.Skill as IBuff).CanDuplicate)
            {
                if (skillList.TryGetValue(newInfo.BuffInfo.CastedBy.Name, out var oldText))
                {
                    // 중복 가능한데 같은 시전자로 등록된게 있다
                    oldText.BuffInfo.Renew();
                    newInfo.BuffInfo.IsPendingKill = true;
                    return true;
                }
                else
                {
                    // 중복 가능한데 같은 시전자로 등록된게 없다                
                    //GetColoredText(out coloredAmount, newInfo.Amount * (newInfo.Skill as IBuff).StackScale * newInfo.BuffInfo.Stack);
                    skillList.Add(newInfo.BuffInfo.CastedBy.Name, newInfo);
                    return false;
                }
            }
            else
            {
                if (skillList.TryGetValue(newInfo.BuffInfo.CastedBy.Name, out var oldText))
                {
                    // 중복 불가능한데 이미 시전자 이름으로 등록된게 있다
                    oldText.BuffInfo.Renew();
                    newInfo.BuffInfo.IsPendingKill = true;
                    return true;
                }
                else
                {
                    //중복 불가능한데 시전자 이름으로 등록된게 없다.
                    //그러면 다른 시전자 이름으로 등록된게 있는가?
                    if (skillList.Count == 0)
                    {
                        //없으니 등록                    
                        //GetColoredText(out coloredAmount, newInfo.Amount * (newInfo.Skill as IBuff).StackScale * newInfo.BuffInfo.Stack);
                        skillList.Add(newInfo.BuffInfo.CastedBy.Name, newInfo);
                        return false;
                    }
                    else
                    {
                        //있으니 기존거 지우고 새거 등록
                        var e = skillList.GetEnumerator();
                        e.MoveNext();
                        var ele = e.Current;
                        ele.Value.BuffInfo.ForceRemove();
                        skillList.Remove(ele.Key);

                        //GetColoredText(out coloredAmount, newInfo.Amount * (newInfo.Skill as IBuff).StackScale * newInfo.BuffInfo.Stack);
                        skillList.Add(newInfo.BuffInfo.CastedBy.Name, newInfo);
                        return false;
                    }
                }
            }
        }
        else
        {
            // 최초등록
            InitSkillList(newInfo);
            return false;
        }
    }

    public bool StackCheck(StatBuffInfo newInfo)
    {
        StatBuffInfo oldBuffInfo = mAppliedBuff[newInfo.Skill.ID][newInfo.BuffInfo.CastedBy.Name];
        if (oldBuffInfo.BuffInfo.Stack < (oldBuffInfo.Skill as IBuff).MaxStack)
        {
            oldBuffInfo.BuffInfo.IncreaseStack();
            return true;
        }
        return false;
    }

    private void RemoveBuffIcon(StatBuffInfo info)
    {
        mAppliedBuff[info.Skill.ID].Remove(info.BuffInfo.CastedBy.Name);
    }

    //100단위로 반환함
    //1단위로 저장 해야함    
    private void AddStatAmount(EAmountType eAmountType, EBuffFactorType eFactorType, float amount)
    {
        if (eAmountType == EAmountType.Percent)
        {
            switch (eFactorType)
            {
                case EBuffFactorType.Damage:                DamagePercent += amount;                                                            break;
                case EBuffFactorType.Armor:                 ArmorPercent += amount;                                                             break;
                case EBuffFactorType.MoveSpeed:             MoveSpeedPercent += amount;                                                         break;
                case EBuffFactorType.CoolDown:              CoolDownPercent += amount;                                                          break;
                case EBuffFactorType.AttackSpeed:           AttackSpeedPercent += amount;                                                       break;
                case EBuffFactorType.Regen:                 HealthRegenPercent += amount;                                                       break;
                case EBuffFactorType.RegenTime:             HealthRegenTimePercent += amount;                                                   break;
                case EBuffFactorType.DamageAmplification:   DamageAmplificationPercent += amount;                                               break;
                case EBuffFactorType.DamageResistance:      DamageResistance += amount;                                                         break;
                case EBuffFactorType.Health:                SetHealth(Health + InitHealth * amount * 0.01f);                                    break;
                case EBuffFactorType.MaxHealth:             SetHealth(Health + InitHealth * amount * 0.01f); InitHealth *= (amount * 0.01f);    break;
                default: throw new ArgumentOutOfRangeException(nameof(eFactorType));
            }
        }
        else
        {
            switch (eFactorType)
            {
                case EBuffFactorType.Damage:                DamagePlus += amount;                                                       break;
                case EBuffFactorType.Armor:                 ArmorPlus += amount;                                                        break;
                case EBuffFactorType.MoveSpeed:             MoveSpeedPlus += amount;                                                    break;
                case EBuffFactorType.CoolDown:              CoolDownPlus += amount;                                                     break;
                case EBuffFactorType.AttackSpeed:           AttackSpeedPlus += amount;                                                  break;
                case EBuffFactorType.Regen:                 HealthRegenPlus += amount;                                                  break;
                case EBuffFactorType.RegenTime:             HealthRegenTimePlus += amount;                                              break;
                case EBuffFactorType.Invincible:            if (amount == 1) { IsInvincible = true; } else { IsInvincible = false; }    break;
                case EBuffFactorType.DamageAmplification:   DamageAmplificationPercent += amount;                                       break;
                case EBuffFactorType.DamageResistance:      DamageResistance += amount;                                                 break;
                case EBuffFactorType.Health:                SetHealth(Health + amount);                                                 break;
                case EBuffFactorType.MaxHealth:             SetHealth(Health + amount); InitHealth += amount;                           break;
                default: throw new ArgumentOutOfRangeException(nameof(eFactorType));
            }
        }
    }

    //refactor
    public void ApplyStatChanged(StatBuffInfo statInfo)
    {
        bool shouldAdd = true;
        if (statInfo.IsRegister)
        {
            bool shouldStackCheck = DuplicateCheck(statInfo);

            if (shouldStackCheck)
            {
                shouldAdd = StackCheck(statInfo);
            }           
        }

        if (statInfo.IsRegister)
        {
            if (shouldAdd)
            {
                foreach (var info in statInfo.BuffFactorInfo)
                {
                    AddStatAmount(info.AmountType, info.FactorType, info.Amount * statInfo.BuffInfo.StackScale * statInfo.BuffInfo.Stack);
                }
                foreach (var tag in statInfo.BanTagList)
                {
                    ApplySkillBan(tag, true);
                }                
            }
        }
        else
        {
            foreach (var info in statInfo.BuffFactorInfo)
            {
                AddStatAmount(info.AmountType, info.FactorType, info.Amount * statInfo.BuffInfo.StackScale * statInfo.BuffInfo.Stack * -1.0f);
            }
            foreach (var tag in statInfo.BanTagList)
            {
                ApplySkillBan(tag, false);
            }
            RemoveBuffIcon(statInfo);
            
        }

        //foreach (var info in statInfo.BuffFactorInfo)
        //{
        //    if (statInfo.IsRegister)
        //    {
        //        if (shouldAdd)
        //        {
        //            AddStatAmount(info.AmountType, info.FactorType, info.Amount * statInfo.BuffInfo.StackScale * statInfo.BuffInfo.Stack);
        //        }
        //    }
        //    else
        //    {
        //        RemoveBuffIcon(statInfo);
        //        AddStatAmount(info.AmountType, info.FactorType, info.Amount * statInfo.BuffInfo.StackScale * statInfo.BuffInfo.Stack * -1.0f);
        //    }
        //}


        //Logger.Log(string.Format("Damage : {0} Armor : {1}", DamagePlus, ArmorPercent));
    }
    public void ApplySkillBan(string tag, bool isRegister)
    {
        GetComponent<PlayerSkillManager>().SetSkillBan(tag, isRegister);
    }

    protected virtual void SetInvincible(bool isRegister)
    {
        IsInvincible = isRegister;
    }

    protected virtual void Awake()
    {
        mAppliedBuff = new Dictionary<uint, Dictionary<string, StatBuffInfo>>(32);
        OnDeath = Die;
        FindObjectOfType<GameManager>().GameOverEvent += OnGameOver;
        FindObjectOfType<GameManager>().GamePauseEvent += OnGamePause;
        FindObjectOfType<GameManager>().GameStartEvent += OnGameStart;
        IsDead = false;

        HealthRegen = InitHealthRegen;
        HealthRegenTime = InitHealthRegenTime;
    }

    protected virtual void OnGameStart(object sender, GameStartEventArgs e)
    {
        Logger.Log("Patents OnGameStart");
    }

    protected virtual void OnGamePause(object sender, GamePauseEventArgs e)
    {
        IsPaused = e.IsPaused;
    }

    protected virtual void OnGameOver(object sender, GameOverEventArgs e)
    {
        Logger.Log("Parents OnGameOver");
    }

    protected virtual void OnEnable()
    {
        TakeDamageEvent += FindObjectOfType<GameManager>().RecordDamage;
    }

    protected void OnDisable()
    {
        //Logger.Log("OnDisable" + name);
        //TakeDamageEvent -= FindObjectOfType<GameManager>().RecordDamage;
    }
}
