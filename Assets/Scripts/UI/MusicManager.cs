using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private AudioSource src;

    private void Awake()
    {
        // 单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);    // 跨场景不销毁
            src = gameObject.AddComponent<AudioSource>();
            src.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 其他脚本调用：换 BGM
    public void PlayBgm(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        src.clip = clip;
        src.volume = volume;
        src.Play();
    }
}