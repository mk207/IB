using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DeadZone : MonoBehaviour
{
    static STakeDamageParams damageParams;

    //public DeadZone Next { get; set; }
    //public Action<DeadZone> ReturnObjectDelegate { get; set; }

    private void Awake()
    {
        damageParams = new STakeDamageParams();
        damageParams.damageAmount = float.MaxValue;
        if(damageParams.causedBy == null)
        {
            damageParams.causedBy = FindObjectOfType<BossInformation>().gameObject;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerInformation player = other.gameObject.GetComponent<PlayerInformation>();
        if (gameObject.activeSelf && player != null)
        {
            player.TakeDamage(damageParams);
            Logger.Log("YOU ACTIVATED MY DEAD ZONE");
        }
        //else
        //{
        //    Logger.Log("FEELINGS OF INVINCIBILITY OVER");
        //}
    }
}
