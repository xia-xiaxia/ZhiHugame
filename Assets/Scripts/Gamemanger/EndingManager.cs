using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 结局管理器：负责结局检测、免死道具处理、结局触发
/// </summary>
public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;

    [Header("引用")]
    public StatModel stats;

    [Header("结局配置")]
    public int yearLimit = 100;
    public bool useDynamicThreshold = true;

    [Header("内部状态")]
    private bool endingTriggered = false;
    private bool waitingForDeathImmunityChoice = false;
    private bool deathImmunityChoiceResult = false;

    [Header("数值快照")]
    private int snapshotKing, snapshotNoble, snapshotScholar, snapshotForeign, snapshotPeople;

    void Awake()
    {
        Instance = this;
    }

    // ===== 重置 =====
    public void Reset()
    {
        endingTriggered = false;
        waitingForDeathImmunityChoice = false;
        deathImmunityChoiceResult = false;
    }

    // ===== 保存数值快照 =====
    public void SaveStatsSnapshot()
    {
        if (stats == null) return;
        
        snapshotKing = stats.king;
        snapshotNoble = stats.noble;
        snapshotScholar = stats.scholar;
        snapshotForeign = stats.foreign;
        snapshotPeople = stats.people;
        
        Debug.Log($"[EndingManager] 保存数值快照: K{snapshotKing} N{snapshotNoble} S{snapshotScholar} F{snapshotForeign} P{snapshotPeople}");
    }

    // ===== 数值变化后调用 =====
    public void OnStatsChanged()
    {
        if (GameLifecycleManager.Instance.GameOver) return;
        CheckAndTriggerEnding();
    }

    // ===== 核心：结局检测 =====
    public void CheckAndTriggerEnding()
    {
        if (endingTriggered || stats == null) return;

        // 年份上限
        if (TurnManager.Instance.year >= yearLimit)
        {
            TriggerEnding("YEAR_LIMIT", $"到达年份上限 {yearLimit}，时代终结。");
            return;
        }

        // 获取阈值
        int kMin = useDynamicThreshold ? stats.kingMin : 20;
        int kMax = useDynamicThreshold ? stats.kingMax : 80;
        int nMin = useDynamicThreshold ? stats.nobleMin : 20;
        int nMax = useDynamicThreshold ? stats.nobleMax : 80;
        int sMin = useDynamicThreshold ? stats.scholarMin : 20;
        int sMax = useDynamicThreshold ? stats.scholarMax : 80;
        int fMin = useDynamicThreshold ? stats.foreignMin : 20;
        int fMax = useDynamicThreshold ? stats.foreignMax : 80;
        int pMin = useDynamicThreshold ? stats.peopleMin : 20;
        int pMax = useDynamicThreshold ? stats.peopleMax : 80;

        // 收集所有死亡类型
        List<DeathInfo> deathList = new List<DeathInfo>();
        
        if (stats.king <= kMin)
            deathList.Add(new DeathInfo(-1, "哀", "在你治下君主沦为了群臣的傀儡，为了权力，他们为你呈上了一杯毒酒"));
        if (stats.king >= kMax)
            deathList.Add(new DeathInfo(1, "躁", "你刚愎自用，人们不满你的专横独断，一场政变宣告了你执政的终结"));
        if (stats.noble <= nMin)
            deathList.Add(new DeathInfo(-3, "灵", "你毫不遮掩对贵族的恶劣态度，一位贵族豢养的死士当庭刺死了你"));
        if (stats.noble >= nMax)
            deathList.Add(new DeathInfo(3, "平", "在你治下贵族逐渐掌握朝中大权，无权的你只得在宫墙之内了却残生"));
        if (stats.scholar <= sMin)
            deathList.Add(new DeathInfo(-2, "幽", "你并不在意卿士，一些失意士人起兵作乱，混乱之中你被砍去头颅"));
        if (stats.scholar >= sMax)
            deathList.Add(new DeathInfo(2, "废", "你的权力逐渐让渡给士族，一家大族逼迫你禅位，你无奈顺从"));
        if (stats.foreign <= fMin)
            deathList.Add(new DeathInfo(-4, "殇", "你轻视外臣，他们怀恨在心，转投他国，得势之后出兵将我国灭亡"));
        if (stats.foreign >= fMax)
            deathList.Add(new DeathInfo(4, "纣", "你执政依赖外臣，外国势力无孔不入，最后我们沦为了大国的傀儡"));
        if (stats.people <= pMin)
            deathList.Add(new DeathInfo(-5, "厉", "你横征暴敛，国人不喜，一场国人暴动将你驱逐出了国家"));
        if (stats.people >= pMax)
            deathList.Add(new DeathInfo(5, "携", "朝堂软弱无力，国人拒不上税，在一次暴动后庶人们一脚踹开了你"));

        if (deathList.Count > 0)
        {
            Debug.Log($"[EndingManager] 检测到 {deathList.Count} 个死亡类型，开始依次处理免死道具");
            StartCoroutine(ProcessDeathListWithImmunity(deathList));
        }
    }

    // ===== 依次处理死亡列表 =====
    private IEnumerator ProcessDeathListWithImmunity(List<DeathInfo> deathList)
    {
        for (int i = 0; i < deathList.Count; i++)
        {
            DeathInfo deathInfo = deathList[i];
            Debug.Log($"[EndingManager] 处理第 {i+1}/{deathList.Count} 个死亡：类型 {deathInfo.deathType}");

            PolicyItem immunityItem = PolicyInventory.Instance?.FindDeathImmunityItem(deathInfo.deathType);
            
            if (immunityItem != null)
            {
                Debug.Log($"[EndingManager] 找到免死道具：{immunityItem.name}，询问玩家是否使用");
                yield return StartCoroutine(AskDeathImmunityChoice(immunityItem, deathInfo.deathType));
                
                if (deathImmunityChoiceResult)
                {
                    Debug.Log($"[EndingManager] 玩家使用免死道具，死亡类型 {deathInfo.deathType} 被阻止");
                    continue;
                }
                else
                {
                    Debug.Log($"[EndingManager] 玩家拒绝使用免死道具，触发结局：{deathInfo.endingId}");
                    TriggerEnding(deathInfo.endingId, deathInfo.endingDesc);
                    yield break;
                }
            }
            else
            {
                Debug.Log($"[EndingManager] 无免死道具，触发结局：{deathInfo.endingId}");
                TriggerEnding(deathInfo.endingId, deathInfo.endingDesc);
                yield break;
            }
        }
        
        Debug.Log($"[EndingManager] 所有 {deathList.Count} 个死亡都被免死道具阻止，游戏继续");
    }

    // ===== 询问玩家是否使用免死道具 =====
    private IEnumerator AskDeathImmunityChoice(PolicyItem item, int deathType)
    {
        waitingForDeathImmunityChoice = true;
        
        UIManager.Instance?.ShowDeathImmunityPrompt(item, deathType);

        while (waitingForDeathImmunityChoice)
        {
            yield return null;
        }

        if (deathImmunityChoiceResult)
        {
            // 消耗道具
            if (item.usageCount > 0) item.usageCount--;
            if (item.usageCount == 0)
            {
                PolicyInventory.Instance?.RemovePolicy(item.id);
            }

            // 恢复数值到50
            RestoreStatByDeathType(deathType);
            
            UIManager.Instance?.UpdateStatText();
            
            if (!string.IsNullOrEmpty(item.deathEffectText))
            {
                UIManager.Instance?.ShowDeathImmunityMessage(item.deathEffectText);
            }
        }
    }

    // ===== 根据死亡类型恢复数值 =====
    private void RestoreStatByDeathType(int deathType)
    {
        switch (deathType)
        {
            case 1:
            case -1:
                stats.king = stats.kingMin + (stats.kingMax - stats.kingMin) / 2;
                Debug.Log($"[EndingManager] 免死道具生效：国君恢复到 一半");
                break;
            case 2:
            case -2:
                stats.scholar = stats.scholarMin + (stats.scholarMax - stats.scholarMin) / 2;
                Debug.Log($"[EndingManager] 免死道具生效：卿士恢复到 一半");
                break;
            case 3:
            case -3:
                stats.noble = stats.nobleMin + (stats.nobleMax - stats.nobleMin) / 2;
                Debug.Log($"[EndingManager] 免死道具生效：贵族恢复到 一半");
                break;
            case 4:
            case -4:
                stats.foreign = stats.foreignMin + (stats.foreignMin - stats.foreignMax) / 2;
                Debug.Log($"[EndingManager] 免死道具生效：外臣恢复到 一半");
                break;
            case 5:
            case -5:
                stats.people = stats.peopleMin + (stats.peopleMax - stats.peopleMin) / 2;
                Debug.Log($"[EndingManager] 免死道具生效：庶人恢复到 一半");
                break;
            case 6:
                stats.king = stats.kingMin + (stats.kingMax - stats.kingMin) / 2;
                stats.noble = stats.nobleMin + (stats.nobleMax - stats.nobleMin) / 2;
                stats.scholar = stats.scholarMin + (stats.scholarMax - stats.scholarMin) / 2;
                stats.foreign = stats.foreignMin + (stats.foreignMax - stats.foreignMin) / 2;
                stats.people = stats.peopleMin + (stats.peopleMax - stats.peopleMin) / 2;
                Debug.Log($"[EndingManager] 免死道具生效：事件死亡，所有数值恢复到 一半");
                break;
        }
    }

    // ===== 玩家选择使用免死道具 =====
    public void OnDeathImmunityUse()
    {
        deathImmunityChoiceResult = true;
        waitingForDeathImmunityChoice = false;
    }

    // ===== 玩家选择不使用免死道具 =====
    public void OnDeathImmunityDecline()
    {
        deathImmunityChoiceResult = false;
        waitingForDeathImmunityChoice = false;
    }

    // ===== 触发结局 =====
    public void TriggerEnding(string endingId, string endingDescription)
    {
        if (endingTriggered) return;

        //全局数据收集
        GameControl.Instance.gameStatistics.GameEnd();
        
        endingTriggered = true;
        GameLifecycleManager.Instance.GameOver = true;

        Debug.Log($"[EndingManager] 结局触发: {endingId} - {endingDescription}");
        
        // 设置所有 StatEffectController 为游戏结束状态
        StatEffectController[] controllers = Object.FindObjectsOfType<StatEffectController>();
        foreach (var controller in controllers)
        {
            controller.SetGameOver(true);
        }
        Debug.Log($"[EndingManager] 已设置 {controllers.Length} 个 StatEffectController 为游戏结束状态");

        UIManager.Instance?.jinYan?.SetActive(false);
        MusicManager.Instance?.PlayDeathSound();

        // 累计货币
        stats.currency += TurnManager.Instance.year;
        Debug.Log($"[EndingManager] 本局存活 {TurnManager.Instance.year} 年，累计货币: {stats.currency}");

        // 重置数值（保留货币和道具，清除BUFF）
        int savedCurrency = stats.currency;
        List<PolicyItem> savedPolicies = new List<PolicyItem>(stats.policyBag);
        stats.ResetToDefault();
        stats.currency = savedCurrency;
        stats.policyBag = savedPolicies;
        PolicyShopUI.Instance?.reRefreshCount(0);
        
        Debug.Log($"[EndingManager] 死亡时重置数值，保留货币 {savedCurrency} 和道具 {savedPolicies.Count} 个，清除所有BUFF");

        // 生成新商店道具
        PolicyManager.Instance?.GenerateShopItems(stats.policyShopCount);

        StartCoroutine(ShowEndingAfterDelay(endingId, endingDescription, TurnManager.Instance.year));
    }

    // ===== 延迟显示结局面板 =====
    private IEnumerator ShowEndingAfterDelay(string endingId, string endingDescription, int survivedYears)
    {
        // 等待2秒，让最后的数值变化动画播放完成
        yield return new WaitForSeconds(2f);

        MusicManager.Instance?.PlayEndingMusic();
        UIManager.Instance?.ShowEndingPanel(endingId, endingDescription, survivedYears);
    }

    // ===== 死亡信息结构 =====
    private class DeathInfo
    {
        public int deathType;
        public string endingId;
        public string endingDesc;
        
        public DeathInfo(int type, string id, string desc)
        {
            deathType = type;
            endingId = id;
            endingDesc = desc;
        }
    }
}
