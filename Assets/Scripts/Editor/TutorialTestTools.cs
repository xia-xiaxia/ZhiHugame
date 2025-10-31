using UnityEngine;
using UnityEditor;

/// <summary>
/// 新手教程测试工具
/// </summary>
public class TutorialTestTools
{
    [MenuItem("工具/教程/重置教程状态（再次触发首次教程）")]
    public static void ResetTutorialStatus()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ResetTutorialStatus();
            EditorUtility.DisplayDialog("成功", "教程状态已重置\n下次启动游戏会再次播放教程", "确定");
        }
        else
        {
            // 如果游戏没运行，直接修改存档
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");
            
            if (System.IO.File.Exists(savePath))
            {
                string json = System.IO.File.ReadAllText(savePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                
                if (saveData != null)
                {
                    saveData.hasSeenTutorial = false;
                    json = JsonUtility.ToJson(saveData, true);
                    System.IO.File.WriteAllText(savePath, json);
                    
                    Debug.Log("[测试工具] 教程状态已重置（通过存档）");
                    EditorUtility.DisplayDialog("成功", "教程状态已重置\n下次启动游戏会再次播放教程", "确定");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "存档文件不存在\n请先运行游戏生成存档", "确定");
            }
        }
    }
    
    [MenuItem("工具/教程/手动播放教程")]
    public static void PlayTutorial()
    {
        if (Application.isPlaying)
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.PlayTutorial();
            }
            else
            {
                Debug.LogWarning("TutorialManager.Instance 不存在，请确保场景中有 TutorialManager");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "请先运行游戏", "确定");
        }
    }
}
