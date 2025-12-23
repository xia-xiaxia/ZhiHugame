using UnityEngine;

/// <summary>
/// 选项效果处理器：负责处理选项的各种效果
/// </summary>
public class OptionEffectHandler : MonoBehaviour
{
    public static OptionEffectHandler Instance;
    public StatModel stats;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    /// <summary>
    /// 应用选项效果
    /// </summary>
    public void ApplyOption(Option opt, int currentYear, int yearDelta)
    {
        if (opt == null)
        {
            Debug.LogError("[OptionEffectHandler] opt 为 null");
            return;
        }

        if (stats == null)
        {
            Debug.LogError("[OptionEffectHandler] stats 未绑定");
            return;
        }

        EventDisplayUI.Instance.currentEventSentences.Clear();

        // 1. 检查特殊结局
        if (TryTriggerSpecialEnding(opt))
        {
            return; // 结局触发后直接返回
        }

        // 2. 应用数值变化
        ApplyStatChanges(opt);

        // 处理事件判定值
        GameControl.Instance.gameStatistics.setJudgeValue(opt.eventFlags);

        // 3. 处理 BUFF 激活
        HandleBuffActivation(opt);

        // 4. 处理后继事件
        HandleNextEvent(opt, currentYear);

        // 5. 处理激活或关闭事件集
        if (opt.randomEventSet != null && opt.randomEventSet.Length > 0)
        {
            EventDatabase.Instance?.ActivateEventSetById(opt.randomEventSet);
            Debug.Log($"[OptionEffectHandler] 更新激活事件集: {opt.randomEventSet}");
        }

        // 6. 更新 UI 和通知游戏控制器
        // 年份增加和BUFF处理
        if (GameControl.Instance != null)
        {
            GameControl.Instance.year += yearDelta;
            if (yearDelta != 0)
            {
                for (int i = 0; i < yearDelta; i++)
                {
                    BuffManager.Instance?.OnYearEnd();
                }
            }
        }
        stats.OnYearEnd();
        UIManager.Instance?.UpdateStatText();
        UIManager.Instance?.ClearText();
        GameControl.Instance?.OnStatsChanged();
    }

    /// <summary>
    /// 尝试触发特殊结局
    /// </summary>
    private bool TryTriggerSpecialEnding(Option opt)
    {
        if (opt.kingChange == 999)
        {
            Debug.Log("[OptionEffectHandler] 触发骑马结局");
            GameControl.Instance?.TriggerEnding("神", "你骑上了那匹马，的确驾驭不住——随后坠马而死");
            return true;
        }
        
        if (opt.kingChange == 666)
        {
            Debug.Log("[OptionEffectHandler] 触发盗匪结局");
            GameControl.Instance?.TriggerEnding("献", "你身先士卒，冲到战场之上拼杀，随后被敌人一剑刺死");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 应用数值变化
    /// </summary>
    private void ApplyStatChanges(Option opt)
    {
        // 使用带锁定检查的方法
        stats.ApplyStatChange(opt.kingChange, opt.nobleChange, opt.scholarChange, opt.foreignChange, opt.peopleChange);

        Debug.Log($"[OptionEffectHandler] 数值变化: " +
            $"王权{opt.kingChange:+#;-#;0} " +
            $"贵族{opt.nobleChange:+#;-#;0} " +
            $"学者{opt.scholarChange:+#;-#;0} " +
            $"外交{opt.foreignChange:+#;-#;0} " +
            $"民心{opt.peopleChange:+#;-#;0}");
    }

    /// <summary>
    /// 处理 BUFF 激活
    /// </summary>
    private void HandleBuffActivation(Option opt)
    {
        if (string.IsNullOrEmpty(opt.activateBUFF))
            return;

        if (BuffManager.Instance == null)
        {
            Debug.LogError("[OptionEffectHandler] BuffManager.Instance 为 null");
            return;
        }

        BuffDefinition buff = BuffManager.Instance.AddBuffById(opt.activateBUFF);
        if (buff != null)
        {
            Debug.Log($"[OptionEffectHandler] 激活 BUFF: {buff.name}");
        }
        else
        {
            Debug.LogWarning($"[OptionEffectHandler] 未找到 BUFF: {opt.activateBUFF}");
        }
    }

    /// <summary>
    /// 处理后继事件
    /// </summary>
    private void HandleNextEvent(Option opt, int currentYear)
    {
        if (string.IsNullOrEmpty(opt.nextEventId) || opt.nextEventId == "0")
            return;

        if (EventSelector.Instance == null)
        {
            Debug.LogError("[OptionEffectHandler] EventSelector.Instance 为 null");
            return;
        }

        // 延时事件
        if (opt.interval > 0)
        {
            int triggerYear = currentYear + opt.interval;
            EventSelector.Instance.AddDelayedEvent(triggerYear, opt.nextEventId);
            Debug.Log($"[OptionEffectHandler] 延时事件: {opt.nextEventId}，{opt.interval}年后触发（第{triggerYear}年）");
        }
        // 立即后继
        else
        {
            EventSelector.Instance.SetNextEventId(opt.nextEventId);
            Debug.Log($"[OptionEffectHandler] 后继事件: {opt.nextEventId}");
        }
    }
}
