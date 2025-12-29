using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 存档数据结构
/// 包含游戏的所有可持久化数据
/// </summary>
[System.Serializable]
public class SaveData
{
    // 版本控制
    public const int CURRENT_VERSION = 2; // 当前存档版本
    public int version = CURRENT_VERSION; // 存档版本号
    
    // 游戏进度
    public int year;
    public int currency;
    public int maxPolicyCount;
    
    // 五维属性
    public int king;
    public int noble;
    public int scholar;
    public int foreign;
    public int people;
    
    // 阈值设置
    public int kingMin;
    public int kingMax;
    public int nobleMin;
    public int nobleMax;
    public int scholarMin;
    public int scholarMax;
    public int foreignMin;
    public int foreignMax;
    public int peopleMin;
    public int peopleMax;

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

    // 政策背包
    public List<PolicyItemData> policyBag = new List<PolicyItemData>();
    
    // BUFF背包
    public List<BuffData> buffBag = new List<BuffData>();
    
    // 延时事件队列
    public List<DelayedEventData> delayedEventQueue = new List<DelayedEventData>();
    
    // 强制后继事件ID（EventSelector的nextEventId）
    public string nextEventId = "0";
    
    // 暂停状态（正在显示的事件）
    public string pausedEventId = "";
    public int pausedSentenceIndex = 0;
    
    // 已使用的事件ID（防止重复）
    public List<UsedEventData> usedEvents = new List<UsedEventData>();
    
    // 激活的事件集索引
    public List<int> activeRandomEventSetIndices = new List<int>();
    
    // 锁定系统
    public List<LayerLockData> activeLayerLocks = new List<LayerLockData>();
    
    // 新手教程标记
    public bool hasSeenTutorial = false;
    
    // 天赋系统
    public int currentTalentPoints = 0;
    public List<string> activatedTalents = new List<string>();
    
    // 天赋效果相关
    public int policyBagSize = 5;
    public int payBackCurrency = 0;
    public float shopMult = 1.0f;
    public float currencyMult = 1.0f;
    public int policyShopCount = 5;
    public int[] refreshPolicyShopCost = new int[4];
    
    // 存档时间戳
    public string saveTime;
    
    // 游戏版本
    public string gameVersion = "1.0.0";


    //GameStatistics内容
    public int currentReignYears;
    public int totalReginYears;
    public int policyUseOutCount;

    public bool[] judgeValue;
    public int[] judgeFirstYear;

    public List<int> activeMissions;
    public List<IntIntEntry> policyUsageCountList;
    public List<IntIntEntry> policyFirstYearList;
    public List<StringBoolEntry> isCompleteList;
    public List<StringPairEntry> runsWithLongReign;


    /// <summary>
    /// 从 StatModel 创建存档数据
    /// </summary>
    public static SaveData FromStatModel(StatModel stats)
    {
        GameStatistics gameStatistics = GameControl.Instance.gameStatistics;
        SaveData data = new SaveData
        {
            year = stats.year,
            currency = stats.currency,
            king = stats.king,
            noble = stats.noble,
            scholar = stats.scholar,
            foreign = stats.foreign,
            people = stats.people,
            kingMin = stats.kingMin,
            kingMax = stats.kingMax,
            nobleMin = stats.nobleMin,
            nobleMax = stats.nobleMax,
            scholarMin = stats.scholarMin,
            scholarMax = stats.scholarMax,
            foreignMin = stats.foreignMin,
            foreignMax = stats.foreignMax,
            peopleMin = stats.peopleMin,
            peopleMax = stats.peopleMax,
            king_delta = stats.king_delta,
            noble_delta = stats.noble_delta,
            scholar_delta = stats.scholar_delta,
            foreign_delta = stats.foreign_delta,
            people_delta = stats.people_delta,
            buff_king_delta = stats.buff_king_delta,
            buff_noble_delta = stats.buff_noble_delta,
            buff_scholar_delta = stats.buff_scholar_delta,
            buff_foreign_delta = stats.buff_foreign_delta,
            buff_people_delta = stats.buff_people_delta,
            hasSeenTutorial = stats.hasSeenTutorial,
            currentTalentPoints = stats.talentPoints,
            activatedTalents = stats.activatedTalents != null ? new List<string>(stats.activatedTalents) : new List<string>(),
            policyBagSize = stats.policyBagSize,
            payBackCurrency = stats.payBackCurrency,
            shopMult = stats.shopMult,
            currencyMult = stats.currencyMult,
            policyShopCount = stats.policyShopCount,
            saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        // 转换字典: policyUsageCount
        data.policyUsageCountList = new List<IntIntEntry>();
        foreach (var kvp in gameStatistics.policyUsageCount)
        {
            data.policyUsageCountList.Add(new IntIntEntry { key = kvp.Key, value = kvp.Value });
        }

        // 转换字典: policyFirstYear
        data.policyFirstYearList = new List<IntIntEntry>();
        foreach (var kvp in gameStatistics.policyFirstYear)
        {
            data.policyFirstYearList.Add(new IntIntEntry { key = kvp.Key, value = kvp.Value });
        }

        // 转换字典: isComplete
        data.isCompleteList = new List<StringBoolEntry>();
        foreach (var kvp in gameStatistics.isComplete)
        {
            data.isCompleteList.Add(new StringBoolEntry { key = kvp.Key, value = kvp.Value });
        }


        //转换字典：runsWithLongReign
        data.runsWithLongReign = new List<StringPairEntry>();
        foreach (var kvp in gameStatistics.runsWithLongReign)
        {
            data.runsWithLongReign.Add(new StringPairEntry
            {
                key = kvp.Key,
                val1 = kvp.Value.x, 
                val2 = kvp.Value.y
            });
        }

        // 复制刷新商店花费数组
        if (stats.refreshPolicyShopCost != null)
        {
            data.refreshPolicyShopCost = new int[stats.refreshPolicyShopCost.Length];
            System.Array.Copy(stats.refreshPolicyShopCost, data.refreshPolicyShopCost, stats.refreshPolicyShopCost.Length);
        }

        // 复制政策背包
        if (stats.policyBag != null)
        {
            foreach (var policy in stats.policyBag)
            {
                if (policy != null)
                {
                    data.policyBag.Add(new PolicyItemData
                    {
                        id = policy.id,
                        type = policy.type,
                        name = policy.name,
                        desc = policy.desc,
                        result = policy.result,
                        targetLayers = policy.targetLayers != null ? new List<int>(policy.targetLayers) : new List<int>(),
                        deathEffectText = policy.deathEffectText,
                        kingChange = policy.kingChange,
                        nobleChange = policy.nobleChange,
                        scholarChange = policy.scholarChange,
                        foreignChange = policy.foreignChange,
                        peopleChange = policy.peopleChange,
                        triggeredBuffId = policy.triggeredBuffId,
                        lockDuration = policy.lockDuration,
                        usageCount = policy.usageCount,
                        cost = policy.cost
                    });
                }
            }
        }
        
        // 复制BUFF背包
        if (stats.buffBag != null)
        {
            foreach (var buff in stats.buffBag)
            {
                if (buff != null)
                {
                    data.buffBag.Add(new BuffData
                    {
                        id = buff.id,
                        name = buff.name,
                        description = buff.description,
                        result = buff.result,
                        duration = buff.duration,
                        kingChange = buff.kingChange,
                        nobleChange = buff.nobleChange,
                        scholarChange = buff.scholarChange,
                        foreignChange = buff.foreignChange,
                        peopleChange = buff.peopleChange
                    });
                }
            }
        }
        
        // 复制延时事件队列
        if (stats.delayedEventQueue != null)
        {
            data.delayedEventQueue = new List<DelayedEventData>(stats.delayedEventQueue);
        }
        
        // 复制锁定系统
        if (stats.activeLayerLocks != null)
        {
            foreach (var layerLock in stats.activeLayerLocks)
            {
                if (layerLock != null)
                {
                    data.activeLayerLocks.Add(new LayerLockData
                    {
                        layer = layerLock.layer,
                        lockIncrease = layerLock.lockIncrease,
                        remainingYears = layerLock.remainingYears
                    });
                }
            }
        }
        
        // 保存事件使用状态（从 EventDatabase 获取）
        if (EventDatabase.Instance != null)
        {
            List<string> usedIds = EventDatabase.Instance.GetUsedEventIds();
            data.usedEvents.Clear();
            foreach (var id in usedIds)
            {
                data.usedEvents.Add(new UsedEventData(id));
            }
            
            // 保存激活的事件集索引
            data.activeRandomEventSetIndices = EventDatabase.Instance.GetActiveEventSetIndices();
        }
        
        // 保存延时事件（从 EventSelector 获取）
        if (EventSelector.Instance != null)
        {
            data.delayedEventQueue = EventSelector.Instance.GetSaveData();
            data.nextEventId = EventSelector.Instance.GetNextEventId();
        }
        
        // 保存暂停状态（正在显示的事件）
        if (EventDisplayUI.Instance != null)
        {
            data.pausedEventId = EventDisplayUI.Instance.GetCurrentEventId() ?? "";
            data.pausedSentenceIndex = EventDisplayUI.Instance.GetCurrentSentenceIndex();
        }
        
        return data;
    }
    
    /// <summary>
    /// 应用存档数据到 StatModel
    /// 新加GameStatistic内容
    /// </summary>
    public void ApplyToStatModel(StatModel stats)
    {
        if (stats == null)
        {
            Debug.LogError("[SaveData] StatModel 为 null");
            return;
        }

        stats.year = year;
        stats.currency = currency;
        stats.king = king;
        stats.noble = noble;
        stats.scholar = scholar;
        stats.foreign = foreign;
        stats.people = people;
        stats.kingMin = kingMin;
        stats.kingMax = kingMax;
        stats.nobleMin = nobleMin;
        stats.nobleMax = nobleMax;
        stats.scholarMin = scholarMin;
        stats.scholarMax = scholarMax;
        stats.foreignMin = foreignMin;
        stats.foreignMax = foreignMax;
        stats.peopleMin = peopleMin;
        stats.peopleMax = peopleMax;

        stats.king_delta = king_delta;
        stats.noble_delta = noble_delta;
        stats.scholar_delta = scholar_delta;
        stats.foreign_delta = foreign_delta;
        stats.people_delta = people_delta;
        stats.buff_king_delta = buff_king_delta;
        stats.buff_noble_delta = buff_noble_delta;
        stats.buff_scholar_delta = buff_scholar_delta;
        stats.buff_foreign_delta = buff_foreign_delta;
        stats.buff_people_delta = buff_people_delta;

        stats.hasSeenTutorial = hasSeenTutorial;
        
        // 恢复天赋系统数据
        stats.talentPoints = currentTalentPoints;
        stats.activatedTalents = activatedTalents != null ? new List<string>(activatedTalents) : new List<string>();
        
        // 恢复天赋效果相关数据
        stats.policyBagSize = policyBagSize;
        stats.payBackCurrency = payBackCurrency;
        stats.shopMult = shopMult;
        stats.currencyMult = currencyMult;
        stats.policyShopCount = policyShopCount;
        
        // 恢复刷新商店花费数组
        if (refreshPolicyShopCost != null && refreshPolicyShopCost.Length > 0)
        {
            stats.refreshPolicyShopCost = new int[refreshPolicyShopCost.Length];
            System.Array.Copy(refreshPolicyShopCost, stats.refreshPolicyShopCost, refreshPolicyShopCost.Length);
        }
        
        // 恢复政策背包
        stats.policyBag.Clear();
        if (policyBag != null)
        {
            foreach (var policyData in policyBag)
            {
                var policy = new PolicyItem
                {
                    id = policyData.id,
                    type = policyData.type,
                    name = policyData.name,
                    desc = policyData.desc,
                    result = policyData.result,
                    targetLayers = policyData.targetLayers != null ? new List<int>(policyData.targetLayers) : new List<int>(),
                    deathEffectText = policyData.deathEffectText,
                    kingChange = policyData.kingChange,
                    nobleChange = policyData.nobleChange,
                    scholarChange = policyData.scholarChange,
                    foreignChange = policyData.foreignChange,
                    peopleChange = policyData.peopleChange,
                    triggeredBuffId = policyData.triggeredBuffId,
                    lockDuration = policyData.lockDuration,
                    usageCount = policyData.usageCount,
                    cost = policyData.cost
                };
                stats.policyBag.Add(policy);
            }
        }
        
        // 恢复BUFF背包
        stats.buffBag.Clear();
        if (buffBag != null)
        {
            foreach (var buffData in buffBag)
            {
                var buff = new BuffDefinition
                {
                    id = buffData.id,
                    name = buffData.name,
                    description = buffData.description,
                    result = buffData.result,
                    duration = buffData.duration,
                    kingChange = buffData.kingChange,
                    nobleChange = buffData.nobleChange,
                    scholarChange = buffData.scholarChange,
                    foreignChange = buffData.foreignChange,
                    peopleChange = buffData.peopleChange
                };
                stats.buffBag.Add(buff);
            }
        }
        
        // 恢复延时事件队列
        stats.delayedEventQueue.Clear();
        if (delayedEventQueue != null)
        {
            stats.delayedEventQueue = new List<DelayedEventData>(delayedEventQueue);
        }
        
        // 恢复锁定系统
        stats.activeLayerLocks.Clear();
        if (activeLayerLocks != null)
        {
            foreach (var lockData in activeLayerLocks)
            {
                stats.activeLayerLocks.Add(new StatModel.LayerLock(lockData.layer, lockData.lockIncrease, lockData.remainingYears));
            }
        }
        
        Debug.Log($"[SaveData] 存档已加载: 年份={year}, 君主={king}, 贵族={noble}");
    }


    /// <summary>
    /// 应用存档数据到 GameStatistics
    /// </summary>
    public void ApplyToGameStatisics(GameStatistics stats)
    {

        stats.currentReignYears = this.currentReignYears;
        stats.totalReginYears = this.totalReginYears;
        stats.policyUseOutCount = this.policyUseOutCount;
        Debug.Log("Length: " + stats.judgeValue.Length);
        if (this.judgeValue != null && this.judgeValue.Length != 0) stats.judgeValue = (bool[])this.judgeValue.Clone();
        else stats.judgeValue = new bool[100];
        if (this.judgeFirstYear != null && this.judgeFirstYear.Length != 0) stats.judgeFirstYear = (int[])this.judgeFirstYear.Clone();
        else stats.judgeFirstYear = new int[100];

        if(this.activeMissions != null) stats.activeMissions = new List<int>(this.activeMissions);

        // 还原字典: policyUsageCount
        stats.policyUsageCount.Clear();
        foreach (var entry in this.policyUsageCountList)
        {
            stats.policyUsageCount[entry.key] = entry.value;
        }

        // 还原字典: policyFirstYear
        stats.policyFirstYear.Clear();
        foreach (var entry in this.policyFirstYearList)
        {
            stats.policyFirstYear[entry.key] = entry.value;
        }

        // 还原字典: isComplete
        stats.isComplete.Clear();
        foreach (var entry in this.isCompleteList)
        {
            stats.isComplete[entry.key] = entry.value;
        }

        // 还原字典: runsWithLongReign
        stats.runsWithLongReign.Clear();
        foreach (var entry in this.runsWithLongReign)
        {
            stats.runsWithLongReign[entry.key] = (entry.val1, entry.val2);
        }
    }
}

/// <summary>
/// 政策数据（用于序列化）
/// </summary>
[System.Serializable]
public class PolicyItemData
{
    public string id;
    public int type;
    public string name;
    public string desc;
    public string result;
    public List<int> targetLayers = new List<int>();
    public string deathEffectText;
    public int kingChange;
    public int nobleChange;
    public int scholarChange;
    public int foreignChange;
    public int peopleChange;
    public string triggeredBuffId;
    public int lockDuration;
    public int usageCount;
    public int cost;
}

/// <summary>
/// BUFF数据（用于序列化）
/// </summary>
[System.Serializable]
public class BuffData
{
    public string id;
    public string name;
    public string description;
    public string result;
    public int duration;
    public int kingChange;
    public int nobleChange;
    public int scholarChange;
    public int foreignChange;
    public int peopleChange;
}

/// <summary>
/// 锁定数据（用于序列化）
/// </summary>
[System.Serializable]
public class LayerLockData
{
    public int layer;
    public bool lockIncrease;
    public int remainingYears;
}

/// <summary>
/// 已使用事件数据（用于序列化）
/// </summary>
[System.Serializable]
public class UsedEventData
{
    public string eventId;
    
    public UsedEventData(string id)
    {
        eventId = id;
    }
    
    // 无参构造函数（Unity JsonUtility 需要）
    public UsedEventData()
    {
        eventId = "";
    }
}

[System.Serializable]
public class IntIntEntry
{
    public int key;
    public int value;
}

[System.Serializable]
public class IntBoolEntry
{
    public int key;
    public bool value;
}

[System.Serializable]
public class StringBoolEntry
{
    public string key;
    public bool value;
}

[System.Serializable] 
public class StringPairEntry
{
    public string key;
    public int val1;
    public int val2;
}