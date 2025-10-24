using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FontShadow : MonoBehaviour
{
    public Text mainText;
    private Text shadowText;
    // Start is called before the first frame update
    void Start()
    {
        shadowText = GetComponent<Text>();
        if (mainText == null)
        {
            Debug.LogWarning("[FontShadow] mainText 未设置！");
            return;
        } 
    }

    // Update is called once per frame
    void Update()
    {
        if (mainText != null && shadowText != null)
        {
            shadowText.text = mainText.text;
        }
    }
}
