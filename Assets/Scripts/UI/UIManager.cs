using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public StatModel stats;
    
    public GameObject jinYan;

    public Text titleText;
    public Text bodyText;
    public Text speakerName;
    public DialoguePanel dialoguePanel;
    public TextMeshProUGUI statText1;
    public TextMeshProUGUI statText2;
    public TextMeshProUGUI statText3;
    public TextMeshProUGUI statText4;
    public TextMeshProUGUI statText5;

    public bool isShow;
    public int eventid = 100;
    public Text currentYearText;

    public Button[] optionButtons = new Button[4]; // 在Inspector拖入4个选项按钮
    public Button nextSentenceButton; // “下一句”按钮
    public Button autoPlayButton; // “自动播放”按钮

    // ===== 新增：结局面板（由 Game Control 统一触发）=====
    public GameObject endingPanel;
    public Text endingText;
    public Button restartButton;

    // ===== 新增：免死道具确认弹窗 =====
    public GameObject deathImmunityPanel;
    public Text deathImmunityText;
    public Button useDeathImmunityButton;

    public Button declineDeathImmunityButton;

    // ===== 新增：免死道具生效文案显示面板 =====
    public GameObject deathImmunityMessagePanel;
    public Text deathImmunityMessageText;
    public Button deathImmunityMessageConfirmButton;

    // ===== 新增：道具菜单弹窗（显示所有道具）=====
    public GameObject policyMenuPanel;
    public Transform policyItemsParent;
    public GameObject policyItemButtonPrefab;
    private List<GameObject> policyItemButtons = new List<GameObject>();

    // ===== 新增：道具商店面板（游戏结束后购买）=====
    public GameObject policyShopPanel;
    public Transform shopItemsParent;
    public GameObject shopItemButtonPrefab;
    public Text currencyText; // 显示可用货币（年数）
    public Button closeShopButton; // 关闭商店继续结局流程
    private List<GameObject> shopItemButtons = new List<GameObject>();

    // ===== 新增：商店页面的背包显示区域 =====
    public Transform shopInventoryParent; // 商店中的背包区域容器
    public GameObject shopInventoryItemPrefab; // 背包道具按钮预制体
    public Text shopInventoryCountText; // 显示背包数量 (例如: "3/5")
    private List<GameObject> shopInventoryButtons = new List<GameObject>();

    // ===== 新增：时局（BUFF）列表面板 =====
    public GameObject buffPanel;
    public Transform buffItemsParent;
    public GameObject buffItemButtonPrefab;
    public Text buffDetailText;
    public Button buffCloseButton;
    private readonly List<GameObject> buffItemButtons = new List<GameObject>();
    private string selectedBuffId = null;

    // ===== 新增：全局道具提示框（用于鼠标悬停显示，需在场景中设置）=====
    public GameObject policyTooltipPanel;
    public Text policyTooltipText;

    // ===== 新增：游戏内退出按钮 =====
    public Button exitToMenuButton; // 游戏中退出到主菜单按钮

    private List<string> currentEventSentences = new List<string>();
    private int currentSentenceIndex = 0;
    private bool waitingForSentence = false;
    private string currentEventId = "";
    private Coroutine autoNextCoroutine = null; // 自动下一句的协程句柄
    private bool autoPlayEnabled = false; // 是否开启自动播放

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 只启用 jinYan（事件显示面板）
        if (jinYan != null) jinYan.SetActive(true);
        if (endingPanel != null) endingPanel.SetActive(false);

        // 绑定游戏内退出按钮
        if (exitToMenuButton != null)
        {
            exitToMenuButton.onClick.RemoveAllListeners();
            exitToMenuButton.onClick.AddListener(OnExitToMenuClicked);
        }

        // 注意：不要在这里自动启动游戏，应该由用户点击开始按钮触发
        // 游戏启动流程：用户点击按钮 → OnStartGameButtonClicked() → CanvasMove.StartGame() → 淡入淡出 → StartGame()

        // 绑定“下一句”按钮
        if (nextSentenceButton != null)
        {
            nextSentenceButton.onClick.RemoveAllListeners();
            nextSentenceButton.onClick.AddListener(OnNextSentenceClicked);
        }

        // 绑定“自动播放”按钮
        if (autoPlayButton != null)
        {
            autoPlayButton.onClick.RemoveAllListeners();
            autoPlayButton.onClick.AddListener(OnAutoPlayClicked);
            UpdateAutoPlayButtonLabel();
        }

    }

    // 暂停游戏
    public void OnPauseGameClicked()
    {
        if (GameControl.Instance != null)
        {
            GameControl.Instance.PauseGameForMenu();
        }
    }

    // 游戏内退出到主菜单
    private void OnExitToMenuClicked()
    {
        if (GameControl.Instance != null)
        {
            GameControl.Instance.ExitToMainMenuFromPause();
        }
    }



    // ====== 事件显示 ======
    public void ShowEvent(string id)
    {
        Debug.Log("ShowEvent被调用，事件ID: " + id);
        currentEventId = id; // 记录当前事件id
        // 标记该事件为已使用，防止本局重复出现
        if (EventManager.Instance != null)
            EventManager.Instance.MarkEventUsed(id);

        if (EventManager.Instance == null)
        {
            Debug.LogError("[UIManager] EventManager.Instance 为 null，无法显示事件");
            return;
        }

        var evt = EventManager.Instance.GetEvent(id);
        if (evt == null)
        {
            Debug.LogError($"无法找到事件ID: {id}");
            return;
        }
        if(evt.speaker != null && evt.speaker!="旁白")
        {
            CharacterManager.Instance.ShowCharacter(evt.speaker);
        }
        if (titleText != null) titleText.text = evt.title ?? string.Empty; else Debug.LogWarning("[UIManager] titleText 未绑定");
        if (speakerName != null) speakerName.text = evt.speaker ?? string.Empty; else Debug.LogWarning("[UIManager] speakerName 未绑定");
        // 按句分割正文（可按'\n'或其它分隔符）
        string body = evt.body ?? string.Empty;
        currentEventSentences = new List<string>(body.Split('\n'));
        currentSentenceIndex = 0;
        if (currentEventSentences.Count <= 1)
        {
            // 只有一句，正文显示后延迟显示选项
            if (dialoguePanel != null)
                dialoguePanel.SetBody(body);
            else
                Debug.LogWarning("[UIManager] dialoguePanel 未绑定，无法播放逐字效果");
            waitingForSentence = true;
            StartCoroutine(ShowOptionsAfterDelay(0.8f)); // 0.8秒后显示选项
        }
        else
        {
            waitingForSentence = true;
            ShowCurrentSentence();
        }
    }

    private void ShowEventOptions(string id)
    {
        var evt = EventManager.Instance.GetEvent(id);
        if (evt == null)
        {
            Debug.LogError($"[ShowEventOptions] 事件ID不存在: {id}");
            return;
        }
        if (evt.options == null)
        {
            Debug.LogError($"[ShowEventOptions] 事件ID {id} 的 options 为空");
            return;
        }
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < evt.options.Count)
            {
                var opt = evt.options[i];
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].GetComponentInChildren<Text>().text = opt.text;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() =>
                {
                    isShow = true;
                    GameControl.Instance.SaveStatsSnapshot();
                    EventManager.Instance.ApplyOption(opt, GameControl.Instance.year);
                    // 优先考虑玩家选择设置的后继事件
                    if (!string.IsNullOrEmpty(opt.nextEventId))
                    {
                        EventManager.Instance.SetNextEventId(opt.nextEventId);
                    }
                    // 每个事件串结束后，应用"时局(BUFF)"的长期影响与时限扣减
                    // 注释掉不存在的方法调用
                    // if (BuffManager.Instance != null)
                    //     BuffManager.Instance.OnEventChainEnd();
                    // 更新UI并再次做死亡/结局判定
                    UpdateStatText();
                    GameControl.Instance?.OnStatsChanged();
                    ClearText();
                    GameControl.Instance.ProcessNextTurn();
                });
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
        // 修复：使用 yearDelta 而不是 yearChange
        GameControl.Instance.year += evt.yearDelta;
        if(evt.yearDelta != 0)
        {
            for(int i = 0; i < evt.yearDelta; i++)
            {
                // 注释掉不存在的方法
                // GameControl.Instance.CheckAndTriggerYearEnding();
                BuffManager.Instance?.OnYearEnd();
            }
        }
    }

    private void ShowCurrentSentence()
    {
        // 隐藏所有选项按钮，直到最后一句
        foreach (var btn in optionButtons)
        {
            if (btn != null) btn.gameObject.SetActive(false);
        }
        if (currentSentenceIndex < currentEventSentences.Count)
        {
            if (dialoguePanel != null)
                dialoguePanel.SetBody(currentEventSentences[currentSentenceIndex]);
            // 自动延迟显示下一句
            currentSentenceIndex++;
            if (currentSentenceIndex < currentEventSentences.Count)
            {
                // 这里可用协程实现自动延迟（如1秒），也可直接递归调用（立即显示）
                if (autoNextCoroutine != null) { StopCoroutine(autoNextCoroutine); autoNextCoroutine = null; }
                if (autoPlayEnabled)
                {
                    autoNextCoroutine = StartCoroutine(AutoShowNextSentence(1.0f)); // 仅在自动播放时自动下一句
                }
            }
            else
            {
                // 全部显示完毕，显示选项
                if (autoNextCoroutine != null) { StopCoroutine(autoNextCoroutine); autoNextCoroutine = null; }
                StartCoroutine(ShowOptionsAfterDelay(1.0f));
            }
        }
    }

    private System.Collections.IEnumerator AutoShowNextSentence(float delay)
    {
        // 若中途关闭了自动播放则直接退出
        if (!autoPlayEnabled)
            yield break;
        yield return new WaitForSeconds(delay);
        if (!autoPlayEnabled)
            yield break;
        ShowCurrentSentence();
    }

    private System.Collections.IEnumerator ShowOptionsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowEventOptions(currentEventId); // 用当前事件id
        waitingForSentence = false;
    }

    // “下一句”按钮点击：优先打断打字，其次前进到下一句
    private void OnNextSentenceClicked()
    {
        if (!waitingForSentence) return;
        
        // 若仍在打字中，先强制完成当前句子
        if (dialoguePanel != null && dialoguePanel.IsTyping)
        {
            dialoguePanel.ForceCompleteTyping();
            return;
        }

        // 打断自动下一句的协程
        if (autoNextCoroutine != null)
        {
            StopCoroutine(autoNextCoroutine);
            autoNextCoroutine = null;
        }
        // 直接显示当前索引对应的句子（ShowCurrentSentence 内部会自增索引）
        ShowCurrentSentence();
    }

    // 兼容老接口：外部调用显示下一句
    public void ShowNextSentence()
    {
        OnNextSentenceClicked();
    }

    // 自动播放按钮点击
    private void OnAutoPlayClicked()
    {
        autoPlayEnabled = !autoPlayEnabled;
        UpdateAutoPlayButtonLabel();

        if (!autoPlayEnabled)
        {
            // 关闭自动播放时，停止任何自动推进
            if (autoNextCoroutine != null)
            {
                StopCoroutine(autoNextCoroutine);
                autoNextCoroutine = null;
            }
        }
        else
        {
            // 开启自动播放：如果当前在句子流程中且不在打字中，并且还有剩余句子，则安排自动下一句
            if (waitingForSentence && dialoguePanel != null && !dialoguePanel.IsTyping && currentSentenceIndex < currentEventSentences.Count)
            {
                if (autoNextCoroutine != null)
                {
                    StopCoroutine(autoNextCoroutine);
                    autoNextCoroutine = null;
                }
                autoNextCoroutine = StartCoroutine(AutoShowNextSentence(0.5f));
            }
        }
    }

    private void UpdateAutoPlayButtonLabel()
    {
        if (autoPlayButton == null) return;
        var txt = autoPlayButton.GetComponentInChildren<Text>();
        if (txt != null)
            txt.text = autoPlayEnabled ? "自动播放：开" : "自动播放：关";
    }

    public void UpdateStatText()
    {
        if (stats == null) return;

        if (statText1 != null) statText1.text = stats.king.ToString();
        if (statText2 != null) statText2.text = stats.noble.ToString();
        if (statText3 != null) statText3.text = stats.scholar.ToString();
        if (statText4 != null) statText4.text = stats.foreign.ToString();
        if (statText5 != null) statText5.text = stats.people.ToString();

    }


    public void ClearText()
    {
        isShow = false;
        if (titleText) titleText.text = string.Empty;
        // 只有在 dialoguePanel 激活时才调用 SetBody，避免协程启动失败
        if (dialoguePanel && dialoguePanel.gameObject.activeInHierarchy)
        {
            dialoguePanel.SetBody(" ");
        }
        // 隐藏所有选项按钮
        foreach (var btn in optionButtons)
        {
            if (btn != null)
            {
                btn.gameObject.SetActive(false);
                btn.onClick.RemoveAllListeners();
                btn.GetComponentInChildren<Text>().text = "";
            }
        }
    }

    // ===== 获取当前事件状态（用于暂停保存）=====
    public string GetCurrentEventId()
    {
        return currentEventId;
    }

    public int GetCurrentSentenceIndex()
    {
        return currentSentenceIndex;
    }

    // ===== 恢复事件状态（用于暂停恢复）=====
    public void RestoreEventState(string eventId, int sentenceIndex)
    {
        currentEventId = eventId;
        currentSentenceIndex = sentenceIndex;
        
        // 重新加载事件
        GameEvent evt = EventManager.Instance.GetEvent(eventId);
        if (evt == null)
        {
            Debug.LogWarning($"[UIManager] 无法恢复事件 {eventId}");
            return;
        }
        
        // 重新设置句子列表
        currentEventSentences.Clear();
        currentEventSentences.Add(evt.title);
        if (!string.IsNullOrEmpty(evt.body))
        {
            string[] sentences = evt.body.Split('\n');
            currentEventSentences.AddRange(sentences);
        }
        
        // 从指定句子开始显示
        if (sentenceIndex >= currentEventSentences.Count)
        {
            // 如果索引超出，直接显示选项
            ShowEventOptions(eventId);
        }
        else
        {
            ShowCurrentSentence();
        }
    }

    public void HideEventOptions()
    {
        foreach (var btn in optionButtons)
        {
            if (btn != null)
            {
                btn.gameObject.SetActive(false);
                btn.onClick.RemoveAllListeners();
                btn.GetComponentInChildren<Text>().text = "";
            }
        }
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
                // 先显示商店，再重开游戏
                ShowPolicyShop();
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
            case 1: return "国君上限危机";
            case -1: return "国君下限危机";
            case 2: return "卿士上限危机";
            case -2: return "卿士下限危机";
            case 3: return "贵族上限危机";
            case -3: return "贵族下限危机";
            case 4: return "外臣上限危机";
            case -4: return "外臣下限危机";
            case 5: return "庶人上限危机";
            case -5: return "庶人下限危机";
            case 6: return "事件强制死亡";
            default: return "未知危机";
        }
    }

    // ===== 显示免死道具生效文案 =====
    public void ShowDeathImmunityMessage(string message)
    {
        if (deathImmunityMessagePanel != null)
        {
            deathImmunityMessagePanel.SetActive(true);
        }

        if (deathImmunityMessageText != null)
        {
            deathImmunityMessageText.text = message;
        }

        // 绑定确认按钮
        if (deathImmunityMessageConfirmButton != null)
        {
            deathImmunityMessageConfirmButton.onClick.RemoveAllListeners();
            deathImmunityMessageConfirmButton.onClick.AddListener(() =>
            {
                if (deathImmunityMessagePanel != null)
                {
                    deathImmunityMessagePanel.SetActive(false);
                }
            });
        }
    }

    // ===== 道具菜单弹窗 =====
    public void ShowPolicyMenu()
    {
        if (policyMenuPanel != null) policyMenuPanel.SetActive(true);

        // 清理旧按钮
        foreach (var btn in policyItemButtons)
            if (btn != null) Destroy(btn);
        policyItemButtons.Clear();

        // 显示所有道具
        if (GameControl.Instance == null || GameControl.Instance.stats == null || GameControl.Instance.stats.policyBag == null)
        {
            Debug.LogWarning("[UIManager] 无法获取道具列表");
            return;
        }

        int idx = 0;
        foreach (var item in GameControl.Instance.stats.policyBag)
        {
            int index = idx++; // 捕获显示顺序索引

            GameObject btn = Instantiate(policyItemButtonPrefab, policyItemsParent);
            
            // 设置 DestroyPolicy 组件的 policyItem 字段
            DestroyPolicy destroyPolicy = btn.GetComponent<DestroyPolicy>();
            if (destroyPolicy != null)
            {
                destroyPolicy.policyItem = item;
            }
            
            // ===== 重要：设置 PolicyInShopTrigger 的道具数据 =====
            PolicyInShopTrigger trigger = btn.GetComponent<PolicyInShopTrigger>();
            if (trigger != null)
            {
                trigger.SetPolicyItem(item);
                Debug.Log($"[UIManager] 为背包道具按钮设置数据: {item.name}");
            }
            
            // 设置道具信息显示
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                string usageText = item.usageCount == -1 ? "无限" : item.usageCount.ToString();
                string typeText = GetPolicyTypeName(item.type);
                btnText.text = $"{item.name}\n类型：{typeText}\n次数：{usageText}\n{item.desc}";
            }

            // 绑定点击事件
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                var capturedItem = item;
                button.onClick.AddListener(() => OnPolicyItemClicked(capturedItem, index));
                
                // 根据道具类型决定是否可点击
                // 免死道具(2)和阈值道具(1)不可主动使用
                if (item.type == 1 || item.type == 2)
                {
                    button.interactable = false;
                }
            }

            policyItemButtons.Add(btn);
        }
    }

    public void HidePolicyMenu()
    {
        if (policyMenuPanel != null) policyMenuPanel.SetActive(false);
    }

    // 道具按钮点击处理
    private void OnPolicyItemClicked(PolicyItem item, int index)
    {
        if (item == null) return;

        switch (item.type)
        {
            case 3: // 跳过道具
                UseSkipPolicy(item, index);
                break;
            case 4: // 调控道具
                UseAdjustPolicy(item, index);
                break;
            default:
                Debug.Log($"[UIManager] 道具类型{item.type}不可主动使用");
                break;
        }
    }

    // 使用跳过道具
    private void UseSkipPolicy(PolicyItem item, int index)
    {
        if (item.usageCount == 0)
        {
            Debug.Log("[UIManager] 道具次数已用尽");
            return;
        }

        // 消耗道具
        if (item.usageCount > 0) item.usageCount--;
        
        Debug.Log($"[UIManager] 使用跳过道具：{item.name}");
        
        // 跳过当前事件，直接抽取新事件
        HidePolicyMenu();
        ClearText();
        GameControl.Instance.ProcessNextTurn();
    }

    // 使用调控道具
    private void UseAdjustPolicy(PolicyItem item, int index)
    {
        if (item.usageCount == 0)
        {
            Debug.Log("[UIManager] 道具次数已用尽");
            return;
        }

        // 消耗道具
        if (item.usageCount > 0) item.usageCount--;
        
        Debug.Log($"[UIManager] 使用调控道具：{item.name}");
        
        // 应用数值变化
        if (stats != null)
        {
            stats.king += item.kingChange;
            stats.noble += item.nobleChange;
            stats.scholar += item.scholarChange;
            stats.foreign += item.foreignChange;
            stats.people += item.peopleChange;
            
            UpdateStatText();
            GameControl.Instance?.OnStatsChanged();
        }
        
        HidePolicyMenu();
    }

    // 获取道具类型名称
    private string GetPolicyTypeName(int type)
    {
        switch (type)
        {
            case 1: return "阈值";
            case 2: return "免死";
            case 3: return "跳过";
            case 4: return "调控";
            default: return "未知";
        }
    }

    // ===== 道具商店UI =====
    public void ShowPolicyShop()
    {
        if (policyShopPanel != null) policyShopPanel.SetActive(true);

        // 隐藏结局面板
        HideEndingPanel();

        // 更新可用货币显示
        UpdateCurrencyDisplay();

        // 刷新商店页面的背包显示
        RefreshShopInventoryDisplay();

        // 清理旧商店道具按钮
        foreach (var btn in shopItemButtons)
            if (btn != null) Destroy(btn);
        shopItemButtons.Clear();

        // 获取本轮商店商品（只显示5个随机道具）
        if (PolicyManager.Instance == null)
        {
            Debug.LogError("[UIManager] PolicyManager 未初始化");
            return;
        }

        List<PolicyItem> shopItems = PolicyManager.Instance.GetCurrentShopItems();
        foreach (var policy in shopItems)
        {
            GameObject btn = Instantiate(shopItemButtonPrefab, shopItemsParent);
            
            // ===== 设置 PolicyInShopTrigger 的道具数据 =====
            PolicyInShopTrigger trigger = btn.GetComponent<PolicyInShopTrigger>();
            if (trigger != null)
            {
                trigger.SetPolicyItem(policy);
                Debug.Log($"[UIManager] 为商店道具按钮设置数据: {policy.name}");
            }
            else
            {
                Debug.LogWarning($"[UIManager] 商店道具按钮缺少 PolicyInShopTrigger 组件");
            }
            
            // PolicyInShopTrigger 会自己处理 UI 显示，这里只需绑定点击事件
            // 绑定购买按钮 - 点击后显示购买确认面板
            Button button = btn.GetComponent<Button>();
            if (button != null && trigger != null)
            {
                button.onClick.AddListener(() => trigger.ShowPurchaseConfirm());
            }
            shopItemButtons.Add(btn);
        }

        // 绑定关闭按钮
        if (closeShopButton != null)
        {
            closeShopButton.onClick.RemoveAllListeners();
            closeShopButton.onClick.AddListener(() =>
            {
                HidePolicyShop();
                // 只重开游戏，不再生成商店道具（已在GameControl中生成）
                if (GameControl.Instance != null)
                    GameControl.Instance.RestartGame();
            });
        }
    }

    public void HidePolicyShop()
    {
        if (policyShopPanel != null) policyShopPanel.SetActive(false);
    }

    // ===== 刷新商店页面的背包显示 =====
    public void RefreshShopInventoryDisplay()
    {
        // 清理旧的背包按钮
        foreach (var btn in shopInventoryButtons)
            if (btn != null) Destroy(btn);
        shopInventoryButtons.Clear();

        if (GameControl.Instance == null || GameControl.Instance.stats == null || GameControl.Instance.stats.policyBag == null)
        {
            if (shopInventoryCountText != null)
                shopInventoryCountText.text = "0/5";
            return;
        }

        var policyBag = GameControl.Instance.stats.policyBag;
        int count = policyBag.Count;

        // 更新背包数量显示
        if (shopInventoryCountText != null)
        {
            shopInventoryCountText.text = $"{count}/5";
        }

        // 显示5个槽位（已占用的显示道具，空槽显示空框或占位符）
        List<PolicyItem> itemList = new List<PolicyItem>(policyBag);
        
        for (int i = 0; i < 5; i++)
        {
            if (shopInventoryParent == null || shopInventoryItemPrefab == null) break;

            GameObject btn = Instantiate(shopInventoryItemPrefab, shopInventoryParent);
            
            if (i < itemList.Count)
            {
                // 已占用的槽位：显示道具
                PolicyItem item = itemList[i];
                
                // 设置 PolicyInInventoryTrigger 的道具数据
                PolicyInInventoryTrigger trigger = btn.GetComponent<PolicyInInventoryTrigger>();
                if (trigger != null)
                {
                    trigger.enabled = true; // 确保组件启用
                    trigger.SetPolicyItem(item);
                }
                else
                {
                    Debug.LogWarning("[UIManager] shopInventoryItemPrefab 缺少 PolicyInInventoryTrigger 组件");
                }

                // 设置道具显示
                Text btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = item.name;
                }

                // 绑定点击事件：显示丢弃确认面板
                Button button = btn.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = true; // 确保按钮可点击
                    if (trigger != null)
                    {
                        button.onClick.AddListener(() => trigger.ShowDiscardConfirm());
                    }
                }
            }
            else
            {
                // 空槽位：显示占位符
                Text btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = "空";
                    btnText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 灰色半透明
                }

                // 禁用按钮
                Button button = btn.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = false;
                }
                
                // 禁用触发器组件
                PolicyInInventoryTrigger trigger = btn.GetComponent<PolicyInInventoryTrigger>();
                if (trigger != null)
                {
                    trigger.enabled = false;
                }
            }

            shopInventoryButtons.Add(btn);
        }
    }


    // ===== 时局（BUFF）列表 UI =====
    public void ShowBuffPanel()
    {
        if (buffPanel != null) buffPanel.SetActive(true);
        RefreshBuffListUI();

        // 绑定关闭按钮
        if (buffCloseButton != null)
        {
            buffCloseButton.onClick.RemoveAllListeners();
            buffCloseButton.onClick.AddListener(() => HideBuffPanel());
        }

        // 不再绑定丢弃按钮
    }

    public void HideBuffPanel()
    {
        if (buffPanel != null) buffPanel.SetActive(false);
        selectedBuffId = null;
        if (buffDetailText != null) buffDetailText.text = "";
    }

    private void RefreshBuffListUI()
    {
        // 清空旧的按钮
        foreach (var go in buffItemButtons)
            if (go != null) Destroy(go);
        buffItemButtons.Clear();

        if (BuffManager.Instance == null || BuffManager.Instance.GetActiveBuffs() == null)
        {
            if (buffDetailText != null) buffDetailText.text = "暂无时局";
            return;
        }

        int idx = 0;
        foreach (var buff in BuffManager.Instance.GetActiveBuffs())
        {
            var btnGo = Instantiate(buffItemButtonPrefab, buffItemsParent);
            var rect = btnGo.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition3D = Vector3.zero;
            }

            // 按钮文字：显示名称 + 剩余时限
            var txt = btnGo.GetComponentInChildren<Text>();
            if (txt != null)
            {
                string dur = buff.duration < 0 ? "∞" : buff.duration.ToString();
                txt.text = $"{buff.name ?? buff.id}  (时限:{dur})";
            }

            var button = btnGo.GetComponent<Button>();
            if (button != null)
            {
                var captured = buff; // 捕获
                button.onClick.AddListener(() => OnBuffItemClicked(captured));
            }

            buffItemButtons.Add(btnGo);
            idx++;
        }

        // 默认选中第一个，显示详情
        if (BuffManager.Instance.GetActiveBuffs().Count > 0)
        {
            OnBuffItemClicked(BuffManager.Instance.GetActiveBuffs()[0]);
        }
        else
        {
            selectedBuffId = null;
            if (buffDetailText != null) buffDetailText.text = "暂无时局";
        }
    }

    private void OnBuffItemClicked(BuffDefinition buff)
    {
        if (buff == null) return;
        selectedBuffId = buff.id;

        if (buffDetailText != null)
        {
            // 组装详细描述
            string dur = buff.duration < 0 ? "∞" : buff.duration.ToString();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"名称：{(buff.name ?? buff.id)}");
            sb.AppendLine($"时限：{dur}");
            if (!string.IsNullOrEmpty(buff.description))
                sb.AppendLine($"描述：{buff.description}");

            // 显示数值变化
            bool hasChanges = false;
            if (buff.kingChange != 0)
            {
                if (!hasChanges) { sb.AppendLine("每年数值变化："); hasChanges = true; }
                sb.AppendLine($" - 国君: {(buff.kingChange >= 0 ? "+" : "")}{buff.kingChange}");
            }
            if (buff.nobleChange != 0)
            {
                if (!hasChanges) { sb.AppendLine("每年数值变化："); hasChanges = true; }
                sb.AppendLine($" - 贵族: {(buff.nobleChange >= 0 ? "+" : "")}{buff.nobleChange}");
            }
            if (buff.scholarChange != 0)
            {
                if (!hasChanges) { sb.AppendLine("每年数值变化："); hasChanges = true; }
                sb.AppendLine($" - 士族: {(buff.scholarChange >= 0 ? "+" : "")}{buff.scholarChange}");
            }
            if (buff.foreignChange != 0)
            {
                if (!hasChanges) { sb.AppendLine("每年数值变化："); hasChanges = true; }
                sb.AppendLine($" - 外臣: {(buff.foreignChange >= 0 ? "+" : "")}{buff.foreignChange}");
            }
            if (buff.peopleChange != 0)
            {
                if (!hasChanges) { sb.AppendLine("每年数值变化："); hasChanges = true; }
                sb.AppendLine($" - 国人: {(buff.peopleChange >= 0 ? "+" : "")}{buff.peopleChange}");
            }

            buffDetailText.text = sb.ToString();
        }
    }


    // 更新货币显示
    public void UpdateCurrencyDisplay()
    {
        if (currencyText != null && GameControl.Instance != null)
        {
            int currency = GameControl.Instance.GetCurrency();
            currencyText.text = $"货币：{currency}";
        }
    }

    // ===== 已废弃：以下购买相关方法不再使用，现由 PolicyInShopTrigger 管理 =====
    /*
    // 商店道具点击（显示购买确认面板）
    private void OnShopItemClicked(PolicyItem policy)
    {
        if (GameControl.Instance == null || policy == null) return;

        // 从 PolicyInShopTrigger 获取实际价格
        PolicyInShopTrigger trigger = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject?.GetComponent<PolicyInShopTrigger>();
        int price = (trigger != null && trigger.policyValue > 0) ? trigger.policyValue : GetPolicyPrice(policy);
        
        // 保存待购买信息
        pendingPurchaseItem = policy;
        pendingPurchasePrice = price;
        pendingPurchaseButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        // 显示购买确认面板
        ShowShopPurchaseConfirmPanel(policy, price);
    }

    // 显示商店购买确认面板
    private void ShowShopPurchaseConfirmPanel(PolicyItem policy, int price)
    {
        if (shopPurchaseConfirmPanel == null)
        {
            Debug.LogWarning("[UIManager] shopPurchaseConfirmPanel 未设置");
            return;
        }

        int currency = GameControl.Instance.GetCurrency();
        
        // 构建提示文本
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"<b>{policy.name}</b>");
        sb.AppendLine($"<color=#FFD700>━━━━━━━━━━</color>");
        sb.AppendLine($"<color=#FFD700>价格：{price} 年</color>");
        sb.AppendLine($"<color=#87CEEB>当前货币：{currency} 年</color>");
        sb.AppendLine($"<color=#FFD700>━━━━━━━━━━</color>");
        
        if (!string.IsNullOrEmpty(policy.desc))
        {
            sb.AppendLine($"\n{policy.desc}");
        }

        // 检查是否可购买
        bool canPurchase = true;
        if (currency < price)
        {
            sb.AppendLine($"\n<color=#FF6B6B>货币不足！</color>");
            canPurchase = false;
        }
        if (GameControl.Instance.stats.policyBag.Count >= 5)
        {
            sb.AppendLine($"\n<color=#FF6B6B>背包已满（最多5件）！</color>");
            canPurchase = false;
        }
        if (GameControl.Instance.GetPolicy(policy.id) != null)
        {
            sb.AppendLine($"\n<color=#FF6B6B>已拥有此道具！</color>");
            canPurchase = false;
        }

        if (shopPurchaseInfoText != null)
        {
            shopPurchaseInfoText.text = sb.ToString();
        }

        // 根据是否可购买设置按钮状态
        if (shopPurchaseConfirmButton != null)
        {
            shopPurchaseConfirmButton.interactable = canPurchase;
        }

        shopPurchaseConfirmPanel.SetActive(true);
        Debug.Log($"[UIManager] 显示购买确认面板: {policy.name}, 价格: {price}");
    }

    // 确认购买
    private void OnShopPurchaseConfirm()
    {
        if (pendingPurchaseItem == null || GameControl.Instance == null)
        {
            Debug.LogWarning("[UIManager] pendingPurchaseItem 或 GameControl 为空");
            OnShopPurchaseCancel();
            return;
        }

        // 扣除货币
        GameControl.Instance.SpendCurrency(pendingPurchasePrice);

        // 添加道具到背包（从 PolicyManager 获取副本）
        PolicyItem newItem = PolicyManager.Instance.GetPolicy(pendingPurchaseItem.id);
        if (newItem != null)
        {
            GameControl.Instance.AddPolicy(newItem);
            Debug.Log($"[UIManager] 购买成功：{pendingPurchaseItem.name}，花费 {pendingPurchasePrice} 年");
        }

        // 从商店列表中移除该道具按钮
        if (pendingPurchaseButton != null)
        {
            shopItemButtons.Remove(pendingPurchaseButton);
            Destroy(pendingPurchaseButton);
            Debug.Log($"[UIManager] 从商店移除道具按钮: {pendingPurchaseItem.name}");
        }

        // 更新货币显示
        UpdateCurrencyDisplay();
        
        // 刷新商店页面的背包显示
        RefreshShopInventoryDisplay();

        // 关闭确认面板
        OnShopPurchaseCancel();
    }

    // 取消购买
    private void OnShopPurchaseCancel()
    {
        if (shopPurchaseConfirmPanel != null)
        {
            shopPurchaseConfirmPanel.SetActive(false);
        }
        pendingPurchaseItem = null;
        pendingPurchaseButton = null;
        pendingPurchasePrice = 0;
    }
    */

}