using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EDifficulty
{
    Easy,
    Normal,
    Hard,
}

public enum EBossType
{
    Vanistoll,
}

//일단 난이도 넘기고
//스킬 적게들고 가는것에대한 업적처리?
public class PlayerData : MonoBehaviour
{
    private EBossType mBossType;
    private EDifficulty mDifficulty;    
    private string mUserName;

    private const string USER_NAME_KEY = "UserName";

    public EBossType BossType { get { return mBossType; } set { mBossType = value; } }
    public int BossTypeInt { get { return (int)mBossType; } set { mBossType = (EBossType)value; } }
    public EDifficulty Difficulty { get { return mDifficulty; } set { mDifficulty = value; } }
    public string UserName { get { return mUserName; } set { mUserName = value; PlayerPrefs.SetString(USER_NAME_KEY, value); } }
    //public float ClearTime;
    //public float ClearDPS;

    private void Readied()
    {
        Logger.Log("PD Ready");
        FindObjectOfType<Test_NextScene>().PD = true;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Difficulty = EDifficulty.Easy;
        mUserName = PlayerPrefs.GetString(USER_NAME_KEY, "Noname");
        Readied();
    }
}