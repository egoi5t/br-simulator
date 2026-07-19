using System.Collections.Generic;
using System.Text;
using UnityEngine;

// ── customer.csv 한 줄 ──
[System.Serializable]
public class CustomerOrder
{
    public string customerId;
    public string customerName;
    public int day;
    public int scoopCount;
    public string container;
    public List<string> flavorIds = new List<string>();
    public string orderLine;
    public string satisfiedLine;
    public string unhappyLine;
}

// ── flavor.csv 한 줄 (맛 사전) ──
[System.Serializable]
public class FlavorData
{
    public string flavorId;
    public string flavorName;
    public string category;
    public string colorHex;
    public int unlockLevel;
}

// 순수 파싱 도구 모음 (씬에 붙일 필요 없음, static 으로 호출)
public static class CsvOrderParser
{
    // ── customer 파싱 ──
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
                customerId = Get(cols, 0),
                customerName = Get(cols, 1),
                day = ParseInt(Get(cols, 2)),
                scoopCount = ParseInt(Get(cols, 3)),
                container = Get(cols, 4),
                flavorIds = SplitFlavors(Get(cols, 5)),
                orderLine = Get(cols, 6),
                satisfiedLine = Get(cols, 7),
                unhappyLine = Get(cols, 8),
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