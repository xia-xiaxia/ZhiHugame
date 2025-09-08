using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class stmcode : MonoBehaviour
{
    public AudioClip Bgm;
    public GameObject stm;
    public GameObject tishi;
    public Image bg;
    public Sprite bg2;
    private bool over=false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKeyDown && !over)
        {
            over = true;
            tishi.SetActive(false);
            MusicManager.Instance.PlayBgm(Bgm);
            bg.sprite= bg2;
        }
        else if(over&Input.GetKeyDown(KeyCode.Return))stm.SetActive(false);
    }
}
