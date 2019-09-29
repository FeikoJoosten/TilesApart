using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AudioTogglers : MonoBehaviour {

    [Header("Sprites")]
    [SerializeField]
    private Sprite enabledSprite = null;
    [SerializeField]
    private Sprite disabledSprite = null;
    [SerializeField]
    private AudioType audioType = AudioType.SFX;

    private Image imageToUse;

    private void Awake() {
        imageToUse = GetComponent<Image>();
    }

    public void Start() {
        AudioManager manager = AudioManager.Instance;

        switch (audioType) {
            case AudioType.SFX:
                UpdateSprite(manager.SfxEnabled);
                break;
            case AudioType.Music:
                UpdateSprite(manager.MusicEnabled);
                break;
        }
    }

    public void ToggleSFXSounds() {
        AudioManager manager = AudioManager.Instance;
        manager.ToggleSFX();

        UpdateSprite(manager.SfxEnabled);
    }

    public void ToggleMusicSounds() {
        AudioManager manager = AudioManager.Instance;
        manager.ToggleMusic();

        UpdateSprite(manager.MusicEnabled);
    }

    // Update sprites from when audio changes
    public void UpdateSprite(bool enabled) {
        if (imageToUse == null) return;

        imageToUse.sprite = enabled == true ? enabledSprite : disabledSprite;
    }
}
