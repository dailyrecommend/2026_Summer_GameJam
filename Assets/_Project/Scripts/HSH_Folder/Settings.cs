using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    
    public GameObject slider1;
    public GameObject slider2;

    public AudioManager volume;

    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    public void UpdateText(float value)
    {
        int percent = Mathf.RoundToInt(value);
        progressText.text = percent + "%";
        if (progressBar.gameObject == slider1)
        {
            volume.SetSfxVolume(percent);
        }
        else
        {
            volume.SetBgmVolume(percent);
        }
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
