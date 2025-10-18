using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public StatModel stats;
    
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

    public bool isShow;
    public int eventid = 100;

    public GameObject optionButtonPrefab;
    public Transform optionsParent;

    private List<GameObject> optionButtons = new List<GameObject>();
    //private bool gameOverTriggered = false;

    // ===== 新增：结局面板（由 GameControl 统一触发）=====
    public GameObject endingPanel;
    public Text endingText;
    public Button restartButton;

    // ===== 新增：免死道具确认弹窗 =====
    public GameObject deathImmunityPanel;
    public Text deathImmunityText;
    public Button useDeathImmunityButton;
    public Button declineDeathImmunityButton;

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

        foreach (var opt in evt.options){
            GameObject btn = Instantiate(optionButtonPrefab, optionsParent);
            btn.GetComponentInChildren<Text>().text = opt.text;

            var capturedOpt = opt; 
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log($"按钮被点击: {capturedOpt.text}，点击时间: {Time.time} ");

                isShow = true;

                // 在应用选项前保存数值快照（用于免死道具恢复）
                GameControl.Instance.SaveStatsSnapshot();

                EventManager.Instance.ApplyOption(capturedOpt, GameControl.Instance.year);
                UpdateStatText();

                if (!string.IsNullOrEmpty(capturedOpt.nextEventId))
                {
                    EventManager.Instance.SetNextEventId(capturedOpt.nextEventId);
                }
                ClearText();
                GameControl.Instance.ProcessNextTurn();
            });
            optionButtons.Add(btn);
            Debug.Log("生成按钮："  + opt.text);
        }

        isShow = true;
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
        isShow = false;
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

    // ===== 免死道具确认弹窗 =====
    public void ShowDeathImmunityPrompt(PolicyItem item, int deathType)
    {
        if (deathImmunityPanel != null) deathImmunityPanel.SetActive(true);
        
        // 构建提示文本
        string deathTypeName = GetDeathTypeName(deathType);
        string promptText = $"检测到致命危机：{deathTypeName}\n\n是否使用国策：{item.name}？\n{item.desc}\n剩余使用次数：{(item.usageCount == -1 ? "无限" : item.usageCount.ToString())}";
        
        if (deathImmunityText != null) deathImmunityText.text = promptText;

        // 绑定按钮
        if (useDeathImmunityButton != null)
        {
            useDeathImmunityButton.onClick.RemoveAllListeners();
            useDeathImmunityButton.onClick.AddListener(() =>
            {
                deathImmunityPanel.SetActive(false);
                GameControl.Instance.OnDeathImmunityUse();
            });
        }

        if (declineDeathImmunityButton != null)
        {
            declineDeathImmunityButton.onClick.RemoveAllListeners();
            declineDeathImmunityButton.onClick.AddListener(() =>
            {
                deathImmunityPanel.SetActive(false);
                GameControl.Instance.OnDeathImmunityDecline();
            });
        }
    }

    // 获取死亡类型名称
    private string GetDeathTypeName(int deathType)
    {
        switch (deathType)
        {
            case 1: return "国君势力失衡";
            case 2: return "士族势力失衡";
            case 3: return "贵族势力失衡";
            case 4: return "外臣势力失衡";
            case 5: return "庶人势力失衡";
            case 6: return "事件强制死亡";
            default: return "未知危机";
        }
    }
}