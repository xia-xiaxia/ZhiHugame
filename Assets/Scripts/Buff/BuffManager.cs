using System;
using System.Collections.Generic;
using UnityEngine;

// Buff长期效果的辅助类
[Serializable]
public class BuffLongTermEffect
{
    public string stat;  // 影响的数值类型："king", "noble", "scholar", "foreign", "people"
    public int delta;    // 每年变化的数值
}

[Serializable]
public class BuffDefinition
{
    public string id;
    public string name; // 时局名称
    public string description;
    public string result;
    public int duration = -1; // -1永久，0立即结束，>0为年数
    public int kingChange;      // 国君，每年的变化值
    public int nobleChange;     // 贵族，每年的变化值
    public int scholarChange;   // 士族，每年的变化值
    public int foreignChange;   // 外臣，每年的变化值
    public int peopleChange;    // 国人，每年的变化值

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
        Debug.Log($"[BuffManager] 添加Buff: {buff.description}");
    }

    // 移除buff
    public void RemoveBuff(BuffDefinition buff)
    {
        activeBuffs.Remove(buff);
        Debug.Log($"[BuffManager] 移除Buff: {buff.description}");
    }

    // 每年结束时调用
    public void OnYearEnd()
    {
        foreach (var buff in new List<BuffDefinition>(activeBuffs))
        {
            // 处理长期影响
            ApplyLongTermEffect(new BuffLongTermEffect { stat = "king", delta = buff.kingChange });
            ApplyLongTermEffect(new BuffLongTermEffect { stat = "noble", delta = buff.nobleChange });
            ApplyLongTermEffect(new BuffLongTermEffect { stat = "scholar", delta = buff.scholarChange });
            ApplyLongTermEffect(new BuffLongTermEffect { stat = "foreign", delta = buff.foreignChange });
            ApplyLongTermEffect(new BuffLongTermEffect { stat = "people", delta = buff.peopleChange });

            // 处理持续时间
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
    public BuffDefinition AddBuffById(string id)
    {
        var buff = buffs.Find(b => b.id == id);
        if (buff != null) AddBuff(buff);
        return buff;
    }

    public void ClearAllBuffs()
    {
        // 逆向撤销阈值
        foreach (var b in new List<BuffDefinition>(activeBuffs))
            RemoveBuff(b);
        activeBuffs.Clear();
    }

    // ====== 新增：对外查询和移除接口，供UI调用 ======
    public IReadOnlyList<BuffDefinition> GetActiveBuffs()
    {
        return activeBuffs.AsReadOnly();
    }

    public bool RemoveBuffById(string id)
    {
        var buff = activeBuffs.Find(b => b.id == id);
        if (buff == null) return false;
        RemoveBuff(buff);
        return true;
    }
}