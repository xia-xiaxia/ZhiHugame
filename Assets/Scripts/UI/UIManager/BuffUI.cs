using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BUFF/时局UI：负责BUFF面板的显示
/// </summary>
public class BuffUI : MonoBehaviour
{
    public static BuffUI Instance;

    [Header("BUFF面板组件")]
    public GameObject buffPanel;
    public Transform buffItemsParent;
    public GameObject buffItemButtonPrefab;
    public Text buffDetailText;
    public Button buffCloseButton;
    public Button buffOpenButton;

    private List<GameObject> buffItemButtons = new List<GameObject>();
    private string selectedBuffId = null;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 绑定打开按钮
        if (buffOpenButton != null)
        {
            buffOpenButton.onClick.RemoveAllListeners();
            buffOpenButton.onClick.AddListener(ShowBuffPanel);
        }
    }

    /// <summary>
    /// 显示BUFF面板
    /// </summary>
    public void ShowBuffPanel()
    {
        if (buffPanel != null) buffPanel.SetActive(true);
        
        RefreshBuffList();

        // 绑定关闭按钮
        if (buffCloseButton != null)
        {
            buffCloseButton.onClick.RemoveAllListeners();
            buffCloseButton.onClick.AddListener(HideBuffPanel);
        }
    }

    /// <summary>
    /// 刷新BUFF列表
    /// </summary>
    private void RefreshBuffList()
    {
        // 清空旧按钮
        foreach (var go in buffItemButtons)
            if (go != null) Destroy(go);
        buffItemButtons.Clear();

        if (BuffManager.Instance == null || BuffManager.Instance.GetActiveBuffs() == null)
        {
            if (buffDetailText != null) buffDetailText.text = "暂无时局";
            return;
        }

        var activeBuffs = BuffManager.Instance.GetActiveBuffs();
        if (activeBuffs.Count == 0)
        {
            if (buffDetailText != null) buffDetailText.text = "暂无时局";
            return;
        }

        if (buffItemsParent == null || buffItemButtonPrefab == null)
        {
            Debug.LogError("[BuffUI] UI组件未设置");
            return;
        }

        foreach (var buff in activeBuffs)
        {
            var btnGo = Instantiate(buffItemButtonPrefab, buffItemsParent);

            // 设置文本
            var txt = btnGo.GetComponentInChildren<Text>();
            if (txt != null)
            {
                string dur = buff.duration < 0 ? "永久" : $"{buff.duration}年";
                string buffName = buff.name ?? buff.id;
                string buffDesc = buff.description ?? "";
                string buffResult = buff.result ?? "";

                txt.text = $"{buffName} (剩余{dur})\n{buffDesc}\n{buffResult}";
            }

            // 绑定点击事件
            var button = btnGo.GetComponent<Button>();
            if (button != null)
            {
                var captured = buff;
                button.onClick.AddListener(() => OnBuffClicked(captured));
            }

            buffItemButtons.Add(btnGo);
        }

        // 默认选中第一个
        if (activeBuffs.Count > 0)
        {
            OnBuffClicked(activeBuffs[0]);
        }
    }

    /// <summary>
    /// BUFF点击处理
    /// </summary>
    private void OnBuffClicked(BuffDefinition buff)
    {
        if (buff == null) return;
        
        selectedBuffId = buff.id;

        if (buffDetailText != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"名称：{(buff.name ?? buff.id)}");
            sb.AppendLine($"时限：{(buff.duration < 0 ? "∞" : buff.duration.ToString())}");
            
            if (!string.IsNullOrEmpty(buff.description))
                sb.AppendLine($"描述：{buff.description}");

            // 显示数值变化
            bool hasChanges = false;
            if (buff.kingChange != 0)
            {
                if (!hasChanges) { sb.AppendLine("每年数值变化："); hasChanges = true; }
                sb.AppendLine($" - 国君: {(buff.kingChange >= 0 ? "+" : "")}{buff.kingChange}");
            }
            if (buff.nobleChange != 0)
            {
                if (!hasChanges) { sb.AppendLine("每年数值变化："); hasChanges = true; }
                sb.AppendLine($" - 贵族: {(buff.nobleChange >= 0 ? "+" : "")}{buff.nobleChange}");
            }
            if (buff.scholarChange != 0)
            {
                if (!hasChanges) { sb.AppendLine("每年数值变化："); hasChanges = true; }
                sb.AppendLine($" - 士族: {(buff.scholarChange >= 0 ? "+" : "")}{buff.scholarChange}");
            }
            if (buff.foreignChange != 0)
            {
                if (!hasChanges) { sb.AppendLine("每年数值变化："); hasChanges = true; }
                sb.AppendLine($" - 外臣: {(buff.foreignChange >= 0 ? "+" : "")}{buff.foreignChange}");
            }
            if (buff.peopleChange != 0)
            {
                if (!hasChanges) { sb.AppendLine("每年数值变化："); hasChanges = true; }
                sb.AppendLine($" - 国人: {(buff.peopleChange >= 0 ? "+" : "")}{buff.peopleChange}");
            }

            buffDetailText.text = sb.ToString();
        }
    }

    /// <summary>
    /// 隐藏BUFF面板
    /// </summary>
    public void HideBuffPanel()
    {
        if (buffPanel != null) buffPanel.SetActive(false);
        selectedBuffId = null;
        if (buffDetailText != null) buffDetailText.text = "";
    }
}
