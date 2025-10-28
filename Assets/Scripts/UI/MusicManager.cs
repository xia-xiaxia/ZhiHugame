using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("音效配置")]
    public AudioClip menuBgm;           // 音效1：开始界面
    public AudioClip gameBgm;           // 音效2：游戏中
    public AudioClip deathSound;        // 音效3：数值超阈值（死亡音效）
    public AudioClip endingBgm;         // 音效4：结局图

    [Header("音量设置")]
    [Range(0f, 1f)]
    public float bgmVolume = 0.7f;
    [Range(0f, 1f)]
    public float deathSoundVolume = 1f;

    private AudioSource bgmSource;      // 背景音乐源
    private AudioSource sfxSource;      // 音效源（用于死亡音效）

    private void Awake()
    {
        // 单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);    // 跨场景不销毁
            
            // 创建两个 AudioSource：一个播放BGM，一个播放音效
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
            
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = deathSoundVolume;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 播放开始界面音乐
    public void PlayMenuMusic()
    {
        PlayBgm(menuBgm, bgmVolume);
        Debug.Log("[MusicManager] 播放开始界面音乐");
    }

    // 播放游戏中音乐
    public void PlayGameMusic()
    {
        PlayBgm(gameBgm, bgmVolume);
        Debug.Log("[MusicManager] 播放游戏音乐");
    }

    // 播放死亡音效（不循环）
    public void PlayDeathSound()
    {
        if (deathSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(deathSound, deathSoundVolume);
            Debug.Log("[MusicManager] 播放死亡音效");
        }
    }

    // 播放结局音乐
    public void PlayEndingMusic()
    {
        PlayBgm(endingBgm, bgmVolume);
        Debug.Log("[MusicManager] 播放结局音乐");
    }

    // 通用 BGM 播放方法
    public void PlayBgm(AudioClip clip, float volume = 1f)
    {
        if (clip == null || bgmSource == null) return;

        // 如果正在播放相同的音乐，不重复播放
        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.Play();
    }

    // 停止背景音乐
    public void StopBgm()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }
}