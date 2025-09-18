using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GameControl : MonoBehaviour
{
    public static GameControl Instance;
    public StatModel stats;  // 统计数据
    
    public bool GameOver = false;
    public bool IsCompleteTask = false;

    // 回合管理相关
    public int turns = 0; // 当前回合数
    private bool waitingForNextTurn = false; // 是否正在等待下一回合
    private Coroutine currentWaitCoroutine = null; // 当前运行的等待协程

    private List<string> lastEvents = new List<string> { " ", " ", " " }; // 记录最后的事件ID

    void Awake()
    {
        Instance = this;
    }

    public void CompleteTask()
    {
        if(EventManager.Instance != null)
        {
            foreach(var eventId in lastEvents)
            {
                if (EventManager.Instance.DetermineNextEventId() == "")
                    IsCompleteTask = true;
            }
        }
        Debug.Log("任务完成！");
        stats.year += 1;
        if (stats.year > 3)
        {
            GameOver = true;
            Debug.Log("游戏结束！");
        }
    }

    // ===== 回合管理逻辑 =====

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        turns = 0;
        GameOver = false;
        Debug.Log("[GameControl] 游戏开始");
        StartCoroutine(wait());
        ProcessNextTurn();
    }

    /// <summary>
    /// 处理下一个回合
    /// </summary>
    public void ProcessNextTurn()
    {
        if (GameOver || waitingForNextTurn)
        {
            Debug.Log($"[GameControl] ProcessNextTurn blocked - GameOver: {GameOver}, waitingForNextTurn: {waitingForNextTurn}");
            return;
        }

        waitingForNextTurn = true;
        turns++;
        Debug.Log($"[GameControl] ProcessNextTurn - Turn: {turns}");

        // 获取下一个事件ID
        string nextEventId = EventManager.Instance.DetermineNextEventId();

        // 显示事件
        ShowEventWithDelay(nextEventId);
    }

    /// <summary>
    /// 延迟显示事件
    /// </summary>
    private void ShowEventWithDelay(string eventId)
    {
        if (currentWaitCoroutine != null)
        {
            StopCoroutine(currentWaitCoroutine);
        }
        currentWaitCoroutine = StartCoroutine(WaitAndShowNextEvent(eventId));
    }

    /// <summary>
    /// 等待并显示下一个事件的协程
    /// </summary>
    private System.Collections.IEnumerator WaitAndShowNextEvent(string eventId)
    {
        Debug.Log($"[GameControl] WaitAndShowNextEvent started for event: {eventId}");

        // 显示 daDian
        if (UIManager.Instance.daDian != null)
        {
            UIManager.Instance.daDian.SetActive(true);
        }

        // 等待 2 秒
        yield return new UnityEngine.WaitForSeconds(2f);

        // 隐藏 daDian
        if (UIManager.Instance.daDian != null)
        {
            UIManager.Instance.daDian.SetActive(false);
        }

        // 显示事件
        UIManager.Instance.ShowEvent(eventId);

        waitingForNextTurn = false;
        currentWaitCoroutine = null;

        Debug.Log($"[GameControl] WaitAndShowNextEvent completed for event: {eventId}");
    }

    private IEnumerator wait()
    {
        yield return new WaitForSeconds(2f);
    }
}
