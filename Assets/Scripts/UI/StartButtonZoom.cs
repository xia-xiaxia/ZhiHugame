using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StartButtonZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("缩放设置")]
    [Tooltip("鼠标悬停时的放大倍数")]
    public float zoomScale = 1.2f;
    
    [Tooltip("缩放动画时长（秒）")]
    public float animationDuration = 0.2f;
    
    [Tooltip("使用平滑缓动效果")]
    public bool useSmoothTransition = true;
    
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // 记录原始缩放
        originalScale = rectTransform.localScale;
    }

    // 鼠标进入时触发
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 停止之前的动画
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        
        // 开始放大动画
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale * zoomScale));
    }

    // 鼠标离开时触发
    public void OnPointerExit(PointerEventData eventData)
    {
        // 停止之前的动画
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        
        // 恢复原始大小
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
    }

    // 平滑缩放到目标大小
    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = rectTransform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // 使用平滑插值或线性插值
            if (useSmoothTransition)
            {
                // 使用 SmoothStep 实现缓动效果
                t = t * t * (3f - 2f * t);
            }
            
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // 确保最终精确到达目标缩放
        rectTransform.localScale = targetScale;
    }
    
    // 当对象被禁用时，恢复原始大小
    void OnDisable()
    {
        if (rectTransform != null && originalScale != Vector3.zero)
        {
            rectTransform.localScale = originalScale;
        }
    }
}
