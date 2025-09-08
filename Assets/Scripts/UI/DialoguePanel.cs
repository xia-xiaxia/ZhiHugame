// 放在你现在接收事件的脚本里
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialoguePanel : MonoBehaviour
{
    public Text bodyText;            // 你的 Legacy Text
    public float typeInterval = 0.05f;
    public GameObject dadian;        // dadian GameObject 引用

    private string currentFullText;  // 当前要打的完整文本
    private Coroutine typeRoutine;
    private string pendingText;      // 等待显示的文本

    private void Awake()
    {
        dadian = UIManager.Instance.daDian;
    }

    // 任何地方原来写 bodyText.text = evt.body; 的地方改成：
    public void SetBody(string newBody)
    {
        // 如果正在打字，立即停掉
        if (typeRoutine != null)
            StopCoroutine(typeRoutine);

        if (newBody == currentFullText)
        {
            // 如果新文本和当前文本一样，就不重打了
            return;
        }
        if (bodyText == null) return;

        // 检查GameObject是否处于活跃状态
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("DialoguePanel游戏对象处于非活跃状态，无法启动协程！直接设置文本。");
            bodyText.text = newBody;
            return;
        }

        currentFullText = newBody;

        // 检查 dadian 是否活跃
        if (dadian != null && dadian.activeSelf)
        {
            // 如果 dadian 正在显示，保存文本等待后续处理
            pendingText = newBody;
            Debug.Log("dadian 正在显示，等待其消失后开始打字");
            StartCoroutine(WaitForDadianToHide());
        }
        else
        {
            // dadian 未显示或为空，立即开始打字
            typeRoutine = StartCoroutine(TypeRoutine());
        }
    }
    
    private IEnumerator WaitForDadianToHide()
    {
        // 等待 dadian 消失
        while (dadian != null && dadian.activeSelf)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // dadian 消失后开始打字
        if (!string.IsNullOrEmpty(pendingText))
        {
            currentFullText = pendingText;
            pendingText = null;
            typeRoutine = StartCoroutine(TypeRoutine());
        }
    }

    private IEnumerator TypeRoutine()
    {
        bodyText.text = "";
        int len = currentFullText.Length;
        for (int i = 0; i <= len; i++)
        {
            bodyText.text = currentFullText.Substring(0, i);
            yield return new WaitForSeconds(typeInterval);
        }
        typeRoutine = null;
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