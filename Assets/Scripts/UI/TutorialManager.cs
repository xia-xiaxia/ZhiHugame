using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 新手教程管理器
/// 负责显示和控制新手教程图片播放
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    
    [Header("教程图片")]
    public List<Sprite> tutorialImages = new List<Sprite>();  // 教程图片列表（4张）
    private Image defaltImage;                             // 默认图片（空）
    
    [Header("UI组件")]
    public GameObject tutorialPanel;        // 教程面板（整个教程UI的根对象）
    public Image tutorialImageDisplay;      // 显示教程图片的Image组件
    public CanvasGroup tutorialCanvasGroup; // 用于淡入淡出效果
    public Text pageIndicator;              // 页码指示器（可选，如 "1/4"）
    public Button skipButton;               // 跳过按钮（可选）
    
    [Header("动画设置")]
    public float fadeInDuration = 0.3f;     // 淡入时长
    public float fadeOutDuration = 0.3f;    // 淡出时长
    public float imageSwitchDuration = 0.2f; // 图片切换动画时长
    
    [Header("其他设置")]
    public StatModel stats;                 // 关联的StatModel
    public GameObject startMenuPanel;       // 开始界面面板（教程结束后返回）
    public GameObject OptionMenuPanel;      // 选项菜单面板
    public bool isPlayingTutorial = false;   // 现在是否在播放教程
    
    private int currentImageIndex = 0;      // 当前显示的图片索引
    private bool isPlaying = false;         // 是否正在播放教程
    private bool isAnimating = false;       // 是否正在播放动画（防止快速点击）
    private System.Action onTutorialComplete; // 教程完成后的回调
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        isPlayingTutorial = false;
    }
    
    private void Start()
    {
        // 初始化：隐藏教程面板
        if (tutorialPanel != null)
        {
            tutorialImageDisplay.sprite = tutorialImages.Count > 0 ? tutorialImages[0] : null;
            tutorialPanel.SetActive(false);
        }
        
        // 绑定跳过按钮
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTutorial);
        }
    }
    
    private void Update()
    {
        // 如果正在播放教程，检测鼠标点击或空格键
        if (isPlaying && !isAnimating)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                NextImage();
            }
        }
    }
    
    /// <summary>
    /// 检查是否是第一次玩游戏，如果是则播放教程
    /// 由"开始游戏"按钮调用，传入开始游戏的回调函数
    /// </summary>
    public void CheckAndPlayTutorialBeforeGame(System.Action onComplete)
    {
        if (stats == null)
        {
            Debug.LogWarning("[TutorialManager] StatModel 未绑定，直接开始游戏");
            onComplete?.Invoke();
            return;
        }
        
        // 如果没有看过教程，先播放教程，完成后再调用回调
        if (!stats.hasSeenTutorial)
        {
            Debug.Log("[TutorialManager] 检测到第一次游戏，播放新手教程");
            PlayTutorialWithCallback(onComplete);
        }
        else
        {
            // 已经看过教程，直接执行回调（开始游戏）
            Debug.Log("[TutorialManager] 已看过教程，直接开始游戏");
            onComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// 播放新手教程（带回调，用于第一次游戏）
    /// </summary>
    private void PlayTutorialWithCallback(System.Action onComplete)
    {
        onTutorialComplete = onComplete;
        PlayTutorial();
    }
    
    /// <summary>
    /// 播放新手教程（公开方法，可由"查看教程"按钮直接调用）
    /// </summary>
    public void PlayTutorial()
    {
        if (tutorialImages == null || tutorialImages.Count == 0)
        {
            Debug.LogError("[TutorialManager] 没有设置教程图片！");
            return;
        }
        
        Debug.Log("[TutorialManager] 开始播放新手教程");
        
        // 隐藏开始界面和选项菜单
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }
        if (OptionMenuPanel != null)
        {
            OptionMenuPanel.SetActive(false);
        }
        
        // 重置状态
        currentImageIndex = 0;
        isPlayingTutorial = true;
        isPlaying = true;
        tutorialImageDisplay.sprite = tutorialImages.Count > 0 ? tutorialImages[0] : null;
        
        // 显示教程面板
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
        
        Debug.Log("[TutorialManager] 教程标志设置完成，isPlayingTutorial = true");
        
        // 开始播放
        StartCoroutine(ShowTutorialSequence());
    }
    
    /// <summary>
    /// 教程播放序列
    /// </summary>
    private IEnumerator ShowTutorialSequence()
    {
        // 淡入效果
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 0f;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                tutorialCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            tutorialCanvasGroup.alpha = 1f;
        }
        
        // 显示第一张图片
        ShowImage(currentImageIndex);
        
        // 等待用户点击（在 Update 中处理）
    }
    
    /// <summary>
    /// 显示指定索引的图片
    /// </summary>
    private void ShowImage(int index)
    {
        if (index < 0 || index >= tutorialImages.Count)
        {
            Debug.LogError($"[TutorialManager] 图片索引越界: {index}");
            return;
        }
        
        if (tutorialImageDisplay != null)
        {
            tutorialImageDisplay.sprite = tutorialImages[index];
        }
        
        // 更新页码指示器
        if (pageIndicator != null)
        {
            pageIndicator.text = $"{index + 1}/{tutorialImages.Count}";
        }
        
        Debug.Log($"[TutorialManager] 显示第 {index + 1}/{tutorialImages.Count} 张教程图片");
    }
    
    /// <summary>
    /// 下一张图片
    /// </summary>
    public void NextImage()
    {
        if (!isPlaying || isAnimating) return;
        
        currentImageIndex++;
        
        // 如果已经是最后一张，结束教程
        if (currentImageIndex >= tutorialImages.Count)
        {
            EndTutorial();
        }
        else
        {
            // 切换到下一张图片（带动画效果）
            StartCoroutine(SwitchImageWithAnimation());
        }
    }
    
    /// <summary>
    /// 上一张图片（可选功能）
    /// </summary>
    public void PreviousImage()
    {
        if (!isPlaying || isAnimating || currentImageIndex <= 0) return;
        
        currentImageIndex--;
        StartCoroutine(SwitchImageWithAnimation());
    }
    
    /// <summary>
    /// 图片切换动画
    /// </summary>
    private IEnumerator SwitchImageWithAnimation()
    {
        isAnimating = true;
        
        // 淡出当前图片
        if (tutorialCanvasGroup != null)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < imageSwitchDuration)
            {
                elapsedTime += Time.deltaTime;
                tutorialCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / imageSwitchDuration);
                yield return null;
            }
            
            tutorialCanvasGroup.alpha = 0f;
        }
        
        // 切换图片
        ShowImage(currentImageIndex);
        
        // 淡入新图片
        if (tutorialCanvasGroup != null)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < imageSwitchDuration)
            {
                elapsedTime += Time.deltaTime;
                tutorialCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / imageSwitchDuration);
                yield return null;
            }
            
            tutorialCanvasGroup.alpha = 1f;
        }
        
        isAnimating = false;
    }
    
    /// <summary>
    /// 跳过教程
    /// </summary>
    public void SkipTutorial()
    {
        if (!isPlaying) return;
        
        Debug.Log("[TutorialManager] 跳过教程");
        
        // 停止所有动画
        StopAllCoroutines();
        
        EndTutorial();
    }
    
    /// <summary>
    /// 结束教程
    /// </summary>
    private void EndTutorial()
    {
        Debug.Log("[TutorialManager] 教程结束");
        
        isPlaying = false;
        
        // 标记已看过教程
        if (stats != null && !stats.hasSeenTutorial)
        {
            stats.hasSeenTutorial = true;
            
            // 保存到存档
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }
        }
        
        // 淡出教程面板
        StartCoroutine(FadeOutTutorial());
    }
    
    /// <summary>
    /// 淡出教程面板
    /// </summary>
    private IEnumerator FadeOutTutorial()
    {
        // 淡出效果
        if (tutorialCanvasGroup != null)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                tutorialCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                yield return null;
            }
            
            tutorialCanvasGroup.alpha = 0f;
        }
        
        // 隐藏教程面板
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        // 先清除教程标志
        isPlayingTutorial = false;
        Debug.Log("[TutorialManager] 教程标志已清除，isPlayingTutorial = false");
        
        // 如果有回调函数（第一次游戏），直接执行回调（让 CanvasMove 继续）
        if (onTutorialComplete != null)
        {
            Debug.Log("[TutorialManager] 教程完成，执行回调继续游戏动画");
            System.Action callback = onTutorialComplete;
            onTutorialComplete = null; // 清空回调
            callback.Invoke();
        }
        else if(GameLifecycleManager.Instance != null && GameLifecycleManager.Instance.isGameing)
        {
            // 正在游戏中播放教程，结束后保持在游戏界面
            GameControl.Instance.continueGame();
            yield return null;
        }
        else
        {
            // 没有回调（从"游戏说明"进入），返回开始界面
            Debug.Log("[TutorialManager] 教程完成，返回开始界面");
            yield return StartCoroutine(ShowStartMenu());
        }
    }
    
    /// <summary>
    /// 显示开始界面（带淡入效果）
    /// </summary>
    private IEnumerator ShowStartMenu()
    {
        OptionMenuPanel?.SetActive(true);
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(true);
            
            // 添加淡入效果
            CanvasGroup menuCanvasGroup = startMenuPanel.GetComponent<CanvasGroup>();
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 0f;
                float elapsedTime = 0f;
                
                while (elapsedTime < fadeInDuration)
                {
                    elapsedTime += Time.deltaTime;
                    menuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                    yield return null;
                }
                
                menuCanvasGroup.alpha = 1f;
            }
        }
    }
    
    /// <summary>
    /// 重置教程状态（用于测试）
    /// </summary>
    public void ResetTutorialStatus()
    {
        if (stats != null)
        {
            stats.hasSeenTutorial = false;
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }
            
            Debug.Log("[TutorialManager] 教程状态已重置");
        }
    }
}
