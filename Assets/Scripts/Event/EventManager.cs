using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 事件管理器作为协调器，整合各个子系统
/// - EventDatabase: 事件数据加载和查询
/// - EventSelector: 事件抽取和白名单管理
/// - OptionEffectHandler: 选项效果处理
/// </summary>
public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    public StatModel stats;
    
    public AudioClip Bgm;
    public AudioSource audioSrc;

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

    private void Start()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateStatText();
    }

    /// <summary>
    /// 获取事件（代理到 EventDatabase）
    /// </summary>
    public GameEvent GetEvent(string eventId)
    {
        if (EventDatabase.Instance == null)
        {
            Debug.LogError("[EventManager] EventDatabase.Instance 为 null");
            return null;
        }
        return EventDatabase.Instance.GetEvent(eventId);
    }

    /// <summary>
    /// 决定下一个事件ID（代理到 EventSelector）
    /// </summary>
    public string DetermineNextEventId()
    {
        if (EventSelector.Instance == null)
        {
            Debug.LogError("[EventManager] EventSelector.Instance 为 null");
            return "0";
        }

        if (GameControl.Instance == null)
        {
            Debug.LogError("[EventManager] GameControl.Instance 为 null");
            return "0";
        }

        return EventSelector.Instance.DetermineNextEventId(GameControl.Instance.year);
    }

    /// <summary>
    /// 应用选项效果（代理到 OptionEffectHandler）
    /// </summary>
    public void ApplyOption(Option opt, int year)
    {
        if (OptionEffectHandler.Instance == null)
        {
            Debug.LogError("[EventManager] OptionEffectHandler.Instance 为 null");
            return;
        }

        OptionEffectHandler.Instance.ApplyOption(opt, year);
    }

    /// <summary>
    /// 设置下一个事件ID（代理到 EventSelector）
    /// </summary>
    public void SetNextEventId(string eventId)
    {
        if (EventSelector.Instance != null)
        {
            EventSelector.Instance.SetNextEventId(eventId);
        }
    }

    /// <summary>
    /// 游戏结束处理
    /// </summary>
    public void HandleGameOver(string reason)
    {
        Debug.LogWarning($"[EventManager] 游戏失败: {reason}");
        
        if (EventSelector.Instance != null)
        {
            EventSelector.Instance.Reset();
        }

        if (stats != null)
        {
            Debug.Log("[EventManager] 可在此重置数值（目前未重置）");
        }

        UIManager.Instance?.UpdateStatText();
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void ReloadAllEventsForRestart()
    {
        if (EventSelector.Instance != null)
        {
            EventSelector.Instance.Reset();
        }

        if (EventDatabase.Instance != null)
        {
            EventDatabase.Instance.LoadEvents();
        }

        Debug.Log("[EventManager] 已重置游戏状态");
    }

    /// <summary>
    /// 清理（兼容旧代码）
    /// </summary>
    public void OnRestartCleanup()
    {
        // 保留空方法以防其他代码调用
    }
}