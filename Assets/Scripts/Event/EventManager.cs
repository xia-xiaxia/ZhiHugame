using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using JsonA;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    public StatModel stats;
    public List<TextAsset> eventJsons;
    public TextAsset historyEventJson;
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

    private Dictionary<string, GameEvent> randomEvents = new Dictionary<string, GameEvent>();
    private Dictionary<string, GameEvent> historyEvents = new Dictionary<string, GameEvent>();

    private List<Dictionary<string, GameEvent>> randomEventList = new List<Dictionary<string, GameEvent>>();
    private List<int> activeRandomEventSetIndices = new List<int>();

    public bool ishistoryEvent = false;
    public int fileIndex = 0;
    public int dcb = 1;

    // 下一个事件（仅选项强制指定时生效）
    private string nextEventId = "0";

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
        historyEvents.Clear();

        for (int i = 0; i < eventJsons.Count; i++)
        {
            var eventJson = eventJsons[i];
            if (eventJson == null)
            {
                Debug.LogWarning($"[EventManager] eventJson[{i}] 为 null");
                continue;
            }

            var currentEventDict = new Dictionary<string, GameEvent>();
            List<GameEvent> all = JsonHelper.FromJson<GameEvent>(eventJson.text);
            foreach (var e in all)
            {
                if (!string.IsNullOrEmpty(e?.id))
                    currentEventDict[e.id] = e;
            }
            randomEventList.Add(currentEventDict);
            activeRandomEventSetIndices.Add(i);
            Debug.Log($"[EventManager] LoadEvents 随机事件集 {i} 加载 {currentEventDict.Count} 条");
        }

        if (historyEventJson != null)
        {
            List<GameEvent> his = JsonHelper.FromJson<GameEvent>(historyEventJson.text);
            foreach (var e in his)
            {
                if (!string.IsNullOrEmpty(e?.id))
                    historyEvents[e.id] = e;
            }
            Debug.Log($"[EventManager] LoadEvents 历史事件加载 {historyEvents.Count} 条");
        }
        else
        {
            Debug.LogWarning("[EventManager] 未提供历史事件 JSON");
        }
    }

    public GameEvent GetEvent(string id, int randomEventSet = -1)
    {
        if (ishistoryEvent)
        {
            if (historyEvents.TryGetValue(id, out var his))
                return his;
            Debug.LogError($"[EventManager] 历史事件 {id} 不存在");
            return null;
        }

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

    public void ApplyOption(Option opt, int turns)
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

        if (opt.randomEventSet > 0)
        {
            int act = opt.randomEventSet - 1;
            if (act >= 0 && act < randomEventList.Count && !activeRandomEventSetIndices.Contains(act))
            {
                activeRandomEventSetIndices.Add(act);
                Debug.Log($"[EventManager] 激活事件集 {opt.randomEventSet}");
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

        if (opt.nextEventId != "0")
        {
            nextEventId = opt.nextEventId;
            Debug.Log($"[EventManager] 设定后继事件 {nextEventId}");
        }
        else if (ishistoryEvent)
        {
            ishistoryEvent = false;
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
        int currentTurn = GameControl.Instance.turns;

        if (GameControl.Instance.IsCompleteTask)
        {
            GameControl.Instance.IsCompleteTask = false;
            return "200";
        }

        if (currentTurn == 1)
        {
            ishistoryEvent = false;
            return "00101";
        }

        // 强制后继（一次性）
        if (!string.IsNullOrEmpty(nextEventId) &&
            nextEventId != "0" &&
            nextEventId != "00101" &&
            nextEventId != "00501")
        {
            string forced = nextEventId;
            nextEventId = "0";
            ishistoryEvent = false;
            return forced;
        }

        // 每4回合尝试主线
        if (currentTurn % 4 == 0 && currentTurn > 0)
        {
            string mainEventId = (currentTurn == 4) ? "00501" : "00501"; // 可扩展后续主线
            ishistoryEvent = true;

            var he = GetEvent(mainEventId);
            if (he != null)
                return mainEventId;

            // 主线缺失 → 使用 ???01 模式随机
            Debug.LogWarning($"[EventManager] 主线事件 {mainEventId} 缺失，使用 ???01 模式随机");
            ishistoryEvent = false;
            return PickRandom01PatternEvent();
        }

        // 普通随机
        ishistoryEvent = false;
        return PickRandomEventFromActiveSets();
    }

    // 从激活集中随机
    private string PickRandomEventFromActiveSets()
    {
        if (randomEventList == null || randomEventList.Count == 0)
            return "0";

        if (activeRandomEventSetIndices == null || activeRandomEventSetIndices.Count == 0)
        {
            activeRandomEventSetIndices = new List<int>();
            for (int i = 0; i < randomEventList.Count; i++)
                activeRandomEventSetIndices.Add(i);
        }

        int safety = 0;
        Dictionary<string, GameEvent> chosen = null;
        int chosenSet = -1;
        while (safety++ < 20)
        {
            int idx = activeRandomEventSetIndices[Random.Range(0, activeRandomEventSetIndices.Count)];
            if (idx >= 0 && idx < randomEventList.Count)
            {
                var dict = randomEventList[idx];
                if (dict != null && dict.Count > 0)
                {
                    chosen = dict;
                    chosenSet = idx;
                    break;
                }
            }
        }

        if (chosen == null || chosen.Count == 0)
        {
            Debug.LogError("[EventManager] 无可用随机事件");
            return "0";
        }

        var keys = new List<string>(chosen.Keys);
        string id = keys[Random.Range(0, keys.Count)];
        Debug.Log($"[EventManager] 随机集 {chosenSet} -> {id}");
        return id;
    }

    // 只挑选以 01 结尾的事件（???01）
    private string PickRandom01PatternEvent()
    {
        List<string> candidates = new List<string>();

        foreach (int idx in activeRandomEventSetIndices)
        {
            if (idx < 0 || idx >= randomEventList.Count) continue;
            var dict = randomEventList[idx];
            if (dict == null) continue;
            foreach (var id in dict.Keys)
            {
                if (id.EndsWith("01"))
                {
                    if (GameControl.Instance != null && GameControl.Instance.turns > 1 && id == "00101")
                        continue;
                    candidates.Add(id);
                }
            }
        }

        if (candidates.Count == 0)
        {
            foreach (var dict in randomEventList)
            {
                if (dict == null) continue;
                foreach (var id in dict.Keys)
                    if (id.EndsWith("01")) candidates.Add(id);
            }
        }

        if (candidates.Count == 0)
        {
            int rnd = Random.Range(1, 10);
            string manual = rnd.ToString("000") + "01";
            Debug.LogWarning($"[EventManager] 无 01 结尾事件，构造兜底 {manual}");
            return manual;
        }

        string pick = candidates[Random.Range(0, candidates.Count)];
        Debug.Log($"[EventManager] PickRandom01PatternEvent -> {pick}");
        return pick;
    }

    public void HandleGameOver(string reason)
    {
        Debug.LogWarning($"[EventManager] 游戏失败: {reason}");
        LoadEvents();
        nextEventId = "0";
        ishistoryEvent = false;

        if (stats != null)
            Debug.Log("[EventManager] 可在此重置数值（目前未重置）");

        UIManager.Instance?.UpdateStatText();
    }

    public void ReloadAllEventsForRestart()
    {
        nextEventId = "0";
        ishistoryEvent = false;
        LoadEvents();
    }

    public void OnRestartCleanup() { }
}