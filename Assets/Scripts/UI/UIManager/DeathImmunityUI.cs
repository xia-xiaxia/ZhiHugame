using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 免死道具UI：负责免死确认和提示的显示
/// </summary>
public class DeathImmunityUI : MonoBehaviour
{
    public static DeathImmunityUI Instance;

    [Header("免死确认弹窗")]
    public GameObject deathImmunityPanel;
    public Text deathImmunityText;
    public Button useDeathImmunityButton;
    public Button declineDeathImmunityButton;

    [Header("免死生效提示")]
    public GameObject deathImmunityMessagePanel;
    public Text deathImmunityMessageText;
    public Button deathImmunityMessageConfirmButton;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 显示免死确认弹窗
    /// </summary>
    public void ShowPrompt(PolicyItem item, int deathType)
    {
        if (deathImmunityPanel != null) deathImmunityPanel.SetActive(true);

        // 构建提示文本
        string deathTypeName = GetDeathTypeName(deathType);
        string promptText = $"检测到致命危机：{deathTypeName}\n\n" +
                           $"是否使用国策：{item.name}？\n{item.desc}\n" +
                           $"剩余使用次数：{(item.usageCount == -1 ? "无限" : item.usageCount.ToString())}";

        if (deathImmunityText != null)
            deathImmunityText.text = promptText;

        // 绑定按钮
        if (useDeathImmunityButton != null)
        {
            useDeathImmunityButton.onClick.RemoveAllListeners();
            useDeathImmunityButton.onClick.AddListener(() =>
            {
                deathImmunityPanel.SetActive(false);
                GameControl.Instance?.OnDeathImmunityUse();
            });
        }

        if (declineDeathImmunityButton != null)
        {
            declineDeathImmunityButton.onClick.RemoveAllListeners();
            declineDeathImmunityButton.onClick.AddListener(() =>
            {
                deathImmunityPanel.SetActive(false);
                GameControl.Instance?.OnDeathImmunityDecline();
            });
        }
    }

    /// <summary>
    /// 显示免死生效提示
    /// </summary>
    public void ShowMessage(string message)
    {
        if (deathImmunityMessagePanel != null)
        {
            deathImmunityMessagePanel.SetActive(true);
        }

        if (deathImmunityMessageText != null)
        {
            deathImmunityMessageText.text = message;
        }

        // 绑定确认按钮
        if (deathImmunityMessageConfirmButton != null)
        {
            deathImmunityMessageConfirmButton.onClick.RemoveAllListeners();
            deathImmunityMessageConfirmButton.onClick.AddListener(() =>
            {
                if (deathImmunityMessagePanel != null)
                {
                    deathImmunityMessagePanel.SetActive(false);
                }
            });
        }
    }

    /// <summary>
    /// 获取死亡类型名称
    /// </summary>
    private string GetDeathTypeName(int deathType)
    {
        switch (deathType)
        {
            case 1: return "国君上限危机";
            case -1: return "国君下限危机";
            case 2: return "卿士上限危机";
            case -2: return "卿士下限危机";
            case 3: return "贵族上限危机";
            case -3: return "贵族下限危机";
            case 4: return "外臣上限危机";
            case -4: return "外臣下限危机";
            case 5: return "庶人上限危机";
            case -5: return "庶人下限危机";
            case 6: return "事件强制死亡";
            default: return "未知危机";
        }
    }
}
