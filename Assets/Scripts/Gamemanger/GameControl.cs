using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour
{
    public static GameControl Instance;
    public StatModel stats;  // 统计数据
    public bool GameOver = false;

    public bool IsCompleteTask = false;
    void Awake()
    {
        Instance = this;
    }

    public void CompleteTask()
    {
        IsCompleteTask = true;
        Debug.Log("任务完成！");
        stats.year += 1;
        if (stats.year > 3)
        {
            GameOver = true;
            Debug.Log("游戏结束！");   
        }
    }
}
