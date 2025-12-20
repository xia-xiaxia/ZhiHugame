using System.Collections;
using System.Collections.Generic;
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
    public int maxPolicyCount = 8; // 最大道具数量
    
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

    //统计变化量，在year_end统一执行动画及数值改动
    private int king_delta = 0;
    private int noble_delta = 0;
    private int scholar_delta = 0;
    private int foreign_delta = 0;
    private int people_delta = 0;

    private int buff_king_delta = 0;
    private int buff_noble_delta = 0;
    private int buff_scholar_delta = 0;
    private int buff_foreign_delta = 0;
    private int buff_people_delta = 0;
    
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
        year = 0;
        
        kingMin = 0; kingMax = 60;
        nobleMin = 0; nobleMax = 60;
        scholarMin = 0; scholarMax = 60;
        foreignMin = 0; foreignMax = 60;
        peopleMin = 0; peopleMax = 60;
        
        // 根据上下限计算中间值
        int kingMid = (kingMin + kingMax) / 2;
        int nobleMid = (nobleMin + nobleMax) / 2;
        int scholarMid = (scholarMin + scholarMax) / 2;
        int foreignMid = (foreignMin + foreignMax) / 2;
        int peopleMid = (peopleMin + peopleMax) / 2;
        
        // 通过属性触发事件
        king = kingMid;
        noble = nobleMid;
        scholar = scholarMid;
        foreign = foreignMid;
        people = peopleMid;
        
        // 清空背包
        policyBag.Clear();
        buffBag.Clear();
        delayedEventQueue.Clear();
        activeLayerLocks.Clear();
        
        Debug.Log($"[StatModel] 重置完成 - 国君:{king} 宗族:{noble} 卿士:{scholar} 外臣:{foreign} 庶人:{people}");
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
                Debug.Log($"[StatModel] 锁定解除: 阶层{activeLayerLocks[i].layer} {(activeLayerLocks[i].lockIncrease ? "禁止上升" : "禁止下降")}");
                activeLayerLocks.RemoveAt(i);
            }
        }
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
        ApplyStatChangeWithLock(2, "卿士", _noble_delta, ref _scholar, () => OnScholarChanged?.Invoke(_scholar), () => BuffOnNobleChanged(buff_noble_delta));
        ApplyStatChangeWithLock(3, "宗族", _scholar_delta, ref _noble, () => OnNobleChanged?.Invoke(_noble), () => BuffOnScholarChanged(buff_scholar_delta));
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
        BuffonChanged?.Invoke();
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

}