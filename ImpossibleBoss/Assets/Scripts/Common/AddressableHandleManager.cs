using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableHandleManager
{
    private List<AsyncOperationHandle> mAddressableHandles;

    public List<AsyncOperationHandle> AddressableHandles { get { return mAddressableHandles; } }

    public AddressableHandleManager(int capacity)
    {
        mAddressableHandles = new List<AsyncOperationHandle>(capacity);
    }

    public void Add(AsyncOperationHandle handle)
    {
        AddressableHandles.Add(handle);
    }

    public void ReleaseAll()
    {
        int count = AddressableHandles.Count;
        for(int index = 0; index < count; index++)
        {
            if (AddressableHandles[index].IsValid())
            {
                Addressables.Release(AddressableHandles[index]);
            }
        }
    }
}
