using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QTE2 : MonoBehaviour
{
    public StatModel stats;
    public EventManager eventManager;
    [Header("UI")]
    public Image point;
    public Text statusText;

    [Header("音效")]
    public AudioSource audioSrc;
    public AudioClip successSFX;
    public AudioClip failSFX;

    [Header("参数")]
    public float speed = 400f;
    public float barWidth = 600f;
    public float coolDown = 0.2f;

    /* ---------- 数据 ---------- */
    public int successCount = 0;
    public int failCount = 0;
    private int combo = 0;
    private int maxCombo = 0;

    /* ---------- 运行控制 ---------- */
    private bool movingRight = true;
    private bool gameOver = false;
    private RectTransform pointRt;
    private float lastHitTime = -999f;

    /* ---------- 判定区 ---------- */
    private float zoneLeft;     // 当前判定区左边界（世界坐标）
    private float zoneWidth;    // 判定区宽度，固定为 barWidth/6
    private Image zoneImg;      // 用来显示判定区的 Image

    /* ---------- 视觉缓存 ---------- */
    private Vector3 normalScale;
    public GameObject bar;
    public ImageQuickPulse targetPulse;

    void Start()
    {
        pointRt = point.GetComponent<RectTransform>();
        normalScale = pointRt.localScale;

        
        UpdateText();
    }

    void Update()
    {
        if(stats.iftl)
        {
            // 动态创建一个判定区图片
            GameObject zoneGo = new GameObject("ZoneImg");
            zoneGo.transform.SetParent(bar.transform);
            zoneImg = zoneGo.AddComponent<Image>();
            zoneImg.color = Color.red;   // 半透明白色，可自行换色
            zoneImg.raycastTarget = false;

            zoneWidth = barWidth / 5f;
            RectTransform zoneRt = zoneGo.GetComponent<RectTransform>();
            zoneRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, zoneWidth);
            zoneRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 90); // 与背景条同高
            zoneRt.anchorMin = zoneRt.anchorMax = new Vector2(0.5f, 0.5f);
            zoneRt.pivot = new Vector2(0, 0.5f);
            combo = 0;
            SpawnZone();   // 首次随机位置
            stats.iftl = false;
            UpdateText();
        }
        if (gameOver&& Input.GetKeyDown(KeyCode.Return))
        {
            gameOver = false;
            Destroy(zoneImg.gameObject);
            eventManager.OverQTE();
            return;
        }

        MovePoint();

        if (Input.GetKeyDown(KeyCode.Space))
            OnSpace();
    }

    /* ---------- 移动点 ---------- */
    void MovePoint()
    {
        float half = barWidth * 0.5f;
        float step = speed * Time.deltaTime * (movingRight ? 1 : -1);
        pointRt.anchoredPosition += new Vector2(step, 0);

        if (pointRt.anchoredPosition.x >= half) movingRight = false;
        if (pointRt.anchoredPosition.x <= -half) movingRight = true;
    }

    /* ---------- 空格判定 ---------- */
    void OnSpace()
    {
        if (gameOver) return;
        if (Time.time - lastHitTime < coolDown) return;
        lastHitTime = Time.time;

        float posX = pointRt.anchoredPosition.x;
        bool ok = posX >= zoneLeft && posX <= zoneLeft + zoneWidth;

        if (ok)
        {
            targetPulse.Pulse();
            successCount++;
            combo++;
            if (combo > maxCombo) maxCombo = combo;

            audioSrc.PlayOneShot(successSFX);
            StartCoroutine(SuccessAnim());

            SpawnZone();                 // ← 重新随机判定区

            if (successCount+failCount >= 8) EndGame();
        }
        else
        {
            failCount++;
            combo = 0;
            audioSrc.PlayOneShot(failSFX);
            if (successCount + failCount >= 8) EndGame();
        }

        UpdateText();
    }

    /* ---------- 随机生成判定区 ---------- */
    void SpawnZone()
    {
        float half = barWidth * 0.5f;
        float maxLeft = -half;                       // 最左端
        float maxRight = half - zoneWidth;           // 最右端（保证不越界）
        zoneLeft = Random.Range(maxLeft, maxRight);

        zoneImg.rectTransform.anchoredPosition = new Vector2(zoneLeft, 0);
    }

    /* ---------- 结束 ---------- */
    void EndGame()
    {
        gameOver = true;
        stats.gold += successCount / 2;
        UpdateText();
    }

    /* ---------- 成功动画 ---------- */
    IEnumerator SuccessAnim()
    {
        pointRt.localScale = normalScale * 1.3f;
        yield return new WaitForSeconds(0.2f);
        pointRt.localScale = normalScale;
    }

    /* ---------- 文本 ---------- */
    void UpdateText()
    {
        if (statusText == null) return;

        if (gameOver)
            statusText.text = $"本次田猎收获颇丰！\n按Enter以回到朝堂\n";
        else
            statusText.text = $"看准时机按空格射箭\n成功:{successCount}  失败:{failCount}  连击:{combo}";
    }
}