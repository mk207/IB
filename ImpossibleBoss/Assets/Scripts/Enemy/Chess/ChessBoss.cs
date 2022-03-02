using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class QuizDeadZone
{    
    private int[] mCoord;
    private int mOrder;
    private bool mbIsReadied;
    private bool mbIsDeadZone;
    private bool mbIsCurrDeadZone;
    private bool mbIsDeadZoneByPiece;

    SuperChessGuide Guide { get; set; }
    // row col
    public int[] Coord { get { return mCoord; } /*set { mCoord = value; }*/ }
    public int Order { get { return mOrder; } set { mOrder = value; } }
    public bool IsReadied { get { return mbIsReadied; } set { mbIsReadied = value; } }
    public bool IsDeadZone { get { return mbIsDeadZone; } set { mbIsDeadZone = value; } }
    public bool IsCurrDeadZone { get { return mbIsCurrDeadZone; } set { mbIsCurrDeadZone = value; } }
    public bool IsDeadZoneByPiece { get { return mbIsDeadZoneByPiece; } }
    public void SetDeadZoneByPiece(bool value)
    {
        mbIsDeadZoneByPiece = value;
        if (value) 
        { 
            Guide.TurnOn(); 
        } 
        else 
        { 
            Guide.TurnOff(); 
        }
    }
    public QuizDeadZone(int row, int col, int order, bool isDeadZone)
    {
        mCoord = new int[2] { row, col };
        mOrder = order;
        mbIsReadied = false;
        mbIsDeadZone = isDeadZone;
        mbIsDeadZoneByPiece = false;
        mbIsCurrDeadZone = false;
    }
    public QuizDeadZone(SuperChessGuide guide, float boxSize, int row, int col, int order, bool isDeadZone)
    {
        mCoord = new int[2] { row, col };
        mOrder = order;
        mbIsReadied = false;
        mbIsDeadZone = isDeadZone;
        mbIsDeadZoneByPiece = false;
        mbIsCurrDeadZone = false;
        Guide = guide;
        Guide.Row = row * boxSize;
        Guide.Col = col * boxSize;
        Guide.gameObject.SetActive(false);
    }
}

public enum EQuizColor : byte
{
    Red,
    Blue
}

internal enum EState
{
    ExcutingPattern,
    Waiting
}

internal enum EBanSkill
{
    SummonCardTornado,    
    BlackAndWhite,
    HeartBlast,
    SummonFlowerCard,
    SummonCrossBomb,
    SummonChess
}

public class ChessBoss : BossInformation
{
    [VisibleOnly]
    public float OutlineWidth = 0.0f;
    [VisibleOnly]
    public float OutlineAlpha;
    public ChessAI m_AI;
    static bool test = false;
    [SerializeField]
    private List<float> m_PatternInterval;
    private EState mState;
    private float mPatternWaitTime;
    [Header("Debug")]
    public bool m_bDebugMode;
    [SerializeField, Header("스킬 밴")]
    private List<EBanSkill> m_EasyBannedSkills;
    [SerializeField]
    private List<EBanSkill> m_NormalBannedSkills;
    [SerializeField]
    private List<EBanSkill> m_HardBannedSkills;
    private List<List<EBanSkill>> m_BanSkills;
    private List<EBanSkill> m_TempBanSkills;
    [SerializeField, Header("깜빡임")]
    private Material m_Material;
    [SerializeField, Tooltip("피격때 깜빡임")]
    private AnimationCurve m_BlinkCurve;
    [SerializeField]
    private float m_DurationBlinkTime;
    private float mElapsedBlinkTime;
    private bool mbShouldBlink;
    private bool mbIsGameOver;

    private Animator mHeartAnim;
    private int HIT_ID = Animator.StringToHash("Hit");
    private Color mHeartColor = new Color(0.2627451f, 0.0f, 0.0f);

    [SerializeField]
    private List<byte> m_PatternCount;

    #region Guide
    [SerializeField, Header("Guide")]
    private List<bool> m_ChessGuide;
    [SerializeField]
    private List<bool> m_BAWGuide;
    [SerializeField]
    private List<bool> m_TornadoGuide;
    [SerializeField]
    private List<bool> m_CrossBombGuide;
    [SerializeField]
    private List<bool> m_QuizGuide;
    [SerializeField]
    private List<bool> m_QuizChessGuide;
    #endregion

    #region Chess Pieces
    [SerializeField, Header("체스말")]
    private float m_PiecesDamage;
    [SerializeField]
    private float m_PiecesKnockBackPower;
    [SerializeField]
    private float m_PieceHealth;
    [SerializeField]
    private float m_SuperPieceHealth;
    [SerializeField]
    private float m_SummonChessInterval;
    private float mSummonChessTime;
    [SerializeField, Tooltip("소환갯수")]
    private List<byte> m_SummonChessCount;
    [SerializeField, Tooltip("바닥 밟았을때 소환갯수")]
    private byte m_PenaltySummonChessCount;
    #endregion
    #region HeartBlast
    [SerializeField, Tooltip("폭발에 걸리는 시간"), Header("심장폭발")]
    private float m_HeartBlastInterval = 0.5f;
    [SerializeField]
    private float m_HeartBlastRange = 2.0f;
    [SerializeField]
    private float m_HeartBlastDamage = 50.0f;
    [SerializeField]
    private Image m_HeartBlastInnerIndicator;
    [SerializeField]
    private Image m_HeartBlastOutline;
    [SerializeField]
    private Image m_HeartBlastSafeZone;
    [SerializeField]
    private float m_SafeZoneAngle;
    private float mSafeZoneDir;
    private float mSafeZoneCos;
    [SerializeField, Tooltip("심폭 횟수")]
    private int m_HeartBlastMaxCount;
    private int mHeartBlastCount;
    [SerializeField]
    private float m_HeartBlastKnockBackSecond;
    STakeDamageParams mHeartBlastDamageParams;
    private float mInnerIndicatorSize;
    [SerializeField, Tooltip("심폭 충전 시간(심장 빛나고 HeartBlastStartDelayTime초 후에 공격시작)")]
    private float m_HeartBlastStartDelayTime;
    private float mHeartBlastStartTime;
    private bool mbIsHeartBlastStarted;
    private bool mbIsHeartBlastCharging;
    #endregion
    #region Card Tornado
    [SerializeField, Header("카드 회오리")]
    private GameObject m_Tornado;
    [SerializeField, Tooltip("토네이도 지속 시간")]
    private float m_TornadoLifeTime = 3.0f;
    [SerializeField, Tooltip("카드 소환 간격")]
    private float m_CardSummonInterval = 0.5f;
    [SerializeField, Tooltip("1 = 1초만에 Range만큼의 거리를 이동함, 0.1 = 10초 동안 Rnage만큼 감")]
    private float m_CardSpeed = 1.0f;
    [SerializeField, Tooltip("카드 사거리")]
    private float m_CardRange = 10.0f;
    [SerializeField]
    private float m_CardDamage;
    [SerializeField]
    private float m_CardKnockBackSecond;

    private ObjectPool<Tornado> mTornadoPool;
    #endregion
    #region BAW
    [SerializeField, Header("깜댕이 흰둥이")]
    private float m_BAWDamage;
    [SerializeField, Tooltip("검,흰 보여주고 몇초 뒤에 데미지 줄것인가")]
    private float m_BAWDamageTime;
    [SerializeField]
    private Image m_BAWIndicator;
    [SerializeField]
    private GameObject m_BAWGuidePrefab;
    private ObjectPool<BAWGuide> mBAWGuidePool;
    private float mBAWDamageTiming;
    private STakeDamageParams mBAWDamageParams;
    private bool mbIsCastingBAW;
    private bool mbIsBlack;
    #endregion
    #region FlowerCard
    [SerializeField, Header("화투")]
    private GameObject m_FlowerCardPrefab;
    [SerializeField]
    private GameObject m_FlowerLinePrefab;
    //[SerializeField, Tooltip("1 = 1쌍")]
    private const int mSummonPairCount = 5;
    [SerializeField, Tooltip("화투를 몇초동안 보여줄 것인가")]
    private float m_CardShowingTime;
    [SerializeField, Tooltip("빨래줄 피해량")]
    private float m_FlowerCardDamage;
    [SerializeField, Tooltip("빨래줄 유지 시간")]
    private float m_LineMaintenanceTime;
    [SerializeField, Tooltip("빨래줄 피해 간격")]
    private float m_FlowerDamageInterval;
    private ObjectPool<FlowerCard> mFlowerCards;
    private Queue<FlowerLine> mActivatedLines;
    private Queue<FlowerLine> mDeactivatedLines;
    private Queue<float> mActivatedLinesTime;
    [SerializeField, Tooltip("빨래줄 생성과 동시에 이펙트 발동시 느려서 Timing만큼 미리 이펙트만 보여줌")]
    private float m_LineEffectTiming;
    private bool mbIsCastingFlowerCard;
    #endregion
    #region CrossBomb
    [SerializeField, Tooltip("폭발딜"), Header("십자폭탄")]
    private float m_CrossBombDamage;
    [SerializeField, Tooltip("바닥딜")]
    private float m_BurningFloorDamage;
    [SerializeField, Tooltip("폭탄 터지는 시간")]
    private float m_CrossBombTimer;
    [SerializeField, Tooltip("바닥틱 간격")]
    private float m_BurningFloorDamageInterval;
    [SerializeField]
    private float m_BurningFloorDurationTime;
    [SerializeField]
    private GameObject m_BombEffect;
    [SerializeField]
    private GameObject m_BombPrefab;
    [SerializeField]
    private GameObject m_BurningFloorPrefab;
    private ObjectPool<CrossBomb> mCrossBombPool;
    private ObjectPool<BurningFloor> mBurningFloorPool;
    private GameObject mBurningFloors;
    private Queue<(List<BurningFloor>, float)> mActivaedFloors;
    private const int BOMB_COUNT = 4;
    private const int FLOOR_COUNT = 16;
    private GameObject mBombs;
    private GameObject mBombEffects;
    #endregion
    #region Quiz
    [SerializeField, Header("퀴즈")]
    private Text m_QuizQuestionText;
    [SerializeField]
    private Transform m_BoardTransform;
    [SerializeField, Tooltip("퀴즈 횟수")]
    private uint m_QuizMaxCount;
    //기본 최대
    private uint QUIZ_MAX_COUNT = 5;
    private int mQuizCount;
    [SerializeField, Tooltip("퀴즈 맞추는 시간")]
    private float m_QuizAnswerTime;
    private float mEndQuizTime;
    [SerializeField, Tooltip("각 문제 간격 시간")]
    private float m_QuizBreakTime;
    private float mNextQuizShowTime;
    [SerializeField, Tooltip(@"퀴즈 패턴 ""시작""할때 몇초의 여유 줄건지")]
    private float m_QuizAlarmDurationTime;
    private float mQuizAlarmEndTime;
    [SerializeField, Tooltip("% 단위")]
    private List<float> m_FirstHeartAdditionalDamage;
    [SerializeField, Tooltip("% 단위")]
    private List<float> m_SecondHeartAdditionalDamage;
    [SerializeField]
    private GameObject m_DeadZonePrefab;
    //DeadZone 오브젝트
    private ObjectPool<ChessDeadZone> mDeadZonePool;
    private List<(QuizDeadZone deadZone, int row, int col)> mNewDeadZone;
    [SerializeField]
    private AudioClip m_AlarmClip;
    [SerializeField]
    private GameObject m_QuizCam;
    [SerializeField]
    private List<int> m_SummonCountSuperPiece;
    [SerializeField, Tooltip("1 = 100%")]
    private float m_InvincibleHealth;
    [SerializeField]
    private GameObject m_ExterminateIndicator;
    private const float EXTERMINATE_INDICATOR_SIZE = 80.0f;
    private bool mbIsExterminating;
    [SerializeField]
    private Image m_QuizBackground;
    private int mQuizAnswer;
    private string mQuizQuestionFormat = @"{0} - {1} = ?";
    //DeadZone정보
    private List<QuizDeadZone> mDeadZoneInfo;
    //private List<int[]> mCurrDeadZone;
    private AsyncOperationHandle mQuizPatternHandle;
    //private Dictionary<int, List<QuizDeadZone>> mDeadZone;
    STakeDamageParams mQuizDamageParams;
    [SerializeField]
    private GameObject m_QuizExterminateEffect;
    //private static STakeDamageParams mSuperPieceDamageParams;
    private EQuizColor mQuizColor;
    [SerializeField]
    private GameObject m_QuizGuidePrefab;
    private ObjectPool<QuizGuide> mQuizGuidePool;
    [SerializeField]
    private GameObject m_SuperChessGuidePrefab;
    private bool mbIsWaitingStartQuiz;
    private bool mbIsQuizShowing;
    private bool mbIsQuizBreakTime;
    private bool mbIsQuizTime = false;
    private bool mbIsTriggeredFirstQuiz = false;
    private bool mbIsTriggeredSecondQuiz = false;
    [SerializeField]
    private bool m_b_IsIgnoreInvincible = false;
    private bool mbIsBreakTimeInvincible = false;
    #endregion

    //[SerializeField]
    //private AssetLabelReference m_AssetLabel;
    [SerializeField]
    private AssetReference m_ChessAssets;

    //private List<AsyncOperationHandle> mAddressableHandles;

    private float mElapsedPatternTime = 0.0f;


    //private Material mMat;

    //반지름을 계산해야해서 10을 곱하는게 아니라 5를 곱해야함
    private const float INDICATOR_SIZE = 5.0f;

    private Material HeartMat { get { return m_Material; } }
    private AnimationCurve BlinkCurve { get { return m_BlinkCurve; } }
    private float DurationBlinkTime { get { return m_DurationBlinkTime; } }
    private float ElapsedBlinkTime { get { return mElapsedBlinkTime; } set { mElapsedBlinkTime = value; } }
    private Color HeartColor { get { return mHeartColor; } set { mHeartColor = value; } }
    private bool ShouldBlink { get { return mbShouldBlink; } set { mbShouldBlink = value; } }
    private Animator HeartAnim { get { return mHeartAnim; } }
    private bool IsGameOver { get { return mbIsGameOver; } set { mbIsGameOver = value; } }
    private List<byte> PatternCount { get { return m_PatternCount; } }
    #region Chess Pieces Property
    public float PiecesDamage { get { return m_PiecesDamage; } }
    public float PiecesKnockBackPower { get { return m_PiecesKnockBackPower; } }
    private float PieceHealth { get { return m_PieceHealth; } }
    private float SuperPieceHealth { get { return m_SuperPieceHealth; } }
    private float SummonChessInterval { get { return m_SummonChessInterval; } }
    private float SummonChessTime { get { return mSummonChessTime; } set { mSummonChessTime = value; } }
    private List<byte> SummonChessCount { get { return m_SummonChessCount; } }
    private byte PenaltySummonChessCount { get { return m_PenaltySummonChessCount; } }
    #endregion
    #region Quiz Property
    private GameObject QuizGuidePrefab { get{ return m_QuizGuidePrefab; } }
    private ObjectPool<QuizGuide> QuizGuidePool { get { return mQuizGuidePool; } }
    private GameObject SuperChessGuidePrefab { get { return m_SuperChessGuidePrefab; } }
    private List<int> SummonCountSuperPiece { get { return m_SummonCountSuperPiece; } }
    private STakeDamageParams QuizDamageParams { get { return mQuizDamageParams; } }
    //private STakeDamageParams SuperPieceDamageParams { get { return mSuperPieceDamageParams; } }
    private Transform BoardTransform { get { return m_BoardTransform; } }
    private Text QuizQuestionText { get { return m_QuizQuestionText; } }
    private GameObject DeadZonePrefab { get { return m_DeadZonePrefab; } }
    private ObjectPool<ChessDeadZone> DeadZonePool { get { return mDeadZonePool; } }
    private List<(QuizDeadZone deadZone, int row, int col)> NewDeadZone { get { return mNewDeadZone; } }
    //public List<int[]> CurrDeadZone { get { return mCurrDeadZone; } }
    public List<QuizDeadZone> DeadZoneInfo { get { return mDeadZoneInfo; } set { mDeadZoneInfo = value; } }
    private AudioClip AlarmClip { get { return m_AlarmClip; } }
    private GameObject QuizCam { get { return m_QuizCam; } }
    private List<float> FirstHeartAdditionalDamage { get { return m_FirstHeartAdditionalDamage; } }
    private List<float> SecondHeartAdditionalDamage { get { return m_SecondHeartAdditionalDamage; } }
    private string QuizQuestionFormat { get { return mQuizQuestionFormat; } }
    private Image QuizBackground { get { return m_QuizBackground; } }
    public int QuizAnswer { get { return mQuizAnswer; } set { mQuizAnswer = value; } }
    private float QuizAnswerTime { get { return m_QuizAnswerTime; } set { m_QuizAnswerTime = value; } }
    private float EndQuizTime { get { return mEndQuizTime; } set { mEndQuizTime = value; } }
    private float QuizTimeFactor { get; set; }
    private float QuizBreakTime { get { return m_QuizBreakTime; } }
    private float QuizAlarmDurationTime { get { return m_QuizAlarmDurationTime; } }
    private float QuizAlarmEndTime { get { return mQuizAlarmEndTime; } set { mQuizAlarmEndTime = value; } }
    private float NextQuizShowTime { get { return mNextQuizShowTime; } set { mNextQuizShowTime = value; } }
    private uint QuizMaxCount { get { return m_QuizMaxCount; } set { m_QuizMaxCount = value; } }
    private int QuizCount { get { return mQuizCount; } set { mQuizCount = value; } }
    private EQuizColor QuizColor { get { return mQuizColor; } set { mQuizColor = value; } }
    private GameObject QuizExterminateEffect { get { return m_QuizExterminateEffect; } }
    private float InvincibleHealth { get { return m_InvincibleHealth; } }
    private GameObject ExterminateIndicator { get { return m_ExterminateIndicator; } }
    private BoxCollider Collider { get; set; }
    private MeshRenderer MeshRenderer { get; set; }    
    private bool IsExterminating { get { return mbIsExterminating; } set { mbIsExterminating = value; } }
    private bool IsQuizTime { get { return mbIsQuizTime; } set { mbIsQuizTime = value; } }
    private bool IsWaitingStartQuiz { get { return mbIsWaitingStartQuiz; } set { mbIsWaitingStartQuiz = value; } }
    private bool IsQuizShowing { get { return mbIsQuizShowing; } set { mbIsQuizShowing = value; } }
    private bool IsQuizBreakTime { get { return mbIsQuizBreakTime; } set { mbIsQuizBreakTime = value; } }
    private bool IsTriggeredFirstQuiz { get { return mbIsTriggeredFirstQuiz; } set { mbIsTriggeredFirstQuiz = value; } }
    private bool IsTriggeredSecondQuiz { get { return mbIsTriggeredSecondQuiz; } set { mbIsTriggeredSecondQuiz = value; } }
    private bool IsIgnoreInvincible { get { return m_b_IsIgnoreInvincible; } }
    private bool IsBreakTimeInvincible { get { return mbIsBreakTimeInvincible; } set { mbIsBreakTimeInvincible = value; } }
    private bool ShouldShowQuizGuid { get; set; } = default;
    #endregion
    #region Tornado Property
    private ObjectPool<Tornado> TornadoPool { get { return mTornadoPool; } }
    private GameObject TornadoPrefab { get { return m_Tornado; } }
    private float TornadoLifeTime { get { return m_TornadoLifeTime; } }
    private float CardSummonInterval { get { return m_CardSummonInterval; } }
    private float CardRange { get { return m_CardRange; } set { m_CardRange = value; } }
    private float CardDamage { get { return m_CardDamage; } set { m_CardDamage = value; } }
    private float CardSpeed { get { return m_CardSpeed; } set { m_CardSpeed = value; } }
    private float CardKnockBackSecond { get { return m_CardKnockBackSecond; } set { m_CardKnockBackSecond = value; } }
    #endregion
    #region Heart Blast Property
    private Image HeartBlastInnerIndicator { get { return m_HeartBlastInnerIndicator; } }
    private Image HeartBlastOutline { get { return m_HeartBlastOutline; } }
    private Image HeartBlastSafeZone { get { return m_HeartBlastSafeZone; } }
    private float SafeZoneAngle { get { return m_SafeZoneAngle; } }
    private float SafeZoneDir { get { return mSafeZoneDir; } set { mSafeZoneDir = value; } }
    private float SafeZoneCos { get { return mSafeZoneCos; } }
    private STakeDamageParams HeartBlastDamageParams { get { return mHeartBlastDamageParams; } }
    private float InnerIndicatorSize { get { return mInnerIndicatorSize; } set { mInnerIndicatorSize = value; } }
    private float HeartBlastStartDelayTime { get { return m_HeartBlastStartDelayTime; } set { m_HeartBlastStartDelayTime = value; } }
    private float HeartBlastStartTime { get { return mHeartBlastStartTime; } set { mHeartBlastStartTime = value; } }
    private float HeartBlastDamage { get { return m_HeartBlastDamage; } }
    private float HeartBlastInterval { get { return m_HeartBlastInterval; } }
    private float HeartBlastRange { get { return m_HeartBlastRange; } set { m_HeartBlastRange = value; } }
    private float HeartBlastKnockBackSecond { get { return m_HeartBlastKnockBackSecond; } }
    private int HeartBlastMaxCount { get { return m_HeartBlastMaxCount; } }
    private int HeartBlastCount { get { return mHeartBlastCount; } set { mHeartBlastCount = value; } }
    private bool IsHeartBlastCharging { get { return mbIsHeartBlastCharging; } set { mbIsHeartBlastCharging = value; } }
    private bool IsHeartBlastStarted { get { return mbIsHeartBlastStarted; } set { mbIsHeartBlastStarted = value; } }

    #endregion
    #region BAW Property
    private Image BAWIndicator { get { return m_BAWIndicator; } }
    private ObjectPool<BAWGuide> BAWGuidePool { get { return mBAWGuidePool; } }
    private float BAWDamage { get { return m_BAWDamage; } }
    private float BAWDamageTime { get { return m_BAWDamageTime; } }
    private float BAWDamageTiming { get { return mBAWDamageTiming; } set { mBAWDamageTiming = value; } }
    private float BAWDamageTimeFactor { get; set; }
    private STakeDamageParams BAWDamageParams { get { return mBAWDamageParams; } }
    private bool IsBlack { get { return mbIsBlack; } set { mbIsBlack = value; } }
    private bool IsCastingBAW { get { return mbIsCastingBAW; } set { mbIsCastingBAW = value; } }
    #endregion
    #region FlowerCard Property    
    private Queue<FlowerLine> ActivatedFlowerLines { get { return mActivatedLines; } }
    private Queue<FlowerLine> DeactivatedFlowerLines { get { return mDeactivatedLines; } }
    private Queue<float> ActivatedLinesTime { get { return mActivatedLinesTime; } }
    private ObjectPool<FlowerCard> FlowerCards { get { return mFlowerCards; } }
    private GameObject FlowerLinePrefab { get { return m_FlowerLinePrefab; } }
    private GameObject FlowerCardPrefab { get { return m_FlowerCardPrefab; } }
    private int SummonPairCount { get { return mSummonPairCount; } }
    private float CardShowingTime { get { return m_CardShowingTime; } }
    public float FlowerCardDamage { get { return m_FlowerCardDamage; } }
    private float LineMaintenanceTime { get { return m_LineMaintenanceTime; } }
    public float FlowerDamageInterval { get { return m_FlowerDamageInterval; } }
    private Queue<float> FlowerShowEndTime { get; set; }
    private int FlowerPivot { get; set; }
    private float LineEffectTiming { get { return m_LineEffectTiming; } }
    private bool IsCastingFlowerCard { get { return mbIsCastingFlowerCard; } set { mbIsCastingFlowerCard = value; } }
    #endregion
    #region CrossBomb Property
    private float CrossBombDamage { get { return m_CrossBombDamage; } }
    private float BurningFloorDamage { get { return m_BurningFloorDamage; } }
    private float CrossBombTimer { get { return m_CrossBombTimer; } }
    private float BurningFloorDamageInterval { get { return m_BurningFloorDamageInterval; } }
    private float BurningFloorDurationTime { get { return m_BurningFloorDurationTime; } }
    private ObjectPool<CrossBomb> CrossBombPool { get { return mCrossBombPool; } }
    private GameObject BombEffect { get { return m_BombEffect; } }
    private GameObject BombPrefab { get { return m_BombPrefab; } }
    private GameObject BurningFloorPrefab { get { return m_BurningFloorPrefab; } }
    private ObjectPool<BurningFloor> BurningFloorPool { get { return mBurningFloorPool; } }
    private Queue<(List<BurningFloor>, float)> ActivaedFloors { get { return mActivaedFloors; } set { ActivaedFloors = value; } }
    #endregion
    private List<List<EBanSkill>> BanSkills { get { return m_BanSkills; } }
    private List<EBanSkill> TempBanSkills { get { return m_TempBanSkills; } }
    private EState State { get { return mState; } set { mState = value; } }
    //private Material Mat { get { return mMat; } }
    public float ElapsedPatternTime { get { return mElapsedPatternTime; } set { mElapsedPatternTime = value; } }
    private List<float> PatternInterval { get { return m_PatternInterval; } set { m_PatternInterval = value; } }
    private float PatternWaitTime { get { return mPatternWaitTime; } }

    private float PatternTime { get; set; }
    //private float StartTime;
    //private float EndTime;

    //private AssetLabelReference AssetLabel { get { return m_AssetLabel; } }
    //private 

    //public int BoardSize;
    //private SBoardInfo[,] mBoardInformation;

    //public bool IsPieceExistent(int x, int y) { return mBoardInformation[x, y].IsChessExists; }
    //public void SetIsPieceExistent(int x, int y, bool newState) { mBoardInformation[x, y].IsChessExists = newState; }
    private ChessAI AI { get { return m_AI; } }

    public override void TakeDamage(STakeDamageParams takeDamageParams)
    {
        base.TakeDamage(takeDamageParams);
        ShouldBlink = true;
        HeartAnim.SetTrigger(HIT_ID);

        float healthPercent = (Health / InitHealth);
        if (healthPercent <= 0.7f && IsTriggeredFirstQuiz == false)
        {
            DamageResistance += FirstHeartAdditionalDamage[Difficult];
            if ((EDifficulty)Difficult != EDifficulty.Easy)
            {
                IsExterminating = true;
                ExterminateIndicator.SetActive(true);
            }
            //IsQuizTime = true;
            IsTriggeredFirstQuiz = true;
            QuizAlarm();
            //StartQuiz();
            //ShowQuiz();
        }                

        if (healthPercent <= 0.3 && IsTriggeredSecondQuiz == false)
        {
            //QuizMaxCount = uint.MaxValue;
            QuizMaxCount = QUIZ_MAX_COUNT + 1;
            DamageResistance += SecondHeartAdditionalDamage[Difficult];
            //IsQuizTime = true;
            IsTriggeredSecondQuiz = true;
            QuizAlarm();
            //StartQuiz();
            //ShowQuiz();
        }

        if (IsQuizTime && IsTriggeredSecondQuiz == false && healthPercent <= InvincibleHealth)
        {
            SetInvincible(true);
            SetHealth(InvincibleHealth * InitHealth);
        }
    }

    private void HeartBlink(float time)
    {
        ElapsedBlinkTime += time;
        if (ElapsedBlinkTime <= DurationBlinkTime)
        {
            //Logger.Log($"Evaluate : {BlinkCurve.Evaluate(ElapsedBlinkTime)} // {ElapsedBlinkTime}");
            HeartMat.SetColor("_BaseColor", new Color(HeartColor.r, HeartColor.g, HeartColor.b, BlinkCurve.Evaluate(ElapsedBlinkTime)));
        }
        else
        {
            ShouldBlink = false;
            ElapsedBlinkTime = 0.0f;
            HeartMat.SetColor("_BaseColor", new Color(HeartColor.r, HeartColor.g, HeartColor.b, 1.0f));
        }

    }

    public void QuizPatternLoad()
    {
        string bit = null;
        Addressables.LoadAssetAsync<TextAsset>("QuizPattern").Completed += (obj) =>
        {
            mQuizPatternHandle = obj;
            bit = obj.Result.text;

            for (int index = 0; index < bit.Length / 10; index++)
            {
                if (bit[index * 10] == '0')
                {
                    DeadZoneInfo[index].IsDeadZone = false;
                }
                else
                {
                    DeadZoneInfo[index].IsDeadZone = true;
                }

                DeadZoneInfo[index].Coord[0] = System.Convert.ToInt32(bit.Substring(index * 10 + 1, 3), 2);
                DeadZoneInfo[index].Coord[1] = System.Convert.ToInt32(bit.Substring(index * 10 + 4, 3), 2);
                DeadZoneInfo[index].Order = System.Convert.ToInt32(bit.Substring(index * 10 + 7, 3), 2);
            }
            Addressables.Release(mQuizPatternHandle);
        };

        //string bit = System.IO.File.ReadAllText(@"Assets/BossStageAssets/Chess/QuizDeadZone/QuizPattern.txt");

        //for (int index = 0; index < bit.Length / 10; index++)
        //{
        //    if (bit[index * 10] == '0')
        //    {
        //        DeadZoneInfo[index].IsDeadZone = false;
        //    }
        //    else
        //    {
        //        DeadZoneInfo[index].IsDeadZone = true;
        //    }

        //    DeadZoneInfo[index].Coord[0] = System.Convert.ToInt32(bit.Substring(index * 10 + 1, 3), 2);
        //    DeadZoneInfo[index].Coord[1] = System.Convert.ToInt32(bit.Substring(index * 10 + 4, 3), 2);
        //    DeadZoneInfo[index].Order = System.Convert.ToInt32(bit.Substring(index * 10 + 7, 3), 2);
        //}

        //bool isDeadZone;
        //int row;
        //int col;
        //int order;
        //for (int index = 0; index < bit.Length / 10; index++)
        //{
        //    isDeadZone = System.Convert.ToBoolean(bit[index * 10]);            
        //    row = System.Convert.ToInt32(bit.Substring(index * 10 + 1, 3), 2);
        //    col = System.Convert.ToInt32(bit.Substring(index * 10 + 4, 3), 2);
        //    order = System.Convert.ToInt32(bit.Substring(index * 10 + 7, 3), 2);

        //    mDeadZone.Add(order, new QuizDeadZone(row, col, order, isDeadZone));           
        //}
    }

    protected override void Awake()
    {        
        base.Awake();
        SetInitEnergy(m_PatternInterval[Difficult]);
        //m_ChessAssets.loada<GameObject>().Completed += OnLoadDone;
        //Addressables.LoadResourceLocationsAsync(AssetLabel.labelString).Completed += OnLoadAssets;

        if (DeadZoneInfo == null)
        {
            DeadZoneInfo = new List<QuizDeadZone>(64);
            //mCurrDeadZone = new List<int[]>(64);

            for (int index = 0; index < 64; index++)
            {
                GameObject guide = Instantiate(m_SuperChessGuidePrefab, Vector3.zero, Quaternion.identity, m_BoardTransform);
                int row = Mathf.FloorToInt(index / 8);
                int col = index % 8;
                guide.transform.position = new Vector3(col * AI.GetBOXSIZE, 0.1f, row * AI.GetBOXSIZE);
                DeadZoneInfo.Add(new QuizDeadZone(guide.GetComponent<SuperChessGuide>(), AI.GetBOXSIZE, row, col, 5, false));
            }
            SuperChessGuide.ShouldShowGuide = m_QuizChessGuide[Difficult];
        }
        QuizPatternLoad();

        mDeadZonePool = new ObjectPool<ChessDeadZone>(64);
        for (int index = 0; index < 64; index++)
        {
            GameObject gameObject = Instantiate(DeadZonePrefab, AI.m_BoardTransform);
            DeadZonePool.RegisterObject(gameObject.GetComponent<ChessDeadZone>());
            gameObject.SetActive(false);
        }

        QuizAlarmEndTime = float.MaxValue;

        mHeartAnim = GetComponentInChildren<Animator>();
        //FindObjectOfType<GameManager>().GameOverEvent += OnGameOver;
        IsGameOver = false;
        IsPaused = false;

        m_BanSkills = new List<List<EBanSkill>>(3);
        m_BanSkills.Add(m_EasyBannedSkills);
        m_BanSkills.Add(m_NormalBannedSkills);
        m_BanSkills.Add(m_HardBannedSkills);
        m_TempBanSkills = new List<EBanSkill>();

        m_ExterminateIndicator = GameObject.Find("ExterminateIndicator");
        ExterminateIndicator.SetActive(false);
        IsExterminating = false;
        mNewDeadZone = new List<(QuizDeadZone deadZone, int row, int col)>(64);

        PatternTime = Time.timeSinceLevelLoad + PatternInterval[Difficult];

        Collider = GetComponent<BoxCollider>();
        MeshRenderer = GetComponentInChildren<MeshRenderer>();

        //ShouldShowQuizGuid = m_QuizGuide[Difficult];
    }

    protected override void OnGamePause(object sender, GamePauseEventArgs e)
    {
        base.OnGamePause(sender, e);
        if (e.IsPaused == false)
        {
            StartCoroutine(Pattern());
        }
    }

    protected override void OnGameStart(object sender, GameStartEventArgs e)
    {
        IsPaused = false;
        StartCoroutine(Pattern());
    }

    protected override void OnGameOver(object sender, GameOverEventArgs e)
    {
        IsGameOver = true;
    }

    protected override void Start()
    {
        SummonChessTime = Time.time + SummonChessInterval;

        ElapsedBlinkTime = 0.0f;
        HeartMat.SetColor("_BaseColor", new Color(HeartColor.r, HeartColor.g, HeartColor.b, 1.0f));

        mHeartBlastDamageParams = new STakeDamageParams();
        mHeartBlastDamageParams.causedBy = gameObject;
        mHeartBlastDamageParams.damageAmount = HeartBlastDamage;
        mHeartBlastDamageParams.DamageAmplificationRate = 0.0f;
        //mHeartBlastDamageParams.cc = ECC.KnockBack;
        //mHeartBlastDamageParams.ccAmount = HeartBlastKnockBackSecond;

        mBAWDamageParams = new STakeDamageParams();
        mBAWDamageParams.causedBy = gameObject;
        mBAWDamageParams.damageAmount = BAWDamage;
        mBAWDamageParams.DamageAmplificationRate = 0.0f;

        mQuizDamageParams = new STakeDamageParams();
        mQuizDamageParams.causedBy = gameObject;
        mQuizDamageParams.damageAmount = 100000000.0f;
        mQuizDamageParams.DamageAmplificationRate = 0.0f;

        //mSuperPieceDamageParams.causedBy = gameObject;
        //mSuperPieceDamageParams.damageAmount = float.MaxValue;
        //mSuperPieceDamageParams.DamageAmplificationRate = 0.0f;

        m_HeartBlastOutline.rectTransform.sizeDelta = new Vector2(HeartBlastRange * INDICATOR_SIZE, HeartBlastRange * INDICATOR_SIZE);
        m_HeartBlastInnerIndicator.rectTransform.sizeDelta = Vector2.zero;
        m_HeartBlastSafeZone.rectTransform.sizeDelta = Vector2.zero;
        m_HeartBlastSafeZone.fillAmount = SafeZoneAngle / 360.0f;
        mSafeZoneCos = Mathf.Cos(SafeZoneAngle * 0.5f * Mathf.Deg2Rad);

        //List<Material> m = new List<Material>();
        //GetComponent<MeshRenderer>().GetMaterials(m);
        //mMat = m[0];

        mTornadoPool = new ObjectPool<Tornado>(10);
        for (int index = 0; index < 10; index++)
        {
            GameObject newTornado = Instantiate(TornadoPrefab, Vector3.zero, Quaternion.identity);
            Tornado tornado = newTornado.GetComponent<Tornado>();
            if (m_bDebugMode)
            {
                tornado.CausedBy = newTornado;
            }
            else
            {
                tornado.CausedBy = gameObject;
            }
            tornado.LifeTime = TornadoLifeTime;
            tornado.CardSummonTime = CardSummonInterval;
            tornado.Damage = CardDamage;
            tornado.Range = CardRange;
            tornado.Speed = CardSpeed;
            tornado.KnockBackSecond = CardKnockBackSecond;
            tornado.ReturnObjectDelegate = TornadoPool.ReturnObject;
            newTornado.SetActive(false);
            TornadoPool.RegisterObject(newTornado.GetComponent<Tornado>());
        }
        Tornado.ShouldShowGuide = m_TornadoGuide[Difficult];

        #region Flower
        mFlowerCards = new ObjectPool<FlowerCard>(SummonPairCount * 14);
        for (int index = 0; index < SummonPairCount * 14; index++)
        {
            GameObject newFlower = Instantiate(FlowerCardPrefab);
            newFlower.transform.SetParent(BoardTransform);
            newFlower.transform.localRotation = Quaternion.identity;
            newFlower.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            newFlower.SetActive(false);
            FlowerCard flower = newFlower.GetComponent<FlowerCard>();
            flower.ReturnObjectDelegate = FlowerCards.ReturnObject;
            FlowerCards.RegisterObject(flower);
        }

        mActivatedLines = new Queue<FlowerLine>(SummonPairCount * 7);
        mDeactivatedLines = new Queue<FlowerLine>(SummonPairCount * 7);
        mActivatedLinesTime = new Queue<float>(/*SummonPairCount*/7);
        FlowerShowEndTime = new Queue<float>(/*SummonPairCount*/7);

        for (int index = 0; index < SummonPairCount * 7; index++)
        {
            GameObject newLine = Instantiate(FlowerLinePrefab);
            FlowerLine newFlowerLine = newLine.GetComponent<FlowerLine>();
            newLine.transform.SetParent(BoardTransform);
            if (m_bDebugMode)
            {
                newFlowerLine.CausedBy = newLine;
            }
            else
            {
                newFlowerLine.CausedBy = gameObject;
            }
            newFlowerLine.Damage = FlowerCardDamage;
            newFlowerLine.DamageInterval = FlowerDamageInterval;
            newFlowerLine.EffectTiming = LineEffectTiming;
            //newFlowerLine.testName = string.Format("Line " + index);
            newLine.SetActive(false);
            DeactivatedFlowerLines.Enqueue(newFlowerLine);
        }
        #endregion
        #region CrossBomb
        mBombs = new GameObject("Bombs");
        mBombEffects = new GameObject("BombEffects");
        mCrossBombPool = new ObjectPool<CrossBomb>(4);
        CrossBomb.Boom = BoomCrossBomb;
        for (int index = 0; index < 4; index++)
        {
            GameObject bomb = Instantiate(BombPrefab, mBombs.transform);
            CrossBomb newBomb = bomb.GetComponent<CrossBomb>();
            newBomb.CausedBy = gameObject;
            newBomb.BombTimer = CrossBombTimer;
            newBomb.BombDamage = CrossBombDamage;
            newBomb.Effect = Instantiate(BombEffect, mBombEffects.transform);
            newBomb.ReturnObjectDelegate = CrossBombPool.ReturnObject;
            bomb.SetActive(false);
            CrossBombPool.RegisterObject(newBomb);
        }
        CrossBomb.ShouldGuide = m_CrossBombGuide[Difficult];

        mBurningFloorPool = new ObjectPool<BurningFloor>(FLOOR_COUNT * BOMB_COUNT);
        mBurningFloors = new GameObject("BurningFloors");
        BurningFloor.mSummonChessDelegate = SummonChessPiece;
        BurningFloor.m_Difficulty = (EDifficulty)Difficult;
        for (int count = 0; count < FLOOR_COUNT * BOMB_COUNT * 2.0f; count++)
        {
            GameObject floor = Instantiate(BurningFloorPrefab, mBurningFloors.transform);
            BurningFloor newFloor = floor.GetComponent<BurningFloor>();
            if (m_bDebugMode)
            {
                newFloor.CausedBy = floor;
            }
            else
            {
                newFloor.CausedBy = gameObject;
            }
            newFloor.Damage = BurningFloorDamage;
            newFloor.Interval = BurningFloorDamageInterval;
            newFloor.DurationTime = BurningFloorDurationTime;
            newFloor.SummonChessCount = PenaltySummonChessCount;
            newFloor.ReturnObjectDelegate = BurningFloorPool.ReturnObject;
            floor.SetActive(false);
            mBurningFloorPool.RegisterObject(newFloor);
        }
        #endregion
        #region BAW Guid
        mBAWGuidePool = new ObjectPool<BAWGuide>(32);
        for (int index = 0; index < 32; index++)
        {
            GameObject newBawGuide = Instantiate(m_BAWGuidePrefab, Vector3.zero, Quaternion.identity, AI.m_BoardTransform);
            newBawGuide.transform.localRotation = Quaternion.identity;
            BAWGuide bawGuide = newBawGuide.GetComponent<BAWGuide>();            
            newBawGuide.SetActive(false);
            BAWGuidePool.RegisterObject(bawGuide);
        }
        #endregion
        #region Quiz Guide
        mQuizGuidePool = new ObjectPool<QuizGuide>(64);
        for (int index = 0; index < 64; index++)
        {
            GameObject newQuizGuide = Instantiate(m_QuizGuidePrefab, Vector3.zero, Quaternion.identity, AI.m_BoardTransform);
            newQuizGuide.transform.localRotation = Quaternion.identity;
            QuizGuide quizGuide = newQuizGuide.GetComponent<QuizGuide>();
            newQuizGuide.SetActive(false);
            QuizGuidePool.RegisterObject(quizGuide);
        }


        #endregion
        ChessPiecesInformation.ShouldShowGuid = m_ChessGuide[Difficult];
        AI.SummonCountSuperPiece = SummonCountSuperPiece[Difficult];
        mPatternWaitTime = 0.2f;
        EndQuizTime = float.MinValue;
        //StartCoroutine(Pattern());
    }

    protected override void Update()
    {
        base.Update();

        if (IsGameOver == true || IsPaused == true)
        {
            return;
        }

        if (OutlineWidth > 5.0f)
        {
            OutlineWidth = 1.0f;
            OutlineAlpha = 1.0f;
        }
        else
        {
            OutlineAlpha = Mathf.Clamp(OutlineAlpha - (Time.deltaTime * 3), 0.0f, 1.0f);
            OutlineWidth += (Time.deltaTime * 5);
        }
        //GetComponent<MeshRenderer>().material.SetFloat("_OutlineAlpha", OutlineAlpha);
        //GetComponent<MeshRenderer>().material.SetFloat("_OutlineWidth", OutlineWidth);

        if (IsQuizTime)
        {
            ElapsedPatternTime = 0;
            SetEnergy(ElapsedPatternTime);
        }
        else
        {
            ElapsedPatternTime += Time.deltaTime;
            SetEnergy(ElapsedPatternTime);
        }
        

        #region Blink
        if (ShouldBlink)
        {
            HeartBlink(Time.deltaTime);
        }
        #endregion

        #region HeartBlast
        if (IsHeartBlastCharging)
        {
            if (InnerIndicatorSize > HeartBlastInterval)
            {
                HeartBlast();
                InnerIndicatorSize = 0.0f;
                HeartBlastInnerIndicator.rectTransform.sizeDelta = new Vector2(0.0f, 0.0f);
                HeartBlastSafeZone.rectTransform.sizeDelta = new Vector2(0.0f, 0.0f);
            }
            else
            {
                InnerIndicatorSize += Time.deltaTime;
                HeartBlastInnerIndicator.rectTransform.sizeDelta = new Vector2(HeartBlastRange * INDICATOR_SIZE * (InnerIndicatorSize / HeartBlastInterval),
                                                                                HeartBlastRange * INDICATOR_SIZE * (InnerIndicatorSize / HeartBlastInterval));
                HeartBlastSafeZone.rectTransform.sizeDelta = new Vector2(HeartBlastRange * INDICATOR_SIZE * (InnerIndicatorSize / HeartBlastInterval),
                                                                                HeartBlastRange * INDICATOR_SIZE * (InnerIndicatorSize / HeartBlastInterval));
                //Logger.Log(string.Format("HeartBlast : {0}", InnerIndicatorSize));
                //Logger.Log($"InnerIndicator : {InnerIndicatorSize}");
            }
        }
        #endregion
        #region Quiz
        if (IsWaitingStartQuiz && QuizAlarmEndTime < Time.time)
        {
            IsWaitingStartQuiz = false;
            StartQuiz();
            ShowQuiz();
        }

        if (IsQuizTime)
        {
            //ChessDeadZone.CheckPlayerPosotion(AI.GetPlayersPos(), DeadZoneInfo);
            foreach (var player in AI.GetPlayersPos())
            {
                int row = Mathf.FloorToInt((player.Value.z + 2.0f) * 0.25f);
                int col = Mathf.FloorToInt((player.Value.x + 2.0f) * 0.25f);
                if (DeadZoneInfo[row * 8 + col].IsCurrDeadZone && DeadZoneInfo[row * 8 + col].IsReadied && player.Key.IsDead == false)
                {
                    player.Key.InstantDeath(true);
                }
            }
        }

        if (ShouldShowQuizGuid)
        {
            float rate = (Time.time - QuizTimeFactor) / (EndQuizTime - QuizTimeFactor);
            int count = QuizGuidePool.ActivatedObject.Count;
            var guids = QuizGuidePool.ActivatedObject;
            for (int index = 0; index < count; ++index)
            {
                guids[index].SetSize(rate);
            }
        }
        #endregion
        #region Exterminate
        if (IsExterminating)
        {
            SetSizeExterminateIndicator();
        }
        #endregion
        #region BAW
        if (IsCastingBAW && m_BAWGuide[Difficult])
        {
            float rate = (Time.time - BAWDamageTimeFactor) / (BAWDamageTiming - BAWDamageTimeFactor);
            int count = BAWGuidePool.ActivatedObject.Count;
            var guids = BAWGuidePool.ActivatedObject;
            for (int index = 0; index < count; ++index)
            {
                guids[index].SetSize(rate);
            }
        }        
        #endregion
    }

    private void SetSizeExterminateIndicator()
    {
        float rate = Mathf.Clamp01(((Health / InitHealth) - InvincibleHealth) / (0.7f - InvincibleHealth));
        ExterminateIndicator.GetComponent<RectTransform>().sizeDelta = new Vector2(EXTERMINATE_INDICATOR_SIZE * rate, EXTERMINATE_INDICATOR_SIZE * rate);
    }

    private IEnumerator Pattern()
    {
        while (IsGameOver == false && IsPaused == false)
        {
            yield return new WaitForSeconds(PatternWaitTime);
            //Logger.Log("Pattern Called");
            if (IsQuizTime && State == EState.Waiting)
            {
                ElapsedPatternTime = 0.0f;
                //퀴즈 패턴
                if (IsQuizShowing)
                {
                    if (EndQuizTime <= Time.time)
                    {
                        EndQuizTime = Time.time + QuizAnswerTime + QuizBreakTime;
                        TakeDamageQuiz();
                        TurnOffChessGuide();
                        QuizBackground.gameObject.SetActive(false);
                        QuizCount++;
                        BreakTime();

                        BanishHeart(true);
                        NextQuizShowTime = Time.time + QuizBreakTime;
                        IsQuizBreakTime = true;
                        if (QuizCount < QuizMaxCount)
                        {
                            Invoke("SetQuizDeadZone", QuizBreakTime - 2.0f);
                        }
                        
                        //SetQuizDeadZone();
                    }

                    if (IsQuizBreakTime == true && NextQuizShowTime <= Time.time)
                    {
                        IsQuizBreakTime = false;
                        BanishHeart(false);
                        ShowQuiz();
                    }

                    if (QuizCount >= QuizMaxCount)
                    {
                        EndQuiz();
                    }
                }
            }
            else
            {
                //if (ElapsedPatternTime > PatternInterval[Difficult])
                if(Time.timeSinceLevelLoad> PatternTime)
                {
                    AI.ChessMove();
                    //AI.SetPositionForNextMove();
                    for (int count = 0; count < PatternCount[Difficult]; count++)
                    {
                        int pattern;
                        State = EState.ExcutingPattern;

                        if (BanSkills[Difficult].Count >= 6 || (BanSkills[Difficult].Count >= 5 && BanSkills[Difficult].Contains(EBanSkill.SummonChess) == false) )
                        {
                            continue;
                        }
                        do
                        {
                            pattern = Random.Range(0, 5);

                        } while (BanSkills[Difficult].Exists(x => x == (EBanSkill)pattern) || TempBanSkills.Exists(x => x == (EBanSkill)pattern) || pattern == (int)EBanSkill.SummonChess);

                        //디버깅용
                        //if (BanSkills.Count + TempBanSkills.Count >= 6)
                        //{
                        //    Logger.Log($"스킬 밴 합 : {BanSkills.Count + TempBanSkills.Count}  (SkillBan {BanSkills.Count}) + (TempSkillBan{TempBanSkills.Count})");
                        //}


                        ExcutePattern(pattern);
                        //임시 밴추가
                        TempBanSkills.Add((EBanSkill)pattern);

                        //테스트용
                        //ExcutePattern(5);

                        //if (!test)
                        //{
                        //    ExcutePattern(4);
                        //    test = true;
                        //}


                    }
                    TempBanSkills.Clear();
                    ElapsedPatternTime = 0.0f;

                    PatternTime = Time.timeSinceLevelLoad + PatternInterval[Difficult];

                    //EndTime = Time.timeSinceLevelLoad;
                    //Logger.Log("Time : " + (EndTime - StartTime));
                    //StartTime = Time.timeSinceLevelLoad;
                }
                //else
                //{
                //    ElapsedPatternTime += PatternWaitTime;
                //}

                //SetEnergy(ElapsedPatternTime);

                #region SummonChess
                if (BanSkills[Difficult].Exists(x => x == EBanSkill.SummonChess) == false && SummonChessTime <= Time.time)
                {
                    SummonChessTime += SummonChessInterval;
                    SummonChessPiece(SummonChessCount[Difficult]);
                }
                #endregion
            }
            #region HeartBlast
            if (IsHeartBlastStarted)
            {
                //충전 끝날때 = 첫타 들어감
                //그러므로 충전하는 모션을 충전 끝나기 전에 시작해야함
                if (HeartBlastStartTime - HeartBlastInterval < Time.time)
                {
                    HeartBlastInnerIndicator.gameObject.SetActive(true);
                    HeartBlastSafeZone.gameObject.SetActive(true);
                    //                                                    안전지대 시작이 왼쪽 모서리 부터 시계방향이기 때문에 각도의 반만큼 반시계로 이동해줌
                    HeartBlastSafeZone.transform.rotation = Quaternion.Euler(90.0f, 0.0f, SafeZoneDir + (SafeZoneAngle / 2.0f));
                    HeartBlastOutline.gameObject.SetActive(true);
                    IsHeartBlastCharging = true;
                    if (HeartBlastStartTime < Time.time)
                    {
                        IsHeartBlastStarted = false;
                        //HeartBlast();
                        //Mat.SetColor("_Color", new Color(1.0f, 0.435294117647f, 0.0f));
                        State = EState.Waiting;
                    }
                }
                //Mat.SetColor("_Color", Mat.GetColor("_Color") * 1.5f);
            }
            #endregion
            #region Black And White(BAW)
            if (IsCastingBAW)
            {
                if (BAWDamageTiming < Time.time)
                {                    
                    ExcuteBlackAndWhiteDamage();
                    foreach (var guide in BAWGuidePool.ActivatedObject)
                    {
                        guide.gameObject.SetActive(false);
                        
                    }
                    BAWGuidePool.ReturnAllObject();
                    State = EState.Waiting;
                }
            }
            #endregion
            #region Flower Card
            if (IsCastingFlowerCard)
            {
                Logger.Log("Connect Call");
                if (FlowerShowEndTime.Peek() < Time.time)
                {
                    Invoke("StartAttackFlowerLine", LineEffectTiming);
                    //ConnectFlowerCard();
                }
                Logger.Log("RemoveFlower Call");
                Logger.Log($"RemoveFlower count : {FlowerShowEndTime.Count}");
                Logger.Log($"RemoveFlower peek : {FlowerShowEndTime.Peek()}");
                if (FlowerShowEndTime.Peek() < Time.time)
                {
                    FlowerShowEndTime.Dequeue();
                    RemoveFlowerCard();
                    //ConnectFlowerCard();
                    State = EState.Waiting;
                }
            }

            if (ActivatedLinesTime.Count > 0)
            {
                Logger.Log("Disconnect Check");
                Logger.Log($"Disconnect count : {ActivatedFlowerLines.Count}");
                Logger.Log($"Disconnect peek : {ActivatedLinesTime.Peek()}");
                if (ActivatedLinesTime.Peek() < Time.time)
                {
                    Logger.Log("Disconnect Call");
                    DisconnectFlowerCard();
                }
            }
            #endregion            
        }
    }

    private void TurnOffChessGuide()
    {
        for (int index = 0; index < 64; ++index)
        {
            DeadZoneInfo[index].SetDeadZoneByPiece(false);
        }
    }

    private void BreakTime()
    {
        QuizQuestionText.color = Color.green;
        QuizQuestionText.text = "Break Time";
        BreakTimeInvincible(true);

        int count = QuizGuidePool.ActivatedObject.Count;
        List<QuizGuide> guides = QuizGuidePool.ActivatedObject;
        for (int index = 0; index < count; ++index)
        {
            guides[index].gameObject.SetActive(false);
        }
        QuizGuidePool.ReturnAllObject();
    }

    private void BanishHeart(bool isVisible)
    {
        if (isVisible)
        {
            Collider.isTrigger = true;
            MeshRenderer.enabled = false;
        }
        else
        {
            Collider.isTrigger = false;
            MeshRenderer.enabled = true;
        }
    }

    private void BreakTimeInvincible(bool IsInvincible)
    {
        if (IsInvincible)
        {
            DamageResistance += 100.0f;
            IsBreakTimeInvincible = true;
        }
        else
        {
            DamageResistance -= 100.0f;
            IsBreakTimeInvincible = false;
        }
        
    }

    private void QuizAlarm()
    {
        Logger.Log("Alarm Start");
        QuizAlarmEndTime = Time.time + QuizAlarmDurationTime;
        IsWaitingStartQuiz = true;
        IsQuizTime = true;
        QuizQuestionText.color = Color.red;
        QuizQuestionText.text = "Quiz begin soon!";
        QuizQuestionText.gameObject.SetActive(true);
        FindObjectOfType<GlobalSound>().OnAlarm(AlarmClip);
        QuizCam.SetActive(true);
        State = EState.Waiting;
        Logger.Log("Remove All Object");
        RemovePatternObject();
        Logger.Log("Invoke SetQuizDeadZone");
        Invoke("SetQuizDeadZone", QuizAlarmDurationTime - 2.0f);

        BreakTimeInvincible(true);
    }

    private void ExcutePattern(int pattern)
    {
        switch (pattern)
        {
            //case 0: SummonCardTornado(); break;
            //case 1: /*SummonChessPiece();*/ break;
            //case 2: BlackAndWhite(); break;
            //case 3: StartHeartBlast(); break;
            //case 4: SummonFlowerCard(); break;
            //case 5: SummonCrossBomb(); break;
            //default: throw new System.ArgumentOutOfRangeException(nameof(pattern));
            case 0: SummonCardTornado(); break;
            case 1: BlackAndWhite(); break;
            case 2: StartHeartBlast(); break;
            case 3: SummonFlowerCard(); break;
            case 4: SummonCrossBomb(); break;
            default: throw new System.ArgumentOutOfRangeException(nameof(pattern));
        }
    }

    private void BoomCrossBomb(int row, int col)
    {
        int boardSize = AI.BoardSize;
        for (int itRow = 0; itRow < boardSize; itRow++)
        {
            BurningFloor newFloor = BurningFloorPool.GetAvailableObject();
            newFloor.SetTimer();
            newFloor.gameObject.SetActive(true);
            newFloor.transform.position = new Vector3(col * AI.GetBOXSIZE, 0.1f, itRow * AI.GetBOXSIZE);
        }
        for (int itCol = 0; itCol < boardSize; itCol++)
        {
            BurningFloor newFloor = BurningFloorPool.GetAvailableObject();
            newFloor.SetTimer();
            newFloor.gameObject.SetActive(true);
            newFloor.transform.position = new Vector3(itCol * AI.GetBOXSIZE, 0.1f, row * AI.GetBOXSIZE);
        }
    }

    private void SummonCrossBomb()
    {
        int col, row;
        col = Random.Range(0, AI.BoardSize);
        row = Random.Range(0, AI.BoardSize);
        CrossBomb newBomb = CrossBombPool.GetAvailableObject();
        newBomb.Row = row;
        newBomb.Col = col;
        newBomb.SetTimer();
        newBomb.gameObject.SetActive(true);
        newBomb.transform.position = new Vector3(col * AI.GetBOXSIZE, 1.0f, row * AI.GetBOXSIZE);
        State = EState.Waiting;
    }

    private void StartHeartBlast()
    {
        Logger.Log("Start Heart Blast");
        IsHeartBlastStarted = true;
        HeartBlastStartTime = Time.time + HeartBlastStartDelayTime;
        SafeZoneDir = Random.Range(0.0f, 360.0f);
        //테스트용
        //SafeZoneDir = 0.0f;
    }

    private void SummonChessPiece(byte summonCount)
    {
        if (BanSkills[Difficult].Exists(x => x == EBanSkill.SummonChess) == false)
        {
            AI.SummonChessPiece(PieceHealth, summonCount);
            State = EState.Waiting;
        }
    }

    private void HeartBlast()
    {
        Vector3 safeZoneDir = new Vector3(0.0f, 0.0f, 1.0f);
        //                                 왜 정방향이 아니라 역방향인지 나도 모르겠음 씨팔
        Quaternion rotation = Quaternion.Euler(0.0f, -SafeZoneDir, 0.0f);
        safeZoneDir = rotation * safeZoneDir;
        Logger.Log($"dir : {safeZoneDir}");

        //float safeZoneCos = Mathf.Cos(SafeZoneAngle * 0.5f * Mathf.Deg2Rad);

        foreach (var playerPos in AI.GetPlayersPos())
        {
            //Logger.Log((transform.position - playerPos.Value).magnitude);
            Vector3 bossToPlayer = (playerPos.Value - transform.position);

            Logger.Log($"angle : {Vector3.Dot(bossToPlayer.normalized, safeZoneDir.normalized)} {SafeZoneCos}");
            bool isInSafeZone = Vector3.Dot(bossToPlayer.normalized, safeZoneDir.normalized) >= SafeZoneCos;
            if (bossToPlayer.magnitude <= HeartBlastRange && isInSafeZone == false)
            {
                playerPos.Key.TakeDamage(HeartBlastDamageParams);
                //Logger.Log($"t : {transform.position} p : {playerPos.Value} m : {(transform.position - playerPos.Value).magnitude}");
            }
        }

        HeartBlastCount++;
        //Logger.Log($"{HeartBlastCount}");

        if (HeartBlastCount >= HeartBlastMaxCount)
        {
            Logger.Log("End Heart Blast");
            HeartBlastCount = 0;
            IsHeartBlastCharging = false;
            HeartBlastInnerIndicator.gameObject.SetActive(false);
            HeartBlastSafeZone.gameObject.SetActive(false);
            HeartBlastOutline.gameObject.SetActive(false);
        }
    }

    private void BlackAndWhite()
    {
        IsCastingBAW = true;
        BAWDamageTimeFactor = Time.time;
        BAWDamageTiming = Time.time + BAWDamageTime;
        IsBlack = System.Convert.ToBoolean(Random.Range(0, 2));

        BAWIndicator.gameObject.SetActive(true);

        if (IsBlack)
        {
            BAWIndicator.color = Color.black;
            SetBAWGuide(true);
        }
        else
        {
            BAWIndicator.color = Color.white;
            SetBAWGuide(false);
        }
    }

    private void SetBAWGuide(bool IsBalck)
    {
        BAWGuide guide;
        float col;
        float row;
        if (IsBalck)
        {
            for (int count = 0; count < 32; ++count)
            {
                guide = BAWGuidePool.GetAvailableObject();
                guide.gameObject.SetActive(true);
                row = Mathf.Floor(count / 4.0f);
                col = (count % 4) * 2.0f + (row + 1) % 2;
                guide.SetPosition(new Vector3(col * AI.GetBOXSIZE, 0.01f, row * AI.GetBOXSIZE));
            }
        }
        else
        {
            for (int count = 0; count < 32; ++count)
            {
                guide = BAWGuidePool.GetAvailableObject();
                guide.gameObject.SetActive(true);
                row = Mathf.Floor(count / 4.0f);
                col = (count % 4) * 2.0f + row % 2;
                guide.SetPosition(new Vector3(col * AI.GetBOXSIZE, 0.01f, row * AI.GetBOXSIZE));
            }
        }
        
    }

    private void ExcuteBlackAndWhiteDamage()
    {
        IsCastingBAW = false;
        //IsCastingBAW = true;
        foreach (var playerPos in AI.GetPlayersPos())
        {
            //0,0의 위치가 체스판 왼쪽아래 꼭지점이 아니라 왼쪽 아래 칸의 정중앙임 이 위치가 체스판의 왼쪽면과 -2만큼 떨어져 있음
            //그러므로 2를 더해줘야 정상적으로 계산이됨
            int row = Mathf.FloorToInt((playerPos.Value.z + 2.0f) / AI.GetBOXSIZE);
            int col = Mathf.FloorToInt((playerPos.Value.x + 2.0f) / AI.GetBOXSIZE);

            //Logger.Log(string.Format("Coord : row:{0} col:{1}", row, col));

            if (IsBlack)
            {
                //행이 짝수면서 열이 홀수면 또는
                //행이 홀수면서 열이 짝수면 검은칸에 있다
                if ((row % 2 == 0 && col % 2 != 0) || (row % 2 != 0 && col % 2 == 0))
                {
                    playerPos.Key.TakeDamage(BAWDamageParams);
                    if (Difficult != (int)EDifficulty.Easy)
                    {
                        SummonChessPiece(PenaltySummonChessCount);
                    }

                }
            }
            else
            {

                //행이 짝수면서 열이 짝수면 또는
                //행이 홀수면서 열이 홀수면 흰칸에 있다
                if ((row % 2 == 0 && col % 2 == 0) || (row % 2 != 0 && col % 2 != 0))
                {
                    playerPos.Key.TakeDamage(BAWDamageParams);
                    if (Difficult != (int)EDifficulty.Easy)
                    {
                        SummonChessPiece(PenaltySummonChessCount);
                    }
                }
            }
        }
        BAWIndicator.gameObject.SetActive(false);
    }

    private void SummonCardTornado()
    {
        int X, Z;
        X = Random.Range(0, AI.BoardSize);
        Z = Random.Range(0, AI.BoardSize);
        Tornado newTornado = TornadoPool.GetAvailableObject();
        newTornado.gameObject.SetActive(true);
        newTornado.ReturnObjectDelegate = TornadoPool.ReturnObject;
        newTornado.transform.position = new Vector3(X * AI.GetBOXSIZE, 1.0f, Z * AI.GetBOXSIZE);
        //Instantiate(m_Tornado, new Vector3(X * AI.GetBOXSIZE, 1.0f, Z * AI.GetBOXSIZE), Quaternion.identity);
    }

    //private bool IsDuplicated(ref int[] ignore, int month)
    //{
    //    for (int ignoreIndex = 0; ignoreIndex < 5; ignoreIndex++)
    //    {
    //        if (ignore[ignoreIndex] == month)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    private void SummonFlowerCard()
    {
        Logger.Log("Summon Flower start");
        IsCastingFlowerCard = true;
        int col, row;
        int month = -1;
        //int[] ignore = new int[5] { -1, -1, -1, -1, -1 };
        Vector3[] pos = new Vector3[2];

        FlowerShowEndTime.Enqueue(Time.time + CardShowingTime);
        ActivatedLinesTime.Enqueue(Time.time + CardShowingTime + LineMaintenanceTime + LineEffectTiming);

        for (int index = 0; index < SummonPairCount * 2; index++)
        {
            col = Random.Range(0, AI.BoardSize);
            row = Random.Range(0, AI.BoardSize);
            int pairIndex = Mathf.FloorToInt((float)index / 2.0f);

            if (AI.FlowerCardExists(row, col))
            {
                index--;
                continue;
            }
            else
            {
                //0 == 짝수 
                if (index % 2 == 0)
                {
                    pos[0] = new Vector3(col * AI.GetBOXSIZE, 0.1f, row * AI.GetBOXSIZE);
                    month++;
                }
                else
                {
                    //홀수에 라인 셋팅
                    pos[1] = new Vector3(col * AI.GetBOXSIZE, 0.1f, row * AI.GetBOXSIZE);
                    FlowerLine newLine = DeactivatedFlowerLines.Dequeue();
                    newLine.SetPos(pos);
                    //newLine.GetComponent<LineRenderer>().SetPositions(pos);
                    ActivatedFlowerLines.Enqueue(newLine);
                }
            }

            //GameObject card = FlowerCards[index].gameObject;
            FlowerCard card = FlowerCards.GetAvailableObject();
            card.transform.position = new Vector3(col * AI.GetBOXSIZE, 0.02f, row * AI.GetBOXSIZE);
            card.GetComponentInChildren<FlowerCard>().SetmMonthImage((ECardType)month);
            AI.SetFlowerCardExists(row, col, true);
            card.Coord = (row, col);
            card.gameObject.SetActive(true);
        }
        ConnectFlowerCard();
        Logger.Log("Summon Flower end");
    }

    private void RemoveFlowerCard()
    {
        Logger.Log("RemoveFlower start");
        if (FlowerShowEndTime.Count == 0)
        {
            IsCastingFlowerCard = false;
        }

        //for (int index = 0; index < SummonPairCount * 2; index++)
        //{
        //    FlowerCards[index].gameObject.SetActive(false);
        //}
        
        for (int index = 0; index < SummonPairCount * 2; index++)
        {
            //Debug.Assert(FlowerCards.ActivatedObject.Count != 0, "Remove Flower Index");
            if (FlowerCards.ActivatedObject.Count != 0)
            {
                FlowerCards.ActivatedObject[0].ReturnObject();
            }
            else
            {
                break;
            }
            
            //FlowerCards.ActivatedObject[index].gameObject.SetActive(false);
            //FlowerCards.ReturnObject(FlowerCards.ActivatedObject[index]);
        }
        Logger.Log("RemoveFlower end");
        //AI.ResetFlowerExistent();
    }

    private void ConnectFlowerCard()
    {
        Logger.Log("Connect start");
        FlowerLine[] lines = ActivatedFlowerLines.ToArray();
        int count = ActivatedFlowerLines.Count;
        for (int index = 0; index < count; index++)
        {
            lines[index].gameObject.SetActive(true);
        }
        Logger.Log("Connect end");
    }

    private void StartAttackFlowerLine()
    {
        FlowerLine[] lines = ActivatedFlowerLines.ToArray();
        int count = ActivatedFlowerLines.Count;
        for (int index = 0; index < count; index++)
        {
            lines[index].CanAttack = true;
        }
    }

    private void DisconnectFlowerCard()
    {
        Logger.Log("Disconnect start");
        FlowerLine returnLine;
        for (int index = 0; index < 5; index++)
        {

            returnLine = ActivatedFlowerLines.Dequeue();
            returnLine.gameObject.SetActive(false);
            DeactivatedFlowerLines.Enqueue(returnLine);
        }
        ActivatedLinesTime.Dequeue();
        Logger.Log("Disconnect end");
    }

    private void SetQuizColor()
    {
        QuizColor = (EQuizColor)Random.Range(0, 2);
        AI.QuizColor = QuizColor;
        if (QuizColor == EQuizColor.Blue)
        {
            QuizQuestionText.color = Color.blue;
        }
        else
        {
            QuizQuestionText.color = Color.red;
        }

    }

    private void StartQuiz()
    {
        //하드모드는 TakeDamage에서 계산
        //if (IsTriggeredSecondQuiz == false && (EDifficulty)Difficult != EDifficulty.Hard)
        //{
        //    SetInvincible(true);
        //}

        AI.ShowUpQuizText();
        IsQuizShowing = true;
        IsQuizBreakTime = false;
        EndQuizTime = Time.time + QuizAnswerTime;
        NextQuizShowTime = float.MaxValue;

        //RemovePatternObject();
    }

    private void RemovePatternObject()
    {
        Logger.Log("Delete CroosBomb");
        DeleteAllCrossBombs();
        Logger.Log("Delete FlowerLine");
        DeleteAllFlowerLines();
        Logger.Log("Delete FlowerCard");
        DeleteAllFlowerCards();
        Logger.Log("Delete Piece");
        DeleteAllChessPieces();
        Logger.Log("Delete Tornado");
        DeleteAllTornados();
    }



    private void DeleteAllTornados()
    {
        //TornadoPool.ReturnAllObject();
        var tornados = TornadoPool.ActivatedObject.ConvertAll(i => i);
        int maxCount = TornadoPool.ActivatedObject.Count;
        for (int index = 0; index < maxCount; index++)
        {
            tornados[index].DeleteAllCards();
            tornados[index].ReturnObject();
        }
    }

    private void DeleteAllFlowerCards()
    {
        //foreach (var flowerCard in FlowerCards)
        //{
        //    flowerCard.gameObject.SetActive(false);
        //}

        for (int index = 0; index < FlowerCards.ActivatedObject.Count; index++)
        {
            FlowerCards.ActivatedObject[index].gameObject.SetActive(false);
        }

        FlowerCards.ReturnAllObject();
        AI.ResetFlowerExistent(FlowerCards.ActivatedObject.Count);
    }

    private void DeleteAllCrossBombs()
    {
        var bombs = CrossBombPool.ActivatedObject.ConvertAll(i => i);
        int maxCount = CrossBombPool.ActivatedObject.Count;
        for (int index = 0; index < maxCount; index++)
        {
            bombs[index].ReturnObject();
        }
        //CrossBombPool.ReturnAllObject();



        foreach (var effect in mBombEffects.GetComponentsInChildren<ParticleSystem>())
        {
            effect.gameObject.SetActive(false);
        }

        var floors = BurningFloorPool.ActivatedObject.ConvertAll(i => i);
        maxCount = BurningFloorPool.ActivatedObject.Count;
        for (int index = 0; index < maxCount; index++)
        {
            floors[index].ReturnObject();
        }
    }

    private void DeleteAllFlowerLines()
    {
        int maxCount = ActivatedLinesTime.Count;
        for (int count = 0; count < maxCount; count++)
        {
            DisconnectFlowerCard();
            //ActivatedLinesTime.Dequeue();
        }
    }

    private void DeleteAllChessPieces()
    {
        AI.DeleteAllChessPieces();
    }

    private void ShowQuiz()
    {
        BreakTimeInvincible(false);

        /*
         * 바닥 숫자 정할때 안전구역 확보하려면 색깔 먼저 정해져야함. 또한 데드존을 기준으로 계산 건너뛰는걸 계산하므로 역시 있어야함.
         * 슈퍼 체스말 소환할때 바닥 숫자를 가져와서 검사하므로 색 -> 바닥 숫자 -> 체스말 순서가 보장돼야함
         * 최종 : 색, 정답 생성, 데드존 생성 -> 바닥 숫자 -> 체스말  소환
         */
        SetQuizColor();
        SetQuizAnswer();
        QuizBackground.gameObject.SetActive(true);
        //SetQuizDeadZone();
        AI.ShowFloorQuizNumber(QuizAnswer, QuizColor == EQuizColor.Red ? true : false);
        if (QuizCount < QUIZ_MAX_COUNT)
        {
            //Logger.Log($"Max count {QuizMaxCount}");
            SummonSuperPieces();
        }

        ShouldShowQuizGuid = m_QuizGuide[Difficult];
        QuizTimeFactor = Time.time;
        if (ShouldShowQuizGuid)
        {
            if (QuizColor == EQuizColor.Red)
            {
                for (int row = 0; row < 8; ++row)
                {
                    for (int col = 0; col < 8; ++col)
                    {
                        if(AI.GetQuizNumber(row, col) == QuizAnswer)
                        {
                            QuizGuide guide = QuizGuidePool.GetAvailableObject();
                            guide.gameObject.SetActive(true);
                            guide.SetPosition(new Vector3(col * AI.GetBOXSIZE, 0.01f, row * AI.GetBOXSIZE));
                        }
                    }
                }
            }
            else
            {
                for (int row = 0; row < 8; ++row)
                {
                    for (int col = 0; col < 8; ++col)
                    {
                        if (AI.GetQuizNumber(row, col) != QuizAnswer)
                        {
                            QuizGuide guide = QuizGuidePool.GetAvailableObject();
                            guide.gameObject.SetActive(true);
                            guide.SetPosition(new Vector3(col * AI.GetBOXSIZE, 0.01f, row * AI.GetBOXSIZE));
                        }
                    }
                }
            }
        }
    }

    private void SummonSuperPieces()
    {
        AI.SummonChessSuperPiece(SuperPieceHealth);
    }

    //임시 함수 
    public void DecreaseResi()
    {
        DamageResistance -= 0.75f;
    }

    private void ClearNewDeadZone()
    {
        foreach (var deadZone in NewDeadZone)
        {
            deadZone.deadZone.IsReadied = true;
        }
        NewDeadZone.Clear();
    }

    public void SetQuizDeadZone()
    {
        Logger.Log("SetQuizDeadZone");        
        
        foreach (var deadZone in DeadZoneInfo)
        {
            if (deadZone.Order == QuizCount)
            {
                deadZone.IsCurrDeadZone = true;
                deadZone.IsReadied = false;
                NewDeadZone.Add((deadZone, deadZone.Coord[0], deadZone.Coord[1]));
            }
        }

        Invoke("ClearNewDeadZone", 2.0f);

        int maxIndex = NewDeadZone.Count;

        for (int index = 0; index < maxIndex; index++)
        {
            ChessDeadZone deadZone = DeadZonePool.GetAvailableObject();
            deadZone.transform.position = new Vector3(NewDeadZone[index].row * AI.GetBOXSIZE, 0.1f, NewDeadZone[index].col * AI.GetBOXSIZE);
            deadZone.transform.rotation = new Quaternion();
            deadZone.SetColor();
            deadZone.gameObject.SetActive(true);
        }        

        //var deadZones = DeadZonePool.ActivatedObject;
        //for (int index = 0; index < deadZones.Count; index++)
        //{
        //    deadZones[index].gameObject.SetActive(false);
        //}
        //DeadZonePool.ReturnAllObject();

        //for (int index = 0; index < 64; index++)
        //{
        //    //AI.SetDeadZone(CurrDeadZone[index][0], CurrDeadZone[index][1]);
        //    if (DeadZoneInfo[index].IsCurrDeadZone)
        //    {
        //        ChessDeadZone deadZone = DeadZonePool.GetAvailableObject();
        //        deadZone.transform.position = new Vector3(DeadZoneInfo[index].Coord[0] * AI.GetBOXSIZE, 0.1f, DeadZoneInfo[index].Coord[1] * AI.GetBOXSIZE);
        //        deadZone.transform.rotation = new Quaternion();
        //        deadZone.SetColor();
        //        deadZone.gameObject.SetActive(true);
        //    }

        //}

        //for (int index = 0; index < CurrDeadZone.Count; index++)
        //{
        //    //AI.SetDeadZone(CurrDeadZone[index][0], CurrDeadZone[index][1]);
        //    DeadZone deadZone = DeadZonePool.GetAvailableObject();
        //    deadZone.transform.position = new Vector3(CurrDeadZone[index][0] * AI.GetBOXSIZE, 0.1f, CurrDeadZone[index][1] * AI.GetBOXSIZE);
        //    deadZone.transform.rotation = new Quaternion();
        //    deadZone.gameObject.SetActive(true);
        //}
    }

    private void SetQuizAnswer()
    {
        int number1 = Random.Range(1, 6);
        int number2 = Random.Range(1, 6);

        QuizAnswer = Mathf.Abs(number1 - number2);

        if (number1 > number2)
        {
            QuizQuestionText.text = string.Format(QuizQuestionFormat, number1, number2);
        }
        else
        {
            QuizQuestionText.text = string.Format(QuizQuestionFormat, number2, number1);
        }

        
    }

    private void TakeDamageQuiz()
    {
        if (QuizColor == EQuizColor.Red)
        {
            foreach (var playerInfo in AI.GetPlayersPos())
            {
                int row = Mathf.FloorToInt((playerInfo.Value.z + 2.0f) / AI.GetBOXSIZE);
                int col = Mathf.FloorToInt((playerInfo.Value.x + 2.0f) / AI.GetBOXSIZE);

                if (AI.GetQuizNumber(row, col) == QuizAnswer) playerInfo.Key.TakeDamage(QuizDamageParams);
            }
        }
        else
        {
            foreach (var playerInfo in AI.GetPlayersPos())
            {
                int row = Mathf.FloorToInt((playerInfo.Value.z + 2.0f) / AI.GetBOXSIZE);
                int col = Mathf.FloorToInt((playerInfo.Value.x + 2.0f) / AI.GetBOXSIZE);

                if (AI.GetQuizNumber(row, col) != QuizAnswer) playerInfo.Key.TakeDamage(QuizDamageParams);
            }
        }

        //공격 예정 마킹은 슈퍼 체스말 소환에서 관리
        //DeadZoneInfo에 슈퍼 체스말의 공격 예정인 곳을 표시함.
        //AttackMarkingSuperPieces();

        foreach (var playerInfo in AI.GetPlayersPos())
        {
            int row = Mathf.FloorToInt((playerInfo.Value.z + 2.0f) / AI.GetBOXSIZE);
            int col = Mathf.FloorToInt((playerInfo.Value.x + 2.0f) / AI.GetBOXSIZE);

            if (DeadZoneInfo[row * 8 + col].IsDeadZoneByPiece)
            {
                playerInfo.Key.TakeDamage(QuizDamageParams);
            }
        }
        AI.ReturnSuperChessPieces();
    }

    private void EndQuiz()
    {
        SetInvincible(false);
        ExterminateIndicator.SetActive(false);
        IsExterminating = false;

        DamageResistance -= 100.0f;

        float healthPercent = (Health / InitHealth);
        if (((EDifficulty)Difficult) != EDifficulty.Easy && IsTriggeredFirstQuiz && healthPercent > InvincibleHealth)
        {
            ExterminatePlayers(IsIgnoreInvincible);
        }

        if (IsTriggeredFirstQuiz == true && IsTriggeredSecondQuiz == false)
        {
            DamageResistance -= FirstHeartAdditionalDamage[Difficult];
        }

        AI.EndQuiz();
        QuizQuestionText.gameObject.SetActive(false);
        var deadZones = DeadZonePool.ActivatedObject;
        for (int index = 0; index < deadZones.Count; index++)
        {
            deadZones[index].gameObject.SetActive(false);
        }
        for (int index = 0; index < 64; index++)
        {
            DeadZoneInfo[index].IsCurrDeadZone = false;
        }

        DeadZonePool.ReturnAllObject();
        QuizCount = 0;
        IsQuizTime = false;
        IsQuizBreakTime = false;
        IsQuizShowing = false;
        QuizCam.SetActive(false);
        BanishHeart(false);
        //체스소환 타이머 초기화
        SummonChessTime = Time.time + SummonChessInterval;
    }

    protected override void ExterminatePlayers(bool ignoreInvincible)
    {
        Vector3 pos = transform.position;
        pos.y = 2.0f;
        GameObject Effect = Instantiate(QuizExterminateEffect, pos, transform.rotation);
        base.ExterminatePlayers(ignoreInvincible);
    }
}
