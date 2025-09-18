using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]   // 用于 Unity 序列化
public class Option
{

    public string text;          // 选项文本
    public int goldChange;       // 每次事件的金币变化
    public int peopleChange;     // 人口变化
    public int zhouLiChange;     // 周礼变化
    public int weiwangChange;    // 威望变化
    public bool oifzz;           // 是否触发治政QTE
    public bool oiftl;           // 是否触发统领QTE
    public bool oifjs;           // 是否触发军事QTE
    public bool oifhm;           // 是否触发后宫QTE
    public string nextEventId = "0";   // 选项对应的下一个决策 ID

    public bool isNextEra = false; // 是否是跳转到下一时代

    
}
