using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FadePulseOnce : MonoBehaviour
{
    [Header("动画时长（秒）")]
    public float fadeInDuration = 0.3f;
    public float holdDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    public float scaleBackDuration = 0.1f;

    private Image image;
    private RectTransform rt;
    private Vector3 originalScale;

    private bool isPlaying;

    void Awake()
    {
        image = GetComponent<Image>();
        rt = transform as RectTransform;
        originalScale = rt.localScale;
        SetAlpha(0f);      // 初始透明
    }

    /// <summary>外部调用：播放一次完整循环</summary>
    public void Play()
    {
        if (isPlaying) return;   // 防止重复
        isPlaying = true;
        SetAlpha(0f);
        rt.localScale = originalScale;
        StartCoroutine(Sequence());
    }

    private System.Collections.IEnumerator Sequence()
    {
        // 1. 淡入 + 放大
        yield return FadeScale(fadeInDuration, 0, 1, originalScale, originalScale * 1.1f);

        // 2. 定格
        yield return new WaitForSeconds(holdDuration);

        // 3. 淡出
        yield return FadeScale(fadeOutDuration, 1, 0, rt.localScale, rt.localScale);

        // 4. 还原大小
        yield return FadeScale(scaleBackDuration, 0, 0, rt.localScale, originalScale);

        isPlaying = false;
    }

    private System.Collections.IEnumerator FadeScale(float duration,
                                                     float fromA, float toA,
                                                     Vector3 fromScale, Vector3 toScale)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            SetAlpha(Mathf.Lerp(fromA, toA, t));
            rt.localScale = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }
        SetAlpha(toA);
        rt.localScale = toScale;
    }

    private void SetAlpha(float a)
    {
        Color c = image.color;
        c.a = a;
        image.color = c;
    }
}