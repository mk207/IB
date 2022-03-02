using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EAudio
{
    BGM,
    Announcer
}

public class GlobalSound : MonoBehaviour
{
    public AudioClip m_Start;
    public AudioClip m_Alarm;
    public AudioClip m_Victory;
    public AudioClip m_Defeat;

    private AudioClip mBossStage;

    private AudioSource mAudioSource;
    private AudioSource mBGMSource;

    [SerializeField]
    private List<AudioClip> m_BGMList;

    private Dictionary<string, AudioClip> mBGMList;

    private OptionManager mOM;

    public AudioClip BossStage { get { return mBossStage; } set { mBossStage = value; } }
    public AudioSource BGM { get { return mBGMSource; } }
    private AudioSource Audio { get { return mAudioSource; } }
    private Dictionary<string, AudioClip> BGMList { get { return mBGMList; } }
    private OptionManager OM { get { return mOM; } }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        mAudioSource = AddAudio(false, false, 1.0f);
        mBGMSource = AddAudio(true, true, 1.0f);
        mBGMList = new Dictionary<string, AudioClip>(10);
        foreach (var bgm in m_BGMList)
        {
            string[] name = bgm.name.Split('_');
            BGMList.Add(name[1], bgm);
        }
    }

    private void Start()
    {
        mOM = FindObjectOfType<OptionManager>();
        ChangeVolume(EVolumeType.Master);
        //SetVolume(EAudio.BGM, 0.05f);
        ChangeBGM(BGMList["Opening"]);        
    }

    public void SetVolume(EAudio kind, float volume)
    {
        switch (kind)
        {
            case EAudio.BGM: BGM.volume = volume; break;
            case EAudio.Announcer: Audio.volume = volume; break;
            default: throw new System.ArgumentOutOfRangeException(nameof(kind));
        }
    }

    public void ChangeVolume(EVolumeType volumeType)
    {
        switch (volumeType)
        {
            case EVolumeType.Master:    ChangeVolume(EVolumeType.BGM); ChangeVolume(EVolumeType.Announcer); break;
            case EVolumeType.BGM:       BGM.volume = OM.MasterVolume * OM.BGMVolume; break;
            case EVolumeType.Announcer: Audio.volume = OM.MasterVolume * OM.AnnouncerVolume; break;
            default: throw new System.ArgumentOutOfRangeException(nameof(volumeType));
        }
    }

    public void UnPauseAnnouncer()
    {
        Audio.UnPause();
    }

    public void PauseAnnouncer()
    {
        Audio.Pause();
    }

    public AudioSource AddAudio(bool loop, bool playAwake, float vol)
    {
        AudioSource newAudio = gameObject.AddComponent<AudioSource>();
        newAudio.loop = loop;
        newAudio.playOnAwake = playAwake;
        newAudio.volume = vol;
        return newAudio;
    }

    public void StartGame()
    {
        if (Audio.isPlaying == false)
        {
            Audio.PlayOneShot(m_Start);
        }        
    }
    public void Victory()
    {
        Audio.PlayOneShot(m_Victory);
    }
    public void Defeat()
    {
        Audio.PlayOneShot(m_Defeat);
    }

    public void OnAlarm(AudioClip clip/*, int repeatCount*/)
    {
        //Audio.clip = clip;
        //Audio.Play();
        if (Audio.isPlaying == false)
        {
            Audio.PlayOneShot(clip);
            Audio.PlayOneShot(m_Alarm);
        }        
    }

    public void ChangeBGM(string BGMName)
    {
        BGM.clip = BGMList[BGMName];
        BGM.Play();
    }

    public void ChangeBGM(AudioClip newBGM)
    {
        BGM.clip = newBGM;
        BGM.Play();
    }
}
