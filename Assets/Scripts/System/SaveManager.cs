using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


/// <summary>
/// 存档管理器
/// 负责游戏数据的保存和加载
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    
    [Header("存档配置")]
    public StatModel stats;  // 关联的StatModel
    public bool autoSaveOnExit = true;  // 退出时自动保存
    public bool autoLoadOnStart = true;  // 启动时自动加载
    
    private string saveFilePath;
    private const string SAVE_FILE_NAME = "savegame.json";
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 设置存档文件路径
        saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        Debug.Log($"[SaveManager] 存档路径: {saveFilePath}");

        //初始化GameStatistics
        GameControl.Instance.gameStatistics.Inititalize();

        // 启动时自动加载存档
        if (autoLoadOnStart)
        {
            LoadGame();
        }
    }
    
    private void Start()
    {

    }
    
    private void OnApplicationQuit()
    {
        // 退出时自动保存
        if (autoSaveOnExit)
        {
            Debug.Log("[SaveManager] 应用程序退出，自动保存游戏");
            SaveGame();
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        // 在移动平台上，暂停时也保存
        #if UNITY_ANDROID || UNITY_IOS
        if (pauseStatus && autoSaveOnExit)
        {
            Debug.Log("[SaveManager] 应用程序暂停，自动保存游戏");
            SaveGame();
        }
        #endif
    }
    
    /// <summary>
    /// 保存游戏
    /// </summary>
    public bool SaveGame()
    {
        if (stats == null)
        {
            Debug.LogError("[SaveManager] StatModel 未绑定，无法保存");
            return false;
        }
        
        try
        {
            // 从StatModel创建存档数据（包含事件使用状态和延时事件）
            SaveData saveData = SaveData.FromStatModel(stats);
            
            // 序列化为JSON
            string json = JsonUtility.ToJson(saveData, true);
            
            // 写入文件
            File.WriteAllText(saveFilePath, json);
            
            Debug.Log($"[SaveManager] 游戏已保存: {saveFilePath}");
            Debug.Log($"[SaveManager] 存档内容: 年份={saveData.year}, 君主={saveData.king}");
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] 保存失败: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// 加载游戏
    /// </summary>
    public bool LoadGame()
    {
        if (stats == null)
        {
            Debug.LogError("[SaveManager] StatModel 未绑定，无法加载");
            return false;
        }
        
        // 检查存档文件是否存在
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning($"[SaveManager] 存档文件不存在: {saveFilePath}，将进行完全初始化");
            
            // 完全重置游戏状态（包括天赋、背包等）
            if (stats != null)
            {
                stats.ResetToDefaultCompletely();
            }
            
            // 重置GameStatistics
            if (GameControl.Instance != null && GameControl.Instance.gameStatistics != null)
            {
                GameControl.Instance.gameStatistics.Inititalize();
            }
            
            // 重置事件数据库
            if (EventDatabase.Instance != null)
            {
                EventDatabase.Instance.ResetPool();
            }
            
            // 清空延时事件
            if (EventSelector.Instance != null)
            {
                EventSelector.Instance.Reset();
            }
            
            // 通知UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateStatText();
            }
            
            Debug.Log("[SaveManager] 完全初始化完成");
            return false;
        }
        
        try
        {
            // 读取文件
            string json = File.ReadAllText(saveFilePath);
            
            // 反序列化
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            
            if (saveData == null)
            {
                Debug.LogError("[SaveManager] 反序列化失败，saveData 为 null");
                return false;
            }
            
            // 应用到StatModel
            saveData.ApplyToStatModel(stats);
            saveData.ApplyToGameStatisics(GameControl.Instance.gameStatistics);
            
            // 恢复事件使用状态
            if (EventDatabase.Instance != null && saveData.usedEvents != null)
            {
                List<string> usedIds = new List<string>();
                foreach (var usedEvent in saveData.usedEvents)
                {
                    if (!string.IsNullOrEmpty(usedEvent.eventId))
                    {
                        usedIds.Add(usedEvent.eventId);
                    }
                }
                
                // 先恢复事件集激活状态（必须在恢复已使用事件之前）
                if (saveData.activeRandomEventSetIndices != null && saveData.activeRandomEventSetIndices.Count > 0)
                {
                    EventDatabase.Instance.RestoreActiveEventSet(saveData.activeRandomEventSetIndices);
                }
                
                // 再恢复已使用的事件
                EventDatabase.Instance.RestoreUsedEvents(usedIds);
            }
            
            // 恢复延时事件
            if (EventSelector.Instance != null && saveData.delayedEventQueue != null)
            {
                EventSelector.Instance.LoadFromSave(saveData.delayedEventQueue);
            }
            
            Debug.Log($"[SaveManager] 游戏已加载: 年份={saveData.year}, 存档时间={saveData.saveTime}");
            
            // 通知UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateStatText();
            }
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] 加载失败: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// 删除存档
    /// </summary>
    public bool DeleteSave()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                Debug.Log("[SaveManager] 存档已删除");
                return true;
            }
            else
            {
                Debug.LogWarning("[SaveManager] 存档文件不存在，无需删除");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] 删除存档失败: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查是否存在存档
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }
    
    /// <summary>
    /// 获取存档信息（不完全加载）
    /// </summary>
    public SaveData GetSaveInfo()
    {
        if (!File.Exists(saveFilePath))
        {
            return null;
        }
        
        try
        {
            string json = File.ReadAllText(saveFilePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] 读取存档信息失败: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 新游戏（重置并删除存档）
    /// </summary>
    public void NewGame()
    {
        // 删除旧存档
        DeleteSave();
        
        if (stats != null)
        {
            stats.ResetToDefaultCompletely();
            Debug.Log("[SaveManager] 开始新游戏，数据已完全重置");
        }
        
        // 重置GameStatistics
        if (GameControl.Instance != null && GameControl.Instance.gameStatistics != null)
        {
            GameControl.Instance.gameStatistics.Inititalize();
        }
        
        // 重置事件数据库
        if (EventDatabase.Instance != null)
        {
            EventDatabase.Instance.ResetPool();
        }
        
        // 清空延时事件
        if (EventSelector.Instance != null)
        {
            EventSelector.Instance.Reset();
        }
        
        // 通知UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStatText();
        }
    }
    
    /// <summary>
    /// 手动保存（给UI按钮调用）
    /// </summary>
    public void ManualSave()
    {
        if (SaveGame())
        {
            // 可以显示保存成功的提示
            Debug.Log("[SaveManager] 手动保存成功");
        }
    }
    
    /// <summary>
    /// 手动加载（给UI按钮调用）
    /// </summary>
    public void ManualLoad()
    {
        if (LoadGame())
        {
            // 可以显示加载成功的提示
            Debug.Log("[SaveManager] 手动加载成功");
        }
    }
    
    /// <summary>
    /// 获取存档文件路径
    /// </summary>
    public string GetSaveFilePath()
    {
        return saveFilePath;
    }
    
    /// <summary>
    /// 在文件资源管理器中打开存档文件夹
    /// </summary>
    public void OpenSaveFolder()
    {
        string folderPath = Path.GetDirectoryName(saveFilePath);
        
        if (Directory.Exists(folderPath))
        {
            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            System.Diagnostics.Process.Start("explorer.exe", folderPath.Replace("/", "\\"));
            #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            System.Diagnostics.Process.Start("open", folderPath);
            #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            System.Diagnostics.Process.Start("xdg-open", folderPath);
            #endif
            
            Debug.Log($"[SaveManager] 打开文件夹: {folderPath}");
        }
        else
        {
            Debug.LogWarning($"[SaveManager] 文件夹不存在: {folderPath}");
        }
    }
    
    /// <summary>
    /// 复制存档路径到剪贴板
    /// </summary>
    public void CopySavePathToClipboard()
    {
        GUIUtility.systemCopyBuffer = saveFilePath;
        Debug.Log($"[SaveManager] 已复制存档路径到剪贴板: {saveFilePath}");
    }
}
