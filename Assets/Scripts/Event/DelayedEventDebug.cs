using UnityEngine;

/// <summary>
/// 间隔事件调试工具
/// 用于查看和测试延时事件队列
/// </summary>
public class DelayedEventDebug : MonoBehaviour
{
    void Update()
    {
        // 按 D 键显示当前延时事件队列
        if (Input.GetKeyDown(KeyCode.D))
        {
            ShowDelayedEventQueue();
        }
        
        // 按 Shift+D 添加测试延时事件
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
        {
            AddTestDelayedEvent();
        }
    }
    
    [ContextMenu("显示延时事件队列")]
    void ShowDelayedEventQueue()
    {
        if (EventManager.Instance == null || EventManager.Instance.stats == null)
        {
            Debug.LogError("[DelayedEventDebug] EventManager 或 stats 未初始化");
            return;
        }
        
        var queue = EventManager.Instance.stats.delayedEventQueue;
        
        Debug.Log("===== 延时事件队列 =====");
        Debug.Log($"[DelayedEventDebug] 当前年份: {GameControl.Instance?.year ?? 0}");
        Debug.Log($"[DelayedEventDebug] 队列长度: {queue.Count}");
        
        if (queue.Count == 0)
        {
            Debug.Log("[DelayedEventDebug] 队列为空");
        }
        else
        {
            // 按触发年份排序显示
            var sortedQueue = new System.Collections.Generic.List<DelayedEventData>(queue);
            sortedQueue.Sort((a, b) => a.triggerYear.CompareTo(b.triggerYear));
            
            foreach (var evt in sortedQueue)
            {
                int yearsLeft = evt.triggerYear - (GameControl.Instance?.year ?? 0);
                Debug.Log($"[DelayedEventDebug] - 事件ID: {evt.eventId}, 触发年份: {evt.triggerYear}, 剩余年数: {yearsLeft}");
            }
        }
        
        Debug.Log("========================");
    }
    
    [ContextMenu("添加测试延时事件")]
    void AddTestDelayedEvent()
    {
        if (EventManager.Instance == null || EventManager.Instance.stats == null)
        {
            Debug.LogError("[DelayedEventDebug] EventManager 或 stats 未初始化");
            return;
        }
        
        if (GameControl.Instance == null)
        {
            Debug.LogError("[DelayedEventDebug] GameControl 未初始化");
            return;
        }
        
        // 添加一个3年后触发的测试事件
        int triggerYear = GameControl.Instance.year + 3;
        string testEventId = "00101"; // 使用一个已知的事件ID
        
        EventManager.Instance.stats.delayedEventQueue.Add(new DelayedEventData(triggerYear, testEventId));
        
        Debug.Log($"[DelayedEventDebug] 已添加测试延时事件: ID={testEventId}, 触发年份={triggerYear}");
        ShowDelayedEventQueue();
    }
    
    [ContextMenu("清空延时事件队列")]
    void ClearDelayedEventQueue()
    {
        if (EventManager.Instance == null || EventManager.Instance.stats == null)
        {
            Debug.LogError("[DelayedEventDebug] EventManager 或 stats 未初始化");
            return;
        }
        
        int count = EventManager.Instance.stats.delayedEventQueue.Count;
        EventManager.Instance.stats.delayedEventQueue.Clear();
        Debug.Log($"[DelayedEventDebug] 已清空延时事件队列，清除了 {count} 个事件");
    }
}
