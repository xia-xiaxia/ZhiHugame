using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalentDebug : MonoBehaviour
{
    
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y))
        {
            TalantManager.Instance.AddTalentPoints(5);
        }
    }
}
