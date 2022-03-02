using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//파괴되지 않는 오브젝트
//스킬매니저를 만들어서 관리하던 뭐하던 1차적으로 GlobalSkillList에서 받아쓰기.
public class GlobalSkillList : MonoBehaviour
{
    //              모든 스킬
    //              skill ID
    private Dictionary<ushort, Skill> mSkillList;
    //private Dictionary<ushort, Skill> mEnemySkillList;
    //              선택한 스킬
    //보스 스테이지 들어가면 이 리스트에 담긴 스킬들을 사용할것임
    private List<Skill> mSelectedSkill;

    public Dictionary<ushort, Skill> SkillList { get { return mSkillList; } }
    //public Dictionary<ushort, Skill> EnemySkillList { get { return mEnemySkillList; } }
    public Skill this[ushort index] { get { return mSkillList[index]; } }
    public List<Skill> SelectedSkills { get{ return mSelectedSkill; } }
    public void SetSelectedSkill(int index, ushort skillID)
    {
        if (index < 0 && index >= 6) UnityEngine.Assertions.Assert.IsTrue(false, "SetSelectedSkill : Invalid index");
        mSelectedSkill[index] = mSkillList[skillID];
    }

    public void UnlockSkill(ushort skillID)
    {
        mSkillList[skillID].CanUse = true;
    }

    //public void RegisterEnemySkill(Skill skill)
    //{
    //    skill.Init();
    //    skill.InitIcon();
    //    SkillList.Add(skill.ID, skill);
    //    EnemySkillList.Add(skill.ID, skill);
    //    Logger.Log("add Enemy skill : " + skill.Name + " " + skill.ID);
    //}

    //public void RemoveEnemySkill()
    //{
    //    //foreach (var skill in EnemySkillList)
    //    //{
    //    //    SkillList.Remove();
    //    //}
    //}

    private void Readied()
    {
        Logger.Log("GSL Ready");
        FindObjectOfType<Test_NextScene>().GSL = true;
    }

    private void Awake()
    {
        mSkillList = new Dictionary<ushort, Skill>(128);
        mSelectedSkill = new List<Skill>(6);
        
        DontDestroyOnLoad(gameObject);
        Readied();
    }

    public void RegisterSkill(Skill skill)
    {
        skill.Init();
        skill.InitIcon();
        mSkillList.Add(skill.ID, skill);
        Logger.Log("add skill : " + skill.Name + " " + skill.ID);
    }

    private void Start()
    {
        foreach (var skill in mSkillList)
        {
            Logger.Log(string.Format("GSL Start : {0} // {1}", skill.Key, skill.Value.Name));
        }

        //테스트용
        //mSelectedSkill[0] = mSkillList[ushort.MaxValue];

        // 임시 초기화. 이후 save load로 기존 스킬 셋 불러올것.
        for (int i = 0; i < 6; i++)
        {
            Logger.Log(string.Format("GSL init timing : {0}", i));
            mSelectedSkill.Add(mSkillList[ushort.MaxValue]);
        }

        enabled = false;
    }
}
