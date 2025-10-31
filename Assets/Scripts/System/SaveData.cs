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
    // 游戏进度
    public int year;
    public int currency;
    
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
    
    // 政策背包
    public List<PolicyItemData> policyBag = new List<PolicyItemData>();
    
    // BUFF背包
    public List<BuffData> buffBag = new List<BuffData>();
    
    // 延时事件队列
    public List<DelayedEventData> delayedEventQueue = new List<DelayedEventData>();
    
    // 已使用的事件ID（防止重复）
    public List<UsedEventData> usedEvents = new List<UsedEventData>();
    
    // 激活的事件集索引
    public List<int> activeRandomEventSetIndices = new List<int>();
    
    // 新手教程标记
    public bool hasSeenTutorial = false;
    
    // 存档时间戳
    public string saveTime;
    
    // 游戏版本
    public string gameVersion = "1.0.0";
    
    /// <summary>
    /// 从 StatModel 创建存档数据
    /// </summary>
    public static SaveData FromStatModel(StatModel stats)
    {
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
            hasSeenTutorial = stats.hasSeenTutorial,
            saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        
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
                        whichChange = policy.whichChange,
                        thresholdDeltaup = policy.thresholdDeltaup,
                        thresholdDeltadown = policy.thresholdDeltadown,
                        deathImmunity = policy.deathImmunity != null ? new List<int>(policy.deathImmunity) : new List<int>(),
                        kingChange = policy.kingChange,
                        nobleChange = policy.nobleChange,
                        scholarChange = policy.scholarChange,
                        foreignChange = policy.foreignChange,
                        peopleChange = policy.peopleChange,
                        usageCount = policy.usageCount,
                        cost = policy.cost,
                        deathdec = policy.deathdec
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
        
        return data;
    }
    
    /// <summary>
    /// 应用存档数据到 StatModel
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
        stats.hasSeenTutorial = hasSeenTutorial;
        
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
                    whichChange = policyData.whichChange,
                    thresholdDeltaup = policyData.thresholdDeltaup,
                    thresholdDeltadown = policyData.thresholdDeltadown,
                    deathImmunity = policyData.deathImmunity != null ? new List<int>(policyData.deathImmunity) : new List<int>(),
                    kingChange = policyData.kingChange,
                    nobleChange = policyData.nobleChange,
                    scholarChange = policyData.scholarChange,
                    foreignChange = policyData.foreignChange,
                    peopleChange = policyData.peopleChange,
                    usageCount = policyData.usageCount,
                    cost = policyData.cost,
                    deathdec = policyData.deathdec
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
        
        Debug.Log($"[SaveData] 存档已加载: 年份={year}, 君主={king}, 贵族={noble}");
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
    public string whichChange;
    public int thresholdDeltaup;
    public int thresholdDeltadown;
    public List<int> deathImmunity = new List<int>();
    public int kingChange;
    public int nobleChange;
    public int scholarChange;
    public int foreignChange;
    public int peopleChange;
    public int usageCount;
    public int cost;
    public string deathdec;
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
/// 已使用事件数据（用于序列化）
/// </summary>
[System.Serializable]
public class UsedEventData
{
    public int setIndex;
    public string eventId;
    
    public UsedEventData(int set, string id)
    {
        setIndex = set;
        eventId = id;
    }
}
