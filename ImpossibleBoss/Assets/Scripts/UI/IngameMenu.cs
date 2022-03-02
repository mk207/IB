using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum EIngameMenuState
{
    Victory,
    Defeat,
    Menu,
    Option,
    Close
}

public enum EVolumeType
{
    Master,
    BGM,
    Announcer,
    All
}

public enum EInnerMenuType
{
    Volume,
    Key
}

public class IngameMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject m_MenuPanel;
    [SerializeField]
    private GameObject m_VictoryPanel;
    [SerializeField]
    private GameObject m_DefeatPanel;
    [SerializeField]
    private GameObject m_OptionPanel;
    [SerializeField]
    private Text m_MasterVolumeText;
    [SerializeField]
    private Text m_BGMVolumeText;
    [SerializeField]
    private Text m_AnnouncerVolumeText;
    [SerializeField]
    private Slider m_MasterVolumeSli;
    [SerializeField]
    private Slider m_BGMVolumeSli;
    [SerializeField]
    private Slider m_AnnouncerVolumeSli;
    [SerializeField]
    private GameObject m_pnlVolume;
    [SerializeField]
    private GameObject m_pnlKey;
    private OptionManager mOM;
    private GlobalSound mGS;
    private EIngameMenuState mState;

    private EIngameMenuState State { get { return mState; } set { mState = value; } }
    private GameObject MenuPanel { get { return m_MenuPanel; } }
    private GameObject VictoryPanel { get { return m_VictoryPanel; } }
    private GameObject DefeatPanel { get { return m_DefeatPanel; } }
    private GameObject OptionPanel { get { return m_OptionPanel; } }
    private Text MasterVolumeText { get { return m_MasterVolumeText; } }
    private Text BGMVolumeText { get { return m_BGMVolumeText; } }
    private Text AnnouncerVolumeText { get { return m_AnnouncerVolumeText; } }
    private Slider MasterVolumeSli { get { return m_MasterVolumeSli; } }
    private Slider BGMVolumeSli { get { return m_BGMVolumeSli; } }
    private Slider AnnouncerVolumeSli { get { return m_AnnouncerVolumeSli; } }
    private GameObject VolumePanel { get { return m_pnlVolume; } }
    private GameObject KeyPanel { get { return m_pnlKey; } }
    private OptionManager OM { get { return mOM; } }
    private GlobalSound GS { get { return mGS; } }
    private void Awake()
    {
        mState = EIngameMenuState.Close;
        mOM = FindObjectOfType<OptionManager>();
        mGS = FindObjectOfType<GlobalSound>();

        //MasterVolumeSli.value = OM.MasterVolume;
        //BGMVolumeSli.value = OM.BGMVolume;
        //AnnouncerVolumeSli.value = OM.AnnouncerVolume;
        //FindObjectOfType<GameManager>().
    }

    private void SetState(EIngameMenuState newState)
    {
        MenuPanel.SetActive(false);
        VictoryPanel.SetActive(false);
        DefeatPanel.SetActive(false);
        OptionPanel.SetActive(false);
        gameObject.SetActive(true);

        switch (newState)
        {
            case EIngameMenuState.Victory:    VictoryPanel.SetActive(true); break;
            case EIngameMenuState.Defeat:     DefeatPanel.SetActive(true);  break;
            case EIngameMenuState.Option:     OptionPanel.SetActive(true);  break;
            case EIngameMenuState.Menu:       MenuPanel.SetActive(true);    break;
            case EIngameMenuState.Close:      gameObject.SetActive(false);  break;
            default: throw new System.ArgumentOutOfRangeException(nameof(newState));
        }
    }

    public void Resume()
    {
        GameManager.Instance.Resume();
        SetState(EIngameMenuState.Close);
        GameManager.Instance.IsInMenu = false;
        GS.UnPauseAnnouncer();
    }

    public void ShowVictory()
    {
        SetState(EIngameMenuState.Victory);
    }
    public void ShowDefeat()
    {
        SetState(EIngameMenuState.Defeat);
    }

    public void ShowMenu()
    {
        SetState(EIngameMenuState.Menu);
        GameManager.Instance.Pause();
        GameManager.Instance.IsInMenu = true;
        GS.PauseAnnouncer();
    }

    public void ShowOption()
    {
        LoadOption();
        SetState(EIngameMenuState.Option);     
    }

    private void LoadOption()
    {
        MasterVolumeSli.value = OM.MasterVolume * 100.0f;
        BGMVolumeSli.value = OM.BGMVolume * 100.0f;
        AnnouncerVolumeSli.value = OM.AnnouncerVolume * 100.0f;
        OnChangeVolumeText(3);
    }

    public void OnChangeVolumeText(int volumeType)
    {
        switch ((EVolumeType)volumeType)
        {
            case EVolumeType.Master:    MasterVolumeText.text = MasterVolumeSli.value.ToString();       OM.MasterVolume = MasterVolumeSli.value;       GS.ChangeVolume(EVolumeType.Master); break;
            case EVolumeType.BGM:       BGMVolumeText.text = BGMVolumeSli.value.ToString();             OM.BGMVolume = BGMVolumeSli.value;             GS.ChangeVolume(EVolumeType.BGM); break;
            case EVolumeType.Announcer: AnnouncerVolumeText.text = AnnouncerVolumeSli.value.ToString(); OM.AnnouncerVolume = AnnouncerVolumeSli.value; GS.ChangeVolume(EVolumeType.Announcer); break;
            case EVolumeType.All: OnChangeVolumeText(0); OnChangeVolumeText(1); OnChangeVolumeText(2); break;
            default: throw new ArgumentOutOfRangeException(nameof(volumeType));
        }
    }

    public void GoToMainScene()
    {
        //DPS 저장후 메인 메뉴화면 로드
        SceneLoadManager.Instance.LoadScene(gameObject.scene, "Scene_Main");
    }

    public void Restart()
    {
        //SceneManager.LoadScene(gameObject.scene.name);
        SceneLoadManager.Instance.LoadScene(gameObject.scene, gameObject.scene.name);
    }

    public void ShowInnerMenu(int menuType)
    {
        VolumePanel.SetActive(false);
        KeyPanel.SetActive(false);
        switch ((EInnerMenuType)menuType)
        {
            case EInnerMenuType.Volume:  VolumePanel.SetActive(true); break;
            case EInnerMenuType.Key:     KeyPanel.SetActive(true); break;
            default: throw new ArgumentOutOfRangeException(nameof(menuType));
        }
    }
}
