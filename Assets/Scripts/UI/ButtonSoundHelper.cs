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
        GameButton,      // 游戏中按钮 - 使用 buttonSound2
        ShopButton       // 结局界面和道具购买界面按钮 - 使用 buttonSound3
    }

    [Header("按钮类型")]
    public ButtonType buttonType = ButtonType.GameButton;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        
        if (button != null)
        {
            // 在按钮点击时添加音效
            button.onClick.AddListener(PlayButtonSound);
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
