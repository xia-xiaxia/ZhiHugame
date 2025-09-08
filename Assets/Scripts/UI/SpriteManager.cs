using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteManager : MonoBehaviour
{
    public Image tianzi;
    public Image qunchen;
    public GameObject qunchen2;
    public Image background;


    public StatModel stats;

    public Sprite tz1;
    public Sprite tz2;
    public Sprite tz3;

    public Sprite bg1;
    public Sprite bg2;
    // Start is called before the first frame update
    public AudioClip Bgm;
    void Start()
    {
        qunchen2.SetActive(false);
        stats.zhouli = 10;
        stats.people = 10;
        stats.gold = 10;
        stats.weiwang = 10;
        stats.iftl=false;
        stats.ifjs = false;
        stats.ifzz = false;
        stats.ifhm = false;
        MusicManager.Instance.PlayBgm(Bgm,0.8f);
    }
    // Update is called once per frame
    void Update()
    {
        if (stats.gold > 20)
            tianzi.sprite = tz2;
        else if(stats.gold >25)
            tianzi.sprite= tz3;

        if (stats.gold > 15)
            background.sprite = bg2;

        if (stats.people > 15)
            qunchen2.SetActive(true);
    }
}
