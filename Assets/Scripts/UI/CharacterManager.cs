using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;
    public List<Sprite> sprites;
    public Dictionary<string, Sprite> characterImages = new Dictionary<string, Sprite>();
    public GameObject character;

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

    }

    void Update()
    {

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
            character.GetComponent<Image>().sprite = characterImages[cleanName];
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
