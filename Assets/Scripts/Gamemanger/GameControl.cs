using UnityEngine;

/// <summary>
/// 游戏控制器（协调器）：统一管理各个子系统
/// 重构后的职责：作为外部接口层，委托给各个专门的管理器处理
/// </summary>
public class GameControl : MonoBehaviour
{
    public static GameControl Instance;

    [Header("数据引用")]
    public StatModel stats;
    public GameObject objectsAboutEvent;

    [Header("配置")]
    public int yearLimit = 100;
    public bool useDynamicThreshold = true;

    // 快速访问属性（向后兼容）
    public bool GameOver => GameLifecycleManager.Instance?.GameOver ?? false;
    public bool GamePaused => GameLifecycleManager.Instance?.GamePaused ?? false;
    public int year
    {
        get => TurnManager.Instance?.year ?? 1;
        set { if (TurnManager.Instance != null) TurnManager.Instance.year = value; }
    }

    void Awake()
    {
        Instance = this;
        InitializeManagers();
    }

    // ===== 初始化所有管理器 =====
    private void InitializeManagers()
    {
        // 确保所有管理器都存在
        if (GameLifecycleManager.Instance == null)
        {
            GameObject lifecycleObj = new GameObject("GameLifecycleManager");
            lifecycleObj.transform.SetParent(transform);
            var lifecycle = lifecycleObj.AddComponent<GameLifecycleManager>();
            lifecycle.stats = stats;
            lifecycle.objectsAboutEvent = objectsAboutEvent;
        }

        if (TurnManager.Instance == null)
        {
            GameObject turnObj = new GameObject("TurnManager");
            turnObj.transform.SetParent(transform);
            var turnMgr = turnObj.AddComponent<TurnManager>();
            turnMgr.stats = stats;
            turnMgr.objectsAboutEvent = objectsAboutEvent;
        }

        if (EndingManager.Instance == null)
        {
            GameObject endingObj = new GameObject("EndingManager");
            endingObj.transform.SetParent(transform);
            var endingMgr = endingObj.AddComponent<EndingManager>();
            endingMgr.stats = stats;
            endingMgr.yearLimit = yearLimit;
            endingMgr.useDynamicThreshold = useDynamicThreshold;
        }

        if (PolicyInventory.Instance == null)
        {
            GameObject policyObj = new GameObject("PolicyInventory");
            policyObj.transform.SetParent(transform);
            var policyInv = policyObj.AddComponent<PolicyInventory>();
            policyInv.stats = stats;
        }

        if (CurrencyManager.Instance == null)
        {
            GameObject currencyObj = new GameObject("CurrencyManager");
            currencyObj.transform.SetParent(transform);
            var currencyMgr = currencyObj.AddComponent<CurrencyManager>();
            currencyMgr.stats = stats;
        }
    }

    // ===== 游戏生命周期（委托给 GameLifecycleManager）=====
    
    public void OnStartGameButtonClicked()
    {
        GameLifecycleManager.Instance?.OnStartGameButtonClicked();
    }

    public void StartGame()
    {
        GameLifecycleManager.Instance?.StartGame();
    }

    public void RestartGame()
    {
        GameLifecycleManager.Instance?.RestartGame();
    }

    public void RestartGameWithAnimation()
    {
        GameLifecycleManager.Instance?.RestartGameWithAnimation();
    }

    public void BackToMainMenu()
    {
        GameLifecycleManager.Instance?.BackToMainMenu();
    }

    public void PauseGameForMenu()
    {
        GameLifecycleManager.Instance?.PauseGameForMenu();
    }

    public void ExitToMainMenuFromPause()
    {
        GameLifecycleManager.Instance?.ExitToMainMenuFromPause();
    }

    public void continueGame()
    {
        GameLifecycleManager.Instance?.ContinueGame();
    }

    public void BackToStartMenu()
    {
        GameLifecycleManager.Instance?.BackToStartMenu();
    }

    public void QuitGame()
    {
        GameLifecycleManager.Instance?.QuitGame();
    }

    // ===== 回合管理（委托给 TurnManager）=====
    
    public void ProcessNextTurn()
    {
        TurnManager.Instance?.ProcessNextTurn();
    }

    public void RecordEvent(string eventId)
    {
        TurnManager.Instance?.RecordEvent(eventId);
    }

    // ===== 结局管理（委托给 EndingManager）=====
    
    public void OnStatsChanged()
    {
        EndingManager.Instance?.OnStatsChanged();
    }

    public void SaveStatsSnapshot()
    {
        EndingManager.Instance?.SaveStatsSnapshot();
    }

    public void TriggerEnding(string endingId, string endingDescription)
    {
        EndingManager.Instance?.TriggerEnding(endingId, endingDescription);
    }

    public void OnDeathImmunityUse()
    {
        EndingManager.Instance?.OnDeathImmunityUse();
    }

    public void OnDeathImmunityDecline()
    {
        EndingManager.Instance?.OnDeathImmunityDecline();
    }

    // ===== 道具管理（委托给 PolicyInventory）=====
    
    public bool AddPolicy(PolicyItem item)
    {
        return PolicyInventory.Instance?.AddPolicy(item) ?? false;
    }

    public bool RemovePolicy(string id)
    {
        return PolicyInventory.Instance?.RemovePolicy(id) ?? false;
    }

    public void ClearPolicies()
    {
        PolicyInventory.Instance?.ClearPolicies();
    }

    public PolicyItem GetPolicy(string id)
    {
        return PolicyInventory.Instance?.GetPolicy(id);
    }

    // ===== 货币管理（委托给 CurrencyManager）=====
    
    public int GetCurrency()
    {
        return CurrencyManager.Instance?.GetCurrency() ?? 0;
    }

    public void SpendCurrency(int amount)
    {
        CurrencyManager.Instance?.SpendCurrency(amount);
    }
}
