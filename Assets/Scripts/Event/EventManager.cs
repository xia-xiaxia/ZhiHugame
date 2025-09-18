using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    public StatModel stats;      // 拖到 Inspector
    public TextAsset eventJson;  // 把 JSON 文件拖到这里
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
    private Dictionary<string, GameEvent> events = new Dictionary<string, GameEvent>();

    private List<GameEvent> tempEvents = new List<GameEvent>();

    private List<GameEvent> historyEvents = new List<GameEvent>();
    // public DaChenMove dc1;
    // public DaChenMove dc2;
    // public DaChenMove dc3;
    public int dcb=1;

    // 事件ID生成相关
    private string nextEventId = "100"; // 下一个要显示的事件ID

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
        GameEvent[] all = JsonHelper.FromJson<GameEvent>(eventJson.text);

        foreach (var e in all)
        {
            e.SetGEventTypeFromId();
            e.SetTEventTypeFromId();
            events[e.id] = e;
            if (e.Gtype == GType.tempEvent)
            {
                tempEvents.Add(e);
            }
            else
            {
                historyEvents.Add(e);
                if (e.nextEventId != -1)
                {
                    Debug.Log($"事件 {e.id} 有后续事件 {e.nextEventId}");
                }
            }
        }
        }

    // 供 UI 调用：返回某个事件
    public GameEvent GetEvent(string id)
    {
        if (events.ContainsKey(id))
        {
            return events[id];
        }
        else
        {
            Debug.LogError($"事件ID '{id}' 不存在于事件字典中!");
            return null;
        }
    }

    // 玩家点了某个选项后调用
    public void ApplyOption(Option opt,int turns)
    {
        //audioSrc?.PlayOneShot(zhong,1.5f);
        
        // if(turns==1||turns%4==0)        target.Play();

        // if (dcb == 1)
        // {
        //     dc1.FlipX();
        //     dc1.MoveRight();
        //     dc2.FlipX();
        //     dc2.MoveLeft();
        //     dcb++;
        // }
        // else if (dcb == 2)
        // {
        //     dc2.FlipX();
        //     dc2.MoveRight();
        //     dc3.FlipX();
        //     dc3.MoveLeft();
        //     dcb++;
        // }
        // else
        // {
        //     dc3.FlipX();
        //     dc3.MoveRight();
        //     dc1.FlipX();
        //     dc1.MoveLeft();
        //     dcb = 1;
        // }

        stats.gold += opt.goldChange;
        stats.people += opt.peopleChange;
        stats.zhouli += opt.zhouLiChange;
        stats.weiwang += opt.weiwangChange;
        
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
        
        UIManager.Instance.UpdateStatText();
        // if (opt.oifzz || opt.oifjs || opt.oiftl||opt.oifhm)
        // {
        //     palace.SetActive(false);
        //     qte.SetActive(true);
        //     if (opt.oifjs)
        //     {
        //         qteimage.sprite = jsimage;
        //         QTEController qtec = qte.GetComponent<QTEController>();
        //         qtec.enabled = true;
        //         qtec.successCount = 0;
        //         qtec.failCount = 0;
        //         opt.oifjs = false;
        //         stats.ifjs = true;
        //         jsjy.SetActive(true);
        //         pandingwenben.localPosition = new Vector3(-260,-265, 0);
        //         MusicManager.Instance.PlayBgm(jsBgm);
        //     }
        //     else if (opt.oiftl)
        //     {
        //         qteimage.sprite = tlimage;
        //         QTE2 qtec = qte.GetComponent<QTE2>();
        //         qtec.successCount = 0;
        //         qtec.failCount = 0;
        //         qtec.enabled = true;
        //         opt.oiftl = false;
        //         stats.iftl = true;
        //         tljy.SetActive(true);
        //         pandingwenben.localPosition = new Vector3(769, -247, 0);
        //         MusicManager.Instance.PlayBgm(tlBgm);
        //     }
        //     else if(opt.oifzz) 
        //     { 
        //         qteimage.sprite = zzimage; 
        //         QTE3 qtec= qte.GetComponent<QTE3>();
        //         qtec.successCount = 0;
        //         qtec.enabled = true;
        //         opt.oifzz = false;
        //         stats.ifzz = true;
        //         zzjy.SetActive(true);
        //         pandingwenben.localPosition = new Vector3(-355, -265, 0);
        //         MusicManager.Instance.PlayBgm(zzBgm);
        //     }
        //     else
        //     {
        //         qteimage.sprite = hmimage;
        //         QTE4 qtec = qte.GetComponent<QTE4>();
        //         qtec.holdTime = 0;
        //         qtec.enabled = true;
        //         opt.oifhm = false;
        //         stats.ifhm = true;
        //         hmjy.SetActive(true);
        //         pandingwenben.localPosition = new Vector3(890, -260, 0);
        //         MusicManager.Instance.PlayBgm(hmBgm);
        //     }

        // }
            UIManager.Instance.UpdateStatText();
            UIManager.Instance.ClearText();


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
            Debug.Log($"[EventManager] DetermineNextEventId: 开场事件 1200101");
            return "1200101";
        }

        // 检查是否有预设的下一个事件ID（非主线回合且非"0"）
        if (!string.IsNullOrEmpty(GetNextEventId()) && GetNextEventId() != "0" && GetNextEventId() != "100")
        {
            string result = GetNextEventId();
            SetNextEventId("100"); // 重置
            Debug.Log($"[EventManager] DetermineNextEventId: 使用预设事件ID {result}");
            return result;
        }
        
        // 4的倍数回合显示主线事件
        if (currentTurn % 4 == 0 && currentTurn > 0)
        {
            string mainEventId = "";

            if (currentTurn == 4)
            {
                // 第一个主线事件ID固定
                mainEventId = GameControl.Instance.stats.year + "1" + "001" + "01";
                Debug.Log($"[EventManager] DetermineNextEventId: 第一个主线事件 {mainEventId}");
            }
            else
            {
                // 后续主线事件：如果有预设的下一个事件ID，使用它
                if (!string.IsNullOrEmpty(GetNextEventId()) && GetNextEventId() != "0" && GetNextEventId() != "100")
                {
                    mainEventId = GetNextEventId();
                    SetNextEventId("100"); // 重置
                    Debug.Log($"[EventManager] DetermineNextEventId: 后续主线事件 {mainEventId}");
                }
                else
                {
                    // 如果没有预设事件ID，说明主线任务链断了，继续随机事件
                    Debug.LogWarning($"[EventManager] 回合 {currentTurn} 应该是主线事件，但没有预设的事件ID，使用随机事件");
                    int fallbackRandomId = Random.Range(1, 100);
                    string fallbackEventId = GameControl.Instance.stats.year + "2" + fallbackRandomId.ToString("000") + "01";
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
        int randomId = Random.Range(1, 100);
        string eventId = GameControl.Instance.stats.year + "2" + randomId.ToString("000") + "01";
        
        Debug.Log($"[EventManager] DetermineNextEventId: 随机事件 {eventId}");
        return eventId;
    }


}