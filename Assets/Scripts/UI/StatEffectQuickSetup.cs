using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 统计数值特效快速设置助手
/// 可以帮助快速为一个数值设置完整的特效显示
/// </summary>
[ExecuteInEditMode]
public class StatEffectQuickSetup : MonoBehaviour
{
    [Header("数值类型")]
    public StatType statType;

    [Header("自动创建特效对象")]
    [Tooltip("点击后会自动创建增加、下降特效和填充条的子对象")]
    public bool autoCreateChildren = false;

    [Header("特效图片资源（可选，用于自动赋值）")]
    public Sprite increaseEffectSprite;
    public Sprite decreaseEffectSprite;
    public Sprite valueBarSprite;

    [Header("特效颜色")]
    public Color increaseEffectColor = new Color(0, 1, 0, 0.8f); // 绿色半透明
    public Color decreaseEffectColor = new Color(1, 0, 0, 0.8f); // 红色半透明
    public Color valueBarColor = Color.white;

    [Header("位置和大小")]
    public Vector2 effectSize = new Vector2(100, 100);
    public Vector2 valueBarSize = new Vector2(200, 20);

    void Update()
    {
        #if UNITY_EDITOR
        if (autoCreateChildren)
        {
            autoCreateChildren = false;
            CreateChildren();
        }
        #endif
    }

    [ContextMenu("创建子对象")]
    void CreateChildren()
    {
        // 检查是否已有 StatEffectController
        StatEffectController controller = GetComponent<StatEffectController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<StatEffectController>();
        }

        controller.statType = statType;

        // 创建增加特效
        GameObject increaseEffect = CreateEffectChild("IncreaseEffect", increaseEffectSprite, increaseEffectColor, effectSize);
        if (increaseEffect != null)
        {
            controller.increaseEffectImage = increaseEffect.GetComponent<Image>();
        }

        // 创建下降特效
        GameObject decreaseEffect = CreateEffectChild("DecreaseEffect", decreaseEffectSprite, decreaseEffectColor, effectSize);
        if (decreaseEffect != null)
        {
            controller.decreaseEffectImage = decreaseEffect.GetComponent<Image>();
        }

        // 创建数值条
        GameObject valueBar = CreateValueBarChild("ValueBar", valueBarSprite, valueBarColor, valueBarSize);
        if (valueBar != null)
        {
            controller.valueFilledImage = valueBar.GetComponent<Image>();
        }

        Debug.Log($"[StatEffectQuickSetup] 已为 {statType} 创建特效子对象");
    }

    GameObject CreateEffectChild(string name, Sprite sprite, Color color, Vector2 size)
    {
        // 检查是否已存在
        Transform existing = transform.Find(name);
        if (existing != null)
        {
            Debug.LogWarning($"[StatEffectQuickSetup] {name} 已存在，跳过创建");
            return existing.gameObject;
        }

        GameObject child = new GameObject(name);
        child.transform.SetParent(transform, false);

        // 添加 RectTransform
        RectTransform rt = child.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        // 添加 Image
        Image image = child.AddComponent<Image>();
        if (sprite != null)
        {
            image.sprite = sprite;
        }
        image.color = color;
        image.raycastTarget = false;

        // 初始隐藏
        child.SetActive(false);

        return child;
    }

    GameObject CreateValueBarChild(string name, Sprite sprite, Color color, Vector2 size)
    {
        // 检查是否已存在
        Transform existing = transform.Find(name);
        if (existing != null)
        {
            Debug.LogWarning($"[StatEffectQuickSetup] {name} 已存在，跳过创建");
            return existing.gameObject;
        }

        GameObject child = new GameObject(name);
        child.transform.SetParent(transform, false);

        // 添加 RectTransform
        RectTransform rt = child.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        // 添加 Image（设置为 Filled 类型）
        Image image = child.AddComponent<Image>();
        if (sprite != null)
        {
            image.sprite = sprite;
        }
        image.color = color;
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = (int)Image.OriginHorizontal.Left;
        image.fillAmount = 0.5f; // 初始 50%
        image.raycastTarget = false;

        return child;
    }

    [ContextMenu("清除所有子对象")]
    void ClearChildren()
    {
        #if UNITY_EDITOR
        // 在编辑模式下安全删除子对象
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        Debug.Log("[StatEffectQuickSetup] 已清除所有子对象");
        #endif
    }
}
