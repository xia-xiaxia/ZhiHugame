using System.Collections.Generic;
using UnityEngine;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;
    public TextAsset endingJsonAsset;
    public List<EndingDefinition> endings = new List<EndingDefinition>();

    private bool endingTriggered = false;
    private static int loadEventsCallCount = 0;

    void Awake()
    {
        Instance = this;
        LoadEndings();
    }

    public void ResetEndingFlag()
    {
        endingTriggered = false;
    }

    void LoadEndings()
    {
        if (endingJsonAsset == null)
        {
            Debug.LogError("[EndingManager] 未指定结局JSON");
            return;
        }
        if (endings == null || endings.Count == 0) // 简单缓存避免重复解析
            endings = new List<EndingDefinition>(JsonHelper.FromJson<EndingDefinition>(endingJsonAsset.text));
    }

    // 检查所有数值结局
    public void CheckStatEnding(StatModel stats)
    {
        if (endingTriggered || GameControl.Instance != null && GameControl.Instance.GameOver) return;
        foreach (var ed in endings)
        {
            if (!string.IsNullOrEmpty(ed.trigger.stat))
            {
                int val = GetStatValue(stats, ed.trigger.stat);
                if ((ed.trigger.type == "max" && val >= ed.trigger.value) ||
                    (ed.trigger.type == "min" && val <= ed.trigger.value))
                {
                    ShowEnding(ed);
                    return;
                }
            }
        }
    }

    // 任务结局触发
    public void CheckTaskEnding(string taskId, bool success)
    {
        if (endingTriggered || GameControl.Instance != null && GameControl.Instance.GameOver) return;
        foreach (var ed in endings)
        {
            if (ed.trigger.taskId == taskId)
            {
                if ((ed.trigger.type == "success" && success) ||
                    (ed.trigger.type == "fail" && !success))
                {
                    ShowEnding(ed);
                    return;
                }
            }
        }
    }

    // 展示结局
    public void ShowEnding(EndingDefinition ed)
    {
        if (endingTriggered) return;
        endingTriggered = true;

        if (GameControl.Instance != null)
            GameControl.Instance.TriggerEnding(ed.id, ed.description);

        // 只触发一次 GameOver
        if (EventManager.Instance != null)
            EventManager.Instance.HandleGameOver("结局:" + ed.id);
    }

    public void LoadEvents()
    {
        loadEventsCallCount++;
        if (loadEventsCallCount > 3)
        {
            Debug.LogWarning($"[EventManager] LoadEvents 调用次数 = {loadEventsCallCount} (请确认是否必要)");
        }
        // ...原始加载逻辑...
    }

    int GetStatValue(StatModel stats, string stat)
    {
        switch (stat)
        {
            case "king": return stats.king;
            case "noble": return stats.noble;
            case "scholar": return stats.scholar;
            case "foreign": return stats.foreign;
            case "people": return stats.people;
            default: return 0;
        }
    }
}
