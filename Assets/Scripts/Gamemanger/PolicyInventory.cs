using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 道具背包管理器：负责道具的添加、删除、查询
/// </summary>
public class PolicyInventory : MonoBehaviour
{
    public static PolicyInventory Instance;

    [Header("引用")]
    public StatModel stats;

    void Awake()
    {
        Instance = this;
    }

    // ===== 添加道具 =====
    public bool AddPolicy(PolicyItem item)
    {
        if (stats == null || stats.policyBag == null) 
        {
            Debug.LogError("[PolicyInventory] stats 或 policyBag 为空");
            return false;
        }
        
        if (stats.policyBag.Count >= stats.maxPolicyCount) 
        {
            Debug.LogWarning("[PolicyInventory] 背包已满，无法添加新道具");
            return false;
        }
        
        // 所有道具都可以叠加，直接添加
        stats.policyBag.Add(item);
        Debug.Log($"[PolicyInventory] 添加道具: {item.name} (背包: {stats.policyBag.Count})");
        return true;
    }

    // ===== 移除道具 =====
    public bool RemovePolicy(string id)
    {
        if (stats == null || stats.policyBag == null) return false;
        
        PolicyItem item = stats.policyBag.FirstOrDefault(p => p.id == id);
        if (item == null) return false;
        
        stats.policyBag.Remove(item);
        Debug.Log($"[PolicyInventory] 移除道具: {item.name}");
        return true;
    }

    // ===== 清空道具 =====
    public void ClearPolicies()
    {
        if (stats != null && stats.policyBag != null)
        {
            stats.policyBag.Clear();
            Debug.Log("[PolicyInventory] 清空所有道具");
        }
    }

    // ===== 获取道具 =====
    public PolicyItem GetPolicy(string id)
    {
        if (stats == null || stats.policyBag == null) return null;
        return stats.policyBag.FirstOrDefault(p => p.id == id);
    }

    // ===== 查找可用的免死道具 =====
    public PolicyItem FindDeathImmunityItem(int deathType)
    {
        if (stats?.policyBag != null)
        {
            foreach (var item in stats.policyBag)
            {
                if (item.type == 2 && 
                    item.usageCount != 0 && 
                    item.targetLayers != null && 
                    item.targetLayers.Contains(deathType))
                {
                    return item;
                }
            }
        }
        return null;
    }

    // ===== 获取背包数量 =====
    public int GetPolicyCount()
    {
        return stats?.policyBag?.Count ?? 0;
    }

    // ===== 背包是否已满 =====
    public bool IsFull()
    {
        return GetPolicyCount() >= stats?.maxPolicyCount;
    }
}
