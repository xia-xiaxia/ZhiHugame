using System;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class GameEvent
{
    public string id;            // 事件唯一标识
    public string title;
    public string body;
    public List<Option> options; // 选项列表

    public int Invterval = 0;

    public int nextEventId = -1; // 用于线性事件的下一个事件ID


}

