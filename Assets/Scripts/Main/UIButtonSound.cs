using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

// 씬에 빈 오브젝트 하나 만들어서 이 스크립트만 붙이면 끝.
// 버튼마다 OnClick()에 따로 연결할 필요 없이, 씬에 있는 모든 Button(비활성화된
// 패널 안의 버튼 포함)을 자동으로 찾아서 클릭할 때 효과음이 나가게 해줌.
public class UIButtonSound : MonoBehaviour
{
    [Header("Audio Mixer 연결")]
    public AudioMixerGroup sfxGroup; // MainMixer의 SFX 그룹 드래그

    [Header("클릭 효과음")]
    public AudioClip clickClip;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.outputAudioMixerGroup = sfxGroup;

        // 비활성화된 패널(SettingPanel, HowToPlayPannel 등) 안의 버튼까지 전부 포함해서 찾음
        Button[] allButtons = FindObjectsOfType<Button>(true);
        foreach (var button in allButtons)
        {
            button.onClick.AddListener(PlayClickSound);
        }

        Debug.Log($"UIButtonSound: 버튼 {allButtons.Length}개에 클릭 효과음을 연결했습니다.");
    }

    private void PlayClickSound()
    {
        if (clickClip == null || audioSource == null) return;
        audioSource.PlayOneShot(clickClip);
    }
}