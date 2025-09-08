// 放在你现在接收事件的脚本里
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialoguePanel : MonoBehaviour
{
    public Text bodyText;            // 你的 Legacy Text
    public float typeInterval = 0.05f;

    private string currentFullText;  // 当前要打的完整文本
    private Coroutine typeRoutine;

    // 任何地方原来写 bodyText.text = evt.body; 的地方改成：
    public void SetBody(string newBody)
    {
        // 如果正在打字，立即停掉
        if (typeRoutine != null)
            StopCoroutine(typeRoutine);

        currentFullText = newBody;
        typeRoutine = StartCoroutine(TypeRoutine());
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
}