using UnityEngine;

/// <summary>
/// 화면③ 전용 효과음 재생 매니저.
/// 컵/뚜껑/쇼핑백 선택, 에러(경고) 시 소리를 재생함.
///
/// 인스펙터 설정:
/// - Audio Source: 소리를 재생할 AudioSource 컴포넌트 (같은 오브젝트에 붙여두면 됨)
/// - Error Sfx / Cup Select Sfx / Lid Select Sfx / Bag Select Sfx: 각 효과음 클립
///
/// 씬마다 하나씩 배치해서 쓰면 됨 (OrderSession처럼 씬 넘어가도 유지될 필요는 없음).
/// </summary>
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance;

    [Header("오디오 소스")]
    [Tooltip("직접 재생하는 용도는 아니고, 여기 설정된 Volume 값만 참고해서 임시 재생기에 적용함")]
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
        tempSource.volume = audioSource != null ? audioSource.volume : 1f;
        tempSource.spatialBlend = 0f; // 2D 사운드
        tempSource.Play();

        Destroy(temp, clip.length);
    }
}