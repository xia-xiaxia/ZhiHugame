using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Game/StatModel1")]
public class StatModel : ScriptableObject
{
    // 初始值均为 50
    public int year = 0;      // 当前年份
    
    // 私有字段
    [SerializeField]
    private int _currency = 0;  // 当前资金（累计）
    [SerializeField]
    private int _king = 50;
    [SerializeField]
    private int _noble = 50;
    [SerializeField]
    private int _scholar = 50;
    [SerializeField]
    private int _foreign = 50;
    [SerializeField]
    private int _people = 50;
    
    // 货币属性（带事件触发）
    public int currency
    {
        get => _currency;
        set
        {
            if (_currency != value)
            {
                _currency = value;
                OnCurrencyChanged?.Invoke();
            }
        }
    }
    
    // 公开属性（带事件触发）
    public int king
    {
        get => _king;
        set
        {
            if (_king != value)
            {
                _king = value;
                OnKingChanged?.Invoke(value);
                OnStatsChanged?.Invoke();
            }
        }
    }
    
    public int noble
    {
        get => _noble;
        set
        {
            if (_noble != value)
            {
                _noble = value;
                OnNobleChanged?.Invoke(value);
                OnStatsChanged?.Invoke();
            }
        }
    }
    
    public int scholar
    {
        get => _scholar;
        set
        {
            if (_scholar != value)
            {
                _scholar = value;
                OnScholarChanged?.Invoke(value);
                OnStatsChanged?.Invoke();
            }
        }
    }
    
    public int foreign
    {
        get => _foreign;
        set
        {
            if (_foreign != value)
            {
                _foreign = value;
                OnForeignChanged?.Invoke(value);
                OnStatsChanged?.Invoke();
            }
        }
    }
    
    public int people
    {
        get => _people;
        set
        {
            if (_people != value)
            {
                _people = value;
                OnPeopleChanged?.Invoke(value);
                OnStatsChanged?.Invoke();
            }
        }
    }

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

    // 计算各属性百分比
    public float Percent => kingMax > 0 ? (king - kingMin) / (kingMax - kingMin) : 0f;
    public float NoblePercent => nobleMax > 0 ? (noble - nobleMin) / (nobleMax - nobleMin) : 0f;
    public float ScholarPercent => scholarMax > 0 ? (scholar - scholarMin) / (scholarMax - scholarMin) : 0f;
    public float ForeignPercent => foreignMax > 0 ? (foreign - foreignMin) / (foreignMax - foreignMin) : 0f;
    public float PeoplePercent => peopleMax > 0 ? (people - peopleMin) / (peopleMax - peopleMin) : 0f;


    // 判断是否越界（触发失败）
    public List<PolicyItem> policyBag = new List<PolicyItem>();
    
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
        _king = _noble = _scholar = _foreign = _people = 50;
        // 通过属性触发事件
        king = 50;
        noble = 50;
        scholar = 50;
        foreign = 50;
        people = 50;
        kingMin = 20; kingMax = 80;
        nobleMin = 20; nobleMax = 80;
        scholarMin = 20; scholarMax = 80;
        foreignMin = 20; foreignMax = 80;
        peopleMin = 20; peopleMax = 80;
    }

    // 事件，当属性变化时触发
    public event System.Action OnStatsChanged;
    public event System.Action OnCurrencyChanged;

    public event System.Action<StatModel> OnPolicyBagChanged;

    public event System.Action<int> OnYearChanged;

    public event System.Action<int> OnKingChanged;
    public event System.Action<int> OnNobleChanged;
    public event System.Action<int> OnScholarChanged;
    public event System.Action<int> OnForeignChanged;
    public event System.Action<int> OnPeopleChanged;

}