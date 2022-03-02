using System;

public class SkillFiredEventArgs : EventArgs
{
    public Skill FiredSkill;
    public SkillFiredEventArgs(Skill skill)
    {
        FiredSkill = skill;
    }
}
