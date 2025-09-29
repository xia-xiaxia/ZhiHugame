using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    public StatModel stats;      // 拖到 Inspector
    public TextAsset historyEventJson;  // 历史事件JSON文件
    public GameObject palace;
    public GameObject qte;
    public Image qteimage;
    public Sprite tlimage;
    public Sprite zzimage;
    public Sprite jsimage;
    public Sprite hmimage;
    public GameObject tljy;
    public GameObject hmjy;
    public GameObject zzjy;
    public GameObject jsjy;
    public Transform pandingwenben;
    public AudioClip tlBgm;
    public AudioClip jsBgm;
    public AudioClip hmBgm;
    public AudioClip zzBgm;
    public AudioClip Bgm;
    public AudioSource audioSrc;
    public AudioClip zhong;
    public bool isHistoryEvent;

    // 分离的事件字典
    public Dictionary<string, HistoryEvent> historyEvents = new Dictionary<string, HistoryEvent>();
    public Dictionary<string, RandomEvent> randomEvents = new Dictionary<string, RandomEvent>();
    public List<Dictionary<string, RandomEvent>> randomEventpool = new List<Dictionary<string, RandomEvent>>(); 

    public int dcb = 1;

    // 事件ID生成相关
    private string nextEventId = "00101"; // 下一个要显示的事件ID
    public int randomID = 0;

    // 历史事件间隔控制
    private int randomEventCounter = 0; // 当前已经过的随机事件数量
    private string pendingHistoryEventId = ""; // 待触发的历史事件ID
    private int requiredRandomEvents = 0; // 需要的随机事件间隔数

    void Awake()
    {
        Instance = this;
        UIManager.Instance.UpdateStatText();
    }

    private IEnumerator Start()
    {
        // 等待 RandomListControler 初始化
        while (RandomListControler.Instance == null)
        {
            yield return null;
        }
        RandomListControler.Instance.LoadRandomEvents();

        // 等待 randomEventpool 初始化完成
        while (RandomListControler.Instance.randomEventpool == null)
        {
            yield return null;
        }

        LoadEvents();
        Debug.Log($"[EventManager] Start: 总共加载了 {historyEvents.Count} 个历史事件和 {randomEvents.Count} 个随机事件");
        GetRandomEvent("00101",1);
    }

    void LoadEvents()
    {
        // 加载历史事件
        if (historyEventJson != null)
        {
            HistoryEvent[] historyEventArray = JsonHelper.FromJson<HistoryEvent>(historyEventJson.text);
            if (historyEventArray == null)
            {
                Debug.LogError("解析历史事件JSON失败，结果为null，请检查JSON格式和内容！");
                return;
            }
            foreach (var evt in historyEventArray)
            {
                if (evt == null)
                {
                    Debug.LogError("解析出的历史事件为 null，请检查 JSON 数据格式！");
                    continue;
                }
                if (string.IsNullOrEmpty(evt.id))
                {
                    Debug.LogError("历史事件 id 为空，请检查 JSON 数据！");
                    continue;
                }
                historyEvents[evt.id] = evt;
                Debug.Log($"加载历史事件: {evt.id}");
            }
            Debug.Log($"总共加载了 {historyEvents.Count} 个历史事件");
        }
        else
        {
            Debug.LogError("历史事件JSON文件未设置！");
        }

        // 加载随机事件
        if (RandomListControler.Instance == null)
        {
            Debug.LogError("RandomListControler.Instance 未初始化！");
            return;
        }
        if (RandomListControler.Instance.randomEventpool == null)
        {
            Debug.LogError("RandomListControler.Instance.randomEventpool 未初始化！");
            return;
        }
        randomEventpool = RandomListControler.Instance.randomEventpool;
        Debug.Log($"总共加载了 {randomEventpool.Count} 个人的随机事件");

        // 检查 randomEventpool 是否为空或内部元素为 null
        if (randomEventpool.Count == 0)
        {
            Debug.LogError("randomEventpool 为空！");
        }
        else
        {
            for (int i = 0; i < randomEventpool.Count; i++)
            {
                if (randomEventpool[i] == null)
                {
                    Debug.LogError($"randomEventpool[{i}] 为 null！");
                }
                else
                {
                    Debug.Log($"randomEventpool[{i}] 包含 {randomEventpool[i].Count} 个事件");
                }
            }
        }
    }

    // 供 UI 调用：返回某个事件（统一接口，自动判断事件类型）ss

    // 获取历史事件
    public HistoryEvent GetHistoryEvent(string id)
    {
        if (historyEvents.ContainsKey(id))
        {
            isHistoryEvent = true;
            return historyEvents[id];
        }
        else
        {
            Debug.LogError($"历史事件ID '{id}' 不存在!");
            return null;
        }
    }

    // 获取随机事件
    public RandomEvent GetRandomEvent(string id, int index)
    {
        if (randomEventpool == null || randomEventpool.Count == 0)
        {
            Debug.LogError("randomEventpool 为空或未初始化！");
            return null;
        }

        if (index < 0 || index >= randomEventpool.Count)
        {
            Debug.LogError($"随机事件池中没有索引 {index} 的事件集，使用默认索引 0");
            index = 0;
        }

        if (randomEventpool[index] == null)
        {
            Debug.LogError($"randomEventpool[{index}] 为 null！");
            return null;
        }

        randomEvents = randomEventpool[index];

        if (randomEvents.ContainsKey(id))
        {
            isHistoryEvent = false;
            return randomEvents[id];
        }
        else
        {
            Debug.LogError($"随机事件ID '{id}' 不存在!");
            return null;
        }
    }

    // 玩家点了某个选项后调用
    public void ApplyOption(Option opt, int turns)
    {
        stats.gold += opt.goldChange;
        Debug.Log("goldchange");
        stats.people += opt.peopleChange;
        Debug.Log("peoplechange");
        stats.zhouli += opt.zhouLiChange;
        Debug.Log("zhoulichange");
        stats.weiwang += opt.weiwangChange;
        Debug.Log("weiwangchange");

        // 只有当nextEventId不为"0"时才设置，为"0"时保持默认值，让系统选择随机事件
        if (opt.nextEventId != "0")
        {
            nextEventId = opt.nextEventId;
            Debug.Log($"[EventManager] ApplyOption: 设置下一个事件ID为 {opt.nextEventId}");
        }
        else
        {
            Debug.Log($"[EventManager] ApplyOption: 选项nextEventId为0，将选择随机事件");
        }

        // 处理历史事件间隔逻辑
        if (opt.Intervals > 0 && !string.IsNullOrEmpty(opt.nextEventId) && opt.nextEventId != "0")
        {
            // 设置待触发的历史事件
            pendingHistoryEventId = opt.nextEventId;
            requiredRandomEvents = opt.Intervals;
            randomEventCounter = 0; // 重置计数器
            isHistoryEvent = true;

            // 清除即时nextEventId，让系统先显示随机事件
            nextEventId = "00101";

            Debug.Log($"[EventManager] ApplyOption: 设置历史事件间隔触发 - 事件ID: {pendingHistoryEventId}, 需要间隔: {requiredRandomEvents} 个随机事件");
        }

        UIManager.Instance.UpdateStatText();
        UIManager.Instance.ClearText();
    }

    // ===== 事件ID生成逻辑 =====

    /// <summary>
    /// 获取下一个事件ID
    /// </summary>
    public string GetNextEventId()
    {
        return nextEventId;
    }

    /// <summary>
    /// 设置下一个事件ID
    /// </summary>
    public void SetNextEventId(string eventId)
    {
        if (eventId == "0") return;
        nextEventId = eventId;
        Debug.Log($"[EventManager] SetNextEventId: {eventId}");
    }

    public string DetermineNextEventId()
    {
        int currentTurn = GameControl.Instance.turns;

        // 检查是否有任务完成事件
        if (GameControl.Instance.IsCompleteTask)
        {
            GameControl.Instance.IsCompleteTask = false; // 重置任务状态
            Debug.Log($"[EventManager] DetermineNextEventId: 任务完成事件 00101");
            return "00101"; // 任务完成事件
        }

        // 第一回合显示开场事件（每个时代的既定事件）
        if (currentTurn == 1)
        {
            Debug.Log($"[EventManager] DetermineNextEventId: 开场事件 00101");
            return "00101";
        }

        // 检查是否有预设的下一个事件ID（非主线回合且非"0"）
        if (!string.IsNullOrEmpty(GetNextEventId()) && GetNextEventId() != "0" && GetNextEventId() != "100")
        {
            string result = GetNextEventId();
            SetNextEventId("00101"); // 重置
            Debug.Log($"[EventManager] DetermineNextEventId: 使用预设事件ID {result}");
            return result;
        }

        // 检查是否需要触发间隔历史事件
        if (!string.IsNullOrEmpty(pendingHistoryEventId) && randomEventCounter >= requiredRandomEvents)
        {
            string historyEventToTrigger = pendingHistoryEventId;

            // 重置间隔事件状态
            pendingHistoryEventId = "";
            requiredRandomEvents = 0;
            randomEventCounter = 0;

            Debug.Log($"[EventManager] DetermineNextEventId: 触发间隔历史事件 {historyEventToTrigger}，已过随机事件数: {randomEventCounter}");
            return historyEventToTrigger;
        }

        // 4的倍数回合显示主线事件
        if (currentTurn % 4 == 0 && currentTurn > 0)
        {
            string mainEventId = "";

            if (currentTurn == 4)
            {
                // 第一个主线事件ID固定
                mainEventId = "00101";
                Debug.Log($"[EventManager] DetermineNextEventId: 第一个主线事件 {mainEventId}");
            }
            else
            {
                // 后续主线事件：如果有预设的下一个事件ID，使用它
                if (!string.IsNullOrEmpty(GetNextEventId()) && GetNextEventId() != "0" && GetNextEventId() != "00101")
                {
                    mainEventId = GetNextEventId();
                    SetNextEventId("00101"); // 重置
                    Debug.Log($"[EventManager] DetermineNextEventId: 后续主线事件 {mainEventId}");
                }
                else
                {
                    // 如果没有预设事件ID，说明主线任务链断了，继续随机事件
                    Debug.LogWarning($"[EventManager] 回合 {currentTurn} 应该是主线事件，但没有预设的事件ID，使用随机事件");
                    return GenerateRandomEventId(true);
                }
            }

            // 检查主线事件ID是否存在（在历史事件中）
            var testEvent = GetHistoryEvent(mainEventId);
            if (testEvent != null)
            {
                return mainEventId;
            }
            else
            {
                Debug.LogWarning($"计算的主线事件ID '{mainEventId}' 不存在，使用随机事件");
                return GenerateRandomEventId(true);
            }
        }

        // 其他情况显示随机事件
        return GenerateRandomEventId(false);
    }

    /// <summary>
    /// 生成随机事件ID并处理间隔计数
    /// </summary>
    private string GenerateRandomEventId(bool isMainEventFallback)
    {
        int randomId = Random.Range(0,6);
        int randomIdex = Random.Range(0, 0);
        randomID = randomIdex;
        string eventId = randomId.ToString("000") + "01";

        // 检查随机事件ID是否存在
        var testEvent = GetRandomEvent(eventId, randomID);
        if (testEvent == null)
        {
            // 如果计算的随机事件不存在，尝试找一个存在的随机事件
            foreach (var kvp in randomEvents)
            {
                if (kvp.Key.StartsWith("0"))
                {
                    eventId = kvp.Key;
                    break;
                }
            }
        }

        // 如果有待触发的间隔历史事件，增加随机事件计数器
        if (!string.IsNullOrEmpty(pendingHistoryEventId))
        {
            randomEventCounter++;
            string logType = isMainEventFallback ? "随机事件（主线缺失替代）" : "随机事件";
            Debug.Log($"[EventManager] DetermineNextEventId: {logType} {eventId}，间隔计数: {randomEventCounter}/{requiredRandomEvents}");
        }
        else
        {
            string logType = isMainEventFallback ? "随机事件（主线缺失替代）" : "随机事件";
            Debug.Log($"[EventManager] DetermineNextEventId: {logType} {eventId}");
        }

        return eventId;
    }

    // ===== 间隔历史事件辅助方法 =====

    /// <summary>
    /// 获取当前间隔事件状态信息
    /// </summary>
    public string GetIntervalEventStatus()
    {
        if (string.IsNullOrEmpty(pendingHistoryEventId))
        {
            return "无待触发的间隔历史事件";
        }

        return $"待触发事件: {pendingHistoryEventId}, 进度: {randomEventCounter}/{requiredRandomEvents}";
    }

    /// <summary>
    /// 强制触发间隔历史事件（调试用）
    /// </summary>
    public void ForceTriggerpendingHistoryEvent()
    {
        if (!string.IsNullOrEmpty(pendingHistoryEventId))
        {
            SetNextEventId(pendingHistoryEventId);
            pendingHistoryEventId = "";
            requiredRandomEvents = 0;
            randomEventCounter = 0;
            Debug.Log($"[EventManager] 强制触发间隔历史事件");
        }
    }

    // ===== 调试和统计方法 =====

    /// <summary>
    /// 获取事件统计信息
    /// </summary>
    public void LogEventStats()
    {
        Debug.Log($"历史事件数量: {historyEvents.Count}");
        Debug.Log($"随机事件数量: {randomEvents.Count}");
        Debug.Log($"当前间隔事件状态: {GetIntervalEventStatus()}");
    }
}