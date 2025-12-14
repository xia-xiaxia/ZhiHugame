using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 事件显示UI：负责事件内容、对话、选项的显示
/// </summary>
public class EventDisplayUI : MonoBehaviour
{
    public static EventDisplayUI Instance;

    [Header("事件显示组件")]
    public Text titleText;
    public Text bodyText;
    public Text speakerName;
    public DialoguePanel dialoguePanel;

    [Header("按钮组件")]
    public Button[] optionButtons = new Button[4];
    public Button nextSentenceButton;
    public Button autoPlayButton;

    // 内部状态
    private List<string> currentEventSentences = new List<string>();
    private int currentSentenceIndex = 0;
    private bool waitingForSentence = false;
    private string currentEventId = "";
    private Coroutine autoNextCoroutine = null;
    private bool autoPlayEnabled = false;
    private bool isFirstShow = true;

    void Awake()
    {
        Instance = this;
        isFirstShow = true;
    }

    void Start()
    {
        // 绑定"下一句"按钮
        if (nextSentenceButton != null)
        {
            nextSentenceButton.onClick.RemoveAllListeners();
            nextSentenceButton.onClick.AddListener(OnNextSentenceClicked);
        }

        // 绑定"自动播放"按钮
        if (autoPlayButton != null)
        {
            autoPlayButton.onClick.RemoveAllListeners();
            autoPlayButton.onClick.AddListener(OnAutoPlayClicked);
            UpdateAutoPlayButtonLabel();
        }
    }

    private void Update()
    {
        GameObject go = GetHoveredUI();
        if(go != null && HasParent(go.transform, "下一句")) 
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll < 0)
            {
                OnNextSentenceClicked();
            } else if(scroll > 0)
            {
                OnPrevSentenceClicked();
            }
        }
    }

    /// <summary>
    /// 显示事件
    /// </summary>
    public void ShowEvent(string id)
    {
        Debug.Log($"[EventDisplayUI] 显示事件: {id}");
        currentEventId = id;

        if (EventManager.Instance == null)
        {
            Debug.LogError("[EventDisplayUI] EventManager.Instance 为 null");
            return;
        }

        var evt = EventManager.Instance.GetEvent(id);
        if (evt == null)
        {
            Debug.LogError($"[EventDisplayUI] 找不到事件: {id}");
            return;
        }

        // 开始协程：先播放角色动画，再显示对话内容
        StartCoroutine(ShowEventWithAnimation(evt));
    }

    /// <summary>
    /// 带动画的事件显示：先播放角色动画，等待完成后再显示对话
    /// </summary>
    private IEnumerator ShowEventWithAnimation(GameEvent evt)
    {
        // 1. 先显示角色立绘并播放动画
        if (!string.IsNullOrEmpty(evt.speaker) && evt.speaker != "旁白")
        {
            CharacterManager.Instance?.ShowCharacter(evt.speaker);
            
            // 检查角色是否真的显示了（可能因为找不到角色图片而没有显示）
            bool characterIsShown = CharacterManager.Instance != null && 
                                   CharacterManager.Instance.character != null && 
                                   CharacterManager.Instance.character.activeSelf;
            
            // 只有角色真正显示时才等待动画
            if (characterIsShown)
            {
                // 等待角色动画播放完成
                if(isFirstShow)
                {
                    isFirstShow = false;
                    if(speakerName != null) speakerName.text = evt.speaker ?? string.Empty;
                    float initialWaitTime = CharacterManager.Instance.entryTime;
                    Debug.Log($"[EventDisplayUI] 首次显示，等待角色初始动画完成，时长: {initialWaitTime}秒");
                    yield return new WaitForSeconds(initialWaitTime);
                }
                else
                {
                    float exitAnimationTime = CharacterManager.Instance.entryTime;
                    Debug.Log($"[EventDisplayUI] 等待角色退场动画，时长: {exitAnimationTime}秒");
                    yield return new WaitForSeconds(exitAnimationTime);
                    if (speakerName != null) speakerName.text = evt.speaker ?? string.Empty;
                    Debug.Log($"[EventDisplayUI] 等待角色入场动画");
                    float entryAnimationTime = CharacterManager.Instance.entryTime;
                    yield return new WaitForSeconds(entryAnimationTime);
                }
            }
            else
            {
                // 角色没有显示，直接显示说话人不等待
                if (speakerName != null) speakerName.text = evt.speaker ?? string.Empty;
            }
        }

        // 2. 角色动画播放完成后，显示标题
        if (titleText != null) titleText.text = evt.title ?? string.Empty;

        // 3. 分割对话句子
        string body = evt.body ?? string.Empty;
        currentEventSentences = new List<string>(body.Split('\n'));
        currentSentenceIndex = 0;

        // 4. 显示对话内容
        if (currentEventSentences.Count <= 1)
        {
            // 只有一句，直接显示后显示选项
            if (dialoguePanel != null)
                dialoguePanel.SetBody(body);
            
            waitingForSentence = true;
            StartCoroutine(ShowOptionsAfterDelay(0.8f));
        }
        else
        {
            // 多句对话，逐句显示
            waitingForSentence = true;
            ShowCurrentSentence();
        }
    }

    // 等待相应的动画播放的时间
    IEnumerator WaitForAnimation(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    /// <summary>
    /// 显示当前句子
    /// </summary>
    private void ShowCurrentSentence()
    {
        // 隐藏选项
        HideOptions();

        if (currentSentenceIndex < currentEventSentences.Count)
        {
            if (dialoguePanel != null)
                dialoguePanel.SetBody(currentEventSentences[currentSentenceIndex]);

            currentSentenceIndex++;

            // 停止之前的自动播放
            if (autoNextCoroutine != null)
            {
                StopCoroutine(autoNextCoroutine);
                autoNextCoroutine = null;
            }

            if (currentSentenceIndex < currentEventSentences.Count)
            {
                // 还有下一句
                if (autoPlayEnabled)
                {
                    autoNextCoroutine = StartCoroutine(AutoShowNextSentence(1.0f));
                }
            }
            else
            {
                // 全部显示完毕，显示选项
                StartCoroutine(ShowOptionsAfterDelay(0.2f));
            }
        }
    }

    private void ShowPrevSentence()
    {
        // 隐藏选项
        HideOptions();
        Debug.Log("Called");
        if (currentSentenceIndex <= currentEventSentences.Count)
        {
            if (currentSentenceIndex != 0) currentSentenceIndex--;
            else return;

            waitingForSentence = true;

            if (dialoguePanel != null)
                dialoguePanel.SetBody(currentEventSentences[currentSentenceIndex]);

            // 停止之前的自动播放
            if (autoNextCoroutine != null)
            {
                StopCoroutine(autoNextCoroutine);
                autoNextCoroutine = null;
            }

            if (currentSentenceIndex < currentEventSentences.Count)
            {
                // 还有下一句
                if (autoPlayEnabled)
                {
                    autoNextCoroutine = StartCoroutine(AutoShowNextSentence(1.0f));
                }
            }
            else
            {
                // 全部显示完毕，显示选项
                StartCoroutine(ShowOptionsAfterDelay(0.2f));
            }
        }
    }

    /// <summary>
    /// 自动显示下一句（协程）
    /// </summary>
    private IEnumerator AutoShowNextSentence(float delay)
    {
        if (!autoPlayEnabled) yield break;
        
        yield return new WaitForSeconds(delay);
        
        if (!autoPlayEnabled) yield break;
        
        ShowCurrentSentence();
    }

    /// <summary>
    /// 延迟显示选项（协程）
    /// </summary>
    private IEnumerator ShowOptionsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ShowEventOptions(currentEventId);
        waitingForSentence = false;
    }

    /// <summary>
    /// 显示事件选项
    /// </summary>
    private void ShowEventOptions(string id)
    {
        var evt = EventManager.Instance?.GetEvent(id);
        if (evt == null || evt.options == null)
        {
            Debug.LogError($"[EventDisplayUI] 事件或选项为空: {id}");
            return;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < evt.options.Count)
            {
                var opt = evt.options[i];
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].GetComponentInChildren<Text>().text = opt.text;
                
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() =>
                {
                    // 播放音效
                    MusicManager.Instance?.PlayButtonSound2();

                    // 应用选项效果
                    GameControl.Instance?.SaveStatsSnapshot();
                    EventManager.Instance?.ApplyOption(opt, GameControl.Instance.year,evt.yearDelta);

                    // 设置后继事件
                    if (!string.IsNullOrEmpty(opt.nextEventId))
                    {
                        EventManager.Instance?.SetNextEventId(opt.nextEventId);
                    }

                    // 更新UI并处理下一回合
                    StatsDisplayUI.Instance?.UpdateStatText();
                    GameControl.Instance?.OnStatsChanged();
                    ClearText();
                    GameControl.Instance?.ProcessNextTurn();
                });
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

    }

    /// <summary>
    /// 下一句按钮点击
    /// </summary>
    private void OnNextSentenceClicked()
    {
        if (!waitingForSentence) return;

        // 若正在打字，先完成打字
        if (dialoguePanel != null && dialoguePanel.IsTyping)
        {
            dialoguePanel.ForceCompleteTyping();
            return;
        }

        // 停止自动播放
        if (autoNextCoroutine != null)
        {
            StopCoroutine(autoNextCoroutine);
            autoNextCoroutine = null;
        }

        ShowCurrentSentence();
    }

    private void OnPrevSentenceClicked()
    {

        // 若正在打字，先完成打字
        if (dialoguePanel != null && dialoguePanel.IsTyping)
        {
            dialoguePanel.ForceCompleteTyping();
            return;
        }

        // 停止自动播放
        if (autoNextCoroutine != null)
        {
            StopCoroutine(autoNextCoroutine);
            autoNextCoroutine = null;
        }

        ShowPrevSentence();
    }

    /// <summary>
    /// 自动播放按钮点击
    /// </summary>
    private void OnAutoPlayClicked()
    {
        autoPlayEnabled = !autoPlayEnabled;
        UpdateAutoPlayButtonLabel();

        if (!autoPlayEnabled)
        {
            // 关闭自动播放
            if (autoNextCoroutine != null)
            {
                StopCoroutine(autoNextCoroutine);
                autoNextCoroutine = null;
            }
        }
        else
        {
            // 开启自动播放
            if (waitingForSentence && 
                dialoguePanel != null && 
                !dialoguePanel.IsTyping && 
                currentSentenceIndex < currentEventSentences.Count)
            {
                if (autoNextCoroutine != null)
                {
                    StopCoroutine(autoNextCoroutine);
                    autoNextCoroutine = null;
                }
                autoNextCoroutine = StartCoroutine(AutoShowNextSentence(0.5f));
            }
        }
    }

    /// <summary>
    /// 更新自动播放按钮文字
    /// </summary>
    private void UpdateAutoPlayButtonLabel()
    {
        if (autoPlayButton == null) return;
        
        var txt = autoPlayButton.GetComponentInChildren<Text>();
        if (txt != null)
            txt.text = autoPlayEnabled ? "自动播放：开" : "自动播放：关";
    }

    /// <summary>
    /// 清空文本
    /// </summary>
    public void ClearText()
    {
        if (titleText) titleText.text = string.Empty;
        
        if (dialoguePanel && dialoguePanel.gameObject.activeInHierarchy)
        {
            dialoguePanel.SetBody(" ");
        }

        HideOptions();
    }

    /// <summary>
    /// 隐藏选项按钮
    /// </summary>
    public void HideOptions()
    {
        foreach (var btn in optionButtons)
        {
            if (btn != null)
            {
                btn.gameObject.SetActive(false);
                btn.onClick.RemoveAllListeners();
                var txt = btn.GetComponentInChildren<Text>();
                if (txt != null) txt.text = "";
            }
        }
    }

    /// <summary>
    /// 获取当前事件ID（用于暂停）
    /// </summary>
    public string GetCurrentEventId() => currentEventId;

    /// <summary>
    /// 获取当前句子索引（用于暂停）
    /// </summary>
    public int GetCurrentSentenceIndex() => currentSentenceIndex;

    /// <summary>
    /// 恢复事件状态（从暂停恢复）
    /// </summary>
    public void RestoreEventState(string eventId, int sentenceIndex)
    {
        currentEventId = eventId;
        currentSentenceIndex = sentenceIndex;

        var evt = EventManager.Instance?.GetEvent(eventId);
        if (evt == null)
        {
            Debug.LogWarning($"[EventDisplayUI] 无法恢复事件: {eventId}");
            return;
        }

        // 重新设置句子列表
        currentEventSentences.Clear();
        currentEventSentences.Add(evt.title);
        if (!string.IsNullOrEmpty(evt.body))
        {
            currentEventSentences.AddRange(evt.body.Split('\n'));
        }

        // 从指定句子开始显示
        if (sentenceIndex >= currentEventSentences.Count)
        {
            ShowEventOptions(eventId);
        }
        else
        {
            ShowCurrentSentence();
        }
    }

    public GameObject GetHoveredUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition; 

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            return results[0].gameObject;
        }
        return null;
    }

    /// <summary>
    /// 检查父级链中是否包含指定名称的物体
    /// </summary>
    public bool HasParent(Transform transform, string parentName)
    {
        Transform curr = transform.parent;
        while (curr != null)
        {
            if (curr.name == parentName)
                return true;

            curr = curr.parent;
        }
        return false;
    }

    /// <summary>
    /// 重置事件显示UI（重开游戏时调用）
    /// </summary>
    public void ResetEventDisplayUI()
    {
        isFirstShow = true;
        currentEventSentences.Clear();
        currentSentenceIndex = 0;
        waitingForSentence = false;
        currentEventId = "";
        
        if (autoNextCoroutine != null)
        {
            StopCoroutine(autoNextCoroutine);
            autoNextCoroutine = null;
        }
        
        Debug.Log("[EventDisplayUI] 已重置事件显示UI");
    }
}
