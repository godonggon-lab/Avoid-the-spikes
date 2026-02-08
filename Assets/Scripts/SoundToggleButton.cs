using UnityEngine;
using UnityEngine.UI;

public class SoundToggleButton : MonoBehaviour
{
    [Header("UI References")]
    public Image buttonImage;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(ToggleSound);
        }
    }

    void Start()
    {
        UpdateIcon();
    }

    void ToggleSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
            UpdateIcon();
        }
    }

    void UpdateIcon()
    {
        if (AudioManager.Instance != null && buttonImage != null)
        {
            bool isMuted = AudioManager.Instance.IsMuted();
            buttonImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
        }
    }
}
