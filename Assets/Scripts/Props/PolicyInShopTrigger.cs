using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PolicyInShopTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("物品信息")]
    public PolicyItem policyItem;
    public int policyValue;

    [Header("提示面板设置")]
    public GameObject tooltipPanel;
    public Text tooltipText;
    public Vector2 tooltipOffset = new Vector2(10, -10);

    [Header("购买确认面板设置")]
    public GameObject purchaseConfirmParent;
    public Button purchaseConfirmButton;
    public Button purchaseCancelButton;

    private Canvas canvas;
    private bool useGlobalTooltip = false;

    void Start()
    {
        if (policyItem != null && policyValue == 0 && policyItem.cost > 0)
        {
            float multiple = Random.Range(0.8f, 1.2f);
            policyValue = Mathf.FloorToInt(policyItem.cost * multiple);
        }
        
        canvas = GetComponentInParent<Canvas>();
        
        if (tooltipPanel == null && UIManager.Instance != null)
        {
            tooltipPanel = UIManager.Instance.policyTooltipPanel;
            tooltipText = UIManager.Instance.policyTooltipText;
            useGlobalTooltip = true;
        }
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        if (purchaseConfirmParent != null)
        {
            purchaseConfirmParent.SetActive(false);
            
            if (purchaseConfirmButton != null)
            {
                purchaseConfirmButton.onClick.RemoveAllListeners();
                purchaseConfirmButton.onClick.AddListener(OnPurchaseConfirm);
            }
            
            if (purchaseCancelButton != null)
            {
                purchaseCancelButton.onClick.RemoveAllListeners();
                purchaseCancelButton.onClick.AddListener(OnPurchaseCancel);
            }
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
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }
    
    private void ShowTooltip()
    {
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

        // 获取 tooltip 面板的尺寸，动态计算偏移
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        float dynamicOffsetX = 0;
        float dynamicOffsetY = 0;
        
        if (tooltipRect != null)
        {
            // 强制重建布局以获取正确的尺寸
            Canvas.ForceUpdateCanvases();
            
            // 垂直偏移：面板高度的一半，显示在鼠标下方
            dynamicOffsetY = -tooltipRect.rect.height / 2f;
            
            // 水平偏移：根据道具按钮在屏幕中的位置决定显示在左侧还是右侧
            RectTransform buttonRect = GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                // 获取按钮在屏幕上的位置
                Vector3 buttonScreenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, buttonRect.position);
                
                // 判断按钮在屏幕左半部分还是右半部分
                if (buttonScreenPos.x < Screen.width / 2f)
                {
                    // 左侧道具，tooltip 显示在鼠标右侧
                    dynamicOffsetX = tooltipRect.rect.width / 2f + 40f;
                }
                else
                {
                    // 右侧道具，tooltip 显示在鼠标左侧
                    dynamicOffsetX = -tooltipRect.rect.width / 2f - 40f;
                }
            }
        }
        
        Vector2 dynamicOffset = new Vector2(dynamicOffsetX, dynamicOffsetY);

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
        
        if (policyItem.cost > 0)
        {
            if (policyValue == 0)
            {
                float multiple = Random.Range(0.8f, 1.2f);
                policyValue = Mathf.FloorToInt(policyItem.cost * multiple);
            }
            sb.AppendLine($"<color=#FFD700>价格：</color>{policyValue} 年");
        }
        
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
        
        return sb.ToString();
    }

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
        
        if (policyItem != null && policyItem.cost > 0)
        {
            float multiple = Random.Range(0.8f, 1.2f);
            policyValue = Mathf.FloorToInt(policyItem.cost * multiple);
        }
        
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
            displayText.text = $"{policyItem.name}\n<color=#FFD700>{policyValue}年</color>";
        }
    }

    public void ShowPurchaseConfirm()
    {
        if (purchaseConfirmParent == null || policyItem == null) return;

        bool canPurchase = CanPurchase();
        if (purchaseConfirmButton != null)
        {
            purchaseConfirmButton.interactable = canPurchase;
        }

        purchaseConfirmParent.SetActive(true);
    }

    private bool CanPurchase()
    {
        if (GameControl.Instance == null) return false;
        
        int currency = GameControl.Instance.GetCurrency();
        
        if (currency < policyValue) return false;
        if (GameControl.Instance.stats.policyBag.Count >= 5) return false;
        if (GameControl.Instance.GetPolicy(policyItem.id) != null) return false;
        
        return true;
    }

    private void OnPurchaseConfirm()
    {
        if (policyItem == null || GameControl.Instance == null)
        {
            OnPurchaseCancel();
            return;
        }

        GameControl.Instance.SpendCurrency(policyValue);

        PolicyItem newItem = PolicyManager.Instance.GetPolicy(policyItem.id);
        if (newItem != null)
        {
            GameControl.Instance.AddPolicy(newItem);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCurrencyDisplay();
            UIManager.Instance.RefreshShopInventoryDisplay();
        }

        gameObject.SetActive(false);
        OnPurchaseCancel();
    }

    private void OnPurchaseCancel()
    {
        if (purchaseConfirmParent != null)
        {
            purchaseConfirmParent.SetActive(false);
        }
    }
}
