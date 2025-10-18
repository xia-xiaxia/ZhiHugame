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

    public int yearDelta = 1; // 本事件推进的年份数，默认1年

    public string speaker;

}

