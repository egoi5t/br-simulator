using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

[System.Serializable]
public class CustomerOrder
{
    public string customerID;
    public string customerName;
    public int day;
    public int scoopCount;
    public string container;
    public List<string> flavorID = new List<string>();
    public string orderLine;
    public string satisfiedLine;
    public string unhappyLine;
}

// ── flavors.csv 한 줄 (맛 사전) ──
[System.Serializable]
public class FlavorData
{
    public string flavorId;    // flavor_id
    public string flavorName;  // flavor_name
    public string category;    // category
    public string colorHex;    // color_hex
    public int unlockLevel; // unlock_level
}

public class CsvOrderParser : MonoBehaviour
{
    public TMP_Text outputText;
    private void Awake()
    {
        // 1) 맛 사전 먼저 읽어서 딕셔너리로
        TextAsset flavorCsv = Resources.Load<TextAsset>("flavors");
        Dictionary<string, FlavorData> flavorTable = ParseFlavors(flavorCsv.text);

        // 2) 주문 읽기
        TextAsset orderCsv = Resources.Load<TextAsset>("customers");
        List<CustomerOrder> orders = ParseCsv(orderCsv.text);

        // 3) 둘을 연결해서 출력
        PrintToUI(orders, flavorTable);
    }

    // ── orders 파싱 ──
    public static List<CustomerOrder> ParseCsv(string csvText)
    {
        var result = new List<CustomerOrder>();
        string[] lines = csvText.Replace("\r", "").Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] cols = SplitCsvLine(lines[i]);

            var order = new CustomerOrder
            {
                customerID = Get(cols, 0),
                customerName = Get(cols, 1),
                day = ParseInt(Get(cols, 2)),
                scoopCount = ParseInt(Get(cols, 3)),
                container = Get(cols, 4),
                flavorID = SplitFlavors(Get(cols, 5)),
                orderLine = Get(cols, 6),
                satisfiedLine = Get(cols, 7),
                unhappyLine = Get(cols, 8),
            };
            result.Add(order);
        }
        return result;
    }

    // ── flavors 파싱 : flavor_id 를 key 로 하는 딕셔너리 ──
    public static Dictionary<string, FlavorData> ParseFlavors(string csvText)
    {
        var table = new Dictionary<string, FlavorData>();
        string[] lines = csvText.Replace("\r", "").Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] cols = SplitCsvLine(lines[i]);

            var f = new FlavorData
            {
                flavorId = Get(cols, 0),
                flavorName = Get(cols, 1),
                category = Get(cols, 2),
                colorHex = Get(cols, 3),
                unlockLevel = ParseInt(Get(cols, 4)),
            };
            table[f.flavorId] = f; // 코드로 바로 찾을 수 있게 저장
        }
        return table;
    }

    // ── 따옴표 존중 분리기 (두 파일 공용) ──
    private static string[] SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { fields.Add(sb.ToString()); sb.Clear(); }
                else sb.Append(c);
            }
        }
        fields.Add(sb.ToString());
        return fields.ToArray();
    }

    private static List<string> SplitFlavors(string raw)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(raw)) return list;

        foreach (string s in raw.Split('|'))
        {
            string t = s.Trim();
            if (t.Length > 0) list.Add(t);
        }
        return list;
    }

    private static int ParseInt(string s)
    {
        return int.TryParse(s, out int v) ? v : 0;
    }

    private static string Get(string[] cols, int idx)
    {
        return idx < cols.Length ? cols[idx].Trim() : "";
    }

    // ── 출력 : 주문의 flavor_id 를 실제 맛 정보로 바꿔서 표시 ──
    private void PrintToUI(
    List<CustomerOrder> orders,
    Dictionary<string, FlavorData> flavorTable)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"총 {orders.Count}건 파싱 완료\n");

        foreach (var o in orders)
        {
            var names = new List<string>();
            foreach (string id in o.flavorID)
            {
                if (flavorTable.TryGetValue(id, out FlavorData f))
                    names.Add($"{f.flavorName}({f.category})");
                else
                    names.Add($"{id}(???)");
            }
            string flavors = string.Join(", ", names);

            sb.AppendLine($"[{o.customerID}] {o.customerName} / Day {o.day} / {o.scoopCount}스쿱 / {o.container}");
            sb.AppendLine($"  맛: {flavors}");
            sb.AppendLine($"  주문: {o.orderLine}");
            sb.AppendLine($"  만족: {o.satisfiedLine}");
            sb.AppendLine($"  불만: {o.unhappyLine}");
            sb.AppendLine();
        }

        outputText.text = sb.ToString();  // ← 완성된 문자열을 UI에 한 번에
    }

}
