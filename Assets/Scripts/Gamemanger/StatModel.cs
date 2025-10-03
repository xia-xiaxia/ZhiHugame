using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Game/StatModel")]
public class StatModel : ScriptableObject
{
    public int turns = 0;
    public int king = 10;
    public int noble = 10;
    public int scholar = 10;
    public int foreign = 10;
    public int people = 10;
    public bool ifjs = false;
    public bool ifzz =false;
    public bool iftl =false;
    public bool ifhm =false;
}
