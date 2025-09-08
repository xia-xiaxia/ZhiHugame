using UnityEngine;
using System.Collections;

/// <summary>
/// 独立于父级，2 秒放大 1.1 倍，2 秒缩回原始大小，循环往复。
/// </summary>
[DisallowMultipleComponent]
public class ScaleLoop : MonoBehaviour
{
    [SerializeField] private float durationPerPhase = 2f;   // 每阶段时长
    [SerializeField] private float scaleFactor = 1.1f;   // 放大倍数

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        // 每次激活都重新启动协程，保证循环永远不过期
        StopAllCoroutines();   // 防止重复
        StartCoroutine(LoopScale());
    }

    private void OnDisable()
    {
        // 物体失活时停止协程，激活后会由 OnEnable 再次启动
        StopAllCoroutines();
    }

    private IEnumerator LoopScale()
    {
        // 无限循环：放大 → 缩小
        while (true)
        {
            yield return ScaleTo(originalScale * scaleFactor, durationPerPhase);
            yield return ScaleTo(originalScale, durationPerPhase);
        }
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale; // 确保最终值精确
    }
}