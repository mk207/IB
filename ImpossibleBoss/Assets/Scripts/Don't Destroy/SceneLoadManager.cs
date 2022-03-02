using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadManager : MonoSingleton<SceneLoadManager>
{
    [SerializeField]
    private CanvasGroup m_FadeCG;
    [SerializeField, Range(0.5f, 2.0f)]
    private float m_FadeDuration = 1.0f;
    [SerializeField]
    private AnimationCurve m_FadeCurve;
    private float mFadeTime = 0.0f;

    private RawImage mPreImage;
    private Texture2D mPreTexture;

    private AsyncOperation mLoadSceneHandle;

    private const string TRANSITION_SCENE = "TransitionScene";

    private GlobalSound mBGM;

    private bool mbIsInTransit = false;

    private AsyncOperation LoadSceneHandle { get { return mLoadSceneHandle; } set { mLoadSceneHandle = value; } }
    private CanvasGroup FadeCG { get { return m_FadeCG; } set { m_FadeCG = value; } }
    private float FadeDuration { get { return m_FadeDuration; } }
    private AnimationCurve FadeCurve { get { return m_FadeCurve; } }
    private float FadeTime { get { return mFadeTime; } set { mFadeTime = value; } }
    private Dictionary<string, LoadSceneMode> mLoadScenes = new Dictionary<string, LoadSceneMode>();
    private RawImage PreImage { get { return mPreImage; } set { mPreImage = value; } }
    private Texture2D PreTexture { get { return mPreTexture; } set { mPreTexture = value; } }
    private GlobalSound BGM { get { return mBGM; } set { mBGM = value; } }
    private bool IsInTransit { get { return mbIsInTransit; } set { mbIsInTransit = value; } }

    private Dictionary<string, LoadSceneMode> LoadScenes { get { return mLoadScenes; } set { mLoadScenes = value; } }
    private void ReadyScene(string[] sceneNames)
    {
        foreach (string sceneName in sceneNames)
        {
            LoadScenes.Add(sceneName, LoadSceneMode.Additive);
        }
    }

    private void ReadyScene(string sceneName)
    {
        LoadScenes.Add(sceneName, LoadSceneMode.Additive);
    }

    private IEnumerator CaptureScene()
    {

        PreTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        yield return new WaitForEndOfFrame();

        PreTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
        PreTexture.Apply();
    }

    private void SetPreImage()
    {
        PreImage.texture = PreTexture;
    }

    private IEnumerator Load()
    {
        foreach (var loadScene in LoadScenes)
        {
            yield return StartCoroutine(LoadScene(loadScene.Key, loadScene.Value));
        }

        StartCoroutine(Fade());
    }

    private IEnumerator LoadScene(string sceneName, LoadSceneMode mode)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, mode);

        Scene loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(loadedScene);
    }

    private IEnumerator Fade()
    {
        //foreach (var loadScene in LoadScenes)
        //{
        //    SceneManager.SetActiveScene(SceneManager.GetSceneByName(loadScene.Key));
        //}

        FadeCG.blocksRaycasts = true;

        

        while (FadeTime <= 1.0f)
        {
            FadeTime += Time.unscaledDeltaTime / FadeDuration;
            FadeCG.alpha = FadeCurve.Evaluate(FadeTime);
            if (FadeCG.alpha >= 0.99f)
            {
                PreImage.gameObject.SetActive(false);
            }
            //Logger.Log($"A : {FadeCG.alpha}");
            yield return null;
        }
        //Logger.Log($"Finish Fade");
        FadeCG.blocksRaycasts = false;
        
        SceneManager.UnloadSceneAsync(TRANSITION_SCENE);
        LoadScenes.Clear();
        IsInTransit = false;
        PreTexture = null;
        PreImage = null;
        FadeCG = null;
    }

    public void LoadMain(bool shouldAutoActive)
    {
        if (IsInTransit == false)
        {
            IsInTransit = true;
            LoadSceneHandle = SceneManager.LoadSceneAsync(TRANSITION_SCENE, LoadSceneMode.Additive);
            LoadSceneHandle.completed += _ =>
            {
                FadeTime = 0.0f;
                FadeCG = FindObjectOfType<CanvasGroup>();

                Scene s = SceneManager.GetSceneByName(TRANSITION_SCENE);
                GameObject[] gos = s.GetRootGameObjects();
                PreImage = gos[0].GetComponentInChildren<RawImage>();

                FadeCG.alpha = 0.0f;
                SetPreImage();
                PreImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                ReadyScene("Scene_Main");
                StartCoroutine(Load());
                BGM.ChangeBGM("Main");
            };
            StartCoroutine(CaptureScene());
            LoadSceneHandle.allowSceneActivation = shouldAutoActive;
        }       
    }

    public void TurnOnScene()
    {       
        LoadSceneHandle.allowSceneActivation = true;
    }

    public void LoadScene(Scene currScene, string sceneName)
    {
        
        if (IsInTransit == false)
        {
            IsInTransit = true;


            LoadSceneHandle = SceneManager.LoadSceneAsync(TRANSITION_SCENE, LoadSceneMode.Additive);
            LoadSceneHandle.completed += _ =>
            {
                StartCoroutine(Unload(currScene));

                FadeTime = 0.0f;
                FadeCG = FindObjectOfType<CanvasGroup>();

                Scene s = SceneManager.GetSceneByName(TRANSITION_SCENE);
                GameObject[] gos = s.GetRootGameObjects();
                PreImage = gos[0].GetComponentInChildren<RawImage>();

                FadeCG.alpha = 0.0f;
                SetPreImage();
                PreImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                ReadyScene(sceneName);
                StartCoroutine(Load());

                BGM.ChangeBGM(sceneName.Split('_')[1]);
            };
            StartCoroutine(CaptureScene());
            LoadSceneHandle.allowSceneActivation = true;
        }
    }

    private IEnumerator Unload(Scene scene)
    {
        while (SceneManager.GetSceneByName(TRANSITION_SCENE).isLoaded == false)
        {            
            yield return null;
        }
        SceneManager.UnloadSceneAsync(scene);
    }


    private void Start()
    {
        BGM = FindObjectOfType<GlobalSound>();
    }

    protected override void Awake()
    {
        base.Awake();        
        DontDestroyOnLoad(gameObject);
    }
}
