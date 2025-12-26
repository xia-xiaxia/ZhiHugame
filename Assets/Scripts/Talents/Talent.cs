using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Talent
{
    public string id;
    public string name;
    [TextArea] public string description;
    public int cost;

    public List<string> preTanlents; 

    // [NonSerialized] 防止 JsonUtility 试图去序列化它导致死循环或报错
    [NonSerialized] 
    public List<Talent> preTalentObjects = new List<Talent>();

    public TalentEffect talentEffect = new TalentEffect();
}

[Serializable]
public class TalentEffect
{
    public int kingLimit;
    public int nobleLimit;
    public int scholarLimit;
    public int foreignLimit;
    public int peopleLimit;
    public int policyShopCount;
    public int[] refreshPolicyShopCost;
    public float shopMult;
    public float currencyMult;
    public int policyBagSizeChange;
    public int payBackCurrency;
}

[Serializable]
public class TalentRoot
{
    public List<Talent> talents = new List<Talent>();
}