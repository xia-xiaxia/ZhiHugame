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
/// 统计数值变化特效控制器（简化版）
/// 直接在现有的数值图标上叠加显示增加/下降特效
/// 支持填充条显示数值百分比
/// </summary>
public class StatEffectController : MonoBehaviour
{
    [Header("数值类型")]
    public StatType statType;

    [Header("数值图标（现有的固定位置图片）")]
    public Image baseIconImage; // 基础数值图标（可选，用于获取位置）

    [Header("特效图片（会在图标位置叠加显示）")]
    public Image increaseEffectImage; // 增加时显示的特效图片
    public Image decreaseEffectImage; // 下降时显示的特效图片

    [Header("数值填充条（可选）")]
    public Image valueFilledImage; // 数值填充图片（显示当前数值百分比）

    [Header("特效设置")]
    public float effectDuration = 0.8f; // 特效显示时长
    public AnimationCurve effectFadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // 淡入淡出曲线
    
    [Header("特效颜色（可选覆盖）")]
    public bool useCustomColors = false;
    public Color increaseColor = new Color(0, 1, 0, 1); // 增加特效颜色（绿色）
    public Color decreaseColor = new Color(1, 0, 0, 1); // 下降特效颜色（红色）

    private StatModel stats;
    private int previousValue;
    private Coroutine currentEffectCoroutine;

    void Start()
    {
        // 获取数值模型
        if (GameControl.Instance != null)
        {
            stats = GameControl.Instance.stats;
        }

        if (stats == null)
        {
            Debug.LogError($"[StatEffectController] 无法获取 StatModel，特效将不会工作");
            return;
        }

        // 初始化前一个数值
        previousValue = GetCurrentValue();

        // 初始化特效图片
        InitializeEffectImages();

        // 初始化填充图片（立即更新到当前数值）
        UpdateFilledImage();

        // 订阅数值变化事件
        SubscribeToEvents();
    }

    void OnEnable()
    {
        // 每次启用时也更新填充条，确保显示正确
        if (stats != null && valueFilledImage != null)
        {
            UpdateFilledImage();
        }
    }

    private void InitializeEffectImages()
    {
        // 设置增加特效图片
        if (increaseEffectImage != null)
        {
            increaseEffectImage.gameObject.SetActive(false);
            if (useCustomColors)
            {
                increaseEffectImage.color = increaseColor;
            }
        }

        // 设置下降特效图片
        if (decreaseEffectImage != null)
        {
            decreaseEffectImage.gameObject.SetActive(false);
            if (useCustomColors)
            {
                decreaseEffectImage.color = decreaseColor;
            }
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
                stats.OnKingChanged += OnValueChanged;
                break;
            case StatType.Noble:
                stats.OnNobleChanged += OnValueChanged;
                break;
            case StatType.Scholar:
                stats.OnScholarChanged += OnValueChanged;
                break;
            case StatType.Foreign:
                stats.OnForeignChanged += OnValueChanged;
                break;
            case StatType.People:
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
                stats.OnKingChanged -= OnValueChanged;
                break;
            case StatType.Noble:
                stats.OnNobleChanged -= OnValueChanged;
                break;
            case StatType.Scholar:
                stats.OnScholarChanged -= OnValueChanged;
                break;
            case StatType.Foreign:
                stats.OnForeignChanged -= OnValueChanged;
                break;
            case StatType.People:
                stats.OnPeopleChanged -= OnValueChanged;
                break;
        }

        stats.OnStatsChanged -= OnStatsChanged;
    }

    private void OnValueChanged(int newValue)
    {
        int delta = newValue - previousValue;
        previousValue = newValue;

        if (delta > 0)
        {
            // 数值增加
            ShowEffect(increaseEffectImage);
        }
        else if (delta < 0)
        {
            // 数值下降
            ShowEffect(decreaseEffectImage);
        }

        // 更新填充图片
        UpdateFilledImage();
    }

    private void OnStatsChanged()
    {
        // 检查数值是否变化
        int currentValue = GetCurrentValue();
        if (currentValue != previousValue)
        {
            OnValueChanged(currentValue);
        }
    }

    private void ShowEffect(Image effectImage)
    {
        if (effectImage == null) return;

        // 如果有正在播放的特效，停止它
        if (currentEffectCoroutine != null)
        {
            StopCoroutine(currentEffectCoroutine);
        }

        // 开始新的特效
        currentEffectCoroutine = StartCoroutine(PlayEffectCoroutine(effectImage));
    }

    private IEnumerator PlayEffectCoroutine(Image effectImage)
    {
        // 显示特效图片
        effectImage.gameObject.SetActive(true);

        float elapsedTime = 0f;
        Color originalColor = effectImage.color;

        while (elapsedTime < effectDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / effectDuration;

            // 使用曲线控制透明度
            float alpha = effectFadeCurve.Evaluate(progress);
            effectImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            // 如果图片是 filled 类型，也可以控制 fillAmount
            if (effectImage.type == Image.Type.Filled)
            {
                effectImage.fillAmount = 1f - progress;
            }

            yield return null;
        }

        // 隐藏特效图片
        effectImage.gameObject.SetActive(false);
        effectImage.color = originalColor;

        currentEffectCoroutine = null;
    }

    private void UpdateFilledImage()
    {
        if (valueFilledImage == null || stats == null) return;

        float fillAmount = GetFillAmount();
        valueFilledImage.fillAmount = fillAmount;
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

        int currentValue = GetCurrentValue();
        int maxValue = GetMaxValue();
        int minValue = GetMinValue();

        if (maxValue <= 0) return 0f;

        return (float)(currentValue - minValue) / (maxValue - minValue);
    }

    private int GetMaxValue()
    {
        if (stats == null) return 100;

        switch (statType)
        {
            case StatType.King:
                return stats.kingMax;
            case StatType.Noble:
                return stats.nobleMax;
            case StatType.Scholar:
                return stats.scholarMax;
            case StatType.Foreign:
                return stats.foreignMax;
            case StatType.People:
                return stats.peopleMax;
            default:
                return 100;
        }
    }

    private int GetMinValue()
    {
        if (stats == null) return 0;

        switch (statType)
        {
            case StatType.King:
                return stats.kingMin;
            case StatType.Noble:
                return stats.nobleMin;
            case StatType.Scholar:
                return stats.scholarMin;
            case StatType.Foreign:
                return stats.foreignMin;
            case StatType.People:
                return stats.peopleMin;
            default:
                return 0;
        }
    }

    // 可选：手动更新数值（用于测试或其他情况）
    public void ForceUpdate()
    {
        if (stats == null) return;
        
        int currentValue = GetCurrentValue();
        previousValue = currentValue;
        UpdateFilledImage();
    }
    
    // 强制刷新填充条（不触发特效，只更新显示）
    public void RefreshFilledImage()
    {
        if (stats == null) return;
        UpdateFilledImage();
    }
}
