using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 天赋按钮脚本，处理鼠标悬停和点击事件
/// </summary>
public class TalentButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("天赋配置")]
    [Tooltip("该按钮对应的天赋ID")]
    public string talentId;
    
    [Header("UI引用")]
    [Tooltip("激活状态显示的子物体（激活后显示）")]
    public GameObject activatedIndicator;
    
    [Tooltip("天赋信息显示UI")]
    public TalentInfoUI talentInfoUI;
    
    [Header("按钮状态")]
    public int normalStat = 0;
    public int canActivateStat = 2;
    public int cannotActivateStat = -1;
    public int activatedStat = 1;
    
    private int currentStat = 0;
    private Talent talent;
    private bool isInitialized = false;
    
    void Start()
    {
        talentInfoUI = UIManager.Instance?.talentTooltipPanel?.GetComponent<TalentInfoUI>();
        activatedIndicator = transform.GetChild(1).gameObject; // 第二个子物体是激活指示器
        InitializeTalent();
    }
    
    void Update()
    {
        // 每帧更新按钮状态颜色
        if (isInitialized)
        {
            UpdateButtonColor();
        }
    }
    
    /// <summary>
    /// 初始化天赋数据
    /// </summary>
    private void InitializeTalent()
    {
        if (string.IsNullOrEmpty(talentId))
        {
            Debug.LogError($"天赋按钮 {gameObject.name} 未设置 talentId！");
            return;
        }
        
        // 检查TalentLoader是否存在
        if (TalentLoader.Instance == null)
        {
            Debug.LogError($"TalentLoader.Instance 为空！请确保场景中有 TalentLoader 对象。");
            return;
        }
        
        // 从TalentLoader获取天赋数据
        talent = TalentLoader.Instance.GetTalentById(talentId);
        if (talent == null)
        {
            Debug.LogError($"找不到ID为 {talentId} 的天赋！");
            return;
        }
        
        // 如果没有指定TalentInfoUI，尝试在场景中查找
        if (talentInfoUI == null)
        {
            talentInfoUI = FindObjectOfType<TalentInfoUI>();
        }
        
        // 检查TalantManager是否存在
        if (TalantManager.Instance == null)
        {
            Debug.LogError($"TalantManager.Instance 为空！请确保场景中有 TalantManager 对象。");
            return;
        }
        
        // 初始化激活状态显示
        if (activatedIndicator != null)
        {
            activatedIndicator.SetActive(TalantManager.Instance.IsTalentActivated(talentId));
        }
        
        isInitialized = true;
    }
    
    /// <summary>
    /// 更新按钮状态
    /// </summary>
    private void UpdateButtonColor()
    {
        if (TalantManager.Instance == null) return;
        
        // 如果已激活，显示激活状态
        if (TalantManager.Instance.IsTalentActivated(talentId))
        {
            currentStat = activatedStat;
            return;
        }
        
        // 检查是否可以激活
        if (TalantManager.Instance.CanActivateTalent(talent))
        {
            currentStat= canActivateStat;
        }
        else
        {
            currentStat = cannotActivateStat;
        }
    }
    
    /// <summary>
    /// 鼠标进入时显示天赋信息
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized || talent == null) return;
        
        if (talentInfoUI != null)
        {
            talentInfoUI.ShowTalentInfo(talent);
        }
    }
    
    /// <summary>
    /// 鼠标离开时隐藏天赋信息
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (talentInfoUI != null)
        {
            talentInfoUI.HideTalentInfo();
        }
    }
    
    /// <summary>
    /// 点击按钮尝试激活天赋
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInitialized || talent == null || TalantManager.Instance == null) return;
        
        // 如果已经激活，不做处理
        if (TalantManager.Instance.IsTalentActivated(talentId))
        {
            Debug.Log($"天赋 {talent.name} 已经激活过了！");
            return;
        }
        
        // 尝试激活天赋
        bool success = TalantManager.Instance.ActivateTalent(talent);
        
        if (success)
        {
            // 激活成功，显示激活指示器
            if (activatedIndicator != null)
            {
                activatedIndicator.SetActive(true);
            }
            
            // 播放成功音效（如果有）
            Debug.Log($"成功激活天赋：{talent.name}");
            
            // 更新天赋信息显示
            if (talentInfoUI != null)
            {
                talentInfoUI.ShowTalentInfo(talent);
            }
        }
        else
        {
            // 激活失败，显示提示
            ShowActivationFailedMessage();
        }
    }
    
    /// <summary>
    /// 显示激活失败的提示信息
    /// </summary>
    private void ShowActivationFailedMessage()
    {
        string reason = GetActivationFailureReason();
        Debug.Log($"无法激活天赋 {talent.name}：{reason}");
        
        // 这里可以显示UI提示，比如弹出一个小提示框
        // 暂时用 Debug.Log 代替
        // TODO: 添加UI提示功能
    }
    
    /// <summary>
    /// 获取激活失败的原因
    /// </summary>
    private string GetActivationFailureReason()
    {
        // 检查天赋点
        if (TalantManager.Instance.currentTalentPoints < talent.cost)
        {
            return $"天赋点不足（需要{talent.cost}点，当前{TalantManager.Instance.currentTalentPoints}点）";
        }
        
        // 检查前置天赋
        if (talent.preTalentObjects != null && talent.preTalentObjects.Count > 0)
        {
            foreach (var preTalent in talent.preTalentObjects)
            {
                if (!TalantManager.Instance.IsTalentActivated(preTalent.id))
                {
                    return $"需要先激活前置天赋：{preTalent.name}";
                }
            }
        }
        
        return "未满足激活条件";
    }
}
