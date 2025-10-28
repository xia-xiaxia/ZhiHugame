using UnityEngine;

/// <summary>
/// 使用 Unity 原生 Cursor API 的自定义鼠标管理器
/// 这种方式性能好，但鼠标图标会受到系统限制
/// </summary>
public class CustomCursorManager : MonoBehaviour
{
    public static CustomCursorManager Instance;

    [Header("鼠标指针纹理")]
    [Tooltip("默认鼠标指针纹理（建议32x32或64x64）")]
    public Texture2D defaultCursor;
    
    [Tooltip("可交互时的鼠标指针纹理")]
    public Texture2D interactCursor;
    
    [Tooltip("点击时的鼠标指针纹理")]
    public Texture2D clickCursor;

    [Header("热点设置")]
    [Tooltip("鼠标热点位置（相对于纹理左上角的像素偏移）")]
    public Vector2 hotspot = Vector2.zero;

    [Header("显示设置")]
    [Tooltip("是否隐藏系统鼠标")]
    public bool hideSystemCursor = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetDefaultCursor();
    }

    /// <summary>
    /// 设置默认鼠标
    /// </summary>
    public void SetDefaultCursor()
    {
        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    /// <summary>
    /// 设置可交互鼠标
    /// </summary>
    public void SetInteractCursor()
    {
        if (interactCursor != null)
        {
            Cursor.SetCursor(interactCursor, hotspot, CursorMode.Auto);
        }
    }

    /// <summary>
    /// 设置点击鼠标
    /// </summary>
    public void SetClickCursor()
    {
        if (clickCursor != null)
        {
            Cursor.SetCursor(clickCursor, hotspot, CursorMode.Auto);
        }
    }

    /// <summary>
    /// 显示/隐藏鼠标
    /// </summary>
    public void SetCursorVisible(bool visible)
    {
        Cursor.visible = visible;
    }

    /// <summary>
    /// 锁定/解锁鼠标
    /// </summary>
    public void SetCursorLockState(CursorLockMode lockMode)
    {
        Cursor.lockState = lockMode;
    }
}
