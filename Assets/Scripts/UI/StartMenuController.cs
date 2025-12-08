using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 开始界面按钮控制示例
/// 展示如何集成新手教程系统
/// </summary>
public class StartMenuController : MonoBehaviour
{
    [Header("按钮引用")]
    public Button startGameButton;      // 开始游戏按钮
    public Button tutorialButton;       // 游戏说明按钮
    public Button quitGameButton;       // 退出游戏按钮
    
    [Header("其他引用")]
    public TutorialManager tutorialManager;  // 新手教程管理器
    public GameObject startMenuPanel;        // 开始界面面板
    
    private void Start()
    {
        // 绑定按钮事件
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }
        
        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(OnTutorialClicked);
        }
        
        if (quitGameButton != null)
        {
            quitGameButton.onClick.AddListener(OnQuitGameClicked);
        }
    }
    
    /// <summary>
    /// 点击"开始游戏"按钮
    /// </summary>
    private void OnStartGameClicked()
    {
        Debug.Log("[StartMenu] 点击开始游戏");
        
        if (tutorialManager != null)
        {
            // 检查是否是第一次游戏，如果是则先播放教程
            tutorialManager.CheckAndPlayTutorialBeforeGame(StartGame);
        }
        else
        {
            // 如果没有教程管理器，直接开始游戏
            StartGame();
        }
    }
    
    /// <summary>
    /// 实际开始游戏的逻辑（触发淡入动画）
    /// </summary>
    private void StartGame()
    {
        Debug.Log("[StartMenu] 触发淡入动画");
        
        // 隐藏开始界面
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }
        
        // 触发淡入动画（CanvasMove 会在动画达到阈值时调用 GameControl.StartGame）
        // 不要在这里调用 GameControl.StartGame()，否则会导致事件被提前抽取
        if (CanvasMove.Instance != null)
        {
            CanvasMove.Instance.StartGame();
        }
        else
        {
            Debug.LogError("[StartMenu] CanvasMove.Instance 为 null，无法启动游戏动画");
        }
    }
    
    /// <summary>
    /// 点击"游戏说明"按钮
    /// </summary>
    private void OnTutorialClicked()
    {
        Debug.Log("[StartMenu] 点击游戏说明");
        
        if (tutorialManager != null)
        {
            // 直接播放教程，不管是否已经看过
            tutorialManager.PlayTutorial();
        }
        else
        {
            Debug.LogWarning("[StartMenu] TutorialManager 未绑定");
        }
    }
    
    /// <summary>
    /// 点击"退出游戏"按钮
    /// </summary>
    private void OnQuitGameClicked()
    {
        Debug.Log("[StartMenu] 点击退出游戏");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
