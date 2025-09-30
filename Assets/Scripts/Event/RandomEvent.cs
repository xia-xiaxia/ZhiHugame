using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RandomEvent
{
    public string id;            // 事件唯一标识
    public string title;         // 事件标题
    public string body;          // 事件内容
    public string person;        // 事件发言人
    public List<Option> options; // 选项列表

    public int Interval = 0;     // 随机事件决策的间隔随机事件数
    public TType Ttype;
    public bool isDustActive = false;    // 事件是否激活
    public int isAffctEvents = 0;     // 影响采用的随机事件集（发言人可能四了）

    public enum TType
    {
        era0,
        era1,
        era2,
        era3
    }

    public void SetTEventTypeFromId()
    {
        if (string.IsNullOrEmpty(id))
        {
            Ttype = TType.era0;
            return;
        }

        char firstChar = id[0];
        switch (firstChar)
        {
            case '1':
                Ttype = TType.era0;
                break;
            case '2':
                Ttype = TType.era1;
                break;
            case '3':
                Ttype = TType.era2;
                break;
            case '4':
                Ttype = TType.era3;
                break;
            default:
                Ttype = TType.era0; // 默认值
                break;
        }

    }
}
