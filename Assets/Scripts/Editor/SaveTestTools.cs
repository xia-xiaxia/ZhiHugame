using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 游戏测试工具：快速重置存档数据
/// 仅用于开发测试，不会包含在正式版本中
/// </summary>
public class SaveTestTools : EditorWindow
{
    private int testYear = 0;
    private int testCurrency = 0;
    private int testKing = 50;
    private int testNoble = 50;
    private int testScholar = 50;
    private int testForeign = 50;
    private int testPeople = 50;
    
    private Vector2 scrollPos;
    
    [MenuItem("工具/存档测试工具")]
    public static void ShowWindow()
    {
        SaveTestTools window = GetWindow<SaveTestTools>("存档测试工具");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }
    
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("存档测试工具", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("此工具仅用于开发测试，可以快速修改或重置存档数据。", MessageType.Info);
        
        GUILayout.Space(10);
        
        // 快捷操作区
        EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("重置为初始值", GUILayout.Height(30)))
        {
            ResetToDefault();
        }
        if (GUILayout.Button("删除存档文件", GUILayout.Height(30)))
        {
            DeleteSaveFile();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("设置为测试极限值（高）", GUILayout.Height(30)))
        {
            SetToHighLimit();
        }
        if (GUILayout.Button("设置为测试极限值（低）", GUILayout.Height(30)))
        {
            SetToLowLimit();
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("设置为临界值测试", GUILayout.Height(30)))
        {
            SetToCriticalValues();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("自定义数值", EditorStyles.boldLabel);
        
        // 基础数据
        testYear = EditorGUILayout.IntField("年份", testYear);
        testCurrency = EditorGUILayout.IntField("货币", testCurrency);
        
        GUILayout.Space(5);
        EditorGUILayout.LabelField("五维属性 (正常范围: 20-80)", EditorStyles.miniLabel);
        
        testKing = EditorGUILayout.IntSlider("君主", testKing, 0, 100);
        testNoble = EditorGUILayout.IntSlider("贵族", testNoble, 0, 100);
        testScholar = EditorGUILayout.IntSlider("士族", testScholar, 0, 100);
        testForeign = EditorGUILayout.IntSlider("外臣", testForeign, 0, 100);
        testPeople = EditorGUILayout.IntSlider("国人", testPeople, 0, 100);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("应用自定义数值到存档", GUILayout.Height(35)))
        {
            ApplyCustomValues();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("存档信息", EditorStyles.boldLabel);
        
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        EditorGUILayout.TextField("存档路径", savePath);
        
        if (File.Exists(savePath))
        {
            FileInfo fileInfo = new FileInfo(savePath);
            EditorGUILayout.LabelField("文件大小", $"{fileInfo.Length} 字节");
            EditorGUILayout.LabelField("修改时间", fileInfo.LastWriteTime.ToString());
            
            GUILayout.Space(5);
            if (GUILayout.Button("查看存档内容"))
            {
                ShowSaveContent();
            }
            if (GUILayout.Button("在资源管理器中打开"))
            {
                OpenSaveFolder();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("存档文件不存在，请先运行游戏生成存档", MessageType.Warning);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    /// <summary>
    /// 重置为游戏初始值
    /// </summary>
    private void ResetToDefault()
    {
        if (!EditorUtility.DisplayDialog("重置存档", 
            "确定要重置存档为初始值吗？\n年份=0, 五维属性=50", "确定", "取消"))
        {
            return;
        }
        
        testYear = 0;
        testCurrency = 0;
        testKing = 50;
        testNoble = 50;
        testScholar = 50;
        testForeign = 50;
        testPeople = 50;
        
        ApplyCustomValues();
        Debug.Log("[测试工具] 存档已重置为初始值");
    }
    
    /// <summary>
    /// 设置为高极限值（测试上限失败）
    /// </summary>
    private void SetToHighLimit()
    {
        if (!EditorUtility.DisplayDialog("设置极限值", 
            "将所有属性设置为接近上限的值（79）\n用于测试上限失败机制", "确定", "取消"))
        {
            return;
        }
        
        testYear = 5;
        testKing = 79;
        testNoble = 79;
        testScholar = 79;
        testForeign = 79;
        testPeople = 79;
        
        ApplyCustomValues();
        Debug.Log("[测试工具] 已设置为高极限值（79）");
    }
    
    /// <summary>
    /// 设置为低极限值（测试下限失败）
    /// </summary>
    private void SetToLowLimit()
    {
        if (!EditorUtility.DisplayDialog("设置极限值", 
            "将所有属性设置为接近下限的值（21）\n用于测试下限失败机制", "确定", "取消"))
        {
            return;
        }
        
        testYear = 5;
        testKing = 21;
        testNoble = 21;
        testScholar = 21;
        testForeign = 21;
        testPeople = 21;
        
        ApplyCustomValues();
        Debug.Log("[测试工具] 已设置为低极限值（21）");
    }
    
    /// <summary>
    /// 设置为临界值（部分高部分低）
    /// </summary>
    private void SetToCriticalValues()
    {
        if (!EditorUtility.DisplayDialog("设置临界值", 
            "设置混合极限值用于测试：\n君主=79, 贵族=21, 士族=50, 外臣=79, 国人=21", "确定", "取消"))
        {
            return;
        }
        
        testYear = 10;
        testKing = 79;
        testNoble = 21;
        testScholar = 50;
        testForeign = 79;
        testPeople = 21;
        
        ApplyCustomValues();
        Debug.Log("[测试工具] 已设置为临界值（混合高低）");
    }
    
    /// <summary>
    /// 应用自定义数值到存档
    /// </summary>
    private void ApplyCustomValues()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        
        try
        {
            // 创建存档数据
            SaveData saveData = new SaveData
            {
                year = testYear,
                currency = testCurrency,
                king = testKing,
                noble = testNoble,
                scholar = testScholar,
                foreign = testForeign,
                people = testPeople,
                kingMin = 20,
                kingMax = 80,
                nobleMin = 20,
                nobleMax = 80,
                scholarMin = 20,
                scholarMax = 80,
                foreignMin = 20,
                foreignMax = 80,
                peopleMin = 20,
                peopleMax = 80,
                saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                gameVersion = "1.0.0 [测试版本]"
            };
            
            // 序列化为JSON
            string json = JsonUtility.ToJson(saveData, true);
            
            // 写入文件
            File.WriteAllText(savePath, json);
            
            Debug.Log($"[测试工具] 存档已更新:\n年份={testYear}, 君主={testKing}, 贵族={testNoble}, 士族={testScholar}, 外臣={testForeign}, 国人={testPeople}");
            EditorUtility.DisplayDialog("成功", "存档数值已更新！\n重新运行游戏即可看到效果", "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[测试工具] 更新存档失败: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"更新存档失败:\n{e.Message}", "确定");
        }
    }
    
    /// <summary>
    /// 删除存档文件
    /// </summary>
    private void DeleteSaveFile()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        
        if (!File.Exists(savePath))
        {
            EditorUtility.DisplayDialog("提示", "存档文件不存在", "确定");
            return;
        }
        
        if (!EditorUtility.DisplayDialog("删除存档", 
            "确定要删除存档文件吗？\n此操作不可恢复！", "删除", "取消"))
        {
            return;
        }
        
        try
        {
            File.Delete(savePath);
            Debug.Log("[测试工具] 存档文件已删除");
            EditorUtility.DisplayDialog("成功", "存档文件已删除", "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[测试工具] 删除失败: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"删除失败:\n{e.Message}", "确定");
        }
    }
    
    /// <summary>
    /// 查看存档内容
    /// </summary>
    private void ShowSaveContent()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        
        if (File.Exists(savePath))
        {
            string content = File.ReadAllText(savePath);
            Debug.Log($"[测试工具] 存档内容:\n{content}");
            EditorUtility.DisplayDialog("存档内容", "存档内容已输出到 Console 窗口", "确定");
        }
    }
    
    /// <summary>
    /// 在资源管理器中打开存档文件夹
    /// </summary>
    private void OpenSaveFolder()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        string folderPath = Path.GetDirectoryName(savePath);
        
        if (Directory.Exists(folderPath))
        {
            #if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", folderPath.Replace("/", "\\"));
            #elif UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("open", folderPath);
            #endif
            Debug.Log($"[测试工具] 打开文件夹: {folderPath}");
        }
    }
}
