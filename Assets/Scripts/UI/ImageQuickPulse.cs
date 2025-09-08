using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageQuickPulse : MonoBehaviour
{
    [Header("动画时长（秒）")]
    public float expandDuration = 0.1f;
    public float shrinkDuration = 0.2f;

    private Image img;
    private Vector3 baseScale;

    private float timer;          // 当前阶段已用时间
    private float duration;       // 当前阶段总时长
    private Vector3 fromScale;    // 当前阶段的起始缩放
    private Vector3 toScale;      // 当前阶段的目标缩放

    void Awake()
    {
        img = GetComponent<Image>();
        baseScale = transform.localScale;
    }

    /// <summary>外部调用：立即开始一次（或打断当前）脉冲</summary>
    public void Pulse()
    {
        // 只要被调用，就用 0.1s 冲到 1.1 倍
        timer = 0;
        duration = expandDuration;
        fromScale = transform.localScale;        // 可能停在 1.0~1.1 的任意值
        toScale = baseScale * 1.1f;
        enabled = true;   // 确保 Update 运行
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, t);

        if (timer >= duration)
        {
            if (toScale == baseScale * 1.1f)   // 刚刚完成放大
            {
                // 接下来 0.2 s 缩回原大小
                timer = 0;
                duration = shrinkDuration;
                fromScale = transform.localScale;
                toScale = baseScale;
            }
            else                               // 刚刚完成缩小
            {
                transform.localScale = baseScale;
                enabled = false;               // 停掉 Update，节省性能
            }
        }
    }
}