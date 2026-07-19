using TMPro;
using UnityEngine;

public class SpeechBubbleUI : MonoBehaviour
{
    [SerializeField]
    private GameObject bubbleRoot;

    [SerializeField]
    private TMP_Text orderText;

    private void Start()
    {
        Hide();
    }

    public void ShowOrder(string order)
    {
        bubbleRoot.SetActive(true);

        orderText.text = order;
    }

    public void Hide()
    {
        bubbleRoot.SetActive(false);
    }
}