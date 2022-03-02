using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tornado : MonoBehaviour, IObjectPool<Tornado>
{
    [SerializeField]
    private GameObject m_Card;
    //[SerializeField, Tooltip("지속 시간")]
    private static float mLifeTime = 3.0f;
    private float mElapsedLifeTime = 0.0f;
    //[SerializeField]
    private static float mCardSummonTime = 0.5f;
    private float mElapsedSummonTime;
    private static GameObject mCausedBy;

    private static float mSpeed = 0.05f;
    private static float mDamage;
    private static float mKnockBackSecond;
    private static float mRange = 10.0f;

    private static ObjectPool<Card> mCardPool;
    private static GameObject mCards;

    [SerializeField]
    private List<Image> m_Guides;
    private List<RectTransform> mGuidesTransform;
    private List<(float X, float Z)> mDir;

    public GameObject CausedBy { get { return mCausedBy; } set { mCausedBy = value; } }
    private ObjectPool<Card> CardPool { get { return mCardPool; } }
    public float LifeTime { private get { return mLifeTime; } set { mLifeTime = value; } }
    private float ElapsedLifeTime { get { return mElapsedLifeTime; } set { mElapsedLifeTime = value; } }
    private float ElapsedSummonTime { get { return mElapsedSummonTime; } set { mElapsedSummonTime = value; } }
    public float CardSummonTime { private get { return mCardSummonTime; } set { mCardSummonTime = value; } }
    private GameObject Card { get { return m_Card; } }
    private bool ShouldSetDir { get; set; } = true;

    public static bool ShouldShowGuide { get; set; } = default;
    public float Range { private get { return mRange; } set { mRange = value; } }
    public float Damage { private get { return mDamage; } set { mDamage = value; } }
    public float Speed { private get { return mSpeed; } set { mSpeed = value; } }
    public float KnockBackSecond { private get { return mKnockBackSecond; } set { mKnockBackSecond = value; } }


    public Tornado Next { get; set; }
    public System.Action<Tornado> ReturnObjectDelegate { get; set; }

    private void Awake()
    {
        mDir = new List<(float X, float Z)>(4);
        mGuidesTransform = new List<RectTransform>(4);
        for (int index = 0; index < 4; index++)
        {
            mGuidesTransform.Add(m_Guides[index].rectTransform);
        }
    }

    private void Start()
    {
        if(mCardPool == null)
        {
            mCards = new GameObject("Cards");
            mCardPool = new ObjectPool<Card>(240);

            Quaternion Rot = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            for (int index = 0; index < 240; index++)
            {
                GameObject newCard = Instantiate(Card, transform.position, Rot, mCards.transform);
                newCard.SetActive(false);
                newCard.GetComponent<Card>().ReturnObjectDelegate = CardPool.ReturnObject;
                newCard.GetComponent<Card>().CausedBy = CausedBy;
                newCard.GetComponent<Card>().Damage = Damage;
                newCard.GetComponent<Card>().Range = Range;
                newCard.GetComponent<Card>().Speed = Speed;
                newCard.GetComponent<Card>().KnockBackSecond = KnockBackSecond;
                CardPool.RegisterObject(newCard.GetComponent<Card>());
            }

            //m_LifeTime = CausedBy.GetComponent<ChessBoss>().;
        }        
    }

    private void OnEnable()
    {
        ElapsedLifeTime = 0.0f;
        ElapsedSummonTime = 0.0f;
    }

    private void OnDisable()
    {
        for (int index = 0; index < 4; index++)
        {
            m_Guides[index].gameObject.SetActive(false);            
        }
    }

    public void ReturnObject()
    {
        gameObject.SetActive(false);
        ReturnObjectDelegate(this);
    }

    void Update()
    {
        if (ShouldSetDir)
        {
            SetDir();
            if (ShouldShowGuide)
            {
                ShowGuide();
            }
            
        }

        if (ElapsedSummonTime >= CardSummonTime)
        {
            SummonCard();
            ElapsedSummonTime = 0.0f;   
        }
        else
        {
            ElapsedSummonTime += Time.deltaTime;
        }

        if (ElapsedLifeTime >= LifeTime)
        {
            ReturnObject();   
        }
        else
        {
            ElapsedLifeTime += Time.deltaTime;
        }
    }

    private void ShowGuide()
    {
        for (int index = 0; index < 4; index++)
        {
            m_Guides[index].gameObject.SetActive(true);
            mGuidesTransform[index].rotation = Quaternion.LookRotation(new Vector3(mDir[index].X, 0.0f, mDir[index].Z)) * Quaternion.Euler(90.0f, 0.0f, 0.0f);
        }
        ShouldSetDir = false;
    }

    private void SetDir()
    {
        for (int index = 0; index < 4; index++)
        {            
            mDir.Add((Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)));
        }
    }

    private void SummonCard()
    {        
        for (int index = 0; index < 4; index++)
        {           
            Card newCard = CardPool.GetAvailableObject();
            newCard.gameObject.SetActive(true);
            newCard.OriginPos = transform.position;
            newCard.Dir = new Vector3(mDir[index].X, 0, mDir[index].Z).normalized;
        }
        mDir.Clear();
        ShouldSetDir = true;
    }

    public void DeleteAllCards()
    {
        var cards = CardPool.ActivatedObject.ConvertAll(i => i);
        int maxCount = CardPool.ActivatedObject.Count;
        for (int index = 0; index < maxCount; index++)
        {
            cards[index].ReturnObject();
        }
    }

    private void OnDestroy()
    {
        mCardPool = null;
    }
}
