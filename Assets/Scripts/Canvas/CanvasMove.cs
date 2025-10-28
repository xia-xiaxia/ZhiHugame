using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasMove : MonoBehaviour
{
    public static CanvasMove Instance;
    
    [Header("面板引用")]
    public CanvasGroup startPanelCanvasGroup;  // 开始界面的 CanvasGroup
    public CanvasGroup gamePanelCanvasGroup;   // 游戏界面（jinyan）的 CanvasGroup
    
    [Header("淡入淡出设置")]
    [Tooltip("淡出时间（秒）")]
    public float fadeOutDuration = 0.8f;
    
    [Tooltip("淡入时间（秒）")]
    public float fadeInDuration = 0.8f;
    
    [Tooltip("完全黑屏的持续时间（秒）")]
    public float blackScreenDuration = 0.3f;
    
    [Tooltip("游戏界面达到多少亮度时开始事件（0-1）")]
    public float eventStartThreshold = 0.8f;

    public bool isReady = false;  // 相机是否准备好
    private bool isTransitioning = false;  // 是否正在进行场景切换动画
    private Coroutine currentTransitionCoroutine = null;  // 当前的转场协程

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        isReady = false;
    }

    void Start()
    {
        // 初始化：开始界面完全显示，游戏界面完全隐藏
        if (startPanelCanvasGroup != null)
        {
            startPanelCanvasGroup.alpha = 1f;
            startPanelCanvasGroup.interactable = true;
            startPanelCanvasGroup.blocksRaycasts = true;
        }
        
        if (gamePanelCanvasGroup != null)
        {
            gamePanelCanvasGroup.alpha = 0f;
            gamePanelCanvasGroup.interactable = false;
            gamePanelCanvasGroup.blocksRaycasts = false;
            gamePanelCanvasGroup.gameObject.SetActive(false);
        }
        
        isReady = false;
        
        // 播放开始界面音乐
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMenuMusic();
        }
        
        Debug.Log("[CanvasMove] 初始化完成");
    }

    /// <summary>
    /// 开始游戏：开始界面淡出 → 黑屏 → 游戏界面淡入
    /// </summary>
    public void StartGame()
    {
        // 如果正在进行转场动画，忽略重复调用
        if (isTransitioning)
        {
            Debug.Log("[CanvasMove] 转场动画正在进行中，忽略重复的 StartGame 调用");
            return;
        }
        
        // 停止之前的协程（如果有）
        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
        }
        
        currentTransitionCoroutine = StartCoroutine(FadeToGame());
    }
    
    /// <summary>
    /// 返回主菜单：游戏界面淡出 → 黑屏 → 开始界面淡入
    /// </summary>
    public void BackToStart()
    {
        // 如果正在进行转场动画，忽略重复调用
        if (isTransitioning)
        {
            Debug.Log("[CanvasMove] 转场动画正在进行中，忽略重复的 BackToStart 调用");
            return;
        }
        
        // 停止之前的协程（如果有）
        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
        }
        
        currentTransitionCoroutine = StartCoroutine(FadeToStart());
    }

    /// <summary>
    /// 开始游戏的淡入淡出流程
    /// </summary>
    IEnumerator FadeToGame()
    {
        isTransitioning = true;  // 标记转场开始
        Debug.Log("[CanvasMove] 开始游戏淡入淡出动画");
        isReady = false;
        
        // 确保游戏界面对象是激活的（但透明）
        if (gamePanelCanvasGroup != null)
        {
            gamePanelCanvasGroup.gameObject.SetActive(true);
            gamePanelCanvasGroup.alpha = 0f;
            gamePanelCanvasGroup.interactable = false;
            gamePanelCanvasGroup.blocksRaycasts = false;
        }

        // 阶段1：开始界面淡出
        yield return StartCoroutine(FadeCanvasGroup(startPanelCanvasGroup, 1f, 0f, fadeOutDuration));
        
        // 禁用开始界面的交互
        if (startPanelCanvasGroup != null)
        {
            startPanelCanvasGroup.interactable = false;
            startPanelCanvasGroup.blocksRaycasts = false;
        }

        // 阶段2：黑屏持续一段时间
        yield return new WaitForSeconds(blackScreenDuration);

        // 播放游戏音乐
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayGameMusic();
        }

        // 阶段3：游戏界面淡入
        bool eventStarted = false;
        float timer = 0f;
        
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeInDuration;
            float alpha = Mathf.Lerp(0f, 1f, t);
            
            if (gamePanelCanvasGroup != null)
            {
                gamePanelCanvasGroup.alpha = alpha;
            }
            
            // 当亮度达到阈值时，通知游戏可以开始事件
            if (!eventStarted && alpha >= eventStartThreshold)
            {
                eventStarted = true;
                isReady = true;
                Debug.Log($"[CanvasMove] 游戏界面亮度达到 {eventStartThreshold}，可以开始事件");
                
                // 通知 GameControl 可以开始游戏
                if (GameControl.Instance != null)
                {
                    GameControl.Instance.StartGame();
                }
            }
            
            yield return null;
        }

        // 确保最终状态
        if (gamePanelCanvasGroup != null)
        {
            gamePanelCanvasGroup.alpha = 1f;
            gamePanelCanvasGroup.interactable = true;
            gamePanelCanvasGroup.blocksRaycasts = true;
        }
        
        // 完全隐藏开始界面
        if (startPanelCanvasGroup != null)
        {
            startPanelCanvasGroup.gameObject.SetActive(false);
        }
        
        isReady = true;
        isTransitioning = false;  // 标记转场结束
        currentTransitionCoroutine = null;
        Debug.Log("[CanvasMove] 游戏界面淡入完成");
    }

    /// <summary>
    /// 返回主菜单的淡入淡出流程
    /// </summary>
    IEnumerator FadeToStart()
    {
        isTransitioning = true;  // 标记转场开始
        Debug.Log("[CanvasMove] 返回主菜单淡入淡出动画");
        isReady = false;
        
        // 停止所有游戏相关的协程
        if (GameControl.Instance != null)
        {
            GameControl.Instance.StopAllCoroutines();
        }
        
        // 确保开始界面对象是激活的（但透明）
        if (startPanelCanvasGroup != null)
        {
            startPanelCanvasGroup.gameObject.SetActive(true);
            startPanelCanvasGroup.alpha = 0f;
            startPanelCanvasGroup.interactable = false;
            startPanelCanvasGroup.blocksRaycasts = false;
        }

        // 阶段1：游戏界面淡出
        yield return StartCoroutine(FadeCanvasGroup(gamePanelCanvasGroup, 1f, 0f, fadeOutDuration));
        
        // 禁用游戏界面的交互
        if (gamePanelCanvasGroup != null)
        {
            gamePanelCanvasGroup.interactable = false;
            gamePanelCanvasGroup.blocksRaycasts = false;
        }

        // 阶段2：黑屏持续一段时间
        yield return new WaitForSeconds(blackScreenDuration);

        // 播放菜单音乐
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMenuMusic();
        }

        // 阶段3：开始界面淡入
        yield return StartCoroutine(FadeCanvasGroup(startPanelCanvasGroup, 0f, 1f, fadeInDuration));

        // 确保最终状态
        if (startPanelCanvasGroup != null)
        {
            startPanelCanvasGroup.alpha = 1f;
            startPanelCanvasGroup.interactable = true;
            startPanelCanvasGroup.blocksRaycasts = true;
        }
        
        // 完全隐藏游戏界面
        if (gamePanelCanvasGroup != null)
        {
            gamePanelCanvasGroup.gameObject.SetActive(false);
        }
        
        isReady = false;
        isTransitioning = false;  // 标记转场结束
        currentTransitionCoroutine = null;
        Debug.Log("[CanvasMove] 开始界面淡入完成");
    }

    /// <summary>
    /// 淡入淡出 CanvasGroup 的通用方法
    /// </summary>
    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("[CanvasMove] CanvasGroup 为空，跳过淡入淡出");
            yield break;
        }

        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            
            // 使用平滑插值
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);
            
            yield return null;
        }

        // 确保最终值精确
        canvasGroup.alpha = endAlpha;
    }
}