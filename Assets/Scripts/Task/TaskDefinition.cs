using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TaskRange
{
    public string stat;
    public int min = int.MinValue;
    public int max = int.MaxValue;

    public bool IsSatisfied(int value)
    {
        return value >= min && value <= max;
    }
}

[Serializable]
public class TaskDefinition
{
    public string id;
    public string description;
    public int timeLimit = -1;
    public List<TaskRange> requiredElements = new List<TaskRange>();
    public List<TaskRange> failLimits = new List<TaskRange>();
    public string successEventId;
    public int successEventFile = 0;
    public string failEventId;
    public int failEventFile = 0;
}