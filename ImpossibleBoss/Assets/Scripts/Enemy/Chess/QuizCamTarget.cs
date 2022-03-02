using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizCamTarget : MonoBehaviour
{
    [SerializeField]
    private Transform m_PlayerTransform;

    private Transform PlayerTransform { get { return m_PlayerTransform; } set { m_PlayerTransform = value; } }

    private void Awake()
    {
        PlayerInformation[] players = FindObjectsOfType<PlayerInformation>();

        foreach (var player in players)
        {
            if (player.tag.Equals("Player"))
            {
                PlayerTransform = player.transform;
                break;
            }
        }
    }

    private void Start()
    {       
        if (PlayerTransform == null)
        {
            Logger.LogError("QuizCamTarget's Player is null");
            gameObject.SetActive(false);
        }
        
    }

    // Update is called once per frame
    void Update()
    {        
        transform.position = new Vector3(14.0f, 0.0f, PlayerTransform.position.z);
    }
}
