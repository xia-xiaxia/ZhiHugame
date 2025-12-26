using UnityEngine;
using System.Collections.Generic;

public class PolicyManager : MonoBehaviour
{
    public static PolicyManager Instance;
    
    public TextAsset policyJson; // 道具配置 JSON
    
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
    public void GenerateShopItems(int count = 8)
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
