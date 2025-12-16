using System.Collections.Generic;

/// <summary>
/// 道具类型枚举
/// </summary>
public enum PolicyType
{
    Locked = 1,      // 锁定道具
    DeathImmunity = 2, // 免死道具
    Skip = 3,        // 跳过道具
    Adjust = 4,      // 调控道具
    Situation = 5    // 时局道具
}

[System.Serializable]
public class PolicyItem
{
    // ===== 基础信息 =====
    public string id;           // 道具ID（序号）
    public string name;         // 道具名称
    public string result;       // 道具词条（效果）
    public string desc;         // 道具描述（文案）
    public int type;            // 道具类型：1锁定 2免死 3跳过 4调控 5时局
    public int usageCount;      // 道具可使用次数（-1为无限，0则销毁此道具）
    public int cost;            // 售价

    // ===== 道具互动的阶层 =====
    // 对于免死：表示免死的阶级（+1为上限爆了，-1为下限问题）
    // 对于锁定：表示锁定的阶级（+1为禁止上升，-1为禁止下降）
    // 1国君 2卿士 3宗族 4外臣 5庶人 6事件类
    public List<int> targetLayers; // 道具互动的阶层（可以是多个）

    // ===== 免死道具相关 =====
    public string deathEffectText; // 免死道具生效文案（旁白）

    // ===== 五大数值变化 =====
    public int kingChange;      // 国君数值变化
    public int nobleChange;     // 卿士数值变化
    public int scholarChange;   // 宗族数值变化
    public int foreignChange;   // 外臣数值变化
    public int peopleChange;    // 庶人数值变化

    // ===== 时局道具相关 =====
    public string triggeredBuffId; // 触发的BUFF ID（如果是时局道具）

    // ===== 锁定道具相关 =====
    public int lockDuration;    // 锁定时长（如果是锁定道具，连续锁定多少年）

    /// <summary>
    /// 获取道具类型枚举
    /// </summary>
    public PolicyType GetPolicyType()
    {
        return (PolicyType)type;
    }

    /// <summary>
    /// 是否可以在事件中主动使用（跳过、调控、锁定、时局道具）
    /// </summary>
    public bool CanUseInEvent()
    {
        return type == 3 || type == 4 || type == 1 || type == 5;
    }

    /// <summary>
    /// 是否自动触发（免死道具）
    /// </summary>
    public bool IsAutoTrigger()
    {
        return type == 2;
    }

    /// <summary>
    /// 是否可叠加
    /// </summary>
    public bool IsStackable()
    {
        return true; // 所有道具都可叠加
    }

    /// <summary>
    /// 是否可主动丢弃
    /// </summary>
    public bool CanDiscard()
    {
        return type != 2; // 除了免死道具，其他都可主动丢弃
    }
}
