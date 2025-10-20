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

    public Button[] optionButtons = new Button[4]; // 在Inspector拖入4个选项按钮

    // ===== 新增：结局面板（由 GameControl 统一触发）=====
    public GameObject endingPanel;
    public Text endingText;
    public Button restartButton;

    // ===== 新增：免死道具确认弹窗 =====
    public GameObject deathImmunityPanel;
    public Text deathImmunityText;
    public Button useDeathImmunityButton;
    public Button declineDeathImmunityButton;

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

    private List<string> currentEventSentences = new List<string>();
    private int currentSentenceIndex = 0;
    private bool waitingForSentence = false;
    private string currentEventId = "";

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
        currentEventId = id; // 记录当前事件id
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
                    UpdateStatText();
                    if (!string.IsNullOrEmpty(opt.nextEventId))
                    {
                        EventManager.Instance.SetNextEventId(opt.nextEventId);
                    }
                    ClearText();
                    GameControl.Instance.ProcessNextTurn();
                });
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
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

        // 原先这里的越界 -> GameOver 判定已移交 GameControl
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
            case 1: return "国君势力失衡";
            case 2: return "士族势力失衡";
            case 3: return "贵族势力失衡";
            case 4: return "外臣势力失衡";
            case 5: return "庶人势力失衡";
            case 6: return "事件强制死亡";
            default: return "未知危机";
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

        for (int i = 0; i < GameControl.Instance.inventory.Count; i++)
        {
            var item = GameControl.Instance.inventory[i];
            int index = i; // 捕获索引

            GameObject btn = Instantiate(policyItemButtonPrefab, policyItemsParent);
            
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
                button.onClick.AddListener(() => OnPolicyItemClicked(item, index));
                
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

        // 清理旧按钮
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
            // 自动归零本地缩放和位置，适配布局组件
            var rect = btn.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition3D = Vector3.zero;
                rect.offsetMin = new Vector2(rect.offsetMin.x, rect.offsetMin.y);
                rect.offsetMax = new Vector2(rect.offsetMax.x, rect.offsetMax.y);
            }
            // 设置道具信息显示
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.text = policy.name;
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

    // 更新货币显示
    private void UpdateCurrencyDisplay()
    {
        if (currencyText != null && GameControl.Instance != null)
        {
            int currency = GameControl.Instance.GetCurrency();
            currencyText.text = $"金币：{currency}";
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

        // 检查是否已拥有相同ID的道具
        bool alreadyOwned = GameControl.Instance.inventory.Exists(item => item.id == policy.id);
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
            GameControl.Instance.inventory.Add(newItem);
            Debug.Log($"[UIManager] 购买成功：{policy.name}，花费 {price} 年");
            // 更新货币显示
            UpdateCurrencyDisplay();
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