
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitSkill : MonoBehaviour
{
    [SerializeField]
    private List<Skill> Skills = new List<Skill>(30);
    private void Readied()
	{
		FindObjectOfType<Test_NextScene>().IS = true;
	}

    void Awake()
    {
        GlobalSkillList list = FindObjectOfType<GlobalSkillList>();
        foreach (var skill in Skills)
        {
            list.RegisterSkill(skill);
        }
        
		//list.RegisterSkill(EmptySkill.Instance());
		//list.RegisterSkill(Madness.Instance());
  //      list.RegisterSkill(FixedArtillery.Instance());
  //      list.RegisterSkill(SnipeStance.Instance());
  //      list.RegisterSkill(ChargeStance.Instance());
  //      list.RegisterSkill(CloseCall.Instance());
  //      list.RegisterSkill(Blink.Instance());
  //      list.RegisterSkill(Lightning.Instance());
  //      list.RegisterSkill(FusilladeAcaneArrow.Instance());
        //list.RegisterSkill(test2.Instance());
        //list.RegisterSkill(test3.Instance());
        //list.RegisterSkill(test4.Instance());
        //list.RegisterSkill(test5.Instance());
        //list.RegisterSkill(test6.Instance());
        //list.RegisterSkill(test7.Instance());
        //list.RegisterSkill(test8.Instance());
        //list.RegisterSkill(test9.Instance());

        Readied();
    }
}