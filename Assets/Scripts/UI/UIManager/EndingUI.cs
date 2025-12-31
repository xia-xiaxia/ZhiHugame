using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 结局UI：负责结局面板的显示和动画
/// </summary>
public class EndingUI : MonoBehaviour
{
    public static EndingUI Instance;

    [Header("结局面板组件")]
    public GameObject blackEnd;
    public GameObject endingPanel;
    public Text endingText;
    public Image endingImage;
    public Image endingImageBottom;
    public Text endingYearText;
    public Button restartButton;
    public GameObject endImageContainer;
    public Image endAnimationImage; // the white image for splash
    public float animationDuration = 0.8f;
    public TextMeshProUGUI getCurrencyText;
    private int getCurrencyCount = 0;

    void Awake()
    {
        Instance = this;
        endImageContainer.SetActive(false);
    }

    void Start()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
            getCurrencyText.text = "";
        }
    }

    /// <summary>
    /// 显示结局面板
    /// </summary>
    public void ShowEnding(string endingId, string description, int survivedYears)
    {
        if(endImageContainer != null)
            endImageContainer.SetActive(true);

        if (endingText != null) endingText.text = description;

        if(getCurrencyText != null)
            getCurrencyText.text = "";

        // 加载结局图片
        LoadEndingImage(endingId);
        getCurrencyCount = (int)(survivedYears * GameControl.Instance.stats.currencyMult) + 1;

        // 启动年数计数动画
        if (endingYearText != null)
        {
            StartCoroutine(AnimateYearCounter(survivedYears));
        }

        // 绑定重开按钮
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateCurrencyGain(survivedYears));
                // // 显示商店
                // PolicyShopUI.Instance?.ShowShop();
            });
        }
    }

    // 加上了演出动画的结局显示
    public void ShowEndingWithAnimation(string endingId, string description, int survivedYears)
    {
        blackEnd.SetActive(true);
        StartCoroutine(ShowEndingSequence(endingId, description, survivedYears));
    }

    IEnumerator ShowEndingSequence(string endingId, string description, int survivedYears)
    {
        // 显示白色闪屏动画
        if (endingPanel != null) endingPanel.SetActive(true);
        
        if (endAnimationImage != null)
        {
            endAnimationImage.gameObject.SetActive(true);
            Color color = endAnimationImage.color;
            color.a = 0f;
            endAnimationImage.color = color;

            // 渐显效果
            float duration = animationDuration;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 0.6f, elapsedTime / duration);
                endAnimationImage.color = color;
                yield return null;
            }
            blackEnd.SetActive(false);
            ShowEnding(endingId, description, survivedYears);

            // 渐隐效果
            duration = animationDuration;
            elapsedTime = 0f;
            while(elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(0.6f, 0f, elapsedTime / duration);
                endAnimationImage.color = color;
                yield return null;
            }
            endAnimationImage.gameObject.SetActive(false);
        }
        else 
        {
            ShowEnding(endingId, description, survivedYears);
        }

    }

    /// <summary>
    /// 加载结局图片
    /// </summary>
    private void LoadEndingImage(string endingId)
    {
        if (endingImage == null || endingImageBottom == null) return;

        Sprite endingSprite = Resources.Load<Sprite>($"Endings/{endingId}");
        if (endingSprite != null)
        {
            endingImage.sprite = endingSprite;
            endingImageBottom.sprite = endingSprite;
            endingImage.gameObject.SetActive(true);
            endingImageBottom.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[EndingUI] 未找到结局图片: Resources/Endings/{endingId}");
            endingImage.gameObject.SetActive(false);
            endingImageBottom.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 年数计数动画
    /// </summary>
    private IEnumerator AnimateYearCounter(int targetYear)
    {
        int currentYear = 0;
        float duration = 2.0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float easedProgress = EaseInOutCubic(progress);
            
            currentYear = Mathf.FloorToInt(Mathf.Lerp(0, targetYear, easedProgress));
            
            if (endingYearText != null)
            {
                endingYearText.text = $"执政:  {currentYear}  年";
            }

            yield return null;
        }

        // 确保最终显示精确值
        if (endingYearText != null)
        {
            endingYearText.text = $"执政:  {targetYear}  年";
        }
        // getCurrencyText.text = $"经验：{GameControl.Instance.GetCurrency() - getCurrencyCount} ";
    }

    // 结算动画
    public IEnumerator AnimateCurrencyGain(int targetYear)
    {
        int displayedCurrency = GameControl.Instance.GetCurrency() - getCurrencyCount;
        float duration = 2.0f;
        float elapsedTime = 0f;
        int currentYear = targetYear;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float easedProgress = EaseInOutCubic(progress);

            displayedCurrency = Mathf.FloorToInt(Mathf.Lerp(GameControl.Instance.GetCurrency() - getCurrencyCount, GameControl.Instance.GetCurrency(), easedProgress));
            currentYear = Mathf.FloorToInt(Mathf.Lerp(targetYear, 0, easedProgress));

            if (getCurrencyText != null)
            {
                getCurrencyText.text = $"经验：{displayedCurrency} ";
            }
            if( endingYearText != null)
            {
                endingYearText.text = $"执政:  {currentYear}  年";
            }

            yield return null;
        }

        // 确保最终显示精确值
        if (getCurrencyText != null)
        {
            getCurrencyText.text = $"经验：{GameControl.Instance.GetCurrency()} ";
        }
        if (endingYearText != null)
        {
            endingYearText.text = $"执政:  0  年";
        }

        yield return new WaitForSeconds(1.0f);
        PolicyShopUI.Instance?.ShowShop();
    }

    /// <summary>
    /// 缓动函数：慢 -> 快 -> 慢
    /// </summary>
    private float EaseInOutCubic(float t)
    {
        if (t < 0.5f)
        {
            return 4f * t * t * t;
        }
        else
        {
            float f = (2f * t - 2f);
            return 0.5f * f * f * f + 1f;
        }
    }

    /// <summary>
    /// 隐藏结局面板
    /// </summary>
    public void HideEnding()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }
    }

   
}
