using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor 工具：快速访问存档文件
/// </summary>
public class SaveFileEditorTools
{
    [MenuItem("工具/存档/打开存档文件夹")]
    public static void OpenSaveFolder()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OpenSaveFolder();
        }
        else
        {
            // 如果游戏没运行，计算路径并打开
            string path = System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");
            string folderPath = System.IO.Path.GetDirectoryName(path);
            
            if (System.IO.Directory.Exists(folderPath))
            {
                #if UNITY_EDITOR_WIN
                System.Diagnostics.Process.Start("explorer.exe", folderPath.Replace("/", "\\"));
                #elif UNITY_EDITOR_OSX
                System.Diagnostics.Process.Start("open", folderPath);
                #endif
                Debug.Log($"打开文件夹: {folderPath}");
            }
            else
            {
                Debug.LogWarning($"文件夹不存在: {folderPath}\n请先运行游戏生成存档文件");
            }
        }
    }
    
    [MenuItem("工具/存档/复制存档路径")]
    public static void CopySavePath()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");
        GUIUtility.systemCopyBuffer = path;
        Debug.Log($"已复制存档路径到剪贴板:\n{path}");
    }
    
    [MenuItem("工具/存档/删除存档文件")]
    public static void DeleteSaveFile()
    {
        if (SaveManager.Instance != null)
        {
            if (EditorUtility.DisplayDialog("删除存档", "确定要删除存档文件吗？", "删除", "取消"))
            {
                SaveManager.Instance.DeleteSave();
            }
        }
        else
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");
            if (System.IO.File.Exists(path))
            {
                if (EditorUtility.DisplayDialog("删除存档", $"确定要删除存档文件吗？\n{path}", "删除", "取消"))
                {
                    System.IO.File.Delete(path);
                    Debug.Log("存档文件已删除");
                }
            }
            else
            {
                Debug.LogWarning("存档文件不存在");
            }
        }
    }
    
    [MenuItem("工具/存档/显示存档信息")]
    public static void ShowSaveInfo()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");
        
        if (System.IO.File.Exists(path))
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            string info = $"存档文件信息:\n" +
                         $"路径: {path}\n" +
                         $"大小: {fileInfo.Length} 字节\n" +
                         $"创建时间: {fileInfo.CreationTime}\n" +
                         $"修改时间: {fileInfo.LastWriteTime}";
            
            Debug.Log(info);
            EditorUtility.DisplayDialog("存档信息", info, "确定");
        }
        else
        {
            Debug.LogWarning("存档文件不存在");
            EditorUtility.DisplayDialog("存档信息", "存档文件不存在\n请先运行游戏生成存档", "确定");
        }
    }
}
