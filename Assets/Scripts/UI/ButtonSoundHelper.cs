using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 按钮音效辅助组件
/// 自动为按钮添加音效，根据按钮类型播放不同音效
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSoundHelper : MonoBehaviour
{
    public enum ButtonType
    {
        MenuButton,      // 开始界面按钮 - 使用 buttonSound1
        ShopButton,      // 游戏中按钮 - 使用 buttonSound2
        GameButton       // 结局界面和道具购买界面按钮 - 使用 buttonSound3
    }

    [Header("按钮类型")]
    public ButtonType buttonType = ButtonType.GameButton;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        
        if (button != null)
        {
 
            // 在 Start() 中添加监听器，确保在其他脚本的 Start() RemoveAllListeners() 之后执行
            // Unity 的 Start() 执行顺序不确定，所以延迟一帧确保
            StartCoroutine(AddListenerNextFrame());
        }
    }

    private System.Collections.IEnumerator AddListenerNextFrame()
    {

        // 等待一帧，确保所有 Start() 中的 RemoveAllListeners() 都已执行
        yield return null;
        if (button != null)
        {
            // 移除可能存在的旧监听器（避免重复添加）
            button.onClick.RemoveListener(PlayButtonSound);
            // 添加音效监听器
            button.onClick.AddListener(PlayButtonSound);
            Debug.Log($"[ButtonSoundHelper] 为按钮 {gameObject.name} 添加了 {buttonType} 类型的音效");
        }
    }

    private void PlayButtonSound()
    {
        if (MusicManager.Instance == null) return;

        switch (buttonType)
        {
            case ButtonType.MenuButton:
                MusicManager.Instance.PlayButtonSound1();
                break;
            case ButtonType.GameButton:
                MusicManager.Instance.PlayButtonSound2();
                break;
            case ButtonType.ShopButton:
                MusicManager.Instance.PlayButtonSound3();
                break;
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayButtonSound);
        }
    }
}
