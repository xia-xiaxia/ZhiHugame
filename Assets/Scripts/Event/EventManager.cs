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
            if(e.Gtype==GType.tempEvent)
            {
                tempEvents.Add(e);
            }
            else
            {
                historyEvents.Add(e);
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


}