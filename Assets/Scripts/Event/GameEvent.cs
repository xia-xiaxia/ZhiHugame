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

    public GType Gtype;

    public TType Ttype;

    public int Invterval = 0;

    public int nextEventId = -1; // 用于线性事件的下一个事件ID

    public void SetGEventTypeFromId()
    {
        if (string.IsNullOrEmpty(id))
        {
            Gtype = GType.tempEvent;
            return;
        }

        char firstChar = id[1];
        switch (firstChar)
        {
            case '1':
                Gtype = GType.tempEvent;
                break;
            case '2':
                Gtype = GType.historyEvent;
                break;
            default:
                Gtype = GType.tempEvent; // 默认值
                break;
        }
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

public enum GType
{
    tempEvent,
    historyEvent

}

public enum TType
{
    era0,
    era1,
    era2,
    era3
}
