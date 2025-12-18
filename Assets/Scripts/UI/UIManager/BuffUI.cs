using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BuffUI : MonoBehaviour
{
    public static BuffUI Instance;

    [Header("面板组件")]
    public GameObject buffPanel;
    public Transform buffItemsParent;
    public GameObject buffItemButtonPrefab;
    public Text buffDetailText;

    [Header("控制按钮")]
    public Button buffOpenButton;
    public Button buffCloseButton;
    public Button prevPageButton;
    public Button nextPageButton;
    public Text pageText;

    private List<GameObject> buffItemButtons = new List<GameObject>();
    private int itemsPerPage = 4;
    private int currentPage = 0;
    private int maxPage = 1;

    void Awake()
    {
        Instance = this;
        if (prevPageButton) prevPageButton.onClick.AddListener(PrevPage);
        if (nextPageButton) nextPageButton.onClick.AddListener(NextPage);
    }

    void Start()
    {
        if (buffOpenButton != null) buffOpenButton.onClick.AddListener(ShowBuffPanel);
        if (buffCloseButton != null) buffCloseButton.onClick.AddListener(HideBuffPanel);
    }

    public void ShowBuffPanel()
    {
        if (buffPanel != null) buffPanel.SetActive(true);
        currentPage = 0;
        RefreshBuffList();
    }

    public void HideBuffPanel()
    {
        if (buffPanel != null) buffPanel.SetActive(false);
        if (buffDetailText != null) buffDetailText.text = "";
    }

    private void RefreshBuffList()
    {
        // 1. 清理并重新生成所有按钮
        foreach (var go in buffItemButtons) if (go != null) Destroy(go);
        buffItemButtons.Clear();

        if (BuffManager.Instance == null) return;
        var activeBuffs = BuffManager.Instance.GetActiveBuffs();

        if (activeBuffs == null || activeBuffs.Count == 0)
        {
            if (buffDetailText != null) buffDetailText.text = "暂无时局";
            UpdatePageDisplay(); // 更新按钮状态
            return;
        }

        // 2. 实例化所有BUFF按钮
        for (int i = 0; i < activeBuffs.Count; i++)
        {
            var buff = activeBuffs[i];
            var btnGo = Instantiate(buffItemButtonPrefab, buffItemsParent);
            SetupBuffButton(btnGo, buff);
            buffItemButtons.Add(btnGo);
        }

        // 3. 计算页数并刷新显示
        maxPage = Mathf.Max(1, Mathf.CeilToInt((float)activeBuffs.Count / itemsPerPage));
        UpdatePageDisplay();

        // 默认显示第一个BUFF详情
        if (activeBuffs.Count > 0) OnBuffClicked(activeBuffs[0]);
    }

    private void SetupBuffButton(GameObject btnGo, BuffDefinition buff)
    {
        var txt = btnGo.GetComponentInChildren<Text>();
        if (txt != null)
        {
            string dur = buff.duration < 0 ? "永久" : $"{buff.duration}年";
            txt.text = $"{buff.name ?? buff.id}\n(剩余{dur})";
        }

        var button = btnGo.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnBuffClicked(buff));
        }
    }

    private void UpdatePageDisplay()
    {
        int start = currentPage * itemsPerPage;
        int end = start + itemsPerPage;

        for (int i = 0; i < buffItemButtons.Count; i++)
        {
            buffItemButtons[i].SetActive(i >= start && i < end);
        }

        if (prevPageButton) prevPageButton.interactable = (currentPage > 0);
        if (nextPageButton) nextPageButton.interactable = (currentPage < maxPage - 1);
        if (pageText) pageText.text = $"{currentPage + 1} / {maxPage}";
    }

    public void NextPage() { if (currentPage < maxPage - 1) { currentPage++; UpdatePageDisplay(); } }
    public void PrevPage() { if (currentPage > 0) { currentPage--; UpdatePageDisplay(); } }

    private void OnBuffClicked(BuffDefinition buff)
    {
        if (buff == null || buffDetailText == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"【{(buff.name ?? buff.id)}】");
        sb.AppendLine($"时限：{(buff.duration < 0 ? "∞" : buff.duration + "年")}");
        if (!string.IsNullOrEmpty(buff.description)) sb.AppendLine($"背景：{buff.description}");
        if (!string.IsNullOrEmpty(buff.result)) sb.AppendLine($"效果：{buff.result}");

        // 数值变化详情
        bool hasChanges = (buff.kingChange != 0 || buff.nobleChange != 0 || buff.scholarChange != 0 || buff.foreignChange != 0 || buff.peopleChange != 0);
        if (hasChanges)
        {
            sb.AppendLine("\n[每年数值修正]");
            if (buff.kingChange != 0) sb.AppendLine($" - 国君: {FormatVal(buff.kingChange)}");
            if (buff.nobleChange != 0) sb.AppendLine($" - 宗族: {FormatVal(buff.nobleChange)}");
            if (buff.scholarChange != 0) sb.AppendLine($" - 卿士: {FormatVal(buff.scholarChange)}");
            if (buff.foreignChange != 0) sb.AppendLine($" - 外臣: {FormatVal(buff.foreignChange)}");
            if (buff.peopleChange != 0) sb.AppendLine($" - 庶人: {FormatVal(buff.peopleChange)}");
        }

        buffDetailText.text = sb.ToString();
    }

    private string FormatVal(int val) => val >= 0 ? $"+{val}" : val.ToString();
}