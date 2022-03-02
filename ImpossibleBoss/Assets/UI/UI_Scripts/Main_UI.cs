using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main_UI : MonoBehaviour
{
    public GameObject ui_canvas;
    GraphicRaycaster ui_raycaster;

    PointerEventData click_data;
    List<RaycastResult> click_results;

    private GameObject pnl_main;
    private GameObject pnl_skill;
    private GameObject pnl_boss;
    private GameObject pnl_option;
    private GameObject pnl_rank;

    private List<GameObject> btn_sskill_list = new List<GameObject>();
    private List<GameObject> btn_sskill_in_pnl_list = new List<GameObject>();

    private List<GameObject> btn_sdifficulty_list = new List<GameObject>();

    private GameObject txt_skill_name_in_pnl;
    private GameObject txt_skill_desp_in_pnl;
    private GameObject txt_skill_cooltime_in_pnl;

    private GameObject txt_select_boss;

    private ushort skill_set_counter = 0;

    public InputField userName_field;

    AsyncOperationHandle BTN_Handle;

    void Start()
    {
        // pnl config. select which pnl will showing
        ui_raycaster = ui_canvas.GetComponent<GraphicRaycaster>();
        click_data = new PointerEventData(EventSystem.current);
        click_results = new List<RaycastResult>();

        pnl_main = GameObject.Find("PNL_main");
        pnl_skill = GameObject.Find("PNL_skill");
        pnl_boss = GameObject.Find("PNL_boss");
        pnl_option = GameObject.Find("PNL_option");
        pnl_rank = GameObject.Find("PNL_rank");

        // selected skilled check & show it
        // main first
        for (int i = 1; i < 7; i++)
        {
            GameObject btn_sskill_obj = GameObject.Find(string.Format("BTN_sskill_{0}", i));
            btn_sskill_list.Add(btn_sskill_obj);
        }
        for (int i = 1; i < 7; i++)
        {
            GameObject btn_sskill_in_pnl_obj = GameObject.Find(string.Format("BTN_sskill_in_pnl_{0}", i));
            btn_sskill_in_pnl_list.Add(btn_sskill_in_pnl_obj);
        }

        // boss select buttons active
        for (int i = 0; i < 3; i++)
        {
            GameObject btn_boss_difficulty_obj = GameObject.Find(string.Format("BTN_boss_difficulty_{0}", i));
            btn_sdifficulty_list.Add(btn_boss_difficulty_obj);
        }
        PlayerData pd = FindObjectOfType<PlayerData>();

        if ((int)pd.Difficulty == 0)
        {
            btn_sdifficulty_list[0].GetComponent<Image>().color = new Color(241 / 255f, 235 / 255f, 115 / 255f);
        }
        else if ((int)pd.Difficulty == 1)
        {
            btn_sdifficulty_list[1].GetComponent<Image>().color = new Color(241 / 255f, 235 / 255f, 115 / 255f);
        }
        else if ((int)pd.Difficulty == 2)
        {
            btn_sdifficulty_list[2].GetComponent<Image>().color = new Color(241 / 255f, 235 / 255f, 115 / 255f);
        }

        // boss select main button 보스명, 난이도로 텍스트 변환
        txt_select_boss = GameObject.Find("TXT_select_boss");
        txt_select_boss.GetComponent<Text>().text = string.Format("1.Vanistoll - {0} -", pd.Difficulty);

        GlobalSkillList gsl = FindObjectOfType<GlobalSkillList>();
        Transform Content_skill_list_transform = GameObject.Find("Content_skill_list").transform;
        
        GameObject prefab_ui_skill_obj = null;
        Addressables.LoadAssetAsync<GameObject>("BTN_skill_list").Completed += (obj) => 
        {
            BTN_Handle = obj;
            prefab_ui_skill_obj = obj.Result; 
            //Logger.Log("BTN_skill_list is Loaded");

            foreach (var skill in gsl.SkillList)
            {
                //Logger.Log(skill.Value.CanUse);
                if (skill.Value.ID == ushort.MaxValue) { continue; }
                if (skill.Value.CanUse == true)
                {
                    GameObject skill_obj = Instantiate(prefab_ui_skill_obj);
                    skill_obj.name = string.Format("skill_obj_{0}", skill.Value.ID);
                    skill_obj.transform.SetParent(Content_skill_list_transform);
                    skill_obj.GetComponent<Image>().sprite = skill.Value.Icon;
                }
                
            }

            UpdateSskillIMG();
        };

        //GameObject prefab_ui_skill_obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/BTN_skill_list.prefab");

        //foreach (var skill in gsl.SkillList)
        //{
        //    Logger.Log("make list (BTN_skill_list)");
        //    if (skill.Value.ID == ushort.MaxValue) { continue; }
        //    GameObject skill_obj = Instantiate(prefab_ui_skill_obj);
        //    skill_obj.name = string.Format("skill_obj_{0}", skill.Value.ID);
        //    skill_obj.transform.SetParent(GameObject.Find("Content_skill_list").transform);
        //    skill_obj.GetComponent<Image>().sprite = skill.Value.Icon;
        //}

        //UpdateSskillIMG();

        // skill 상세 페이지
        // start 시에는 skill list 1번으로
        txt_skill_name_in_pnl = GameObject.Find("TXT_skill_name_in_pnl");
        txt_skill_desp_in_pnl = GameObject.Find("TXT_skill_desp_in_pnl");
        txt_skill_cooltime_in_pnl = GameObject.Find("TXT_skill_cooltime_in_pnl");
       
        txt_skill_name_in_pnl.GetComponent<Localize>().SetID(gsl.SkillList[1].Name);
        txt_skill_cooltime_in_pnl.GetComponent<Text>().text = string.Format("{0}초", gsl.SkillList[1].CoolDown);
        txt_skill_desp_in_pnl.GetComponent<Localize>().SetID(gsl.SkillList[1].Description);

        pnl_skill.SetActive(false);
        pnl_boss.SetActive(false);
        pnl_option.SetActive(false);
        pnl_rank.SetActive(false);
        pnl_main.SetActive(true);
        
        if (pd.UserName != "Noname")
        {
            userName_field.text = pd.UserName;
        }
        userName_field.characterLimit = 10;
        userName_field.onValueChanged.AddListener(
             (word) => userName_field.text = Regex.Replace(word, @"[^0-9a-zA-Z_]", "")
        );

        // rank db select
        SelectSoloRankResponse rank_data = GetRankData(0, (int)pd.Difficulty, 0, 3);
        SetSmallRank(rank_data);
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            GetUIElementsClicked();
        }
    }
    void GetUIElementsClicked()
    {
        click_data.position = Mouse.current.position.ReadValue();
        click_results.Clear();

        ui_raycaster.Raycast(click_data, click_results);

        foreach (RaycastResult result in click_results)
        {
            GameObject ui_element = result.gameObject;

            //Logger.Log(ui_element.name);

            if (pnl_skill.activeSelf == true)
            {
                if (skill_set_counter == 0)
                {
                    if (ui_element.CompareTag("sskill_pnl"))
                    {
                        skill_set_counter = ushort.Parse(ui_element.name.Split('_')[4]);
                        // 해당 sskill_pnl 노랑변환 (RGB + 투명도)
                        //ui_element.GetComponent<Renderer>().material.color = new Color(241/255f, 235/255f, 115/255f);
                        ui_element.GetComponent<Image>().color = new Color(241 / 255f, 235 / 255f, 115 / 255f);
                    }
                    else if (ui_element.CompareTag("skill_list_pnl"))
                    {
                        // 스킬 상세창 변경
                        GlobalSkillList gsl = FindObjectOfType<GlobalSkillList>();

                        txt_skill_name_in_pnl.GetComponent<Localize>().SetID(gsl.SkillList[ushort.Parse(ui_element.name.Split('_')[2])].Name);
                        txt_skill_cooltime_in_pnl.GetComponent<Text>().text = string.Format("{0}초", gsl.SkillList[ushort.Parse(ui_element.name.Split('_')[2])].CoolDown);
                        txt_skill_desp_in_pnl.GetComponent<Localize>().SetID(gsl.SkillList[ushort.Parse(ui_element.name.Split('_')[2])].Description);
                    }
                }
                else
                {
                    if (ui_element.CompareTag("sskill_pnl"))
                    {
                        if (skill_set_counter == ushort.Parse(ui_element.name.Split('_')[4]))
                        {
                            btn_sskill_in_pnl_list[skill_set_counter - 1].GetComponent<Image>().color = new Color(255 / 255f, 255 / 255f, 255 / 255f);
                            skill_set_counter = 0;
                        }
                        else
                        {
                            // 다른거 클릭시 기존 클릭한거 노랑변환 해제
                            btn_sskill_in_pnl_list[skill_set_counter - 1].GetComponent<Image>().color = new Color(255 / 255f, 255 / 255f, 255 / 255f);
                            // 새 클릭한걸로 counter 변경
                            skill_set_counter = ushort.Parse(ui_element.name.Split('_')[4]);
                            // 해당 sskill_pnl 노랑변환
                            ui_element.GetComponent<Image>().color = new Color(241 / 255f, 235 / 255f, 115 / 255f);
                        }
                    }
                    else if (ui_element.CompareTag("skill_list_pnl"))
                    {
                        // gsl 변환
                        GlobalSkillListSetting(skill_set_counter, ui_element);

                        // 노랑변환 해제
                        btn_sskill_in_pnl_list[skill_set_counter - 1].GetComponent<Image>().color = new Color(255 / 255f, 255 / 255f, 255 / 255f);

                        // counter reset
                        skill_set_counter = 0;
                    }
                }
            }
            
            if (pnl_boss.activeSelf == true)
            {
                if (ui_element.CompareTag("sdifficulty_pnl"))
                {
                    // 노랑변환 셋 다 해제
                    for (int i=0; i<3; i++)
                    {
                        btn_sdifficulty_list[i].GetComponent<Image>().color = new Color(255 / 255f, 255 / 255f, 255 / 255f);
                    }
                    // 해당 버튼 노랑변환
                    btn_sdifficulty_list[int.Parse(ui_element.name.Split('_')[3])].GetComponent<Image>().color = new Color(241 / 255f, 235 / 255f, 115 / 255f);

                    // pd 세팅
                    PlayerData pd = FindObjectOfType<PlayerData>();
                    pd.Difficulty = (EDifficulty)(int.Parse(ui_element.name.Split('_')[3]));
                }
            }
        }
    }
    public void GlobalSkillListSetting(ushort counter, GameObject ui_element)
    {
        GlobalSkillList gsl = FindObjectOfType<GlobalSkillList>();
        // if : gsl 안에 이미 skill_obj ID와 동일한 ID 있으면, 해당 항목 empty 처리
        for (int i = 0; i < 6; i++)
        {
            if (gsl.SelectedSkills[i].ID == ushort.Parse(ui_element.name.Split('_')[2]))
            {
                gsl.SetSelectedSkill(i, ushort.MaxValue);
            }
        }

        // gsl에 setting
        gsl.SetSelectedSkill(Convert.ToInt32(counter) - 1, ushort.Parse(ui_element.name.Split('_')[2]));

        // sskill, sskill_pnl img 변환
        UpdateSskillIMG();
    }

    public void UpdateSskillIMG()
    {
        GlobalSkillList gsl = FindObjectOfType<GlobalSkillList>();
        for (int i = 0; i < 6; i++)
        {
            Logger.Log(btn_sskill_list[i]);
            btn_sskill_list[i].GetComponent<Image>().sprite = gsl.SelectedSkills[i].Icon;
            btn_sskill_in_pnl_list[i].GetComponent<Image>().sprite = gsl.SelectedSkills[i].Icon;
        }

    }

    public SelectSoloRankResponse GetRankData(int bossType, int bossDiff, int offset, int maxRows)
    {
        DBConnect dbc = FindObjectOfType<DBConnect>();
        SelectSoloRankRequest body = new SelectSoloRankRequest();
        body.bossType = bossType;
        body.bossDiff = bossDiff;
        body.offset = offset;
        body.maxRows = maxRows;

        SelectSoloRankResponse result = dbc.SelectSoloRank(body);
        return result;
    }

    public void SetSmallRank(SelectSoloRankResponse data)
    {
        int i = 1;
        if (data == null)
        {
            for(int l=0; l<3; l++)
            {
                GameObject userName = GameObject.Find(string.Format("PNL_data_in_small_rank_{0}", i)).transform.GetChild(0).gameObject;
                userName.GetComponent<Text>().text = "-";

                GameObject dealPerSec = GameObject.Find(string.Format("PNL_data_in_small_rank_{0}", i)).transform.GetChild(1).gameObject;
                dealPerSec.GetComponent<Text>().text = "-";

                GameObject clearTime = GameObject.Find(string.Format("PNL_data_in_small_rank_{0}", i)).transform.GetChild(2).gameObject;
                clearTime.GetComponent<Text>().text = "-";

                GameObject createdAt = GameObject.Find(string.Format("PNL_data_in_small_rank_{0}", i)).transform.GetChild(3).gameObject;
                createdAt.GetComponent<Text>().text = "-";

                i++;
            }
        }
        else
        {
            foreach (SelectSoloRankResultDetail list in data.Body)
            {
                GameObject userName = GameObject.Find(string.Format("PNL_data_in_small_rank_{0}", i)).transform.GetChild(0).gameObject;
                userName.GetComponent<Text>().text = list.userName;

                GameObject dealPerSec = GameObject.Find(string.Format("PNL_data_in_small_rank_{0}", i)).transform.GetChild(1).gameObject;
                dealPerSec.GetComponent<Text>().text = list.dealPerSec.ToString("F2");

                GameObject clearTime = GameObject.Find(string.Format("PNL_data_in_small_rank_{0}", i)).transform.GetChild(2).gameObject;
                clearTime.GetComponent<Text>().text = ((int)(list.clearTime / 60)).ToString() + ":" + ((int)list.clearTime % 60).ToString() + ":" + (list.clearTime % 60).ToString("F2").Split('.')[1];

                GameObject createdAt = GameObject.Find(string.Format("PNL_data_in_small_rank_{0}", i)).transform.GetChild(3).gameObject;
                createdAt.GetComponent<Text>().text = list.createdAt.Split(' ')[0] + "\n" + list.createdAt.Split(' ')[1];

                i++;
            }
        }
    }

    public void ClickBTNQuit()
    {
        Application.Quit();
    }

    public void ClickBTNGameStart()
    {
        // username 저장
        PlayerData pd = FindObjectOfType<PlayerData>();
        if (userName_field.text != null)
        {
            pd.UserName = userName_field.text;
            PlayerPrefs.SetString("UserName", userName_field.text);
        }

        // game scene 실행
        SceneLoadManager.Instance.LoadScene(gameObject.scene, "Scene_Vanistoll");
    }

    public void ClickSkillSellect()
    {
        pnl_skill.SetActive(true);
        pnl_main.SetActive(false);
    }

    public void ClickOption()
    {
        pnl_option.SetActive(true);
        pnl_main.SetActive(false);
    }

    public void ClickRank()
    {
        pnl_rank.SetActive(true);
        pnl_main.SetActive(false);
    }

    public void ClickBossSellect()
    {
        pnl_boss.SetActive(true);
        pnl_main.SetActive(false);
    }

    public void ClickBackToMain()
    {
        if (skill_set_counter != 0)
        {
            btn_sskill_in_pnl_list[skill_set_counter - 1].GetComponent<Image>().color = new Color(255 / 255f, 255 / 255f, 255 / 255f);
            skill_set_counter = 0;
        }

        pnl_rank.SetActive(false);
        pnl_skill.SetActive(false);
        pnl_option.SetActive(false);
        pnl_boss.SetActive(false);
        pnl_main.SetActive(true);

        PlayerData pd = FindObjectOfType<PlayerData>();
        txt_select_boss = GameObject.Find("TXT_select_boss");
        txt_select_boss.GetComponent<Text>().text = string.Format("1.Vanistoll - {0} -", pd.Difficulty);

        SelectSoloRankResponse rank_data = GetRankData(0, (int)pd.Difficulty, 0, 3);
        SetSmallRank(rank_data);
    }

    public void SetPnlSkillList()
    {

    }
}
