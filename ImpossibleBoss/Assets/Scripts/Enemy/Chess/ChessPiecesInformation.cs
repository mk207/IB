using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EPieceType
{
    Pawn,
    Rook,
    Bishop,
    Knight,
    King,
    Queen
}
//internal enum EPieceColor
//{
//    Red,
//    Blue,
//    Black,
//    Green,
//    White,
//    Magenta
//}

internal enum EChessDir
{
    Up,
    Down,
    Left,
    Right,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}

public class ChessPiecesInformation : LivingEntity
{
    [VisibleOnly]
    private EPieceType mePieceType;
    private static ChessBoss mBoss;
    private static ChessAI mAI;
    private object mNext;
    private int mX, mZ;
    private Vector3 mPrePos;
    private Vector3 mDestinationPos;
    private float mPosLerp;
    private static STakeDamageParams mDamageParams;
    private MeshRenderer mMeshRenderer;
    private MaterialPropertyBlock mMPB;
    private float mDissolvePower;
    private bool mbShouldMove;
    private bool mbIsSetNextPos;
    private bool mbIsSuperPiece;
    private bool mbIsDying;
    //체스 한칸 사이즈
    private float BOARD_BOX_SIZE = 4;

    private float mDamageInterval = 0.5f;
    private float mRemainingNextDamageTime = 0.0f;

    private static Skill mCurse;

    [SerializeField]
    private RectTransform m_Guide;
    public static bool ShouldShowGuid { get; set; }
    private Vector3 GuideZeroScale { get; set; } = new Vector3(1.0f, 0.0f, 1.0f);
    private Vector3 PreGuideScale { get; set; }

    [SerializeField]
    private RectTransform m_KingGuide;
    [SerializeField]
    private RectTransform m_KingInnerGuide;
    

    private int CurrRow { get; set; }
    private int CurrCol { get; set; }
    private int NextRow { get; set; }
    private int NextCol { get; set; }
    private int PreRow { get; set; }
    private int PreCol { get; set; }    

    public float DamageInterval { get { return mDamageInterval; } set { mDamageInterval = value; } }

    public ChessAI AI { get { return mAI; } set { mAI = value; } }
    public EPieceType PieceType { get { return mePieceType; } private set { mePieceType = value; } }
    public void SetPieceType(EPieceType pieceType, Mesh mesh, Material mat)
    {
        PieceType = pieceType;
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = mat;
        //if (pieceType == EPieceType.Pawn)
        //{
        //    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        //}
        //else
        //{
        //    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        //}

        if (IsSuperPiece)
        {
            switch (PieceType)
            {
                case EPieceType.Pawn: SetMPBColor(Color.cyan); break;
                case EPieceType.Bishop: SetMPBColor(Color.red); break;
                case EPieceType.King: SetMPBColor(Color.blue); SetKingGuide(true); break;
                case EPieceType.Knight: SetMPBColor(Color.green); break;
                case EPieceType.Queen: SetMPBColor(Color.magenta); break;
                case EPieceType.Rook: SetMPBColor(Color.white); break;
            }
        }
        else
        {
            SetMPBColor(Color.white);
        }
    }
    public object Next { get { return mNext; } set { mNext = value; } }
    public bool IsSuperPiece { get { return mbIsSuperPiece; } set { mbIsSuperPiece = value; } }
    private MeshRenderer MeshRenderer { get { return mMeshRenderer; } }
    private MaterialPropertyBlock MPB { get { return mMPB; } }
    private float DissolvePower { get { return mDissolvePower; } set { mDissolvePower = value; } }
    private ChessBoss Boss { get { return mBoss; } }
    private bool ShouldMove { get { return mbShouldMove; } set { mbShouldMove = value; } }
    private bool IsSetNextPos { get { return mbIsSetNextPos; } set { mbIsSetNextPos = value; } }
    private bool IsDying { get { return mbIsDying; } set { mbIsDying = value; } }
    private Vector3 PrePos { get { return mPrePos; } set { mPrePos = value; } }
    private Vector3 DestinationPos { get { return mDestinationPos; } set { mDestinationPos = value; } }
    private float PosLerp { get { return mPosLerp; } set { mPosLerp = value; } }
    private STakeDamageParams DamageParams { get { return mDamageParams; } }
    private float RemainingNextDamageTime { get { return mRemainingNextDamageTime; } set { mRemainingNextDamageTime = value; } }

    private static bool IsDebugMode { get; set; }

    public void SetCoord(int col, int row)
    {
        mZ = row;
        mX = col;
        CurrCol = col;
        CurrRow = row;
    }

    public void GetCoord(out int row, out int col)
    {
        row = mZ;
        col = mX;        
    }

    public void SetMPBColor(Vector4 newColor)
    {
        MPB.SetColor("_Color", newColor);
        MeshRenderer.SetPropertyBlock(MPB);
    }

    public void InitDissolve()
    {
        MPB.SetFloat("_Dp", 1.6f);
        MeshRenderer.SetPropertyBlock(MPB);
    }

    private Skill Curse { get { return mCurse; } }

    protected override void Die()
    {
        if (IsDying == false)
        {
            base.Die();
            IsDying = true;
            DissolvePower = 1.6f;
            GetComponent<BoxCollider>().enabled = false;

            (Curse as ICast).Cast(this);

            //임시
            //Boss.DecreaseResi();

            if (IsSuperPiece)
            {
                AI.ReAttackMarkingSuperPieces();
            }
        }
        
        //오브젝트풀에 반납
        //AI.ReturnChessPiece(this);        
    }

    private void KingAttack()
    {
        Vector3 pos = transform.position;
        float x = pos.x;
        float z = pos.z;
        //4로 계산하면 한칸의 중앙부터 옆칸의 중앙까지므로 2를 더해줘야 옆칸 전체를 검사함.
        float boxSize = BOARD_BOX_SIZE + 2;

        foreach (var playerInfo in AI.GetPlayersPos())
        {
            Vector3 playerPos = playerInfo.Value;
            if ((x - boxSize <= playerPos.x && x + boxSize >= playerPos.x) && (z - boxSize <= playerPos.z && z + boxSize >= playerPos.z))
            {
                playerInfo.Key.TakeDamage(DamageParams);
            }
        }
    }

    private void SetKingGuide(bool isTurnOn)
    {
        if (isTurnOn && IsSuperPiece == false)
        {
            m_KingGuide.gameObject.SetActive(true);
            m_KingInnerGuide.localScale = Vector3.zero;
        }
        else
        {
            m_KingGuide.gameObject.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();
        //슈퍼 체스말은 이동 안함.
        if (ShouldMove && !IsSuperPiece)
        {
            transform.position = Vector3.Lerp(PrePos, DestinationPos, PosLerp);
            m_Guide.localScale = Vector3.Lerp(PreGuideScale, GuideZeroScale, PosLerp);
            if (PieceType == EPieceType.King)
            {
                m_KingInnerGuide.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, PosLerp);
                SetKingGuidePos();
            }
            
            PosLerp = Mathf.Clamp01(PosLerp + Time.deltaTime);
            //Logger.Log("Pieces Updating");
            if (PosLerp >= 1.0f)
            {
                ShouldMove = false;
                SetPositionForNextMove();
                if (PieceType == EPieceType.King)
                {
                    KingAttack();
                }
            }
        }
        else
        {
            PosLerp = 0.0f;
        }

        if (RemainingNextDamageTime > 0.0f)
        {
            RemainingNextDamageTime -= Time.deltaTime;
        }

        if (IsDying)
        {
            DissolvePower -= Time.deltaTime;
            MPB.SetFloat("_Dp", DissolvePower);
            MeshRenderer.SetPropertyBlock(MPB);
            if (DissolvePower <= 0.0f)
            {
                IsDying = false;
                AI.ReturnChessPiece(this);                
            }
        }
    }

    public void AttackMarkingSuperPiece()
    {
        switch (PieceType)
        {
            case EPieceType.Pawn:   AttackMarkingSuperPawn(); break;
            case EPieceType.Knight: AttackMarkingSuperKnight(); break;
            case EPieceType.Rook:   AttackMarkingSuperRook(); break;
            case EPieceType.Bishop: AttackMarkingSuperBishop(); break;
            case EPieceType.King:   AttackMarkingSuperKing(); break;
            case EPieceType.Queen:  AttackMarkingSuperQueen(); break;
            default: throw new System.ArgumentOutOfRangeException(nameof(PieceType));
        }
    }

    private bool TryMarkingDeadZone(int row, int col)
    {
        if ((row >= 0 && row < AI.BoardSize) && (col >= 0 && col < AI.BoardSize))
        {
            Boss.DeadZoneInfo[row * 8 + col].SetDeadZoneByPiece(true);
            return true;
        }
        return false;
    }

    private void AttackMarkingSuperQueen()
    {
        //int CurrRow, CurrCol;
        //int NextRow, NextCol;
       // GetCoord(out CurrRow, out CurrCol);

        //직선 검사
        for (int row = 0; row < AI.BoardSize; row++)
        {
            Boss.DeadZoneInfo[row * 8 + CurrCol].SetDeadZoneByPiece(true);
        }
        for (int col = 0; col < AI.BoardSize; col++)
        {
            Boss.DeadZoneInfo[CurrRow * 8 + col].SetDeadZoneByPiece(true);
        }

        //대각선 검사
        for (int index = 0; index < AI.BoardSize; index++)
        {
            //우상
            NextRow = CurrRow + index;
            NextCol = CurrCol + index;
            TryMarkingDeadZone(NextRow, NextCol);
            //우하
            NextRow = CurrRow + index;
            NextCol = CurrCol - index;
            TryMarkingDeadZone(NextRow, NextCol);
            //좌상
            NextRow = CurrRow - index;
            NextCol = CurrCol + index;
            TryMarkingDeadZone(NextRow, NextCol);
            //좌하
            NextRow = CurrRow - index;
            NextCol = CurrCol - index;
            TryMarkingDeadZone(NextRow, NextCol);
        }

        Boss.DeadZoneInfo[CurrRow * 8 + CurrCol].SetDeadZoneByPiece(true);
    }

    private void AttackMarkingSuperKing()
    {
        //int CurrRow, CurrCol;
        //int NextRow, NextCol;
       // GetCoord(out CurrRow, out CurrCol);

        //직선 검사
        NextRow = CurrRow + 1;
        NextCol = CurrCol;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow - 1;
        NextCol = CurrCol;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow;
        NextCol = CurrCol + 1;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow;
        NextCol = CurrCol - 1;
        TryMarkingDeadZone(NextRow, NextCol);

        //대각선 검사
        NextRow = CurrRow + 1;
        NextCol = CurrCol + 1;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow + 1;
        NextCol = CurrCol - 1;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow - 1;
        NextCol = CurrCol + 1;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow - 1;
        NextCol = CurrCol - 1;
        TryMarkingDeadZone(NextRow, NextCol);

        Boss.DeadZoneInfo[CurrRow * 8 + CurrCol].SetDeadZoneByPiece(true);
    }

    private void AttackMarkingSuperBishop()
    {
        //int CurrRow, CurrCol;
        //int NextRow, NextCol;
       // GetCoord(out CurrRow, out CurrCol);

        for (int index = 0; index < AI.BoardSize; index++)
        {
            NextRow = CurrRow + index;
            NextCol = CurrCol + index;
            TryMarkingDeadZone(NextRow, NextCol);

            NextRow = CurrRow + index;
            NextCol = CurrCol - index;
            TryMarkingDeadZone(NextRow, NextCol);

            NextRow = CurrRow - index;
            NextCol = CurrCol + index;
            TryMarkingDeadZone(NextRow, NextCol);

            NextRow = CurrRow - index;
            NextCol = CurrCol - index;
            TryMarkingDeadZone(NextRow, NextCol);
        }

        Boss.DeadZoneInfo[CurrRow * 8 + CurrCol].SetDeadZoneByPiece(true);
    }

    private void AttackMarkingSuperKnight()
    {
        //int CurrRow, CurrCol;
        //int NextRow, NextCol;
       // GetCoord(out CurrRow, out CurrCol);

        NextRow = CurrRow + 2;
        NextCol = CurrCol + 1;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow + 2;
        NextCol = CurrCol - 1;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow - 2;
        NextCol = CurrCol + 1;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow - 2;
        NextCol = CurrCol + 1;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow + 1;
        NextCol = CurrCol + 2;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow - 1;
        NextCol = CurrCol + 2;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow + 1;
        NextCol = CurrCol - 2;
        TryMarkingDeadZone(NextRow, NextCol);

        NextRow = CurrRow - 1;
        NextCol = CurrCol - 2;
        TryMarkingDeadZone(NextRow, NextCol);

        Boss.DeadZoneInfo[CurrRow * 8 + CurrCol].SetDeadZoneByPiece(true);
    }

    private void AttackMarkingSuperPawn()
    {
        //int NextRow, NextCol;
        //int CurrRow, CurrCol;
        // GetCoord(out CurrRow, out CurrCol);
        ////GetCoord(out NextRow, out NextCol);
        NextRow = CurrRow;
        NextCol = CurrCol;
 
        EChessDir dir;

        while(true)
        {
            dir = (EChessDir)Random.Range(0, 4);
            switch (dir)
            {
                case EChessDir.Up:      NextRow = CurrRow + 1; break;
                case EChessDir.Down:    NextRow = CurrRow - 1; break;
                case EChessDir.Left:    NextCol = CurrCol - 1; break;
                case EChessDir.Right:   NextCol = CurrCol + 1; break;
            }
            if(TryMarkingDeadZone(NextRow, NextCol))
            {
                break;
            }
        }
        Boss.DeadZoneInfo[CurrRow * 8 + CurrCol].SetDeadZoneByPiece(true);
    }

    private void AttackMarkingSuperRook()
    {
        //int CurrRow, CurrCol;
       // GetCoord(out CurrRow, out CurrCol);

        for (int row = 0; row < AI.BoardSize; row++)
        {
            Boss.DeadZoneInfo[row * 8 + CurrCol].SetDeadZoneByPiece(true);           
        }
        for (int col = 0; col < AI.BoardSize; col++)
        {
            Boss.DeadZoneInfo[CurrRow * 8 + col].SetDeadZoneByPiece(true);
        }

        Boss.DeadZoneInfo[CurrRow * 8 + CurrCol].SetDeadZoneByPiece(true);
    }

    public void Move()
    {

        //bool IsValidMove;

        //테스트
        //ShouldMove = PawnMove();        
        //ShouldMove = RookMove();
        //ShouldMove = BishopMove();
        //ShouldMove = QueenMove();
        //ShouldMove = KnightMove();

        //슈퍼 체스말은 이동안함.
        if (IsSuperPiece)
        {
            return;
        }

        //if (ShouldMove)
        if (IsSetNextPos)
        {
            PrePos = transform.position;
            int col, row;
            GetCoord(out row, out col);
            DestinationPos = new Vector3(col * BOARD_BOX_SIZE, 0, row * BOARD_BOX_SIZE);
            IsSetNextPos = false;
            ShouldMove = true;
        }
    }

    public void SetPositionForNextMove()
    {
        switch (PieceType)
        {
            case EPieceType.Pawn: IsSetNextPos = PawnMove(); break;
            case EPieceType.Knight: IsSetNextPos = KnightMove(); break;
            case EPieceType.Rook: IsSetNextPos = RookMove(); break;
            case EPieceType.Bishop: IsSetNextPos = BishopMove(); break;
            case EPieceType.King: IsSetNextPos = KingMove(); break;
            case EPieceType.Queen: IsSetNextPos = QueenMove(); break;
            default: throw new System.ArgumentOutOfRangeException(nameof(PieceType));
        }
        if (ShouldShowGuid)
        {
            SetGuide();
        }
        
    }

    private void SetGuide()
    {
        float dRow = PreRow - NextRow;
        float dCol = PreCol - NextCol;
        float dis = Mathf.Sqrt(dCol * dCol + dRow * dRow);
        

        float rot = 0.0f;

        if (dRow == 0 || dCol == 0)
        {
            //dRow 또는 dCol이 0일 경우 십자이동뿐임
            if (dRow == 0)
            {
                if (dCol < 0)
                {
                    rot = -90.0f;
                }
                else
                {
                    rot = 90.0f;
                }
            }
            else
            {
                if (dRow < 0)
                {
                    rot = 0.0f;
                }
                else
                {
                    rot = 180.0f;
                }
            }
        }
        else
        {
            rot = Mathf.Atan(dCol / dRow) * Mathf.Rad2Deg;
            if (dRow < 0.0f)
            {
                //이동 방향이 위쪽이면 -1곱하기
                rot *= -1.0f;
            }
            else
            {
                //아래쪽이면 뒤집어줌
                rot = 180.0f - rot;
            }
        }    

        dis *= 0.4f;
        PreGuideScale = new Vector3(1.0f, dis, 1.0f);
        m_Guide.localScale = PreGuideScale;

        m_Guide.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, rot));
    }

    protected override void Awake()
    {
        base.Awake();
        if (mDamageParams.causedBy == null)
        {
            mBoss = FindObjectOfType<ChessBoss>();
            if (Boss.m_bDebugMode)
            {
                mDamageParams.causedBy = gameObject;
            }
            else
            {                
                mDamageParams.causedBy = Boss.gameObject;
            }

            mDamageParams.damageAmount = Boss.PiecesDamage;
            mDamageParams.cc = ECC.KnockBack;
            mDamageParams.ccAmount = Boss.PiecesKnockBackPower;
        }

        if (AI == null)
        {
            mAI = FindObjectOfType<ChessAI>();
        }

        mMeshRenderer = GetComponent<MeshRenderer>();
        mMPB = new MaterialPropertyBlock();

        if (mCurse == null)
        {
            //mCurse = new CurseOfChessPiece();
            mCurse = CurseOfChessPiece.CreateInstance("CurseOfChessPiece") as Skill;
            mCurse.Init();
            mCurse.InitIcon();
        }
    }

    protected override void Start()
    {
        base.Start();

        SetHealth(InitHealth);
        BOARD_BOX_SIZE = AI.GetBOXSIZE;
        Physics.IgnoreCollision(GetComponentInChildren<Collider>(), AI.m_ChessPrefab.GetComponentInChildren<Collider>());
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        IsDead = false;
        GetComponent<BoxCollider>().enabled = true;
        RemainingNextDamageTime = DamageInterval;
        if (ShouldShowGuid)
        {
            m_Guide.gameObject.SetActive(true);
            if (PieceType == EPieceType.King)
            {
                m_KingGuide.gameObject.SetActive(true);
            }
        }
        else
        {
            m_Guide.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        base.OnDisable();
        SetKingGuide(false);        
        IsDying = false;
    }

    private bool IsValidCoord(int row, int col)
    {
        if ((row >= 0 && row < AI.BoardSize) && (col >= 0 && col < AI.BoardSize))
        {
            if (AI.IsPieceExistent(row, col))
            {
                return false;
            }
            else
            {
                AI.SetIsPieceExistent(row, col, true);
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    private void SetPrePos()
    {
        PreRow = CurrRow;
        PreCol = CurrCol;
    }

    private bool PawnMove()
    {
        //int NextCol, NextRow;
        //int CurrCol, CurrRow;
        //GetCoord(out NextRow, out NextCol);
        // GetCoord(out CurrRow, out CurrCol);
        SetPrePos();
        NextRow = CurrRow;
        NextCol = CurrCol;
        int pathFindCount = 0;

        do
        {
            pathFindCount++;
            NextRow = CurrRow;
            NextCol = CurrCol;
            EChessDir dir = (EChessDir)Random.Range(0, 8);

            switch (dir)
            {
                case EChessDir.Up:      NextRow += 1; break;
                case EChessDir.Down:    NextRow -= 1; break;
                case EChessDir.Left:    NextCol -= 1;  break;
                case EChessDir.Right:   NextCol += 1;  break;
                default: continue;
            }
        } while (IsValidCoord(NextRow, NextCol) == false && pathFindCount <= 50);

        if (pathFindCount <= 50)
        {
            AI.SetIsPieceExistent(CurrRow, CurrCol, false);
            SetCoord(NextCol, NextRow);
        }
        return true;
    }

    private bool KnightMove()
    {

        //int NextCol, NextRow;
        //int CurrCol, CurrRow;
        int pathFindCount = 0;
        // GetCoord(out CurrRow, out CurrCol);
        SetPrePos();
        do
        {
            pathFindCount++;
            //GetCoord(out NextRow, out NextCol);
            NextRow = CurrRow;
            NextCol = CurrCol;
            //나이트는 enum대로 할 수 없으니 Up down left right에 함
            EChessDir dir = (EChessDir)Random.Range(0, 8);

            switch (dir)
            {
                //UP: Right Up
                case EChessDir.Up: NextCol += 1; NextRow += 2; break;
                //Down: Right Down
                case EChessDir.Down: NextCol -= 1; NextRow += 2; break;
                //Left: Left Up
                case EChessDir.Left: NextCol += 1; NextRow -= 2; break;
                //Right: Left Down
                case EChessDir.Right: NextCol -= 1; NextRow -= 2; break;
                case EChessDir.UpLeft: NextRow -= 1; NextCol += 2; break;
                case EChessDir.UpRight: NextRow += 1; NextCol += 2; break;
                case EChessDir.DownLeft: NextRow -= 1; NextCol -= 2; break;
                case EChessDir.DownRight: NextRow += 1; NextCol -= 2; break;
            }
        } while (IsValidCoord(NextRow, NextCol) == false && pathFindCount <= 50);

        if (pathFindCount <= 50)
        {
            AI.SetIsPieceExistent(CurrRow, CurrCol, false);
            SetCoord(NextCol, NextRow);
        }
        return true;
    }

    private bool KingMove()
    {
        //int NextCol, NextRow;
        //int CurrCol, CurrRow;
        int Distance;
        int pathFindCount = 0;
        // GetCoord(out CurrRow, out CurrCol);
        SetPrePos();
        do
        {
            pathFindCount++;
            //GetCoord(out NextRow, out NextCol);
            NextRow = CurrRow;
            NextCol = CurrCol;
            Distance = 1;
            EChessDir dir = (EChessDir)Random.Range(0, 8);

            switch (dir)
            {
                case EChessDir.Up: NextRow += Distance; break;
                case EChessDir.Down: NextRow -= Distance; break;
                case EChessDir.Left: NextCol -= Distance; break;
                case EChessDir.Right: NextCol += Distance; break;
                case EChessDir.UpLeft: NextRow += Distance; NextCol -= Distance; break;
                case EChessDir.UpRight: NextRow += Distance; NextCol += Distance; break;
                case EChessDir.DownLeft: NextRow -= Distance; NextCol -= Distance; break;
                case EChessDir.DownRight: NextRow -= Distance; NextCol += Distance; break;
            }
        } while (IsValidCoord(NextRow, NextCol) == false && pathFindCount <= 50);

        if (pathFindCount <= 50)
        {
            AI.SetIsPieceExistent(CurrRow, CurrCol, false);
            SetCoord(NextCol, NextRow);
        }

        SetKingGuidePos();
        m_KingInnerGuide.localScale = Vector3.zero;
        return true;
    }

    private void SetKingGuidePos()
    {
        m_KingGuide.position = new Vector3(NextCol * BOARD_BOX_SIZE, 0.1f, NextRow * BOARD_BOX_SIZE);
    }

    private bool QueenMove()
    {
        //int NextCol, NextRow;
        //int CurrCol, CurrRow;
        int Distance;
        int pathFindCount = 0;
        // GetCoord(out CurrRow, out CurrCol);
        SetPrePos();
        do
        {
            pathFindCount++;
            //GetCoord(out NextRow, out NextCol);
            NextRow = CurrRow;
            NextCol = CurrCol;
            Distance = Random.Range(0, AI.BoardSize);
            EChessDir dir = (EChessDir)Random.Range(0, 8);

            switch (dir)
            {
                case EChessDir.Up: NextRow += Distance; break;
                case EChessDir.Down: NextRow -= Distance; break;
                case EChessDir.Left: NextCol -= Distance; break;
                case EChessDir.Right: NextCol += Distance; break;
                case EChessDir.UpLeft: NextRow += Distance; NextCol -= Distance; break;
                case EChessDir.UpRight: NextRow += Distance; NextCol += Distance; break;
                case EChessDir.DownLeft: NextRow -= Distance; NextCol -= Distance; break;
                case EChessDir.DownRight: NextRow -= Distance; NextCol += Distance; break;
            }
        } while (IsValidCoord(NextRow, NextCol) == false && pathFindCount <= 50);

        if (pathFindCount <= 50)
        {
            AI.SetIsPieceExistent(CurrRow, CurrCol, false);
            SetCoord(NextCol, NextRow);
        }
        return true;
    }

    private bool BishopMove()
    {
        //int NextCol, NextRow;
        //int CurrCol, CurrRow;
        int Distance;
        int pathFindCount = 0;
        // GetCoord(out CurrRow, out CurrCol);
        SetPrePos();
        do
        {
            pathFindCount++;
            //GetCoord(out NextRow, out NextCol);
            NextRow = CurrRow;
            NextCol = CurrCol;
            Distance = Random.Range(0, AI.BoardSize);
            EChessDir dir = (EChessDir)Random.Range(4, 8);

            switch (dir)
            {
                case EChessDir.UpLeft: NextRow += Distance; NextCol -= Distance; break;
                case EChessDir.UpRight: NextRow += Distance; NextCol += Distance; break;
                case EChessDir.DownLeft: NextRow -= Distance; NextCol -= Distance; break;
                case EChessDir.DownRight: NextRow -= Distance; NextCol += Distance; break;
            }
        } while (IsValidCoord(NextRow, NextCol) == false && pathFindCount <= 50);

        if (pathFindCount <= 50)
        {
            AI.SetIsPieceExistent(CurrRow, CurrCol, false);
            SetCoord(NextCol, NextRow);
        }
        return true;
    }

    private bool RookMove()
    {
        //int NextCol, NextRow;
        //int CurrCol, CurrRow;
        int Distance;
        int pathFindCount = 0;
        // GetCoord(out CurrRow, out CurrCol);
        SetPrePos();
        do
        {
            pathFindCount++;
            //GetCoord(out NextRow, out NextCol);
            NextRow = CurrRow;
            NextCol = CurrCol;
            Distance = Random.Range(0, AI.BoardSize);
            EChessDir dir = (EChessDir)Random.Range(0, 4);

            switch (dir)
            {
                case EChessDir.Up: NextRow += Distance; break;
                case EChessDir.Down: NextRow -= Distance; break;
                case EChessDir.Left: NextCol -= Distance; break;
                case EChessDir.Right: NextCol += Distance; break;
            }
        } while (IsValidCoord(NextRow, NextCol) == false && pathFindCount <= 50);

        if (pathFindCount <= 50)
        {
            AI.SetIsPieceExistent(CurrRow, CurrCol, false);
            SetCoord(NextCol, NextRow);
        }
        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (RemainingNextDamageTime <= 0.0f && collision.gameObject.tag == "Player")
        {
            if (Boss.m_bDebugMode)
            {
                STakeDamageParams param = DamageParams;
                param.causedBy = gameObject;
                collision.gameObject.GetComponent<PlayerInformation>().TakeDamage(param);
                RemainingNextDamageTime = DamageInterval;
            }
            else
            {
                collision.gameObject.GetComponent<PlayerInformation>().TakeDamage(DamageParams);
                RemainingNextDamageTime = DamageInterval;
            }
        }
    }
}
