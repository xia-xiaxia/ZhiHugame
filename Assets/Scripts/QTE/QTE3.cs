using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QTE3 : MonoBehaviour
{
    public StatModel stats;
    public EventManager eventManager;
    [Header("UI")]
    public Image point;
    public Text statusText;

    [Header("音效")]
    public AudioSource audioSrc;
    public AudioClip successSFX;

    [Header("参数")]
    public float fallSpeed = 300f;   // 向左漂移速度（像素/秒）
    public float pushForce = 80f;    // 每敲一次空格向右推多大幅度
    public float barWidth = 600f;   // 条总宽（像素）
    public int winCount = 3;      // 成功几次结束

    /* ------------- 运行数据 ------------- */
    private RectTransform pointRt;
    public int successCount = 0;
    private float lastPushTime = 0f;   // 防止同一帧多次触发

    /* ------------- 视觉缓存 ------------- */
    private Vector3 normalScale;
    private float enterTime;
    private bool gameOver;
    public ImageQuickPulse targetPulse;
    void Start()
    {
        pointRt = point.GetComponent<RectTransform>();
        normalScale = pointRt.localScale;
        ResetPoint();   // 开局置左
        UpdateText();
    }

    void Update()
    {
        if (stats.ifzz)
        {
            pointRt.anchoredPosition = new Vector2(-barWidth * 0.5f, 0);
            stats.ifzz = false;
            enterTime = Time.time;
            gameOver = false;
            UpdateText();
            stats.people += successCount;
        }
        if (gameOver && Input.GetKeyDown(KeyCode.Return)) 
        {
                eventManager.OverQTE();
                return;
        }

        /* 1. 持续向左漂移 */
        pointRt.anchoredPosition += Vector2.left * fallSpeed * Time.deltaTime;

        /* 2. 夹住左边界，防止掉出去 */
        float leftEdge = -barWidth * 0.5f;
        pointRt.anchoredPosition =
            new Vector2(Mathf.Max(pointRt.anchoredPosition.x, leftEdge),
                        pointRt.anchoredPosition.y);

        /* 3. 狂按空格 */
        if (Input.GetKeyDown(KeyCode.Space)&&!gameOver)
        {
            targetPulse.Pulse();
            PushRight();
            audioSrc?.PlayOneShot(successSFX);
        }

        /* 4. 检测到达右端 */
        float rightEdge = barWidth * 0.5f;
        if (pointRt.anchoredPosition.x >= rightEdge)
            OnSuccess();

        if(Time.time-enterTime>=8&&gameOver==false)
        {
            gameOver = true;
            
            UpdateText();
        }
    }

    void PushRight()
    {
        if (Time.time == lastPushTime) return;   // 防同一帧多次
        lastPushTime = Time.time;

        pointRt.anchoredPosition += Vector2.right * pushForce;
    }

    void OnSuccess()
    {
        successCount++;
        audioSrc?.PlayOneShot(successSFX);
        StartCoroutine(SuccessAnim());
        ResetPoint();
        UpdateText();
    }

    void ResetPoint()
    {
        pointRt.anchoredPosition = new Vector2(-barWidth * 0.5f, 0);
    }

    IEnumerator SuccessAnim()
    {
        pointRt.localScale = normalScale * 1.4f;
        yield return new WaitForSeconds(0.2f);
        pointRt.localScale = normalScale;
    }

    void UpdateText()
    {
        if (statusText == null) return;

        if (gameOver)
            statusText.text = $"成功发起 {successCount} 次进攻\n按Enter以回到朝堂";
        else
            statusText.text = $"快速按 空格 可以擂鼓\n可以振奋我军士气";
    }
}