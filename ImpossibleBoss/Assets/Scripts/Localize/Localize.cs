using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Localize : MonoBehaviour
{
    private static OptionManager OM;
    private static string[] Languages;
    private AsyncOperationHandle LanguageHandle;
    public string ID;
    public UnityEngine.UI.Text m_Text;
    private string mString;
    //System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))");

    public string String { get { return mString; } }
    
    private void Awake()
    {
        if(OM)
        {
            return;
        }
        else
        {
            OM = FindObjectOfType<OptionManager>();
        }

        if(Languages == null)
        {
            TextAsset t = Resources.Load<TextAsset>("Language/Language");
            Languages = t.text.Split('\r', '\n');
            //Languages = System.IO.File.ReadAllLines(t.text);
            //Languages = System.IO.File.ReadAllLines(@"Assets/Resources/Language/Language - Language.tsv");
            //Languages = System.IO.File.ReadAllLines(Application.dataPath + @"/Resources/Language/Language.tsv");
            //Addressables.LoadAssetAsync<string[]>("Language").Completed += (obj) => { LanguageHandle = obj; Languages = obj.Result; };
        }               
        //Logger.Log(string.Format("{0} Awake", gameObject));
    }

    private void Start()
    {
        OM.OnChangeLanguage += ChangeLanguage;
        //ChangeLanguage();
        //Logger.Log(string.Format("{0} Start : {1}", gameObject, String));
    }

    public void SetID(string id)
    {
        ID = id;
        ChangeLanguage();
    }

    public void ChangeLanguage()
    {
        //Logger.Log(string.Format("{0} ChangeLang", gameObject));
        if (GetString(ID) && gameObject.activeSelf)
        {
            if (m_Text != null)
            {
                m_Text.text = String;
            }
            else
            {
                GetComponent<UnityEngine.UI.Text>().text = String;
            }            
        }
        else
        {
            if (m_Text != null)
            {
                m_Text.text = "ID is wrong";
            }
            else
            {
                GetComponent<UnityEngine.UI.Text>().text = "ID is wrong";
            }            
        }
        Logger.Log($"{ID} // {gameObject.activeSelf}");
    }

    private bool GetString(string id)
    {
        int index = ((int)OM.Language);
        IEnumerable<string> strs = Languages;

        var columnQuery =
            from line in strs
            let elements = line.Split('\t')
            //let elements = rex.Split(line)
            where elements[0] == id
            select elements[index];

        var results = columnQuery.ToString();

        if(columnQuery.Count() == 0)
        {
            return false;
        }
        else
        {
            mString = columnQuery.First();
            
            //foreach (string s in columnQuery)
            //{
            //    Logger.Log(string.Format("Localized string : {0}", s));
            //}

            return true;
        }
        

        
        
    }
}
