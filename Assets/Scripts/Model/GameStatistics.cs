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

    public Dictionary<string, int> policyUsageCount = new Dictionary<string, int>();
    public Dictionary<string, int> policyFirstYear = new Dictionary<string, int>();

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

    void Restart()
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



}
