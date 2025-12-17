using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 数值类型枚举
/// </summary>
public enum StatType
{
    King,    // 国君
    Noble,   // 贵族
    Scholar, // 士族
    Foreign, // 外臣
    People   // 国人
}

/// <summary>
/// 统计数值变化特效控制器（重构版）
/// 修复：
/// 1. 特效颜色透明度卡在中间不变的问题
/// 2. 游戏结束时不应该重置数值和播放动画
/// 3. 游戏重开后特效图片透明度异常的问题
/// </summary>
public class StatEffectController : MonoBehaviour
{
    [Header("数值类型")]
    public StatType statType;

    [Header("数值图标（现有的固定位置图片）")]
    public Image baseIconImage;

    [Header("特效图片（会在图标位置叠加显示）")]
    public Image increaseEffectImage;
    public Image decreaseEffectImage;

    [Header("数值填充条（可选）")]
    public Image valueFilledImage;

    [Header("特效设置")]
    public float effectDuration = 1.5f;
    public AnimationCurve effectFadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("填充条平滑设置")]
    public float fillSmoothDuration = 0.3f;
    public AnimationCurve fillSmoothCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("特效颜色（可选覆盖）")]
    public bool useCustomColors = false;
    public Color increaseColor = new Color(0, 1, 0, 1);
    public Color decreaseColor = new Color(1, 0, 0, 1);

    private StatModel stats;
    private int previousValue;
    private Coroutine currentEffectCoroutine;
    private Coroutine currentFillCoroutine;
    private bool isGameOver = false; // 游戏结束标志
    private Color increaseOriginalColor; // 保存增加特效的初始颜色
    private Color decreaseOriginalColor; // 保存下降特效的初始颜色

    private Buffanime buffanime;

    void Start()
    {
        InitializeController();
        buffanime = GetComponent<Buffanime>();
    }

    void OnEnable()
    {
        // 每次启用时重新初始化（修复重开游戏后的问题）
        if (stats != null)
        {
            InitializeController();
        }
    }

    /// <summary>
    /// 初始化控制器（统一的初始化逻辑）
    /// </summary>
    private void InitializeController()
    {
        // 获取数值模型
        if (stats == null && GameControl.Instance != null)
        {
            stats = GameControl.Instance.stats;
        }

        if (stats == null)
        {
            Debug.LogError($"[StatEffectController] {statType} - 无法获取 StatModel");
            return;
        }

        // 重置游戏结束标志
        isGameOver = false;

        // 初始化前一个数值
        previousValue = GetCurrentValue();

        // 初始化特效图片
        InitializeEffectImages();

        // 立即更新填充条（不播放动画）
        if (valueFilledImage != null)
        {
            valueFilledImage.fillAmount = GetFillAmount();
        }

        // 订阅事件（先取消再订阅，避免重复）
        UnsubscribeFromEvents();
        SubscribeToEvents();
        
        Debug.Log($"[StatEffectController] {statType} - 初始化完成，当前值={previousValue}，isGameOver={isGameOver}");
    }

    private void InitializeEffectImages()
    {
        // 初始化增加特效图片
        if (increaseEffectImage != null)
        {
            increaseEffectImage.gameObject.SetActive(false);
            Color targetColor = useCustomColors ? increaseColor : increaseEffectImage.color;
            increaseOriginalColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0); // 透明度设为0
            increaseEffectImage.color = increaseOriginalColor;
            Debug.Log($"[StatEffectController] {statType} - 增加特效初始化: {increaseEffectImage.name}, 颜色={increaseOriginalColor}");
        }
        else
        {
            Debug.LogWarning($"[StatEffectController] {statType} - increaseEffectImage 未配置！");
        }

        // 初始化下降特效图片
        if (decreaseEffectImage != null)
        {
            decreaseEffectImage.gameObject.SetActive(false);
            Color targetColor = useCustomColors ? decreaseColor : decreaseEffectImage.color;
            decreaseOriginalColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0); // 透明度设为0
            decreaseEffectImage.color = decreaseOriginalColor;
            Debug.Log($"[StatEffectController] {statType} - 下降特效初始化: {decreaseEffectImage.name}, 颜色={decreaseOriginalColor}");
        }
        else
        {
            Debug.LogWarning($"[StatEffectController] {statType} - decreaseEffectImage 未配置！");
        }

        // 如果有基础图标，确保特效图片在同一位置
        if (baseIconImage != null)
        {
            AlignEffectToIcon(increaseEffectImage);
            AlignEffectToIcon(decreaseEffectImage);
        }
    }

    private void AlignEffectToIcon(Image effectImage)
    {
        if (effectImage == null || baseIconImage == null) return;

        RectTransform effectRect = effectImage.GetComponent<RectTransform>();
        RectTransform iconRect = baseIconImage.GetComponent<RectTransform>();

        if (effectRect != null && iconRect != null)
        {
            // 复制位置和大小
            effectRect.anchorMin = iconRect.anchorMin;
            effectRect.anchorMax = iconRect.anchorMax;
            effectRect.anchoredPosition = iconRect.anchoredPosition;
            effectRect.sizeDelta = iconRect.sizeDelta;
        }
    }

    void OnDisable()
    {
        // 停止所有协程
        StopAllCoroutines();
        currentEffectCoroutine = null;
        currentFillCoroutine = null;
        
        // 重置特效图片状态
        if (increaseEffectImage != null && increaseOriginalColor != default(Color))
        {
            increaseEffectImage.gameObject.SetActive(false);
            increaseEffectImage.color = increaseOriginalColor;
        }
        if (decreaseEffectImage != null && decreaseOriginalColor != default(Color))
        {
            decreaseEffectImage.gameObject.SetActive(false);
            decreaseEffectImage.color = decreaseOriginalColor;
        }
        
        Debug.Log($"[StatEffectController] {statType} - OnDisable 完成");
    }

    void OnDestroy()
    {
        // 取消订阅事件
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (stats == null) return;

        switch (statType)
        {
            case StatType.King:
                stats.BuffOnKingChanged += PlayBuffAnime;
                stats.OnKingChanged += OnValueChanged;
                break;
                
            case StatType.Noble:
                stats.BuffOnNobleChanged += PlayBuffAnime;
                stats.OnNobleChanged += OnValueChanged;
                break;
            case StatType.Scholar:
                stats.BuffOnScholarChanged += PlayBuffAnime;
                stats.OnScholarChanged += OnValueChanged;
                break;
            case StatType.Foreign:
                stats.BuffOnForeignChanged += PlayBuffAnime;
                stats.OnForeignChanged += OnValueChanged;
                break;
            case StatType.People:
                stats.BuffOnPeopleChanged += PlayBuffAnime;
                stats.OnPeopleChanged += OnValueChanged;
                break;
        }

        // 通用变化事件
        stats.OnStatsChanged += OnStatsChanged;
    }

    private void UnsubscribeFromEvents()
    {
        if (stats == null) return;

        switch (statType)
        {
            case StatType.King:
                stats.BuffOnKingChanged -= PlayBuffAnime;
                stats.OnKingChanged -= OnValueChanged;
                break;
            case StatType.Noble:
                stats.BuffOnNobleChanged -= PlayBuffAnime;
                stats.OnNobleChanged -= OnValueChanged;
                break;
            case StatType.Scholar:
                stats.BuffOnScholarChanged -= PlayBuffAnime;
                stats.OnScholarChanged -= OnValueChanged;
                break;
            case StatType.Foreign:
                stats.BuffOnForeignChanged -= PlayBuffAnime;
                stats.OnForeignChanged -= OnValueChanged;
                break;
            case StatType.People:
                stats.BuffOnPeopleChanged -= PlayBuffAnime;
                stats.OnPeopleChanged -= OnValueChanged;
                break;
        }

        stats.OnStatsChanged -= OnStatsChanged;
    }

    private void PlayBuffAnime(int delta)
    {
        buffanime.PlayBuffAnime(delta);
    }

    private void OnValueChanged(int newValue)
    {
        // 如果游戏结束，忽略数值变化（避免在结局时重置数值触发动画）
        if (isGameOver)
        {
            Debug.Log($"[StatEffectController] {statType} - 游戏已结束，忽略数值变化");
            return;
        }

        int delta = newValue - previousValue;
        
        if (delta == 0) return; // 没有变化则不处理
        
        Debug.Log($"[StatEffectController] {statType} - 数值变化: 旧值={previousValue}, 新值={newValue}, 变化量={delta}");
        
        previousValue = newValue;

        if (delta > 0)
        {
            // 数值增加
            ShowEffect(increaseEffectImage, increaseOriginalColor);
        }
        else if (delta < 0)
        {
            // 数值下降
            ShowEffect(decreaseEffectImage, decreaseOriginalColor);
        }

        // 更新填充图片
        UpdateFilledImage();
    }

    private void OnStatsChanged()
    {
        // 如果游戏结束，忽略
        if (isGameOver) return;
        
        // 检查数值是否变化
        int currentValue = GetCurrentValue();
        if (currentValue != previousValue)
        {
            OnValueChanged(currentValue);
        }
    }

    private void ShowEffect(Image effectImage, Color baseColor)
    {
        if (effectImage == null)
        {
            Debug.LogWarning($"[StatEffectController] {statType} - effectImage 为空，无法播放特效");
            return;
        }

        // 停止当前正在播放的特效
        if (currentEffectCoroutine != null)
        {
            Debug.Log($"[StatEffectController] {statType} - 停止旧协程");
            StopCoroutine(currentEffectCoroutine);
            currentEffectCoroutine = null;
        }

        // 开始新的特效
        Debug.Log($"[StatEffectController] {statType} - 开始播放特效，颜色={baseColor}");
        currentEffectCoroutine = StartCoroutine(PlayEffectCoroutine(effectImage, baseColor));
    }

    private IEnumerator PlayEffectCoroutine(Image effectImage, Color baseColor)
    {
        if (effectImage == null)
        {
            Debug.LogError($"[StatEffectController] {statType} - 协程启动失败：effectImage 为空");
            yield break;
        }

        Debug.Log($"[协程开始] {statType} - effectImage={effectImage.name}, 开始时间={Time.time}");

        // 确保特效图片激活
        effectImage.gameObject.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < effectDuration)
        {
            if (effectImage == null)
            {
                Debug.LogError($"[协程中断] {statType} - effectImage 在播放中途被销毁");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / effectDuration);

            // 使用曲线控制透明度（0->1->0）
            float alpha = effectFadeCurve.Evaluate(progress);
            effectImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

            yield return null;
        }

        // 确保最终状态：隐藏并重置颜色
        if (effectImage != null)
        {
            effectImage.gameObject.SetActive(false);
            effectImage.color = baseColor; // 透明度回到0
            Debug.Log($"[协程正常结束] {statType} - effectImage={effectImage.name}, 结束时间={Time.time}");
        }

        currentEffectCoroutine = null;
    }

    private void UpdateFilledImage()
    {
        if (valueFilledImage == null || stats == null) return;

        float targetFillAmount = GetFillAmount();
        
        // 停止之前的平滑动画
        if (currentFillCoroutine != null)
        {
            StopCoroutine(currentFillCoroutine);
        }
        
        // 启动平滑填充动画
        currentFillCoroutine = StartCoroutine(SmoothFillCoroutine(targetFillAmount));
    }
    
    private IEnumerator SmoothFillCoroutine(float targetFillAmount)
    {
        if (valueFilledImage == null) yield break;
        
        float startFillAmount = valueFilledImage.fillAmount;
        float elapsedTime = 0f;
        
        // 如果起始值和目标值相同，直接返回
        if (Mathf.Approximately(startFillAmount, targetFillAmount))
        {
            yield break;
        }
        
        while (elapsedTime < fillSmoothDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fillSmoothDuration);
            
            // 使用曲线进行平滑插值
            float curveValue = fillSmoothCurve.Evaluate(progress);
            valueFilledImage.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, curveValue);
            
            yield return null;
        }
        
        // 确保最终值精确
        valueFilledImage.fillAmount = targetFillAmount;
        
        // 添加调试日志
        int currentValue = GetCurrentValue();
        Debug.Log($"[StatEffectController] {statType} - 平滑填充完成: 当前值={currentValue}, fillAmount={targetFillAmount}");
        
        currentFillCoroutine = null;
    }

    private int GetCurrentValue()
    {
        if (stats == null) return 0;

        switch (statType)
        {
            case StatType.King:
                return stats.king;
            case StatType.Noble:
                return stats.noble;
            case StatType.Scholar:
                return stats.scholar;
            case StatType.Foreign:
                return stats.foreign;
            case StatType.People:
                return stats.people;
            default:
                return 0;
        }
    }

    private float GetFillAmount()
    {
        if (stats == null) return 0f;

        // 直接使用 StatModel 中的百分比属性
        switch (statType)
        {
            case StatType.King:
                return stats.KingPercent;
            case StatType.Noble:
                return stats.NoblePercent;
            case StatType.Scholar:
                return stats.ScholarPercent;
            case StatType.Foreign:
                return stats.ForeignPercent;
            case StatType.People:
                return stats.PeoplePercent;
            default:
                return 0f;
        }
    }

    /// <summary>
    /// 设置游戏结束状态（游戏结束时调用，停止响应数值变化）
    /// </summary>
    public void SetGameOver(bool gameOver)
    {
        isGameOver = gameOver;
        Debug.Log($"[StatEffectController] {statType} - SetGameOver: {gameOver}");
    }

    /// <summary>
    /// 刷新填充条（游戏重开时调用，不播放特效动画）
    /// </summary>
    public void RefreshFilledImage()
    {
        if (stats == null)
        {
            Debug.LogWarning($"[StatEffectController] {statType} - RefreshFilledImage: stats 为空");
            return;
        }
        
        // 停止所有正在播放的动画
        StopAllCoroutines();
        currentEffectCoroutine = null;
        currentFillCoroutine = null;
        
        // 重置特效图片状态
        if (increaseEffectImage != null)
        {
            increaseEffectImage.gameObject.SetActive(false);
            increaseEffectImage.color = increaseOriginalColor;
        }
        if (decreaseEffectImage != null)
        {
            decreaseEffectImage.gameObject.SetActive(false);
            decreaseEffectImage.color = decreaseOriginalColor;
        }
        
        // 重置游戏结束标志
        isGameOver = false;
        
        // 更新 previousValue 为当前值
        previousValue = GetCurrentValue();
        
        // 立即更新填充条（不播放动画）
        if (valueFilledImage != null)
        {
            valueFilledImage.fillAmount = GetFillAmount();
        }
        
        Debug.Log($"[StatEffectController] {statType} - RefreshFilledImage 完成，当前值={previousValue}");
    }
    
    /// <summary>
    /// 手动更新数值（用于测试）
    /// </summary>
    public void ForceUpdate()
    {
        if (stats == null) return;
        
        int currentValue = GetCurrentValue();
        previousValue = currentValue;
        
        if (valueFilledImage != null)
        {
            valueFilledImage.fillAmount = GetFillAmount();
        }
    }
}
