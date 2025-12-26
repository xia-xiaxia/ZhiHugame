using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI管理器（协调器）：统一管理所有UI子系统
/// 重构后的职责：作为外部接口层，委托给各个专门的UI类处理
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("主UI容器")]
    public GameObject jinYan; // 事件显示面板容器
    public StatModel stats;

    [Header("游戏菜单")]
    public Button exitToMenuButton; // 游戏中退出到主菜单按钮

    [Header("道具提示框")]
    public GameObject policyTooltipPanel;
    public TextMeshProUGUI policyTooltipText;

    [Header("道具商店")]
    public int maxRefreshCount = 4;
    public int[] refreshCosts = new int[4] { 5, 10, 20, 50 };
    public Text CurrencyText;   

    // 兼容性字段（用于Inspector引用，会转发到子系统）
    [HideInInspector] public bool isShow;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 启用主UI容器
        if (jinYan != null) jinYan.SetActive(true);

        // 绑定游戏内退出按钮
        if (exitToMenuButton != null)
        {
            exitToMenuButton.onClick.RemoveAllListeners();
            exitToMenuButton.onClick.AddListener(OnExitToMenuClicked);
        }
    }

    // ===== 游戏菜单控制 =====

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void OnPauseGameClicked()
    {
        GameControl.Instance?.PauseGameForMenu();
    }

    /// <summary>
    /// 游戏内退出到主菜单
    /// </summary>
    private void OnExitToMenuClicked()
    {
        GameControl.Instance?.ExitToMainMenuFromPause();
    }

    // ===== 事件显示（委托给 EventDisplayUI）=====

    /// <summary>
    /// 显示事件
    /// </summary>
    public void ShowEvent(string id)
    {
        EventDisplayUI.Instance?.ShowEvent(id);
    }

    /// <summary>
    /// 显示下一句
    /// </summary>
    public void ShowNextSentence()
    {
        // 保留此方法以兼容外部调用
        // EventDisplayUI 内部会处理
    }

    /// <summary>
    /// 清空事件文本
    /// </summary>
    public void ClearText()
    {
        EventDisplayUI.Instance?.ClearText();
    }

    /// <summary>
    /// 隐藏事件选项
    /// </summary>
    public void HideEventOptions()
    {
        EventDisplayUI.Instance?.HideOptions();
    }

    /// <summary>
    /// 获取当前事件ID（用于暂停保存）
    /// </summary>
    public string GetCurrentEventId()
    {
        return EventDisplayUI.Instance?.GetCurrentEventId() ?? "";
    }

    /// <summary>
    /// 获取当前句子索引（用于暂停保存）
    /// </summary>
    public int GetCurrentSentenceIndex()
    {
        return EventDisplayUI.Instance?.GetCurrentSentenceIndex() ?? 0;
    }

    /// <summary>
    /// 恢复事件状态（从暂停恢复）
    /// </summary>
    public void RestoreEventState(string eventId, int sentenceIndex)
    {
        EventDisplayUI.Instance?.RestoreEventState(eventId, sentenceIndex);
    }

    // ===== 数值显示（委托给 StatsDisplayUI）=====

    /// <summary>
    /// 更新数值显示
    /// </summary>
    public void UpdateStatText()
    {
        StatsDisplayUI.Instance?.UpdateStatText();
    }

    /// <summary>
    /// 更新货币显示
    /// </summary>
    public void UpdateCurrencyDisplay()
    {
        PolicyShopUI.Instance?.UpdateCurrencyDisplay();
    }

    // ===== 结局面板（委托给 EndingUI）=====

    /// <summary>
    /// 显示结局面板
    /// </summary>
    public void ShowEndingPanel(string endingId, string description, int survivedYears)
    {
        // EndingUI.Instance?.ShowEnding(endingId, description, survivedYears);
        EndingUI.Instance?.ShowEndingWithAnimation(endingId, description, survivedYears);
    }

    /// <summary>
    /// 隐藏结局面板
    /// </summary>
    public void HideEndingPanel()
    {
        EndingUI.Instance?.HideEnding();
    }

    // ===== 免死道具UI（委托给 DeathImmunityUI）=====

    /// <summary>
    /// 显示免死道具确认弹窗
    /// </summary>
    public void ShowDeathImmunityPrompt(PolicyItem item, int deathType)
    {
        DeathImmunityUI.Instance?.ShowPrompt(item, deathType);
    }

    /// <summary>
    /// 显示免死道具生效提示
    /// </summary>
    public void ShowDeathImmunityMessage(string message)
    {
        DeathImmunityUI.Instance?.ShowMessage(message);
    }

    // ===== 道具菜单（委托给 PolicyMenuUI）=====

    /// <summary>
    /// 显示道具菜单
    /// </summary>
    public void ShowPolicyMenu()
    {
        PolicyMenuUI.Instance?.ShowMenu();
    }

    /// <summary>
    /// 隐藏道具菜单
    /// </summary>
    public void HidePolicyMenu()
    {
        PolicyMenuUI.Instance?.HideMenu();
    }

    // ===== 道具商店（委托给 PolicyShopUI）=====

    /// <summary>
    /// 显示道具商店
    /// </summary>
    public void ShowPolicyShop()
    {
        PolicyShopUI.Instance?.ShowShop();
    }

    /// <summary>
    /// 隐藏道具商店
    /// </summary>
    public void HidePolicyShop()
    {
        PolicyShopUI.Instance?.HideShop();
    }

    /// <summary>
    /// 刷新商店背包显示
    /// </summary>
    public void RefreshShopInventoryDisplay()
    {
        PolicyShopUI.Instance?.RefreshInventoryDisplay();
    }

    // ===== BUFF面板（委托给 BuffUI）=====

    /// <summary>
    /// 显示BUFF面板
    /// </summary>
    public void ShowBuffPanel()
    {
        BuffUI.Instance?.ShowBuffPanel();
    }

    /// <summary>
    /// 隐藏BUFF面板
    /// </summary>
    public void HideBuffPanel()
    {
        BuffUI.Instance?.HideBuffPanel();
    }
}