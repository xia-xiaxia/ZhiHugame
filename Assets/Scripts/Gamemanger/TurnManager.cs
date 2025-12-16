using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 回合管理器：负责回合推进、事件显示、事件记录
/// </summary>
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("回合数据")]
    public int year = 1;
    
    [Header("引用")]
    public StatModel stats;
    public GameObject objectsAboutEvent;

    [Header("内部状态")]
    private bool waitingForNextTurn = false;
    private Coroutine currentWaitCoroutine = null;
    private List<string> lastEvents = new List<string> { " ", " ", " " };

    void Awake()
    {
        Instance = this;
    }

    // ===== 重置回合 =====
    public void ResetTurn()
    {
        year = 1;
        waitingForNextTurn = false;
        currentWaitCoroutine = null;
        lastEvents.Clear();
        lastEvents.Add(" ");
        lastEvents.Add(" ");
        lastEvents.Add(" ");
    }

    // ===== 回合推进 =====
    public void ProcessNextTurn()
    {
        if (GameLifecycleManager.Instance.GameOver || waitingForNextTurn)
        {
            Debug.Log($"[TurnManager] ProcessNextTurn blocked - GameOver: {GameLifecycleManager.Instance.GameOver}, waiting: {waitingForNextTurn}");
            return;
        }

        if (UIManager.Instance?.jinYan != null)
        {
            if (!UIManager.Instance.jinYan.activeSelf)
                UIManager.Instance.jinYan.SetActive(true);
        }

        waitingForNextTurn = true;
        Debug.Log($"[TurnManager] Year -> {year}");
        
        stats.year = year;
        if (StatsDisplayUI.Instance?.currentYearText != null)
        {
            StatsDisplayUI.Instance.currentYearText.text = "第" + year.ToString() + "年";
        }

        // 减少锁定时长
        stats.DecrementLayerLocks();

        // 检查结局
        EndingManager.Instance?.CheckAndTriggerEnding();
        
        if (GameLifecycleManager.Instance.GameOver)
        {
            waitingForNextTurn = false;
            return;
        }

        string nextEventId = EventManager.Instance.DetermineNextEventId();
        ShowEventWithDelay(nextEventId);
    }

    // ===== 事件显示延迟 =====
    private void ShowEventWithDelay(string eventId)
    {
        if (currentWaitCoroutine != null)
            StopCoroutine(currentWaitCoroutine);
        
        currentWaitCoroutine = StartCoroutine(WaitAndShowNextEvent(eventId));
    }

    private IEnumerator WaitAndShowNextEvent(string eventId)
    {
        if (GameLifecycleManager.Instance.GameOver)
        {
            waitingForNextTurn = false;
            yield break;
        }

        if (objectsAboutEvent != null)
            objectsAboutEvent.SetActive(false);

        //yield return new WaitForSeconds(0.5f);

        if (!GameLifecycleManager.Instance.GameOver && UIManager.Instance != null)
        {
            if (objectsAboutEvent != null)
                objectsAboutEvent.SetActive(true);
            
            if (UIManager.Instance.jinYan != null && !UIManager.Instance.jinYan.activeSelf)
                UIManager.Instance.jinYan.SetActive(true);
            
            UIManager.Instance.ShowEvent(eventId);
        }
        
        waitingForNextTurn = false;
        currentWaitCoroutine = null;
    }

    // ===== 记录事件历史 =====
    public void RecordEvent(string eventId)
    {
        lastEvents.Add(eventId);
        if (lastEvents.Count > 3)
            lastEvents.RemoveAt(0);
    }

    // ===== 停止等待协程 =====
    public void StopWaitingCoroutine()
    {
        if (currentWaitCoroutine != null)
        {
            StopCoroutine(currentWaitCoroutine);
            currentWaitCoroutine = null;
        }
        waitingForNextTurn = false;
    }
}
