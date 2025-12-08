using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDatabase : MonoBehaviour
{
    // 管理事件的加载和查询
    public static EventDatabase Instance;
    JsonHelper jsonHelper = new JsonHelper();
    public List<TextAsset> eventJsons; // 用于在Inspector中绑定多个事件JSON文件
    public Dictionary<string, Dictionary<string, GameEvent>> eventDictionaries = new Dictionary<string, Dictionary<string, GameEvent>>();
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            LoadEvents();
        }
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void LoadEvents()
    {
        eventDictionaries.Clear();
        foreach (var json in eventJsons)
        {
            string speaker = "";
            GameEvent[] events = JsonHelper.FromJson<GameEvent>(json.text);
            Dictionary<string, GameEvent> eventDict = new Dictionary<string, GameEvent>();
            foreach (var gameEvent in events)
            {
                eventDict[gameEvent.id] = gameEvent;
                speaker = gameEvent.speaker.ToString();
            }
            eventDictionaries.Add(speaker,eventDict);
        }
    }
}
