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
        Debug.Log("[PolicyMenuUI] Awake - Instance 已设置");
    }

    void Start()
    {
        Debug.Log($"[PolicyMenuUI] Start - policyMenuPanel: {policyMenuPanel != null}, policyItemsParent: {policyItemsParent != null}, policyItemButtonPrefab: {policyItemButtonPrefab != null}, stats: {stats != null}");
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
            case 1: // 锁定道具
                UseThresholdPolicy(item);
                break;
            case 3: // 跳过道具
                UseSkipPolicy(item);
                break;
            case 4: // 调控道具
                UseAdjustPolicy(item);
                break;
            case 5: // 时局道具
                UseSituationPolicy(item);
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
            foreach (int layer in item.targetLayers)
            {
                int absLayer = System.Math.Abs(layer);
                bool lockIncrease = layer > 0; // 正数禁止上升，负数禁止下降
                
                stats.AddLayerLock(absLayer, lockIncrease, item.lockDuration);
                
                string layerName = GetLayerName(absLayer);
                string lockType = lockIncrease ? "上升" : "下降";
                Debug.Log($"[PolicyMenuUI] 锁定道具效果：{layerName} 禁止{lockType}，持续{item.lockDuration}年");
            }
        }
        
        // 应用数值变化（如果有）
        if (stats != null && (item.kingChange != 0 || item.nobleChange != 0 || item.scholarChange != 0 || item.foreignChange != 0 || item.peopleChange != 0))
        {
            stats.ApplyStatChange(item.kingChange, item.nobleChange, item.scholarChange, item.foreignChange, item.peopleChange);
            
            Debug.Log($"[PolicyMenuUI] 锁定道具数值变化：国君{item.kingChange:+#;-#;0} 宗族{item.nobleChange:+#;-#;0} 卿士{item.scholarChange:+#;-#;0} 外臣{item.foreignChange:+#;-#;0} 庶人{item.peopleChange:+#;-#;0}");
            
            StatsDisplayUI.Instance?.UpdateStatText();
            GameControl.Instance?.OnStatsChanged();
        }
        
        Debug.Log($"[PolicyMenuUI] 使用锁定道具：{item.name}");

        HideMenu();
    }
    
    /// <summary>
    /// 获取阶层名称
    /// </summary>
    private string GetLayerName(int layer)
    {
        switch (layer)
        {
            case 1: return "国君";
            case 2: return "卿士";
            case 3: return "宗族";
            case 4: return "外臣";
            case 5: return "庶人";
            default: return $"阶层{layer}";
        }
    }

    /// <summary>
    /// 使用跳过道具
    /// </summary>
    private void UseSkipPolicy(PolicyItem item)
    {
        GameControl.Instance.year++;
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

        // 应用数值变化（使用带锁定检查的方法）
        if (stats != null)
        {
            stats.ApplyStatChange(item.kingChange, item.nobleChange, item.scholarChange, item.foreignChange, item.peopleChange);

            StatsDisplayUI.Instance?.UpdateStatText();
            GameControl.Instance?.OnStatsChanged();
        }

        HideMenu();
    }

    /// <summary>    /// 使用时局道具
    /// </summary>
    private void UseSituationPolicy(PolicyItem item)
    {
        if (item.usageCount == 0) return;

        // 消耗次数
        if (item.usageCount > 0) item.usageCount--;
        if (item.usageCount == 0)
        {
            GameControl.Instance?.RemovePolicy(item.id);
        }

        // 触发BUFF
        if (!string.IsNullOrEmpty(item.triggeredBuffId))
        {
            Debug.Log($"[PolicyMenuUI] 使用时局道具: {item.name}，触发BUFF: {item.triggeredBuffId}");
            BuffDefinition buff = BuffManager.Instance?.AddBuffById(item.triggeredBuffId);
            
            if (buff != null)
            {
                Debug.Log($"[PolicyMenuUI] BUFF添加成功: {buff.name}");
                // 显示BUFF添加提示
                if (BuffUI.Instance != null)
                {
                    BuffUI.Instance.ShowBuffPanel();
                }
            }
            else
            {
                Debug.LogError($"[PolicyMenuUI] BUFF添加失败: {item.triggeredBuffId}");
            }
        }
        else
        {
            Debug.LogWarning($"[PolicyMenuUI] 时局道具 {item.name} 没有配置 triggeredBuffId");
        }

        HideMenu();
    }

    /// <summary>    /// 隐藏菜单
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
            case 1: return "锁定";
            case 2: return "免死";
            case 3: return "跳过";
            case 4: return "调控";
            case 5: return "时局";
            default: return "未知";
        }
    }
}
