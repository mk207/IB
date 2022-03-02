using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//public enum EDifficulty
//{
//    Easy,
//    Nomal,
//    Hard
//}

public class PlayerDamageInfo
{
    private float mDPS;
    private float mTotalDamage;
    private float mTime;

    public float DPS { get { return mDPS; } private set { mDPS = value; } }
    public float TotalDamage { get { return mTotalDamage; } set { mTotalDamage = value; } }
    public float PlayTime { get { return mTime; } set { mTime = value; } }

    public void StartStopwatch()
    {
        PlayTime = Time.time;
    }

    public void EndStopwatch()
    {
        PlayTime = Time.time - PlayTime;
        CalcDPS();
    }

    public void CalcDPS()
    {
        DPS = TotalDamage / PlayTime;
    }
}

public class GameManager : MonoSingleton<GameManager>
{
    public BossInformation boss;
    //private Dictionary<byte, PlayerDamageInfo> mPlayerDamageInfo;
    PlayerDamageInfo mPlayerDamageInfo;
    //public Dictionary<string, float> TotalDamage;
    private Dictionary<byte, bool> mPlayerAlive;
    private float mTotalDamage;
    public GameObject m_ReadyStart;
    public List<Sprite> m_ReadyImages;
    private int mDifficulty;
    [SerializeField]
    private IngameMenu m_Menu;
    [SerializeField]
    private Animator m_Animator;
    [SerializeField]
    private float m_WatingTimeToStart;
    private float mStartTime;
    [SerializeField]
    private Text m_DefeatTime;
    [SerializeField]
    private Text m_DefeatDPS;
    [SerializeField]
    private Text m_VictoryTime;
    [SerializeField]
    private Text m_VictoryDPS;
    private GlobalSound mAudio;
    private UnityEngine.UI.Image mReadyStartImage;
    private byte mLocalPlayerID;
    private bool mbIsGameover;
    private bool mbIsReady;
    private bool mbIsPaused;
    private bool mbIsInMenu;
    private bool mbIsCountDown;
    private bool mbIsPlayingCountAnnounce;

    public event EventHandler<GamePauseEventArgs> GamePauseEvent;
    public event EventHandler<GameStartEventArgs> GameStartEvent;
    public event EventHandler<GameOverEventArgs> GameOverEvent;
    public event EventHandler<VictoryEventArgs> VictoryEvent;
    public event EventHandler<DefeatEventArgs> DefeatEvent;
    
    public float TotalDamage { get { return mTotalDamage; } set { mTotalDamage = value; } }
    private IngameMenu GameMenu { get { return m_Menu; } }
    private Text DefeatTime { get { return m_DefeatTime; } set { m_DefeatTime = value; } }
    private Text DefeatDPS { get { return m_DefeatDPS; } set { m_DefeatDPS = value; } }
    private Text VictoryTime { get { return m_VictoryTime; } set { m_VictoryTime = value; } }
    private Text VictoryDPS { get { return m_VictoryDPS; } set { m_VictoryDPS = value; } }
    private Animator Anim { get { return m_Animator; } }
    private GlobalSound Audio { get { return mAudio; } }
    private Dictionary<byte, bool> PlayerAlive { get { return mPlayerAlive; } }
    //private Dictionary<byte, PlayerDamageInfo> PlayerDamageInfo { get { return mPlayerDamageInfo; } set { mPlayerDamageInfo = value; } }
    private PlayerDamageInfo LocalPlayerDamageInfo { get { return mPlayerDamageInfo; } set { mPlayerDamageInfo = value; } }
    private float WatingTimeToStart { get { return m_WatingTimeToStart; } }
    private float StartTime { get { return mStartTime; } set { mStartTime = value; } }
    public byte LocalPlayerID { get { return mLocalPlayerID; } set { mLocalPlayerID = value; } }
    public bool IsPaused { get { return mbIsPaused; } set { mbIsPaused = value; } }
    public bool IsReady { get { return mbIsReady; } set { mbIsReady = value; } }
    public int Difficulty { get { return mDifficulty; } set { mDifficulty = value; } }
    public bool IsGameover { get { return mbIsGameover; } private set { mbIsGameover = value; } } // 게임 오버 상태
    public bool IsInMenu { get { return mbIsInMenu; } set { mbIsInMenu = value; } }
    private bool IsCountDown { get { return mbIsCountDown; } set { mbIsCountDown = value; } }
    private bool IsPlayingCountAnnounce { get { return mbIsPlayingCountAnnounce; } set { mbIsPlayingCountAnnounce = value; } }
    private bool IsAlive(byte ID)
    {
        return mPlayerAlive[ID];
    }

    public void RegisterPlayer(byte ID)
    {
        PlayerAlive.Add(ID, true);
    }

    protected override void Awake()
    {
        base.Awake();
        mReadyStartImage = m_ReadyStart.GetComponent<UnityEngine.UI.Image>();
        FindObjectOfType<BossInformation>().BossDieEvent += Victory;
        //mAnimator = GetComponentInChildren<Animator>();
        mPlayerAlive = new Dictionary<byte, bool>(4);
        //멀티 도입하면 바꿔야함
        LocalPlayerID = 0;
        mAudio = FindObjectOfType<GlobalSound>();
        mPlayerDamageInfo = new PlayerDamageInfo();
        IsCountDown = true;
        IsPlayingCountAnnounce = false;
        //Logger.Log("Game Manager Awake");
    }

    private void Start()
    {
        Logger.Log("GameManager Start");

        PlayerInformation[] playerInformation = FindObjectsOfType<PlayerInformation>();
        ReadyToStart();
        IsInMenu = false;
    }

    private void Update()
    {
        int second = Mathf.FloorToInt(StartTime -= (Time.unscaledDeltaTime * (IsInMenu ? 0.0f : 1.0f) )) + 1;
        if (IsReady && second < 1)
        {
            LocalPlayerDamageInfo.StartStopwatch();
            IsReady = false;
            m_ReadyStart.GetComponent<UnityEngine.UI.Image>().sprite = m_ReadyImages[0];            
            OnGameStart(new GameStartEventArgs());
            IsCountDown = false;
            Resume();
        }
        else
        {            
            //Logger.Log(string.Format("second {0}  Start {1}  Real{2}", second, StartTime, Time.realTimeSinceStartup));
            if (second < 4 && second > 0)
            {
                m_ReadyStart.GetComponent<UnityEngine.UI.Image>().sprite = m_ReadyImages[second];
                if (second == 3 && IsPlayingCountAnnounce == false)
                {
                    Audio.StartGame();
                    IsPlayingCountAnnounce = true;
                }
            }            
        }

        if(IsReady == false)
        {
            Color color = mReadyStartImage.color;
            if (color.a <= 0.0f)
            {
                enabled = false;
                m_ReadyStart.SetActive(false);
            }
            else
            {
                color.a -= Time.deltaTime;
                mReadyStartImage.color = color;
            }            
        }        
    }

    public void PlayerDie(byte ID)
    {
        PlayerAlive[ID] = false;
        if (ID == LocalPlayerID)
        {
            LocalPlayerDamageInfo.EndStopwatch();            
        }

        foreach (var alive in PlayerAlive)
        {
            //한명이라도 살아있으면 함수종료
            if (alive.Value)
            {
                return;
            }
        }
        GameOver();
    }

    private void Victory(object sender, BossDieEventArgs e)
    {
        IsGameover = true;
        //FindObjectOfType<IngameMenu>().ShowVictory();
        LocalPlayerDamageInfo.EndStopwatch();
        VictoryDPS.text = LocalPlayerDamageInfo.DPS.ToString();
        VictoryTime.text = TimeSpan.FromSeconds(LocalPlayerDamageInfo.PlayTime).ToString(@"mm\:ss\.fff");
        GameMenu.ShowVictory();
        Anim.SetTrigger("Victory");
        UnlockSkill(e.UnlockID);
        OnGameOver(new GameOverEventArgs(LocalPlayerDamageInfo.PlayTime));
        Audio.Victory();


        PlayerData pd = FindObjectOfType<PlayerData>();
        InsertSoloRankRequest request = new InsertSoloRankRequest();
        request.userName = pd.UserName;
        request.bossType = (int)pd.BossType;
        request.bossDiff = (int)pd.Difficulty;
        request.clearTime = LocalPlayerDamageInfo.PlayTime;
        request.dealPerSec = LocalPlayerDamageInfo.DPS;

        DBConnect.InsertSoloRank(request);
        //Pause();
        //VictoryEventArgs v = new VictoryEventArgs(Time.realTimeSinceStartup - StartTime);        
        //OnVictory(v);
    }

    private void GameOver()
    {
        IsGameover = true;
        //Invoke("ShowDefaet", 2.0f);
        //ShowDefaet();
        DefeatDPS.text = LocalPlayerDamageInfo.DPS.ToString();
        DefeatTime.text = TimeSpan.FromSeconds(LocalPlayerDamageInfo.PlayTime).ToString(@"mm\:ss\.fff");
        GameMenu.ShowDefeat();
        Anim.SetTrigger("Defeat");
        OnGameOver(new GameOverEventArgs(LocalPlayerDamageInfo.PlayTime));
        Audio.Defeat();
        //Pause();
        //DefeatEventArgs e = new DefeatEventArgs();
        //OnDefeat(e);
        //UIManager.instance.SetActiveGameoverUI(true);
    }

    public void Menu()
    {
        Pause();
    }

    private void UnlockSkill(ushort skillID)
    {
        //PlayerSkillManager
    }

    public void RecordDamage(object sender, TakeDamageEventArgs e)
    {
        PlayerInformation causedBy = e.TakeDamageParams.causedBy.GetComponent<PlayerInformation>();

        //피해 유발자가 로컬 플레이어면 기록하기
        if (causedBy != null && causedBy.PlayerID == LocalPlayerID && IsGameover == false)
        {
            LocalPlayerDamageInfo.TotalDamage += e.TakeDamageParams.damageAmount;
        }

        //멀티 추가하면 위 코드 사용
        //LocalPlayerDamageInfo.TotalDamage += e.TakeDamageParams.damageAmount;
    }

    public void ReadyToStart()
    {
        IsReady = true;
        m_ReadyStart.gameObject.SetActive(true);
        m_ReadyStart.GetComponent<UnityEngine.UI.Image>().sprite = m_ReadyImages[4];
        Pause();
        //StartTime = Time.realtimeSinceStartup + WatingTimeToStart;
        StartTime = WatingTimeToStart;
    }

    public void Resume()
    {
        if (IsCountDown == false)
        {
            IsPaused = false;
            EventHandler<GamePauseEventArgs> temp = GamePauseEvent;
            if (temp != null)
            {
                temp(this, new GamePauseEventArgs(IsPaused));
            }
            Time.timeScale = 1.0f;
        }

    }

    public void Pause()
    {
        IsPaused = true;
        EventHandler<GamePauseEventArgs> temp = GamePauseEvent;
        if (temp != null)
        {
            temp(this, new GamePauseEventArgs(IsPaused));
        }
        Time.timeScale = 0.0f;
    }

    private void OnGameStart(GameStartEventArgs e)
    {
        EventHandler<GameStartEventArgs> temp = GameStartEvent;

        if (temp != null)
        {
            temp(this, e);
        }
    }

    private void OnGameOver(GameOverEventArgs e)
    {
        Logger.Log("OnGameOver");
        EventHandler<GameOverEventArgs> temp = GameOverEvent;

        if (temp != null)
        {
            temp(this, e);
        }
    }

    private void OnVictory(VictoryEventArgs e)
    {
        EventHandler<VictoryEventArgs> temp = VictoryEvent;

        if (temp != null)
        {
            temp(this, e);
        }
    }

    private void OnDefeat(DefeatEventArgs e)
    {
        EventHandler<DefeatEventArgs> temp = DefeatEvent;

        if (temp != null)
        {
            temp(this, e);
        }
    }
}
