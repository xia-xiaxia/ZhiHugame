using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GameControl : MonoBehaviour
{
    public static GameControl Instance;
    public StatModel stats;  // 统计数据

    // 玩家国策（道具）栏，最多5个
    public Dictionary<string, PolicyItem> inventory = new Dictionary<string, PolicyItem>(5);

    // 添加道具（如已满需丢弃一个）
    public bool AddPolicy(PolicyItem item)
    {
        if (inventory.Count >= 5) return false;
        inventory.Add(item.id, item);
        return true;
    }

    // 丢弃道具
    public bool RemovePolicy(string id)
    {
        if (!inventory.ContainsKey(id)) return false;
        inventory.Remove(id);
        return true;
    }

    // 清空所有道具（如新开局）
    public void ClearPolicies()
    {
        inventory.Clear();
    }

    public bool GameOver = false;
    public bool GamePaused = false; // 游戏是否暂停（退出到主菜单）

    // 回合管理
    public int year = 1;
    private bool waitingForNextTurn = false;
    private Coroutine currentWaitCoroutine = null;

    private List<string> lastEvents = new List<string> { " ", " ", " " };

    // 暂停时的事件快照
    private string pausedEventId = null; // 暂停时正在播放的事件ID
    private int pausedSentenceIndex = 0; // 暂停时的句子索引

    // UI 相关
    public GameObject man;
    public GameObject objectsAboutEvent;


    // ====== 新增：结局与阈值相关 ======
    public int yearLimit = 100;          // 年份上限（Inspector 可调）
    private bool endingTriggered = false;
    // 是否使用 StatModel 内的动态阈值（否则使用固定值）
    public bool useDynamicThreshold = true;

    // 保存点击选项前的数值快照（用于免死道具恢复）
    private int snapshotKing, snapshotNoble, snapshotScholar, snapshotForeign, snapshotPeople;

    // 免死道具等待玩家选择的标志
    private bool waitingForDeathImmunityChoice = false;
    private bool deathImmunityChoiceResult = false;

    void Awake()
    {
        Instance = this;
        if (objectsAboutEvent != null)
            objectsAboutEvent.SetActive(false);
    }

    

    // ===== 开始游戏 =====
    public void StartGame()
    {
        // 如果是从暂停恢复，重新播放暂停时的事件
        if (GamePaused)
        {
            Debug.Log("[GameControl] 从暂停恢复游戏");
            GamePaused = false;
            
            if (CanvasMove.Instance != null)
                CanvasMove.Instance.StartGame(); // 切换到游戏场景
            
            // 恢复暂停时的事件
            if (!string.IsNullOrEmpty(pausedEventId))
            {
                Debug.Log($"[GameControl] 恢复暂停时的事件: {pausedEventId}, 句子索引: {pausedSentenceIndex}");
                if (UIManager.Instance != null)
                {
                    // 重新播放事件（从头开始）
                    UIManager.Instance.ShowEvent(pausedEventId);
                    // 如果需要恢复到具体句子，可以调用 UIManager 的恢复方法
                    // UIManager.Instance.RestoreEventState(pausedEventId, pausedSentenceIndex);
                }
                // 清除快照
                pausedEventId = null;
                pausedSentenceIndex = 0;
            }
            return;
        }

        // 检查相机是否准备好
        if (CanvasMove.Instance != null && !CanvasMove.Instance.isReady)
        {
            Debug.Log("[GameControl] 等待相机移动完成...");
            StartCoroutine(WaitForCameraReady());
            return;
        }
        year = 1;
        GameOver = false;
        endingTriggered = false;
        waitingForNextTurn = false;
        UIManager.Instance.daDian.SetActive(true);
        
        Debug.Log("[GameControl] 游戏开始");
        StartCoroutine(wait());
        ProcessNextTurn();
    }

    // 等待相机准备完成的协程
    private IEnumerator WaitForCameraReady()
    {
        while (CanvasMove.Instance != null && !CanvasMove.Instance.isReady)
        {
            yield return null; // 等待一帧
        }

        // 相机准备好后开始游戏
        Debug.Log("[GameControl]画面准备完成，开始游戏");
    year = 1;
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

    // 保存数值快照（选项应用前调用）
    public void SaveStatsSnapshot()
    {
        if (stats == null) return;
        snapshotKing = stats.king;
        snapshotNoble = stats.noble;
        snapshotScholar = stats.scholar;
        snapshotForeign = stats.foreign;
        snapshotPeople = stats.people;
        Debug.Log($"[GameControl] 保存数值快照: K{snapshotKing} N{snapshotNoble} S{snapshotScholar} F{snapshotForeign} P{snapshotPeople}");
    }

    // ===== 回合推进 =====
    public void ProcessNextTurn()
    {
        if (GameOver || waitingForNextTurn)
        {
            Debug.Log($"[GameControl] ProcessNextTurn blocked - GameOver: {GameOver}, waiting: {waitingForNextTurn}");
            return;
        }
        if(UIManager.Instance != null)
            if(UIManager.Instance.jinYan != null)
                if(!UIManager.Instance.jinYan.activeSelf)
                    UIManager.Instance.jinYan.SetActive(true);

        waitingForNextTurn = true;
        Debug.Log($"[GameControl] Year -> {year}");

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
            if (objectsAboutEvent != null)
                objectsAboutEvent.SetActive(false);
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
        {
            if (objectsAboutEvent != null)
                objectsAboutEvent.SetActive(true);
            UIManager.Instance.ShowEvent(eventId);
        }
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

        // 年份上限
        if (year >= yearLimit)
        {
            TriggerEnding("YEAR_LIMIT", $"到达年份上限 {yearLimit}，时代终结。");
            return;
        }

        // 取阈值（先应用所有阈值道具）
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

        // 应用所有阈值道具
        foreach (var item in inventory.Values)
        {
            if (item.type == 1 && item.whichChange == "king")
            {
                kMin += item.thresholdDeltadown;
                kMax += item.thresholdDeltaup;
            }
            if (item.type == 1 && item.whichChange == "noble")
            {
                nMin += item.thresholdDeltadown;
                nMax += item.thresholdDeltaup;
            }
            if (item.type == 1 && item.whichChange == "scholar")
            {
                sMin += item.thresholdDeltadown;
                sMax += item.thresholdDeltaup;
            }
            if (item.type == 1 && item.whichChange == "foreign")
            {
                fMin += item.thresholdDeltadown;
                fMax += item.thresholdDeltaup;
            }
            if (item.type == 1 && item.whichChange == "people")
            {
                pMin += item.thresholdDeltadown;
                pMax += item.thresholdDeltaup;
            }
            else
            {
                Debug.LogWarning($"[GameControl] 道具 {item.id} 的 whichChange 字段无效或未设置，无法应用阈值变化");
                continue;
            }

        }

        // 各属性越界检测（按优先顺序，遇到死亡先判免死道具）
        if (stats.king <= kMin)
        {
            if (TryUseDeathImmunity(-1)) return; // -1 表示国君下限
            TriggerEnding("KING_LOW", "国君势微，诸侯并起。"); return;
        }
        if (stats.king >= kMax)
        {
            if (TryUseDeathImmunity(1)) return; // 1 表示国君上限
            TriggerEnding("KING_HIGH", "国君权力过盛，天下动荡。"); return;
        }
        if (stats.noble <= nMin)
        {
            if (TryUseDeathImmunity(-3)) return; // -3 表示贵族下限
            TriggerEnding("NOBLE_LOW", "贵族式微，权力真空。"); return;
        }
        if (stats.noble >= nMax)
        {
            if (TryUseDeathImmunity(3)) return; // 3 表示贵族上限
            TriggerEnding("NOBLE_HIGH", "贵族权势滔天，王权旁落。"); return;
        }
        if (stats.scholar <= sMin)
        {
            if (TryUseDeathImmunity(-2)) return; // -2 表示卿士下限
            TriggerEnding("SCHOLAR_LOW", "士族凋零，典章失传。"); return;
        }
        if (stats.scholar >= sMax)
        {
            if (TryUseDeathImmunity(2)) return; // 2 表示卿士上限
            TriggerEnding("SCHOLAR_HIGH", "士族擅权，政务迟滞。"); return;
        }
        if (stats.foreign <= fMin)
        {
            if (TryUseDeathImmunity(-4)) return; // -4 表示外臣下限
            TriggerEnding("FOREIGN_LOW", "外臣尽失，朝堂孤立。"); return;
        }
        if (stats.foreign >= fMax)
        {
            if (TryUseDeathImmunity(4)) return; // 4 表示外臣上限
            TriggerEnding("FOREIGN_HIGH", "外臣干政，内权旁落。"); return;
        }
        if (stats.people <= pMin)
        {
            if (TryUseDeathImmunity(-5)) return; // -5 表示庶人下限
            TriggerEnding("PEOPLE_LOW", "民怨沸腾，天下反叛。"); return;
        }
        if (stats.people >= pMax)
        {
            if (TryUseDeathImmunity(5)) return; // 5 表示庶人上限
            TriggerEnding("PEOPLE_HIGH", "民意汹涌，改朝换代。"); return;
        }
    }

    // 免死道具判定与消耗
    private bool TryUseDeathImmunity(int deathType)
    {
        foreach (var item in inventory.Values)
        {
            if (item.type == 2 && item.usageCount != 0 && item.deathImmunity != null && item.deathImmunity.Contains(deathType))
            {
                // 弹窗询问玩家是否使用免死道具
                StartCoroutine(AskDeathImmunityChoice(item, deathType));
                return true; // 暂停结局判定，等待玩家选择
            }
        }
        return false;
    }

    // 询问玩家是否使用免死道具的协程
    private System.Collections.IEnumerator AskDeathImmunityChoice(PolicyItem item, int deathType)
    {
        waitingForDeathImmunityChoice = true;
        
        // 显示弹窗
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDeathImmunityPrompt(item, deathType);
        }

        // 等待玩家选择
        while (waitingForDeathImmunityChoice)
        {
            yield return null;
        }

        // 根据玩家选择执行
        if (deathImmunityChoiceResult)
        {
            // 玩家选择使用：消耗道具并恢复数值
            if (item.usageCount > 0) item.usageCount--;
            if (item.usageCount == 0)
            {
                // 次数用尽，从背包移除
                RemovePolicy(item.id);
            }
            
            stats.king = snapshotKing;
            stats.noble = snapshotNoble;
            stats.scholar = snapshotScholar;
            stats.foreign = snapshotForeign;
            stats.people = snapshotPeople;
            
            Debug.Log($"[GameControl] 玩家选择使用免死道具，类型{deathType}，道具ID:{item.id}，恢复到快照数值");
            UIManager.Instance?.UpdateStatText();
            
            // 显示免死道具生效文案
            if (!string.IsNullOrEmpty(item.deathdec) && UIManager.Instance != null)
            {
                UIManager.Instance.ShowDeathImmunityMessage(item.deathdec);
            }
        }
        else
        {
            // 玩家选择不使用：继续触发结局
            Debug.Log($"[GameControl] 玩家选择不使用免死道具，继续结局判定");
            // 这里需要重新触发结局，因为之前返回了 true 中断了判定
            // 暂时让游戏继续，实际可以在这里直接调用对应的 TriggerEnding
        }
    }

    // UIManager 调用：玩家选择使用免死道具
    public void OnDeathImmunityUse()
    {
        deathImmunityChoiceResult = true;
        waitingForDeathImmunityChoice = false;
    }

    // UIManager 调用：玩家选择不使用免死道具
    public void OnDeathImmunityDecline()
    {
        deathImmunityChoiceResult = false;
        waitingForDeathImmunityChoice = false;
    }

    // ===== 结局触发 =====
    public void TriggerEnding(string endingId, string endingDescription)
    {
        if (endingTriggered) return;
        endingTriggered = true;
        GameOver = true;
        Debug.Log($"[GameControl] 结局触发: {endingId} - {endingDescription}");

        // 游戏结束时，先生成新一轮商店道具
        if (PolicyManager.Instance != null)
            PolicyManager.Instance.GenerateShopItems(5);

        // 展示结局UI（需要 UIManager 实现 ShowEndingPanel）
        if (UIManager.Instance != null)
            UIManager.Instance.ShowEndingPanel(endingDescription);
    }

    // ===== 重开游戏 =====
    public void RestartGame()
    {
        // 重开时也生成新一轮商店道具
        if (PolicyManager.Instance != null)
            PolicyManager.Instance.GenerateShopItems(5);

        GameOver = false;
        endingTriggered = false;
        GamePaused = false; // 清除暂停状态
        year = 1;
        waitingForNextTurn = false;
        currentWaitCoroutine = null;
        lastEvents.Clear();
        lastEvents.Add(" ");
        lastEvents.Add(" ");
        lastEvents.Add(" ");

        // 清除暂停快照
        pausedEventId = null;
        pausedSentenceIndex = 0;

        if (stats != null)
            stats.ResetToDefault();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideEndingPanel();
            UIManager.Instance.UpdateStatText();
            UIManager.Instance.daDian.SetActive(true);
            UIManager.Instance.jinYan.SetActive(false);
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.ReloadAllEventsForRestart();
            EventManager.Instance.OnRestartCleanup();
        }

        BackToStartMenu();

        Debug.Log("[GameControl] 重开完成，等待玩家点击开始游戏");
        // 移除自动 ProcessNextTurn()，等待玩家手动点击开始游戏按钮
    }

    public void BackToStartMenu()
    {
        
        if( CanvasMove.Instance != null)
        {
            CanvasMove.Instance.BackToStart();
            
        }
        Debug.Log("[GameControl] 返回主菜单完成");
    }

    // ===== 暂停游戏并返回主菜单（不重置进度）=====
    public void PauseAndBackToMenu()
    {
        GamePaused = true;
        
        // 保存当前事件状态（快照）
        if (UIManager.Instance != null)
        {
            pausedEventId = UIManager.Instance.GetCurrentEventId();
            pausedSentenceIndex = UIManager.Instance.GetCurrentSentenceIndex();
            Debug.Log($"[GameControl] 游戏暂停，保存事件快照: {pausedEventId}, 句子索引: {pausedSentenceIndex}");
            
            // 隐藏游戏UI
            UIManager.Instance.HideEventOptions();
            UIManager.Instance.ClearText();
            if (UIManager.Instance.jinYan != null)
                UIManager.Instance.jinYan.SetActive(false);
        }
        else
        {
            Debug.Log("[GameControl] 游戏暂停，返回主菜单");
        }
        
        // 返回主菜单
        if (CanvasMove.Instance != null)
        {
            CanvasMove.Instance.BackToStart();
        }
    }

    // ===== 退出游戏 =====
    public void QuitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }

    // 获取玩家货币（活了多少年）
    public int GetCurrency()
    {
        return year;
    }

    // 扣除货币（购买道具时）
    public void SpendCurrency(int amount)
    {
        year -= amount;
        if (year < 0) year = 0;
        Debug.Log($"[GameControl] 花费 {amount} 年，剩余 {year} 年");
    }
}
