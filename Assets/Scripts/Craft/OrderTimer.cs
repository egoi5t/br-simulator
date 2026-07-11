using UnityEngine;

public class OrderTimer : MonoBehaviour
{
    private float elapsedTime = 0f;
    private bool isTiming = false;

    public void StartTimer()
    {
        elapsedTime = 0f;
        isTiming = true;
        Debug.Log("[타이머] 시작됨");
    }

    public void StopTimer()
    {
        isTiming = false;
        Debug.Log("[타이머] 중지됨");
    }

    private void Update()
    {
        if (isTiming)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}