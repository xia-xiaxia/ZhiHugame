using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件选择器：负责事件抽取逻辑（优先级管理）
/// 延时事件存储在 StatModel.delayedEventQueue 中以支持存档
/// </summary>
public class EventSelector : MonoBehaviour
{
    public static EventSelector Instance;
    
    // 强制后继事件（一次性）
    private string nextEventId = "0";
    
    // 引用 StatModel（用于访问延时事件队列）
    private StatModel stats => EventManager.Instance?.stats;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    /// <summary>
    /// 决定下一个事件ID
    /// 优先级：延时事件 > 强制后继 > 随机抽取
    /// </summary>
    public string DetermineNextEventId(int currentYear)
    {
        if (stats == null || stats.delayedEventQueue == null)
        {
            Debug.LogError("[EventSelector] stats 或 delayedEventQueue 为 null");
            return PickRandomEvent();
        }
        
        // 1. 检查延时事件
        for (int i = stats.delayedEventQueue.Count - 1; i >= 0; i--)
        {
            var delayed = stats.delayedEventQueue[i];
            if (delayed.triggerYear <= currentYear)
            {
                stats.delayedEventQueue.RemoveAt(i);
                Debug.Log($"[EventSelector] 触发延时事件: {delayed.eventId}，触发年份: {delayed.triggerYear}，当前年份: {currentYear}");
                return delayed.eventId;
            }
        }

        // 2. 检查强制后继
        if (!string.IsNullOrEmpty(nextEventId) && nextEventId != "0")
        {
            string forced = nextEventId;
            nextEventId = "0";
            Debug.Log($"[EventSelector] 触发强制后继事件: {forced}");
            return forced;
        }

        // 3. 随机抽取未使用的事件
        return PickRandomEvent();
    }
    
    /// <summary>
    /// 随机抽取事件（内部方法）
    /// </summary>
    private string PickRandomEvent()
    {
        GameEvent randomEvent = EventDatabase.Instance?.GetRandomEventUnique();
        if (randomEvent != null)
        {
            Debug.Log($"[EventSelector] 随机抽取事件: {randomEvent.id}");
            return randomEvent.id;
        }

        Debug.LogWarning("[EventSelector] 没有可用事件了！");
        return "0";
    }

    /// <summary>
    /// 设置强制后继事件
    /// </summary>
    public void SetNextEventId(string eventId)
    {
        if (!string.IsNullOrEmpty(eventId) && eventId != "0")
        {
            nextEventId = eventId;
            Debug.Log($"[EventSelector] 设置后继事件: {eventId}");
        }
    }

    /// <summary>
    /// 添加延时事件
    /// </summary>
    public void AddDelayedEvent(int triggerYear, string eventId)
    {
        if (stats == null || stats.delayedEventQueue == null)
        {
            Debug.LogError("[EventSelector] stats 或 delayedEventQueue 为 null，无法添加延时事件");
            return;
        }
        
        if (!string.IsNullOrEmpty(eventId) && eventId != "0")
        {
            stats.delayedEventQueue.Add(new DelayedEventData(triggerYear, eventId));
            Debug.Log($"[EventSelector] 添加延时事件: {eventId}，触发年份: {triggerYear}，当前队列长度: {stats.delayedEventQueue.Count}");
        }
    }

    /// <summary>
    /// 重置所有状态（用于重新开始游戏）
    /// </summary>
    public void Reset()
    {
        if (stats != null && stats.delayedEventQueue != null)
        {
            stats.delayedEventQueue.Clear();
        }
        
        nextEventId = "0";
        
        // 重置事件池（EventDatabase 负责管理已使用状态）
        if (EventDatabase.Instance != null)
        {
            EventDatabase.Instance.ResetPool();
        }
        
        Debug.Log("[EventSelector] 已重置");
    }

    /// <summary>
    /// 从存档数据恢复状态（直接操作 stats.delayedEventQueue）
    /// </summary>
    public void LoadFromSave(List<DelayedEventData> delayedQueue)
    {
        if (stats == null || stats.delayedEventQueue == null)
        {
            Debug.LogError("[EventSelector] stats 或 delayedEventQueue 为 null");
            return;
        }
        
        stats.delayedEventQueue.Clear();
        if (delayedQueue != null)
        {
            stats.delayedEventQueue.AddRange(delayedQueue);
        }
        Debug.Log($"[EventSelector] 从存档加载 {stats.delayedEventQueue.Count} 个延时事件");
    }

    /// <summary>
    /// 获取当前状态用于存档（返回 stats.delayedEventQueue）
    /// </summary>
    public List<DelayedEventData> GetSaveData()
    {
        if (stats != null && stats.delayedEventQueue != null)
        {
            return new List<DelayedEventData>(stats.delayedEventQueue);
        }
        return new List<DelayedEventData>();
    }
    
    /// <summary>
    /// 获取强制后继事件ID（用于存档）
    /// </summary>
    public string GetNextEventId()
    {
        return nextEventId ?? "0";
    }
}