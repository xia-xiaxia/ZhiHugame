using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 开屏Logo显示脚本
/// 每次打开游戏时显示Logo，然后淡出并显示开始界面
/// </summary>
public class SplashScreen : MonoBehaviour
{
    [Header("Logo配置")]
    public Image logoImage;              // Logo图片
    public CanvasGroup logoCanvasGroup;  // Logo的CanvasGroup（用于淡入淡出）
    
    [Header("开始界面")]
    public GameObject startMenuPanel;    // 开始界面面板
    public CanvasGroup startMenuCanvasGroup; // 开始界面的CanvasGroup（可选，用于淡入效果）
    
    [Header("时间设置")]
    public float logoDisplayTime = 2f;   // Logo显示时长（秒）
    public float fadeOutDuration = 1f;   // Logo淡出时长（秒）
    public float fadeInDuration = 0.5f;  // 开始界面淡入时长（秒，如果为0则直接显示）
    
    [Header("跳过设置")]
    public bool canSkip = true;          // 是否允许点击跳过
    public KeyCode skipKey = KeyCode.Space; // 跳过按键
    
    private bool isSkipped = false;
    private bool isPlaying = false;

    private void Start()
    {
        if(!logoCanvasGroup.gameObject.activeSelf)
        {
            logoCanvasGroup.gameObject.SetActive(true);
        }
        // 初始化：隐藏开始界面，显示Logo
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }
        
        if (startMenuCanvasGroup != null)
        {
            startMenuCanvasGroup.alpha = 0f;
        }
        
        if (logoCanvasGroup != null)
        {
            logoCanvasGroup.alpha = 1f;
        }
        
        // 开始播放开屏动画
        StartCoroutine(PlaySplashSequence());
    }

    private void Update()
    {
        // 检测跳过输入
        if (canSkip && isPlaying && !isSkipped)
        {
            if (Input.GetKeyDown(skipKey) || Input.GetMouseButtonDown(0))
            {
                Skip();
            }
        }
    }

    private IEnumerator PlaySplashSequence()
    {
        isPlaying = true;
        
        // 步骤1: 显示Logo
        Debug.Log("[SplashScreen] 显示Logo");
        
        // 等待指定时间
        float elapsedTime = 0f;
        while (elapsedTime < logoDisplayTime && !isSkipped)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 如果被跳过，立即结束
        if (isSkipped)
        {
            ShowStartMenu();
            yield break;
        }
        
        // 步骤2: Logo淡出
        Debug.Log("[SplashScreen] Logo淡出");
        if (logoCanvasGroup != null)
        {
            elapsedTime = 0f;
            float startAlpha = logoCanvasGroup.alpha;

            while (elapsedTime < fadeOutDuration && !isSkipped)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeOutDuration;
                logoCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
                yield return null;
            }

            logoCanvasGroup.alpha = 0f;
        }
        
        logoCanvasGroup.gameObject.SetActive(false);
        // 步骤3: 显示开始界面
        ShowStartMenu();
        
        isPlaying = false;
    }

    private void ShowStartMenu()
    {
        Debug.Log("[SplashScreen] 显示开始界面");
        
        // 隐藏Logo
        if (gameObject != null)
        {
            gameObject.SetActive(false);
        }
        
        // 显示开始界面
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(true);
        }
        
        // 如果需要淡入效果
        if (startMenuCanvasGroup != null && fadeInDuration > 0f)
        {
            StartCoroutine(FadeInStartMenu());
        }
        else if (startMenuCanvasGroup != null)
        {
            // 直接显示
            startMenuCanvasGroup.alpha = 1f;
        }
    }

    private IEnumerator FadeInStartMenu()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeInDuration;
            startMenuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            yield return null;
        }
        
        startMenuCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 跳过开屏动画
    /// </summary>
    public void Skip()
    {
        if (isSkipped) return;
        
        isSkipped = true;
        Debug.Log("[SplashScreen] 跳过开屏动画");
        
        // 停止所有协程
        StopAllCoroutines();
        
        // 直接显示开始界面
        ShowStartMenu();
    }

    /// <summary>
    /// 提供给按钮调用的跳过方法
    /// </summary>
    public void OnSkipButtonClicked()
    {
        Skip();
    }
}
