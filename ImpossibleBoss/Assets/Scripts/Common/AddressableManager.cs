using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.AddressableAssets.ResourceLocators;

public class AddressableManager : MonoBehaviour
{
    [SerializeField] 
    private Text m_SizeText;
    [SerializeField]
    private Text m_ProgressText;
    [SerializeField]
    private Text m_VersionText;

    private float mLerp = 0.0f;
    private bool mbIsReadied = default;

    //[Space]
    //[Header("�ٿ�ε带 ���ϴ� ���� �Ǵ� ����鿡 ���Ե� ���̺��� �ƹ��ų� �Է����ּ���.")]
    //[SerializeField] string LableForBundleDown = string.Empty;

    private string mProgressFormat = "Download progress: {0:P2} - {1:.00}Mb / {2:.00}Mb";
    //[SerializeField]
    //private List<string> m_Lables;
    //AssetLabelReference mLables;
    private List<AsyncOperationHandle> mHandles;

    private async void Awake()
    {
        //Caching.ClearCache();

        StartCoroutine(UpdateCatalogs());

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Readied();
            return;
        }
        
        var resourceLocator = await Addressables.InitializeAsync().Task;
        var allKeys = resourceLocator.Keys;
        float totalDownloadSizeMb = ByteToMb(await Addressables.GetDownloadSizeAsync(allKeys).Task);
        float downloadedMb = 0.0f;

        Debug.Log("Download Mbs required: " + totalDownloadSizeMb);
        foreach (var key in allKeys)
        {
            float keyDownloadSizeMb = ByteToMb(await Addressables.GetDownloadSizeAsync(key).Task);
            if (keyDownloadSizeMb <= 0) continue;

            var keyDownloadOperation = Addressables.DownloadDependenciesAsync(key);

            if (keyDownloadOperation.IsValid() == false) continue;

            keyDownloadOperation.Completed += obj => { Logger.Log($"Downloaded: {obj.Result.ToString()}"); /*Addressables.Release(obj);*/ };

            while (!keyDownloadOperation.IsDone)
            {
                await Task.Yield();
                float acquiredMb = downloadedMb + (keyDownloadOperation.PercentComplete * keyDownloadSizeMb);
                float totalProgressPercentage = (acquiredMb / totalDownloadSizeMb);
                m_ProgressText.text = string.Format(mProgressFormat, totalProgressPercentage, acquiredMb, totalDownloadSizeMb);
                //Logger.Log($"Downloaded: {acquiredMb}");
            }
            Addressables.Release(keyDownloadOperation);
            downloadedMb += keyDownloadSizeMb;
        }

        Readied();
    }

    private IEnumerator UpdateCatalogs()
    {
        List<string> catalogsToUpdate = new List<string>();
        AsyncOperationHandle<List<string>> checkForUpdateHandle = Addressables.CheckForCatalogUpdates();
        checkForUpdateHandle.Completed += op =>
        {
            catalogsToUpdate.AddRange(op.Result);
        };
        yield return checkForUpdateHandle;
        Debug.Log($"count: {catalogsToUpdate.Count}");

        if (catalogsToUpdate.Count > 0)
        {
            AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
            yield return updateHandle;
        }

        Readied();
    }
    static float ByteToMb(long bytes)
    {
        return bytes * 0.000001f;
    }

    //private System.Collections.IEnumerator UpdatePercent()
    //{
    //    float downloadPercent = 0.0f;

    //    while (downloadPercent < 1.0f || mHandles.Count != 0)
    //    {
    //        downloadPercent = 0.0f;
    //        for (int count = 0; count < m_Lables.Count; ++count)
    //        {
    //            downloadPercent += mHandles[count].PercentComplete;
    //        }
    //        m_ProgressText.text = string.Format(mProgressFormat, downloadPercent / m_Lables.Count * 100.0f);
    //        yield return null;
    //    }

    //    Readied();
    //}

    private void Readied()
    {
        FindObjectOfType<Test_NextScene>().AD = true;
        m_ProgressText.text = "PRESS ANY KEY TO START";
        mbIsReadied = true;
    }

    private void Update()
    {
        if (mbIsReadied)
        {
            mLerp = Mathf.Abs(Mathf.Sin(Time.timeSinceLevelLoad));
            m_ProgressText.color = new Color(1.0f, 1.0f, 1.0f, mLerp);
        }        
    }

    //public void _Click_BundleDown()
    //{
    //    Addressables.DownloadDependenciesAsync(Lables).Completed +=
    //        (AsyncOperationHandle Handle) =>
    //        {
    //            //DownloadPercent������Ƽ�� �ٿ�ε� ������ Ȯ���� �� ����.
    //            //ex) float DownloadPercent = Handle.PercentComplete;

    //            //Debug.Log("�ٿ�ε� �Ϸ�!");

    //            //�ٿ�ε尡 ������ �޸� ����.
    //            Addressables.Release(Handle);

    //        };
    //}

    //public void _Click_CheckTheDownloadFileSize()
    //{
    //    //ũ�⸦ Ȯ���� ���� �Ǵ� ����鿡 ���Ե� ���̺��� ���ڷ� �ָ� ��.
    //    //longŸ������ ��ȯ�Ǵ°� Ư¡��.
    //    Addressables.GetDownloadSizeAsync(Lables).Completed +=
    //        (AsyncOperationHandle<long> SizeHandle) =>
    //        {
    //            string sizeText = string.Concat(SizeHandle.Result, " byte");

    //            SizeText.text = sizeText;

    //            //�޸� ����.
    //            Addressables.Release(SizeHandle);
    //        };


    //}
}
