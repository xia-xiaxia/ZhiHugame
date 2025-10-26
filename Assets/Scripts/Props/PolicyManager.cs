using UnityEngine;
using System.Collections.Generic;
using JsonA;

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

    // 创建道具副本
    private PolicyItem CopyPolicy(PolicyItem original)
    {
        return new PolicyItem
        {
            id = original.id,
            type = original.type,
            name = original.name,
            desc = original.desc,
            cost = original.cost,
            result = original.result,
            whichChange = original.whichChange,
            thresholdDeltaup = original.thresholdDeltaup,
            thresholdDeltadown = original.thresholdDeltadown,
            deathImmunity = original.deathImmunity != null ? new List<int>(original.deathImmunity) : new List<int>(),
            kingChange = original.kingChange,
            nobleChange = original.nobleChange,
            scholarChange = original.scholarChange,
            foreignChange = original.foreignChange,
            peopleChange = original.peopleChange,
            usageCount = original.usageCount,
            deathdec = original.deathdec
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
    public void GenerateShopItems(int count = 5)
    {
        var all = new List<PolicyItem>(allPolicies.Values);
        currentShopItems.Clear();
        if (all.Count <= count)
        {
            foreach (var p in all) currentShopItems.Add(CopyPolicy(p));
        }
        else
        {
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
        }
    }

    // 获取本轮商店商品
    public List<PolicyItem> GetCurrentShopItems()
    {
        return new List<PolicyItem>(currentShopItems);
    }
}
