using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMove : MonoBehaviour
{

    public static CameraMove Instance;
    void Awake()
    {
        Instance = this;
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        isReady = false;
    }
    public Button startButton;
    
    [Header("移动设置")]
    public Vector3 targetPosition = new Vector3(580, 270, -448); // 目标位置
    public float moveSpeed = 2f; // 移动速度
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 移动曲线
    
    private Vector3 startPosition;
    private bool isMoving = false;

    public bool isReady = false;

    void Start()
    {
        startPosition = transform.position;
        
        // 绑定按钮点击事件
        if (startButton != null)
        {
            startButton.onClick.AddListener(MoveCameraToTarget);
        }
    }

    // 移动相机到目标位置
    public void MoveCameraToTarget()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveCameraCoroutine());
        }
    }

    // 移动协程
    private IEnumerator MoveCameraCoroutine()
    {
        isMoving = true;
        Vector3 fromPosition = transform.position;
        float elapsed = 0f;
        float duration = Vector3.Distance(fromPosition, targetPosition) / moveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 使用曲线插值实现平滑移动
            float curveValue = moveCurve.Evaluate(t);
            transform.position = Vector3.Lerp(fromPosition, targetPosition, curveValue);

            yield return null;
        }

        // 确保最终位置准确
        transform.position = targetPosition;
        isMoving = false;
        isReady = true;
    }

    // 重置相机位置
    public void ResetCameraPosition()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveCameraToPosition(startPosition));
        }
    }

    // 移动到指定位置的通用方法
    private IEnumerator MoveCameraToPosition(Vector3 destination)
    {
        isMoving = true;
        Vector3 fromPosition = transform.position;
        float elapsed = 0f;
        float duration = Vector3.Distance(fromPosition, destination) / moveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float curveValue = moveCurve.Evaluate(t);
            transform.position = Vector3.Lerp(fromPosition, destination, curveValue);
            
            yield return null;
        }

        transform.position = destination;
        isMoving = false;
    }
}