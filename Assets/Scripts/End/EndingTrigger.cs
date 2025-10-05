using System;
using UnityEngine;

[Serializable]
public class EndingTrigger
{
    public string stat;      // king/noble/scholar/foreign/people
    public string type;      // "max" 或 "min" 或 "task"
    public int value;        // 阈值
    public string taskId;    // 任务触发时用
}

[Serializable]
public class EndingDefinition
{
    public string id;
    public string description;
    public EndingTrigger trigger;
}