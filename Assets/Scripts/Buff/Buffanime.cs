using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buffanime : MonoBehaviour
{
    public static Buffanime Instance;
    public GameObject buffImage1;
    public GameObject buffImage2;

    private Coroutine currentCoroutine = null;

    //设置为和光效持续时间相同
    float duration;
    void Start()
    {
        buffImage1.SetActive(false);
        buffImage2.SetActive(false);

        duration = GetComponent<StatEffectController>().effectDuration;
    }

    public void ShowBuff(int delta)
    {
        if(delta > 0)
        {
            buffImage1.SetActive(true);
            buffImage2.SetActive(false);
        } else if(delta < 0)
        {
            buffImage1.SetActive(false);
            buffImage2.SetActive(true);
        } else
        {
            buffImage1.SetActive(false);
            buffImage2.SetActive(false);
        }
    }

    public void PlayBuffAnime(int delta)
    {

       if(currentCoroutine != null)
       {
           StopCoroutine(currentCoroutine);
           buffImage1.SetActive(false);
           buffImage2.SetActive(false);
       }

       currentCoroutine = StartCoroutine(playAnime(delta));
    }

    public IEnumerator playAnime(int x)
    {
        if (x > 0)
        {
            buffImage1.SetActive(true);

            yield return new WaitForSeconds(duration);
            buffImage1.SetActive(false);
        }
        else if (x < 0)
        {
            buffImage2.SetActive(true);

            yield return new WaitForSeconds(duration);
            buffImage2.SetActive(false);
        }

        yield break;
    }
    

}
