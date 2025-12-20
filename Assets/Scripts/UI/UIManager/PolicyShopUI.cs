using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 道具商店UI：负责结局后的道具购买和背包显示
/// </summary>
public class PolicyShopUI : MonoBehaviour
{
    public static PolicyShopUI Instance;

    [Header("商店面板组件")]
    public GameObject policyShopPanel;
    public List<GameObject> shopPolicyItems; // 商店中用于显示道具的槽位预设
    public GameObject shopItemButtonPrefab;
    public Text currencyText;
    public Button restartGameButton;
    public Button backToMenuButton;

    [Header("商店背包显示")]
    public Transform shopInventoryParent;
    public GameObject shopInventoryItemPrefab;
    public Text shopInventoryCountText;

    private List<GameObject> shopItemButtons = new List<GameObject>();
    private List<GameObject> shopInventoryButtons = new List<GameObject>();
    
    // 避免显示多个确认面版导致覆盖
    public Queue<GameObject> activePurchaseConfirms = new Queue<GameObject>();

    // 刷新相关
    private int maxRefreshCount = 4;
    private int[] refreshCosts = new int[4] { 5, 10, 20, 50 };
    private int refreshCount = 0;
    private float currentCurrency = 0;

    void Awake()
    {
        Instance = this;
        if(UIManager.Instance != null)
        {
            maxRefreshCount = UIManager.Instance.maxRefreshCount;
            refreshCosts = UIManager.Instance.refreshCosts;
            currencyText = UIManager.Instance.CurrencyText;
            currentCurrency = 0;
        }
        refreshCount = 0;
    }

    /// <summary>
    /// 显示商店
    /// </summary>
    public void ShowShop()
    {
        if (policyShopPanel != null) policyShopPanel.SetActive(true);

        // 隐藏结局面板
        EndingUI.Instance?.HideEnding();

        // 停止结局音乐
        MusicManager.Instance?.StopBgm();

        // 更新货币和背包显示
        UpdateCurrencyDisplay();
        RefreshInventoryDisplay();

        // 清空旧商店按钮列表（不销毁对象，因为 shopPolicyItems 会重用）
        shopItemButtons.Clear();

        // 获取商店商品
        if (PolicyManager.Instance == null)
        {
            Debug.LogError("[PolicyShopUI] PolicyManager 未初始化");
            return;
        }

        List<PolicyItem> shopItems = PolicyManager.Instance.GetCurrentShopItems();
        
        // 确保有足够的槽位
        int maxDisplaySlots = 8; // 显示的最大槽位数
        if (shopPolicyItems.Count < maxDisplaySlots)
        {
            Debug.LogWarning($"[PolicyShopUI] shopPolicyItems 只有 {shopPolicyItems.Count} 个，建议至少配置 {maxDisplaySlots} 个");
            maxDisplaySlots = shopPolicyItems.Count;
        }
        
        // 只取前 maxDisplaySlots 个槽位用于洗牌
        List<GameObject> availableSlots = new List<GameObject>(shopPolicyItems);

        // 对 availableSlots 进行随机洗牌 (Fisher-Yates Shuffle 算法)
        for (int i = 0; i < availableSlots.Count; i++)
        {
            GameObject temp = availableSlots[i];
            int randomIndex = Random.Range(i, availableSlots.Count);
            availableSlots[i] = availableSlots[randomIndex];
            availableSlots[randomIndex] = temp;
        }
        
        // 先隐藏所有槽位
        foreach (var slot in shopPolicyItems)
        {
            if (slot != null) slot.SetActive(false);
        }
        
        // 显示商品
        int displayCount = Mathf.Min(shopItems.Count, availableSlots.Count);
        for (int i = 0; i < displayCount; i++)
        {
            PolicyItem policy = shopItems[i];
            GameObject btn = availableSlots[i];

            // 设置触发器
            PolicyInShopTrigger trigger = btn.GetComponent<PolicyInShopTrigger>();
            if (trigger != null)
            {
                trigger.SetPolicyItem(policy);
            }

            // 绑定点击事件
            Button button = btn.GetComponent<Button>();
            if (button != null && trigger != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => trigger.ShowPurchaseConfirm());
            }

            shopItemButtons.Add(btn);
            btn.SetActive(true);
        }
        
        Debug.Log($"[PolicyShopUI] 商店显示完成：生成了 {shopItems.Count} 个商品，显示了 {displayCount} 个");

        // 绑定按钮
        BindButtons();
    }

    // 刷新道具
    public void RefreshShopItems()
    {
        if(UIManager.Instance != null)
        {
            if(refreshCount >= maxRefreshCount) 
            {
                Debug.Log("[PolicyShopUI] 道具刷新次数已达上限，无法继续刷新");
                return;
            }
            if(UIManager.Instance.stats.currency < refreshCosts[refreshCount])
            {
                Debug.Log("[PolicyShopUI] 货币不足，无法刷新道具");
                return;
            }
            refreshCount++;
            UIManager.Instance.stats.currency -= refreshCosts[refreshCount - 1];
            UpdateCurrencyDisplay();
            Debug.Log($"[PolicyShopUI] 道具刷新次数：{refreshCount}/{maxRefreshCount}");
        }
        // 获取商店商品
        if (PolicyManager.Instance == null)
        {
            Debug.LogError("[PolicyShopUI] PolicyManager 未初始化");
            return;
        }
        PolicyManager.Instance.GenerateShopItems(8);
        List<PolicyItem> shopItems = PolicyManager.Instance.GetCurrentShopItems();
        
        // 确保有足够的槽位
        int maxDisplaySlots = 8; // 显示的最大槽位数
        if (shopPolicyItems.Count < maxDisplaySlots)
        {
            Debug.LogWarning($"[PolicyShopUI] shopPolicyItems 只有 {shopPolicyItems.Count} 个，建议至少配置 {maxDisplaySlots} 个");
            maxDisplaySlots = shopPolicyItems.Count;
        }
        
        // 过滤掉已被销毁的槽位，只保留有效的
        List<GameObject> validSlots = new List<GameObject>();
        foreach (var slot in shopPolicyItems)
        {
            if (slot != null)
            {
                validSlots.Add(slot);
            }
            else
            {
                Debug.LogWarning("[PolicyShopUI] RefreshShopItems: 发现已被销毁的槽位");
            }
        }
        
        if (validSlots.Count == 0)
        {
            Debug.LogError("[PolicyShopUI] RefreshShopItems: 所有槽位都已被销毁！");
            return;
        }
        
        // 只取前 maxDisplaySlots 个有效槽位用于洗牌
        List<GameObject> availableSlots = validSlots.GetRange(0, Mathf.Min(maxDisplaySlots, validSlots.Count));

        // 对 availableSlots 进行随机洗牌 (Fisher-Yates Shuffle 算法)
        for (int i = 0; i < availableSlots.Count; i++)
        {
            GameObject temp = availableSlots[i];
            int randomIndex = Random.Range(i, availableSlots.Count);
            availableSlots[i] = availableSlots[randomIndex];
            availableSlots[randomIndex] = temp;
        }
        
        // 先隐藏所有有效槽位
        foreach (var slot in validSlots)
        {
            if (slot != null) slot.SetActive(false);
        }
        
        // 显示商品
        int displayCount = Mathf.Min(shopItems.Count, availableSlots.Count);
        for (int i = 0; i < displayCount; i++)
        {
            PolicyItem policy = shopItems[i];
            GameObject btn = availableSlots[i];

            // 再次检查对象是否有效
            if (btn == null)
            {
                Debug.LogWarning($"[PolicyShopUI] RefreshShopItems: 槽位 {i} 在使用前被销毁");
                continue;
            }

            // 设置触发器
            PolicyInShopTrigger trigger = btn.GetComponent<PolicyInShopTrigger>();
            if (trigger != null)
            {
                trigger.SetPolicyItem(policy);
            }

            // 绑定点击事件
            Button button = btn.GetComponent<Button>();
            if (button != null && trigger != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => trigger.ShowPurchaseConfirm());
            }

            shopItemButtons.Add(btn);
            btn.SetActive(true);
        }
        
        Debug.Log($"[PolicyShopUI] 刷新商店完成：生成了 {shopItems.Count} 个商品，显示了 {displayCount} 个");

    }

    public int GetRefreshCount()
    {
        return refreshCount;
    }
    public void reRefreshCount(int value)
    {
        refreshCount = value;
    }

    /// <summary>
    /// 绑定按钮事件
    /// </summary>
    private void BindButtons()
    {
        if (restartGameButton != null)
        {
            restartGameButton.onClick.RemoveAllListeners();
            restartGameButton.onClick.AddListener(() =>
            {
                MusicManager.Instance?.PlayButtonSound3();
                GameControl.Instance?.RestartGameWithAnimation();
            });
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveAllListeners();
            backToMenuButton.onClick.AddListener(() =>
            {
                MusicManager.Instance?.PlayButtonSound3();
                GameControl.Instance?.BackToMainMenu();
            });
        }
    }

    /// <summary>
    /// 更新货币显示
    /// </summary>
    public void UpdateCurrencyDisplay()
    {
        if (currencyText != null && GameControl.Instance != null)
        {
            StartCoroutine(AnimateYearCounter(GameControl.Instance.GetCurrency()));
        }
    }

    /// <summary>
    /// 刷新背包显示
    /// </summary>
    public void RefreshInventoryDisplay()
    {
        // 清理旧按钮
        foreach (var btn in shopInventoryButtons)
            if (btn != null) Destroy(btn);
        shopInventoryButtons.Clear();

        if (GameControl.Instance == null || GameControl.Instance.stats == null)
        {
            if (shopInventoryCountText != null)
                shopInventoryCountText.text = "0/5";
            return;
        }

        var policyBag = GameControl.Instance.stats.policyBag;
        int count = policyBag.Count;

        // 更新数量
        if (shopInventoryCountText != null)
        {
            shopInventoryCountText.text = $"{count}/5";
        }

        // 显示8个槽位
        List<PolicyItem> itemList = new List<PolicyItem>(policyBag);

        for (int i = 0; i < 8; i++)
        {
            if (shopInventoryParent == null || shopInventoryItemPrefab == null) break;

            GameObject btn = Instantiate(shopInventoryItemPrefab, shopInventoryParent);

            if (i < itemList.Count)
            {
                // 已占用槽位
                PolicyItem item = itemList[i];

                PolicyInInventoryTrigger trigger = btn.GetComponent<PolicyInInventoryTrigger>();
                if (trigger != null)
                {
                    trigger.enabled = true;
                    trigger.SetPolicyItem(item);
                }

                Text btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = item.name;
                }

                Button button = btn.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = true;
                    if (trigger != null)
                    {
                        button.onClick.AddListener(() => trigger.ShowDiscardConfirm());
                    }
                }
            }
            else
            {
                // 空槽位
                Text btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = "空";
                    btnText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }

                Button button = btn.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = false;
                }

                PolicyInInventoryTrigger trigger = btn.GetComponent<PolicyInInventoryTrigger>();
                if (trigger != null)
                {
                    trigger.enabled = false;
                }
            }

            shopInventoryButtons.Add(btn);
        }
    }

    /// <summary>
    /// 隐藏商店
    /// </summary>
    public void HideShop()
    {
        if(activePurchaseConfirms.Count > 0)
        {
            while(activePurchaseConfirms.Count > 0)
            {
                GameObject purchaseConfirm = activePurchaseConfirms.Dequeue();
                if (purchaseConfirm != null)
                {
                    purchaseConfirm.SetActive(false);
                }
            }
        }
        if(UIManager.Instance != null)
        {
            if(UIManager.Instance.policyTooltipPanel != null)
            {
                UIManager.Instance.policyTooltipText.text = "";
                UIManager.Instance.policyTooltipPanel.SetActive(false);
            };
        }
        if (policyShopPanel != null)
        {
            policyShopPanel.SetActive(false);
        }
    }

    // 第一次显示shop时播放货币增加动画
    private IEnumerator AnimateYearCounter(int targetCurrency)
    {
        float duration = 2.0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float easedProgress = EaseInOutCubic(progress);
            
            currentCurrency = Mathf.FloorToInt(Mathf.Lerp(currentCurrency, targetCurrency, easedProgress));
            
            if (currencyText != null)
            {
                currencyText.text = $"{currentCurrency}";
            }

            yield return null;
        }

        // 确保最终显示精确值
        if (currencyText != null)
        {
            currencyText.text = $"{targetCurrency}";
        }
    }

    /// 缓动函数：慢 -> 快 -> 慢
    private float EaseInOutCubic(float t)
    {
        if (t < 0.5f)
        {
            return 4f * t * t * t;
        }
        else
        {
            float f = (2f * t - 2f);
            return 0.5f * f * f * f + 1f;
        }
    }

    // 获取打开的确认面板数量
    public int GetActivePurchaseConfirmCount()
    {
        return activePurchaseConfirms.Count;
    }

}
