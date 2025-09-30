using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    public string id;                        // 任务唯一标识（如 "001001"）
    public string serial;                    // 任务序号（如 "001"）
    public string description;               // 任务描述
    
    // 时限设置
    public int timeLimit;                    // 时限（0=立即判定，-1=无时限，其他=具体回合数）
    
    // 任务要素（所有要素都为真才算成功）
    public List<TaskElement> taskElements = new List<TaskElement>();
    
    // 任务限制（任何一个限制触发就直接失败）
    public List<TaskLimitation> taskLimitations = new List<TaskLimitation>();
    
    // 结果事件
    public string successEventId;            // 完成触发的事件ID
    public string failureEventId;            // 失败触发的事件ID
    
    // 任务状态
    public bool isActive = false;            // 任务是否已激活
    public bool isCompleted = false;         // 任务是否已完成
    public int activationTurn = 0;           // 任务激活的回合数
    
    // 原有字段（保持兼容性）
    public bool isTimeLimited;               // 是否是限时任务（根据timeLimit自动计算）
    public string nextTaskId;                // 下一个任务的ID（如果有的话）
    public List<Option> options;             // 选项列表
    public int Intervals = 0;                // 历史事件决策的间隔随机事件数

    /// <summary>
    /// 激活任务
    /// </summary>
    public void ActivateTask(int currentTurn)
    {
        isActive = true;
        activationTurn = currentTurn;
        isTimeLimited = (timeLimit > 0);
        
        Debug.Log($"[Task] 任务 {id} ({serial}) 已激活，描述: {description}");
        
        // 根据时限类型输出不同信息
        if (timeLimit == 0)
        {
            Debug.Log($"[Task] 任务 {id} 为立即判定任务");
        }
        else if (timeLimit == -1)
        {
            Debug.Log($"[Task] 任务 {id} 为无时限任务");
        }
        else
        {
            Debug.Log($"[Task] 任务 {id} 时限为 {timeLimit} 回合");
        }
    }
    
    /// <summary>
    /// 检查任务是否应该判定（基于时限）
    /// </summary>
    public bool ShouldEvaluate(int currentTurn)
    {
        if (!isActive || isCompleted) return false;
        
        if (timeLimit == 0)
        {
            // 立即判定
            return true;
        }
        else if (timeLimit == -1)
        {
            // 无时限，需要外部触发判定
            return false;
        }
        else
        {
            // 检查是否到达时限
            return (currentTurn - activationTurn) >= timeLimit;
        }
    }
    
    /// <summary>
    /// 强制判定任务（用于无时限任务或手动触发）
    /// </summary>
    public TaskResult ForceEvaluate(StatModel stats)
    {
        Debug.Log($"[Task] 强制判定任务 {id}");
        return EvaluateTask(stats);
    }
    
    /// <summary>
    /// 评估任务完成情况
    /// </summary>
    public TaskResult EvaluateTask(StatModel stats)
    {
        if (isCompleted)
        {
            Debug.LogWarning($"[Task] 任务 {id} 已经完成，无需重复评估");
            return TaskResult.AlreadyCompleted;
        }
        
        Debug.Log($"[Task] 开始评估任务 {id}: {description}");
        
        // 1. 首先检查任务限制（任何一个限制触发就直接失败）
        foreach (var limitation in taskLimitations)
        {
            if (limitation.CheckLimitation(stats))
            {
                Debug.Log($"[Task] 任务 {id} 触发限制失败: {limitation.description}");
                CompleteTask(false);
                return TaskResult.Failed;
            }
        }
        
        // 2. 检查所有任务要素（必须全部满足）
        bool allElementsSatisfied = true;
        foreach (var element in taskElements)
        {
            if (!element.CheckCondition(stats))
            {
                Debug.Log($"[Task] 任务 {id} 要素未满足: {element.description}");
                allElementsSatisfied = false;
                break;
            }
        }
        
        if (allElementsSatisfied)
        {
            // 所有要素都满足，任务成功
            Debug.Log($"[Task] 任务 {id} 成功完成! 所有任务要素都已满足");
            CompleteTask(true);
            return TaskResult.Success;
        }
        else
        {
            // 有要素未满足，任务失败
            Debug.Log($"[Task] 任务 {id} 失败，存在未满足的任务要素");
            CompleteTask(false);
            return TaskResult.Failed;
        }
    }
    
    /// <summary>
    /// 完成任务并触发相应事件
    /// </summary>
    private void CompleteTask(bool success)
    {
        isCompleted = true;
        
        if (success)
        {
            Debug.Log($"[Task] ✅ 任务 {id} 成功完成，将触发事件: {successEventId}");
            if (!string.IsNullOrEmpty(successEventId) && successEventId != "0")
            {
                EventManager.Instance.SetNextEventId(successEventId);
            }
        }
        else
        {
            Debug.Log($"[Task] ❌ 任务 {id} 失败，将触发事件: {failureEventId}");
            if (!string.IsNullOrEmpty(failureEventId) && failureEventId != "0")
            {
                EventManager.Instance.SetNextEventId(failureEventId);
            }
        }
    }
    
    /// <summary>
    /// 获取任务的详细状态描述
    /// </summary>
    public string GetTaskStatus(StatModel stats)
    {
        if (!isActive)
            return $"📝 任务 {id} ({serial}): 未激活";
            
        if (isCompleted)
            return $"✅ 任务 {id} ({serial}): 已完成";
        
        string status = $"📋 任务 {id} ({serial}): {description}\n";
        
        // 时限信息
        if (timeLimit == 0)
            status += "⏰ 立即判定任务\n";
        else if (timeLimit == -1)
            status += "⏰ 无时限任务\n";
        else
            status += $"⏰ 时限: {timeLimit} 回合 (已过: {GameControl.Instance.turns - activationTurn} 回合)\n";
        
        // 任务要素状态
        if (taskElements.Count > 0)
        {
            status += "📋 任务要素 (全部满足才能成功):\n";
            foreach (var element in taskElements)
            {
                status += $"  {element.GetStatusDescription(stats)}\n";
            }
        }
        
        // 任务限制状态
        if (taskLimitations.Count > 0)
        {
            status += "🚫 任务限制 (触发任何一个即失败):\n";
            foreach (var limitation in taskLimitations)
            {
                status += $"  {limitation.GetStatusDescription(stats)}\n";
            }
        }
        
        return status;
    }
    
    /// <summary>
    /// 获取任务的简短状态
    /// </summary>
    public string GetShortStatus()
    {
        if (!isActive)
            return $"任务 {serial}: 未激活";
        if (isCompleted)
            return $"任务 {serial}: 已完成";
            
        string timeInfo = "";
        if (timeLimit == 0)
            timeInfo = " [立即]";
        else if (timeLimit == -1)
            timeInfo = " [无时限]";
        else
            timeInfo = $" [时限:{timeLimit}回合]";
            
        return $"任务 {serial}: 进行中{timeInfo}";
    }
}

/// <summary>
/// 任务评估结果枚举
/// </summary>
public enum TaskResult
{
    Success,            // 成功
    Failed,             // 失败
    InProgress,         // 进行中
    AlreadyCompleted    // 已完成
}