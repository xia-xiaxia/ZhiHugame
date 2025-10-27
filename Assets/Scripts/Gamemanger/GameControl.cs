using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class GameControl : MonoBehaviour
{
    public static GameControl Instance;
    public StatModel stats;  // 统计数据
    // 注意：currency 现在从 stats.currency 读取，不再使用独立字段

    // 添加道具（如已满需丢弃一个）
    public bool AddPolicy(PolicyItem item)
    {
        if (stats == null || stats.policyBag == null) return false;
        if (stats.policyBag.Count >= 5) return false;
        
        // 检查是否已存在
        if (stats.policyBag.Any(p => p.id == item.id)) return false;
        
        stats.policyBag.Add(item);
        return true;
    }

    // 丢弃道具
    public bool RemovePolicy(string id)
    {
        if (stats == null || stats.policyBag == null) return false;
        
        PolicyItem item = stats.policyBag.FirstOrDefault(p => p.id == id);
        if (item == null) return false;
        
        stats.policyBag.Remove(item);
        return true;
    }

    // 清空所有道具（如新开局）
    public void ClearPolicies()
    {
        if (stats != null && stats.policyBag != null)
        {
            stats.policyBag.Clear();
        }
    }
    
    // 获取道具（通过ID）
    public PolicyItem GetPolicy(string id)
    {
        if (stats == null || stats.policyBag == null) return null;
        return stats.policyBag.FirstOrDefault(p => p.id == id);
    }

    public bool GameOver = false;
    public bool GamePaused = false; // 游戏是否暂停（退出到主菜单）

    // 回合管理
    public int year = 1;
    private bool waitingForNextTurn = false;
    private Coroutine currentWaitCoroutine = null;
    
    // 防止 StartGame 重复调用
    private bool isGameStarting = false;

    private List<string> lastEvents = new List<string> { " ", " ", " " };

    // 暂停时的事件快照
    private string pausedEventId = null; // 暂停时正在播放的事件ID
    private int pausedSentenceIndex = 0; // 暂停时的句子索引

    // UI 相关
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
    public List<PolicyItem> policyBag = new List<PolicyItem>();

    void Awake()
    {
        Instance = this;
        if (objectsAboutEvent != null)
            objectsAboutEvent.SetActive(false);
    }

    // ===== 开始游戏按钮调用（初始化并触发淡入淡出）=====
    public void OnStartGameButtonClicked()
    {
        Debug.Log("[GameControl] 开始游戏按钮被点击");
        
        // 重置游戏启动标志
        isGameStarting = false;
        
        // 如果是从暂停状态恢复
        if (GamePaused)
        {
            Debug.Log("[GameControl] 从暂停状态恢复游戏");
            GamePaused = false;
            GameOver = false;
            
            // 切换场景（会触发淡入淡出）
            if (CanvasMove.Instance != null)
            {
                CanvasMove.Instance.StartGame();
            }
            
            return;
        }
        
        // 新游戏初始化
        Debug.Log("[GameControl] 初始化新游戏数据");
        GameOver = false;
        GamePaused = false;
        year = 1;
        
        // 重置数值（但不重置 currency，让它累计）
        stats.king = 50;
        stats.noble = 50;
        stats.scholar = 50;
        stats.foreign = 50;
        stats.people = 50;
        stats.year = 1;
        // stats.currency 保持不变，累计上一局的
        
        // policyBag 保留，不清空（道具可以累计到下一局）
        // ClearPolicies(); // 注释掉，道具应该保留
        Debug.Log($"[GameControl] 保留道具数量: {stats.policyBag.Count}");
        
        // 清除暂停快照
        pausedEventId = null;
        pausedSentenceIndex = 0;

        if (objectsAboutEvent != null)
            objectsAboutEvent.SetActive(false);
        
        // 触发场景切换淡入淡出（CanvasMove 会在合适时机调用 StartGame）
        if (CanvasMove.Instance != null)
        {
            CanvasMove.Instance.StartGame();
        }
    }

    // ===== 由 CanvasMove 在淡入到阈值时调用（开始游戏逻辑）=====
    public void StartGame()
    {
        // 防止重复调用
        if (isGameStarting)
        {
            Debug.Log("[GameControl] StartGame 已在执行中，忽略重复调用");
            return;
        }
        
        isGameStarting = true;
        Debug.Log("[GameControl] 开始游戏（由CanvasMove调用）");
        
        // 确保 objectsAboutEvent 初始隐藏，等待事件显示协程控制
        if (objectsAboutEvent != null)
        {
            objectsAboutEvent.SetActive(false);
        }
        
        // 如果是从暂停恢复
        if (!string.IsNullOrEmpty(pausedEventId))
        {
            Debug.Log($"[GameControl] 恢复暂停的事件: {pausedEventId}, 句子索引: {pausedSentenceIndex}");
            
            // 显示游戏 UI 面板（但不显示 objectsAboutEvent）
            if (UIManager.Instance != null && UIManager.Instance.jinYan != null)
            {
                UIManager.Instance.jinYan.SetActive(true);
            }
            
            // 延迟显示事件内容，让淡入动画完成（等待淡入从0.8到1.0 + 额外缓冲）
            StartCoroutine(DelayedRestoreEvent(pausedEventId, pausedSentenceIndex));
            
            // 清除暂停快照
            pausedEventId = null;
            pausedSentenceIndex = 0;
            
            return;
        }
        
        // 新游戏：显示第一个事件
        Debug.Log("[GameControl] 新游戏，显示第一个事件");
        
        // 显示游戏 UI 面板
        if (UIManager.Instance != null && UIManager.Instance.jinYan != null)
        {
            UIManager.Instance.jinYan.SetActive(true);
        }
        
        // 更新 UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStatText();
            UIManager.Instance.UpdateCurrencyDisplay();
        }
        
        // 延迟显示第一个事件，等待淡入动画完全完成
        StartCoroutine(DelayedProcessFirstTurn());
    }

    // 延迟处理第一回合（等待淡入动画完成）
    private IEnumerator DelayedProcessFirstTurn()
    {
        // 等待淡入动画完全完成（从0.8到1.0大约需要0.2秒，再加0.3秒缓冲）
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("[GameControl] 淡入动画完成，开始显示第一个事件");
        ProcessNextTurn();
        
        // 重置标志，允许下次调用
        isGameStarting = false;
    }

    // 延迟恢复暂停的事件（等待淡入动画完成）
    private IEnumerator DelayedRestoreEvent(string eventId, int sentenceIndex)
    {
        // 等待淡入动画完全完成
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("[GameControl] 淡入动画完成，恢复暂停的事件");
        
        // 显示事件面板
        if (objectsAboutEvent != null)
        {
            objectsAboutEvent.SetActive(true);
        }
        
        // 恢复事件状态
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RestoreEventState(eventId, sentenceIndex);
        }
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
        stats.year = year;
        UIManager.Instance.currentYearText.text = "第" + year.ToString() + "年";

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

        // 使用 jinYan 作为主事件面板，不再在此隐藏它，避免误关。
        if (objectsAboutEvent != null)
            objectsAboutEvent.SetActive(false);

        // 保留一个轻微延时以避免突兀切换（可按需调整/删除）
        yield return new WaitForSeconds(0.5f);

        if (!GameOver && UIManager.Instance != null)
        {
            if (objectsAboutEvent != null)
                objectsAboutEvent.SetActive(true);
            // 确保 jinYan 已开启
            if (UIManager.Instance.jinYan != null && !UIManager.Instance.jinYan.activeSelf)
                UIManager.Instance.jinYan.SetActive(true);
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

        // 直接使用 StatModel 中的阈值（阈值道具使用时已修改这些值）
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

        // 各属性越界检测（按优先顺序，遇到死亡先判免死道具）
        if (stats.king <= kMin)
        {
            if (TryUseDeathImmunity(-1)) return; // -1 表示国君下限
            TriggerEnding("哀", "在你治下君主逐渐沦为群臣的傀儡，为了更好的掌控局面，他们为你呈上了一杯毒酒"); return;
        }
        if (stats.king >= kMax)
        {
            if (TryUseDeathImmunity(1)) return; // 1 表示国君上限
            TriggerEnding("躁", "你刚愎自用，人们不满你的专横独断，一场政变宣告了你执政的终结"); return;
        }
        if (stats.noble <= nMin)
        {
            if (TryUseDeathImmunity(-3)) return; // -3 表示贵族下限
            TriggerEnding("灵", "你毫不遮掩对贵族的恶劣态度，一位贵族豢养的死士当庭刺死了你"); return;
        }
        if (stats.noble >= nMax)
        {
            if (TryUseDeathImmunity(3)) return; // 3 表示贵族上限
            TriggerEnding("平", "在你治下贵族逐渐掌握朝中大权，你大权旁落，在宫墙之内了却残生"); return;
        }
        if (stats.scholar <= sMin)
        {
            if (TryUseDeathImmunity(-2)) return; // -2 表示卿士下限
            TriggerEnding("幽", "你并不在意卿士们，一些失意士人起兵作乱，你也在这场叛乱中被砍去头颅"); return;
        }
        if (stats.scholar >= sMax)
        {
            if (TryUseDeathImmunity(2)) return; // 2 表示卿士上限
            TriggerEnding("废", "你将权力越来越多的让渡给士族，一家大族逼迫你禅位，你无奈顺从"); return;
        }
        if (stats.foreign <= fMin)
        {
            if (TryUseDeathImmunity(-4)) return; // -4 表示外臣下限
            TriggerEnding("殇", "你听不进外臣的劝谏，一些失望的外臣找到大国发兵来犯，你死于战乱之中"); return;
        }
        if (stats.foreign >= fMax)
        {
            if (TryUseDeathImmunity(4)) return; // 4 表示外臣上限
            TriggerEnding("纣", "你执政依赖外臣，本国利益被逐渐掏空，最后沦为了大国的傀儡"); return;
        }
        if (stats.people <= pMin)
        {
            if (TryUseDeathImmunity(-5)) return; // -5 表示庶人下限
            TriggerEnding("厉", "你的朝堂横征暴敛，国人不喜，一场国人暴动将你驱逐出了国家"); return;
        }
        if (stats.people >= pMax)
        {
            if (TryUseDeathImmunity(5)) return; // 5 表示庶人上限
            TriggerEnding("携", "你的朝堂软弱无力，国人拒不上税，在一次暴动后庶人们一脚踹开了你"); return;
        }
    }

    // 免死道具判定与消耗
    private bool TryUseDeathImmunity(int deathType)
    {
        if (stats.policyBag != null)
        {
            foreach (var item in stats.policyBag)
            {
                if (item.type == 2 && item.usageCount != 0 && item.deathImmunity != null && item.deathImmunity.Contains(deathType))
                {
                    // 弹窗询问玩家是否使用免死道具
                    StartCoroutine(AskDeathImmunityChoice(item, deathType));
                    return true; // 暂停结局判定，等待玩家选择
                }
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

        // 游戏结束时，将当前年数加到累计货币中
        stats.currency += year;
        Debug.Log($"[GameControl] 本局存活 {year} 年，累计货币: {stats.currency}");

        // 游戏结束时，先生成新一轮商店道具
        if (PolicyManager.Instance != null)
            PolicyManager.Instance.GenerateShopItems(5);

        // 展示结局UI（需要 UIManager 实现 ShowEndingPanel）
        if (UIManager.Instance != null)
            UIManager.Instance.ShowEndingPanel(endingId, endingDescription, year);
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
        isGameStarting = false; // 重置游戏启动标志
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
            // 重开时显示 jinYan
            if (UIManager.Instance.jinYan != null)
                UIManager.Instance.jinYan.SetActive(true);
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
    
    public void continueGame()
    {
        if(GamePaused)
        {
            Debug.Log("[GameControl] continueGame - 直接恢复游戏，不播放动画");
            GamePaused = false;
            GameOver = false;
            
            // 菜单面板的隐藏由按钮自己的 SetActive 控制
            // 这里只需要取消暂停状态即可
            
            return;
        }
    }

    public void BackToStartMenu()
    {
        // 重置游戏启动标志
        isGameStarting = false;
        
        if (CanvasMove.Instance != null)
        {
            CanvasMove.Instance.BackToStart();
        }
        Debug.Log("[GameControl] 返回主菜单完成");
    }

    // ===== 暂停游戏并返回主菜单（不重置进度）=====

    // 点击菜单按钮 -> 只暂停游戏并保存快照（不返回主菜单）
    public void PauseGameForMenu()
    {
        GamePaused = true;

        // 保存当前事件状态（快照）
        if (UIManager.Instance != null)
        {
            if (objectsAboutEvent != null)
                objectsAboutEvent.SetActive(false);

            pausedEventId = UIManager.Instance.GetCurrentEventId();
            pausedSentenceIndex = UIManager.Instance.GetCurrentSentenceIndex();
            Debug.Log($"[GameControl] 游戏暂停，保存事件快照: {pausedEventId}, 句子索引: {pausedSentenceIndex}");

            // 停止当前等待显示事件的协程（但不 StopAllCoroutines，避免意外停止其他协程）
            if (currentWaitCoroutine != null)
            {
                StopCoroutine(currentWaitCoroutine);
                currentWaitCoroutine = null;
            }
            waitingForNextTurn = false;

         }
        else
        {
            Debug.Log("[GameControl] 游戏暂停（UI 管理器不可用）");
        }

        // 此函数不切换到主菜单，仅保持暂停状态，等待玩家在菜单中选择退出或继续
    }

    // 菜单中点击退出 -> 真正返回主菜单（保留 paused 快照以便可能恢复）
    public void ExitToMainMenuFromPause()
    {
        // 保证处于暂停状态
        GamePaused = true;

        if (CanvasMove.Instance != null)
        {
            CanvasMove.Instance.BackToStart();
        }

        Debug.Log("[GameControl] 从暂停状态返回主菜单完成");
    }

    // ===== 退出游戏 =====
    public void QuitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }

    // 获取玩家货币（累计的总货币）
    public int GetCurrency()
    {
        return stats != null ? stats.currency : 0;
    }

    // 扣除货币（购买道具时）
    public void SpendCurrency(int amount)
    {
        if (stats == null) return;
        stats.currency -= amount;
        if (stats.currency < 0) stats.currency = 0;
        Debug.Log($"[GameControl] 花费 {amount} 货币，剩余 {stats.currency} 货币");
        
        // 更新UI显示
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCurrencyDisplay();
        }
    }
}
