using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件选择器：负责事件抽取逻辑和白名单管理
/// </summary>
public class EventSelector : MonoBehaviour
{
    public static EventSelector Instance;
    
    // 已使用的事件集合（防重复）
    private HashSet<string> usedEventIds = new HashSet<string>();
    
    // 延时事件队列（年份, 事件ID）
    private List<(int triggerYear, string eventId)> delayedEvents = new List<(int, string)>();
    
    // 强制后继事件（一次性）
    private string nextEventId = "0";

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
        // 1. 检查延时事件
        for (int i = delayedEvents.Count - 1; i >= 0; i--)
        {
            var delayed = delayedEvents[i];
            if (delayed.triggerYear <= currentYear)
            {
                delayedEvents.RemoveAt(i);
                Debug.Log($"[EventSelector] 触发延时事件: {delayed.eventId}");
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
        GameEvent randomEvent = EventDatabase.Instance.GetRandomEventUnique();
        if (randomEvent != null)
        {
            MarkEventUsed(randomEvent.id);
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
        if (!string.IsNullOrEmpty(eventId) && eventId != "0")
        {
            delayedEvents.Add((triggerYear, eventId));
            Debug.Log($"[EventSelector] 添加延时事件: {eventId}，触发年份: {triggerYear}");
        }
    }

    /// <summary>
    /// 标记事件已使用
    /// </summary>
    public void MarkEventUsed(string eventId)
    {
        if (!string.IsNullOrEmpty(eventId))
        {
            usedEventIds.Add(eventId);
        }
    }

    /// <summary>
    /// 检查事件是否已使用
    /// </summary>
    public bool IsEventUsed(string eventId)
    {
        return usedEventIds.Contains(eventId);
    }

    /// <summary>
    /// 重置所有状态（用于重新开始游戏）
    /// </summary>
    public void Reset()
    {
        usedEventIds.Clear();
        delayedEvents.Clear();
        nextEventId = "0";
        EventDatabase.Instance.ResetPool();
        Debug.Log("[EventSelector] 已重置");
    }

    /// <summary>
    /// 从存档数据恢复状态
    /// </summary>
    public void LoadFromSave(List<string> usedIds, List<DelayedEventData> delayedQueue)
    {
        usedEventIds.Clear();
        if (usedIds != null)
        {
            foreach (var id in usedIds)
                usedEventIds.Add(id);
        }

        delayedEvents.Clear();
        if (delayedQueue != null)
        {
            foreach (var data in delayedQueue)
                delayedEvents.Add((data.triggerYear, data.eventId));
        }
    }

    /// <summary>
    /// 获取当前状态用于存档
    /// </summary>
    public (List<string> usedIds, List<DelayedEventData> delayedQueue) GetSaveData()
    {
        var usedList = new List<string>(usedEventIds);
        var delayedList = new List<DelayedEventData>();
        foreach (var (year, id) in delayedEvents)
        {
            delayedList.Add(new DelayedEventData(year, id));
        }
        return (usedList, delayedList);
    }
}