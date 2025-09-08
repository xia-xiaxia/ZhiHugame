using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QTE4 : MonoBehaviour
{
    public StatModel stats;          // 用于记录成绩
    public EventManager eventManager;

    [Header("UI")]
    public Image point;              // 判定点
    public Text statusText;          // 提示文字

    [Header("音效")]
    public AudioSource audioSrc;
    public AudioClip inAreaSFX;      // 第一次进入目标区
    public AudioClip failSFX;        // 掉出目标区

    [Header("参数")]
    public float fallSpeed = 250f;   // 左漂速度（像素/秒）
    public float pushForce = 80f;    // 空格推力（像素）
    public float barWidth = 600f;    // 判定条总宽（像素）
    public float targetHoldTime = 5f;// 需要连续维持的时长

    /* ------------- 运行数据 ------------- */
    private RectTransform pointRt;
    private bool inTargetArea;       // 当前是否在目标区
    private int everEntered;        // 0未进入过 1进入了 2出来了
    private float enterTime;         // 第一次进入目标区的时间
    public float holdTime;          // 已维持的时长


    /* ------------- 视觉缓存 ------------- */
    private Vector3 normalScale;
    public GameObject bar;
    /* 计算目标区左右边界 */
    private float targetLeft => -barWidth * 0.16f;   // 中间 1/3 左边界
    private float targetRight => barWidth * 0.16f;    // 中间 1/3 右边界

    [Header("目标区视觉")]
    public Color targetAreaColor = Color.red; // 灰+半透明

    private Image targetAreaImg;   // 动态创建的目标区
    public ImageQuickPulse targetPulse;
    void Start()
    {
        pointRt = point.GetComponent<RectTransform>();
        normalScale = pointRt.localScale;
        UpdateText();
    }

    void Update()
    {

        if (everEntered==2 && Input.GetKeyDown(KeyCode.Return))
        {
            Destroy(targetAreaImg.gameObject);
            stats.weiwang += Mathf.FloorToInt(holdTime);
            eventManager.OverQTE();
            return;
        }
        if (stats.ifhm)
        {
            everEntered = 0;
            pointRt.anchoredPosition = new Vector2(-barWidth * 0.5f, 0);
            stats.ifhm = false;
            CreateTargetAreaVisual();
            stats.ifhm = false;
            UpdateText();
        }
        /* 1. 持续向左漂移 */
        pointRt.anchoredPosition += Vector2.left * fallSpeed * Time.deltaTime;

        /* 2. 夹住左右边界，防止掉出整条判定条 */
        float leftEdge = -barWidth * 0.5f;
        float rightEdge = barWidth * 0.5f;
        float x = Mathf.Clamp(pointRt.anchoredPosition.x, leftEdge, rightEdge);
        pointRt.anchoredPosition = new Vector2(x, pointRt.anchoredPosition.y);

        /* 3. 空格向右推 */
        if (Input.GetKeyDown(KeyCode.Space)&&everEntered!=2)
            PushRight();

        /* 4. 判断是否在目标区 */
        bool nowIn = x >= targetLeft && x <= targetRight;

        /* 第一次进入 */
        if (nowIn && everEntered==0)
        {
            everEntered = 1;
            inTargetArea = true;
            enterTime = Time.time;
            audioSrc?.PlayOneShot(inAreaSFX);
        }

        /* 持续在目标区内计时 */
        if (nowIn && everEntered==1)
        {
            holdTime = Time.time - enterTime;
            if (holdTime >= targetHoldTime) everEntered = 2;
        }

        /* 第一次掉出目标区 */
        if (!nowIn && everEntered == 1 && inTargetArea)
        {
            inTargetArea = false;
            audioSrc?.PlayOneShot(failSFX);
            everEntered=2;
        }

        UpdateText();
    }

    void PushRight()
    {
        pointRt.anchoredPosition += Vector2.right * pushForce;
        audioSrc?.PlayOneShot(inAreaSFX);
        targetPulse.Pulse();
    }



    /* 结束 QTE */

    void UpdateText()
    {
        if (statusText == null) return;

        if (everEntered==2)
        {
            statusText.text = $"会盟圆满结束！\n按Enter以回到朝堂\n";
        }
        else
        {
            if (everEntered==0)
                statusText.text = "按 空格 主持会盟\n要被主持在合适的程度";
            else
                statusText.text = $"主持中… {holdTime:F2} / {targetHoldTime:F1} 天";
        }
    }

    void CreateTargetAreaVisual()
    {
        // 动态新建
        GameObject go = new GameObject("TargetArea");
        go.transform.SetParent(bar.transform); // 放在同一层级
        targetAreaImg = go.AddComponent<Image>();
        targetAreaImg.color = targetAreaColor;
        targetAreaImg.raycastTarget = false; // 不拦截点击

        // RectTransform 设置
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(barWidth * 0.33f, 90f); // 高度自己改
        rt.anchoredPosition = Vector2.zero;                  // 居中
    }
}