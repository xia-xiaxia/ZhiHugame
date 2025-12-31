using System.Collections.Generic;
using System.Security;
using TMPro;
using UnityEngine;

public class EventListSetDebug : MonoBehaviour
{
    public static EventListSetDebug Instance;
    public StatModel stats;
    public TextMeshProUGUI nowEventListText;

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
        }
        nowEventListText.gameObject.SetActive(false);
    }

    void Update()
    {
        // 按 E 键显示当前事件列表
        if (Input.GetKeyDown(KeyCode.E) && nowEventListText.gameObject.activeSelf == false)
        {
            nowEventListText.gameObject.SetActive(true);
            ShowEventList();
        }
        
        // 按 Shift+E 添加测试事件
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.E))
        {
            nowEventListText.gameObject.SetActive(true);
            AddTestEvent(3);
        }

        if(Input.GetKeyUp(KeyCode.E) && nowEventListText.gameObject.activeSelf)
        {
            nowEventListText.gameObject.SetActive(false);
        }
    }
    
    private void ShowEventList()
    {
        List<int> activeEventSets = EventDatabase.Instance.GetActiveEventSetIndices();
        for (int i = 0; i < activeEventSets.Count; i++)
        {
            activeEventSets[i]++;
        }
        string activeSetsStr = string.Join(", ", activeEventSets);
        int totalEvents = EventDatabase.Instance.GetTotalEventCount();
        Debug.Log($"[EventListSetDebug] 当前激活事件集列表: {activeSetsStr}, 事件总数: {totalEvents}");
        nowEventListText.text = $"当前激活事件集列表:\n{activeSetsStr}\n\n激活的事件总数: {totalEvents}";


    }

    private void AddTestEvent(int index)
    {
        EventDatabase.Instance.ActivateEventSetById(index);
        Debug.Log($"[EventListSetDebug] 添加测试事件集 ID: {index}");
        List<int> activeEventSets = EventDatabase.Instance.GetActiveEventSetIndices();
        string activeSetsStr = string.Join(", ", activeEventSets);
        int totalEvents = EventDatabase.Instance.GetTotalEventCount();
        Debug.Log($"[EventListSetDebug] 当前激活事件集列表: {activeSetsStr}, 事件总数: {totalEvents}");
        nowEventListText.text = $"添加测试事件集 ID: {index}\n当前激活事件集列表:\n{activeSetsStr}\n\n激活的事件总数: {totalEvents}";
    }
}