using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Test_NextScene : MonoBehaviour
{
    //Init Skill
    public bool IS { get; set; } = default;
    //Option Manager
    public bool OM { get; set; } = default;
    //Player Data
    public bool PD { get; set; } = default;
    //Global Skill List
    public bool GSL { get; set; } = default;
    //Addressable Data
    public bool AD { get; set; } = default;

    private Scene TransitionScene { get; set; }

    private bool IsAllReady()
    {
        return IS && OM && GSL && PD && AD;
    }

    private IEnumerator LoadScene()
    {
        TransitionScene = SceneManager.GetSceneByName("TransitionScene");
        
        while (true)
        {
            //Logger.Log(string.Format("progress {0}", Keyboard.current.anyKey.wasPressedThisFrame));
            if (IsAllReady() && Keyboard.current.anyKey.wasPressedThisFrame && TransitionScene.IsValid())
            {
                SceneLoadManager.Instance.TurnOnScene();   
            }

            if (TransitionScene.isLoaded)
            {
                Scene s = SceneManager.GetSceneByName("InitDontDestroy");
                GameObject[] gos = s.GetRootGameObjects();
                foreach (var go in gos)
                {
                    if (go.name == "Camera")
                    {
                        go.GetComponent<AudioListener>().enabled = false;
                    }
                }
                SceneManager.UnloadSceneAsync("InitDontDestroy");
            }
            yield return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneLoadManager.Instance.LoadMain(false);
        //SceneLoadManager.Instance.LoadMain(true);
        StartCoroutine(LoadScene());
    }
}
