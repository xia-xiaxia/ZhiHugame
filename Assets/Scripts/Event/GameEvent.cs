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

    public void SetGEventTypeFromId()
    {
        if (string.IsNullOrEmpty(id))
        {
            Gtype = GType.Event;
            return;
        }

        char firstChar = id[0];
        switch (firstChar)
        {
            case '0':
                Gtype = GType.Event;
                break;
            case '1':
                Gtype = GType.Mask;
                break;
            default:
                Gtype = GType.Event; // 默认值
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

        char firstChar = id[1];
        switch (firstChar)
        {
            case '0':
                Ttype = TType.era0;
                break;
            case '1':
                Ttype = TType.era1;
                break;
            case '2':
                Ttype = TType.era2;
                break;
            case '3':
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
    Event,
    Mask

}

public enum TType
{
    era0,
    era1,
    era2,
    era3
}
