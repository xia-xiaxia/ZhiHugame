// FadeTextAlpha.cs
using UnityEngine;
using UnityEngine.UI;               // UI.Text 用这个
// using TMPro;                     // 如果用 TextMeshProUGUI，把上一行注释掉，用这行

public class FadeTextAlpha : MonoBehaviour
{
    [Header("目标组件")]
    [SerializeField] private Graphic targetGraphic;   // 对 Text、TextMeshProUGUI、Image 都通用
    [Header("淡入淡出时间（秒）")]
    [SerializeField] private float fadeDuration = 2f;
    [Header("最暗透明度（0~1）")]
    [SerializeField] private float minAlpha = 0.6f;

    private float direction = -1;   // -1 表示正在变暗，1 表示正在变亮

    void Awake()
    {
        // 如果没有手动拖组件，自动获取
        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();
    }

    void Update()
    {
        // 计算透明度变化
        float delta = (1f / fadeDuration) * Time.deltaTime;
        Color c = targetGraphic.color;
        c.a += delta * direction;

        // 到达边界时反向
        if (c.a <= minAlpha)
        {
            c.a = minAlpha;
            direction = 1;
        }
        else if (c.a >= 1f)
        {
            c.a = 1f;
            direction = -1;
        }

        targetGraphic.color = c;
    }
}