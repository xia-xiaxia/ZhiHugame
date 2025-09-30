using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryEvent
{
    public string id;            // 事件唯一标识
    public string title;         // 事件标题
    public string body;          // 事件内容
    public string person;        // 事件发言人
    public List<Option> options; // 选项列表

<<<<<<< HEAD:Assets/Scripts/Event/GameEvent.cs
    public int Invterval = 0;

    public int nextEventId = -1; // 用于线性事件的下一个事件ID


}

=======
    public int Interval = 0;     // 历史事件决策的间隔随机事件数

    public int nextEventId = -1; // 用于线性事件的下一个事件ID

    public bool isDustActive = false;    // 事件是否激活
    public int isAffctEvents = 0;     // 影响采用的随机事件集（发言人可能四了）

    public TType Ttype;

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
>>>>>>> 7cf8af2542cd309f4def5973e6b761bc57f4e617:Assets/Scripts/Event/HistoryEvent.cs
