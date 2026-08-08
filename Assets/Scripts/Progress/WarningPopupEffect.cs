using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 잘못된 행동(뚜껑 오답, 이른 쇼핑백 클릭, 시간 초과 등) 시
/// 마우스 근처에 경고 표시를 띄우고, 시간이 지나면서 위로 떠오르며 서서히 사라지게 하는 효과.
///
/// FeedbackManager(화면②에서 쓰는 공용 피드백)와는 별개의, 화면③ 전용 독립 시스템.
///
/// 인스펙터 설정:
/// - Canvas Parent: 경고 표시가 생성될 Canvas (또는 그 하위 RectTransform). 보통 최상위 Canvas 연결.
/// - Warning Icon Sprite: (선택) 경고 아이콘 이미지. 비워두면 느낌표("!") 텍스트로 대체.
///
/// 씬마다 하나씩 배치해서 쓰면 됨 (OrderSession처럼 씬 넘어가도 유지될 필요는 없음).
/// </summary>
public class WarningPopupEffect : MonoBehaviour
{
    public static WarningPopupEffect Instance;

    [Header("생성 위치")]
    [Tooltip("경고 표시가 생성될 Canvas(또는 하위 RectTransform). 보통 최상위 Canvas 연결")]
    public Transform canvasParent;

    [Header("비주얼")]
    [Tooltip("비워두면 아래 메시지 텍스트로 표시됨. 채우면 이 이미지가 우선됨")]
    public Sprite warningIconSprite;
    public Color warningColor = new Color(0.95f, 0.2f, 0.2f, 1f);
    [Tooltip("한글이 깨지지 않는 폰트 에셋 연결 (예: MalgunGothic SDF). 비워두면 기본 폰트라 한글이 네모로 보일 수 있음")]
    public TMP_FontAsset koreanFontAsset;
    [Tooltip("메시지 텍스트 폰트 크기")]
    public float fontSize = 32f;
    [Tooltip("텍스트가 표시될 박스 크기 (가로, 세로)")]
    public Vector2 textBoxSize = new Vector2(240f, 70f);
    [Tooltip("Warning Icon Sprite를 쓸 때 아이콘 크기")]
    public float iconSize = 40f;

    [Header("애니메이션")]
    [Tooltip("위로 떠오르는 거리(px)")]
    public float floatDistance = 60f;
    [Tooltip("떠오르면서 사라지기까지 걸리는 시간(초)")]
    public float duration = 1f;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>마우스 현재 위치에 경고 표시(짧은 설명 문구)를 띄움.</summary>
    public void PlayWarningAtMouse(string message = "잘못됐어요!")
    {
        if (canvasParent == null)
        {
            Debug.LogWarning("WarningPopupEffect: Canvas Parent가 연결되지 않았습니다.");
            return;
        }

        StartCoroutine(SpawnAndAnimate(Input.mousePosition, message));
    }

    private IEnumerator SpawnAndAnimate(Vector2 screenPos, string message)
    {
        GameObject popup = new GameObject("WarningPopup", typeof(RectTransform));
        popup.transform.SetParent(canvasParent, worldPositionStays: false);

        var rect = popup.GetComponent<RectTransform>();
        rect.position = screenPos; // 마우스 화면 좌표에 바로 배치 (Screen Space - Overlay 기준)

        Graphic visual;
        if (warningIconSprite != null)
        {
            rect.sizeDelta = new Vector2(iconSize, iconSize);
            var img = popup.AddComponent<Image>();
            img.sprite = warningIconSprite;
            img.color = Color.white;
            visual = img;
        }
        else
        {
            rect.sizeDelta = textBoxSize;
            var text = popup.AddComponent<TextMeshProUGUI>();
            text.text = message;
            if (koreanFontAsset != null)
                text.font = koreanFontAsset;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = true;
            text.color = warningColor;
            visual = text;
        }

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * floatDistance;
        Color startColor = visual.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            visual.color = c;

            yield return null;
        }

        Destroy(popup);
    }
}