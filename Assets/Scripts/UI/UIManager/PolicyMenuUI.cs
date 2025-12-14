using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 道具菜单UI：负责游戏中背包道具的显示和使用
/// </summary>
public class PolicyMenuUI : MonoBehaviour
{
    public static PolicyMenuUI Instance;

    [Header("道具菜单组件")]
    public GameObject policyMenuPanel;
    public Transform policyItemsParent;
    public GameObject policyItemButtonPrefab;

    [Header("数据源")]
    public StatModel stats;

    private List<GameObject> policyItemButtons = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 显示道具菜单
    /// </summary>
    public void ShowMenu()
    {
        Debug.Log("[PolicyMenuUI] ShowMenu 被调用");
        
        if (policyMenuPanel != null) 
        {
            policyMenuPanel.SetActive(true);
            Debug.Log("[PolicyMenuUI] 道具菜单面板已激活");
        }
        else
        {
            Debug.LogError("[PolicyMenuUI] policyMenuPanel 为空！");
        }

        RefreshPolicyList();
    }

    /// <summary>
    /// 刷新道具列表
    /// </summary>
    private void RefreshPolicyList()
    {
        Debug.Log("[PolicyMenuUI] RefreshPolicyList 开始");
        
        // 清理旧按钮
        foreach (var btn in policyItemButtons)
            if (btn != null) Destroy(btn);
        policyItemButtons.Clear();

        if (GameControl.Instance == null)
        {
            Debug.LogError("[PolicyMenuUI] GameControl.Instance 为空");
            return;
        }
        
        if (stats == null)
        {
            Debug.LogError("[PolicyMenuUI] stats 为空，尝试从 GameControl 获取");
            stats = GameControl.Instance.stats;
            if (stats == null)
            {
                Debug.LogError("[PolicyMenuUI] 从 GameControl 也无法获取 stats");
                return;
            }
        }
        
        if (stats.policyBag == null)
        {
            Debug.LogError("[PolicyMenuUI] stats.policyBag 为空");
            return;
        }

        if (policyItemsParent == null)
        {
            Debug.LogError("[PolicyMenuUI] policyItemsParent 为空");
            return;
        }
        
        if (policyItemButtonPrefab == null)
        {
            Debug.LogError("[PolicyMenuUI] policyItemButtonPrefab 为空");
            return;
        }

        Debug.Log($"[PolicyMenuUI] 背包中有 {stats.policyBag.Count} 个道具，开始创建UI");

        int idx = 0;
        foreach (var item in stats.policyBag)
        {
            int index = idx++;
            GameObject btn = Instantiate(policyItemButtonPrefab, policyItemsParent);

            // 设置 PolicyInShopTrigger
            PolicyInShopTrigger trigger = btn.GetComponent<PolicyInShopTrigger>();
            if (trigger != null)
            {
                trigger.SetPolicyItem(item);
            }

            // 设置 DestroyPolicy
            DestroyPolicy destroyPolicy = btn.GetComponent<DestroyPolicy>();
            if (destroyPolicy != null)
            {
                destroyPolicy.policyItem = item;
            }

            // 设置显示文本
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                string usageText = item.usageCount == -1 ? "无限" : item.usageCount.ToString();
                string typeText = GetPolicyTypeName(item.type);
                btnText.text = $"{item.name} 类型：{typeText} 次数：{usageText}\n{item.desc}\n{item.result}";
            }

            // 绑定点击事件
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                var capturedItem = item;
                button.onClick.AddListener(() => OnPolicyClicked(capturedItem, index));

                // 免死道具不可主动使用
                if (item.type == 2)
                {
                    button.interactable = false;
                }
            }

            policyItemButtons.Add(btn);
        }
    }

    /// <summary>
    /// 道具点击处理
    /// </summary>
    private void OnPolicyClicked(PolicyItem item, int index)
    {
        if (item == null) return;

        switch (item.type)
        {
            case 1: // 阈值道具
                UseThresholdPolicy(item);
                break;
            case 3: // 跳过道具
                UseSkipPolicy(item);
                break;
            case 4: // 调控道具
                UseAdjustPolicy(item);
                break;
            default:
                Debug.Log($"[PolicyMenuUI] 道具类型{item.type}不可主动使用");
                break;
        }
    }

    /// <summary>
    /// 使用锁定道具
    /// </summary>
    private void UseThresholdPolicy(PolicyItem item)
    {
        if (item.usageCount == 0) return;

        // 消耗次数
        if (item.usageCount > 0) item.usageCount--;
        if (item.usageCount == 0)
        {
            GameControl.Instance?.RemovePolicy(item.id);
        }

        // 应用锁定效果
        if (stats != null && item.targetLayers != null)
        {
            // 根据 targetLayers 锁定对应的数值
            // 这里需要实现锁定逻辑，例如设置锁定标志
            // TODO: 实现具体的锁定机制（例如在StatModel中添加锁定字段）
            
            foreach (int layer in item.targetLayers)
            {
                int absLayer = System.Math.Abs(layer);
                bool lockIncrease = layer > 0; // 正数禁止上升，负数禁止下降
                
                Debug.Log($"[PolicyMenuUI] 锁定道具效果：阶层{absLayer}，{(lockIncrease ? "禁止上升" : "禁止下降")}，持续{item.lockDuration}年");
                
                // 这里应该调用相应的锁定方法
                // 例如: stats.LockLayer(absLayer, lockIncrease, item.lockDuration);
            }
            
            Debug.Log($"[PolicyMenuUI] 使用锁定道具：{item.name}");
        }

        HideMenu();
    }

    /// <summary>
    /// 使用跳过道具
    /// </summary>
    private void UseSkipPolicy(PolicyItem item)
    {
        if (item.usageCount == 0) return;

        if (item.usageCount > 0) item.usageCount--;
        if (item.usageCount == 0)
        {
            GameControl.Instance?.RemovePolicy(item.id);
        }

        HideMenu();
        EventDisplayUI.Instance?.ClearText();
        GameControl.Instance?.ProcessNextTurn();
    }

    /// <summary>
    /// 使用调控道具
    /// </summary>
    private void UseAdjustPolicy(PolicyItem item)
    {
        if (item.usageCount == 0) return;

        if (item.usageCount > 0) item.usageCount--;
        if (item.usageCount == 0)
        {
            GameControl.Instance?.RemovePolicy(item.id);
        }

        // 应用数值变化
        if (stats != null)
        {
            stats.king += item.kingChange;
            stats.noble += item.nobleChange;
            stats.scholar += item.scholarChange;
            stats.foreign += item.foreignChange;
            stats.people += item.peopleChange;

            StatsDisplayUI.Instance?.UpdateStatText();
            GameControl.Instance?.OnStatsChanged();
        }

        HideMenu();
    }

    /// <summary>
    /// 隐藏菜单
    /// </summary>
    public void HideMenu()
    {
        if (policyMenuPanel != null)
        {
            policyMenuPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 获取道具类型名称
    /// </summary>
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
}
