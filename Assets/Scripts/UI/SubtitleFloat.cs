using UnityEngine;
using System.Collections;

/// <summary>
/// 让 UI 图片（Logo）在屏幕里做平滑浮动：水平晃动、上下起伏。
/// 完全独立于父级，激活/失活不影响循环。
/// </summary>
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class LogoFloat : MonoBehaviour
{
    [Header("水平晃动")]
    [Tooltip("左右晃动的最大像素距离")]
    public float swingRange = 30f;

    [Tooltip("左右晃动一个来回的时长（秒）")]
    public float swingPeriod = 4f;

    [Header("上下浮动")]
    [Tooltip("上下浮动的最大像素距离")]
    public float bobRange = 20f;

    [Tooltip("上下浮动一个来回的时长（秒）")]
    public float bobPeriod = 3f;

    private RectTransform rt;
    private Vector2 startAnchoredPos;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        startAnchoredPos = rt.anchoredPosition;
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(FloatLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator FloatLoop()
    {
        while (true)
        {
            // 用 Time.time 做周期函数，保证协程被中断再恢复也连续
            float swingOffset = Mathf.Sin(Time.time * (2f * Mathf.PI / swingPeriod)) * swingRange;
            float bobOffset = Mathf.Sin(Time.time * (2f * Mathf.PI / bobPeriod)) * bobRange;

            rt.anchoredPosition = startAnchoredPos + new Vector2(swingOffset, bobOffset);
            yield return null;
        }
    }
}