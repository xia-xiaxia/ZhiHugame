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

    void Awake()
    {
        Instance = this;
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
}
