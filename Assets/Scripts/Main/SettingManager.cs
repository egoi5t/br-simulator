using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;


public class SettingsManager : MonoBehaviour
{
    [Header("Audio Mixer (Expose한 파라미터 이름과 정확히 일치해야 함)")]
    public AudioMixer mainMixer;
    private const string SFX_PARAM = "SFXVolume";
    private const string BGM_PARAM = "BGMVolume";

    [Header("슬라이더 연결")]
    public Slider sfxSlider;
    public Slider bgmSlider;
    public Slider brightnessSlider;

    [Header("밝기 조절용 화면 전체 오버레이 (Canvas 맨 아래, 색은 코드가 자동으로 바꿈)")]
    public Image brightnessOverlay;
    [Range(0f, 1f)] public float maxDarkAlpha = 0.85f;  // 왼쪽 끝(가장 어둡게)일 때 검은 오버레이 최대 알파
    [Range(0f, 1f)] public float maxBrightAlpha = 0.4f; // 오른쪽 끝(가장 밝게)일 때 흰 오버레이 최대 알파

    [Header("기본값 (0~1)")]
    [Range(0f, 1f)] public float defaultSfx = 0.5f;
    [Range(0f, 1f)] public float defaultBgm = 0.5f;
    [Range(0f, 1f)] public float defaultBrightness = 0.5f; // 1 = 가장 밝음(오버레이 투명)

    private const string PREF_SFX = "setting_sfx";
    private const string PREF_BGM = "setting_bgm";
    private const string PREF_BRIGHTNESS = "setting_brightness";

    void Start()
    {
        // 저장된 값 불러오기 (없으면 기본값)
        float sfx = PlayerPrefs.GetFloat(PREF_SFX, defaultSfx);
        float bgm = PlayerPrefs.GetFloat(PREF_BGM, defaultBgm);
        float brightness = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, defaultBrightness);

        // 슬라이더 초기값 세팅 (이벤트 중복 호출 방지 위해 SetValueWithoutNotify 사용)
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfx);
        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(bgm);
        if (brightnessSlider != null) brightnessSlider.SetValueWithoutNotify(brightness);

        ApplySfxVolume(sfx);
        ApplyBgmVolume(bgm);
        ApplyBrightness(brightness);

        // 슬라이더 값이 바뀔 때마다 호출될 리스너 등록
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        if (brightnessSlider != null) brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
    }

    // ── 슬라이더 콜백 (인스펙터에서 따로 연결할 필요 없음, 코드로 자동 등록됨) ──
    private void OnSfxChanged(float value)
    {
        ApplySfxVolume(value);
        PlayerPrefs.SetFloat(PREF_SFX, value);
    }

    private void OnBgmChanged(float value)
    {
        ApplyBgmVolume(value);
        PlayerPrefs.SetFloat(PREF_BGM, value);
    }

    private void OnBrightnessChanged(float value)
    {
        ApplyBrightness(value);
        PlayerPrefs.SetFloat(PREF_BRIGHTNESS, value);
    }

    // ── 실제 적용 로직 ──
    // 슬라이더 0~1을 "0.5 = 원래(정상)" 기준으로 재해석해서 배율로 변환
    // 0 → 0배(무음), 0.5 → 1배(정상), 1 → 2배(증폭)
    private void ApplySfxVolume(float linear01)
    {
        if (mainMixer == null) return;
        mainMixer.SetFloat(SFX_PARAM, MultiplierToDecibel(linear01 * 2f));
    }

    private void ApplyBgmVolume(float linear01)
    {
        if (mainMixer == null) return;
        mainMixer.SetFloat(BGM_PARAM, MultiplierToDecibel(linear01 * 2f));
    }

    private void ApplyBrightness(float linear01)
    {
        if (brightnessOverlay == null) return;

        float diff = linear01 - 0.5f; // -0.5(가장 어둡게) ~ 0(정상) ~ +0.5(가장 밝게)
        Color c;
        float alpha;

        if (diff < 0f)
        {
            // 중앙보다 왼쪽: 검은 오버레이로 어둡게
            alpha = Mathf.Lerp(0f, maxDarkAlpha, -diff / 0.5f);
            c = Color.black;
        }
        else
        {
            // 중앙보다 오른쪽: 흰 오버레이로 밝게(워시아웃 효과)
            alpha = Mathf.Lerp(0f, maxBrightAlpha, diff / 0.5f);
            c = Color.white;
        }

        c.a = alpha;
        brightnessOverlay.color = c;
    }

    // 배율(0~2)을 오디오 믹서용 데시벨로 변환. 1배(정상)일 때 0dB.
    private float MultiplierToDecibel(float multiplier)
    {
        float v = Mathf.Clamp(multiplier, 0.0001f, 2f); // 0이면 log10(0) 에러 나서 최소값 보정
        return Mathf.Log10(v) * 20f;
    }
}
