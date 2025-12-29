// 放在你现在接收事件的脚本里
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class  DialoguePanel : MonoBehaviour
{
    public Text bodyText;            
    public float typeInterval = 0.05f;

    private string currentFullText;  // 当前要打的完整文本
    private Coroutine typeRoutine;
    private string pendingText;      // 等待显示的文本

        // 对外暴露：是否正在打字
        public bool IsTyping => typeRoutine != null;

        // 对外暴露：强制完成当前打字（立即显示完整句子）
        public void ForceCompleteTyping()
        {
            if (typeRoutine != null)
            {
                StopCoroutine(typeRoutine);
                typeRoutine = null;
            }
            if (bodyText != null)
                bodyText.text = currentFullText ?? string.Empty;
        }

    private void Awake()
    {
    }

    public void SetBody(string newBody)
    {
        if (bodyText == null) return;

        // 如果正在打字，立即停掉
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        if (newBody == currentFullText)
        {
            // 新文本和当前文本一样，直接返回
            return;
        }

        // 如果面板不在激活状态，直接设置文本
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("DialoguePanel 游戏对象处于非活跃状态，直接设置文本。");
            bodyText.text = newBody;
            currentFullText = newBody;
            pendingText = null;
            return;
        }

        currentFullText = newBody;
        pendingText = null;
        typeRoutine = StartCoroutine(TypeRoutine());
    }
    

    private IEnumerator TypeRoutine()
    {
        bodyText.text = "";
        List<string> frames = GenerateFrames(currentFullText);

        foreach (string frame in frames)
        {
            bodyText.text = frame;
            yield return new WaitForSeconds(typeInterval);
        }

        yield return new WaitForSeconds(0.1f);
        typeRoutine = null;
    }


    private List<string> GenerateFrames(string text)
    {
        List<string> frames = new List<string>();
        Stack<string> tagStack = new Stack<string>(); 
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '<')
            {
                int end = text.IndexOf('>', i);
                if (end != -1)
                {
                    string tag = text.Substring(i, end - i + 1);

                    if (tag.StartsWith("</")) 
                    {
                        if (tagStack.Count > 0) tagStack.Pop();
                    }
                    else if (!tag.EndsWith("/>")) 
                    {
                        string tagName = Regex.Match(tag, @"<(\w+)=?").Groups[1].Value;
                        tagStack.Push($"</{tagName}>");
                    }

                    sb.Append(tag);
                    i = end;
                    continue;
                }
            }

            sb.Append(text[i]);

            StringBuilder frameBuilder = new StringBuilder(sb.ToString());
            foreach (string closeTag in tagStack)
            {
                frameBuilder.Append(closeTag);
            }

            frames.Add(frameBuilder.ToString());
        }

        return frames;
    }

    // 外部调用，当 dadian 消失时立即检查是否需要开始打字
    public void OnDadianHidden()
    {
        if (!string.IsNullOrEmpty(pendingText) && typeRoutine == null)
        {
            currentFullText = pendingText;
            pendingText = null;
            
            if (gameObject.activeInHierarchy)
            {
                typeRoutine = StartCoroutine(TypeRoutine());
            }
        }
    }
}