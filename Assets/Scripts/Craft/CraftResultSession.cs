using System.Collections.Generic;
using UnityEngine;

// 화면2 -> 화면3 으로 아이스크림 완성 결과를 넘기는 세션.
// 씬이 바뀌어도 유지됨 (OrderSession과 같은 패턴).

// [담기 씬] 완성되면:
//   CraftResultSession.Instance.SetResult(containerIndex, flavorIds);

// [포장 씬] 담당자가 읽을 때:
//   int container = CraftResultSession.Instance.ContainerIndex;
//   List<string> flavors = CraftResultSession.Instance.FlavorIds; (담긴 순서 그대로)
public class CraftResultSession : MonoBehaviour
{
    private static CraftResultSession _instance;

    public static CraftResultSession Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("CraftResultSession");
                _instance = go.AddComponent<CraftResultSession>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // 1~6 (잇코~록코)
    public int ContainerIndex { get; private set; }

    // 담긴 순서 그대로. flavorIds[0] = 1번째로 담은 맛
    public List<string> FlavorIds { get; private set; }

    public void SetResult(int containerIndex, List<string> flavorIds)
    {
        ContainerIndex = containerIndex;
        FlavorIds = new List<string>(flavorIds);
    }
}