using UnityEngine;

/// <summary>
/// 货币管理器：负责游戏货币的获取、消费
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("引用")]
    public StatModel stats;

    void Awake()
    {
        Instance = this;
    }

    // ===== 获取货币 =====
    public int GetCurrency()
    {
        return stats != null ? stats.currency : 0;
    }

    // ===== 消费货币 =====
    public void SpendCurrency(int amount)
    {
        if (stats == null) return;
        
        stats.currency -= amount;
        if (stats.currency < 0) stats.currency = 0;
        
        Debug.Log($"[CurrencyManager] 花费 {amount} 货币，剩余 {stats.currency} 货币");
        
        UIManager.Instance?.UpdateCurrencyDisplay();
    }

    // ===== 增加货币 =====
    public void AddCurrency(int amount)
    {
        if (stats == null) return;
        float gainCurrency = amount*stats.currencyMult;
        stats.currency += (int)gainCurrency;
        Debug.Log($"[CurrencyManager] 获得 {gainCurrency} 货币，总计 {stats.currency} 货币");
        
        UIManager.Instance?.UpdateCurrencyDisplay();
    }

    // ===== 检查是否有足够货币 =====
    public bool HasEnoughCurrency(int amount)
    {
        return GetCurrency() >= amount;
    }
}
