using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestTrap : MonoBehaviour{

    float second = 0.0f;
    bool once = false;
    private void OnTriggerStay(Collider other)
    {
        if (!once)
        {
            once = true;
            STakeDamageParams takeDamageParams = new STakeDamageParams();
            takeDamageParams.causedBy = gameObject;
            takeDamageParams.damageAmount = 50000.0f;
            takeDamageParams.DamageAmplificationRate = 0.0f;
            other.GetComponent<LivingEntity>().TakeDamage(takeDamageParams);
        }
        

        //if(second <= 0.0f)
        //{
        //    STakeDamageParams takeDamageParams = new STakeDamageParams();
        //    takeDamageParams.causedBy = gameObject;
        //    takeDamageParams.damageAmount = 50000.0f;
        //    takeDamageParams.DamageAmplificationRate = 0.0f;
        //    other.GetComponent<LivingEntity>().TakeDamage(takeDamageParams);
        //    second = 0.5f;
        //}
        //else
        //{
        //    second -= Time.deltaTime;
        //    Logger.Log(string.Format("Trap : {0} // {1}", other, second));
        //}
    }

}
