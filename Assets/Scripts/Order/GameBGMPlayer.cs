using UnityEngine;
using UnityEngine.Audio;

// 메인 씬을 제외한 나머지 씬들(주문/컵선택/제작 등)에서 공용으로 쓰는 배경음악.
//
// 사용법: OrderScene에 빈 오브젝트 하나 만들어서 이 스크립트만 붙여두면 됨.
// 컵선택/제작 씬에는 따로 안 놔도 됨 - DontDestroyOnLoad로 자동으로 유지됨.
//
// 씬을 나갔다가(컵선택→제작→...) 다시 OrderScene으로 돌아오면, OrderScene에 있던
// 이 오브젝트가 또 생성되긴 하지만, 이미 재생 중인 인스턴스가 있으면 그 중복 오브젝트는
// 스스로 파괴되고 원래 있던 브금은 처음부터 다시 재생되지 않고 그대로 이어짐.
public class GameBGMPlayer : MonoBehaviour
{
    private static GameBGMPlayer _instance;
    public static GameBGMPlayer Instance => _instance;
    private AudioSource audioSource;

    [Header("Audio Mixer 연결")]
    public AudioMixerGroup bgmGroup; // MainMixer의 BGM 그룹 드래그

    [Header("브금 클립")]
    public AudioClip bgmClip;

    void Awake()
    {
        // 이미 재생 중인 인스턴스가 있으면(씬을 나갔다 돌아온 경우) 이 중복 오브젝트는 그냥 파괴
        if (_instance != null)
        {
            // 2026-08-10: if BGM was stopped at an ending, resume it when re-entering OrderScene (new game)
            if (_instance.audioSource != null && !_instance.audioSource.isPlaying)
                _instance.audioSource.Play();
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = bgmClip;
        audioSource.outputAudioMixerGroup = bgmGroup;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.Play();
    }

    // 2026-08-10: stop gameplay BGM when daily settlement completes (entering ending)
    public void StopBgm()
    {
        if (audioSource != null) audioSource.Stop();
    }

    // 2026-08-10: resume gameplay BGM (used by 'next day' button after settlement)
    public void PlayBgm()
    {
        if (audioSource != null && !audioSource.isPlaying) audioSource.Play();
    }
}