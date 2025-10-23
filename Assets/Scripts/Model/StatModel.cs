using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Game/StatModel1")]
public class StatModel : ScriptableObject
{
    // 初始值均为 50
    public int year = 0;      // 当前年份
    public int king = 50;     // 国君
    public int noble = 50;    // 贵族
    public int scholar = 50;  // 士族
    public int foreign = 50;  // 外臣
    public int people = 50;   // 国人

    // 失败阈值常量

    public int kingMin = 20;
    public int kingMax = 80;
    public int nobleMin = 20;
    public int nobleMax = 80;
    public int scholarMin = 20;
    public int scholarMax = 80;
    public int foreignMin = 20;
    public int foreignMax = 80;
    public int peopleMin = 20;
    public int peopleMax = 80;
    // 判断是否越界（触发失败）
    public bool IsOutOfBounds()
    {
        return king < kingMin || king > kingMax
            || noble < nobleMin || noble > nobleMax
            || scholar < scholarMin || scholar > scholarMax
            || foreign < foreignMin || foreign > foreignMax
            || people < peopleMin || people > peopleMax;
    }

    // 可调用的重置方法（重开时恢复初始值）
    public void ResetToDefault()
    {
        year = 0;
        king = noble = scholar = foreign = people = 50;
        kingMin = 20; kingMax = 80;
        nobleMin = 20; nobleMax = 80;
        scholarMin = 20; scholarMax = 80;
        foreignMin = 20; foreignMax = 80;
        peopleMin = 20; peopleMax = 80;
    }
}