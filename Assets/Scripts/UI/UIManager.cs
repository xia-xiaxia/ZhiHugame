using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // 原来的 4 个引用
    public StatModel stats;
    public Image gameover;
    public Sprite end1;
    public Sprite end2;
    public Sprite end3;
    public Sprite end4;
    public Sprite end5;


    public GameObject daDian;

    public GameObject jinYan;

    public Text titleText;
    public Text bodyText;
    public DialoguePanel dialoguePanel;
    // public Text statText1;
    // public Text statText2;
    // public Text statText3;
    // public Text statText4;
    public bool ifShow;
    public int eventid = 100;
    public int turns = 0;
    public AudioClip GE;
    public AudioClip BE;

    // 新增：按钮预制体 & 父容器
    public GameObject optionButtonPrefab;
    public Transform optionsParent;   // 随便建一个空物体当父节点，方便排版

    public bool allgone = false;

    // 运行时生成的按钮缓存
    private List<GameObject> optionButtons = new List<GameObject>();

    private string nextId;
    private bool waitingForNextTurn = false; // 是否正在等待下一回合
    private Coroutine currentWaitCoroutine = null; // 当前运行的等待协程

    void Awake() { Instance = this; }

    void Start()
    {
        // 初始状态：隐藏 daDian
        if (daDian != null)
        {
            daDian.SetActive(false);
        }
    }
    IEnumerator Fade()
    {
        Color c = gameover.color; c.a = 0; gameover.color = c;
        for (float t = 0; t < 2f; t += Time.deltaTime)
        {
            c.a = t / 2f;
            gameover.color = c;
            yield return null;
        }
        c.a = 1;
        gameover.color = c;
    }
    private void Update()
    {
        if (stats.gold <= 0 && !allgone)
        {
            gameover.sprite = end5;
            MusicManager.Instance.PlayBgm(BE, 0.8f);
            gameover.raycastTarget = true;
            allgone = true;
            StartCoroutine(Fade());

        }

        if (stats.people <= 0 && !allgone)
        {
            gameover.sprite = end2;
            MusicManager.Instance.PlayBgm(BE, 0.8f);
            gameover.raycastTarget = true;
            allgone = true;
            StartCoroutine(Fade());
        }
        if (stats.weiwang <= 0 && !allgone)
        {
            gameover.sprite = end4;
            MusicManager.Instance.PlayBgm(BE, 0.8f);
            gameover.raycastTarget = true;
            allgone = true;
            StartCoroutine(Fade());
        }
        if (stats.zhouli <= 0 && !allgone)
        {
            gameover.sprite = end1;
            MusicManager.Instance.PlayBgm(BE, 0.8f);
            gameover.raycastTarget = true;
            allgone = true;
            StartCoroutine(Fade());
        }


        if (!ifShow && !waitingForNextTurn)
        {
            Debug.Log("Update检测到可以开始新回合，当前turns: " + turns);
            turns++;
            Debug.Log("turns递增后: " + turns);
            
            // 设置等待状态，防止重复执行
            waitingForNextTurn = true;
            if (turns == 65 && !allgone)
            {
                gameover.sprite = end3;
                MusicManager.Instance.PlayBgm(GE, 0.8f);
                allgone = true;
                StartCoroutine(Fade());
                gameover.raycastTarget = true;
            }
            else if (turns == 1)
            {
                ShowEvent("301");
                Debug.Log("显示事件301");
            }
            else if (turns % 4 != 0)
            {
                int randomid = Random.Range(101, 148);
                string rdm = "" + randomid;
                ShowEvent(rdm);
                Debug.Log("显示随机事件: " + rdm);
            }
            else if (turns % 4 == 0 && turns < 64 && nextId != null && turns != 0)
            {
                ShowEvent(nextId);
                nextId = null;
                Debug.Log("显示指定事件: " + nextId);
            }
            else if (turns % 4 == 0 && turns < 64 && turns != 0)
            {
                // 当 turns 是4的倍数但 nextId 为空时的处理
                int idn = 200 + turns / 4;
                string calculatedId = "" + idn;
                Debug.Log("尝试显示计算事件: " + calculatedId);    
                
                // 检查计算出的事件ID是否存在
                var testEvent = EventManager.Instance.GetEvent(calculatedId);
                if (testEvent != null)
                {
                    ShowEvent(calculatedId);
                    Debug.Log("显示计算事件: " + calculatedId);
                }
                else
                {
                    Debug.LogWarning($"计算的事件ID '{calculatedId}' 不存在，使用随机事件");
                    int randomid = Random.Range(101, 148);
                    string rdm = "" + randomid;
                    ShowEvent(rdm);
                    Debug.Log("显示备用随机事件: " + rdm);
                }
            }

        }
    }


    // 关键：根据事件选项数量动态生成按钮
    public void ShowEvent(string id)
    {
        Debug.Log("ShowEvent被调用，事件ID: " + id);
        
        // 停止当前运行的协程（如果有的话）
        if (currentWaitCoroutine != null)
        {
            StopCoroutine(currentWaitCoroutine);
            currentWaitCoroutine = null;
            Debug.Log("ShowEvent中停止了之前的协程");
            
            // 由于停止了协程，需要手动执行协程本来要做的清理工作
            HideDaDian();
            Debug.Log("手动隐藏dadian，因为协程被停止了");
        }
        
        // 不在这里设置ifShow，等按钮生成后再设置
        
        // 1. 清掉上一次生成的按钮
        foreach (var b in optionButtons) Destroy(b);
        optionButtons.Clear();

        // 2. 取事件
        var evt = EventManager.Instance.GetEvent(id);
        if (evt == null)
        {
            Debug.LogError($"无法找到事件ID: {id}，停止显示事件");
            return;
        }
        titleText.text = evt.title;
        dialoguePanel.SetBody(evt.body);

        // 3. 根据选项数量生成按钮
        Debug.Log($"事件 {id} 有 {evt.options.Count} 个选项");
        
        foreach (var opt in evt.options)
        {
            GameObject btn = Instantiate(optionButtonPrefab, optionsParent);
            btn.GetComponentInChildren<Text>().text = opt.text;

            // 把点击事件绑进去（注意闭包陷阱，用局部变量）
            var capturedOpt = opt;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log($"按钮被点击，当前turns: {turns}，点击时间: {Time.time}");
                
                // 停止之前的协程（如果存在）
                if (currentWaitCoroutine != null)
                {
                    StopCoroutine(currentWaitCoroutine);
                    Debug.Log("停止之前的协程");
                }
                
                // 立即设置为正在显示状态，避免Update中重复触发
                ifShow = true;
                waitingForNextTurn = false; // 重置等待状态
                Debug.Log("设置ifShow=true, waitingForNextTurn=false");
                
                // 玩家做出选择后，立即显示 dadian
                ShowDaDian();
                
                EventManager.Instance.ApplyOption(capturedOpt, turns);
                UpdateStatText();
                
                // 设置下一个事件ID（可选，如果选项指定了的话）
                if (!string.IsNullOrEmpty(capturedOpt.nextEventId))
                {
                    nextId = capturedOpt.nextEventId;
                }
                // 不再在这里预计算其他事件ID，让Update方法统一处理
                
                // 等待一小会儿后抽取下一个任务
                currentWaitCoroutine = StartCoroutine(WaitAndShowNextEvent());
            });

            optionButtons.Add(btn);
            Debug.Log("生成按钮: " + opt.text);
        }
        
        // 所有按钮生成完毕后，设置ifShow = true
        ifShow = true;
        Debug.Log("事件显示完成，设置ifShow=true");
    }

    public void UpdateStatText()
    {
        // var s = EventManager.Instance.stats;
        // statText1.text = $"{s.gold}";
        // statText2.text = $"{s.people}";
        // statText3.text = $"{s.weiwang}";
        // statText4.text = $"{s.zhouli}";

    }

    public void ClearText()
    {
        ifShow = false;
        // 1. 清空文字
        titleText.text = string.Empty;
        dialoguePanel.SetBody(" ");

        // 2. 销毁所有选项按钮
        foreach (var b in optionButtons)
            Destroy(b);
        optionButtons.Clear();
    }
    
    public void ShowDaDian()
    {
        // 只显示 dadian，不再管 jinYan
        if (daDian != null)
        {
            daDian.SetActive(true);
        }
    }
    
    public void HideDaDian()
    {
        // 隐藏 dadian
        if (daDian != null)
        {
            daDian.SetActive(false);
        }
        
        // 通知 DialoguePanel dadian 已隐藏
        if (dialoguePanel != null)
        {
            dialoguePanel.OnDadianHidden();
        }
    }
    
    // 等待一小段时间后显示下一个事件的协程
    private IEnumerator WaitAndShowNextEvent()
    {
        float startTime = Time.time;
        Debug.Log($"开始等待协程，开始时间: {startTime}");
        
        // 等待1.5秒（可以根据需要调整时间）
        yield return new WaitForSeconds(1.5f);
        
        float endTime = Time.time;
        Debug.Log($"协程等待结束，结束时间: {endTime}，等待了: {endTime - startTime}秒");
        Debug.Log("隐藏dadian");
        
        // 隐藏 dadian
        HideDaDian();
        
        Debug.Log("协程结束，准备下一回合");
        // 重置状态，允许下一回合开始
        ifShow = false;
        waitingForNextTurn = false;
        currentWaitCoroutine = null; // 清空协程引用
    }
}