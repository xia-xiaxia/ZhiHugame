using System.Collections;
using System.Collections.Generic;
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

    //事件判定值
    public bool[] judgeValue = new bool[100];
    public int[] judgeFirstYear = new int[100];
    
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

    public void Restart()
    {
        currentReignYears = 0;
        policyUsageCount.Clear();
        policyFirstYear.Clear();
        for(int i = 0; i < 100; i++)
        {
            judgeValue[i] = false;
            judgeFirstYear[i] = -1;
        }
    }

    public void GameEnd()
    {
        currentReignYears = TurnManager.Instance.year;
        totalReginYears += TurnManager.Instance.year;
    }

    public void usePolicy(PolicyItem item)
    {
        int key = int.Parse(item.id);
        policyUsageCount[key] = policyUsageCount.GetValueOrDefault(key, 0) + 1;
        policyFirstYear.TryAdd(key, TurnManager.Instance.year);
        if(item.usageCount == 1)
        {
            policyUseOutCount++;
        }
    }

    public void setJudgeValue(int id)
    {
        if (judgeValue[id] == false)
        {
            judgeValue[id] = true;
            judgeFirstYear[id] = TurnManager.Instance.year;
        }
    }

    public void setJudgeValue(List<int> ids)
    {
        foreach (int id in ids)
        {
            if (judgeValue[id] == false)
            {
                judgeValue[id] = true;
                judgeFirstYear[id] = TurnManager.Instance.year;
            }
        }
    }
}
