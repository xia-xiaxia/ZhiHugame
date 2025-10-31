using UnityEngine;

/// <summary>
/// BuffManager调试工具
/// 用于检查BUFF系统是否正确设置和运行
/// </summary>
public class BuffManagerDebug : MonoBehaviour
{
    void Start()
    {
        // 延迟检查，确保所有管理器都已初始化
        Invoke("CheckBuffManager", 1f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            CheckBuffManager();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TestAddBuff001();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestYearEnd();
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ShowActiveBuffs();
        }
    }

    void CheckBuffManager()
    {
        Debug.Log("===== BuffManager 调试检查 =====");
        
        if (BuffManager.Instance == null)
        {
            Debug.LogError("[BuffManagerDebug] BuffManager.Instance 为 null！");
            Debug.LogError("[BuffManagerDebug] 请确保场景中有 BuffManager 对象！");
            return;
        }
        
        Debug.Log("[BuffManagerDebug] BuffManager.Instance 存在 ✓");
        
        if (BuffManager.Instance.buffJson == null)
        {
            Debug.LogError("[BuffManagerDebug] BuffManager.buffJson 未绑定！");
            Debug.LogError("[BuffManagerDebug] 请在 Inspector 中将 buff.json 文件拖到 BuffManager 的 buffJson 字段！");
            return;
        }
        
        Debug.Log("[BuffManagerDebug] buffJson 已绑定 ✓");
        
        if (BuffManager.Instance.buffs == null || BuffManager.Instance.buffs.Count == 0)
        {
            Debug.LogError("[BuffManagerDebug] BUFF定义列表为空！");
            Debug.LogError("[BuffManagerDebug] 可能JSON解析失败或JSON文件为空");
            return;
        }
        
        Debug.Log($"[BuffManagerDebug] 已加载 {BuffManager.Instance.buffs.Count} 个BUFF定义 ✓");
        
        foreach (var buff in BuffManager.Instance.buffs)
        {
            Debug.Log($"[BuffManagerDebug] - BUFF: ID={buff.id}, Name={buff.name}, Duration={buff.duration}");
        }
        
        Debug.Log($"[BuffManagerDebug] 当前激活的BUFF数量: {BuffManager.Instance.GetActiveBuffs().Count}");
        
        Debug.Log("===== BuffManager 检查完成 =====");
    }
    
    // 用于测试添加BUFF（可以在Inspector中通过按钮或其他方式调用）
    [ContextMenu("测试添加BUFF 001")]
    void TestAddBuff001()
    {
        if (BuffManager.Instance != null)
        {
            Debug.Log("[BuffManagerDebug] 测试添加BUFF 001");
            var buff = BuffManager.Instance.AddBuffById("001");
            if (buff != null)
            {
                Debug.Log($"[BuffManagerDebug] 成功添加BUFF: {buff.name}");
            }
        }
    }
    
    [ContextMenu("测试年度结束")]
    void TestYearEnd()
    {
        if (BuffManager.Instance != null)
        {
            Debug.Log("[BuffManagerDebug] 测试调用 OnYearEnd");
            BuffManager.Instance.OnYearEnd();
        }
    }
    
    [ContextMenu("显示当前激活的BUFF")]
    void ShowActiveBuffs()
    {
        if (BuffManager.Instance != null)
        {
            Debug.Log($"[BuffManagerDebug] 当前激活的BUFF数量: {BuffManager.Instance.GetActiveBuffs().Count}");
            foreach (var buff in BuffManager.Instance.GetActiveBuffs())
            {
                Debug.Log($"[BuffManagerDebug] - {buff.name} (ID: {buff.id}, 剩余时限: {buff.duration})");
            }
        }
    }
    
    [ContextMenu("显示BUFF面板UI")]
    void ShowBuffPanel()
    {
        if (UIManager.Instance != null)
        {
            Debug.Log("[BuffManagerDebug] 测试显示BUFF面板");
            UIManager.Instance.ShowBuffPanel();
        }
        else
        {
            Debug.LogError("[BuffManagerDebug] UIManager.Instance 为 null！");
        }
    }
}
