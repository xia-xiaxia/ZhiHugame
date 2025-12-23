using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Talent
{
    public string id;
    public string name;
    public string description;
    public int cost;
    public List<Talent> preTanlents = new List<Talent>();

    public TalentEffect talentEffect = new TalentEffect();

}

[Serializable]
public class TalentEffect
{
    public int kingLimitChange;
    public int nobleLimitChange;
    public int scholarLimitChange;
    public int foreignLimitChange;
    public int peopleLimitChange;
    public int policyShopCount;
    public int[] refreshPolicyShopCost;
    public float shopMult;
    public float currencyMult;
    public int policyBagSizeChange;
    public int payBackCurrency;
}