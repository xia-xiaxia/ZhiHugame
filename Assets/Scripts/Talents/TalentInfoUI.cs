using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 天赋信息UI显示脚本
/// 用于显示鼠标悬停在天赋按钮上时的详细信息
/// </summary>
public class TalentInfoUI : MonoBehaviour
{
    [Header("UI组件")]
    [Tooltip("信息文本框")]
    public TextMeshProUGUI infoText;
    
    [Header("显示设置")]
    [Tooltip("信息面板")]
    public GameObject infoPanel;
    
    [Tooltip("是否自动隐藏")]
    public bool autoHide = true;
    
    void Start()
    {
        // 初始时隐藏信息面板
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 显示天赋信息
    /// </summary>
    public void ShowTalentInfo(Talent talent)
    {
        if (talent == null) return;
        
        // 显示信息面板
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }
        
        // 显示天赋信息
        if (infoText != null)
        {
            infoText.text = BuildInfoText(talent);
        }
    }
    
    /// <summary>
    /// 构建天赋信息文本
    /// </summary>
    private string BuildInfoText(Talent talent)
    {
        string info = "";
        
        // 激活状态
        bool isActivated = TalantManager.Instance.IsTalentActivated(talent.id);
        if (isActivated)
        {
            info += "<color=yellow>[已激活]</color>\n";
        }
        else
        {
            bool canActivate = TalantManager.Instance.CanActivateTalent(talent);
            if (canActivate)
            {
                info += "<color=green>[可激活]</color>\n";
            }
            else
            {
                info += "<color=red>[不可激活]</color>\n";
            }
        }
        
        // 天赋名称
        info += $"<size=120%>{talent.name}</size>/\n\n";
        
        // 天赋描述
        info += $"{talent.description}\n\n";
        

        // // 前置天赋
        // if (talent.preTalentObjects != null && talent.preTalentObjects.Count > 0)
        // {
        //     info += "<前置天赋：\n";
        //     foreach (var preTalent in talent.preTalentObjects)
        //     {
        //         bool preActivated = TalantManager.Instance.IsTalentActivated(preTalent.id);
        //         string status = preActivated ? "<color=green>✓</color>" : "<color=red>✗</color>";
        //         info += $"  {status} {preTalent.name}\n";
        //     }
        //     info += "\n";
        // }
        // else
        // {
        //     info += "<无>前置天赋：无\n\n";
        // }

        // 天赋消耗
        int currentPoints = TalantManager.Instance.currentTalentPoints;
        bool hasEnoughPoints = currentPoints >= talent.cost;
        string costColor = hasEnoughPoints ? "<color=green>" : "<color=red>";
        info += $"消耗：{costColor}{talent.cost}</color> 天赋点\n";
        // info += $"当前天赋点：{currentPoints}\n\n";
        
        // // 天赋效果
        // info += GetTalentEffectDescription(talent.talentEffect);
        
        return info;
    }
    
    /// <summary>
    /// 隐藏天赋信息
    /// </summary>
    public void HideTalentInfo()
    {
        if (autoHide && infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 获取天赋效果的描述文本
    /// </summary>
    private string GetTalentEffectDescription(TalentEffect effect)
    {
        if (effect == null) return "无特殊效果";
        
        List<string> effectList = new List<string>();
        
        // 人口限制相关
        if (effect.kingLimit != 0)
            effectList.Add($"国王人口限制：{(effect.kingLimit > 0 ? "+" : "")}{effect.kingLimit}");
        
        if (effect.nobleLimit != 0)
            effectList.Add($"贵族人口限制：{(effect.nobleLimit > 0 ? "+" : "")}{effect.nobleLimit}");
        
        if (effect.scholarLimit != 0)
            effectList.Add($"学者人口限制：{(effect.scholarLimit > 0 ? "+" : "")}{effect.scholarLimit}");
        
        if (effect.foreignLimit != 0)
            effectList.Add($"外国人口限制：{(effect.foreignLimit > 0 ? "+" : "")}{effect.foreignLimit}");
        
        if (effect.peopleLimit != 0)
            effectList.Add($"平民人口限制：{(effect.peopleLimit > 0 ? "+" : "")}{effect.peopleLimit}");
        
        // 商店相关
        if (effect.policyShopCount != 0)
            effectList.Add($"政策商店数量：{(effect.policyShopCount > 0 ? "+" : "")}{effect.policyShopCount}");
        
        if (effect.shopMult != 0)
            effectList.Add($"商店倍率：×{(1 + effect.shopMult):F2}");
        
        // 货币相关
        if (effect.currencyMult != 0)
            effectList.Add($"货币获取倍率：×{(1 + effect.currencyMult):F2}");
        
        if (effect.payBackCurrency != 0)
            effectList.Add($"货币返还：{effect.payBackCurrency}");
        
        // 背包相关
        if (effect.policyBagSizeChange != 0)
            effectList.Add($"政策背包容量：{(effect.policyBagSizeChange > 0 ? "+" : "")}{effect.policyBagSizeChange}");
        
        if (effectList.Count == 0)
            return "无特殊效果";
        
        return "效果：\n" + string.Join("\n", effectList);
    }
}
