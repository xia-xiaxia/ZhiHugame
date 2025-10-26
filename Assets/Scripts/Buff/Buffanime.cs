using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buffanime : MonoBehaviour
{
    public static Buffanime Instance;
    public GameObject buffImage1;
    public GameObject buffImage2;
    void Start()
    {
        buffImage1.SetActive(false);
        buffImage2.SetActive(false);
    }

    void Update()
    {

    }

    public void Buffstart()
    {
        buffImage1.SetActive(false);
        buffImage2.SetActive(false);
    }

    public IEnumerator playAnime(int x)
    {
        if (x > 0)
        {
            buffImage1.SetActive(true);

            yield return new WaitForSeconds(1f);
            buffImage1.SetActive(false);
        }
        else if (x < 0)
        {
            buffImage2.SetActive(true);

            yield return new WaitForSeconds(1f);
            buffImage2.SetActive(false);
        }

        yield break;
    }
    
    public void BuffStay()
    {
        StartCoroutine(playAnime(1));
    }
}
