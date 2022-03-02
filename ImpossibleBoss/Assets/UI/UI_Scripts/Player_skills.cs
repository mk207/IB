using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_skills : MonoBehaviour
{
    public static Player_skills instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance != this)
                Destroy(this.gameObject);
        }
    }



}
