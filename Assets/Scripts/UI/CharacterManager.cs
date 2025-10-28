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
        if (characterImages.ContainsKey(name))
        {
            character.SetActive(true);
            character.GetComponent<Image>().sprite = characterImages[name];
        }
        else
        {
            Debug.LogWarning($"[CharacterManager] 未找到角色图片: {name}");
        }
    }
}
