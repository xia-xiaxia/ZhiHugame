using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

/// <summary>
/// 延时事件数据（用于间隔事件持久化）
/// </summary>
[System.Serializable]
public class DelayedEventData
{
    public int triggerYear;  // 触发年份
    public string eventId;   // 事件ID
    
    public DelayedEventData(int year, string id)
    {
        triggerYear = year;
        eventId = id;
    }
}

[System.Serializable]
[CreateAssetMenu(menuName = "Game/StatModel1")]
public class StatModel : ScriptableObject
{
    // 初始值均为 阈值的一半
    public int year = 0;      // 当前年份
    
    // 私有字段
    [SerializeField]
    private int _currency = 0;  // 当前资金（累计）
    [SerializeField]
    private int _king = 30;
    [SerializeField]
    private int _noble = 30;
    [SerializeField]
    private int _scholar = 30;
    [SerializeField]
    private int _foreign = 30;
    [SerializeField]
    private int _people = 30;
    [SerializeField]
    private int _talentPoints = 0; // 当前天赋点数
    [SerializeField]
    private int _policyBagSize = 4; // 当前道具包大小
    [SerializeField]
    private int _payBackCurrency = 0; // 退还的货币数
    [SerializeField]
    private float _shopMult = 1.0f; // 商店折扣倍率
    [SerializeField]
    private float _currencyMult = 1.0f; // 货币获取倍率
    [SerializeField]
    private int _policyShopCount = 4; // 道具商店数量

    //统计变化量，在year_end统一执行动画及数值改动
    public int king_delta = 0;
    public int noble_delta = 0;
    public int scholar_delta = 0;
    public int foreign_delta = 0;
    public int people_delta = 0;

    public int buff_king_delta = 0;
    public int buff_noble_delta = 0;
    public int buff_scholar_delta = 0;
    public int buff_foreign_delta = 0;
    public int buff_people_delta = 0;
    
    // 天赋点属性
    public int talentPoints
    {
        get => _talentPoints;
        set
        {
            if (_talentPoints != value)
            {
                _talentPoints = value;
                OnStatsChanged?.Invoke();
            }
        }
    }
    // 道具商店数量属性
    public int policyShopCount
    {
        get => _policyShopCount;
        set
        {
            if (_policyShopCount != value)
            {
                _policyShopCount = value;
                OnStatsChanged?.Invoke();
            }
        }
    }
    // 道具包大小属性
    public int policyBagSize
    {
        get => _policyBagSize;
        set
        {
            if (_policyBagSize != value)
            {
                _policyBagSize = value;
                OnStatsChanged?.Invoke();
            }
        }
    }
    public int payBackCurrency
    {
        get => _payBackCurrency;
        set
        {
            if (_payBackCurrency != value)
            {
                _payBackCurrency = value;
                OnStatsChanged?.Invoke();
            }
        }
    }
    public float shopMult
    {
        get => _shopMult;
        set
        {
            if (_shopMult != value)
            {
                _shopMult = value;
                OnStatsChanged?.Invoke();
            }
        }
    }
    public float currencyMult
    {
        get => _currencyMult;
        set
        {
            if (_currencyMult != value)
            {
                _currencyMult = value;
                OnStatsChanged?.Invoke();
            }
        }
    }
    
    // 货币属性
    public int currency
    {
        get => _currency;
        set
        {
            if (_currency != value)
            {
                _currency = value;
                OnCurrencyChanged?.Invoke();
            }
        }
    }

    // 公开属性
    public int king
    {
        get => _king;
        set
        {
            if (_king != value)
            {
                _king = value;
                OnKingChanged?.Invoke(value);
                OnStatsChanged?.Invoke();
            }
        }
    }
    
    public int noble
    {
        get => _noble;
        set
        {
            if (_noble != value)
            {
                _noble = value;
                OnNobleChanged?.Invoke(value);
                OnStatsChanged?.Invoke();
            }
        }
    }
    
    public int scholar
    {
        get => _scholar;
        set
        {
            if (_scholar != value)
            {
                _scholar = value;
                OnScholarChanged?.Invoke(value);
                OnStatsChanged?.Invoke();
            }
        }
    }
    
    public int foreign
    {
        get => _foreign;
        set
        {
            if (_foreign != value)
            {
                _foreign = value;
                OnForeignChanged?.Invoke(value);
                OnStatsChanged?.Invoke();
            }
        }
    }
    
    public int people
    {
        get => _people;
        set
        {
            if (_people != value)
            {
                _people = value;
                OnPeopleChanged?.Invoke(value);
                OnStatsChanged?.Invoke();
            }
        }
    }

    // 失败阈值常量

    public int kingMin = 0;
    public int kingMax = 60;
    public int nobleMin = 0;
    public int nobleMax = 60;
    public int scholarMin = 0;
    public int scholarMax = 60;
    public int foreignMin = 0;
    public int foreignMax = 60;
    public int peopleMin = 0;
    public int peopleMax = 60;

    // 计算各属性百分比
    public float KingPercent => kingMax > kingMin ? (float)(king - kingMin) / (float)(kingMax - kingMin) : 0f;
    public float NoblePercent => nobleMax > nobleMin ? (float)(noble - nobleMin) / (float)(nobleMax - nobleMin) : 0f;
    public float ScholarPercent => scholarMax > scholarMin ? (float)(scholar - scholarMin) / (float)(scholarMax - scholarMin) : 0f;
    public float ForeignPercent => foreignMax > foreignMin ? (float)(foreign - foreignMin) / (float)(foreignMax - foreignMin) : 0f;
    public float PeoplePercent => peopleMax > peopleMin ? (float)(people - peopleMin) / (float)(peopleMax - peopleMin) : 0f;


    // 判断是否越界（触发失败）
    public List<PolicyItem> policyBag = new List<PolicyItem>();
    
    // BUFF背包（持久化BUFF列表）
    public List<BuffDefinition> buffBag = new List<BuffDefinition>();
    
    // 间隔事件队列（持久化延时事件）
    public List<DelayedEventData> delayedEventQueue = new List<DelayedEventData>();
    
    // 已激活的天赋列表（存储天赋ID）
    public List<string> activatedTalents = new List<string>();
    // 刷新花费数组
    public int[] refreshPolicyShopCost = new int[4] { 5, 10, 20, 50 };
    
    // 新手教程标记（是否已看过教程）
    public bool hasSeenTutorial = false;
    
    // 锁定系统
    [System.Serializable]
    public class LayerLock
    {
        public int layer;           // 阶层：1国君 2卿士 3宗族 4外臣 5庶人
        public bool lockIncrease;   // true=禁止上升, false=禁止下降
        public int remainingYears;  // 剩余回合数
        
        public LayerLock(int layer, bool lockIncrease, int duration)
        {
            this.layer = layer;
            this.lockIncrease = lockIncrease;
            this.remainingYears = duration;
        }
    }
    
    public List<LayerLock> activeLayerLocks = new List<LayerLock>();
    
    public bool IsOutOfBounds()
    {
        return king < kingMin || king > kingMax
            || noble < nobleMin || noble > nobleMax
            || scholar < scholarMin || scholar > scholarMax
            || foreign < foreignMin || foreign > foreignMax
            || people < peopleMin || people > peopleMax;
    }

    // 可调用的重置方法（重开时恢复初始值）
    public void ResetToDefault()
    {
        // 重置年份
        year = 0;

        // 不再重置上下限，保留天赋带来的阈值修改
        // 根据当前上下限计算中间值
        int kingMid = (kingMin + kingMax) / 2;
        int nobleMid = (nobleMin + nobleMax) / 2;
        int scholarMid = (scholarMin + scholarMax) / 2;
        int foreignMid = (foreignMin + foreignMax) / 2;
        int peopleMid = (peopleMin + peopleMax) / 2;

        // 通过属性触发事件（仅重置当前数值到中位）
        king = kingMid;
        noble = nobleMid;
        scholar = scholarMid;
        foreign = foreignMid;
        people = peopleMid;

        // 清空需要在重开时重置的运行期数据
        policyBag.Clear();
        buffBag.Clear();
        delayedEventQueue.Clear();
        activeLayerLocks.Clear();

        // 保留已激活天赋与天赋点数，确保跨局持久
        // activatedTalents 和 talentPoints 不再在此处清空或归零

        Debug.Log($"[StatModel] 重置完成（保留天赋） - 国君:{king} 宗族:{noble} 卿士:{scholar} 外臣:{foreign} 庶人:{people}");
    }
    
    /// <summary>
    /// 完全重置（包括天赋、背包等所有数据）
    /// 用于新游戏或删除存档时
    /// </summary>
    public void ResetToDefaultCompletely()
    {
        // 重置年份和货币
        year = 0;
        currency = 0;

        // 重置五维属性到初始值
        kingMin = 0;
        kingMax = 60;
        nobleMin = 0;
        nobleMax = 60;
        scholarMin = 0;
        scholarMax = 60;
        foreignMin = 0;
        foreignMax = 60;
        peopleMin = 0;
        peopleMax = 60;

        // 重置到中间值
        king = 30;
        noble = 30;
        scholar = 30;
        foreign = 30;
        people = 30;
        
        // 重置增量
        king_delta = 0;
        noble_delta = 0;
        scholar_delta = 0;
        foreign_delta = 0;
        people_delta = 0;
        buff_king_delta = 0;
        buff_noble_delta = 0;
        buff_scholar_delta = 0;
        buff_foreign_delta = 0;
        buff_people_delta = 0;

        // 清空所有列表
        policyBag.Clear();
        buffBag.Clear();
        delayedEventQueue.Clear();
        activeLayerLocks.Clear();
        
        // 重置天赋系统
        activatedTalents.Clear();
        talentPoints = 0;
        
        // 重置天赋效果数据
        policyBagSize = 4;
        payBackCurrency = 0;
        shopMult = 1.0f;
        currencyMult = 1.0f;
        policyShopCount = 4;
        refreshPolicyShopCost = new int[] { 5, 10, 20, 50 };
        
        // 重置教程标记
        hasSeenTutorial = false;

        Debug.Log("[StatModel] 完全重置完成 - 所有数据已恢复到初始状态");
    }

    public bool isBagFull(int count = 1)
    {
        if (this.policyBag.Count + count > this.policyBagSize)
        {
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 添加阶层锁定
    /// </summary>
    public void AddLayerLock(int layer, bool lockIncrease, int duration)
    {
        // 检查是否已有相同的锁定
        LayerLock existingLock = activeLayerLocks.Find(l => l.layer == layer && l.lockIncrease == lockIncrease);
        if (existingLock != null)
        {
            // 叠加时长
            existingLock.remainingYears += duration;
            Debug.Log($"[StatModel] 叠加锁定: 阶层{layer} {(lockIncrease ? "禁止上升" : "禁止下降")} 新时长:{existingLock.remainingYears}年");
        }
        else
        {
            // 新增锁定
            activeLayerLocks.Add(new LayerLock(layer, lockIncrease, duration));
            Debug.Log($"[StatModel] 添加锁定: 阶层{layer} {(lockIncrease ? "禁止上升" : "禁止下降")} 时长:{duration}年");
            
            // 触发锁定添加事件 (layer, lockIncrease, isAdded=true)
            OnLayerLockChanged?.Invoke(layer, lockIncrease, true);
        }
    }
    
    /// <summary>
    /// 减少所有锁定的剩余回合（每回合调用）
    /// </summary>
    public void DecrementLayerLocks()
    {
        for (int i = activeLayerLocks.Count - 1; i >= 0; i--)
        {
            activeLayerLocks[i].remainingYears--;
            if (activeLayerLocks[i].remainingYears <= 0)
            {
                int layer = activeLayerLocks[i].layer;
                bool lockIncrease = activeLayerLocks[i].lockIncrease;
                Debug.Log($"[StatModel] 锁定解除: 阶层{layer} {(lockIncrease ? "禁止上升" : "禁止下降")}");
                activeLayerLocks.RemoveAt(i);
                
                // 触发锁定移除事件 (layer, lockIncrease, isAdded=false)
                OnLayerLockChanged?.Invoke(layer, lockIncrease, false);
            }
        }
    }

    /// <summary>
    /// 根据layer获得锁定状态
    /// 0 =》 无锁定
    /// 1 =》 半锁定
    /// 2 =》 半锁定
    /// 3 =》 全锁定
    /// </summary>
    public int GetLayerLockStat(int layer)
    {
        int inc = 0;
        int dec = 0;

        for (int i = activeLayerLocks.Count - 1; i >= 0; i--)
        {

            if (activeLayerLocks[i].layer == layer)
            {
                if (activeLayerLocks[i].lockIncrease) inc = 1;
                else dec = 2;
            }
        }

        return inc + dec;
    }

    /// <summary>
    /// 检查某阶层某方向是否被锁定
    /// </summary>
    public bool IsLayerLocked(int layer, bool isIncrease)
    {
        return activeLayerLocks.Exists(l => l.layer == layer && l.lockIncrease == isIncrease);
    }
    
    /// <summary>
    /// 统计变化量
    /// type=0代表Option 1代表buff 2代表道具
    /// </summary>
    public void ApplyStatChange(int kingDelta, int nobleDelta, int scholarDelta, int foreignDelta, int peopleDelta, int type=0)
    {
        if(type == 0 ||  type == 1)
        {
            king_delta += kingDelta;
            noble_delta += nobleDelta;
            scholar_delta += scholarDelta;
            foreign_delta += foreignDelta;
            people_delta += peopleDelta;
        }

        if(type == 1)
        {
            buff_king_delta += kingDelta;
            buff_noble_delta += nobleDelta;
            buff_scholar_delta += scholarDelta;
            buff_foreign_delta += foreignDelta;
            buff_people_delta += peopleDelta;
        }

        if(type == 2)
        {
            ApplyAllStat(kingDelta, nobleDelta, scholarDelta, foreignDelta, peopleDelta);
        }
    }

    private void ApplyAllStat(int _king_delta, int _noble_delta, int _scholar_delta, int _foreign_delta, int _people_delta)
    {
        ApplyStatChangeWithLock(1, "国君", _king_delta, ref _king, () => OnKingChanged?.Invoke(_king), () => BuffOnKingChanged(buff_king_delta));
        ApplyStatChangeWithLock(3, "宗族", _noble_delta, ref _noble, () => OnNobleChanged?.Invoke(_noble), () => BuffOnNobleChanged(buff_noble_delta));
        ApplyStatChangeWithLock(2, "卿士", _scholar_delta, ref _scholar, () => OnScholarChanged?.Invoke(_scholar), () => BuffOnScholarChanged(buff_scholar_delta));
        ApplyStatChangeWithLock(4, "外臣", _foreign_delta, ref _foreign, () => OnForeignChanged?.Invoke(_foreign), () => BuffOnForeignChanged(buff_foreign_delta));
        ApplyStatChangeWithLock(5, "庶人", _people_delta, ref _people, () => OnPeopleChanged?.Invoke(_people), () => BuffOnPeopleChanged(buff_people_delta));
        OnStatsChanged?.Invoke();
    }
    
    public void PlayBuffAnime()
    {
        
    }
    
    public void OnYearEnd()
    {
        ApplyAllStat(king_delta, noble_delta, scholar_delta, foreign_delta, people_delta);

        king_delta = 0;
        noble_delta = 0;
        scholar_delta = 0;
        foreign_delta = 0;
        people_delta = 0;

        buff_king_delta = 0;
        buff_noble_delta = 0;
        buff_scholar_delta = 0;
        buff_foreign_delta = 0;
        buff_people_delta = 0;

    }

    /// <summary>
    /// 对单个属性应用带锁定检查的数值变化
    /// </summary>
    private void ApplyStatChangeWithLock(int layer, string layerName, int delta, ref int statValue, System.Action onChanged, System.Action BuffonChanged)
    {
        if (delta == 0) return;

        bool isIncrease = delta > 0;
        if (IsLayerLocked(layer, isIncrease))
        {
            Debug.Log($"[StatModel] {layerName}{(isIncrease ? "上升" : "下降")}被锁定，变化无效");
            return;
        }

        statValue += delta;
        onChanged?.Invoke();
        //去除第一版的Buffd动画实现
        //BuffonChanged?.Invoke();
    }

    // 事件，当属性变化时触发
    public event System.Action OnStatsChanged;
    public event System.Action OnCurrencyChanged;
    public event System.Action<int> OnKingChanged;
    public event System.Action<int> OnNobleChanged;
    public event System.Action<int> OnScholarChanged;
    public event System.Action<int> OnForeignChanged;
    public event System.Action<int> OnPeopleChanged;

    //For buff
    public event System.Action<int> BuffOnKingChanged;
    public event System.Action<int> BuffOnNobleChanged;
    public event System.Action<int> BuffOnScholarChanged;
    public event System.Action<int> BuffOnForeignChanged;
    public event System.Action<int> BuffOnPeopleChanged;

    // 锁定状态改变事件 (layer, lockIncrease, isAdded)
    public event System.Action<int, bool, bool> OnLayerLockChanged;

}