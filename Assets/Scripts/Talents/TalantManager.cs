using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TalantManager : MonoBehaviour
{
    public static TalantManager Instance;
    
    [Header("数据引用")]
    public StatModel stats;  // 关联的StatModel
    
    // 当前可用的天赋点数（从StatModel读取）
    public int currentTalentPoints 
    { 
        get => stats != null ? stats.talentPoints : 0;
        set 
        { 
            if (stats != null) 
                stats.talentPoints = value; 
        }
    }
    
    // 已激活的天赋列表（从StatModel读取）
    public HashSet<string> activatedTalents 
    { 
        get => stats != null ? new HashSet<string>(stats.activatedTalents) : new HashSet<string>();
        set 
        { 
            if (stats != null) 
                stats.activatedTalents = value.ToList(); 
        }
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        TalentLoader.Instance.Load();
    }

    void Start()
    {
        // 检查StatModel是否绑定
        if (stats == null)
        {
            Debug.LogError("[TalantManager] StatModel 未绑定！");
        }
    }

    void Update()
    {
        
    }
    
    /// <summary>
    /// 增加天赋点
    /// </summary>
    public void AddTalentPoints(int amount)
    {
        if (stats == null) return;
        
        stats.talentPoints += amount;
        Debug.Log($"获得 {amount} 点天赋点，当前天赋点：{stats.talentPoints}");
    }
    
    /// <summary>
    /// 检查是否可以激活某个天赋
    /// </summary>
    public bool CanActivateTalent(Talent talent)
    {
        if (stats == null) return false;
        
        // 如果已经激活过，不能再次激活
        if (stats.activatedTalents.Contains(talent.id))
        {
            return false;
        }
        
        // 检查天赋点是否足够
        if (stats.talentPoints < talent.cost)
        {
            return false;
        }
        
        // 检查所有前置天赋是否都已激活
        if (talent.preTalentObjects != null && talent.preTalentObjects.Count > 0)
        {
            foreach (var preTalent in talent.preTalentObjects)
            {
                if (!stats.activatedTalents.Contains(preTalent.id))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 激活天赋
    /// </summary>
    public bool ActivateTalent(Talent talent)
    {
        if (stats == null || !CanActivateTalent(talent))
        {
            return false;
        }
        
        // 消耗天赋点
        stats.talentPoints -= talent.cost;
        
        // 记录已激活的天赋
        if (!stats.activatedTalents.Contains(talent.id))
        {
            stats.activatedTalents.Add(talent.id);
        }
        
        // 应用天赋效果
        ApplyTalentEffect(talent);
        
        Debug.Log($"成功激活天赋：{talent.name}，剩余天赋点：{stats.talentPoints}");
        return true;
    }
    
    /// <summary>
    /// 应用天赋效果
    /// </summary>
    private void ApplyTalentEffect(Talent talent)
    {
        if (stats == null || talent == null || talent.talentEffect == null) return;
        
        TalentEffect effect = talent.talentEffect;
        
        // 应用人口限制变化
        if (effect.kingLimit != 0)
            stats.kingMax = effect.kingLimit;
        
        if (effect.nobleLimit != 0)
            stats.nobleMax = effect.nobleLimit;
        
        if (effect.scholarLimit != 0)
            stats.scholarMax = effect.scholarLimit;
        
        if (effect.foreignLimit != 0)
            stats.foreignMax = effect.foreignLimit;
        
        if (effect.peopleLimit != 0)
            stats.peopleMax = effect.peopleLimit;
        
        // 应用商店相关效果
        if (effect.policyShopCount != 0)
        {
            stats.policyShopCount = effect.policyShopCount;
            // 这里可以通知商店系统更新商品数量
            Debug.Log($"商店商品数量设置为：{effect.policyShopCount}");
        }
        
        if (effect.shopMult != 0)
            stats.shopMult = effect.shopMult;
        
        // 应用货币相关效果
        if (effect.currencyMult != 0)
            stats.currencyMult = effect.currencyMult;
        
        if (effect.payBackCurrency != 0)
            stats.payBackCurrency = effect.payBackCurrency;
        
        // 应用背包容量变化
        if (effect.policyBagSizeChange != 0)
            stats.policyBagSize = effect.policyBagSizeChange;
        
        Debug.Log($"应用天赋效果：{talent.name}");
        // 应用道具商店刷新花费
        if (effect.refreshPolicyShopCost != null && effect.refreshPolicyShopCost.Length > 0)
        {
            stats.refreshPolicyShopCost = effect.refreshPolicyShopCost;
        }
    }
    
    /// <summary>
    /// 检查某个天赋是否已激活
    /// </summary>
    public bool IsTalentActivated(string talentId)
    {
        if (stats == null) return false;
        return stats.activatedTalents.Contains(talentId);
    }
}
