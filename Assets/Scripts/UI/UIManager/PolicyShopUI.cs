using System.Collections.Generic;
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
    public Transform shopItemsParent;
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

    void Awake()
    {
        Instance = this;
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

        // 清理旧商店按钮
        foreach (var btn in shopItemButtons)
            if (btn != null) Destroy(btn);
        shopItemButtons.Clear();

        // 获取商店商品
        if (PolicyManager.Instance == null)
        {
            Debug.LogError("[PolicyShopUI] PolicyManager 未初始化");
            return;
        }

        List<PolicyItem> shopItems = PolicyManager.Instance.GetCurrentShopItems();
        foreach (var policy in shopItems)
        {
            GameObject btn = Instantiate(shopItemButtonPrefab, shopItemsParent);

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
                button.onClick.AddListener(() => trigger.ShowPurchaseConfirm());
            }

            shopItemButtons.Add(btn);
        }

        // 绑定按钮
        BindButtons();
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
            int currency = GameControl.Instance.GetCurrency();
            currencyText.text = $"经验：{currency}";
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

        // 显示5个槽位
        List<PolicyItem> itemList = new List<PolicyItem>(policyBag);

        for (int i = 0; i < 5; i++)
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
        if (policyShopPanel != null)
        {
            policyShopPanel.SetActive(false);
        }
    }
}
