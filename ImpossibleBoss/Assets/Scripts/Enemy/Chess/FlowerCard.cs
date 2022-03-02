using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public enum ECardType
{
    Joker,
    Club,
    Diamond,
    Heart,
    Spade
}

public class FlowerCard : MonoBehaviour, IObjectPool<FlowerCard>
{
    private static ChessAI mAI;
    private static Sprite[] mMonthImage;
    private static AddressableHandleManager mAddressableHandles;
    private (int Row, int Col) mCoord;
    private Image mFlowerImage;

    public ChessAI AI { get { return mAI; } set { mAI = value; } }
    public (int Row, int Col) Coord { get { return mCoord; } set { mCoord = value; } }
    private Image FlowerImage { get { return mFlowerImage; } }
    
    public FlowerCard Next { get; set; }
    public Action<FlowerCard> ReturnObjectDelegate { get; set; }

    public void ReturnObject()
    {
        AI.SetFlowerCardExists(Coord.Row, Coord.Col, false);
        gameObject.SetActive(false);
        ReturnObjectDelegate(this);
    }

    private Sprite GetMonthImage(ECardType month)
    {
        return mMonthImage[(int)month];
    }

    public void SetmMonthImage(ECardType month) 
    {
        switch (month)
        {
            case ECardType.Joker: FlowerImage.sprite = GetMonthImage(ECardType.Joker); break;
            case ECardType.Club: FlowerImage.sprite = GetMonthImage(ECardType.Club); break;
            case ECardType.Diamond: FlowerImage.sprite = GetMonthImage(ECardType.Diamond); break;
            case ECardType.Heart: FlowerImage.sprite = GetMonthImage(ECardType.Heart); break;
            case ECardType.Spade: FlowerImage.sprite = GetMonthImage(ECardType.Spade); break;            
            default: throw new System.ArgumentOutOfRangeException(nameof(month));
        }
    }
    private void Awake()
    {
        mFlowerImage = gameObject.GetComponentInChildren<Image>();
        if (AI == null)
        {
            AI = FindObjectOfType<ChessAI>();
        }

        if (mAddressableHandles == null || mAddressableHandles.AddressableHandles.Count > 0)
        {
            mMonthImage = new Sprite[5];
            mAddressableHandles = new AddressableHandleManager(5);

            Addressables.LoadAssetAsync<Sprite>("FlowerCard/Joker").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[(int)ECardType.Joker] = obj.Result; };
            Addressables.LoadAssetAsync<Sprite>("FlowerCard/Club").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[(int)ECardType.Club] = obj.Result; };
            Addressables.LoadAssetAsync<Sprite>("FlowerCard/Diamond").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[(int)ECardType.Diamond] = obj.Result; };
            Addressables.LoadAssetAsync<Sprite>("FlowerCard/Heart").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[(int)ECardType.Heart] = obj.Result; };
            Addressables.LoadAssetAsync<Sprite>("FlowerCard/Spade").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[(int)ECardType.Spade] = obj.Result; };

            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerJan").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[0] = obj.Result; };
            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerFeb").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[1] = obj.Result; };
            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerMar").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[2] = obj.Result; };
            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerApr").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[3] = obj.Result; };
            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerMay").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[4] = obj.Result; };
            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerJun").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[5] = obj.Result; };
            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerJul").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[6] = obj.Result; };
            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerAug").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[7] = obj.Result; };
            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerSep").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[8] = obj.Result; };
            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerOct").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[9] = obj.Result; };
            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerNov").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[10] = obj.Result; };
            //Addressables.LoadAssetAsync<Sprite>("ChessFlowerDec").Completed += (obj) => { mAddressableHandles.Add(obj); mMonthImage[11] = obj.Result; };

            //mMonthImage[0] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/Jan");
            //mMonthImage[1] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/Feb");
            //mMonthImage[2] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/Mar");
            //mMonthImage[3] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/Apr");
            //mMonthImage[4] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/May");
            //mMonthImage[5] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/Jun");
            //mMonthImage[6] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/Jul");
            //mMonthImage[7] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/Aug");
            //mMonthImage[8] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/Sep");
            //mMonthImage[9] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/Oct");
            //mMonthImage[10] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/Nov");
            //mMonthImage[11] = Resources.Load<Sprite>("Enemy/Chess/FlowerCard/Dec");
        }
    }

    private void OnDestroy()
    {
        if (mAddressableHandles.AddressableHandles.Count > 0 && mAddressableHandles.AddressableHandles[0].IsValid())
        {
            mAddressableHandles.ReleaseAll();
        }
        
    }
}
