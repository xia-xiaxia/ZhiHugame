using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class PolicyManager : MonoBehaviour
{
    public static PolicyManager Instance;
    
    public TextAsset policyJson; // 道具配置 JSON
    
    [Header("道具类型图片配置")]
    public Sprite lockedSprite;        // 锁定道具图片 (type=1)
    public Sprite deathImmunitySprite; // 免死道具图片 (type=2)
    public Sprite skipSprite;          // 跳过道具图片 (type=3)
    public Sprite adjustSprite;        // 调控道具图片 (type=4)
    public Sprite situationSprite;     // 时局道具图片 (type=5)
    
    [Header("道具类型高亮特效配置")]
    public Sprite lockedHighlight;        // 锁定道具高亮 (type=1)
    public Sprite deathImmunityHighlight; // 免死道具高亮 (type=2)
    public Sprite skipHighlight;          // 跳过道具高亮 (type=3)
    public Sprite adjustHighlight;        // 调控道具高亮 (type=4)
    public Sprite situationHighlight;     // 时局道具高亮 (type=5)
    
    [Header("空道具槽位配置")]
    public Sprite emptySlotSprite;        // 空槽位图片
    public Sprite emptySlotHighlight;     // 空槽位高亮特效
    
    private Dictionary<string, PolicyItem> allPolicies = new Dictionary<string, PolicyItem>();
    private List<PolicyItem> currentShopItems = new List<PolicyItem>();


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            LoadPolicies();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void LoadPolicies()
    {
        if (policyJson == null)
        {
            Debug.LogWarning("[PolicyManager] 未配置道具 JSON 文件");
            return;
        }

        PolicyItem[] policies = JsonHelper.FromJson<PolicyItem>(policyJson.text);
        
        foreach (var policy in policies)
        {
            if (!string.IsNullOrEmpty(policy?.id))
            {
                allPolicies[policy.id] = policy;
            }
        }

        Debug.Log($"[PolicyManager] 加载了 {allPolicies.Count} 个道具配置");
    }

    // 根据 ID 获取道具（返回副本，避免修改原始数据）
    public PolicyItem GetPolicy(string id)
    {
        if (allPolicies.TryGetValue(id, out var policy))
        {
            return CopyPolicy(policy);
        }
        Debug.LogWarning($"[PolicyManager] 未找到道具 ID: {id}");
        return null;
    }

    //TaskSlot使用
    public string GetPolicyName(string id) 
    {
        if (allPolicies.TryGetValue(id, out var policy))
        {
            return policy.name;
        }
        Debug.LogWarning($"[PolicyManager] 未找到道具 ID: {id}");
        return null;
    }
    
    /// <summary>
    /// 根据道具类型获取对应的图片
    /// </summary>
    public Sprite GetPolicySpriteByType(int type)
    {
        switch (type)
        {
            case 1: return lockedSprite;        // 锁定道具
            case 2: return deathImmunitySprite; // 免死道具
            case 3: return skipSprite;          // 跳过道具
            case 4: return adjustSprite;        // 调控道具
            case 5: return situationSprite;     // 时局道具
            default:
                Debug.LogWarning($"[PolicyManager] 未知的道具类型: {type}");
                return null;
        }
    }
    
    /// <summary>
    /// 根据道具获取对应的图片
    /// </summary>
    public Sprite GetPolicySpriteByPolicy(PolicyItem policy)
    {
        if (policy == null) return null;
        return GetPolicySpriteByType(policy.type);
    }
    
    /// <summary>
    /// 根据道具类型获取对应的高亮特效
    /// </summary>
    public Sprite GetPolicyHighlightByType(int type)
    {
        switch (type)
        {
            case 1: return lockedHighlight;        // 锁定道具
            case 2: return deathImmunityHighlight; // 免死道具
            case 3: return skipHighlight;          // 跳过道具
            case 4: return adjustHighlight;        // 调控道具
            case 5: return situationHighlight;     // 时局道具
            default:
                Debug.LogWarning($"[PolicyManager] 未知的道具类型: {type}");
                return null;
        }
    }
    
    /// <summary>
    /// 根据道具获取对应的高亮特效
    /// </summary>
    public Sprite GetPolicyHighlightByPolicy(PolicyItem policy)
    {
        if (policy == null) return null;
        return GetPolicyHighlightByType(policy.type);
    }

    // 创建道具副本
    private PolicyItem CopyPolicy(PolicyItem original)
    {
        return new PolicyItem
        {
            // 基础信息
            id = original.id,
            name = original.name,
            result = original.result,
            desc = original.desc,
            type = original.type,
            usageCount = original.usageCount,
            cost = original.cost,
            
            // 互动阶层
            targetLayers = original.targetLayers != null ? new List<int>(original.targetLayers) : new List<int>(),
            
            // 免死道具
            deathEffectText = original.deathEffectText,
            
            // 五大数值变化
            kingChange = original.kingChange,
            nobleChange = original.nobleChange,
            scholarChange = original.scholarChange,
            foreignChange = original.foreignChange,
            peopleChange = original.peopleChange,
            
            // 时局道具
            triggeredBuffId = original.triggeredBuffId,
            
            // 锁定道具
            lockDuration = original.lockDuration
        };
    }

    // 获取所有道具列表（用于商店显示）
    public List<PolicyItem> GetAllPolicies()
    {
        List<PolicyItem> result = new List<PolicyItem>();
        foreach (var policy in allPolicies.Values)
        {
            result.Add(CopyPolicy(policy));
        }
        return result;
    }

    // 按类型获取道具列表
    public List<PolicyItem> GetPoliciesByType(int type)
    {
        List<PolicyItem> result = new List<PolicyItem>();
        foreach (var policy in allPolicies.Values)
        {
            if (policy.type == type)
            {
                result.Add(CopyPolicy(policy));
            }
        }
        return result;
    }

    // 随机抽取n个道具作为商店商品
    public void GenerateShopItems(int count = 4)
    {
        Debug.Log($"[PolicyManager] 开始生成商店道具，请求数量={count}，可用道具总数={allPolicies.Count}");
        
        var all = new List<PolicyItem>(allPolicies.Values);
        currentShopItems.Clear();
        
        if (all.Count <= count)
        {
            // 如果可用道具少于请求数量，全部添加
            foreach (var p in all) currentShopItems.Add(CopyPolicy(p));
            Debug.Log($"[PolicyManager] 可用道具不足，添加了全部 {currentShopItems.Count} 个道具");
        }
        else
        {
            // 随机抽取
            System.Random rnd = new System.Random();
            var picked = new HashSet<int>();
            while (currentShopItems.Count < count)
            {
                int idx = rnd.Next(all.Count);
                if (!picked.Contains(idx))
                {
                    picked.Add(idx);
                    currentShopItems.Add(CopyPolicy(all[idx]));
                }
            }
            Debug.Log($"[PolicyManager] 随机生成了 {currentShopItems.Count} 个商店道具");
        }
        
        // 打印生成的道具列表
        for (int i = 0; i < currentShopItems.Count; i++)
        {
            Debug.Log($"[PolicyManager] 商店道具{i+1}: ID={currentShopItems[i].id}, 名称={currentShopItems[i].name}, 类型={currentShopItems[i].type}");
        }
    }

    // 获取本轮商店商品
    public List<PolicyItem> GetCurrentShopItems()
    {
        return new List<PolicyItem>(currentShopItems);
    }

}
