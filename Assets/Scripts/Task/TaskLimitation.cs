using System;
using UnityEngine;

[System.Serializable]
public class TaskLimitation
{
    public string statType;      // 数值类型：gold、people、zhouli、weiwang、year
    public string operator_;     // 操作符：">", "<", ">=", "<=", "==", "!="
    public int value;           // 限制值
    public string description;  // 限制描述

    /// <summary>
    /// 检查是否触发了任务限制（触发限制 = 任务失败）
    /// </summary>
    public bool CheckLimitation(StatModel stats)
    {
        int currentValue = GetStatValue(stats, statType);
        
        switch (operator_)
        {
            case ">":
                return currentValue > value;
            case "<":
                return currentValue < value;
            case ">=":
                return currentValue >= value;
            case "<=":
                return currentValue <= value;
            case "==":
                return currentValue == value;
            case "!=":
                return currentValue != value;
            default:
                Debug.LogError($"未知的操作符: {operator_}");
                return false;
        }
    }

    /// <summary>
    /// 根据统计类型获取对应的数值
    /// </summary>
    private int GetStatValue(StatModel stats, string statType)
    {
        switch (statType.ToLower())
        {
            case "gold":
                return stats.gold;
            case "people":
                return stats.people;
            case "zhouli":
                return stats.zhouli;
            case "weiwang":
                return stats.weiwang;
            case "year":
                return stats.year;
            default:
                Debug.LogError($"未知的统计类型: {statType}");
                return 0;
        }
    }

    /// <summary>
    /// 获取限制状态的描述信息
    /// </summary>
    public string GetStatusDescription(StatModel stats)
    {
        int currentValue = GetStatValue(stats, statType);
        bool isTriggered = CheckLimitation(stats);
        string status = isTriggered ? "No" : "Yes";
        
        return $"{status} {description} (当前: {currentValue}, 限制: {operator_} {value})";
    }
}