using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class MissionUI : MonoBehaviour
{

    //当前在第几页
    public int nowPage = 1;
    private int maxPage = 1;

    public GameObject missionMenu;

    public TaskSlot[] taskslots;

    public TextMeshProUGUI pageNum;

    public bool canReward = false;
   
    //供button调用
    public void DisplayWithReward()
    {

        missionMenu.SetActive(true);
        canReward = true;

        //目前的逻辑是在激活Menu的时候判断任务是否完成
        MissionManager.Instance.CheckComplete();

        RefreshPage();
    }

    public void DisplayWithoutReward()
    {
        missionMenu.SetActive(true);
        canReward = false;

        //目前的逻辑是在激活Menu的时候判断任务是否完成
        MissionManager.Instance.CheckComplete();

        RefreshPage();
    }

    public void Close()
    {
        missionMenu.SetActive(false);
    }

    private void RefreshPage()
    {
        maxPage = Mathf.CeilToInt(MissionManager.Instance.activeMissions.Count / 3.0f);

        int startIndex = (nowPage - 1) * 3;

        for (int i = 0; i < 3; i++)
        {
            int currentIndex = startIndex + i;

            if (currentIndex < MissionManager.Instance.activeMissions.Count)
            {
                int missionId = MissionManager.Instance.activeMissions[currentIndex];

                taskslots[i].Display(MissionManager.Instance.GetTaskDataById(missionId));
            }
            else
            {
                taskslots[i].Display(null);
            }
        }

        DisplayPageNum();
    }

    public void PrevPage()
    {
        if (nowPage == 1) return;
        nowPage--;
        RefreshPage();
    }
    public void NextPage()
    {
        if (nowPage * 3 >= MissionManager.Instance.activeMissions.Count) return;
        nowPage++;
        RefreshPage();
    }

    private void DisplayPageNum()
    {
        pageNum.text = nowPage.ToString() + "/" + maxPage.ToString() + "页";
    }
}
