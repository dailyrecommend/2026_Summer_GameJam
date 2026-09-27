using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public AudioManager volume;

    [Header("UI 텍스트 연결")]
    [SerializeField] private TMP_Text sfxProgressText;
    [SerializeField] private TMP_Text bgmProgressText;

    // 유니티 인스펙터에서 SFX 슬라이더의 OnValueChanged에 이 함수를 연결하세요.
    public void UpdateSfxVolume(float value)
    {
        int percent = Mathf.RoundToInt(value);
        if (sfxProgressText != null)
            sfxProgressText.text = percent + "%";

        volume.SetSfxVolume(percent);
    }

    // 유니티 인스펙터에서 BGM 슬라이더의 OnValueChanged에 이 함수를 연결하세요.
    public void UpdateBgmVolume(float value)
    {
        int percent = Mathf.RoundToInt(value);
        if (bgmProgressText != null)
            bgmProgressText.text = percent + "%";

        volume.SetBgmVolume(percent);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}