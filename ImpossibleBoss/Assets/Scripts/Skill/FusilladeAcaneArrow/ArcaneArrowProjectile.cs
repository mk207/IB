using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcaneArrowProjectile : MonoBehaviour
{
    //DestinationPos�� ���������� �����ؾ� �ϹǷ� transform�� �޾Ƽ� position�� ������.
    private Transform mDestinationPos;
    private Vector3 mDeparturePos;
    private STakeDamageParams mDamageParams;
    private float mLerpAlpha;


    public Transform DestinationPos { get { return mDestinationPos; } set { mDestinationPos = value; } }
    public Vector3 DeparturePos { get { return mDeparturePos; } set { mDeparturePos = value; } }
    public STakeDamageParams DamageParams { get { return mDamageParams; } set { mDamageParams = value; } }
    private float LerpAlpha { get { return mLerpAlpha; } set { mLerpAlpha = value; } }

    private void Start()
    {
        LerpAlpha = 0.0f;
        transform.position = DeparturePos;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(DeparturePos, DestinationPos.position, LerpAlpha);
        LerpAlpha += Time.deltaTime * 2.0f;

        if (LerpAlpha >= 1.0f) 
        {
            DestinationPos.GetComponent<LivingEntity>().TakeDamage(DamageParams);
            Destroy(gameObject);
        }
    }
}
