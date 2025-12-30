using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class EventDatabase : MonoBehaviour
{
    public static EventDatabase Instance;

    // 存储所有事件，Key 是处理后的 "文件名_ID"
    private Dictionary<string, GameEvent> globalEventDict = new Dictionary<string, GameEvent>();
    
    // 随机池：存储处理后的 "文件名_ID"，用于随机抽取
    private List<string> availableEventIds = new List<string>();

    public List<TextAsset> eventJsons;
    
    // 当前激活的事件集（用于持久化保存）
    private List<TextAsset> activeEventJsons = new List<TextAsset>();

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
            Debug.Log("[EventDatabase] 已创建实例，等待从存档加载事件集状态");
        }
    }

    public void LoadEvents(List<TextAsset> availableJsons = null)
    {
        if (availableJsons == null)
        {
            availableJsons = activeEventJsons.Count > 0 ? activeEventJsons : eventJsons;
        }

        globalEventDict.Clear();
        availableEventIds.Clear();

        foreach (var jsonAsset in availableJsons)
        {
            if (jsonAsset == null) continue;

            string fileName = jsonAsset.name; // 使用文件名作为命名空间
            GameEvent[] events = JsonHelper.FromJson<GameEvent>(jsonAsset.text);

            if (events == null) continue;

            foreach (var evt in events)
            {
                // 在内存中修改 ID，避免冲突，且无需修改类定义

                // 1. 记录原始 ID (用于调试)
                string originalId = evt.id;
                
                // 2. 生成全局唯一 ID (文件名_原始ID)
                string uniqueId = $"{fileName}_{originalId}";

                // 3. 修改内存中对象的 ID
                evt.id = uniqueId;

                // 4. 处理选项 (Option) 中的连接关系
                if (evt.options != null)
                {
                    foreach (var opt in evt.options)
                    {
                        if (!string.IsNullOrEmpty(opt.nextEventId))
                        {
                            // 如果 nextEventId 还没包含前缀，就加上前缀
                            // (这里加个判断防止你手动写了跨文件跳转)
                            if (!opt.nextEventId.Contains("_")) 
                            {
                                // 智能补全ID：如果不足5位数，从当前事件ID取前3位补全
                                string nextId = opt.nextEventId;
                                if (nextId.Length < 5 && originalId.Length >= 3)
                                {
                                    // 从当前事件ID取前3位 + nextEventId，补全为5位
                                    string prefix = originalId.Substring(0, 3);
                                    nextId = prefix + nextId.PadLeft(2, '0');
                                    Debug.Log($"[EventDatabase] ID自动补全: {opt.nextEventId} -> {nextId} (基于当前事件 {originalId})");
                                }
                                opt.nextEventId = $"{fileName}_{nextId}";
                            }
                        }
                    }
                }

                // 存入字典和随机池
                if (!globalEventDict.ContainsKey(uniqueId))
                {
                    globalEventDict.Add(uniqueId, evt);
                    availableEventIds.Add(uniqueId);
                }
                else
                {
                    Debug.LogError($"[EventDatabase] ID冲突警告: 文件 {fileName} 中的事件 {originalId} 在合并后重复，请检查。");
                }
            }
        }

        Debug.Log($"加载完毕。事件库大小: {globalEventDict.Count}，随机池剩余: {availableEventIds.Count}");
    }

    /// <summary>
    /// 获取当前激活的事件总数
    /// </summary>
    public int GetTotalEventCount()
    {
        return globalEventDict.Count;
    }

    // 根据 ID 获取事件 (这里的 ID 必须是带前缀的唯一 ID)
    public GameEvent GetEvent(string globalId)
    {
        if (string.IsNullOrEmpty(globalId)) return null;

        if (globalEventDict.TryGetValue(globalId, out GameEvent evt))
        {
            return evt;
        }
        else
        {
            Debug.LogWarning($"[EventDatabase] 找不到事件: {globalId}");
            return null;
        }
    }

    // 随机抽取一个不重复的事件
    public GameEvent GetRandomEventUnique()
    {
        if (availableEventIds.Count == 0)
        {
            Debug.Log("没有更多随机事件了！");
            return null;
        }
        bool isEndWithOne = false;
        while (!isEndWithOne)
        {
            // 1. 随机选
            int index = Random.Range(0, availableEventIds.Count);
            string pickedId = availableEventIds[index];

            if (!string.IsNullOrEmpty(pickedId) && pickedId[^1] == '1')
            {
                Debug.Log($"[EventDatabase] 抽取到事件: {pickedId}");
                isEndWithOne = true;
                // 2. 从池中移除 (洗牌移除法，效率最高)
                availableEventIds[index] = availableEventIds[availableEventIds.Count - 1];
                availableEventIds.RemoveAt(availableEventIds.Count - 1);

                // 3. 返回
                return globalEventDict[pickedId];
            }
        }
        return null;
    }
    
    // 把事件放回池子（例如读档重置时）
    public void ResetPool()
    {
        availableEventIds.Clear();
        foreach(var key in globalEventDict.Keys)
        {
            availableEventIds.Add(key);
        }
        Debug.Log($"[EventDatabase] 事件池已重置，可用事件: {availableEventIds.Count}");
    }

    /// <summary>
    /// 获取已使用的事件ID列表（用于存档）
    /// </summary>
    public List<string> GetUsedEventIds()
    {
        List<string> usedIds = new List<string>();
        foreach (var id in globalEventDict.Keys)
        {
            if (!availableEventIds.Contains(id))
            {
                usedIds.Add(id);
            }
        }
        return usedIds;
    }

    /// <summary>
    /// 从存档恢复已使用事件状态
    /// </summary>
    public void RestoreUsedEvents(List<string> usedIds)
    {
        if (usedIds == null) return;

        // 先重置池子
        ResetPool();

        // 移除已使用的事件
        foreach (var usedId in usedIds)
        {
            availableEventIds.Remove(usedId);
        }

        Debug.Log($"[EventDatabase] 从存档恢复，移除 {usedIds.Count} 个已使用事件，剩余 {availableEventIds.Count} 个可用");
    }
    /// <summary>
    /// 根据事件集ID激活或关闭事件集
    /// </summary>
    public void ActivateEventSetById(int[] randomEventSet)
    {
        if (randomEventSet == null || randomEventSet.Length == 0)
        {
            Debug.LogWarning("[EventDatabase] 传入的事件集数组为空，不执行任何操作");
            return;
        }

        foreach (var fileid in randomEventSet)
        {
            if(fileid < 0)
            {
                int index = -fileid - 1;
                if(index >= 0 && index < eventJsons.Count)
                {
                    activeEventJsons.Remove(eventJsons[index]);
                    Debug.Log($"[EventDatabase] 关闭事件集: {eventJsons[index].name}");
                }
                else
                {
                    Debug.LogWarning($"[EventDatabase] 关闭事件集失败: 索引 {index} 越界");
                }
            }
            else if(fileid > 0)
            {
                int index = fileid - 1;
                if(index >= 0 && index < eventJsons.Count)
                {
                    if(!activeEventJsons.Contains(eventJsons[index]))
                    {
                        activeEventJsons.Add(eventJsons[index]);
                        Debug.Log($"[EventDatabase] 激活事件集: {eventJsons[index].name}");
                    }
                } 
                else
                {
                    Debug.LogWarning($"[EventDatabase] 激活事件集失败: 索引 {index} 越界");
                }
            }
            else
            {
                Debug.LogWarning($"[EventDatabase] 跳过无效的事件集ID: {fileid} (0表示无操作)");
            }
        }
        LoadEvents(activeEventJsons);
        Debug.Log($"[EventDatabase] 事件集激活状态已更新并持久化");
    }
    
    /// <summary>
    /// 获取当前激活的事件集索引（用于存档）
    /// </summary>
    public List<int> GetActiveEventSetIndices()
    {
        List<int> activeIndices = new List<int>();
        foreach (var activeJson in activeEventJsons)
        {
            int index = eventJsons.IndexOf(activeJson);
            if (index >= 0)
            {
                activeIndices.Add(index);
            }
        }
        Debug.Log($"[EventDatabase] 获取激活事件集索引: 共 {activeIndices.Count} 个");
        return activeIndices;
    }
    
    /// <summary>
    /// 从存档恢复激活的事件集状态
    /// </summary>
    public void RestoreActiveEventSet(List<int> activeIndices)
    {
        if (activeIndices == null || activeIndices.Count == 0)
        {
            Debug.LogWarning("[EventDatabase] 恢复事件集失败: 索引列表为空，将激活所有事件集");
            activeEventJsons = new List<TextAsset>(eventJsons);
            LoadEvents(activeEventJsons);
            return;
        }
        
        activeEventJsons.Clear();
        foreach (var index in activeIndices)
        {
            if (index >= 0 && index < eventJsons.Count)
            {
                activeEventJsons.Add(eventJsons[index]);
            }
            else
            {
                Debug.LogWarning($"[EventDatabase] 恢复事件集时索引 {index} 越界，跳过");
            }
        }
        
        LoadEvents(activeEventJsons);
        Debug.Log($"[EventDatabase] 从存档恢复事件集激活状态: {activeEventJsons.Count} 个事件集已激活");
    }

    public void ActivateEventSetById(int randomEventSet)
    {
        if (randomEventSet < 0)
        {
            int index = -randomEventSet - 1;
            if (index >= 0 && index < eventJsons.Count)
            {
                activeEventJsons.Remove(eventJsons[index]);
                Debug.Log($"[EventDatabase] 关闭事件集: {eventJsons[index].name}");
            }
            else
            {
                Debug.LogWarning($"[EventDatabase] 关闭事件集失败: 索引 {index} 越界");
            }
        }
        else if (randomEventSet > 0)
        {
            int index = randomEventSet - 1;
            if (index >= 0 && index < eventJsons.Count)
            {
                if (!activeEventJsons.Contains(eventJsons[index]))
                {
                    activeEventJsons.Add(eventJsons[index]);
                    Debug.Log($"[EventDatabase] 激活事件集: {eventJsons[index].name}");
                }
            }
            else
            {
                Debug.LogWarning($"[EventDatabase] 激活事件集失败: 索引 {index} 越界");
            }
        }
        LoadEvents(activeEventJsons);
        Debug.Log($"[EventDatabase] 单个事件集激活状态已更新");
    }
}