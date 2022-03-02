using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public struct SBoardInfo
{
    public int QuizNumber;
    public bool ChessExists;
    public bool FlowerCardExists;
    public Text NumberText;
}

public struct SFlowerCoord
{
    public int row;
    public int col;
    public SFlowerCoord(int _row, int _col)
    {
        row = _row;
        col = _col;
    }
}

public class ChessAI : MonoBehaviour
{
    public int m_BoardSize;
    public float m_MoveTime;
    public GameObject m_ChessPrefab;
    public Dictionary<EPieceType, Mesh> m_PieceMesh;
    public Text m_TextPrefab;
    public Transform m_BoardTransform;
    public Material m_NormalPieceMaterial;
    public Material m_SuperPieceMaterial;
    //왼쪽아래가 0,0임. 0행 최하단, BoardSize행 최상단 
    private SBoardInfo[,] mBoardQuizInfo;
    private int mQuizAnswer;
    private Dictionary<PlayerInformation, Vector3> mPlayerPos;
    private List<GameObject> mChessPool;
    private List<ChessPiecesInformation> mActivatedChess;
    private GameObject mAvailableChess;
    private ChessBoss mBoss;
    //private List<QuizDeadZone> mBoss.DeadZoneInfo;
    private float mDurationMoveTime;
    private const float BOXSIZE = 4.0f;
    private EQuizColor mQuizColor;
    private bool mbIsGameOver;

    private int mSummonCountSuperPiece;

    [SerializeField]
    private bool m_AnswerDebugMode = false;

    private static int testCount = 0;

    private AddressableHandleManager mAddressableHandles;

    private List<SFlowerCoord> mFlowerCoord;

    private ChessBoss Boss { get { return mBoss; } }
    private int QuizAnswer { get { return mQuizAnswer; } set { mQuizAnswer = value; } }
    //private List<QuizDeadZone> Boss.DeadZoneInfo { get { return mBoss.DeadZoneInfo; } set { mBoss.DeadZoneInfo = value; } }
    private float DurationMoveTime { get { return mDurationMoveTime; } set { mDurationMoveTime = value; } }
    private float MoveTime { get { return m_MoveTime; } }
    private List<SFlowerCoord> FlowerCoord { get { return mFlowerCoord; } }
    public void ResetFlowerExistent(int count)
    {
        for (int index = 0; index < count; index++)
        {
            SetFlowerCardExists(FlowerCoord[index].row, FlowerCoord[index].col, false);
        }
        FlowerCoord.Clear();
    }
    public int BoardSize { get { return m_BoardSize; } set { m_BoardSize = value; } }
    public float GetBOXSIZE { get { return BOXSIZE; } }
    public int GetQuizNumber(int row, int col) { return mBoardQuizInfo[row, col].QuizNumber; }
    public bool IsPieceExistent(int row, int col) { return mBoardQuizInfo[row, col].ChessExists; }
    public bool FlowerCardExists(int row, int col) { return mBoardQuizInfo[row, col].FlowerCardExists; }
    public void SetIsPieceExistent(int row, int col, bool newState) { mBoardQuizInfo[row, col].ChessExists = newState; /*if (newState) { mBoardQuizInfo[row, col].NumberText.text = "1"; } else { mBoardQuizInfo[row, col].NumberText.text = "0"; }*/ }
    public void SetFlowerCardExists(int row, int col, bool newState) { mBoardQuizInfo[row, col].FlowerCardExists = newState; if (newState) FlowerCoord.Add(new SFlowerCoord(row, col)); }

    //public Vector3 PlayerPos { get { return mPlayerPos; } set { mPlayerPos = value; } }
    public void SetPlayerPos(PlayerInformation player, Vector3 newPos)
    {
        if (mPlayerPos.TryGetValue(player, out Vector3 t) == true)
        {
            mPlayerPos[player] = newPos;
        }
        else
        {
            mPlayerPos.Add(player, newPos);
        }
    }
    public Vector3 GetPlayerPos(PlayerInformation player) { return mPlayerPos[player]; }
    public Dictionary<PlayerInformation, Vector3> GetPlayersPos() { return mPlayerPos; }
    private List<GameObject> ChessPool { get { return mChessPool; } }
    private List<ChessPiecesInformation> ActivatedChess { get { return mActivatedChess; } }
    private void AddActivatedChess(ChessPiecesInformation newPirce) { mActivatedChess.Add(newPirce); }
    private void ClearBoardInfo()
    {
        for (int outterIndex = 0; outterIndex < BoardSize; outterIndex++)
        {
            for (int innerIndex = 0; innerIndex < BoardSize; innerIndex++)
            {
                mBoardQuizInfo[outterIndex, innerIndex].ChessExists = false;
            }
        }
    }

    private int GetActivatedCount() { return mActivatedChess.Count; }

    private Mesh GetMesh(EPieceType type)
    {
        return m_PieceMesh[type];
    }

    private GameObject AvailableChess { set { mAvailableChess = value; } }
    private GameObject GetAvailableChess() { return mAvailableChess; }
    private bool IsGameOver { get { return mbIsGameOver; } set { mbIsGameOver = value; } }
    public EQuizColor QuizColor { get { return mQuizColor; } set { mQuizColor = value; } }
    public int SummonCountSuperPiece { get { return mSummonCountSuperPiece; } set { mSummonCountSuperPiece = value; } }

    public void SummonChessPiece(float health, byte maxCount)
    {
        byte summonCount = 0;

        //퀴즈패턴 체스말 3개 대비용으로 61개까지만 소환함
        while (summonCount < maxCount && GetActivatedCount() < 64 - SummonCountSuperPiece)
        {
            int col;
            int row;
            if (GetAvailablePlace(out row, out col))
            {
                EPieceType type = (EPieceType)Random.Range(0, 6);
                GameObject newPiece = GetAvailableChess();
                ChessPiecesInformation pieceInfo = newPiece.GetComponent<ChessPiecesInformation>();
                pieceInfo.InitHealth = health;
                pieceInfo.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
                pieceInfo.IsSuperPiece = false;
                pieceInfo.SetPieceType(type, GetMesh(type), m_NormalPieceMaterial);
                pieceInfo.SetCoord(col, row);                
                pieceInfo.InitDissolve();                
                newPiece.GetComponentInChildren<Transform>().position = new Vector3(col * GetBOXSIZE, 0.0f, row * GetBOXSIZE);
                //pieceInfo.AI = this;
                newPiece.SetActive(true);
                AvailableChess = pieceInfo.Next as GameObject;
                AddActivatedChess(pieceInfo);
                SetIsPieceExistent(row, col, true);
                pieceInfo.SetPositionForNextMove();
                summonCount++;
            }
            else
            {
                break;
            }
        }
    }

    public void SummonChessSuperPiece(float health)
    {
        int summonCount = 0;

        while (summonCount < SummonCountSuperPiece && GetActivatedCount() < 64)
        {
            int row;
            int col;

            if (GetAvailablePlace(out row, out col))
            {
                EPieceType type = (EPieceType)Random.Range(0, 6);
                GameObject newPiece = GetAvailableChess();
                ChessPiecesInformation pieceInfo = newPiece.GetComponent<ChessPiecesInformation>();
                pieceInfo.InitHealth = health;
                pieceInfo.transform.localScale = new Vector3(3.4f, 3.4f, 3.4f);
                pieceInfo.IsSuperPiece = true;
                pieceInfo.SetPieceType(type, GetMesh(type), m_NormalPieceMaterial);
                //pieceInfo.SetMPBColor(new Color(1.0f, 0.231372549f, 0.231372549f, 1.0f));
                pieceInfo.SetCoord(col, row);
                pieceInfo.InitDissolve();
                newPiece.GetComponentInChildren<Transform>().position = new Vector3(col * GetBOXSIZE, 0.0f, row * GetBOXSIZE);
                newPiece.SetActive(true);
                AvailableChess = pieceInfo.Next as GameObject;
                AddActivatedChess(pieceInfo);
                SetIsPieceExistent(row, col, true);
                summonCount++;
            }

            if (summonCount >= SummonCountSuperPiece)
            {
                ReAttackMarkingSuperPieces();

                //마킹 초기화
                //for (int index = 0; index < 64; index++)
                //{
                //    Boss.DeadZoneInfo[index].IsDeadZoneByPiece = false;
                //}
                //AttackMarkingSuperPieces();

                if (IsExistentSafeZone())
                {
                    //생존구역 있으면 끝
                    return;
                }
                else
                {
                    //없으면 지우고 다시루프
                    ReturnSuperChessPieces();
                    //SummonChessSuperPiece(health);
                    summonCount = 0;
                }

            }
        }

        //if (summonCount >= 3)
        //{
        //    //마킹 초기화
        //    for (int index = 0; index < 64; index++)
        //    {
        //        Boss.DeadZoneInfo[index].IsDeadZoneByPiece = false;
        //    }
        //    AttackMarkingSuperPieces();

        //    if (IsExistentSafeZone())
        //    {
        //        //생존구역 있으면 끝
        //        return;
        //    }
        //    else
        //    {
        //        //없으면 지우고 재귀호출
        //        ReturnSuperChessPieces();
        //        SummonChessSuperPiece(health);
        //        summonCount = 0;
        //    }

        //}
    }

    public void ReAttackMarkingSuperPieces()
    {
        for (int index = 0; index < 64; index++)
        {
            Boss.DeadZoneInfo[index].SetDeadZoneByPiece(false);
        }
        AttackMarkingSuperPieces();
    }

    private bool IsExistentSafeZone()
    {
        int safeCount = 0;
        bool isBlue = false;
        bool isAnswerMatch;
        bool isSafeZone;

        if (QuizColor == EQuizColor.Blue)
        {
            isBlue = true;
        }

        for (int index = 0; index < 64; index++)
        {
            int row = Boss.DeadZoneInfo[index].Coord[0];
            int col = Boss.DeadZoneInfo[index].Coord[1];
            isAnswerMatch = GetQuizNumber(row, col) == QuizAnswer;
            isSafeZone = Boss.DeadZoneInfo[index].IsCurrDeadZone == false && Boss.DeadZoneInfo[index].IsDeadZoneByPiece == false;
            if (isSafeZone && ((isAnswerMatch && isBlue) || (isAnswerMatch == false && isBlue == false)))
            {
                safeCount++;
            }

            //슈퍼 체스말에 의한 데드존이 아니면서 해당 구역이 정답이면 safeCount++
            //if (Boss.DeadZoneInfo[index].IsDeadZoneByPiece == false && Boss.DeadZoneInfo[index].IsDeadZone == false && ( (isAnswerMatch && isBlue) || (isAnswerMatch == false && isBlue == false)) )
            //{
            //    safeCount++;
            //}
        }

        if (safeCount == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool GetAvailablePlace(out int row, out int col)
    {
        bool result = true;
        row = Random.Range(0, BoardSize);
        col = Random.Range(0, BoardSize);

        if (GetActivatedCount() >= 60)
        {
            for (int index = 0; index < 64; index++)
            {

                result &= IsPieceExistent(index / 8, index % 8);

                if (result == false)
                {
                    row = index / 8;
                    col = index % 8;
                    return true;
                }
            }
            for (int index = 0; index < 64; index += 8)
            {
                Logger.Log(IsPieceExistent(index / 8, index % 8) + " " + IsPieceExistent(index / 8, index + 1 % 8) + " " + IsPieceExistent(index / 8, index + 2 % 8) + " " + IsPieceExistent(index / 8, index + 3 % 8)
                    + " " + IsPieceExistent(index / 8, index + 4 % 8) + " " + IsPieceExistent(index / 8, index + 5 % 8) + " " + IsPieceExistent(index / 8, index + 6 % 8)
                    + " " + IsPieceExistent(index / 8, index + 7 % 8));
            }
            //Logger.Log(testCount++ + "사용 가능한 장소 존재? : " + !result + " " + row + " " + col);
        }
        else
        {
            //60개 이하면 랜덤돌리기
            do
            {
                row = Random.Range(0, BoardSize);
                col = Random.Range(0, BoardSize);
            }
            while (IsPieceExistent(row, col));

            return true;
        }
        return false;
    }

    public void ResourcesUnload()
    {
        Resources.UnloadUnusedAssets();
    }

    private void PlayerMovingEvent(object sender, MoveStateChangeArgs e)
    {
        SetPlayerPos(e.Player, e.PlayerPos);
        //Logger.Log(string.Format("PlayerPos : {0}", PlayerPos));
    }
    private void Awake()
    {
        FindObjectOfType<GameManager>().GameOverEvent += OnGameOver;
        IsGameOver = false;
    }

    protected void OnGameOver(object sender, GameOverEventArgs e)
    {
        IsGameOver = true;
    }

    private void Start()
    {
        mBoss = FindObjectOfType<ChessBoss>();
        mPlayerPos = new Dictionary<PlayerInformation, Vector3>();
        AvailableChess = null;
        m_PieceMesh = new Dictionary<EPieceType, Mesh>(6);
        mBoardQuizInfo = new SBoardInfo[BoardSize, BoardSize];
        mActivatedChess = new List<ChessPiecesInformation>(BoardSize * BoardSize);
        mChessPool = new List<GameObject>(BoardSize * BoardSize);
        mFlowerCoord = new List<SFlowerCoord>(10);
        FindObjectOfType<PlayerInformation>().GetComponentInChildren<PlayerMovement>().PlayerMovingEvent += PlayerMovingEvent;
        for (int index = 0; index < BoardSize * BoardSize; index++)
        {
            GameObject GO = Instantiate(m_ChessPrefab);
            mChessPool.Add(GO);
            GO.GetComponent<ChessPiecesInformation>().Next = GetAvailableChess();
            AvailableChess = GO;
            GO.SetActive(false);
        }

        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                mBoardQuizInfo[row, col].NumberText = Instantiate(m_TextPrefab);
                Transform trans = mBoardQuizInfo[row, col].NumberText.transform;
                mBoardQuizInfo[row, col].NumberText.fontSize = 125;

                trans.SetParent(m_BoardTransform);
                //Canvas의 크기 50,50 체스보드 = 8,8칸  50 / 8 = 6.25
                trans.localPosition = new Vector3(-21.875f + 6.25f * col, -21.875f + 6.25f * row, -1.0f);
                trans.localRotation = Quaternion.Euler(Vector3.zero);
                trans.localScale = new Vector3(0.05f, 0.05f, 0.05f);

                mBoardQuizInfo[row, col].NumberText.gameObject.SetActive(false);
            }
        }

        //m_PieceMesh.Add(EPieceType.Bishop, Resources.Load<Mesh>("Enemy/Chess/ChessPieceMesh/bishop"));
        //m_PieceMesh.Add(EPieceType.King, Resources.Load<Mesh>("Enemy/Chess/ChessPieceMesh/king"));
        //m_PieceMesh.Add(EPieceType.Knight, Resources.Load<Mesh>("Enemy/Chess/ChessPieceMesh/knight"));
        //m_PieceMesh.Add(EPieceType.Pawn, Resources.Load<Mesh>("Enemy/Chess/ChessPieceMesh/pawn"));
        //m_PieceMesh.Add(EPieceType.Queen, Resources.Load<Mesh>("Enemy/Chess/ChessPieceMesh/queen"));
        //m_PieceMesh.Add(EPieceType.Rook, Resources.Load<Mesh>("Enemy/Chess/ChessPieceMesh/rook"));

        mAddressableHandles = new AddressableHandleManager(6);

        Addressables.LoadAssetAsync<Mesh>("ChessBishop").Completed += (obj) => { mAddressableHandles.Add(obj); m_PieceMesh.Add(EPieceType.Bishop, obj.Result); };
        Addressables.LoadAssetAsync<Mesh>("ChessKing").Completed += (obj) => { mAddressableHandles.Add(obj); m_PieceMesh.Add(EPieceType.King, obj.Result); };
        Addressables.LoadAssetAsync<Mesh>("ChessKnight").Completed += (obj) => { mAddressableHandles.Add(obj); m_PieceMesh.Add(EPieceType.Knight, obj.Result); };
        Addressables.LoadAssetAsync<Mesh>("ChessPawn").Completed += (obj) => { mAddressableHandles.Add(obj); m_PieceMesh.Add(EPieceType.Pawn, obj.Result); };
        Addressables.LoadAssetAsync<Mesh>("ChessQueen").Completed += (obj) => { mAddressableHandles.Add(obj); m_PieceMesh.Add(EPieceType.Queen, obj.Result); };
        Addressables.LoadAssetAsync<Mesh>("ChessRook").Completed += (obj) => { mAddressableHandles.Add(obj); m_PieceMesh.Add(EPieceType.Rook, obj.Result); };

        //m_PieceMesh.Add(EPieceType.Bishop,    Addressables.LoadAssetAsync<Mesh>("ChessBishop").Result);
        //m_PieceMesh.Add(EPieceType.King,      Addressables.LoadAssetAsync<Mesh>("ChessKing").Result);
        //m_PieceMesh.Add(EPieceType.Knight,    Addressables.LoadAssetAsync<Mesh>("ChessKnight").Result);
        //m_PieceMesh.Add(EPieceType.Pawn,      Addressables.LoadAssetAsync<Mesh>("ChessPawn").Result);
        //m_PieceMesh.Add(EPieceType.Queen,     Addressables.LoadAssetAsync<Mesh>("ChessQueen").Result);
        //m_PieceMesh.Add(EPieceType.Rook,      Addressables.LoadAssetAsync<Mesh>("ChessRook").Result);

        //Addressables.LoadAssetAsync<Mesh>("ChessBishop").Completed += OnAddMesh;
        DurationMoveTime = 0.0f;
    }

    private void OnAddMesh(AsyncOperationHandle<Mesh> obj)
    {
        mAddressableHandles.Add(obj);

    }



    // Update is called once per frame
    //void Update()
    //{
    //    if (IsGameOver == true)
    //    {
    //        return;
    //    }

    //    if (DurationMoveTime < MoveTime)
    //    {
    //        DurationMoveTime += Time.deltaTime;
    //    }
    //    else
    //    {
    //        for (int index = 0; index < GetActivatedCount(); index++)
    //        {
    //            ActivatedChess[index].Move();
    //        }
    //        DurationMoveTime = 0.0f;
    //    }
    //}

    public void ChessMove()
    {
        for (int index = 0; index < GetActivatedCount(); index++)
        {
            ActivatedChess[index].Move();
        }
    }

    //public void SetPositionForNextMove()
    //{
    //    for (int index = 0; index < GetActivatedCount(); index++)
    //    {
    //        ActivatedChess[index].SetPositionForNextMove();
    //    }        
    //}

    public void AttackMarkingSuperPieces()
    {
        for (int index = 0; index < ActivatedChess.Count; index++)
        {
            if (ActivatedChess[index].IsSuperPiece && ActivatedChess[index].IsDead == false)
            {
                ActivatedChess[index].AttackMarkingSuperPiece();
            }
        }
    }

    public void ShowUpQuizText()
    {
        foreach (var info in mBoardQuizInfo)
        {
            info.NumberText.gameObject.SetActive(true);
        }
    }
    public void ShowFloorQuizNumber(int answer, bool isRed)
    {
        Dictionary<(int row, int col), bool> ignoreMatrix = new Dictionary<(int row, int col), bool>(64);
        QuizAnswer = answer;
        int deadZoneCount = 0;

        for (int index = 0; index < 64; index++)
        {
            if (Boss.DeadZoneInfo[index].IsCurrDeadZone)
            {
                ignoreMatrix.Add((Boss.DeadZoneInfo[index].Coord[0], Boss.DeadZoneInfo[index].Coord[1]), true);
                deadZoneCount++;
            }
        }
        
        //for (int index = 0; index < deadZoneCount; index++)
        //{
        //    ignoreMatrix.Add((currDeadZone[index][0], currDeadZone[index][1]), true);
        //}

        //맵이 즉사구역으로 가득찼다면 함수 종료
        if (ignoreMatrix.Count == 64)
        {
            return;
        }

        ResetQuizNumber();
        //맵 전체에 골고루 번호 뿌리기
        int quizNumber = Random.Range(0, 5);
        for (int count = 0; count < 64 - deadZoneCount;)
        {
            int row = Random.Range(0, 8);
            int col = Random.Range(0, 8);

            if (!ignoreMatrix.ContainsKey((row, col)) && mBoardQuizInfo[row, col].QuizNumber == -1)
            {
                mBoardQuizInfo[row, col].QuizNumber = quizNumber;
                //디버깅용 뒷자리
                //mBoardQuizInfo[row, col].NumberText.text = quizNumber.ToString() + (Boss.DeadZoneInfo[row * 8 + col].IsDeadZoneByPiece ? "1" : "0");
                if (m_AnswerDebugMode)
                {
                    mBoardQuizInfo[row, col].NumberText.fontSize = 25;
                    if (isRed)
                    {
                        int num = mBoardQuizInfo[row, col].QuizNumber;
                        bool isPiece = Boss.DeadZoneInfo[row * 8 + col].IsDeadZoneByPiece;
                        string p = isPiece ? "t" : "f";
                        mBoardQuizInfo[row, col].NumberText.text = (num == QuizAnswer || (isPiece ? true : false)) ? $"D {num} {p}" : $"S {num} {p}";
                    }
                    else
                    {
                        int num = mBoardQuizInfo[row, col].QuizNumber;
                        bool isPiece = Boss.DeadZoneInfo[row * 8 + col].IsDeadZoneByPiece;
                        string p = isPiece ? "t" : "f";
                        mBoardQuizInfo[row, col].NumberText.text = (num == QuizAnswer && (isPiece ? false : true)) ? $"S {num} {p}" : $"D {num} {p}";
                    }

                }
                else
                {
                    mBoardQuizInfo[row, col].NumberText.text = quizNumber.ToString();
                }

                quizNumber = (quizNumber + 1) % 5;
                count++;
            }
        }

        //답 보장 하나 만들기
        //체스말에 의한 즉사구역은 체스 소환때 검사함
        if (isRed)
        {
            //Red
            int safeNumber;

            while (true)
            {
                safeNumber = Random.Range(0, 5);

                if (safeNumber != answer)
                {
                    break;
                }
            }

            while (true)
            {
                int row = Random.Range(0, 8);
                int col = Random.Range(0, 8);

                if (!ignoreMatrix.ContainsKey((row, col)))
                {
                    Logger.Log($"(red)Make Safe Zone : {row}, {col}, {safeNumber}");
                    mBoardQuizInfo[row, col].QuizNumber = safeNumber;
                    mBoardQuizInfo[row, col].NumberText.text = safeNumber.ToString();
                    break;
                }
            }
        }
        else
        {
            //Blue
            while (true)
            {
                int row = Random.Range(0, 8);
                int col = Random.Range(0, 8);

                if (!ignoreMatrix.ContainsKey((row, col)))
                {
                    Logger.Log($"(blue)Make Safe Zone : {row}, {col}, {answer}");
                    mBoardQuizInfo[row, col].QuizNumber = answer;
                    mBoardQuizInfo[row, col].NumberText.text = answer.ToString();
                    break;
                }
            }
        }

        ignoreMatrix.Clear();
    }

    private void ResetQuizNumber()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                mBoardQuizInfo[row, col].QuizNumber = -1;
                mBoardQuizInfo[row, col].NumberText.text = " ";
            }
        }
    }

    //디버깅용
    public void SetDeadZone(int row, int col)
    {
        mBoardQuizInfo[row, col].NumberText.text = "D";
    }

    public void EndQuiz()
    {
        foreach (var info in mBoardQuizInfo)
        {
            info.NumberText.gameObject.SetActive(false);
        }
        foreach (var info in Boss.DeadZoneInfo)
        {
            info.SetDeadZoneByPiece(false);
        }
        
    }

    public void ReturnChessPiece(ChessPiecesInformation chessInfo)
    {
        int row;
        int col;

        chessInfo.Next = GetAvailableChess();
        AvailableChess = chessInfo.gameObject;
        chessInfo.gameObject.SetActive(false);
        ActivatedChess.Remove(chessInfo);

        chessInfo.GetCoord(out row, out col);
        SetIsPieceExistent(row, col, false);
    }

    public void ReturnSuperChessPieces()
    {
        List<ChessPiecesInformation> temp = new List<ChessPiecesInformation>(3);
        for (int index = 0; index < ActivatedChess.Count; index++)
        {
            if (ActivatedChess[index].IsSuperPiece)
            {
                temp.Add(ActivatedChess[index]);
            }
        }

        for (int index = 0; index < temp.Count; index++)
        {
            ReturnChessPiece(temp[index]);
        }
        temp = null;
    }

    public void DeleteAllChessPieces()
    {
        int piecesCount = ActivatedChess.Count;
        for (int count = 0; count < piecesCount; count++)
        {
            Logger.Log($"Delete All Chess Pieces Count : {count}");
            ReturnChessPiece(ActivatedChess[0]);
        }
    }

    private void OnDestroy()
    {
        mAddressableHandles.ReleaseAll();
        //foreach (var handle in mAddressableHandles)
        //{
        //    Addressables.Release(handle);
        //}

    }
}
