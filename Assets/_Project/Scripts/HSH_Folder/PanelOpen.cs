using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject panel;
    public GameObject image;
    public GameObject button;
    public GameObject Text1;
    public GameObject Text2;
    public GameObject slider1;
    public GameObject slider2;

    void Start()
    {
        panel.SetActive(false);
        image.SetActive(false);
        button.SetActive(false);
        Text1.SetActive(false);
        Text2.SetActive(false);
        slider1.SetActive(false);
        slider2.SetActive(false);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!panel.activeSelf)
            {
                panel.SetActive(true);
                image.SetActive(true);
                button.SetActive(true);
                Text1.SetActive(true);
                Text2.SetActive(true);
                slider1.SetActive(true);
                slider2.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                panel.SetActive(false);
                image.SetActive(false);
                button.SetActive(false);
                Text1.SetActive(false);
                Text2.SetActive(false);
                slider1.SetActive(false);
                slider2.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }
}
