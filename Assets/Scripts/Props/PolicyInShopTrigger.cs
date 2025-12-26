using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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
    public TextMeshProUGUI tooltipText;
    public Vector2 tooltipOffset = new Vector2(10, -10);

    [Header("购买确认面板设置")]
    public GameObject purchaseConfirmParent;
    public Button purchaseConfirmButton;
    public Button purchaseCancelButton;

    private Canvas canvas;
    private bool useGlobalTooltip = false;
    private float currentShopMult = 1.0f;

    void Start()
    {
        currentShopMult = GameControl.Instance != null ? GameControl.Instance.stats.shopMult : 1f;

        if (policyItem != null && policyValue == 0 && policyItem.cost > 0)
        {
            float multiple = Random.Range(0.8f, 1.2f);
            policyValue = Mathf.FloorToInt(policyItem.cost * multiple * currentShopMult);
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

        // 如果 tooltipText 为空，尝试从面板查找 TextMeshProUGUI
        if (tooltipText == null)
        {
            tooltipText = tooltipPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tooltipText == null && UIManager.Instance != null)
            {
                tooltipText = UIManager.Instance.policyTooltipText as TextMeshProUGUI;
            }
            if (tooltipText == null)
            {
                Debug.LogError("[PolicyInShopTrigger] 无法找到 TextMeshProUGUI 组件");
                return;
            }
        }

        string tooltipContent = BuildTooltipText();
        tooltipText.text = tooltipContent;
        
        // 确保文本对象激活
        if (!tooltipText.gameObject.activeSelf)
        {
            tooltipText.gameObject.SetActive(true);
        }

        tooltipPanel.SetActive(true);
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
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
                policyValue = Mathf.FloorToInt(policyItem.cost * multiple * currentShopMult);
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
        
        
        return sb.ToString();
    }

    private string GetPolicyTypeName(int type)
    {
        switch (type)
        {
            case 1: return "锁定道具";
            case 2: return "免死道具";
            case 3: return "跳过道具";
            case 4: return "调控道具";
            case 5: return "时局道具";
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
            case 3: return "宗族上限";
            case -3: return "宗族下限";
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
        
         // UpdateButtonDisplay();
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
            displayText.text = $"{policyItem.name}";
        }
    }

    public void ShowPurchaseConfirm()
    {
        int activePurchaseConfirmCount = PolicyShopUI.Instance?.GetActivePurchaseConfirmCount() ?? 0;
        if (activePurchaseConfirmCount >= 1)
        {
            Debug.LogWarning("[PolicyInShopTrigger] 已有购买确认面板打开，无法再次打开");
            for(int i = 0; i < activePurchaseConfirmCount; i++)
            {
                GameObject purchaseConfirm = PolicyShopUI.Instance.activePurchaseConfirms.Dequeue();
                if (purchaseConfirm != null)
                {
                    purchaseConfirm.SetActive(false);
                }
            }
        }
        if (purchaseConfirmParent == null || policyItem == null) return;

        bool canPurchase = CanPurchase();
        if (purchaseConfirmButton != null)
        {
            purchaseConfirmButton.interactable = canPurchase;
        }

        purchaseConfirmParent.SetActive(true);
        PolicyShopUI.Instance?.activePurchaseConfirms.Enqueue(purchaseConfirmParent);
    }

    private bool CanPurchase()
    {
        if (GameControl.Instance == null) return false;
        
        int currency = GameControl.Instance.GetCurrency();
        
        if (currency < policyValue) return false;
        if (GameControl.Instance.stats.policyBag.Count >= GameControl.Instance.stats.maxPolicyCount) return false;
        if (GameControl.Instance.GetPolicy(policyItem.id) != null) return false;
        
        return true;
    }

    private void OnPurchaseConfirm()
    {
        if (policyItem == null || GameControl.Instance == null)
        {
            Debug.LogError("[PolicyInShopTrigger] policyItem 或 GameControl.Instance 为空");
            OnPurchaseCancel();
            return;
        }

        Debug.Log($"[PolicyInShopTrigger] 购买道具: {policyItem.name} (ID: {policyItem.id})");
        
        GameControl.Instance.SpendCurrency(policyValue);

        PolicyItem newItem = PolicyManager.Instance.GetPolicy(policyItem.id);
        if (newItem != null)
        {
            Debug.Log($"[PolicyInShopTrigger] 获取道具副本成功: {newItem.name}");
            bool success = GameControl.Instance.AddPolicy(newItem);
            Debug.Log($"[PolicyInShopTrigger] 添加到背包结果: {success}");
        }
        else
        {
            Debug.LogError($"[PolicyInShopTrigger] 无法获取道具副本: {policyItem.id}");
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
