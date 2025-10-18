using System;
using UnityEngine;

[Serializable]
public class EndingTrigger
{
    public string stat;      // king/noble/scholar/foreign/people
    public string type;      // "max" 或 "min"
    public int value;        // 阈值
}

[Serializable]
public class EndingDefinition
{
    public string id;
    public string description;
    public EndingTrigger trigger;
}