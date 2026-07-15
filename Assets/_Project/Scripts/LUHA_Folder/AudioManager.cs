using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    // sfxClips 배열 순서를 이 enum 순서와 맞춰서 인스펙터에 채운다.
    public enum Sfx
    {
        CardFlip,
        CardHover,
        CardSelect,
        CardDraw,
        PanelSwitch,
        StageSwitch,
        SpecialActivate,
        PlayerRoundWin, // 승부에서 플레이어가 이겼을 때(이름만 바뀜, 기존 CardWin과 같은 인덱스 — 기존 인스펙터 클립 유지됨)
        RoundDraw, // 무승부(카드 드로우의 CardDraw와 구분)
        EndingClear, // 마지막 스테이지 클리어 → 엔딩 진입
        PlayerRoundLose, // 승부에서 플레이어가 졌을 때
        StageWin,        // 스테이지(게임) 승리
        StageLose,       // 스테이지(게임) 패배
    }

    public enum Bgm { } // bgm 목록 추가하기

    [Header("#BGM")]
    public AudioClip[] bgmClips;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;
    AudioSource bgmPlayer;

    [Header("#SFX")]
    public AudioClip[] sfxClips;
    [Range(0f, 1f)]
    public float sfxVolume = 0.5f;
    public int channels = 20;
    AudioSource[] sfxPlayers;
    int channelIndex;

    [Header("#SFX 겹침 방지")]
    [Tooltip("같은 효과음이 이 시간(초) 안에 다시 요청되면 무시(초근접 중복 호출로 커지는 것 방지)")]
    [SerializeField] float sfxCooldown = 0.03f;
    [Tooltip("같은 클립이 동시에 겹쳐 재생될 수 있는 최대 개수(0=제한 없음)")]
    [SerializeField] int maxVoicesPerClip = 3;
    float[] _lastPlayTime; // sfxClips 인덱스별 마지막 재생 시각(unscaled)

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

        // 씬이 전환되더라도 자동 재생되지 않도록 설정
        bgmPlayer.playOnAwake = false;

        // 배경음은 반복 재생
        bgmPlayer.loop = true;

        // Inspector에서 설정한 볼륨 적용
        bgmPlayer.volume = bgmVolume;

        // 시작 클립이 있으면 재생 (없으면 스킵 — 배열 비어있어도 크래시 안 나게)
        if (bgmClips != null && bgmClips.Length > 0 && bgmClips[0] != null)
        {
            bgmPlayer.clip = bgmClips[0];
            bgmPlayer.Play();
        }

        GameObject sfxObject = new GameObject("SFX Player");

        sfxObject.transform.parent = transform;

        // AudioSource 배열 생성
        sfxPlayers = new AudioSource[channels];

        // AudioSource 생성 및 설정
        for (int i = 0; i < channels; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
        }

        _lastPlayTime = new float[sfxClips != null ? sfxClips.Length : 0];
        for (int i = 0; i < _lastPlayTime.Length; i++) _lastPlayTime[i] = -999f;
    }

    /// <summary>효과음 재생. index: 재생할 효과음 번호(sfxClips 인덱스).</summary>
    public void PlaySfx(int index)
    {
        if (sfxClips == null || index < 0 || index >= sfxClips.Length || sfxClips[index] == null) return;
        if (sfxPlayers == null || sfxPlayers.Length == 0) return;

        // 1) 아주 짧은 시간 안에 같은 효과음이 재요청되면 무시(같은 프레임/직후 중복 호출 방지).
        if (_lastPlayTime != null && index < _lastPlayTime.Length && sfxCooldown > 0f)
        {
            if (Time.unscaledTime - _lastPlayTime[index] < sfxCooldown) return;
        }

        AudioClip clip = sfxClips[index];

        // 2) 같은 클립이 이미 여러 채널에서 겹쳐 재생 중이면(과도한 중첩) 더 이상 안 늘림.
        if (maxVoicesPerClip > 0)
        {
            int playing = 0;
            for (int i = 0; i < sfxPlayers.Length; i++)
                if (sfxPlayers[i].isPlaying && sfxPlayers[i].clip == clip) playing++;
            if (playing >= maxVoicesPerClip) return;
        }

        for (int i = 0; i < channels; i++)
        {
            // 다음 채널로 이동
            channelIndex++;

            // 마지막이면 처음으로
            if (channelIndex >= channels)
                channelIndex = 0;

            // 재생 중이면 다음 채널 확인
            if (sfxPlayers[channelIndex].isPlaying)
                continue;

            // 재생할 효과음 지정
            sfxPlayers[channelIndex].clip = clip;

            // 재생
            sfxPlayers[channelIndex].Play();

            if (_lastPlayTime != null && index < _lastPlayTime.Length)
                _lastPlayTime[index] = Time.unscaledTime;

            break;
        }
    }

    /// <summary>이름 있는 효과음 재생(카드 플립/호버/선택 등). enum 순서 = sfxClips 배열 순서.</summary>
    public void PlaySfx(Sfx sfx) => PlaySfx((int)sfx);

    public void SetBgmVolume(float volume)
    {
        bgmVolume = volume;
        if (bgmPlayer != null) bgmPlayer.volume = bgmVolume / 100f;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
        if (sfxPlayers == null) return;
        foreach (AudioSource player in sfxPlayers)
        {
            player.volume = sfxVolume / 100f;
        }
    }

    public void PlayBgm(Bgm bgm)
    {
        int index = (int)bgm;
        if (bgmClips == null || index < 0 || index >= bgmClips.Length) return;

        if (bgmPlayer.clip == bgmClips[index])
            return;

        bgmPlayer.Stop();
        bgmPlayer.clip = bgmClips[index];
        bgmPlayer.Play();
    }

    // 스테이지마다 다른 브금처럼, 고정 배열 인덱스가 아니라 클립을 직접 넘겨 재생할 때 사용.
    public void PlayBgm(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmPlayer.clip == clip && bgmPlayer.isPlaying) return;

        bgmPlayer.Stop();
        bgmPlayer.clip = clip;
        bgmPlayer.Play();
    }
}
