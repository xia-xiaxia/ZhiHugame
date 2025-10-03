using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // 原来的 4 个引用
    public StatModel1 stats;
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
    public TextMeshProUGUI statText1;
    public TextMeshProUGUI statText2;
    public TextMeshProUGUI statText3;
    public TextMeshProUGUI statText4;
    public TextMeshProUGUI statText5;

    public bool ifShow;
    public int eventid = 100;
    public AudioClip GE;
    public AudioClip BE;

    // 新增：按钮预制体 & 父容器
    public GameObject optionButtonPrefab;
    public Transform optionsParent;   // 随便建一个空物体当父节点，方便排版

    public bool allgone = false;

    // 运行时生成的按钮缓存
    private List<GameObject> optionButtons = new List<GameObject>();

    private bool gameOverTriggered = false;

    void Awake() { Instance = this; }

    void Start()
    {
        // 初始状态：隐藏 daDian
        if (daDian != null)
        {
            daDian.SetActive(false);
        }
        
        // 开始游戏
        GameControl.Instance.StartGame();
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
        // 只检查游戏结束条件，不管理回合
        if (stats.noble <= 0 && !allgone)
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
        
        if (stats.king <= 0 && !allgone)
        {
            gameover.sprite = end4;
            MusicManager.Instance.PlayBgm(BE, 0.8f);
            gameover.raycastTarget = true;
            allgone = true;
            StartCoroutine(Fade());
        }
        
        if (stats.scholar <= 0 && !allgone)
        {
            gameover.sprite = end1;
            MusicManager.Instance.PlayBgm(BE, 0.8f);
            gameover.raycastTarget = true;
            allgone = true;
            StartCoroutine(Fade());
        }

        // 检查回合数达到65时的游戏胜利条件
        if (GameControl.Instance.turns >= 65 && !allgone)
        {
            gameover.sprite = end3;
            MusicManager.Instance.PlayBgm(GE, 0.8f);
            allgone = true;
            StartCoroutine(Fade());
            gameover.raycastTarget = true;
        }
    }
        // 关键：根据事件选项数量动态生成按钮
    public void ShowEvent(string id)
    {
        Debug.Log("ShowEvent被调用，事件ID: " + id);

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
                Debug.Log($"按钮被点击: {capturedOpt.text}，点击时间: {Time.time}");

                // 立即设置为正在显示状态，避免Update中重复触发
                ifShow = true;
                Debug.Log("设置ifShow=true");

                // 应用选项效果
                EventManager.Instance.ApplyOption(capturedOpt, GameControl.Instance.turns);
                UpdateStatText();

                // 设置下一个事件ID（如果选项指定了的话）
                if (!string.IsNullOrEmpty(capturedOpt.nextEventId))
                {
                    EventManager.Instance.SetNextEventId(capturedOpt.nextEventId);
                }

                // 清理当前UI状态
                ClearText();

                // 让 GameControl 处理下一回合
                GameControl.Instance.ProcessNextTurn();
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
        if (stats == null)
        {
            Debug.LogError("[UIManager] stats 为 null，无法更新数值显示");
            return;
        }

        statText1.text = $"{stats.king}";
        statText2.text = $"{stats.noble}";
        statText3.text = $"{stats.scholar}";
        statText4.text = $"{stats.foreign}";
        statText5.text = $"{stats.people}";

        // 检查是否触发失败（只触发一次）
        if (!gameOverTriggered && stats.IsOutOfBounds())
        {
            gameOverTriggered = true;
            Debug.LogWarning("[UIManager] 检测到统计数值越界，触发游戏失败");
            if (EventManager.Instance != null)
            {
                EventManager.Instance.HandleGameOver("统计数值越界");
            }
        }
    }

    // 在需要时复位 gameOverTriggered（例如 Start 新游戏或重置界面时）
    public void ResetGameOverFlag()
    {
        gameOverTriggered = false;
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
}