using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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
    
    // 用于在Inspector中绑定buff.json文件
    public TextAsset buffJson;
    
    public List<BuffDefinition> buffs = new List<BuffDefinition>();

    public List<Buffanime> animes = new List<Buffanime>();

    private int[] deltas = new int[5];

    // activeBuffs 现在引用 stats.buffBag
    // 不再使用私有列表，改为通过属性访问 StatModel 中的 buffBag
    private List<BuffDefinition> ActiveBuffs
    {
        get
        {
            if (EventManager.Instance != null && EventManager.Instance.stats != null)
            {
                return EventManager.Instance.stats.buffBag;
            }
            return new List<BuffDefinition>(); // 返回空列表作为后备
        }
    }


    void Awake()
    {
        Instance = this;
        LoadBuffs();
    }

    // 从JSON加载所有BUFF定义
    void LoadBuffs()
    {
        buffs.Clear();
        
        if (buffJson == null)
        {
            Debug.LogError("[BuffManager] buffJson 未绑定，无法加载BUFF定义");
            return;
        }

        try
        {
            BuffDefinition[] allBuffs = JsonHelper.FromJson<BuffDefinition>(buffJson.text);
            buffs.AddRange(allBuffs);
            Debug.Log($"[BuffManager] 加载了 {buffs.Count} 个BUFF定义");
            
            // 输出加载的BUFF ID用于调试
            foreach (var buff in buffs)
            {
                Debug.Log($"[BuffManager] 加载BUFF: ID={buff.id}, Name={buff.name}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BuffManager] 解析BUFF JSON失败: {ex.Message}");
        }
    }

    // 添加buff（创建副本，避免修改原始定义）
    public void AddBuff(BuffDefinition buff)
    {
        // 创建一个新的BUFF实例（深拷贝），避免修改原始定义
        BuffDefinition buffInstance = new BuffDefinition
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
        };

        //重复添加buff覆盖时间
        foreach(var abuff in ActiveBuffs)
        {
            if(abuff.id == buffInstance.id)
            {
                abuff.duration = buffInstance.duration;
                Debug.Log($"[BuffManager] 重复添加Buff: {buffInstance.name} (ID: {buffInstance.id}, 时限: {buffInstance.duration})");
                return;
            }
        }
        
        ActiveBuffs.Add(buffInstance);
        ShowBuff();
        Debug.Log($"[BuffManager] 添加Buff: {buffInstance.name} (ID: {buffInstance.id}, 时限: {buffInstance.duration})");
    }

    // 移除buff
    public void RemoveBuff(BuffDefinition buff)
    {
        ActiveBuffs.Remove(buff);
        Debug.Log($"[BuffManager] 移除Buff: {buff.name} (ID: {buff.id})");
        ShowBuff();
    }

    public void ShowBuff()
    {
        for (int i = 0; i < 5; i++) deltas[i] = 0;

        foreach(var abuff in ActiveBuffs)
        {
            deltas[0] += abuff.kingChange;
            deltas[1] += abuff.nobleChange;
            deltas[2] += abuff.scholarChange;
            deltas[3] += abuff.foreignChange;
            deltas[4] += abuff.peopleChange;
        }

        for(int i =0; i < 5; i++)
        {
            animes[i].ShowBuff(deltas[i]);
        }
    }

    // 每年结束时调用
    public void OnYearEnd()
    {

        Debug.Log($"[BuffManager] OnYearEnd 开始，当前激活BUFF数量: {ActiveBuffs.Count}");
        
        // 先应用所有BUFF的效果
        foreach (var buff in ActiveBuffs)
        {
            Debug.Log($"[BuffManager] 应用BUFF效果: {buff.name}, 剩余时限: {buff.duration}");
            
            // 应用数值变化
            if (buff.kingChange != 0)
            {
                ApplyLongTermEffect(new BuffLongTermEffect { stat = "king", delta = buff.kingChange });
                Debug.Log($"[BuffManager] - 国君变化: {buff.kingChange}");
            }
            if (buff.nobleChange != 0)
            {
                ApplyLongTermEffect(new BuffLongTermEffect { stat = "noble", delta = buff.nobleChange });
                Debug.Log($"[BuffManager] - 贵族变化: {buff.nobleChange}");
            }
            if (buff.scholarChange != 0)
            {
                ApplyLongTermEffect(new BuffLongTermEffect { stat = "scholar", delta = buff.scholarChange });
                Debug.Log($"[BuffManager] - 士族变化: {buff.scholarChange}");
            }
            if (buff.foreignChange != 0)
            {
                ApplyLongTermEffect(new BuffLongTermEffect { stat = "foreign", delta = buff.foreignChange });
                Debug.Log($"[BuffManager] - 外臣变化: {buff.foreignChange}");
            }
            if (buff.peopleChange != 0)
            {
                ApplyLongTermEffect(new BuffLongTermEffect { stat = "people", delta = buff.peopleChange });
                Debug.Log($"[BuffManager] - 国人变化: {buff.peopleChange}");
            }
        }
        

        // 然后处理时限并移除过期的BUFF
        List<BuffDefinition> buffsToRemove = new List<BuffDefinition>();
        foreach (var buff in ActiveBuffs)
        {
            // 处理持续时间（-1表示永久，不处理）
            if (buff.duration > 0)
            {
                buff.duration--;
                Debug.Log($"[BuffManager] BUFF时限递减: {buff.name}, 剩余时限: {buff.duration}");
                
                if (buff.duration == 0)
                {
                    buffsToRemove.Add(buff);
                    Debug.Log($"[BuffManager] BUFF时限到期，标记移除: {buff.name}");
                }
            }
        }
        
        // 移除过期的BUFF
        foreach (var buff in buffsToRemove)
        {
            RemoveBuff(buff);
        }
        
        Debug.Log($"[BuffManager] OnYearEnd 结束，当前激活BUFF数量: {ActiveBuffs.Count}");
    }


    // 应用长期影响
    private void ApplyLongTermEffect(BuffLongTermEffect eff)
    {
        var stats = EventManager.Instance.stats;
        int kingDelta = 0, nobleDelta = 0, scholarDelta = 0, foreignDelta = 0, peopleDelta = 0;
        
        switch (eff.stat)
        {
            case "king": kingDelta = eff.delta;  break; 
            case "noble":  nobleDelta = eff.delta; break; 
            case "scholar":  scholarDelta = eff.delta;  break; 
            case "foreign":  foreignDelta = eff.delta; break; 
            case "people":  peopleDelta = eff.delta;  break; 
        }
        
        // 使用带锁定检查的方法
        stats.ApplyStatChange(kingDelta, nobleDelta, scholarDelta, foreignDelta, peopleDelta, 1);
    }

    // 可扩展：通过ID查找并添加Buff
    public BuffDefinition AddBuffById(string id)
    {
        Debug.Log($"[BuffManager] 尝试通过ID添加BUFF: {id}");
        
        var buff = buffs.Find(b => b.id == id);
        if (buff != null)
        {
            Debug.Log($"[BuffManager] 找到BUFF定义: {buff.name} (ID: {buff.id})");
            AddBuff(buff);
            return buff;
        }
        else
        {
            Debug.LogError($"[BuffManager] 未找到ID为 {id} 的BUFF定义！");
            Debug.Log($"[BuffManager] 当前已加载的BUFF数量: {buffs.Count}");
            foreach (var b in buffs)
            {
                Debug.Log($"[BuffManager] - 已加载BUFF: ID={b.id}, Name={b.name}");
            }
        }
        return null;
    }

    public void ClearAllBuffs()
    {
        // 逆向撤销阈值
        foreach (var b in new List<BuffDefinition>(ActiveBuffs))
            RemoveBuff(b);
        ActiveBuffs.Clear();
        ShowBuff();
    }

    // ====== 新增：对外查询和移除接口，供UI调用 ======
    public IReadOnlyList<BuffDefinition> GetActiveBuffs()
    {
        return ActiveBuffs.AsReadOnly();
    }

    public bool RemoveBuffById(string id)
    {
        var buff = ActiveBuffs.Find(b => b.id == id);
        if (buff == null) return false;
        RemoveBuff(buff);
        return true;
    }
}