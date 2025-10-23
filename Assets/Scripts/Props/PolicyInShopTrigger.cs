using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PolicyInShopTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("物品信息")]
    public PolicyItem policyItem; // 关联的道具数据

    [Header("提示面板设置（可选，不设置则自动从UIManager获取）")]
    public GameObject tooltipPanel; // 提示面板
    public Text tooltipText; // 提示文本
    public Vector2 tooltipOffset = new Vector2(10, -10); // 提示框偏移

    private Canvas canvas; // 用于计算UI位置
    private bool useGlobalTooltip = false; // 是否使用全局提示框

    void Start()
    {
        // 获取Canvas（用于UI坐标转换）
        canvas = GetComponentInParent<Canvas>();
        
        if (canvas != null)
        {
            Debug.Log($"[PolicyInShopTrigger] Canvas找到，模式: {canvas.renderMode}");
        }
        else
        {
            Debug.LogWarning("[PolicyInShopTrigger] 未找到父级Canvas");
        }
        
        // 如果没有手动设置tooltip，尝试从UIManager获取
        if (tooltipPanel == null && UIManager.Instance != null)
        {
            tooltipPanel = UIManager.Instance.policyTooltipPanel;
            tooltipText = UIManager.Instance.policyTooltipText;
            useGlobalTooltip = true;
            
            if (tooltipPanel != null)
            {
                Debug.Log($"[PolicyInShopTrigger] 从UIManager获取到全局Tooltip面板: {tooltipPanel.name}");
            }
            else
            {
                Debug.LogWarning("[PolicyInShopTrigger] UIManager的policyTooltipPanel未设置");
            }
            
            if (tooltipText != null)
            {
                Debug.Log($"[PolicyInShopTrigger] 从UIManager获取到Tooltip文本组件");
            }
            else
            {
                Debug.LogWarning("[PolicyInShopTrigger] UIManager的policyTooltipText未设置");
            }
        }
        
        // 确保提示面板一开始是隐藏的
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            Debug.Log("[PolicyInShopTrigger] 初始化完成，Tooltip面板已隐藏");
        }
    }

    void Update()
    {
        // 如果提示面板显示中，更新其位置跟随鼠标
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            UpdateTooltipPosition();
        }
    }
    
    void OnDestroy()
    {
        // 销毁时确保隐藏tooltip（如果使用的是全局tooltip）
        if (useGlobalTooltip && tooltipPanel != null && tooltipPanel.activeSelf)
        {
            tooltipPanel.SetActive(false);
            Debug.Log("[PolicyInShopTrigger] 对象销毁，隐藏Tooltip面板");
        }
    }

    // 鼠标进入时触发
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    // 鼠标离开时触发
    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    // 显示提示信息
    private void ShowTooltip()
    {
        if (tooltipPanel == null)
        {
            Debug.LogWarning("[PolicyInShopTrigger] tooltipPanel为null，无法显示提示");
            return;
        }
        
        if (policyItem == null)
        {
            Debug.LogWarning("[PolicyInShopTrigger] policyItem为null，无法显示提示");
            return;
        }

        // 构建提示文本
        string tooltipContent = BuildTooltipText();
        
        if (tooltipText != null)
        {
            tooltipText.text = tooltipContent;
            Debug.Log($"[PolicyInShopTrigger] 设置提示文本: {policyItem.name}");
        }
        else
        {
            Debug.LogWarning("[PolicyInShopTrigger] tooltipText为null");
        }

        // 显示面板
        tooltipPanel.SetActive(true);
        Debug.Log($"[PolicyInShopTrigger] 显示提示面板: {tooltipPanel.name}");
        
        // 更新位置
        UpdateTooltipPosition();
    }

    // 隐藏提示信息
    private void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    // 更新提示框位置（跟随鼠标）
    private void UpdateTooltipPosition()
    {
        if (tooltipPanel == null) return;

        // 对于 Screen Space - Overlay，直接使用屏幕坐标
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // 直接使用鼠标位置加上偏移
            tooltipPanel.transform.position = Input.mousePosition + new Vector3(tooltipOffset.x, tooltipOffset.y, 0);
        }
        else if (canvas != null)
        {
            // 其他模式使用坐标转换
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out localPoint
            );
            tooltipPanel.transform.localPosition = localPoint + tooltipOffset;
        }
        else
        {
            // 没有canvas，直接使用世界坐标
            tooltipPanel.transform.position = Input.mousePosition + new Vector3(tooltipOffset.x, tooltipOffset.y, 0);
        }
    }

    // 构建提示文本内容
    private string BuildTooltipText()
    {
        if (policyItem == null) return "无物品信息";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        // 基本信息
        sb.AppendLine($"<b>{policyItem.name}</b>");
        sb.AppendLine($"<color=#FFD700>━━━━━━━━━━</color>");
        
        // 类型
        string typeText = GetPolicyTypeName(policyItem.type);
        sb.AppendLine($"<color=#87CEEB>类型：</color>{typeText}");
        
        // 价格
        if (policyItem.cost > 0)
        {
            sb.AppendLine($"<color=#FFD700>价格：</color>{policyItem.cost} 年");
        }
        
        // 使用次数
        string usageText = policyItem.usageCount == -1 ? "无限" : policyItem.usageCount.ToString();
        sb.AppendLine($"<color=#90EE90>次数：</color>{usageText}");
        
        sb.AppendLine($"<color=#FFD700>━━━━━━━━━━</color>");
        
        // 描述
        if (!string.IsNullOrEmpty(policyItem.desc))
        {
            sb.AppendLine($"<color=#CCCCCC>{policyItem.desc}</color>");
        }
        
        // 效果说明
        if (!string.IsNullOrEmpty(policyItem.result))
        {
            sb.AppendLine($"\n<color=#98FB98>效果：{policyItem.result}</color>");
        }
        
        // 根据类型显示详细效果
        switch (policyItem.type)
        {
            case 1: // 阈值道具
                if (!string.IsNullOrEmpty(policyItem.whichChange))
                {
                    sb.AppendLine($"\n<color=#FFA500>影响：{GetStatName(policyItem.whichChange)}</color>");
                    sb.AppendLine($"上限变化：{(policyItem.thresholdDeltaup >= 0 ? "+" : "")}{policyItem.thresholdDeltaup}");
                    sb.AppendLine($"下限变化：{(policyItem.thresholdDeltadown >= 0 ? "+" : "")}{policyItem.thresholdDeltadown}");
                }
                break;
                
            case 2: // 免死道具
                if (policyItem.deathImmunity != null && policyItem.deathImmunity.Count > 0)
                {
                    sb.AppendLine($"\n<color=#FF6B6B>免死类型：</color>");
                    foreach (int deathType in policyItem.deathImmunity)
                    {
                        sb.AppendLine($"  • {GetDeathTypeName(deathType)}");
                    }
                }
                break;
                
            case 3: // 跳过道具
                sb.AppendLine($"\n<color=#87CEEB>可跳过当前事件</color>");
                break;
                
            case 4: // 调控道具
                sb.AppendLine($"\n<color=#FFB6C1>数值变化：</color>");
                if (policyItem.kingChange != 0) sb.AppendLine($"  国君：{(policyItem.kingChange >= 0 ? "+" : "")}{policyItem.kingChange}");
                if (policyItem.nobleChange != 0) sb.AppendLine($"  贵族：{(policyItem.nobleChange >= 0 ? "+" : "")}{policyItem.nobleChange}");
                if (policyItem.scholarChange != 0) sb.AppendLine($"  卿士：{(policyItem.scholarChange >= 0 ? "+" : "")}{policyItem.scholarChange}");
                if (policyItem.foreignChange != 0) sb.AppendLine($"  外臣：{(policyItem.foreignChange >= 0 ? "+" : "")}{policyItem.foreignChange}");
                if (policyItem.peopleChange != 0) sb.AppendLine($"  庶人：{(policyItem.peopleChange >= 0 ? "+" : "")}{policyItem.peopleChange}");
                break;
        }
        
        return sb.ToString();
    }

    // 获取道具类型名称
    private string GetPolicyTypeName(int type)
    {
        switch (type)
        {
            case 1: return "阈值调整";
            case 2: return "免死金牌";
            case 3: return "跳过事件";
            case 4: return "数值调控";
            default: return "未知类型";
        }
    }

    // 获取属性名称
    private string GetStatName(string stat)
    {
        switch (stat)
        {
            case "king": return "国君";
            case "noble": return "贵族";
            case "scholar": return "卿士";
            case "foreign": return "外臣";
            case "people": return "庶人";
            default: return stat;
        }
    }

    // 获取死亡类型名称
    private string GetDeathTypeName(int deathType)
    {
        switch (deathType)
        {
            case 1: return "国君上限";
            case -1: return "国君下限";
            case 2: return "卿士上限";
            case -2: return "卿士下限";
            case 3: return "贵族上限";
            case -3: return "贵族下限";
            case 4: return "外臣上限";
            case -4: return "外臣下限";
            case 5: return "庶人上限";
            case -5: return "庶人下限";
            case 6: return "事件死亡";
            default: return $"类型{deathType}";
        }
    }

    // 公共方法：设置道具数据
    public void SetPolicyItem(PolicyItem item)
    {
        policyItem = item;
    }
}
