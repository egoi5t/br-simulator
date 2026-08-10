using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 화면③ 전용 효과음 재생 매니저.
/// 컵/뚜껑/쇼핑백 선택, 에러(경고) 시 소리를 재생함.
///
/// ★ 중요: 소리가 타이틀 설정의 SFX 볼륨 슬라이더에 묶이려면
///    반드시 인스펙터에서 Sfx Group 에 MainMixer 의 "SFX" 그룹을 연결해야 함.
///
/// 인스펙터 설정:
/// - Sfx Group : MainMixer 의 SFX AudioMixerGroup 드래그 (설정 슬라이더 연동에 필수)
/// - Error Sfx / Cup Select Sfx / Lid Select Sfx / Bag Select Sfx: 각 효과음 클립
///
/// 씬마다 하나씩 배치해서 쓰면 됨 (OrderSession처럼 씬 넘어가도 유지될 필요는 없음).
/// </summary>
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance;

    [Header("Audio Mixer (필수)")]
    [Tooltip("MainMixer 의 SFX 그룹을 연결. 이게 있어야 설정의 SFX 슬라이더가 이 소리들을 조절함")]
    public AudioMixerGroup sfxGroup;

    [Header("오디오 소스 (선택)")]
    [Tooltip("클립별 상대 볼륨이 필요할 때만 사용. 전체 볼륨은 Sfx Group(믹서)이 관리함")]
    public AudioSource audioSource;

    [Header("효과음 클립")]
    public AudioClip errorSfx;
    public AudioClip cupSelectSfx;
    public AudioClip lidSelectSfx;
    public AudioClip bagSelectSfx;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayError() => Play(errorSfx);
    public void PlayCupSelect() => Play(cupSelectSfx);
    public void PlayLidSelect() => Play(lidSelectSfx);
    public void PlayBagSelect() => Play(bagSelectSfx);

    private void Play(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("SfxManager: 재생하려는 클립이 비어있습니다.");
            return;
        }

        // 공유 AudioSource 하나로 PlayOneShot을 연타하면 가끔 씹히는 경우가 있어서,
        // 매번 임시 재생기를 만들어서 트는 방식으로 처리 (여러 소리가 겹쳐도 확실하게 다 재생됨).
        // 클립 길이만큼만 살아있다가 자동으로 정리됨.
        GameObject temp = new GameObject("SfxOneShot");
        temp.transform.SetParent(transform, worldPositionStays: false);

        AudioSource tempSource = temp.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.outputAudioMixerGroup = sfxGroup; // ★ SFX 그룹으로 라우팅 (슬라이더 연동 핵심)
        tempSource.volume = audioSource != null ? audioSource.volume : 1f;
        tempSource.spatialBlend = 0f; // 2D 사운드
        tempSource.Play();

        Destroy(temp, clip.length);
    }
}