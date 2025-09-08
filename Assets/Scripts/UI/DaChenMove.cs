// using UnityEngine;

// /// <summary>
// /// 平滑移动到固定坐标 + 翻转
// /// 挂在 Logo 的 Image 上即可
// /// </summary>
// [RequireComponent(typeof(RectTransform))]
// public class DaChenMove : MonoBehaviour
// {
//     [Tooltip("向左移动到的目标 X 坐标")]
//     public float leftTargetX = -840f;

//     [Tooltip("向右移动到的目标 X 坐标")]
//     public float rightTargetX = 930f;

//     [Tooltip("移动耗时（秒）")]
//     public float moveTime = 0.8f;

//     private RectTransform rt;

//     private void Awake()
//     {
//         rt = GetComponent<RectTransform>();
//     }

//     // 外部调用 -------------------------------------------------
//     public void MoveLeft() => StartCoroutine(Move(leftTargetX));
//     public void MoveRight() => StartCoroutine(Move(rightTargetX));
//     public void FlipX() => rt.rotation *= Quaternion.Euler(0, 180, 0);

//     // 协程：平滑移动到指定 X -------------------------------
//     private System.Collections.IEnumerator Move(float targetX)
//     {
//         Vector2 start = rt.anchoredPosition;
//         Vector2 end = new Vector2(targetX, start.y);

//         float timer = 0f;
//         while (timer < moveTime)
//         {
//             timer += Time.deltaTime;
//             rt.anchoredPosition = Vector2.Lerp(start, end, timer / moveTime);
//             yield return null;
//         }
//         rt.anchoredPosition = end;        // 确保最终位置精确
//     }
// }