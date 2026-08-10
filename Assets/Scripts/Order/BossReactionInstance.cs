using TMPro;
using UnityEngine;

// BossAngry 판정일 때 CustomerManager가 Instantiate로 생성하고,
// 시간 지나면 Destroy하는 방식. 씬에 미리 배치해두고 SetActive로 껐다 켰다 하지 않음.
public class BossReactionInstance : MonoBehaviour
{
    public TMP_Text nagText;

    public void SetLine(string line)
    {
        if (nagText != null)
            nagText.text = line;
    }
}