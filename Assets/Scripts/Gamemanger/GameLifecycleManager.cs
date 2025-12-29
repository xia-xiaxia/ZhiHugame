using UnityEngine;
using System.Collections;

/// <summary>
/// 游戏生命周期管理器：负责游戏启动、暂停、恢复、重开等生命周期相关逻辑
/// </summary>
public class GameLifecycleManager : MonoBehaviour
{
    public static GameLifecycleManager Instance;

    [Header("引用")]
    public StatModel stats;
    public GameObject objectsAboutEvent;

    [Header("状态标志")]
    public bool GameOver = false;
    public bool GamePaused = false;
    private bool isGameStarting = false;

    [Header("暂停快照")]
    private string pausedEventId = null;
    private int pausedSentenceIndex = 0;

    void Awake()
    {
        Instance = this;
    }

    // ===== 开始游戏按钮调用 =====
    public void OnStartGameButtonClicked()
    {
        Debug.Log("[GameLifecycle] 开始游戏按钮被点击");
        
        isGameStarting = false;
        
        // 从暂停恢复
        if (GamePaused)
        {
            Debug.Log("[GameLifecycle] 从暂停状态恢复游戏");
            GamePaused = false;
            GameOver = false;
            
            if (CanvasMove.Instance != null)
            {
                CanvasMove.Instance.StartGame();
            }
            return;
        }
        
        // 新游戏初始化
        InitializeNewGame();
        
        // 触发场景切换
        if (CanvasMove.Instance != null)
        {
            CanvasMove.Instance.StartGame();
        }
    }

    // ===== 初始化新游戏 =====
    private void InitializeNewGame()
    {
        Debug.Log("[GameLifecycle] 初始化新游戏数据");
        
        GameOver = false;
        GamePaused = false;
        TurnManager.Instance.year = 1;
        
        // 重置数值到当前上下限的一半（保留货币、道具、BUFF、天赋与其阈值影响）
        int kingMid = (stats.kingMin + stats.kingMax) / 2;
        int nobleMid = (stats.nobleMin + stats.nobleMax) / 2;
        int scholarMid = (stats.scholarMin + stats.scholarMax) / 2;
        int foreignMid = (stats.foreignMin + stats.foreignMax) / 2;
        int peopleMid = (stats.peopleMin + stats.peopleMax) / 2;

        stats.king = kingMid;
        stats.noble = nobleMid;
        stats.scholar = scholarMid;
        stats.foreign = foreignMid;
        stats.people = peopleMid;
        
        Debug.Log($"[GameLifecycle] 保留道具数量: {stats.policyBag.Count}, BUFF数量: {stats.buffBag.Count}, 已激活天赋: {stats.activatedTalents.Count}");
        
        // 清除暂停快照
        pausedEventId = null;
        pausedSentenceIndex = 0;

        if (objectsAboutEvent != null)
            objectsAboutEvent.SetActive(false);
    }

    // ===== 由 CanvasMove 调用：开始游戏逻辑 =====
    public void StartGame()
    {
        if (isGameStarting)
        {
            Debug.Log("[GameLifecycle] StartGame 已在执行中，忽略重复调用");
            return;
        }
        if(TutorialManager.Instance != null && TutorialManager.Instance.isPlayingTutorial)
        {
            Debug.Log("[GameLifecycle] 教程正在播放中，延迟启动游戏");
            return;
        }
        isGameStarting = true;
        Debug.Log("[GameLifecycle] 开始游戏（由CanvasMove调用）");
        
        if (objectsAboutEvent != null)
        {
            objectsAboutEvent.SetActive(false);
        }
        
        // 从暂停恢复
        if (!string.IsNullOrEmpty(pausedEventId))
        {
            StartCoroutine(RestoreFromPause());
            return;
        }
        
        // 新游戏
        StartCoroutine(StartNewGame());
    }

    // ===== 恢复暂停的游戏 =====
    private IEnumerator RestoreFromPause()
    {
        Debug.Log($"[GameLifecycle] 恢复暂停的事件: {pausedEventId}, 句子索引: {pausedSentenceIndex}");
        
        if (UIManager.Instance?.jinYan != null)
        {
            UIManager.Instance.jinYan.SetActive(true);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (objectsAboutEvent != null)
        {
            objectsAboutEvent.SetActive(true);
        }
        
        UIManager.Instance?.RestoreEventState(pausedEventId, pausedSentenceIndex);
        
        pausedEventId = null;
        pausedSentenceIndex = 0;
        isGameStarting = false;
    }

    // ===== 开始新游戏 =====
    private IEnumerator StartNewGame()
    {
        Debug.Log("[GameLifecycle] 新游戏，显示第一个事件");
        
        if (UIManager.Instance?.jinYan != null)
        {
            UIManager.Instance.jinYan.SetActive(true);
        }

        //全局统计的初始化
        GameControl.Instance.gameStatistics.Restart();
        
        UIManager.Instance?.UpdateStatText();
        UIManager.Instance?.UpdateCurrencyDisplay();
        
        yield return new WaitForSeconds(0.5f);

        Debug.Log("[GameLifecycle] 淡入动画完成，开始显示第一个事件");
        TurnManager.Instance?.ProcessNextTurn();
        
        isGameStarting = false;
    }

    // ===== 重开游戏 =====
    public void RestartGame()
    {
        if (PolicyManager.Instance != null)
            PolicyManager.Instance.GenerateShopItems(stats.policyShopCount);

        //全局统计的初始化
        GameControl.Instance.gameStatistics.Restart();

        //Buff清除
        BuffManager.Instance.ClearAllBuffs();

        GameOver = false;
        GamePaused = false;
        TurnManager.Instance.ResetTurn();
        EndingManager.Instance.Reset();
        
        pausedEventId = null;
        pausedSentenceIndex = 0;
        isGameStarting = false;

        UIManager.Instance?.HideEndingPanel();
        UIManager.Instance?.UpdateStatText();
        
        RefreshAllStatFilledImages();

        EventManager.Instance?.ReloadAllEventsForRestart();
        EventManager.Instance?.OnRestartCleanup();

        // 重置角色管理器，清除上一局的动画
        CharacterManager.Instance?.ResetCharacterManager();
        
        // 重置事件显示UI，清除isFirstShow标志
        EventDisplayUI.Instance?.ResetEventDisplayUI();

        Debug.Log("[GameLifecycle] 重开游戏准备完成");
    }

    // ===== 刷新所有数值填充图片 =====
    private void RefreshAllStatFilledImages()
    {
        StatEffectController[] controllers = FindObjectsOfType<StatEffectController>();
        
        if (controllers.Length > 0)
        {
            Debug.Log($"[GameLifecycle] 找到 {controllers.Length} 个 StatEffectController，开始刷新填充图片");
            foreach (var controller in controllers)
            {
                controller.RefreshFilledImage();
            }
        }
    }

    // ===== 重开游戏并播放动画 =====
    public void RestartGameWithAnimation()
    {
        RestartGame();
        
        UIManager.Instance?.HidePolicyShop();
        MusicManager.Instance?.PlayGameMusic();
        
        StartCoroutine(RestartGameAnimationCoroutine());
    }

    private IEnumerator RestartGameAnimationCoroutine()
    {
        // 确保jinYan显示
        if (UIManager.Instance?.jinYan != null)
        {
            UIManager.Instance.jinYan.SetActive(true);
        }
        
        if (objectsAboutEvent != null)
        {
            objectsAboutEvent.SetActive(false);
        }
        
        yield return new WaitForSeconds(0.3f);
        
        if (objectsAboutEvent != null)
        {
            objectsAboutEvent.SetActive(true);
        }
        
        RefreshAllStatFilledImages();
        
        // 确保isGameStarting为false，允许ProcessNextTurn执行
        isGameStarting = false;
        
        TurnManager.Instance?.ProcessNextTurn();
        
        Debug.Log("[GameLifecycle] 重开游戏动画完成");
    }

    // ===== 回到主菜单 =====
    public void BackToMainMenu()
    {
        RestartGame();
        UIManager.Instance?.HidePolicyShop();
        CanvasMove.Instance?.BackToStart();
        Debug.Log("[GameLifecycle] 回到主菜单");
    }

    // ===== 暂停游戏 =====
    public void PauseGameForMenu()
    {
        GamePaused = true;

        if (UIManager.Instance != null)
        {
            if (objectsAboutEvent != null)
                objectsAboutEvent.SetActive(false);

            pausedEventId = UIManager.Instance.GetCurrentEventId();
            pausedSentenceIndex = UIManager.Instance.GetCurrentSentenceIndex();
            Debug.Log($"[GameLifecycle] 游戏暂停，保存事件快照: {pausedEventId}, 句子索引: {pausedSentenceIndex}");

            TurnManager.Instance?.StopWaitingCoroutine();
        }
    }

    // ===== 从暂停返回主菜单 =====
    public void ExitToMainMenuFromPause()
    {
        GamePaused = true;
        CanvasMove.Instance?.BackToStart();
        Debug.Log("[GameLifecycle] 从暂停状态返回主菜单完成");
    }

    // ===== 继续游戏（从菜单恢复）=====
    public void ContinueGame()
    {
        if (GamePaused)
        {
            Debug.Log("[GameLifecycle] 继续游戏，不播放动画");
            GamePaused = false;
            GameOver = false;
            
            if (objectsAboutEvent != null)
            {
                objectsAboutEvent.SetActive(true);
            }
        }
    }

    // ===== 返回主菜单 =====
    public void BackToStartMenu()
    {
        isGameStarting = false;
        CanvasMove.Instance?.BackToStart();
        Debug.Log("[GameLifecycle] 返回主菜单完成");
    }

    // ===== 退出游戏 =====
    public void QuitGame()
    {
        Debug.Log("[GameLifecycle] 退出游戏");
        Application.Quit();
    }
    
    // ===== 设置暂停状态（从存档恢复）=====
    public void SetPausedState(string eventId, int sentenceIndex)
    {
        pausedEventId = eventId;
        pausedSentenceIndex = sentenceIndex;
        Debug.Log($"[GameLifecycle] 设置暂停状态: 事件={eventId}, 句子={sentenceIndex}");
    }
}
