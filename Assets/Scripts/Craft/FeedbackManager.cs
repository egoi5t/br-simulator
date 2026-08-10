using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance;

    [Header("X 이미지 표시용 (재사용, 위치만 옮겨서 사용)")]
    public RectTransform errorIcon;
    public Image errorIconImage;
    public float iconFlashDuration = 0.3f;

    [Header("화면 흔들림 대상")]
    public RectTransform shakeTarget;
    public float shakeDuration = 0.3f;
    public float shakeStrength = 15f;

    private Vector2 originalShakePosition;

    private void Awake()
    {
        Instance = this;
        if (shakeTarget != null)
        {
            originalShakePosition = shakeTarget.anchoredPosition;
        }

        if (errorIconImage != null)
        {
            SetIconAlpha(0f);
        }
    }

    private void SetIconAlpha(float alpha)
    {
        if (errorIconImage == null) return;
        Color c = errorIconImage.color;
        c.a = alpha;
        errorIconImage.color = c;
    }

    private IEnumerator ShakeRoutine()
    {
        if (shakeTarget == null) yield break;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float strength = shakeStrength * (1f - elapsed / shakeDuration);
            Vector2 randomOffset = Random.insideUnitCircle * strength;
            shakeTarget.anchoredPosition = originalShakePosition + randomOffset;
            yield return null;
        }

        shakeTarget.anchoredPosition = originalShakePosition;
    }

    public void PlayErrorFeedbackAtMouse()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine());
        StartCoroutine(IconFlashAtScreenPosition(Input.mousePosition));
        CraftSfxManager.Instance?.PlayError();
    }

    private IEnumerator IconFlashAtScreenPosition(Vector2 screenPos)
    {
        errorIcon.position = screenPos; // 화면 좌표 그대로 (Screen Space - Overlay 기준)
        errorIcon.gameObject.SetActive(true);
        SetIconAlpha(1f);

        yield return new WaitForSeconds(iconFlashDuration);

        SetIconAlpha(0f);
        errorIcon.gameObject.SetActive(false);
    }
}