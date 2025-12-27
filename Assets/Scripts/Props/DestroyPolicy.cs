using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPolicy : MonoBehaviour
{
    // 公共字段，由外部设置（例如在UIManager创建按钮时）
    [HideInInspector]
    public PolicyItem policyItem;
    private int abandonPolicyAndGetCurrency = 0;

    void Start()
    {
        // 如果 policyItem 未设置，输出警告
        if (policyItem == null)
        {
            Debug.LogWarning("DestroyPolicy: policyItem 未设置！");
        }
    }

    void Update()
    {

    }
    
    public void destroyThis()
    {
        if (policyItem != null)
        {
            abandonPolicyAndGetCurrency = GameControl.Instance?.stats?.payBackCurrency ?? 0;
            CurrencyManager.Instance?.AddCurrency(abandonPolicyAndGetCurrency);
            Debug.Log($"[PolicyInShopTrigger] 道具已丢弃，获得货币：{abandonPolicyAndGetCurrency}");
            GameControl.Instance.RemovePolicy(policyItem.id);
        }
        Destroy(gameObject);
    }
}
