using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]   // 用于 Unity 序列化
public class Option
{
    public string text;          // 选项文本
    
    // 五大数值变化
    public int goldChange;       // 金币变化
    public int peopleChange;     // 人口变化
    public int zhouLiChange;     // 周礼变化
    public int weiwangChange;    // 威望变化
    
    // 特殊内容变化（道具等）
    public string specialItemChange;  // 特殊道具变化（待完善）
    
    // 该选项后继事件ID（如果无后继决策，这里为0）
    public string nextEventId = "0";
    
    // 决策间隔
    public int interval = 0;
    
    // 激活任务
    public string activateTask = "";
    
    // 影响采用的随机事件集
    public int randomEventSet = 0;
    
}

    
