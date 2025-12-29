using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 数值显示UI：负责五维数值和年份的显示
/// </summary>
public class StatsDisplayUI : MonoBehaviour
{
    public static StatsDisplayUI Instance;

    [Header("数值显示")]
    public TextMeshProUGUI statText1; // 王权
    public TextMeshProUGUI statText2; // 贵族
    public TextMeshProUGUI statText3; // 学者
    public TextMeshProUGUI statText4; // 外交
    public TextMeshProUGUI statText5; // 民心

    [Header("年份显示")]
    public Text currentYearText;

    [Header("数据源")]
    public StatModel stats;
    
    [Header("锁定特效图片（所有阶层共用）")]
    public GameObject sharedLockEffect; // 共用的锁定特效图片
    
    [Header("各阶层图标位置（用于定位特效）")]
    public RectTransform kingIconTransform;   // 国君图标位置
    public RectTransform nobleIconTransform;  // 贵族图标位置
    public RectTransform scholarIconTransform; // 学者图标位置
    public RectTransform foreignIconTransform; // 外交图标位置
    public RectTransform peopleIconTransform;  // 民心图标位置
    
    [Header("各阶层原始图标Image组件")]
    public Image kingIcon;   // 国君图标
    public Image nobleIcon;  // 贵族图标
    public Image scholarIcon; // 学者图标
    public Image foreignIcon; // 外交图标
    public Image peopleIcon;  // 民心图标
    
    // 追踪当前锁定的阶层
    private int currentLockedLayer = -1;

    void Awake()
    {
        Instance = this;
        
        // 初始化所有锁定特效为隐藏状态
        InitializeLockEffects();
    }
    
    void Start()
    {
        // 订阅锁定状态改变事件
        if (stats != null)
        {
            stats.OnLayerLockChanged += OnLayerLockChanged;
        }
    }
    
    void OnDestroy()
    {
        // 取消订阅
        if (stats != null)
        {
            stats.OnLayerLockChanged -= OnLayerLockChanged;
        }
    }

    /// <summary>
    /// 更新所有数值显示
    /// </summary>
    public void UpdateStatText()
    {
        if (stats == null)
        {
            Debug.LogWarning("[StatsDisplayUI] stats 未设置");
            return;
        }

        if (statText1 != null) statText1.text = stats.king.ToString();
        if (statText2 != null) statText2.text = stats.noble.ToString();
        if (statText3 != null) statText3.text = stats.scholar.ToString();
        if (statText4 != null) statText4.text = stats.foreign.ToString();
        if (statText5 != null) statText5.text = stats.people.ToString();
    }

    /// <summary>
    /// 更新年份显示
    /// </summary>
    public void UpdateYearText(int year)
    {
        if (currentYearText != null)
        {
            currentYearText.text = $"第{year}年";
        }
    }

    /// <summary>
    /// 更新货币显示
    /// </summary>
    public void UpdateCurrencyDisplay(int currency, Text currencyText)
    {
        if (currencyText != null)
        {
            currencyText.text = $"经验：{currency}";
        }
    }
    
    /// <summary>
    /// 初始化锁定特效为隐藏状态
    /// </summary>
    private void InitializeLockEffects()
    {
        if (sharedLockEffect != null)
        {
            sharedLockEffect.SetActive(false);
            currentLockedLayer = -1;
        }
    }
    
    /// <summary>
    /// 处理锁定状态改变事件
    /// </summary>
    private void OnLayerLockChanged(int layer, bool lockIncrease, bool isAdded)
    {
        if (isAdded)
        {
            // 添加锁定时，显示特效并移动到对应阶层位置
            ShowLockEffectAtLayer(layer);
            Debug.Log($"[StatsDisplayUI] 锁定特效显示 - 阶层:{layer}, 禁止{(lockIncrease ? "上升" : "下降")}");
        }
        else
        {
            // 移除锁定时，检查该阶层是否还有其他锁定
            bool hasOtherLock = CheckIfLayerHasOtherLock(layer);
            if (!hasOtherLock)
            {
                // 如果该阶层没有其他锁定了，检查是否需要移动到其他锁定阶层或隐藏
                UpdateLockEffectDisplay();
                Debug.Log($"[StatsDisplayUI] 阶层{layer}锁定已全部解除");
            }
        }
    }
    
    /// <summary>
    /// 在指定阶层位置显示锁定特效
    /// </summary>
    private void ShowLockEffectAtLayer(int layer)
    {
        if (sharedLockEffect == null)
        {
            Debug.LogWarning("[StatsDisplayUI] 共用锁定特效图片未配置！");
            return;
        }
        
        // 隐藏当前锁定阶层的原图标（如果有）
        if (currentLockedLayer != -1 && currentLockedLayer != layer)
        {
            SetLayerIconVisibility(currentLockedLayer, true);
        }
        
        RectTransform targetPosition = GetLayerIconTransform(layer);
        Image targetIcon = GetLayerIcon(layer);
        
        if (targetPosition != null)
        {
            // 隐藏原图标
            if (targetIcon != null)
            {
                targetIcon.enabled = false;
            }
            
            // 移动特效到目标位置
            RectTransform effectRect = sharedLockEffect.GetComponent<RectTransform>();
            if (effectRect != null)
            {
                effectRect.position = targetPosition.position;
            }
            
            sharedLockEffect.SetActive(true);
            currentLockedLayer = layer;
        }
        else
        {
            Debug.LogWarning($"[StatsDisplayUI] 未找到阶层{layer}的图标位置！");
        }
    }
    
    /// <summary>
    /// 更新锁定特效显示（检查是否有其他锁定需要显示）
    /// </summary>
    private void UpdateLockEffectDisplay()
    {
        if (stats == null || stats.activeLayerLocks == null)
        {
            HideLockEffect();
            return;
        }
        
        // 查找第一个有锁定的阶层
        if (stats.activeLayerLocks.Count > 0)
        {
            int firstLockedLayer = stats.activeLayerLocks[0].layer;
            ShowLockEffectAtLayer(firstLockedLayer);
        }
        else
        {
            HideLockEffect();
        }
    }
    
    /// <summary>
    /// 隐藏锁定特效
    /// </summary>
    private void HideLockEffect()
    {
        // 恢复当前锁定阶层的原图标
        if (currentLockedLayer != -1)
        {
            SetLayerIconVisibility(currentLockedLayer, true);
        }
        
        if (sharedLockEffect != null)
        {
            sharedLockEffect.SetActive(false);
            currentLockedLayer = -1;
        }
    }
    
    /// <summary>
    /// 根据阶层获取对应的图标位置
    /// </summary>
    private RectTransform GetLayerIconTransform(int layer)
    {
        switch (layer)
        {
            case 1: // 国君
                return kingIconTransform;
            case 2: // 卿士（学者）
                return scholarIconTransform;
            case 3: // 宗族（贵族）
                return nobleIconTransform;
            case 4: // 外臣（外交）
                return foreignIconTransform;
            case 5: // 庶人（民心）
                return peopleIconTransform;
            default:
                return null;
        }
    }
    
    /// <summary>
    /// 根据阶层获取对应的图标Image组件
    /// </summary>
    private Image GetLayerIcon(int layer)
    {
        switch (layer)
        {
            case 1: // 国君
                return kingIcon;
            case 2: // 卿士（学者）
                return scholarIcon;
            case 3: // 宗族（贵族）
                return nobleIcon;
            case 4: // 外臣（外交）
                return foreignIcon;
            case 5: // 庶人（民心）
                return peopleIcon;
            default:
                return null;
        }
    }
    
    /// <summary>
    /// 设置指定阶层图标的显示/隐藏状态
    /// </summary>
    private void SetLayerIconVisibility(int layer, bool visible)
    {
        Image icon = GetLayerIcon(layer);
        if (icon != null)
        {
            icon.enabled = visible;
        }
    }
    
    /// <summary>
    /// 检查某阶层是否还有其他锁定
    /// </summary>
    private bool CheckIfLayerHasOtherLock(int layer)
    {
        if (stats == null || stats.activeLayerLocks == null) return false;
        
        foreach (var lockData in stats.activeLayerLocks)
        {
            if (lockData.layer == layer)
            {
                return true;
            }
        }
        return false;
    }
}
