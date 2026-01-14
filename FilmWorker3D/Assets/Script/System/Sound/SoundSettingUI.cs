using UnityEngine;
using UnityEngine.UI;

public class SoundSettingUI : MonoBehaviour
{
    [Header("UIスライダー")]
    public Slider bgmSlider;
    public Slider seSlider;

    private const string BgmPrefKey = "BgmVolume";
    private const string SePrefKey = "SeVolume";

    void Start()
    {
        // 保存された音量設定を読み込み（なければ1.0f）
        float savedBgm = PlayerPrefs.GetFloat(BgmPrefKey, 1.0f);
        float savedSe = PlayerPrefs.GetFloat(SePrefKey, 1.0f);

        // SoundManagerに反映
        SoundManager.BgmVolume = savedBgm;
        SoundManager.SEVolume = savedSe;

        // スライダーに反映
        bgmSlider.value = savedBgm;
        seSlider.value = savedSe;

        // イベント登録
        bgmSlider.onValueChanged.AddListener(SetBgmVolume);
        seSlider.onValueChanged.AddListener(SetSeVolume);

        // 再生中のBGMにも反映
        var source = SoundManager.GetBgmSource();
        if (source != null)
        {
            source.volume = savedBgm;
        }
    }

    public void SetBgmVolume(float volume)
    {
        SoundManager.BgmVolume = volume;
        PlayerPrefs.SetFloat(BgmPrefKey, volume);

        var source = SoundManager.GetBgmSource();
        if (source != null)
        {
            source.volume = volume;
        }
    }

    public void SetSeVolume(float volume)
    {
        SoundManager.SEVolume = volume;
        PlayerPrefs.SetFloat(SePrefKey, volume);
    }
}
