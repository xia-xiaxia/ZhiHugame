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

    // 回合管理
    public int turns = 0;
    private bool waitingForNextTurn = false;
    private Coroutine currentWaitCoroutine = null;

    private List<string> lastEvents = new List<string> { " ", " ", " " };

    public GameObject man;
    // ====== 新增：结局与阈值相关 ======
    public int turnLimit = 60;          // 回合上限（Inspector 可调）
    private bool endingTriggered = false;
    // 是否使用 StatModel 内的动态阈值（否则使用固定值）
    public bool useDynamicThreshold = true;

    void Awake()
    {
        Instance = this;
    }

    // ===== 任务完成示例（保持原逻辑）=====
    public void CompleteTask()
    {
        if (EventManager.Instance != null)
        {
            foreach (var eventId in lastEvents)
            {
                if (EventManager.Instance.DetermineNextEventId() == "")
                    IsCompleteTask = true;
            }
        }
        Debug.Log("任务完成！");
    }

    // ===== 开始游戏 =====
    public void StartGame()
    {
        // 检查相机是否准备好
        if (CameraMove.Instance != null && !CameraMove.Instance.isReady)
        {
            Debug.Log("[GameControl] 等待相机移动完成...");
            StartCoroutine(WaitForCameraReady());
            return;
        }

        turns = 0;
        GameOver = false;
        endingTriggered = false;
        waitingForNextTurn = false;
        //UIManager.Instance.daDian.SetActive(true);
        Debug.Log("[GameControl] 游戏开始");
        StartCoroutine(wait());
        ProcessNextTurn();
    }

    // 等待相机准备完成的协程
    private IEnumerator WaitForCameraReady()
    {
        while (CameraMove.Instance != null && !CameraMove.Instance.isReady)
        {
            yield return null; // 等待一帧
        }

        // 相机准备好后开始游戏
        Debug.Log("[GameControl] 相机准备完成，开始游戏");
        turns = 0;
        GameOver = false;
        endingTriggered = false;
        waitingForNextTurn = false;
        StartCoroutine(wait());
        ProcessNextTurn();
    }

    // ===== 外部在数值变化后调用（例如在 EventManager.ApplyOption 里调用 GameControl.Instance.OnStatsChanged();）=====
    public void OnStatsChanged()
    {
        if (GameOver) return;
        CheckAndTriggerEnding(); // 数据变化后立即检测
    }

    // ===== 回合推进 =====
    public void ProcessNextTurn()
    {
        if (GameOver || waitingForNextTurn)
        {
            Debug.Log($"[GameControl] ProcessNextTurn blocked - GameOver: {GameOver}, waiting: {waitingForNextTurn}");
            return;
        }

        waitingForNextTurn = true;
        turns++;
        Debug.Log($"[GameControl] Turn -> {turns}");

        // 回合数也可能触发结局
        CheckAndTriggerEnding();
        if (GameOver) { waitingForNextTurn = false; return; }

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
        if (GameOver)
        {
            waitingForNextTurn = false;
            yield break;
        }

        // 显示大殿
        if (UIManager.Instance != null && UIManager.Instance.daDian != null)
        {
            UIManager.Instance.daDian.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            if (man != null)
            {
                man.SetActive(true);
            }
        }

        yield return new WaitForSeconds(1f);

        if (UIManager.Instance != null && UIManager.Instance.daDian != null)
        {
            UIManager.Instance.daDian.SetActive(false);
            man.SetActive(false);
        }

        if (!GameOver && UIManager.Instance != null)
            UIManager.Instance.ShowEvent(eventId);

        waitingForNextTurn = false;
        currentWaitCoroutine = null;
    }

    private IEnumerator wait()
    {
        yield return new WaitForSeconds(2f);
    }

    public void RecordEvent(string eventId)
    {
        lastEvents.Add(eventId);
        if (lastEvents.Count > 3)
            lastEvents.RemoveAt(0);
    }

    // ===== 核心：结局检测 =====
    private void CheckAndTriggerEnding()
    {
        if (endingTriggered || stats == null) return;

        // 回合上限
        if (turns >= turnLimit)
        {
            TriggerEnding("TURN_LIMIT", $"到达回合上限 {turnLimit}，时代终结。");
            return;
        }

        // 取阈值
        int kMin = useDynamicThreshold ? stats.kingMin : 20;
        int kMax = useDynamicThreshold ? stats.kingMax : 80;
        int nMin = useDynamicThreshold ? stats.nobleMin : 20;
        int nMax = useDynamicThreshold ? stats.nobleMax : 80;
        int sMin = useDynamicThreshold ? stats.scholarMin : 20;
        int sMax = useDynamicThreshold ? stats.scholarMax : 80;
        int fMin = useDynamicThreshold ? stats.foreignMin : 20;
        int fMax = useDynamicThreshold ? stats.foreignMax : 80;
        int pMin = useDynamicThreshold ? stats.peopleMin : 20;
        int pMax = useDynamicThreshold ? stats.peopleMax : 80;

        // 各属性越界检测（按优先顺序）
        if (stats.king <= kMin) { TriggerEnding("KING_LOW", "国君势微，诸侯并起。"); return; }
        if (stats.king >= kMax) { TriggerEnding("KING_HIGH", "国君权力过盛，天下动荡。"); return; }

        if (stats.noble <= nMin) { TriggerEnding("NOBLE_LOW", "贵族式微，权力真空。"); return; }
        if (stats.noble >= nMax) { TriggerEnding("NOBLE_HIGH", "贵族权势滔天，王权旁落。"); return; }

        if (stats.scholar <= sMin) { TriggerEnding("SCHOLAR_LOW", "士族凋零，典章失传。"); return; }
        if (stats.scholar >= sMax) { TriggerEnding("SCHOLAR_HIGH", "士族擅权，政务迟滞。"); return; }

        if (stats.foreign <= fMin) { TriggerEnding("FOREIGN_LOW", "外臣尽失，朝堂孤立。"); return; }
        if (stats.foreign >= fMax) { TriggerEnding("FOREIGN_HIGH", "外臣干政，内权旁落。"); return; }

        if (stats.people <= pMin) { TriggerEnding("PEOPLE_LOW", "民怨沸腾，天下反叛。"); return; }
        if (stats.people >= pMax) { TriggerEnding("PEOPLE_HIGH", "民意汹涌，改朝换代。"); return; }
    }

    // ===== 结局触发 =====
    public void TriggerEnding(string endingId, string endingDescription)
    {
        if (endingTriggered) return;
        endingTriggered = true;
        GameOver = true;
        Debug.Log($"[GameControl] 结局触发: {endingId} - {endingDescription}");

        // 展示结局UI（需要 UIManager 实现 ShowEndingPanel）
        if (UIManager.Instance != null)
            UIManager.Instance.ShowEndingPanel(endingDescription);
    }

    // ===== 重开 =====
    public void RestartGame()
    {
        GameOver = false;
        endingTriggered = false;
        turns = 0;
        waitingForNextTurn = false;
        currentWaitCoroutine = null;
        lastEvents.Clear();
        lastEvents.Add(" ");
        lastEvents.Add(" ");
        lastEvents.Add(" ");

        if (stats != null)
            stats.ResetToDefault();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideEndingPanel();
            UIManager.Instance.UpdateStatText();
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.ReloadAllEventsForRestart();
            EventManager.Instance.OnRestartCleanup();
        }

        Debug.Log("[GameControl] 重开完成");
        ProcessNextTurn();
    }
    
    // ===== 退出游戏 =====
    public void QuitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }
}
