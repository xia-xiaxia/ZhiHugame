using UnityEngine;

/// <summary>
/// 新手教程调试助手
/// 帮助排查教程播放失败的问题
/// </summary>
public class TutorialDebugHelper : MonoBehaviour
{
    public TutorialManager tutorialManager;
    
    [Header("调试按钮")]
    public KeyCode debugKey = KeyCode.F1;
    
    private void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            CheckTutorialSetup();
        }
    }
    
    [ContextMenu("检查教程配置")]
    public void CheckTutorialSetup()
    {
        Debug.Log("========== 新手教程配置检查 ==========");
        
        if (tutorialManager == null)
        {
            Debug.LogError("❌ TutorialManager 未绑定！");
            return;
        }
        
        Debug.Log($"✅ TutorialManager 已绑定: {tutorialManager.name}");
        
        // 检查教程图片
        if (tutorialManager.tutorialImages == null || tutorialManager.tutorialImages.Count == 0)
        {
            Debug.LogError("❌ 教程图片列表为空！请在 Inspector 中添加教程图片");
        }
        else
        {
            Debug.Log($"✅ 教程图片数量: {tutorialManager.tutorialImages.Count}");
            for (int i = 0; i < tutorialManager.tutorialImages.Count; i++)
            {
                if (tutorialManager.tutorialImages[i] == null)
                {
                    Debug.LogError($"❌ 第 {i + 1} 张图片为空！");
                }
                else
                {
                    Debug.Log($"  ✅ 图片 {i + 1}: {tutorialManager.tutorialImages[i].name}");
                }
            }
        }
        
        // 检查UI组件
        if (tutorialManager.tutorialPanel == null)
        {
            Debug.LogError("❌ tutorialPanel 未绑定！");
        }
        else
        {
            Debug.Log($"✅ tutorialPanel: {tutorialManager.tutorialPanel.name}");
            Debug.Log($"   当前状态: {(tutorialManager.tutorialPanel.activeSelf ? "显示" : "隐藏")}");
        }
        
        if (tutorialManager.tutorialImageDisplay == null)
        {
            Debug.LogError("❌ tutorialImageDisplay 未绑定！");
        }
        else
        {
            Debug.Log($"✅ tutorialImageDisplay: {tutorialManager.tutorialImageDisplay.name}");
        }
        
        if (tutorialManager.tutorialCanvasGroup == null)
        {
            Debug.LogWarning("⚠️ tutorialCanvasGroup 未绑定（可选，但建议添加以获得淡入淡出效果）");
        }
        else
        {
            Debug.Log($"✅ tutorialCanvasGroup: {tutorialManager.tutorialCanvasGroup.name}");
            Debug.Log($"   当前 Alpha: {tutorialManager.tutorialCanvasGroup.alpha}");
        }
        
        // 检查StatModel
        if (tutorialManager.stats == null)
        {
            Debug.LogError("❌ StatModel 未绑定！");
        }
        else
        {
            Debug.Log($"✅ StatModel: {tutorialManager.stats.name}");
            Debug.Log($"   hasSeenTutorial: {tutorialManager.stats.hasSeenTutorial}");
        }
        
        // 检查开始界面
        if (tutorialManager.startMenuPanel == null)
        {
            Debug.LogWarning("⚠️ startMenuPanel 未绑定");
        }
        else
        {
            Debug.Log($"✅ startMenuPanel: {tutorialManager.startMenuPanel.name}");
            Debug.Log($"   当前状态: {(tutorialManager.startMenuPanel.activeSelf ? "显示" : "隐藏")}");
        }
        
        Debug.Log("========================================");
    }
    
    [ContextMenu("强制播放教程")]
    public void ForcePlayTutorial()
    {
        if (tutorialManager != null)
        {
            Debug.Log("[调试] 强制播放教程");
            tutorialManager.PlayTutorial();
        }
        else
        {
            Debug.LogError("[调试] TutorialManager 未绑定");
        }
    }
    
    [ContextMenu("重置教程状态")]
    public void ResetTutorialStatus()
    {
        if (tutorialManager != null)
        {
            tutorialManager.ResetTutorialStatus();
            Debug.Log("[调试] 教程状态已重置");
        }
    }
    
    [ContextMenu("检查 TutorialManager Instance")]
    public void CheckInstance()
    {
        if (TutorialManager.Instance == null)
        {
            Debug.LogError("❌ TutorialManager.Instance 为 null！");
        }
        else
        {
            Debug.Log($"✅ TutorialManager.Instance 存在: {TutorialManager.Instance.name}");
        }
    }
}
