using UnityEngine;

public class Customer : MonoBehaviour
{
    private CustomerData customerData;

    [SerializeField]
    private SpriteRenderer customerRenderer;

    public CustomerData CustomerData => customerData;

    public void Initialize(CustomerData data)
    {
        customerData = data;

        ApplyCustomerVisual();
    }

    private void ApplyCustomerVisual()
    {
        //엑셀파싱 후 id 대조하여 아트 이미지 로드할 예정
    }

    public string GetOrderLine()
    {
        return customerData.OrderLine;
    }
}