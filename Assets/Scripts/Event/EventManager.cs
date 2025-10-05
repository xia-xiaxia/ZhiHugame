using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    public StatModel1 stats;      // 拖到 Inspector
    public List<TextAsset> eventJsons;  // 把 JSON 文件拖到这里
    public TextAsset historyEventJson;  // 把 JSON 文件拖到这里
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
    // public FadePulseOnce target;
    private Dictionary<string, GameEvent> randomEvents = new Dictionary<string, GameEvent>();
    private Dictionary<string, GameEvent> historyEvents = new Dictionary<string, GameEvent>();

    private List<Dictionary<string,GameEvent>> randomEventList = new List<Dictionary<string,GameEvent>>();
    private List<int> activeRandomEventSetIndices = new List<int>();

    public bool ishistoryEvent = false;

    // public DaChenMove dc1;
    // public DaChenMove dc2;
    // public DaChenMove dc3;
    public int dcb = 1;

    // 事件ID生成相关
    private string nextEventId = "000"; // 下一个要显示的事件ID

    private void Start()
    {
        UIManager.Instance.UpdateStatText();
    }
    void Awake()
    {
        Instance = this;
        LoadEvents();
    }

    void LoadEvents()
    {
        // 直接从 JSON 文件中加载事件
        randomEventList.Clear();
        activeRandomEventSetIndices.Clear();

        for (int i = 0; i < eventJsons.Count; i++)
        {
            var eventJson = eventJsons[i];
            var currentEventDict = new Dictionary<string, GameEvent>();
            GameEvent[] all = JsonHelper.FromJson<GameEvent>(eventJson.text);

            foreach (var e in all)
            {
                if (!string.IsNullOrEmpty(e?.id))
                    currentEventDict[e.id] = e;
            }
            randomEventList.Add(currentEventDict);
            // 默认激活全部事件集
            activeRandomEventSetIndices.Add(i);
            Debug.Log($"[EventManager] LoadEvents: 加载事件集 {i} (count={currentEventDict.Count})");
        }

        // 历史事件加载（如果有）保持原样
        if (historyEventJson != null)
        {
            historyEvents.Clear();
            var his = JsonHelper.FromJson<GameEvent>(historyEventJson.text);
            foreach (var e in his) if (!string.IsNullOrEmpty(e?.id)) historyEvents[e.id] = e;
            Debug.Log($"[EventManager] LoadEvents: 加载历史事件 count={historyEvents.Count}");
        }
    }

    // 供 UI 调用：返回某个事件
    public GameEvent GetEvent(string id, int randomEventSet = -1)
    {
        if (ishistoryEvent)
        {
            if (historyEvents.ContainsKey(id))
            {
                Debug.Log($"[EventManager] 获取历史事件ID: {id}");
                return historyEvents[id];
            }
            else
            {
                Debug.LogError($"历史事件ID '{id}' 不存在于历史事件字典中!");
                return null;
            }
        }
        
        // 尝试从所有激活的随机事件集中查找
        foreach (int index in activeRandomEventSetIndices)
        {
            if (index < randomEventList.Count && randomEventList[index].ContainsKey(id))
            {
                return randomEventList[index][id];
            }
        }

        // 如果找不到，作为后备方案，尝试从所有随机事件集中查找
        foreach (var eventDict in randomEventList)
        {
            if (eventDict.ContainsKey(id))
            {
                Debug.LogWarning($"事件ID '{id}' 在非激活的事件集中找到。");
                return eventDict[id];
            }
        }

        Debug.LogError($"事件ID '{id}' 在任何事件字典中都不存在!");
        return null;
    }

    // 玩家点了某个选项后调用
    public void ApplyOption(Option opt,int turns)
    {
        // 安全检查
        if (opt == null)
        {
            Debug.LogError("[EventManager] ApplyOption: 参数 opt 为 null，操作中止");
            return;
        }
        if (stats == null)
        {
            Debug.LogError("[EventManager] ApplyOption: stats 未绑定（请在 Inspector 中把 StatModel1 拖入 EventManager.stats）");
            return;
        }

        // 应用数值变化
        stats.king += opt.kingChange;
        stats.noble += opt.nobleChange;
        stats.scholar += opt.scholarChange;
        stats.foreign += opt.foreignChange;
        stats.people += opt.peopleChange;

        // --- 处理 randomEventSet 逻辑 ---
        if (opt.randomEventSet > 0)
        {
            int activateIndex = opt.randomEventSet - 1;
            if (activateIndex >= 0 && activateIndex < randomEventList.Count)
            {
                if (!activeRandomEventSetIndices.Contains(activateIndex))
                {
                    activeRandomEventSetIndices.Add(activateIndex);
                    Debug.Log($"[EventManager] 激活了第 {opt.randomEventSet} 个随机事件集。");
                }
            }
            else Debug.LogWarning($"[EventManager] ApplyOption: 无效的激活索引 {opt.randomEventSet}");
        }
        else if (opt.randomEventSet < 0)
        {
            int hideIndex = -opt.randomEventSet - 1;
            if (hideIndex >= 0 && hideIndex < randomEventList.Count && activeRandomEventSetIndices.Contains(hideIndex))
            {
                activeRandomEventSetIndices.Remove(hideIndex);
                Debug.Log($"[EventManager] 隐藏了第 {-opt.randomEventSet} 个随机事件集。");
            }
            else Debug.LogWarning($"[EventManager] ApplyOption: 无效的隐藏索引 {-opt.randomEventSet}");
        }

        // 处理 nextEventId
        if (opt.nextEventId != "0")
        {
            nextEventId = opt.nextEventId;
            Debug.Log($"[EventManager] ApplyOption: 设置下一个事件ID为 {opt.nextEventId}");
        }
        else
        {
            Debug.Log($"[EventManager] ApplyOption: 选项nextEventId为0，将选择随机事件");
            if (ishistoryEvent)
            {
                ishistoryEvent = false;
                Debug.Log($"[EventManager] ApplyOption: 历史事件链结束，重置历史事件标志");
            }
        }

        // 更新 UI （安全判断）
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStatText();
            UIManager.Instance.ClearText();
        }
        else
        {
            Debug.LogWarning("[EventManager] ApplyOption: UIManager.Instance 为 null，跳过 UI 更新");
        }
    }
    // public void OverQTE()
    // {
    //     QTEController qtec1 = qte.GetComponent<QTEController>();
    //     QTE2 qtec2 = qte.GetComponent<QTE2>();
    //     QTE3 qtec3 = qte.GetComponent<QTE3>();
    //     QTE4 qtec4 = qte.GetComponent<QTE4>();
    //     qtec1.enabled =false;
    //     qtec2.enabled =false;
    //     qtec3.enabled =false;
    //     qtec4.enabled =false;
    //     jsjy.SetActive(false);
    //     hmjy.SetActive(false);
    //     zzjy.SetActive(false);
    //     tljy.SetActive(false);
    //     palace.SetActive(true);
    //     qte.SetActive(false);

    //     MusicManager.Instance.PlayBgm(Bgm,0.8f);
    //     UIManager.Instance.ClearText();
    //     UIManager.Instance.UpdateStatText();

    // }


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
        if(eventId == "0") return;
        nextEventId = eventId;
        Debug.Log($"[EventManager] SetNextEventId: {eventId}");
    }
    
    /// <summary>
    /// 根据游戏状态决定下一个事件ID
    /// </summary>
    public string DetermineNextEventId()
    {
        int currentTurn = GameControl.Instance.turns;
        
        // 检查是否有任务完成事件
        if (GameControl.Instance.IsCompleteTask)
        {
            GameControl.Instance.IsCompleteTask = false; // 重置任务状态
            Debug.Log($"[EventManager] DetermineNextEventId: 任务完成事件 200");
            return "200"; // 任务完成事件
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
            SetNextEventId("126"); // 重置
            Debug.Log($"[EventManager] DetermineNextEventId: 使用预设事件ID {result}");
            return result;
        }
        
        // 4的倍数回合显示主线事件
        if (currentTurn % 4 == 0 && currentTurn > 0)
        {
            string mainEventId = "";

            if (currentTurn == 4)
            {
                ishistoryEvent = true;
                // 第一个主线事件ID固定
                mainEventId =  "00501";
                Debug.Log($"[EventManager] DetermineNextEventId: 第一个主线事件 {mainEventId}");
            }
            else
            {
                // 后续主线事件：如果有预设的下一个事件ID，使用它
                if (!string.IsNullOrEmpty(GetNextEventId()) && GetNextEventId() != "0" && GetNextEventId() != "100")
                {
                    ishistoryEvent = true;
                    mainEventId = GetNextEventId();
                    SetNextEventId("00501"); // 重置
                    Debug.Log($"[EventManager] DetermineNextEventId: 后续主线事件 {mainEventId}");
                }
                else
                {
                    // 如果没有预设事件ID，说明主线任务链断了，继续随机事件
                    ishistoryEvent = false;
                    Debug.LogWarning($"[EventManager] 回合 {currentTurn} 应该是主线事件，但没有预设的事件ID，使用随机事件");
                    int fallbackRandomId = Random.Range(1, 5);
                    string fallbackEventId = fallbackRandomId.ToString("000") + "01";
                    Debug.Log($"[EventManager] DetermineNextEventId: 随机事件（主线缺失替代） {fallbackEventId}");
                    return fallbackEventId;
                }
            }

            // 检查主线事件ID是否存在
            var testEvent = GetEvent(mainEventId);
            if (testEvent != null)
            {
                return mainEventId;
            }
            else
            {
                Debug.LogWarning($"计算的主线事件ID '{mainEventId}' 不存在，使用随机事件");
                // 继续到随机事件逻辑
            }
        }
        
        // 其他情况显示随机事件
        if (activeRandomEventSetIndices.Count == 0)
        {
            Debug.LogError("[EventManager] 没有可用的随机事件集！请检查逻辑。");
            return "00101"; // 返回一个默认的后备事件
        }

        // 从激活的事件集列表中随机选一个
        int randomSetIndex = activeRandomEventSetIndices[Random.Range(0, activeRandomEventSetIndices.Count)];
        var targetEventDict = randomEventList[randomSetIndex];

        if (targetEventDict.Count == 0)
        {
            Debug.LogError($"[EventManager] 选中的事件集 {randomSetIndex} 为空！");
            return "00101"; // 返回一个默认的后备事件
        }

        // 从选中的事件集中随机选一个事件
        List<string> eventIds = new List<string>(targetEventDict.Keys);
        string eventId = eventIds[Random.Range(0, eventIds.Count)];
        
        Debug.Log($"[EventManager] DetermineNextEventId: 从事件集 {randomSetIndex} 中抽取随机事件 {eventId}");
        return eventId;
    }

    // 游戏失败处理：重置随机事件集、保存剧情进度、重置数值
    public void HandleGameOver(string reason)
    {
        Debug.LogWarning($"[EventManager] 游戏失败触发: {reason}");

        // 保存剧情推进（即使事件重置，整体进度推进）
        int progress = PlayerPrefs.GetInt("StoryProgress", 0) + 1;
        PlayerPrefs.SetInt("StoryProgress", progress);
        PlayerPrefs.Save();
        Debug.Log($"[EventManager] StoryProgress 增加到 {progress}");

        // 重载事件（将所有事件集恢复为默认激活状态）
        LoadEvents();

        // 重置 nextEventId / 标志
        nextEventId = "100";
        ishistoryEvent = false;

        // 重置玩家数值（若需要由 GameControl 统一控制也可调用）
        if (stats != null)
        {
            //stats.ResetToDefault();
            Debug.Log("[EventManager] Player stats 已重置为默认值 (50)");
        }

        // 更新 UI 显示
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStatText();
            Debug.Log("[EventManager] 通知 UI 更新数值显示");
        }

        // 如需其它重置行为（例如回到主菜单、播放破局动画等），在这里扩展
    }

    // 在抽取随机事件处，请使用 activeRandomEventSetIndices 列表决定抽取范围
    private string PickRandomEventFromActiveSets()
    {
        if (randomEventList == null || randomEventList.Count == 0)
        {
            Debug.LogError("[EventManager] 无随机事件可用！");
            return "0";
        }

        if (activeRandomEventSetIndices == null || activeRandomEventSetIndices.Count == 0)
        {
            // 恢复默认激活全部，避免没有可抽取集
            activeRandomEventSetIndices = new List<int>();
            for (int i = 0; i < randomEventList.Count; i++) activeRandomEventSetIndices.Add(i);
            Debug.LogWarning("[EventManager] activeRandomEventSetIndices 为空，已恢复全部激活");
        }

        int setIndex = activeRandomEventSetIndices[Random.Range(0, activeRandomEventSetIndices.Count)];
        var dict = randomEventList[setIndex];
        if (dict == null || dict.Count == 0)
        {
            // 找到任意非空集作为后备
            for (int i = 0; i < randomEventList.Count; i++)
            {
                if (randomEventList[i] != null && randomEventList[i].Count > 0)
                {
                    dict = randomEventList[i];
                    setIndex = i;
                    break;
                }
            }
        }

        if (dict == null || dict.Count == 0)
        {
            Debug.LogError("[EventManager] 所有事件集均为空");
            return "0";
        }

        var keys = new List<string>(dict.Keys);
        string id = keys[Random.Range(0, keys.Count)];
        Debug.Log($"[EventManager] PickRandomEventFromActiveSets: 从事件集 {setIndex} 抽到 {id}");
        return id;
    }


}