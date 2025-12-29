using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PolicyInInventoryTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("物品信息")]
    public PolicyItem policyItem;
    
    [Header("道具图片设置")]
    public Image policyIconImage;      // 道具图标图片
    public Image highlightEffectImage; // 高亮特效图片
    public bool autoLoadSprite = true; // 是否自动加载图片

    [Header("提示面板设置")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;
    public Vector2 tooltipOffset = new Vector2(10, -10);

    [Header("丢弃确认面板设置")]
    public GameObject discardConfirmParent;
    public Button discardConfirmButton;
    public Button discardCancelButton;
    public Text discardInfoText;
    // 丢弃道具获得的货币数
    private int abandonPolicyAndGetCurrency = 0;

    private Canvas canvas;
    private bool useGlobalTooltip = false;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        
        // 自动加载道具图片
        if (autoLoadSprite)
        {
            LoadPolicySprite();
        }
        
        // 初始化高亮特效为隐藏状态
        if (highlightEffectImage != null)
        {
            highlightEffectImage.gameObject.SetActive(false);
        }
        
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
    
    /// <summary>
    /// 自动加载道具图片（根据类型）
    /// </summary>
    private void LoadPolicySprite()
    {
        if (policyItem == null || PolicyManager.Instance == null)
        {
            return;
        }
        
        // 加载道具图标
        if (policyIconImage != null)
        {
            Sprite sprite = PolicyManager.Instance.GetPolicySpriteByPolicy(policyItem);
            if (sprite != null)
            {
                policyIconImage.sprite = sprite;
                Debug.Log($"[PolicyInInventoryTrigger] 加载道具图片: {policyItem.name}, 类型={policyItem.type}");
            }
            else
            {
                Debug.LogWarning($"[PolicyInInventoryTrigger] 未能加载道具图片: {policyItem.name}, 类型={policyItem.type}");
            }
        }
        
        // 加载高亮特效
        if (highlightEffectImage != null)
        {
            Sprite highlight = PolicyManager.Instance.GetPolicyHighlightByPolicy(policyItem);
            if (highlight != null)
            {
                highlightEffectImage.sprite = highlight;
                Debug.Log($"[PolicyInInventoryTrigger] 加载高亮特效: {policyItem.name}, 类型={policyItem.type}");
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
        Debug.Log("[PolicyInInventoryTrigger] OnPointerEnter 触发");
        ShowTooltip();
        ShowHighlightEffect();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
        HideHighlightEffect();
    }
    
    private void ShowTooltip()
    {
        // 如果还没初始化，尝试重新初始化
        if (tooltipPanel == null)
        {
            InitializeTooltip();
        }
        
        if (tooltipPanel == null || policyItem == null) return;

        // 如果 tooltipText 为空，尝试从面板查找 TextMeshProUGUI
        if (tooltipText == null)
        {
            tooltipText = tooltipPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tooltipText == null && UIManager.Instance != null)
            {
                tooltipText = UIManager.Instance.policyTooltipText;
            }
            if (tooltipText == null)
            {
                Debug.LogError("[PolicyInInventoryTrigger] 无法找到 TextMeshProUGUI 组件");
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
    
    /// <summary>
    /// 显示高亮特效
    /// </summary>
    private void ShowHighlightEffect()
    {
        if (highlightEffectImage != null)
        {
            highlightEffectImage.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// 隐藏高亮特效
    /// </summary>
    private void HideHighlightEffect()
    {
        if (highlightEffectImage != null)
        {
            highlightEffectImage.gameObject.SetActive(false);
        }
    }
    private string BuildTooltipText()
    {
        if (policyItem == null) return "无物品信息";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        sb.AppendLine($"<size=120%>{policyItem.name}</size>");
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
        
        
        sb.AppendLine($"\n<color=#FFA500>点击可丢弃此道具</color>");
        
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
        abandonPolicyAndGetCurrency = GameControl.Instance?.stats?.payBackCurrency ?? 0;
        CurrencyManager.Instance?.AddCurrency(abandonPolicyAndGetCurrency);
        Debug.Log($"[PolicyInShopTrigger] 道具已丢弃，获得货币：{abandonPolicyAndGetCurrency}");
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
