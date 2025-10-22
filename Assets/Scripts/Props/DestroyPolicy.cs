using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPolicy : MonoBehaviour
{
    private GameObject gt;
    private PolicyItem policyItem;
    void Start()
    {
        gt = this.gameObject;
        policyItem = gt.GetComponent<PolicyItem>();
    }

    void Update()
    {

    }
    
    public void destroyThis()
    {
        Destroy(gameObject);
        GameControl.Instance.RemovePolicy(policyItem.id);
    }
}
