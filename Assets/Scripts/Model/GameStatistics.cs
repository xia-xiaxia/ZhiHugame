using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(menuName = "Game/GameStatistics")]
public class GameStatistics : ScriptableObject
{
    [SerializeField]
    private int _currentReignYears;
    [SerializeField]
    private int _totalReginYears;
    [SerializeField]
    private int _policyUseOutCount;

    public Dictionary<int, int> policyUsageCount = new Dictionary<int, int>();
    public Dictionary<int, int> policyFirstYear = new Dictionary<int, int>();
    //统计存活x年累计y次
    public Dictionary<string, (int x, int y)> runsWithLongReign = new Dictionary<string, (int x, int y)>();

    // 事件判定相关
    public bool[] judgeValue = new bool[100];
    public int[] judgeFirstYear = new int[100];


    //MissionManager 相关
    public List<int> activeMissions = new List<int>();
    public Dictionary<string, bool> isComplete = new Dictionary<string, bool>();

    public int currentReignYears
    {
        get => _currentReignYears;
        set
        {
            _currentReignYears = value;
        }
    }

    public int totalReginYears
    {
        get => _totalReginYears;
        set
        {
            _totalReginYears = value;
        }
    }

    public int policyUseOutCount
    {
        get => _policyUseOutCount;
        set
        {
            _policyUseOutCount = value;
        }
    }

    public int GetPolicyUsage(int id)
    {
        return policyUsageCount.TryGetValue(id, out int count) ? count : 0;
    }

    public int GetPolicyFirstYear(int id)
    {
        return policyFirstYear.TryGetValue(id, out int year) ? year : -1;
    }

    public void Inititalize()
    {
        activeMissions.Clear();
        isComplete.Clear();
        runsWithLongReign.Clear();
        totalReginYears = 0;
    }

    public void Restart()
    {
        policyUseOutCount = 0;
        currentReignYears = 0;
        policyUsageCount.Clear();
        policyFirstYear.Clear();
        for(int i = 0; i < 100; i++)
        {
            judgeValue[i] = false;
            judgeFirstYear[i] = -1;
        }

        //MissionManager的Restart相关内容
        MissionManager.Instance.StartNewGame();
    }

    public void GameEnd()
    {
        if (TurnManager.Instance != null)
        {
            currentReignYears = TurnManager.Instance.year;
            totalReginYears += TurnManager.Instance.year;
            foreach (var key in runsWithLongReign.Keys.ToList())
            {
                var (limit, count) = runsWithLongReign[key]; 

                if (currentReignYears >= limit)
                {
                    runsWithLongReign[key] = (limit, count + 1);
                }
            }
        }
        else
        {
            Debug.LogWarning("[GameStatistics] TurnManager.Instance 为 null，无法记录年份");
        }
    }

    public void usePolicy(PolicyItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("[GameStatistics] PolicyItem 为 null，忽略 usePolicy 调用");
            return;
        }

        if (!int.TryParse(item.id, out int key))
        {
            Debug.LogWarning($"[GameStatistics] PolicyItem.id 无法解析为整数: {item.id}");
            return;
        }

        policyUsageCount[key] = policyUsageCount.GetValueOrDefault(key, 0) + 1;
        int year = TurnManager.Instance != null ? TurnManager.Instance.year : 0;
        if (!policyFirstYear.ContainsKey(key))
        {
            policyFirstYear[key] = year;
        }

        if (item.usageCount == 1)
        {
            policyUseOutCount++;
        }
    }

    public void setJudgeValue(int[] eventFlags)
    {
        // 输入空安全
        if (eventFlags == null || eventFlags.Length == 0)
        {
            return;
        }

        // 内部数组空安全
        if (judgeValue == null || judgeValue.Length == 0)
        {
            judgeValue = new bool[100];
        }
        if (judgeFirstYear == null || judgeFirstYear.Length == 0)
        {
            judgeFirstYear = new int[100];
            for (int i = 0; i < judgeFirstYear.Length; i++)
            {
                judgeFirstYear[i] = -1;
            }
        }

        foreach (var id in eventFlags)
        {
            int key = Mathf.Abs(id);
            bool flag = id > 0;

            // 边界安全：忽略超出数组长度的索引
            if (key < 0 || key >= judgeValue.Length || key >= judgeFirstYear.Length)
            {
                Debug.LogWarning($"[GameStatistics] eventFlag 索引越界: {key}");
                continue;
            }

            if (flag)
            {
                if (!judgeValue[key])
                {
                    judgeValue[key] = true;
                    judgeFirstYear[key] = TurnManager.Instance != null ? TurnManager.Instance.year : 0;
                }
            }
            else
            {
                if (judgeValue[key])
                {
                    judgeValue[key] = false;
                    judgeFirstYear[key] = -1;
                }
            }
        }
    }

    //统计runsWithLongReign
    public void Register(MissionData m)
    {
        int limit = 0;
        foreach (var c in m.conditions)
        {
            if (c.type == "runsWithLongReign")
            {
                limit = c.paramList[1];
                break;
            }
        }
        if (limit == 0) return; 
        runsWithLongReign[m.id] = (limit, 0);
    }

    public void UnRegister(string id)
    {
        runsWithLongReign.Remove(id);
    }

}
