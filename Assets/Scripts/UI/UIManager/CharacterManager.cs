using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;
    public List<Sprite> sprites;
    public Dictionary<string, Sprite> characterImages = new Dictionary<string, Sprite>();
    public GameObject character;

 
    public float entryTime = 0.8f;

    public float minBrightness = 0.3f;
    public float maxBrightness = 1.0f;

    public bool isAnimeComplete = false;
    private Image image;

    private bool isFirst = true;

    public RectTransform leftRectTransform;
    public RectTransform rightRectTransform;
    public RectTransform rectTransform;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        characterImages = new Dictionary<string, Sprite>(){
            {"九溪", sprites[0]},
            {"共伯", sprites[1]},
            {"南生", sprites[2]},
            {"常万", sprites[3]},
            {"敬开", sprites[4]},
            {"白产", sprites[5]},
            {"谷雨", sprites[6]}
        };

        image = character.GetComponent<Image>();
    }

    void Update()
    {

    }

    /// <summary>
    /// 先快后慢的缓动函数（EaseOutExpo）
    /// </summary>
    private float EaseOutExpo(float t)
    {
        return t >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
    }

    IEnumerator CharacterExit(string cleanName)
    {
        float duration = 0f;
        while(duration < entryTime)
        {
            duration += Time.deltaTime;
            float t = duration / entryTime;
            // 颜色使用缓动函数
            float easedT = EaseOutExpo(t);
            float k = Mathf.Lerp(maxBrightness, minBrightness, easedT);
            Color newColor = new Color(k, k, k, 1);
            image.color = newColor;
            // 位置使用缓动函数
            float newx = Mathf.Lerp(leftRectTransform.position.x, rightRectTransform.position.x, easedT);
            Vector2 position = rectTransform.position;
            position.x = newx;
            rectTransform.position = position;
            yield return null;
        }
        StartCoroutine(CharacterEntry(cleanName));
    }

    IEnumerator CharacterEntry(string cleanName)
    {
        image.sprite = characterImages[cleanName];
        float duration = 0f;
        while (duration < entryTime)
        {
            duration += Time.deltaTime;
            float t = duration / entryTime;
            // 颜色使用缓动函数
            float easedT = EaseOutExpo(t);
            float k = Mathf.Lerp(minBrightness, maxBrightness, easedT);
            Color newColor = new Color(k, k, k, 1);
            image.color = newColor;
            // 位置使用缓动函数
            float newx = Mathf.Lerp(rightRectTransform.position.x, leftRectTransform.position.x, easedT);
            Vector2 position = rectTransform.position;
            position.x = newx;
            rectTransform.position = position;
            yield return null;
        }
    }


    public void ShowCharacter(string name)
    {
        // 防御性检查：去除空白字符和特殊字符
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("[CharacterManager] ShowCharacter 收到空字符串或null");
            return;
        }

        // 去除前后空白字符
        string cleanName = name.Trim();
        
        // 调试：显示原始名称和清理后的名称
        if (cleanName != name)
        {
            Debug.Log($"[CharacterManager] 清理角色名称: 原始='{name}' (长度:{name.Length}), 清理后='{cleanName}' (长度:{cleanName.Length})");
        }
        
        // 调试：显示每个字符的Unicode编码
        Debug.Log($"[CharacterManager] 角色名称字符编码: {string.Join(", ", System.Array.ConvertAll(cleanName.ToCharArray(), c => $"{c}(U+{((int)c):X4})"))}");
        
        if (characterImages.ContainsKey(cleanName))
        {
            character.SetActive(true);
            if(isFirst)
            {
                StartCoroutine(CharacterEntry(cleanName));
                isFirst = false;
                return;
            }
            StartCoroutine(CharacterExit(cleanName));
            Debug.Log($"[CharacterManager] 成功显示角色: {cleanName}");
        }
        else
        {
            Debug.LogWarning($"[CharacterManager] 未找到角色图片: '{cleanName}' (原始: '{name}')");
            Debug.Log($"[CharacterManager] 可用角色列表: {string.Join(", ", characterImages.Keys)}");
        }
    }

    public void CharacterAnime()
    {
        

        
    }
}
