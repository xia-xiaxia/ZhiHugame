using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PolicyMenuUI : MonoBehaviour
{
    public static PolicyMenuUI Instance;

    [Header("UI组件")]
    public GameObject policyMenuPanel;
    public Transform policyItemsParent; 
    public GameObject policyItemButtonPrefab;

    [Header("翻页控制")]
    public Button prevPageButton;
    public Button nextPageButton;
    public Text pageText;

    [Header("数据源")]
    public StatModel stats;

    private List<GameObject> policyItemButtons = new List<GameObject>();
    private int itemsPerPage = 4;
    private int currentPage = 0;
    private int maxPage = 1;

    void Awake()
    {
        Instance = this;
        if (prevPageButton) prevPageButton.onClick.AddListener(PrevPage);
        if (nextPageButton) nextPageButton.onClick.AddListener(NextPage);
    }

    public void ShowMenu()
    {
        if (policyMenuPanel != null) policyMenuPanel.SetActive(true);
        currentPage = 0;
        RefreshPolicyList();
    }

    public void HideMenu()
    {
        if (policyMenuPanel != null) policyMenuPanel.SetActive(false);
    }

    private void RefreshPolicyList()
    {
        foreach (var btn in policyItemButtons) if (btn != null) Destroy(btn);
        policyItemButtons.Clear();

        if (GameControl.Instance == null || GameControl.Instance.stats.policyBag == null) return;

        var bag = GameControl.Instance.stats.policyBag;
        stats = GameControl.Instance.stats;

        // 1. 实例化所有道具
        for (int i = 0; i < bag.Count; i++)
        {
            var item = bag[i];
            int index = i;
            GameObject btn = Instantiate(policyItemButtonPrefab, policyItemsParent);
            SetupButton(btn, item, index);
            policyItemButtons.Add(btn);
        }

        // 2. 计算分页并更新显示
        maxPage = Mathf.Max(1, Mathf.CeilToInt((float)bag.Count / itemsPerPage));
        UpdatePageDisplay();
    }

    private void SetupButton(GameObject btn, PolicyItem item, int index)
    {
        // 组件赋值
        var trigger = btn.GetComponent<PolicyInShopTrigger>();
        if (trigger != null) trigger.SetPolicyItem(item);

        var destroyPolicy = btn.GetComponent<DestroyPolicy>();
        if (destroyPolicy != null) destroyPolicy.policyItem = item;

        // 文本显示
        Text btnText = btn.GetComponentInChildren<Text>();
        if (btnText != null)
        {
            string usageText = item.usageCount == -1 ? "无限" : item.usageCount.ToString();
            btnText.text = $"{item.name} [{GetPolicyTypeName(item.type)}]\n次数：{usageText}\n{item.desc}\n{item.result}";
        }

        // 点击事件
        Button button = btn.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnPolicyClicked(item, index));
            if (item.type == 2) button.interactable = false; // 免死道具不可主动使用
        }
    }

    private void UpdatePageDisplay()
    {
        int start = currentPage * itemsPerPage;
        int end = start + itemsPerPage;

        for (int i = 0; i < policyItemButtons.Count; i++)
        {
            policyItemButtons[i].SetActive(i >= start && i < end);
        }

        if (prevPageButton) prevPageButton.interactable = (currentPage > 0);
        if (nextPageButton) nextPageButton.interactable = (currentPage < maxPage - 1);
        if (pageText) pageText.text = $"{currentPage + 1} / {maxPage}";
    }

    public void NextPage() { if (currentPage < maxPage - 1) { currentPage++; UpdatePageDisplay(); } }
    public void PrevPage() { if (currentPage > 0) { currentPage--; UpdatePageDisplay(); } }

    private void OnPolicyClicked(PolicyItem item, int index)
    {
        if (item == null) return;
        switch (item.type)
        {
            case 1: UseThresholdPolicy(item); break;
            case 3: UseSkipPolicy(item); break;
            case 4: UseAdjustPolicy(item); break;
            case 5: UseSituationPolicy(item); break;
        }
    }

    private void ConsumePolicy(PolicyItem item)
    {
        if (item.usageCount > 0) item.usageCount--;
        if (item.usageCount == 0) GameControl.Instance?.RemovePolicy(item.id);
        HideMenu();
    }

    private void UseThresholdPolicy(PolicyItem item)
    {
        if (item.usageCount == 0) return;

        //全局统计
        GameControl.Instance.gameStatistics.usePolicy(item.id);

        if (stats != null && item.targetLayers != null)
        {
            foreach (int layer in item.targetLayers)
            {
                stats.AddLayerLock(System.Math.Abs(layer), layer > 0, item.lockDuration);
            }
        }
        
        if (stats != null)
        {
            stats.ApplyStatChange(item.kingChange, item.nobleChange, item.scholarChange, item.foreignChange, item.peopleChange, 2);
            StatsDisplayUI.Instance?.UpdateStatText();
            GameControl.Instance?.OnStatsChanged();
        }
        ConsumePolicy(item);
    }

    private void UseSkipPolicy(PolicyItem item)
    {
        //全局统计
        GameControl.Instance.gameStatistics.usePolicy(item.id);

        GameControl.Instance.year++;
        ConsumePolicy(item);
        EventDisplayUI.Instance?.ClearText();
        GameControl.Instance?.ProcessNextTurn();
    }

    private void UseAdjustPolicy(PolicyItem item)
    {
        //全局统计
        GameControl.Instance.gameStatistics.usePolicy(item.id);

        if (stats != null)
        {
            stats.ApplyStatChange(item.kingChange, item.nobleChange, item.scholarChange, item.foreignChange, item.peopleChange, 2);
            StatsDisplayUI.Instance?.UpdateStatText();
            GameControl.Instance?.OnStatsChanged();
        }
        ConsumePolicy(item);
    }

    private void UseSituationPolicy(PolicyItem item)
    {
        //全局统计
        GameControl.Instance.gameStatistics.usePolicy(item.id);

        if (!string.IsNullOrEmpty(item.triggeredBuffId))
        {
            BuffManager.Instance?.AddBuffById(item.triggeredBuffId);
            BuffUI.Instance?.ShowBuffPanel();
        }
        ConsumePolicy(item);
    }

    private string GetPolicyTypeName(int type)
    {
        switch (type)
        {
            case 1: return "锁定";
            case 2: return "免死";
            case 3: return "跳过";
            case 4: return "调控";
            case 5: return "时局";
            default: return "未知";
        }
    }
}