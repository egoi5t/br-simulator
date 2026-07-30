using System.Collections.Generic;
using System.Text;
using UnityEngine;

// ── customers.csv 한 줄 ──
// 컬럼 순서: customer_id, scoop_count, flavor_ids, order_line, paper, satisfied_line, unsatisfied_line
[System.Serializable]
public class CustomerOrder
{
    public string customerId;
    public int scoopCount;
    public List<string> flavorIds = new List<string>();
    public string orderLine;
    public string paper;            // 주문서(영수증)에 적힐 요약 텍스트. 예: "니코, 민초 쿠앤크"
    public string satisfiedLine;
    public string unsatisfiedLine;
}

// ── flavors.csv 한 줄 (맛 사전) ──
// 컬럼 순서: flavor_id, flavor_name
[System.Serializable]
public class FlavorData
{
    public string flavorId;
    public string flavorName;
}

// 순수 파싱 도구 모음 (씬에 붙일 필요 없음, static 으로 호출)
public static class CsvOrderParser
{
    // ── customer 파싱 ──
    public static List<CustomerOrder> ParseCsv(string csvText)
    {
        var result = new List<CustomerOrder>();
        string[] lines = csvText.Replace("\r", "").Split('\n');

        for (int i = 1; i < lines.Length; i++) // 0번째는 헤더라 건너뜀
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] cols = SplitCsvLine(lines[i]);

            var order = new CustomerOrder
            {
                customerId = Get(cols, 0),
                scoopCount = ParseInt(Get(cols, 1)),
                flavorIds = SplitFlavors(Get(cols, 2)),
                orderLine = Get(cols, 3),
                paper = Get(cols, 4),
                satisfiedLine = Get(cols, 5),
                unsatisfiedLine = Get(cols, 6),
            };
            result.Add(order);
        }
        return result;
    }

    // ── flavor 파싱 : flavor_id 를 key 로 하는 딕셔너리 ──
    public static Dictionary<string, FlavorData> ParseFlavors(string csvText)
    {
        var table = new Dictionary<string, FlavorData>();
        string[] lines = csvText.Replace("\r", "").Split('\n');

        for (int i = 1; i < lines.Length; i++) // 0번째는 헤더라 건너뜀
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] cols = SplitCsvLine(lines[i]);

            var f = new FlavorData
            {
                flavorId = Get(cols, 0),
                flavorName = Get(cols, 1),
            };
            table[f.flavorId] = f;
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

    // flavor_ids : "FLV-001|FLV-010" → ["FLV-001", "FLV-010"]
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
}