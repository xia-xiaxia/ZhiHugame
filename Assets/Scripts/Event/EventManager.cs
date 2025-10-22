using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using JsonA;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    public StatModel stats;
    public List<TextAsset> eventJsons;
    public GameObject palace;
    
    public AudioClip Bgm;
    public AudioSource audioSrc;

    private List<Dictionary<string, GameEvent>> randomEventList = new List<Dictionary<string, GameEvent>>();
    private List<int> activeRandomEventSetIndices = new List<int>();

    public int fileIndex = 0;

    // 下一个事件（仅选项强制指定时生效）
    private string nextEventId = "0";

    // 延时事件队列：每项 (触发年份, 事件ID)
    private List<(int triggerYear, string eventId)> delayedEvents = new List<(int, string)>();

    // 本局已出现过的事件集合（用于避免重复）
    private HashSet<string> usedEventIds = new HashSet<string>();
    // 白名单：每个事件集可抽取的事件ID集合
    private Dictionary<int, HashSet<string>> availableAllBySet = new Dictionary<int, HashSet<string>>();
    private Dictionary<int, HashSet<string>> available01BySet = new Dictionary<int, HashSet<string>>();

    private void Start()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateStatText();
    }

    void Awake()
    {
        Instance = this;
        LoadEvents();
    }

    void LoadEvents()
    {
        randomEventList.Clear();
        activeRandomEventSetIndices.Clear();
        usedEventIds.Clear();
        availableAllBySet.Clear();
        available01BySet.Clear();

        for (int i = 0; i < eventJsons.Count; i++)
        {
            var eventJson = eventJsons[i];
            if (eventJson == null)
            {
                Debug.LogWarning($"[EventManager] eventJson[{i}] 为 null");
                continue;
            }

            var currentEventDict = new Dictionary<string, GameEvent>();
            GameEvent[] all = JsonHelper.FromJson<GameEvent>(eventJson.text);
            foreach (var e in all)
            {
                if (!string.IsNullOrEmpty(e?.id))
                    currentEventDict[e.id] = e;
            }
            randomEventList.Add(currentEventDict);
            activeRandomEventSetIndices.Add(i);
            Debug.Log($"[EventManager] LoadEvents 随机事件集 {i} 加载 {currentEventDict.Count} 条");

            // 初始化白名单集合
            var allSet = new HashSet<string>(currentEventDict.Keys);
            var only01 = new HashSet<string>();
            foreach (var k in currentEventDict.Keys)
            {
                if (k.EndsWith("01")) only01.Add(k);
            }
            availableAllBySet[i] = allSet;
            available01BySet[i] = only01;
        }
    }

    public GameEvent GetEvent(string id, int randomEventSet = -1)
    {
        foreach (int idx in activeRandomEventSetIndices)
        {
            if (idx >= 0 && idx < randomEventList.Count &&
                randomEventList[idx].TryGetValue(id, out var ge))
            {
                fileIndex = idx;
                return ge;
            }
        }

        foreach (var dict in randomEventList)
        {
            if (dict != null && dict.TryGetValue(id, out var ge2))
            {
                Debug.LogWarning($"[EventManager] 事件 {id} 在未激活事件集中被找到");
                return ge2;
            }
        }

        Debug.LogError($"[EventManager] 未找到事件 {id}");
        return null;
    }

    // 标记一个事件为已使用
    public void MarkEventUsed(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            usedEventIds.Add(id);
            // 从所有白名单中移除
            foreach (var kv in availableAllBySet)
                kv.Value.Remove(id);
            foreach (var kv in available01BySet)
                kv.Value.Remove(id);
            // 使用后清理已耗尽的激活事件集
            CleanupExhaustedSets();
        }
    }

    // 查询事件是否已使用
    private bool IsEventUsed(string id)
    {
        return !string.IsNullOrEmpty(id) && usedEventIds.Contains(id);
    }

    // 判断某事件是否仍在白名单（可抽取）
    private bool IsAvailable(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        foreach (var hs in availableAllBySet.Values)
            if (hs.Contains(id)) return true;
        return false;
    }

    // 自动清理：若某激活事件集白名单耗尽，则自动隐藏该集
    private void CleanupExhaustedSets()
    {
        if (activeRandomEventSetIndices == null || activeRandomEventSetIndices.Count == 0) return;
        for (int i = activeRandomEventSetIndices.Count - 1; i >= 0; i--)
        {
            int idx = activeRandomEventSetIndices[i];
            if (availableAllBySet.TryGetValue(idx, out var hs))
            {
                if (hs == null || hs.Count == 0)
                {
                    activeRandomEventSetIndices.RemoveAt(i);
                    Debug.Log($"[EventManager] 自动隐藏事件集 {idx + 1}（白名单耗尽）");
                }
            }
        }
    }

    public void ApplyOption(Option opt, int year)
    {
        if (opt == null)
        {
            Debug.LogError("[EventManager] ApplyOption: opt 为 null");
            return;
        }
        if (stats == null)
        {
            Debug.LogError("[EventManager] ApplyOption: stats 未绑定");
            return;
        }

        stats.king += opt.kingChange;
        stats.noble += opt.nobleChange;
        stats.scholar += opt.scholarChange;
        stats.foreign += opt.foreignChange;
        stats.people += opt.peopleChange;

        Debug.Log("[EventManager] ApplyOption: " +
            $"K:{opt.kingChange} N:{opt.nobleChange} S:{opt.scholarChange} F:{opt.foreignChange} P:{opt.peopleChange}");
        if (opt.randomEventSet > 0)
        {
            int act = opt.randomEventSet - 1;
            if (act >= 0 && act < randomEventList.Count && !activeRandomEventSetIndices.Contains(act))
            {
                activeRandomEventSetIndices.Add(act);
                Debug.Log($"[EventManager] 激活事件集 {opt.randomEventSet}");

                // 为新激活的事件集准备白名单（若不存在）并剔除已使用
                if (!availableAllBySet.ContainsKey(act))
                {
                    var dict = randomEventList[act];
                    var allSet = new HashSet<string>(dict.Keys);
                    var only01 = new HashSet<string>();
                    foreach (var k in dict.Keys) if (k.EndsWith("01")) only01.Add(k);
                    availableAllBySet[act] = allSet;
                    available01BySet[act] = only01;
                }
                foreach (var used in usedEventIds)
                {
                    availableAllBySet[act].Remove(used);
                    available01BySet[act].Remove(used);
                }
            }
        }
        else if (opt.randomEventSet < 0)
        {
            int hide = -opt.randomEventSet - 1;
            if (hide >= 0 && activeRandomEventSetIndices.Contains(hide))
            {
                activeRandomEventSetIndices.Remove(hide);
                Debug.Log($"[EventManager] 隐藏事件集 {-opt.randomEventSet}");
            }
        }

        if (!string.IsNullOrEmpty(opt.nextEventId) && opt.nextEventId != "0")
        {
            if (opt.interval > 0)
            {
                int triggerYear = GameControl.Instance.year + opt.interval;
                delayedEvents.Add((triggerYear, opt.nextEventId));
                Debug.Log($"[EventManager] 延时插入事件 {opt.nextEventId}，将在第 {triggerYear} 年触发");
            }
            else
            {
                nextEventId = opt.nextEventId;
                Debug.Log($"[EventManager] 设定后继事件 {nextEventId}");
            }
        }


        UIManager.Instance?.UpdateStatText();
        UIManager.Instance?.ClearText();
        GameControl.Instance?.OnStatsChanged();
    }

    public string GetNextEventId() => nextEventId;

    public void SetNextEventId(string eventId, int file = 0)
    {
        if (string.IsNullOrEmpty(eventId) || eventId == "0") return;
        nextEventId = eventId;
        if (file != 0) fileIndex = file;
        Debug.Log($"[EventManager] SetNextEventId -> {eventId}");
    }

    // 核心决定逻辑
    public string DetermineNextEventId()
    {
        int currentYear = GameControl.Instance.year;
        // 清理白名单耗尽的激活事件集
        CleanupExhaustedSets();

        // 优先处理延时事件队列
        for (int i = 0; i < delayedEvents.Count; i++)
        {
            var (triggerYear, eventId) = delayedEvents[i];
            if (triggerYear <= currentYear)
            {
                delayedEvents.RemoveAt(i);
                if (!IsAvailable(eventId))
                {
                    Debug.Log($"[EventManager] 跳过不可抽取的延时事件 {eventId}");
                    i--; // 调整索引，继续检查后续
                    continue;
                }
                Debug.Log($"[EventManager] 触发延时事件 {eventId} 于第 {currentYear} 年");
                return eventId;
            }
        }

        // 强制后继（一次性）
        if (!string.IsNullOrEmpty(nextEventId) && nextEventId != "0")
        {
            string forced = nextEventId;
            nextEventId = "0";
            if (IsAvailable(forced))
                return forced;
            Debug.Log($"[EventManager] 跳过不可抽取的强制后继事件 {forced}");
        }

        // 没有后继决策：随机抽一个 00x 的事件，并跳到第一个决策 00x01
        return PickRandom01PatternEvent();
    }
    // 只挑选以 01 结尾的事件（00x01）
    private string PickRandom01PatternEvent()
    {
        // 清理白名单耗尽的激活事件集
        CleanupExhaustedSets();
        List<string> candidates = new List<string>();

        // 优先在激活事件集中挑选
        foreach (int idx in activeRandomEventSetIndices)
        {
            if (!available01BySet.TryGetValue(idx, out var hs) || hs == null) continue;
            foreach (var id in hs)
            {
                // 可选：避免非首回合再次进入开场事件
                if (GameControl.Instance != null && GameControl.Instance.year > 1 && id == "00101")
                    continue;
                candidates.Add(id);
            }
        }

        // 如果激活集中没有，则在全部事件中找
        if (candidates.Count == 0)
        {
            foreach (var kv in available01BySet)
            {
                var hs = kv.Value;
                if (hs == null) continue;
                foreach (var id in hs)
                {
                    if (GameControl.Instance != null && GameControl.Instance.year > 1 && id == "00101")
                        continue;
                    candidates.Add(id);
                }
            }
        }

        if (candidates.Count == 0)
        {
            // 兜底：没有任何 01 事件时，退回到通用随机
            Debug.LogWarning("[EventManager] 未找到任何未使用的 01 事件，回退到通用随机");
            return PickRandomEventFromActiveSets();
        }

        string pick = candidates[Random.Range(0, candidates.Count)];
        Debug.Log($"[EventManager] PickRandom01PatternEvent -> {pick}");
        return pick;
    }
    // 从激活集中随机
    private string PickRandomEventFromActiveSets()
    {
        // 清理白名单耗尽的激活事件集
        CleanupExhaustedSets();
        if (randomEventList == null || randomEventList.Count == 0)
            return "0";

        if (activeRandomEventSetIndices == null || activeRandomEventSetIndices.Count == 0)
        {
            activeRandomEventSetIndices = new List<int>();
            for (int i = 0; i < randomEventList.Count; i++)
                activeRandomEventSetIndices.Add(i);
        }

        // 从激活事件集的白名单中挑选
        List<(int setIdx, string id)> pool = new List<(int, string)>();
        foreach (int idx in activeRandomEventSetIndices)
        {
            if (!availableAllBySet.TryGetValue(idx, out var hs) || hs == null || hs.Count == 0) continue;
            foreach (var id in hs)
                pool.Add((idx, id));
        }
        if (pool.Count == 0)
        {
            Debug.LogWarning("[EventManager] 激活事件集中无可抽取事件");
            return "0";
        }
        var pick = pool[Random.Range(0, pool.Count)];
        Debug.Log($"[EventManager] 随机集 {pick.setIdx} -> {pick.id}");
        return pick.id;
    }

    

    public void HandleGameOver(string reason)
    {
        Debug.LogWarning($"[EventManager] 游戏失败: {reason}");
        LoadEvents();
        nextEventId = "0";

        if (stats != null)
            Debug.Log("[EventManager] 可在此重置数值（目前未重置）");

        UIManager.Instance?.UpdateStatText();
    }

    public void ReloadAllEventsForRestart()
    {
        nextEventId = "0";
        LoadEvents();
        usedEventIds.Clear();
    }

    public void OnRestartCleanup() { }

}