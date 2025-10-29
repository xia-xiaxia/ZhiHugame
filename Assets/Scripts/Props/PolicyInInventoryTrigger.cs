using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PolicyInInventoryTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("物品信息")]
    public PolicyItem policyItem;

    [Header("提示面板设置")]
    public GameObject tooltipPanel;
    public Text tooltipText;
    public Vector2 tooltipOffset = new Vector2(10, -10);

    [Header("丢弃确认面板设置")]
    public GameObject discardConfirmParent;
    public Button discardConfirmButton;
    public Button discardCancelButton;
    public Text discardInfoText;

    private Canvas canvas;
    private bool useGlobalTooltip = false;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        
        InitializeTooltip();

        if (discardConfirmParent != null)
        {
            discardConfirmParent.SetActive(false);
            
            if (discardConfirmButton != null)
            {
                discardConfirmButton.onClick.RemoveAllListeners();
                discardConfirmButton.onClick.AddListener(OnDiscardConfirm);
            }
            
            if (discardCancelButton != null)
            {
                discardCancelButton.onClick.RemoveAllListeners();
                discardCancelButton.onClick.AddListener(OnDiscardCancel);
            }
        }
    }

    private void InitializeTooltip()
    {
        if (tooltipPanel == null && UIManager.Instance != null)
        {
            tooltipPanel = UIManager.Instance.policyTooltipPanel;
            tooltipText = UIManager.Instance.policyTooltipText;
            useGlobalTooltip = true;
            
            if (tooltipPanel != null)
            {
                Debug.Log($"[PolicyInInventoryTrigger] 成功从 UIManager 获取全局 tooltip");
            }
            else
            {
                Debug.LogWarning($"[PolicyInInventoryTrigger] UIManager.policyTooltipPanel 为空，请在 Inspector 中为 UIManager 赋值");
            }
        }
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            UpdateTooltipPosition();
        }
    }
    
    void OnDestroy()
    {
        if (useGlobalTooltip && tooltipPanel != null && tooltipPanel.activeSelf)
        {
            tooltipPanel.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("[PolicyInInventoryTrigger] OnPointerEnter 触发");
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }
    
    private void ShowTooltip()
    {
        // 如果还没初始化，尝试重新初始化
        if (tooltipPanel == null)
        {
            InitializeTooltip();
        }
        
        if (tooltipPanel == null || policyItem == null) return;

        string tooltipContent = BuildTooltipText();
        
        if (tooltipText != null)
        {
            tooltipText.text = tooltipContent;
        }

        tooltipPanel.SetActive(true);
        UpdateTooltipPosition();
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void UpdateTooltipPosition()
    {
        if (tooltipPanel == null) return;

        // 获取 tooltip 面板的高度，动态计算垂直偏移
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        float dynamicOffsetY = 0;
        
        if (tooltipRect != null)
        {
            // 强制重建布局以获取正确的高度
            Canvas.ForceUpdateCanvases();
            // 正数表示向上，面板高度的一半加上额外间距，显示在鼠标上方
            dynamicOffsetY = tooltipRect.rect.height / 2f + 20f;
        }
        
        Vector2 dynamicOffset = new Vector2(tooltipOffset.x, dynamicOffsetY);

        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            tooltipPanel.transform.position = Input.mousePosition + new Vector3(dynamicOffset.x, dynamicOffset.y, 0);
        }
        else if (canvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out localPoint
            );
            tooltipPanel.transform.localPosition = localPoint + dynamicOffset;
        }
        else
        {
            tooltipPanel.transform.position = Input.mousePosition + new Vector3(dynamicOffset.x, dynamicOffset.y, 0);
        }
    }

    private string BuildTooltipText()
    {
        if (policyItem == null) return "无物品信息";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        sb.AppendLine($"<b>{policyItem.name}</b>");
        sb.AppendLine($"<color=#FFD700></color>");
        
        string typeText = GetPolicyTypeName(policyItem.type);
        sb.AppendLine($"<color=#87CEEB>类型：</color>{typeText}");
        
        string usageText = policyItem.usageCount == -1 ? "无限" : policyItem.usageCount.ToString();
        sb.AppendLine($"<color=#90EE90>次数：</color>{usageText}");
        
        sb.AppendLine($"<color=#FFD700></color>");
        
        if (!string.IsNullOrEmpty(policyItem.desc))
        {
            sb.AppendLine($"<color=#CCCCCC>{policyItem.desc}</color>");
        }
        
        if (!string.IsNullOrEmpty(policyItem.result))
        {
            sb.AppendLine($"\n<color=#98FB98>效果：{policyItem.result}</color>");
        }
        
        // switch (policyItem.type)
        // {
        //     case 1:
        //         if (!string.IsNullOrEmpty(policyItem.whichChange))
        //         {
        //             sb.AppendLine($"\n<color=#FFA500>影响：{GetStatName(policyItem.whichChange)}</color>");
        //             sb.AppendLine($"上限变化：{(policyItem.thresholdDeltaup >= 0 ? "+" : "")}{policyItem.thresholdDeltaup}");
        //             sb.AppendLine($"下限变化：{(policyItem.thresholdDeltadown >= 0 ? "+" : "")}{policyItem.thresholdDeltadown}");
        //         }
        //         break;
                
        //     case 2:
        //         if (policyItem.deathImmunity != null && policyItem.deathImmunity.Count > 0)
        //         {
        //             sb.AppendLine($"\n<color=#FF6B6B>免死类型：</color>");
        //             foreach (int deathType in policyItem.deathImmunity)
        //             {
        //                 sb.AppendLine($"   {GetDeathTypeName(deathType)}");
        //             }
        //         }
        //         break;
                
        //     case 3:
        //         sb.AppendLine($"\n<color=#87CEEB>可跳过当前事件</color>");
        //         break;
                
        //     case 4:
        //         sb.AppendLine($"\n<color=#FFB6C1>数值变化：</color>");
        //         if (policyItem.kingChange != 0) sb.AppendLine($"  国君：{(policyItem.kingChange >= 0 ? "+" : "")}{policyItem.kingChange}");
        //         if (policyItem.nobleChange != 0) sb.AppendLine($"  贵族：{(policyItem.nobleChange >= 0 ? "+" : "")}{policyItem.nobleChange}");
        //         if (policyItem.scholarChange != 0) sb.AppendLine($"  卿士：{(policyItem.scholarChange >= 0 ? "+" : "")}{policyItem.scholarChange}");
        //         if (policyItem.foreignChange != 0) sb.AppendLine($"  外臣：{(policyItem.foreignChange >= 0 ? "+" : "")}{policyItem.foreignChange}");
        //         if (policyItem.peopleChange != 0) sb.AppendLine($"  庶人：{(policyItem.peopleChange >= 0 ? "+" : "")}{policyItem.peopleChange}");
        //         break;
        // }
        
        sb.AppendLine($"\n<color=#FFA500>点击可丢弃此道具</color>");
        
        return sb.ToString();
    }

    private string GetPolicyTypeName(int type)
    {
        switch (type)
        {
            case 1: return "阈值调整";
            case 2: return "免除死亡";
            case 3: return "跳过事件";
            case 4: return "数值调控";
            default: return "未知类型";
        }
    }

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

    public void SetPolicyItem(PolicyItem item)
    {
        policyItem = item;
        UpdateButtonDisplay();
    }
    
    private void UpdateButtonDisplay()
    {
        if (policyItem == null) return;
        
        Transform infoTextTransform = transform.Find("InfoText");
        Text displayText = null;
        
        if (infoTextTransform != null)
        {
            displayText = infoTextTransform.GetComponent<Text>();
        }
        else
        {
            displayText = GetComponentInChildren<Text>();
        }
        
        if (displayText != null)
        {
            displayText.text = policyItem.name;
        }
    }

    public void ShowDiscardConfirm()
    {
        if (discardConfirmParent == null || policyItem == null) return;

        discardConfirmParent.SetActive(true);
    }

    private void OnDiscardConfirm()
    {
        if (policyItem == null || GameControl.Instance == null)
        {
            OnDiscardCancel();
            return;
        }

        GameControl.Instance.RemovePolicy(policyItem.id);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshShopInventoryDisplay();
        }

        gameObject.SetActive(false);
        OnDiscardCancel();
    }

    private void OnDiscardCancel()
    {
        if (discardConfirmParent != null)
        {
            discardConfirmParent.SetActive(false);
        }
    }
}
