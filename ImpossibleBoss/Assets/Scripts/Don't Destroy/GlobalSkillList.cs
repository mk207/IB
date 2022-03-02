using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//�ı����� �ʴ� ������Ʈ
//��ų�Ŵ����� ���� �����ϴ� ���ϴ� 1�������� GlobalSkillList���� �޾ƾ���.
public class GlobalSkillList : MonoBehaviour
{
    //              ��� ��ų
    //              skill ID
    private Dictionary<ushort, Skill> mSkillList;
    //private Dictionary<ushort, Skill> mEnemySkillList;
    //              ������ ��ų
    //���� �������� ���� �� ����Ʈ�� ��� ��ų���� ����Ұ���
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

        //�׽�Ʈ��
        //mSelectedSkill[0] = mSkillList[ushort.MaxValue];

        // �ӽ� �ʱ�ȭ. ���� save load�� ���� ��ų �� �ҷ��ð�.
        for (int i = 0; i < 6; i++)
        {
            Logger.Log(string.Format("GSL init timing : {0}", i));
            mSelectedSkill.Add(mSkillList[ushort.MaxValue]);
        }

        enabled = false;
    }
}
