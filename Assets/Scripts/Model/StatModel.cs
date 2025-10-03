using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Game/StatModel1")]
public class StatModel1 : ScriptableObject
{
    // 初始值均为 50
    public int king = 50;     // 国君
    public int noble = 50;    // 贵族
    public int scholar = 50;  // 士族
    public int foreign = 50;  // 外臣
    public int people = 50;   // 国人

    // 失败阈值常量
    public const int MIN_LIMIT = 20;
    public const int MAX_LIMIT = 80;

    // 判断是否越界（触发失败）
    public bool IsOutOfBounds()
    {
        return king < MIN_LIMIT || king > MAX_LIMIT
            || noble < MIN_LIMIT || noble > MAX_LIMIT
            || scholar < MIN_LIMIT || scholar > MAX_LIMIT
            || foreign < MIN_LIMIT || foreign > MAX_LIMIT
            || people < MIN_LIMIT || people > MAX_LIMIT;
    }

    // 可调用的重置方法（重开时恢复初始值）
    public void ResetToDefault()
    {
        king = noble = scholar = foreign = people = 50;
    }
}