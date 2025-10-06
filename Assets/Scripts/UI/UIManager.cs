using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

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
    public TextMeshProUGUI statText1;
    public TextMeshProUGUI statText2;
    public TextMeshProUGUI statText3;
    public TextMeshProUGUI statText4;
    public TextMeshProUGUI statText5;

    public bool ifShow;
    public int eventid = 100;
    public AudioClip GE;
    public AudioClip BE;

    public GameObject optionButtonPrefab;
    public Transform optionsParent;

    public bool allgone = false;
    private List<GameObject> optionButtons = new List<GameObject>();
    //private bool gameOverTriggered = false;

    // ===== 新增：结局面板（由 GameControl 统一触发）=====
    public GameObject endingPanel;
    public Text endingText;
    public Button restartButton;

    void Awake() { Instance = this; }

    void Start()
    {
        if (daDian != null) daDian.SetActive(true);
        if (endingPanel != null) endingPanel.SetActive(false);
        // 启动游戏（确保 GameControl 已在场景中）
        if (GameControl.Instance != null)
            GameControl.Instance.StartGame();
    }



    // ====== 事件显示 ======
    public void ShowEvent(string id)
    {
        Debug.Log("ShowEvent被调用，事件ID: " + id);

        // 清理旧按钮
        foreach (var b in optionButtons) 
            if (b != null) Destroy(b);
        optionButtons.Clear();

        var evt = EventManager.Instance.GetEvent(id);
        if (evt == null)
        {
            Debug.LogError($"无法找到事件ID: {id}");
            return;
        }

        titleText.text = evt.title;
        dialoguePanel.SetBody(evt.body);

        Debug.Log($"事件 {id} 有 {evt.options.Count} 个选项");

        // 修复：正确捕获循环变量并确保UI顺序
        for (int i = 0; i < evt.options.Count; i++)
        {
            var opt = evt.options[i];
            Debug.Log($"创建UI按钮 {i} (从上到下第{i+1}个): {opt.text}, 数值变化: K{opt.kingChange} N{opt.nobleChange} S{opt.scholarChange} F{opt.foreignChange} P{opt.peopleChange}");
            
            GameObject btn = Instantiate(optionButtonPrefab, optionsParent);
            
            // 强制设置按钮在UI中的顺序
            btn.transform.SetSiblingIndex(i);
            
            btn.GetComponentInChildren<Text>().text = $"{i+1}. {opt.text}";
            
            // 关键修复：创建局部副本并添加索引调试
            var capturedOpt = opt;
            var optionIndex = i; // 捕获索引用于调试
            
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log($"点击了UI第 {optionIndex+1} 个按钮 (数组索引{optionIndex}): {capturedOpt.text}");
                Debug.Log($"应用数值变化: K{capturedOpt.kingChange} N{capturedOpt.nobleChange} S{capturedOpt.scholarChange} F{capturedOpt.foreignChange} P{capturedOpt.peopleChange}");
                
                ifShow = true;
                EventManager.Instance.ApplyOption(capturedOpt, GameControl.Instance.turns);
                UpdateStatText();

                if (!string.IsNullOrEmpty(capturedOpt.nextEventId))
                    EventManager.Instance.SetNextEventId(capturedOpt.nextEventId, 0);

                ClearText();
                GameControl.Instance.ProcessNextTurn();
            });
            
            optionButtons.Add(btn);
        }

        ifShow = true;
    }

    public void UpdateStatText()
    {
        if (stats == null) return;

        if (statText1 != null) statText1.text = stats.king.ToString();
        if (statText2 != null) statText2.text = stats.noble.ToString();
        if (statText3 != null) statText3.text = stats.scholar.ToString();
        if (statText4 != null) statText4.text = stats.foreign.ToString();
        if (statText5 != null) statText5.text = stats.people.ToString();

        // 原先这里的越界 -> GameOver 判定已移交 GameControl
    }



    public void ClearText()
    {
        ifShow = false;
        if (titleText) titleText.text = string.Empty;
        if (dialoguePanel) dialoguePanel.SetBody(" ");
        foreach (var b in optionButtons) Destroy(b);
        optionButtons.Clear();
    }

    public void HideEventOptions()
    {
        foreach (var b in optionButtons) Destroy(b);
        optionButtons.Clear();
    }

    // ===== 结局面板显示/隐藏（供 GameControl 调用）=====
    public void ShowEndingPanel(string description)
    {
        if (endingPanel != null) endingPanel.SetActive(true);
        if (endingText != null) endingText.text = description;

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() =>
            {
                if (GameControl.Instance != null)
                    GameControl.Instance.RestartGame();
            });
        }

        // 隐藏事件 UI
        HideEventOptions();
        ClearText();
    }

    public void HideEndingPanel()
    {
        if (endingPanel != null) endingPanel.SetActive(false);
    }

    public void ShowDaDian()
    {
        if (daDian != null) daDian.SetActive(true);
    }

    public void HideDaDian()
    {
        if (daDian != null) daDian.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.OnDadianHidden();
    }
}