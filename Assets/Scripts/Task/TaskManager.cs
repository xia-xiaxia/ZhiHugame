using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using JetBrains.Annotations;


public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    // 将 JSON 作为 TextAsset 拖入 Inspector（推荐）
    public TextAsset taskJsonAsset;

    public Dictionary<string, TaskDefinition> taskDict = new Dictionary<string, TaskDefinition>();

    public GameObject taskPanel;
    public GameObject taskItemPrefab;
    public Text taskText;
    public GameObject taskDescription;
    public Text taskDescriptionText;


    private class TaskInstance
    {
        public TaskDefinition def;
        public int startTurn;
        public bool finished;
    }

    private List<TaskInstance> running = new List<TaskInstance>();
    // 记录“已在运行或已排队”的任务ID，避免同一任务被多次启动
    private readonly HashSet<string> activeTaskIds = new HashSet<string>();

    public int maxConcurrentTasks = 100;
    public bool preventDuplicate = true;
    public int unlimitedTaskSafetyLimit = 200; // 超过该回合数仍未手动结束则强制判定失败

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);

        LoadTasksFromJson();
        Debug.Log("[TaskManager] 初始化完成");
        taskDescription.SetActive(false);
    }

    // 自动加载任务（优先使用 Inspector 中的 TextAsset）
    public void LoadTasksFromJson()
    {
        string json = null;

        if (taskJsonAsset != null)
        {
            json = taskJsonAsset.text;
            Debug.Log("[TaskManager] 从 Inspector TextAsset 加载任务 JSON");
        }

        foreach (var task in JsonHelper.FromJson<TaskDefinition>(json))
        {
            if (taskDict.ContainsKey(task.id))
                Debug.LogWarning($"[TaskManager] 任务 ID 重复: {task.id}，后者将覆盖前者");
            taskDict[task.id] = task;
        }

        Debug.Log($"[TaskManager] 已加载任务数: {taskDict.Count}");
    }

    // 启动任务
    public void StartTask(string id)
    {
        if (!taskDict.TryGetValue(id, out var def))
        {
            Debug.LogWarning($"[TaskManager] 未找到任务 ID: {id}");
            return;
        }

        // 先做去重，占位到 activeTaskIds，彻底避免同帧多次启动
        if (preventDuplicate)
        {
            if (activeTaskIds.Contains(id))
            {
                Debug.Log($"[TaskManager] 任务 {id} 已在运行或等待中，跳过重复启动");
                return;
            }
            activeTaskIds.Add(id);
        }

        StartCoroutine(StartTaskCoroutine(def));
    }

    private IEnumerator StartTaskCoroutine(TaskDefinition def)
    {
        if (running.Count >= maxConcurrentTasks)
        {
            Debug.LogWarning($"[TaskManager] 任务并发超过上限 {maxConcurrentTasks}，拒绝启动 {def.id}");
            // 并发受限时，需要把占位释放掉
            activeTaskIds.Remove(def.id);
            yield break;
        }

        var inst = new TaskInstance { def = def, startTurn = GetCurrentTurn(), finished = false };
        running.Add(inst);
        GameObject currentTask = Instantiate(taskItemPrefab, taskPanel.transform);
        currentTask.GetComponentInChildren<Text>().text = def.description;
        Button taskButton = currentTask.GetComponentInChildren<Button>();
        taskButton.onClick.AddListener(() => { ShowTaskDescription(def); }); // 点击查看详细显示

        Debug.Log($"[TaskManager] 任务 {def.id} 开始 (timeLimit={def.timeLimit})");

        if (def.timeLimit == 0)
        {
            HandleImmediateTask(inst);
            yield break;
        }
        else if (def.timeLimit < 0)
        {
            yield return StartCoroutine(HandleUnlimitedTask(inst));
            yield break;
        }
        else
        {
            yield return StartCoroutine(HandleTimedTask(inst));
        }
    }

    // 立即判定任务
    private void HandleImmediateTask(TaskInstance inst)
    {
        EvaluateAndFinish(inst);
    }

    // 无时限任务处理
    private IEnumerator HandleUnlimitedTask(TaskInstance inst)
    {
        while (!inst.finished)
        {
            if (GetCurrentTurn() - inst.startTurn > unlimitedTaskSafetyLimit)
            {
                Debug.LogWarning($"[TaskManager] 无时限任务 {inst.def.id} 超过安全回合，强制失败判定");
                EvaluateAndFinish(inst);
                break;
            }
            yield return null;
        }
    }

    // 有时限任务处理
    private IEnumerator HandleTimedTask(TaskInstance inst)
    {
        int targetTurn = inst.startTurn + inst.def.timeLimit;
        while (!inst.finished && GetCurrentTurn() < targetTurn)
            yield return null;

        if (!inst.finished)
            EvaluateAndFinish(inst);
    }

    public void EvaluateAndFinishById(string id)
    {
        var inst = running.Find(x => x.def.id == id && !x.finished);
        if (inst != null) EvaluateAndFinish(inst);
    }

    private void EvaluateAndFinish(TaskInstance inst)
    {
        if (inst == null || inst.finished) return;
        inst.finished = true;

        bool failLimitHit = CheckAnyLimit(inst.def);
        bool allElementsOk = CheckAllElements(inst.def);

        string triggerEventId = null;
        if (failLimitHit)
        {
            triggerEventId = inst.def.failEventId;
            GameControl.Instance.CompleteTask(triggerEventId);
            Debug.Log($"[TaskManager] 任务 {inst.def.id} 直接失败（触发限制）");
            EventManager.Instance?.SetNextEventId(triggerEventId, inst.def.failEventFile);
        }
        else if (allElementsOk)
        {
            triggerEventId = inst.def.successEventId;
            GameControl.Instance.CompleteTask(triggerEventId);
            Debug.Log($"[TaskManager] 任务 {inst.def.id} 成功");
            EventManager.Instance?.SetNextEventId(triggerEventId, inst.def.successEventFile);
        }
        else
        {
            triggerEventId = inst.def.failEventId;
            Debug.Log($"[TaskManager] 任务 {inst.def.id} 失败（要素未达成）");
        }

        running.Remove(inst);
        // 任务真正结束后，释放占位，允许再次启动
        activeTaskIds.Remove(inst.def.id);
    }

    private int GetStatValue(string stat)
    {
        if (EventManager.Instance == null || EventManager.Instance.stats == null)
        {
            Debug.LogWarning("[TaskManager] 无法读取 StatModel（EventManager.Instance.stats 为 null）");
            return 0;
        }
        var s = EventManager.Instance.stats;
        switch (stat.ToLower())
        {
            case "king": return s.king;
            case "noble": return s.noble;
            case "scholar": return s.scholar;
            case "foreign": return s.foreign;
            case "people": return s.people;
            default:
                Debug.LogWarning($"[TaskManager] 未知统计字段 {stat}");
                return 0;
        }
    }

    private bool CheckAllElements(TaskDefinition def)
    {
        foreach (var el in def.requiredElements)
        {
            int val = GetStatValue(el.stat);
            if (!el.IsSatisfied(val)) return false;
        }
        return true;
    }

    private bool CheckAnyLimit(TaskDefinition def)
    {
        foreach (var lim in def.failLimits)
        {
            int val = GetStatValue(lim.stat);
            if (lim.IsSatisfied(val)) return true;
        }
        return false;
    }

    public List<string> GetRunningTaskIds()
    {
        var list = new List<string>();
        foreach (var r in running) list.Add(r.def.id);
        return list;
    }

    // 获取当前回合数（请根据你的 GameControl 实现调整）
    private int GetCurrentTurn()
    {
        return GameControl.Instance != null ? GameControl.Instance.turns : 0;
    }

    public void ResetAllTasks()
    {
        StopAllCoroutines();
        running.Clear();
        activeTaskIds.Clear();
    }

    public void ShowTaskDescription(TaskDefinition def)
    {
        if (taskDescription != null && taskDescriptionText != null)
        {
            taskDescriptionText.text = def.description + "\n" + "时限：" + def.timeLimit + " 回合";
            taskDescription.SetActive(!taskDescription.activeSelf);
        }
    }
}

// 辅助类：支持 List<T> 的 Json 解析
public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public List<T> array;
    }
}