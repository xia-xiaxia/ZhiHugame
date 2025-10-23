using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Option
{
    public string text;
    public int kingChange;      // 国君
    public int nobleChange;     // 贵族
    public int scholarChange;   // 士族
    public int foreignChange;   // 外臣
    public int peopleChange;    // 国人

    public string specialChange;
    public string nextEventId;
    public int interval;
    public int randomEventSet;
    public string activateBUFF; // 激活BUFF
}


