using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuffThresholdChange
{
    public string stat;
    public int minDelta;
    public int maxDelta;
}

[Serializable]
public class BuffLongTermEffect
{
    public string stat;
    public int delta;
}

[Serializable]
public class BuffDefinition
{
    public string id;
    public string description;
    public int duration = -1; // -1永久，0立即结束，>0为剩余事件数
    public List<BuffThresholdChange> thresholdChanges = new List<BuffThresholdChange>();
    public List<BuffLongTermEffect> longTermEffects = new List<BuffLongTermEffect>();
}

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance;
    public List<BuffDefinition> buffs = new List<BuffDefinition>();

    private List<BuffDefinition> activeBuffs = new List<BuffDefinition>();

    void Awake()
    {
        Instance = this;
    }

    // 添加buff
    public void AddBuff(BuffDefinition buff)
    {
        activeBuffs.Add(buff);
        ApplyThresholdChange(buff, true);
        Debug.Log($"[BuffManager] 添加Buff: {buff.description}");
    }

    // 移除buff
    public void RemoveBuff(BuffDefinition buff)
    {
        activeBuffs.Remove(buff);
        ApplyThresholdChange(buff, false);
        Debug.Log($"[BuffManager] 移除Buff: {buff.description}");
    }

    // 每次事件串结束时调用
    public void OnEventChainEnd()
    {
        foreach (var buff in new List<BuffDefinition>(activeBuffs))
        {
            foreach (var eff in buff.longTermEffects)
            {
                ApplyLongTermEffect(eff);
            }
            if (buff.duration > 0)
            {
                buff.duration--;
                if (buff.duration == 0)
                {
                    RemoveBuff(buff);
                }
            }
        }
    }

    // 应用阈值变化
    private void ApplyThresholdChange(BuffDefinition buff, bool add)
    {
        var stats = EventManager.Instance.stats;
        foreach (var change in buff.thresholdChanges)
        {
            switch (change.stat)
            {
                case "king":
                    stats.kingMin += add ? change.minDelta : -change.minDelta;
                    stats.kingMax += add ? change.maxDelta : -change.maxDelta;
                    break;
                case "noble":
                    stats.nobleMin += add ? change.minDelta : -change.minDelta;
                    stats.nobleMax += add ? change.maxDelta : -change.maxDelta;
                    break;
                case "scholar":
                    stats.scholarMin += add ? change.minDelta : -change.minDelta;
                    stats.scholarMax += add ? change.maxDelta : -change.maxDelta;
                    break;
                case "foreign":
                    stats.foreignMin += add ? change.minDelta : -change.minDelta;
                    stats.foreignMax += add ? change.maxDelta : -change.maxDelta;
                    break;
                case "people":
                    stats.peopleMin += add ? change.minDelta : -change.minDelta;
                    stats.peopleMax += add ? change.maxDelta : -change.maxDelta;
                    break;
            }
        }
    }

    // 应用长期影响
    private void ApplyLongTermEffect(BuffLongTermEffect eff)
    {
        var stats = EventManager.Instance.stats;
        switch (eff.stat)
        {
            case "king": stats.king += eff.delta; break;
            case "noble": stats.noble += eff.delta; break;
            case "scholar": stats.scholar += eff.delta; break;
            case "foreign": stats.foreign += eff.delta; break;
            case "people": stats.people += eff.delta; break;
        }
    }

    // 可扩展：通过ID查找并添加Buff
    public void AddBuffById(string id)
    {
        var buff = buffs.Find(b => b.id == id);
        if (buff != null) AddBuff(buff);
    }

    public void ClearAllBuffs()
    {
        // 逆向撤销阈值
        foreach (var b in new List<BuffDefinition>(activeBuffs))
            RemoveBuff(b);
        activeBuffs.Clear();
    }
}