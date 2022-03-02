using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ECastMotion
{
    Magic,
    Rolling,
    Dash
}
public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}

//[System.Serializable]
//public class CastMotion
//{
//    [SerializeField]
//    private AnimationClip m_Cast;
//    private static AnimationClip m_Die;
//    private static AnimationClip m_Stun;
//    private static AnimationClip m_Move;
//    private static AnimationClip m_Idle;

//    public AnimationClip Cast { get { return m_Cast; } }
//    public AnimationClip Die { get { return m_Die; } }
//    public AnimationClip Stun { get { return m_Stun; } }
//    public AnimationClip Move { get { return m_Move; } }
//    public AnimationClip Idle { get { return m_Idle; } }

//    public void Init()
//    {
//        //static 들만 초기화
//        //m_Die;
//        //m_Stun;
//        //m_Move;
//        //m_Idle;
//    }
//}

public class SwapCastMotion : MonoBehaviour
{
    //[SerializeField]
    //private List<CastMotion> m_CastMotions;

    //private List<CastMotion> CastMotions { get { return m_CastMotions; } }

    [SerializeField]
    private List<AnimationClip> m_CastMotions;
    [SerializeField]
    private List<AnimationClip> m_ChannelingStartMotions;
    [SerializeField]
    private List<AnimationClip> m_ChannelingMotions;
    [SerializeField]
    private List<AnimationClip> m_ChannelingEndMotions;
    [SerializeField]
    private Dictionary<int, AnimationClip> m_test;

    private List<AnimationClip> CastMotions { get { return m_CastMotions; } }
    private List<AnimationClip> ChannelingStartMotions { get { return m_ChannelingStartMotions; } }
    private List<AnimationClip> ChannelingMotions { get { return m_ChannelingMotions; } }
    private List<AnimationClip> ChannelingEndMotions { get { return m_ChannelingEndMotions; } }
    protected Animator animator;
    protected AnimatorOverrideController animatorOverrideController;

    private int mCastIndex;
    private int mChannelingIndex;
    public int CastIndex { get { return mCastIndex; } set { mCastIndex = value; } }
    public int ChannelingIndex { get { return mChannelingIndex; } set { mChannelingIndex = value; } }

    protected AnimationClipOverrides clipOverrides;
    public void Start()
    {
        animator = GetComponent<Animator>();
        CastIndex = 0;

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        //m_CastMotions = new List<CastMotion>();

        //CastMotions[0].Init();
        //clipOverrides["Die"]  = CastMotions[0].Die;
        //clipOverrides["Stun"] = CastMotions[0].Stun;
        //clipOverrides["Move"] = CastMotions[0].Move;
        //clipOverrides["Idle"] = CastMotions[0].Idle;
    }

    public void SwapController(AnimatorOverrideController aoc)
    {
        animator.runtimeAnimatorController = aoc;
    }

    public void SwapCast()
    {
        clipOverrides["Cast"] = CastMotions[CastIndex];

        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    public void SwapChanneling()
    {
        clipOverrides["ChannelingStart"] = ChannelingStartMotions[ChannelingIndex];
        clipOverrides["Channeling"] = ChannelingMotions[ChannelingIndex];
        clipOverrides["ChannelingEnd"] = ChannelingEndMotions[ChannelingIndex];
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }
}
