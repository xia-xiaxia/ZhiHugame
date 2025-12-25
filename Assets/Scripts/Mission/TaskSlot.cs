using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskSlot : MonoBehaviour
{
    //这个脚本用来控制TaskSlot的显示内容。

    public TextMeshProUGUI taskName;
    public TextMeshProUGUI taskDesc;
    public TextMeshProUGUI taskReward;

    [SerializeField]
    string prefixInDesc = "     ";
    [SerializeField]
    string colorString = "<color=#7CFF7C>";
    [SerializeField]
    string colorStringEnd = "</color>";
    [SerializeField]
    string intervalInReward = "    "; //四个空格

    public GameObject comp0;
    public GameObject comp1;


    public void Display(MissionData missionData)
    {
        if(missionData == null)
        {
            this.gameObject.SetActive(false);
            return;
        } else
        {
            this .gameObject.SetActive(true);
        }
        taskName.text = missionData.name;
        taskDesc.text = prefixInDesc + missionData.description;

        string rewardStr = "";

        if(missionData.rewardPolicyId != 0)
        {
            rewardStr += colorString + "国策" + colorStringEnd + "奖励：" + missionData.rewardPolicyId.ToString() + intervalInReward;
        }

        if(missionData.rewardTalent != 0)
        {
            rewardStr += colorString + "天赋点" + colorStringEnd + "奖励：" + missionData.rewardTalent.ToString();
        }

        taskReward.text = rewardStr;

        if(MissionManager.Instance.isComplete(missionData.id))
        {
            comp0.SetActive(false);
            comp1.SetActive(true);
        } else
        {
            comp0.SetActive(true);
            comp1.SetActive(false);
        }
    }

}
