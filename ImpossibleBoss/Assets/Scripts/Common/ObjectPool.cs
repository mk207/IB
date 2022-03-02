using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectPool<Type>
{
    public Type Next { get; set; }
    public System.Action<Type> ReturnObjectDelegate { get; set; }
}

public class ObjectPool<Type> where Type : IObjectPool<Type>
{
    private List<Type> mPool;
    private List<Type> mActivatedObject;
    Type mAvailableObject;

    private List<Type> Pool { get { return mPool; } }
    private Type AvailableObject { get { return mAvailableObject; } set { mAvailableObject = value; } }

    public List<Type> ActivatedObject { get { return mActivatedObject; } }

    public Type GetAvailableObject()
    {
        Type newObject = AvailableObject;
        AvailableObject = newObject.Next;
        AddActivatedObject(newObject);
        return newObject;
    }
    //[("ONLY USE JUST SET ACTIVE FALSE")]
    public void ReturnAllObject()
    {
        for (int index = 0; index < mActivatedObject.Count; index++)
        {
            //ReturnObject(temp[index]);
            mActivatedObject[index].Next = AvailableObject;
            AvailableObject = mActivatedObject[index];
        }
        mActivatedObject.Clear();
        //var objects = ActivatedObject.ConvertAll(i => i);
        //int maxCount = ActivatedObject.Count;
        //for (int index = 0; index < maxCount; index++)
        //{
        //    objects[index].ReturnObjectDelegate(objects[index]);
        //    //ReturnObject(objects[index]);
        //}

    }

    public void ReturnObject(Type returnObject)
    {
        returnObject.Next = AvailableObject;
        AvailableObject = returnObject;
        RemoveActivatedObject(returnObject);
    }

    public void RegisterObject(Type newObject)
    {
        newObject.Next = AvailableObject;
        AvailableObject = newObject;
        Pool.Add(newObject);
    }

    private void AddActivatedObject(Type addObject)
    {
        mActivatedObject.Add(addObject);
    }
    private void RemoveActivatedObject(Type removeObject)
    {
        mActivatedObject.Remove(removeObject);
    }    

    public ObjectPool(int reserve)
    {        
        mPool = new List<Type>(reserve);
        mActivatedObject = new List<Type>(reserve);                
    }

    
}
