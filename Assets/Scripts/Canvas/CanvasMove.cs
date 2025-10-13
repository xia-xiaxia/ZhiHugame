using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CanvasMove : MonoBehaviour
{
    public static CanvasMove Instance;
    void Awake()
    {
        Instance = this;
        isReady = false;
    }

    public Transform startPanel;        // 起始位置
    public float moveDuration = 1f;       // 动画时间

    public bool isReady = false;          // 相机是否准备好

    void Start()
    {
        if (transform.position != startPanel.position)
            startPanel.position = transform.position;
        isReady = false;
    }
    
    public void StartGame()
    {
        StartCoroutine(MoveUP());
    }

    IEnumerator MoveUP()
    {
        Vector3 startBeginPos = startPanel.position;
        Vector3 startEndPos = startPanel.position + (new Vector3(0, 1800, 0));

        float timer = 0;
        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / moveDuration;
            startPanel.position = Vector3.Lerp(startBeginPos, startEndPos, t);
            yield return null;
        }
        startPanel.position = startEndPos;
        startPanel.gameObject.SetActive(false);
        isReady = true;
    }
    

}