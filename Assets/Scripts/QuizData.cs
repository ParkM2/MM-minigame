using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// 퀴즈 한 문제
public class QuizQuestion
{
    public string topic;
    public string question;
    public string[] options = new string[4];
    public int answer;          // 1~4
    public string explanation;  // 해설 (선택)
}

// StreamingAssets/quiz_data.csv 를 읽어 주제별 문제 목록을 만든다.
// 빌드 후에도 [게임폴더]_Data/StreamingAssets/quiz_data.csv 만 고치면 반영된다.
//
// CSV 열: 주제, 문제, 보기1, 보기2, 보기3, 보기4, 정답(1~4), 해설(선택)
// - 첫 줄은 머리글. 빈 줄과 # 로 시작하는 줄은 무시.
// - 쉼표가 들어간 칸은 큰따옴표로 감싼다.  예) "몽환의 벨트, 반지"
// - 문제 안에서 줄을 바꾸려면 \n 을 적는다.  예) 루시드 보상으로 얻을 수 있는\n아이템이 아닌것은?
public static class QuizData
{
    public const string FileName = "quiz_data.csv";

    public static List<string> Topics = new List<string>();                  // CSV에 처음 나온 순서
    public static Dictionary<string, List<QuizQuestion>> ByTopic = new Dictionary<string, List<QuizQuestion>>();
    public static List<string> Errors = new List<string>();

    public static bool Load()
    {
        Topics.Clear(); ByTopic.Clear(); Errors.Clear();

        string path = Path.Combine(Application.streamingAssetsPath, FileName);
        if (!File.Exists(path))
        {
            Errors.Add("CSV 파일을 찾을 수 없습니다: " + path);
            return false;
        }

        string text = File.ReadAllText(path, Encoding.UTF8);
        if (text.IndexOf('\uFFFD') >= 0)
        {
            // 엑셀 기본 'CSV(쉼표로 분리)'는 한글이 깨진다 → 'CSV UTF-8'로 저장해야 함
            Errors.Add("CSV 한글이 깨졌어요. 엑셀에서 'CSV UTF-8(쉼표로 분리)'로 저장해주세요");
            return false;
        }
        var rows = ParseCsv(text);

        for (int r = 0; r < rows.Count; r++)
        {
            var c = rows[r];
            int line = r + 1;
            if (c.Count == 0 || (c.Count == 1 && c[0].Trim() == "")) continue;
            string first = c[0].Trim();
            if (first.StartsWith("#")) continue;
            if (r == 0 && (first == "주제" || first.ToLower() == "topic")) continue; // 머리글

            if (c.Count < 7) { Errors.Add($"{line}행: 칸이 7개보다 적습니다"); continue; }
            if (!int.TryParse(c[6].Trim(), out int ans) || ans < 1 || ans > 4)
            {
                Errors.Add($"{line}행: 정답은 1~4 숫자여야 합니다 ('{c[6]}')");
                continue;
            }

            var q = new QuizQuestion
            {
                topic = first,
                question = Unescape(c[1].Trim()),
                answer = ans,
                explanation = c.Count > 7 ? Unescape(c[7].Trim()) : ""
            };
            for (int i = 0; i < 4; i++) q.options[i] = c[2 + i].Trim();

            if (string.IsNullOrEmpty(q.topic) || string.IsNullOrEmpty(q.question))
            {
                Errors.Add($"{line}행: 주제나 문제가 비어 있습니다");
                continue;
            }

            if (!ByTopic.ContainsKey(q.topic))
            {
                ByTopic[q.topic] = new List<QuizQuestion>();
                Topics.Add(q.topic);
            }
            ByTopic[q.topic].Add(q);
        }

        foreach (var e in Errors) Debug.LogWarning("[QuizData] " + e);
        return Topics.Count > 0;
    }

    // 주제에서 count개를 중복 없이 무작위로 뽑는다
    public static List<QuizQuestion> PickQuestions(string topic, int count)
    {
        var pool = new List<QuizQuestion>(ByTopic[topic]);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        if (count < pool.Count) pool.RemoveRange(count, pool.Count - count);
        return pool;
    }

    static string Unescape(string s) => s.Replace("\\n", "\n");

    // 따옴표·쉼표·줄바꿈을 처리하는 간단한 CSV 파서
    static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var cell = new StringBuilder();
        bool inQuotes = false;
        if (text.Length > 0 && text[0] == '﻿') text = text.Substring(1); // BOM 제거

        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; }
                    else inQuotes = false;
                }
                else cell.Append(ch);
            }
            else if (ch == '"') inQuotes = true;
            else if (ch == ',') { row.Add(cell.ToString()); cell.Clear(); }
            else if (ch == '\r') { }
            else if (ch == '\n') { row.Add(cell.ToString()); cell.Clear(); rows.Add(row); row = new List<string>(); }
            else cell.Append(ch);
        }
        if (cell.Length > 0 || row.Count > 0) { row.Add(cell.ToString()); rows.Add(row); }
        return rows;
    }
}
