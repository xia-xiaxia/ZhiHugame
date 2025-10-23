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
    private int currentYear;
    public Text currentYearText;

    public Button[] optionButtons = new Button[4]; // 在Inspector拖入4个选项按钮

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

    void Awake() { Instance = this; }

    void Start()
    {
        if (daDian != null) daDian.SetActive(true);
        if (endingPanel != null) endingPanel.SetActive(false);
    
        
        // 绑定游戏内退出按钮
        if (exitToMenuButton != null)
        {
            exitToMenuButton.onClick.RemoveAllListeners();
            exitToMenuButton.onClick.AddListener(OnExitToMenuClicked);
        }

        // 启动游戏（确保 GameControl 已在场景中）
        if (GameControl.Instance != null)
            GameControl.Instance.StartGame();
            
    }

    // 游戏内退出到主菜单
    private void OnExitToMenuClicked()
    {
        if (GameControl.Instance != null)
        {
            GameControl.Instance.PauseAndBackToMenu();
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
        var evt = EventManager.Instance.GetEvent(id);
        if (evt == null)
        {
            Debug.LogError($"无法找到事件ID: {id}");
            return;
        }
        titleText.text = evt.title;
        // 按句分割正文（可按'\n'或其它分隔符）
        currentEventSentences = new List<string>(evt.body.Split('\n'));
        currentSentenceIndex = 0;
        if (currentEventSentences.Count <= 1)
        {
            // 只有一句，正文显示后延迟显示选项
            dialoguePanel.SetBody(evt.body);
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
            dialoguePanel.SetBody(currentEventSentences[currentSentenceIndex]);
            // 自动延迟显示下一句
            currentSentenceIndex++;
            if (currentSentenceIndex < currentEventSentences.Count)
            {
                // 这里可用协程实现自动延迟（如1秒），也可直接递归调用（立即显示）
                StartCoroutine(AutoShowNextSentence(1.0f)); // 1秒后自动显示下一句
            }
            else
            {
                // 全部显示完毕，显示选项
                StartCoroutine(ShowOptionsAfterDelay(1.0f));
            }
        }
    }

    private System.Collections.IEnumerator AutoShowNextSentence(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowCurrentSentence();
    }

    private System.Collections.IEnumerator ShowOptionsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowEventOptions(currentEventId); // 用当前事件id
        waitingForSentence = false;
    }

    // 新增：外部调用，显示下一句
    public void ShowNextSentence()
    {
        if (!waitingForSentence) return;
        currentSentenceIndex++;
        ShowCurrentSentence();
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
        if (GameControl.Instance == null || GameControl.Instance.inventory == null)
        {
            Debug.LogWarning("[UIManager] 无法获取道具列表");
            return;
        }

        int idx = 0;
        // 修复：inventory 为按字符串键索引的集合，改用 Values 遍历，避免整数下标访问
        foreach (var item in GameControl.Instance.inventory.Values)
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
            
            // 设置道具信息显示
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.text = policy.name+policy.desc;
            }
            // 绑定购买按钮
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                var capturedPolicy = policy;
                button.onClick.AddListener(() => OnShopItemClicked(capturedPolicy));
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
    private void RefreshShopInventoryDisplay()
    {
        // 清理旧的背包按钮
        foreach (var btn in shopInventoryButtons)
            if (btn != null) Destroy(btn);
        shopInventoryButtons.Clear();

        if (GameControl.Instance == null || GameControl.Instance.inventory == null)
        {
            if (shopInventoryCountText != null)
                shopInventoryCountText.text = "0/5";
            return;
        }

        var inventory = GameControl.Instance.inventory;
        int count = inventory.Count;

        // 更新背包数量显示
        if (shopInventoryCountText != null)
        {
            shopInventoryCountText.text = $"{count}/5";
        }

        // 显示每个背包道具
        foreach (var item in inventory.Values)
        {
            if (shopInventoryParent == null || shopInventoryItemPrefab == null) break;

            GameObject btn = Instantiate(shopInventoryItemPrefab, shopInventoryParent);

            // 设置 PolicyInShopTrigger 的道具数据（用于鼠标悬停显示详情）
            PolicyInShopTrigger trigger = btn.GetComponent<PolicyInShopTrigger>();
            if (trigger != null)
            {
                trigger.SetPolicyItem(item);
            }

            // 设置道具显示（简化版，只显示名称或图标）
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.text = item.name;
            }

            // 绑定点击事件：确认是否丢弃
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                var capturedItem = item;
                button.onClick.AddListener(() => OnShopInventoryItemClicked(capturedItem));
            }

            shopInventoryButtons.Add(btn);
        }
    }

    // 商店页面背包道具点击事件：询问是否丢弃
    private void OnShopInventoryItemClicked(PolicyItem item)
    {
        if (item == null || GameControl.Instance == null) return;

        // TODO: 这里可以添加确认弹窗UI
        // 目前使用Debug.Log模拟确认，实际应该弹出确认对话框
        Debug.Log($"[UIManager] 点击背包道具: {item.name}，是否丢弃？");
        
        // 简化处理：直接丢弃（您可以根据需要添加确认弹窗）
        // 例如：ShowConfirmDialog("确定要丢弃道具吗？", () => { 丢弃逻辑 });
        
        bool confirmed = true; // 临时自动确认
        
        if (confirmed)
        {
            Debug.Log($"[UIManager] 确认丢弃道具: {item.name}");
            GameControl.Instance.RemovePolicy(item.id);
            
            // 刷新背包显示
            RefreshShopInventoryDisplay();
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
    private void UpdateCurrencyDisplay()
    {
        if (currencyText != null && GameControl.Instance != null)
        {
            int currency = GameControl.Instance.GetCurrency();
            currencyText.text = $"货币：{currency}";
        }
    }

    // 商店道具点击（购买）
    private void OnShopItemClicked(PolicyItem policy)
    {
        if (GameControl.Instance == null) return;

        int price = GetPolicyPrice(policy);
        int currency = GameControl.Instance.GetCurrency();

        // 检查货币是否足够
        if (currency < price)
        {
            Debug.Log($"[UIManager] 货币不足，需要 {price} 年，当前只有 {currency} 年");
            return;
        }

        // 检查背包是否已满
        if (GameControl.Instance.inventory.Count >= 5)
        {
            Debug.Log("[UIManager] 背包已满（最多5件道具）");
            return;
        }

        // 检查是否已拥有相同ID的道具（按ID为键）
        bool alreadyOwned = GameControl.Instance.inventory.ContainsKey(policy.id);
        if (alreadyOwned)
        {
            Debug.Log($"[UIManager] 已拥有道具：{policy.name}");
            return;
        }

        // 扣除货币（这里需要GameControl提供扣除方法）
        GameControl.Instance.SpendCurrency(price);

        // 添加道具到背包（从 PolicyManager 获取副本）
        PolicyItem newItem = PolicyManager.Instance.GetPolicy(policy.id);
        if (newItem != null)
        {
            GameControl.Instance.inventory.Add(newItem.id, newItem);
            Debug.Log($"[UIManager] 购买成功：{policy.name}，花费 {price} 年");
            
            // 更新货币显示
            UpdateCurrencyDisplay();
            
            // 刷新商店页面的背包显示
            RefreshShopInventoryDisplay();
        }
    }

    // 计算道具价格（可自定义规则）
    private int GetPolicyPrice(PolicyItem policy)
    {
        switch (policy.type)
        {
            case 1: return 30; // 阈值道具
            case 2: return 50; // 免死道具
            case 3: return 20; // 跳过道具
            case 4: return 40; // 调控道具
            default: return 10;
        }
    }
}