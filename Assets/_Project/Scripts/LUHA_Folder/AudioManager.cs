using Unity.VisualScripting;
using UnityEngine;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public enum Sfx { };//나중에 사운드 넣으면 추가하기


    [Header("#BGM")]
    public AudioClip bgmClip;
    [Range(0f, 1f)]
    public float bgmVolume;
    AudioSource bgmPlayer;

    [Header("#SFX")]
    public AudioClip[] sfxClips;
    [Range(0f, 1f)]
    public float sfxVolume;
    public int channels=20;
    AudioSource[] sfxPlayers;
    int channelIndex;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);

            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Init()
    {
        GameObject bgmObject = new GameObject("BGM Player");

        // AudioManager의 자식으로 설정
        bgmObject.transform.parent = transform;

        // BGM Player에 AudioSource 추가
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        
        // 게임이 시작되자마자 자동 재생하지 않음
        bgmPlayer.playOnAwake = false;
        
        // 배경음악 반복 재생
        bgmPlayer.loop = true;

        // Inspector에서 설정한 볼륨 적용
        bgmPlayer.volume = bgmVolume;

        // 재생할 음악 지정
        bgmPlayer.clip = bgmClip;

        // 음악 재생
        bgmPlayer.Play();

        GameObject sfxObject = new GameObject("SFX Player");

        sfxObject.transform.parent = transform;

        // AudioSource 배열 생성
        sfxPlayers = new AudioSource[channels];

        // AudioSource 여러 개 생성
        for (int i = 0; i < channels; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
        }
    }
    // 효과음 재생
    // index : 재생할 효과음 번호
    public void PlaySfx(int index)
    {
        for (int i = 0; i < channels; i++)
        {
            // 다음 채널로 이동
            channelIndex++;

            // 마지막이면 처음으로
            if (channelIndex >= channels)
                channelIndex = 0;

            // 사용 중이면 다음 채널 확인
            if (sfxPlayers[channelIndex].isPlaying)
                continue;

            // 사용할 효과음 지정
            sfxPlayers[channelIndex].clip = sfxClips[index];

            // 재생
            sfxPlayers[channelIndex].Play();

            break;
        }
    }



}
