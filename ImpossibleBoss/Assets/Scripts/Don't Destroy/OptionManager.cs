using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public enum ELanguage
{
    Kor = 1,
    Eng
}

public class OptionManager : MonoBehaviour
{
    //[SerializeField]
    //private OptionData m_OptionData;

    public event System.Action OnChangeLanguage;

    private float mMasterVolume;
    private float mBGMVolume;
    private float mAnnouncerVolume;
    private ELanguage mLanguage;
    //private GlobalSound mGS;

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string ANNOUNCER_VOLUME_KEY = "AnnouncerVolume";
    private const string LANGUAGE_KEY = "Language";

    public float MasterVolume { get { return mMasterVolume; } set { mMasterVolume = value * 0.01f; PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, mMasterVolume); /*mGS.ChangeVolume(EVolumeType.Master);*/ } }
    public float BGMVolume { get { return mBGMVolume; } set { mBGMVolume = value * 0.01f; PlayerPrefs.SetFloat(BGM_VOLUME_KEY, mBGMVolume); /*mGS.ChangeVolume(EVolumeType.BGM);*/ } }
    public float AnnouncerVolume { get { return mAnnouncerVolume; } set { mAnnouncerVolume = value * 0.01f; PlayerPrefs.SetFloat(ANNOUNCER_VOLUME_KEY, mAnnouncerVolume); /*mGS.ChangeVolume(EVolumeType.Announcer);*/ } }
    public ELanguage Language { get { return mLanguage; } set { mLanguage = value; PlayerPrefs.SetInt(LANGUAGE_KEY, (int)mLanguage); } }

    private bool[] mbIsSmartCasting = new bool[6];

    public void SetSmartCasting(bool state, int index)
    {
        mbIsSmartCasting[index] = state;
    }
    public bool IsSmartCastingByIndex(int index) 
    {
        return mbIsSmartCasting[index];
    }
    public bool[] IsSmartCasting { get { return mbIsSmartCasting; } }

    public void ChangeLanguage()
    {
        if(OnChangeLanguage != null)
        {
            OnChangeLanguage.Invoke();
        }
        
    }

    private void Readied()
    {
        Logger.Log("OM Ready");        
        FindObjectOfType<Test_NextScene>().OM = true;
    }

    private void Save()
    {
        //string dataPath = Application.persistentDataPath;

        //var serializer = new XmlSerializer(typeof(OptionData));
        //var stream = new FileStream(dataPath + "/" + SaveName + ".save", FileMode.Create);
        //serializer.Serialize(stream, m_OptionData);
        //stream.Close();

        //Logger.Log("Option Saved");
    }

    private void Load()
    {
        Language = (ELanguage)PlayerPrefs.GetInt(LANGUAGE_KEY, 1);
        mMasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1.0f);
        mBGMVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1.0f);
        mAnnouncerVolume = PlayerPrefs.GetFloat(ANNOUNCER_VOLUME_KEY, 1.0f);

        //string dataPath = Application.persistentDataPath;

        //if (System.IO.File.Exists(dataPath + "/Default.save"))
        //{
        //    var serializer = new XmlSerializer(typeof(OptionData));
        //    var stream = new FileStream(dataPath + "/Default.save", FileMode.Open);
        //    m_OptionData = serializer.Deserialize(stream) as OptionData;
        //    stream.Close();

        //    Logger.Log("Option Loaded");
        //}
        //else
        //{
        //    m_OptionData = new OptionData();
        //    SaveName = "Default";
        //    Language = ELanguage.Kor;
        //    MasterVolume = 100.0f;
        //    BGMVolume = 100.0f;
        //    AnnouncerVolume = 100.0f;

        //    Logger.Log("Make First Option Data");
        //    Save();            
        //}
    }

    private void DeleteSaveData()
    {
        //string dataPath = Application.persistentDataPath;

        //if (System.IO.File.Exists(dataPath + "/" + SaveName + ".save"))
        //{
        //    File.Delete(dataPath + "/" + SaveName + ".save");

        //    Logger.Log("Option Deleted");
        //}
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Load();

        //Language = (ELanguage)PlayerPrefs.GetInt(LANGUAGE_KEY, 0);
        //MasterVolume = PlayerPrefs.GetInt(MASTER_VOLUME_KEY, 1);
        //BGMVolume = PlayerPrefs.GetInt(BGM_VOLUME_KEY, 1);
        //AnnouncerVolume = PlayerPrefs.GetInt(ANNOUNCER_VOLUME_KEY, 1);

        //옵션데이타는 로컬에 저장했다가 읽어들이는 작업 해야함
        //MasterVolume = 100.0f;
        //BGMVolume = 100.0f;
        //Language = ELanguage.Kor;
        enabled = false;
        Readied();
    }

    //private void Start()
    //{
    //    //mGS = FindObjectOfType<GlobalSound>();
    //    Load();
    //}
}

//[System.Serializable]
//public class OptionData
//{
//    public string m_SaveName;
//    public ELanguage m_Language;
//    public float m_MasterVolume;
//    public float m_BGMVolume;
//    public float m_AnnouncerVolume;
//    //private float mEffectVolume;
//}