using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class TalantManager : MonoBehaviour
{
    public static TalantManager Instance;

    [Header("数据引用")]
    public StatModel stats;  // 关联的StatModel
    public TextMeshProUGUI talentPointsText; // 显示天赋点数的UI文本

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
        // 尝试自动绑定 StatModel（优先使用 SaveManager 的引用）
        if (stats == null && SaveManager.Instance != null)
        {
            stats = SaveManager.Instance.stats;
        }
    }

    void Start()
    {
        // 再次检查 StatModel 是否绑定，防止加载顺序导致为空
        if (stats == null && SaveManager.Instance != null)
        {
            stats = SaveManager.Instance.stats;
        }
        if (stats == null)
        {
            Debug.LogError("[TalantManager] StatModel 未绑定！请在场景中将 TalantManager 的 stats 指向 SaveManager 的 StatModel。");
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
        UpdateTalentPointsUI();
        Debug.Log($"获得 {amount} 点天赋点，当前天赋点：{stats.talentPoints}");

        // 获得天赋点后自动保存，确保进度持久化
        if (SaveManager.Instance != null)
        {
            bool ok = SaveManager.Instance.SaveGame();
            if (ok)
            {
                Debug.Log("[TalantManager] 获得天赋点后已自动保存");
            }
            else
            {
                Debug.LogWarning("[TalantManager] 获得天赋点后自动保存失败");
            }
        }
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
        UpdateTalentPointsUI();

        // 记录已激活的天赋
        if (!stats.activatedTalents.Contains(talent.id))
        {
            stats.activatedTalents.Add(talent.id);
        }

        // 应用天赋效果
        ApplyTalentEffect(talent);

        // 激活天赋后，自动保存游戏进度（保留天赋与其效果）
        saveTalentData();

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
            // 如果商店已打开，刷新显示
            if (PolicyShopUI.Instance != null && PolicyShopUI.Instance.policyShopPanel != null && 
                PolicyShopUI.Instance.policyShopPanel.activeSelf)
            {
                int refreshCount = PolicyShopUI.Instance.GetRefreshCount();
                PolicyShopUI.Instance.RefreshShopItems();
                PolicyShopUI.Instance.reRefreshCount(refreshCount); // 保持刷新次数不变
            }
            Debug.Log($"商店商品数量设置为：{effect.policyShopCount}");
            Debug.Log("stats.policyShopCount=" + stats.policyShopCount);
        }

        if (effect.shopMult != 0)
        {
            stats.shopMult = effect.shopMult;
            Debug.Log($"商店倍率设置为：{effect.shopMult}");
            Debug.Log("stats.shopMult=" + stats.shopMult);
        }

        // 应用货币相关效果
        if (effect.currencyMult != 0)
        {
            stats.currencyMult = effect.currencyMult;
            Debug.Log($"货币获取倍率设置为：{effect.currencyMult}");
            Debug.Log("stats.currencyMult=" + stats.currencyMult);
        }

        if (effect.payBackCurrency != 0)
        {
            stats.payBackCurrency = effect.payBackCurrency;
            Debug.Log($"货币返还设置为：{effect.payBackCurrency}");
            Debug.Log("stats.payBackCurrency=" + stats.payBackCurrency);
        }

        // 应用背包容量变化
        if (effect.policyBagSizeChange != 0)
        {
                stats.policyBagSize = effect.policyBagSizeChange;
                PolicyShopUI.Instance?.RefreshInventoryDisplay();
                Debug.Log($"政策背包容量设置为：{effect.policyBagSizeChange}");
                Debug.Log("stats.policyBagSize=" + stats.policyBagSize);
        }

        Debug.Log($"应用天赋效果：{talent.name}");
        // 应用道具商店刷新花费
        if (effect.refreshPolicyShopCost != null && effect.refreshPolicyShopCost.Length > 0)
        {
            stats.refreshPolicyShopCost = effect.refreshPolicyShopCost;
            Debug.Log($"道具商店刷新花费已更新");
            Debug.Log("stats.refreshPolicyShopCost=" + string.Join(",", stats.refreshPolicyShopCost));
        }

        // 应用天赋效果完成后，立即保存以持久化所有修改字段
        saveTalentData();
    }

    /// <summary>
    /// 检查某个天赋是否已激活
    /// </summary>
    public bool IsTalentActivated(string talentId)
    {
        if (stats == null) return false;
        return stats.activatedTalents.Contains(talentId);
    }

    void UpdateTalentPointsUI()
    {
        if (talentPointsText != null)
        {
            talentPointsText.text = currentTalentPoints.ToString();
        }
    }

    public void saveTalentData()
    {
        if (SaveManager.Instance != null)
        {
            bool ok = SaveManager.Instance.SaveGame();
            if (ok)
            {
                Debug.Log("[TalantManager] 天赋数据已保存");
            }
            else
            {
                Debug.LogWarning("[TalantManager] 天赋数据保存失败");
            }
        }
    }
}
