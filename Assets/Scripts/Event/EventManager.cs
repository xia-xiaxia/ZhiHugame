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

    // 专用：BUFF（时局）触发事件的独立数据源
    public TextAsset buffEventJson; // 指向一个仅存放BUFF触发事件的JSON
    private Dictionary<string, GameEvent> buffEvents = new Dictionary<string, GameEvent>();

    public int fileIndex = 0;

    // 下一个事件（仅选项强制指定时生效）
    private string nextEventId = "0";

    // 延时事件队列：每项 (触发年份, 事件ID)
    private List<(int triggerYear, string eventId)> delayedEvents = new List<(int, string)>();

    // 本局已出现过的事件集合（用于避免重复）
    // 修改：使用 (事件集索引, 事件ID) 的组合来跟踪，这样不同事件集中的相同ID事件可以分别抽取
    private HashSet<(int setIndex, string eventId)> usedEvents = new HashSet<(int, string)>();
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

    void Update()
    {
        // 调试信息：显示激活的事件集和可抽取事件总数
        int totalAvailable = 0;
        foreach (var kv in availableAllBySet)
        {
            totalAvailable += kv.Value.Count;
        }
        Debug.Log("当前激活的事件集有：" + string.Join(", ", activeRandomEventSetIndices));
        Debug.Log("当前可抽取的事件总数为：" + totalAvailable);
    }

    void LoadEvents()
    {
        randomEventList.Clear();
        activeRandomEventSetIndices.Clear();
        usedEvents.Clear();
        availableAllBySet.Clear();
        available01BySet.Clear();
        buffEvents.Clear();

        for (int i = 0; i < eventJsons.Count; i++)
        {
            var eventJson = eventJsons[i];
            if (eventJson == null)
            {
                Debug.LogWarning($"[EventManager] eventJson[{i}] 为 null");
                continue;
            }

            var currentEventDict = new Dictionary<string, GameEvent>();
            GameEvent[] all = new GameEvent[0];
            try
            {
                all = JsonHelper.FromJson<GameEvent>(eventJson.text);
            }
            catch (System.Exception ex)
            {
                string snippet = "";
                try { var t = eventJson.text ?? ""; snippet = t.Length > 200 ? t.Substring(0, 200) + "..." : t; } catch { snippet = "<unable to read text>"; }
                Debug.LogError($"[EventManager] 解析事件 JSON 失败: {eventJson.name} 异常: {ex.Message}\n片段: {snippet}");
                // 跳过当前文件，继续尝试其他文件
                continue;
            }
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
        // 优先从 fileIndex 指定的事件集中查找（抽取方法已经设置了正确的 fileIndex）
        if (fileIndex >= 0 && fileIndex < randomEventList.Count)
        {
            if (randomEventList[fileIndex].TryGetValue(id, out var gePreferred))
            {
                // 找到了，直接返回（fileIndex 保持不变）
                Debug.Log($"[EventManager] GetEvent({id}) 从事件集 {fileIndex} 中找到");
                return gePreferred;
            }
        }

        // 如果 fileIndex 指定的事件集中没有，再从所有事件集中查找
        for (int idx = 0; idx < randomEventList.Count; idx++)
        {
            if (idx >= 0 && idx < randomEventList.Count &&
                randomEventList[idx].TryGetValue(id, out var ge))
            {
                fileIndex = idx;
                Debug.Log($"[EventManager] GetEvent({id}) 从事件集 {idx} 中找到（回退查找）");
                return ge;
            }
        }

        Debug.LogError($"[EventManager] 未找到事件 {id}");
        return null;
    }

    // 标记一个事件为已使用（需要知道是哪个事件集的事件）
    public void MarkEventUsed(string id, int setIndex)
    {
        if (!string.IsNullOrEmpty(id))
        {
            var eventKey = (setIndex, id);
            if (usedEvents.Contains(eventKey))
            {
                Debug.LogWarning($"[EventManager] 事件 ({setIndex}, {id}) 已经被使用过了！可能出现重复事件BUG");
            }
            else
            {
                usedEvents.Add(eventKey);
                Debug.Log($"[EventManager] 标记事件为已使用: 事件集 {setIndex}, ID {id}");
            }
            
            // 只从对应事件集的白名单中移除
            if (availableAllBySet.TryGetValue(setIndex, out var allSet))
                allSet.Remove(id);
            if (available01BySet.TryGetValue(setIndex, out var set01))
                set01.Remove(id);
            // 使用后清理已耗尽的激活事件集
            CleanupExhaustedSets();
        }
    }

    // 查询某个事件集中的某个事件是否已使用
    private bool IsEventUsed(string id, int setIndex)
    {
        return !string.IsNullOrEmpty(id) && usedEvents.Contains((setIndex, id));
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

        // 检查特殊结局触发（骑马事件和盗匪事件）
        if (opt.kingChange == 999)
        {
            // 骑马事件触发结局
            Debug.Log("[EventManager] 触发骑马结局");
            if (GameControl.Instance != null)
            {
                GameControl.Instance.TriggerEnding("神", "你骑上了那匹马，的确驾驭不住——随后坠马而死");
            }
            return;
        }
        else if (opt.kingChange == 666)
        {
            // 盗匪事件触发结局
            Debug.Log("[EventManager] 触发盗匪结局");
            if (GameControl.Instance != null)
            {
                GameControl.Instance.TriggerEnding("献", "你冲到战场之上拼杀，随后被敌人一剑刺死");
            }
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
                // 从该事件集的白名单中移除已使用的事件
                foreach (var (setIdx, eventId) in usedEvents)
                {
                    if (setIdx == act)
                    {
                        availableAllBySet[act].Remove(eventId);
                        available01BySet[act].Remove(eventId);
                    }
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

        // 处理激活BUFF
        if (!string.IsNullOrEmpty(opt.activateBUFF))
        {
            if (BuffManager.Instance != null)
            {
                BuffDefinition buff = BuffManager.Instance.AddBuffById(opt.activateBUFF);
                if (buff != null)
                {
                    Debug.Log($"[EventManager] 激活BUFF: {buff.name}");
                }
                else
                {
                    Debug.LogWarning($"[EventManager] 未找到要激活的BUFF: {opt.activateBUFF}");
                }
            }
            else
            {
                Debug.LogError($"[EventManager] BuffManager.Instance 为 null，无法激活BUFF: {opt.activateBUFF}");
            }
        }

        // 处理后继事件
        if (!string.IsNullOrEmpty(opt.nextEventId) && opt.nextEventId != "0")
        {
            if (opt.interval > 0)
            {
                if (GameControl.Instance != null)
                {
                    int triggerYear = GameControl.Instance.year + opt.interval;
                    delayedEvents.Add((triggerYear, opt.nextEventId));
                    Debug.Log($"[EventManager] 延时插入事件 {opt.nextEventId}，将在第 {triggerYear} 年触发");
                }
                else
                {
                    Debug.LogError($"[EventManager] GameControl.Instance 为 null，无法处理延时事件");
                }
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

    // 使指定事件可被抽取：写入白名单（必要时初始化集合）
    public bool MakeEventAvailable(int setIndex, string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        if (setIndex < 0 || setIndex >= randomEventList.Count) return false;
        var dict = randomEventList[setIndex];
        if (dict == null || !dict.ContainsKey(id)) return false;

        if (!availableAllBySet.TryGetValue(setIndex, out var allSet) || allSet == null)
        {
            allSet = new HashSet<string>();
            availableAllBySet[setIndex] = allSet;
        }
        allSet.Add(id);

        if (!available01BySet.TryGetValue(setIndex, out var only01) || only01 == null)
        {
            only01 = new HashSet<string>();
            available01BySet[setIndex] = only01;
        }
        if (id.EndsWith("01"))
            only01.Add(id);

        return true;
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
            // 若在普通白名单可抽取，或为 BUFF 专用事件（允许绕过白名单）
            if (IsAvailable(forced) || (buffEvents != null && buffEvents.ContainsKey(forced)))
                return forced;
            Debug.Log($"[EventManager] 跳过不可抽取的强制后继事件 {forced}");
        }

        // 没有后继决策：随机抽一个 00x 的事件，并跳到第一个决策 00x01
        return PickRandom01PatternEvent();
    }
    // 只挑选以 01 结尾的事件（0??01 格式，例如：00101, 00201, 00301）
    // 新逻辑：从所有未使用的事件中随机抽取（不再限制激活事件集）
    // 白名单机制会自动排除已使用的事件，确保只抽取未使用的事件
    private string PickRandom01PatternEvent()
    {
        // 注释掉激活事件集的清理和限制，改为从所有事件集中抽取
        // CleanupExhaustedSets();
        
        // 注释掉激活事件集的检查
        // if (activeRandomEventSetIndices == null || activeRandomEventSetIndices.Count == 0)
        // {
        //     Debug.LogWarning("[EventManager] 没有激活的事件集");
        //     return "0";
        // }

        // 从所有事件集中收集可用的 01 事件（不再限制激活事件集）
        List<(int setIdx, string id)> pool = new List<(int, string)>();
        
        // 遍历所有事件集（不再限制为激活的事件集）
        for (int idx = 0; idx < randomEventList.Count; idx++)
        {
            if (available01BySet.TryGetValue(idx, out var hs) && hs != null && hs.Count > 0)
            {
                foreach (var id in hs)
                {
                    pool.Add((idx, id));
                }
            }
        }

        if (pool.Count == 0)
        {
            Debug.LogWarning("[EventManager] 所有事件集中没有可用的 0??01 格式事件，回退到通用随机");
            return PickRandomEventFromActiveSets();
        }

        // 从所有可用的 01 事件中随机选择一个
        var selected = pool[Random.Range(0, pool.Count)];
        int selectedSetIndex = selected.setIdx;
        string selectedId = selected.id;

        // 设置 fileIndex，这样后续的 GetEvent() 和 MarkEventUsed() 能够正确识别事件集
        fileIndex = selectedSetIndex;

        Debug.Log($"[EventManager] PickRandom01PatternEvent -> 事件集 {selectedSetIndex}, ID: {selectedId} (格式: 0??01), 总池大小: {pool.Count}");
        return selectedId;
    }
    // 从所有事件集中随机（不再限制激活事件集）
    private string PickRandomEventFromActiveSets()
    {
        // 注释掉激活事件集的清理和限制
        // CleanupExhaustedSets();
        // Debug.Log("激活的事件集有：[" + string.Join(", ", activeRandomEventSetIndices) + "]");
        
        if (randomEventList == null || randomEventList.Count == 0)
            return "0";

        // 注释掉激活事件集的初始化检查
        // if (activeRandomEventSetIndices == null || activeRandomEventSetIndices.Count == 0)
        // {
        //     activeRandomEventSetIndices = new List<int>();
        //     for (int i = 0; i < randomEventList.Count; i++)
        //         activeRandomEventSetIndices.Add(i);
        // }

        // 从所有事件集的白名单中挑选（不再限制激活事件集）
        List<(int setIdx, string id)> pool = new List<(int, string)>();
        for (int idx = 0; idx < randomEventList.Count; idx++)
        {
            if (!availableAllBySet.TryGetValue(idx, out var hs) || hs == null || hs.Count == 0) continue;
            foreach (var id in hs)
                pool.Add((idx, id));
        }
        if (pool.Count == 0)
        {
            Debug.LogWarning("[EventManager] 所有事件集中无可抽取事件");
            return "0";
        }
        var pick = pool[Random.Range(0, pool.Count)];
        
        // 设置 fileIndex，这样后续的 GetEvent() 和 MarkEventUsed() 能够正确识别事件集
        fileIndex = pick.setIdx;
        
        Debug.Log($"[EventManager] 随机集 {pick.setIdx} -> {pick.id}, 总池大小: {pool.Count}");
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
        LoadEvents();  // LoadEvents() 已经会清理 usedEvents 和重置白名单
        // 不需要再次清理 usedEvents，因为 LoadEvents() 已经做了
    }

    public void OnRestartCleanup() { }

}