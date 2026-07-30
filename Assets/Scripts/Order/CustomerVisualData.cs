using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomerVisualEntry
{
    public string customerId;   // CSV의 customer_id 와 정확히 동일해야 함 (예: CUST-001)
    public Sprite customerSprite;
}

// Project 창에서 우클릭 > Create > IceCreamGame > Customer Visual Data 로 생성
[CreateAssetMenu(fileName = "CustomerVisualData", menuName = "IceCreamGame/Customer Visual Data")]
public class CustomerVisualData : ScriptableObject
{
    public List<CustomerVisualEntry> entries = new List<CustomerVisualEntry>();

    private Dictionary<string, Sprite> _lookup;

    public Sprite GetSprite(string customerId)
    {
        // 처음 조회할 때 한 번만 딕셔너리로 캐싱 (매번 리스트 순회하지 않도록)
        if (_lookup == null)
        {
            _lookup = new Dictionary<string, Sprite>();
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.customerId)) continue;
                _lookup[entry.customerId] = entry.customerSprite;
            }
        }

        if (_lookup.TryGetValue(customerId, out Sprite sprite))
            return sprite;

        Debug.LogWarning($"CustomerVisualData: '{customerId}'에 매핑된 스프라이트가 없습니다.");
        return null;
    }
}