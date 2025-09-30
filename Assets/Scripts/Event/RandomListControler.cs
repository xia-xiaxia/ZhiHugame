using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomListControler : MonoBehaviour
{
    public static RandomListControler Instance;
    public Dictionary<string, RandomEvent> randomEvents = new Dictionary<string, RandomEvent>();
    public List<Dictionary<string, RandomEvent>> randomEventpool = new List<Dictionary<string, RandomEvent>>();
    public List<TextAsset> randomEventJsons = new List<TextAsset>();
    public int killedCount = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Debug.Log("Jsons Count: " + randomEventJsons.Count);
    }

    public void LoadRandomEvents()
    {

        for (int i = 0; i < randomEventJsons.Count; i++)
        {
            if(i == killedCount)
            {
                Debug.Log("跳过被杀死的事件集，索引: " + i);
                continue; // 跳过被杀死的事件
            }
            
            TextAsset jsonFile = randomEventJsons[i];
            if (jsonFile != null)
            {
                RandomEvent[] randomEventArray = JsonHelper.FromJson<RandomEvent>(jsonFile.text);
                if (randomEventArray != null)
                {
                    foreach (var randomEvent in randomEventArray)
                    {
                        randomEvents[randomEvent.id] = randomEvent;
                    }
                    Debug.Log($"已加载 {jsonFile.name}，包含 {randomEventArray.Length} 个随机事件");
                    randomEventpool.Add(new Dictionary<string, RandomEvent>(randomEvents));
                }
                else
                {
                    Debug.LogWarning($"解析 {jsonFile.name} 时结果为 null，请检查 JSON 格式或 EventRoot 实现");
                }
            }
            else
            {
                Debug.LogWarning($"随机事件 JSON 文件未分配：索引 {i}");
            }

        }

        Debug.Log($"总共加载了 {randomEvents.Count} 个随机事件");
    }
}