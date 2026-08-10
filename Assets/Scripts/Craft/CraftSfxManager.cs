using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 화면②(제작/담기) 전용 효과음 재생 매니저.
/// 도구 선택 / 맛 담기 / 완성 / 에러(경고) 시 소리를 재생함.
///
/// ★ 중요: 소리가 타이틀 설정의 SFX 볼륨 슬라이더에 묶이려면
///    반드시 인스펙터에서 Sfx Group 에 MainMixer 의 "SFX" 그룹을 연결해야 함.
///    (연결 안 하면 Master 로 빠져서 슬라이더가 안 먹음)
///
/// 인스펙터 설정:
/// - Sfx Group        : MainMixer 의 SFX AudioMixerGroup 드래그
/// - Tool Select Sfx  : 도구(스쿱/스패출러) 선택 소리
/// - Add Flavor Sfx   : 맛 한 스쿱 담을 때 소리
/// - Complete Sfx     : 담기 완료 소리
/// - Error Sfx        : 잘못된 조작(경고) 소리
///
/// 씬마다 하나씩 배치해서 쓰면 됨 (씬 넘어가도 유지될 필요는 없음).
/// </summary>
public class CraftSfxManager : MonoBehaviour
{
    public static CraftSfxManager Instance;

    [Header("Audio Mixer (필수)")]
    [Tooltip("MainMixer 의 SFX 그룹을 연결. 이게 있어야 설정의 SFX 슬라이더가 이 소리들을 조절함")]
    public AudioMixerGroup sfxGroup;

    [Header("효과음 클립")]
    public AudioClip toolSelectSfx;
    public AudioClip addFlavorSfx;
    public AudioClip completeSfx;
    public AudioClip errorSfx;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayToolSelect() => Play(toolSelectSfx);
    public void PlayAddFlavor()  => Play(addFlavorSfx);
    public void PlayComplete()   => Play(completeSfx);
    public void PlayError()      => Play(errorSfx);

    private void Play(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("CraftSfxManager: 재생하려는 클립이 비어있습니다.");
            return;
        }

        // 여러 소리가 겹쳐도 다 들리도록 매번 임시 재생기를 만들어서 트고, 끝나면 자동 정리.
        GameObject temp = new GameObject("CraftSfxOneShot");
        temp.transform.SetParent(transform, worldPositionStays: false);

        AudioSource tempSource = temp.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.outputAudioMixerGroup = sfxGroup; // ★ SFX 그룹으로 라우팅 (슬라이더 연동 핵심)
        tempSource.spatialBlend = 0f;                // 2D 사운드
        tempSource.Play();

        Destroy(temp, clip.length);
    }
}
