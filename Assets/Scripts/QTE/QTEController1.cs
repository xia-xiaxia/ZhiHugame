//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class QTEController : MonoBehaviour
//{
//    public StatModel stats;
//    public EventManager eventManager;
//    [Header("UI")]
//    public Image point;
//    public Text statusText;

//    [Header("��Ч")]
//    public AudioSource audioSrc;
//    public AudioClip successSFX;
//    public AudioClip failSFX;

//    [Header("����")]
//    public float speed = 400f;
//    public float barWidth = 600f;
//    public float coolDown = 0.2f;

//    /* ---------- ���� ---------- */
//    public int successCount = 0;
//    public int failCount = 0;
//    public int combo = 0;
//    public int maxCombo = 0;

//    /* ---------- ���п��� ---------- */
//    private bool movingRight = true;
//    private bool gameOver = false;
//    private RectTransform pointRt;
//    private float lastHitTime = -999f;

//    /* ---------- �ж������ӻ� ---------- */
//    private Image leftZoneImg;
//    private Image rightZoneImg;
//    private readonly Color zoneColor = Color.red; // ��͸��

//    /* ---------- �Ӿ����� ---------- */
//    private Vector3 normalScale;
//    public GameObject bar;
//    public ImageQuickPulse targetPulse;

//    private float activeLeft;   // ��Ч������ x
//    private float activeRight;  // ��Ч������ x


//    void Start()
//    {
//        pointRt = point.GetComponent<RectTransform>();
//        normalScale = pointRt.localScale;


//    }

//    /* ---------- ���������ж��� ---------- */
//    void CreateZoneImages()
//    {
//        float barRealWidth = bar.GetComponent<RectTransform>().rect.width; // ���� 1200
//        activeLeft = -barRealWidth * 0.5f + (barRealWidth - barWidth) * 0.5f;
//        activeRight = activeLeft + barWidth;

//        float zoneWidth = barWidth / 6f;

//        // �Ѻ���������Ч������
//        leftZoneImg = MakeZoneImage("LeftZone", activeLeft);
//        rightZoneImg = MakeZoneImage("RightZone", activeRight - zoneWidth);
//    }
//    Image MakeZoneImage(string name, float leftEdge)
//    {
//        GameObject go = new GameObject(name);
//        go.transform.SetParent(bar.transform);
//        Image img = go.AddComponent<Image>();
//        img.color = zoneColor;
//        img.raycastTarget = false;

//        RectTransform rt = go.GetComponent<RectTransform>();
//        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barWidth / 6f);
//        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 90);
//        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
//        rt.pivot = new Vector2(0, 0.5f);
//        rt.anchoredPosition = new Vector2(leftEdge, 0);

//        return img;
//    }

//    /* ---------- �����߼���ɰ���ȫһ�� ---------- */
//    void Update()
//    {
//        if (stats.ifjs)
//        {
//            CreateZoneImages();  // ���������ж���
//            UpdateText();
//            stats.ifjs=false;
//            combo = 0;
//            UpdateText();
            
//        }
//        if (gameOver && Input.GetKeyDown(KeyCode.Return))
//        {
//            gameOver = false;
//            Destroy(leftZoneImg.gameObject);
//            Destroy(rightZoneImg.gameObject);
//            eventManager.OverQTE();
//            return;
//        }

//        MovePoint();

//        if (Input.GetKeyDown(KeyCode.Space))
//            OnSpace();
//    }

//    void MovePoint()
//    {
//        float step = speed * Time.deltaTime * (movingRight ? 1 : -1);
//        pointRt.anchoredPosition += new Vector2(step, 0);

//        if (pointRt.anchoredPosition.x >= activeRight) movingRight = false;
//        if (pointRt.anchoredPosition.x <= activeLeft) movingRight = true;
//    }

//    void OnSpace()
//    {
//        if (gameOver) return;
//        if (Time.time - lastHitTime < coolDown) return;
//        lastHitTime = Time.time;

//        bool ok = IsInLeftZone() || IsInRightZone();

//        if (ok)
//        {
//            successCount++;
//            combo++;
//            if (combo > maxCombo) maxCombo = combo;
//            targetPulse.Pulse();
//            audioSrc?.PlayOneShot(successSFX);
//            StartCoroutine(SuccessAnim());

//            if (successCount + failCount >= 8) EndGame();
//        }
//        else
//        {
//            failCount++;
//            combo = 0;
//            audioSrc?.PlayOneShot(failSFX);
//            if (successCount + failCount >= 8) EndGame();
//        }

//        UpdateText();
//    }

//    bool IsInLeftZone()
//    {
//        float zoneWidth = barWidth / 6f;
//        float px = pointRt.anchoredPosition.x;
//        return px >= activeLeft && px <= activeLeft + zoneWidth;
//    }

//    bool IsInRightZone()
//    {
//        float zoneWidth = barWidth / 6f;
//        float px = pointRt.anchoredPosition.x;
//        return px <= activeRight && px >= activeRight - zoneWidth;
//    }

//    void EndGame()
//    {
//        gameOver = true;
//        stats.zhouli += successCount / 2;
//        UpdateText();
//    }

//    IEnumerator SuccessAnim()
//    {
//        pointRt.localScale = normalScale * 1.3f;
//        yield return new WaitForSeconds(0.2f);
//        pointRt.localScale = normalScale;
//    }

//    void UpdateText()
//    {
//        if (statusText == null) return;

//        if (gameOver)
//            statusText.text = $"���μ���Բ��������\n��Enter�Իص�����\n";
//        else
//            statusText.text = $"��׼ʱ�����ո��޹�\n�ɹ�:{successCount}  ʧ��:{failCount}  ����:{combo}";
//    }
//}