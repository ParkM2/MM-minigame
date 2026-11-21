using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 주제 돌림판. 모든 칸이 같은 크기이고, 결과는 먼저 균등 확률로 뽑은 뒤 그 칸에 멈추도록 회전한다.
public class TopicWheel : MonoBehaviour
{
    static readonly Color[] SliceColors =
    {
        new Color(0.98f, 0.65f, 0.13f), // 주황
        new Color(0.56f, 0.30f, 0.78f), // 보라
        new Color(1.00f, 0.76f, 0.25f), // 밝은 주황
        new Color(0.92f, 0.22f, 0.58f), // 분홍
        new Color(0.30f, 0.70f, 0.90f),
        new Color(0.45f, 0.80f, 0.45f),
    };

    const float SpinDuration = 3.5f; // 회전 시간(초)
    const int ExtraTurns = 5;        // 멈추기 전 추가 회전 수

    RectTransform disc;
    readonly List<RectTransform> labels = new List<RectTransform>();
    List<string> topics;
    public bool IsSpinning { get; private set; }

    // 캔버스 위에 돌림판 UI를 만든다
    public static TopicWheel Create(Canvas canvas, Font font, List<string> topics, Vector2 position, float size)
    {
        var root = new GameObject("TopicWheel", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size + 40);
        rt.anchoredPosition = position;

        var wheel = root.AddComponent<TopicWheel>();
        wheel.topics = topics;
        wheel.Build(font, size);
        return wheel;
    }

    void Build(Font font, float size)
    {
        int n = topics.Count;

        // 회전하는 원판
        var discGo = new GameObject("Disc", typeof(RectTransform));
        discGo.transform.SetParent(transform, false);
        disc = discGo.GetComponent<RectTransform>();
        disc.sizeDelta = new Vector2(size, size);
        disc.anchoredPosition = new Vector2(0, -20);
        var img = discGo.AddComponent<Image>();
        img.sprite = MakeWheelSprite(n, 512);
        img.raycastTarget = false;

        // 칸 이름 (원판과 함께 회전)
        float sliceAngle = 360f / n;
        for (int i = 0; i < n; i++)
        {
            float mid = 90f - (i + 0.5f) * sliceAngle; // 0번 칸이 12시에서 시계 방향으로 시작
            float rad = mid * Mathf.Deg2Rad;
            var label = new GameObject("Label" + i, typeof(RectTransform));
            label.transform.SetParent(disc, false);
            var lrt = label.GetComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(size * 0.3f, size * 0.2f);
            lrt.anchoredPosition = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * size * 0.27f;
            labels.Add(lrt); // 글자는 원판이 돌아도 항상 똑바로 보이게 유지 (KeepLabelsUpright)
            var t = label.AddComponent<Text>();
            t.font = font;
            t.text = topics[i];
            t.fontSize = Mathf.RoundToInt(size * 0.07f);
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 12;
            t.resizeTextMaxSize = t.fontSize;
            t.raycastTarget = false;
            var o = label.AddComponent<Outline>();
            o.effectColor = new Color(0, 0, 0, 0.6f);
        }

        // 가운데 원
        var hub = new GameObject("Hub", typeof(RectTransform));
        hub.transform.SetParent(transform, false);
        var hrt = hub.GetComponent<RectTransform>();
        hrt.sizeDelta = new Vector2(size * 0.15f, size * 0.15f);
        hrt.anchoredPosition = disc.anchoredPosition;
        var himg = hub.AddComponent<Image>();
        himg.sprite = MakeHubSprite(128);
        himg.color = Color.white;
        himg.raycastTarget = false;

        // 12시 방향 핀 모양 바늘 (고정)
        var pointer = new GameObject("Pointer", typeof(RectTransform));
        pointer.transform.SetParent(transform, false);
        var prt = pointer.GetComponent<RectTransform>();
        prt.pivot = new Vector2(0.5f, 0f);                       // 뾰족한 끝 기준
        prt.sizeDelta = new Vector2(size * 0.13f, size * 0.13f * 128f / 96f);
        prt.anchoredPosition = new Vector2(0, -20 + size * 0.5f - size * 0.07f);
        var pimg = pointer.AddComponent<Image>();
        pimg.sprite = MakePinSprite(96, 128);
        pimg.color = Color.white;
        pimg.raycastTarget = false;
    }

    // 결과 칸을 균등 확률로 뽑고 그 칸에서 멈추도록 돌린다
    public void Spin(Action<int> onDone)
    {
        if (IsSpinning) return;
        int result = UnityEngine.Random.Range(0, topics.Count); // 모든 주제 동일 확률
        StartCoroutine(SpinRoutine(result, onDone));
    }

    IEnumerator SpinRoutine(int result, Action<int> onDone)
    {
        IsSpinning = true;
        float slice = 360f / topics.Count;
        // 원판을 z축으로 +angle 만큼 돌리면(반시계) 12시 바늘 아래 칸이 바뀐다.
        // i번 칸 중심이 12시에 오려면 원판 회전 = (i + 0.5) * slice (+ 칸 안에서 약간의 무작위 위치)
        float jitter = UnityEngine.Random.Range(-0.35f, 0.35f) * slice;
        float target = 360f * ExtraTurns + (result + 0.5f) * slice + jitter;
        float start = disc.localEulerAngles.z % 360f;

        for (float t = 0f; t < SpinDuration; t += Time.deltaTime)
        {
            float k = t / SpinDuration;
            float eased = 1f - Mathf.Pow(1f - k, 3f); // 점점 느려지게
            disc.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(start, target, eased));
            KeepLabelsUpright();
            yield return null;
        }
        disc.localRotation = Quaternion.Euler(0, 0, target);
        KeepLabelsUpright();
        IsSpinning = false;
        onDone?.Invoke(result);
    }

    void KeepLabelsUpright()
    {
        foreach (var l in labels) l.rotation = Quaternion.identity;
    }

    // ---------- 절차적 스프라이트 ----------
    // 크림색 테두리 + 동심원 결이 있는 같은 넓이의 칸
    static Sprite MakeWheelSprite(int n, int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color32[size * size];
        float c = size / 2f, r = c - 2f, rimW = size * 0.06f, inner = r - rimW;
        float slice = 360f / n;
        Color rimCol = new Color(0.95f, 0.90f, 0.80f);
        Color edgeCol = new Color(0.55f, 0.40f, 0.25f);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - c + 0.5f, dy = y - c + 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                Color col;
                if (d > r) col = new Color(0, 0, 0, 0);
                else if (d > inner)
                {
                    float t = (d - inner) / rimW;
                    col = rimCol * (1f - 0.25f * Mathf.Abs(t - 0.5f) * 2f); col.a = 1f;
                }
                else if (d > inner - 3f) col = edgeCol;
                else
                {
                    // 12시에서 시계 방향 각도 → 칸 번호 (모든 칸 같은 각도)
                    float ang = (90f - Mathf.Atan2(dy, dx) * Mathf.Rad2Deg + 360f) % 360f;
                    int idx = Mathf.Min(n - 1, (int)(ang / slice));
                    float ring = 1f + 0.07f * Mathf.Sin(d / inner * Mathf.PI * 2f * 5f);
                    float shade = 1f - 0.12f * (d / inner) * (d / inner);
                    col = SliceColors[idx % SliceColors.Length] * (ring * shade); col.a = 1f;
                }
                px[y * size + x] = col;
            }
        tex.SetPixels32(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // 갈색 가운데 축
    static Sprite MakeHubSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color32[size * size];
        float c = size / 2f, r = c - 1f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - c + 0.5f, dy = y - c + 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                Color col = d > r ? new Color(0, 0, 0, 0)
                    : d > r * 0.7f ? new Color(0.45f, 0.28f, 0.15f)
                    : d > r * 0.4f ? new Color(0.75f, 0.60f, 0.42f)
                    : new Color(0.30f, 0.18f, 0.10f);
                px[y * size + x] = col;
            }
        tex.SetPixels32(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // 아래를 가리키는 빨간 핀
    static Sprite MakePinSprite(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px = new Color32[w * h];
        float cx = w / 2f, cy = h - w / 2f, r = w / 2f - 4f;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = x - cx + 0.5f, dy = y - cy + 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float half = Mathf.Clamp01((y - 4f) / (cy - 4f)) * r * 0.85f;
                bool inside = d <= r || (y < cy && Mathf.Abs(dx) <= half);
                Color col = !inside ? new Color(0, 0, 0, 0)
                    : d <= r * 0.28f ? new Color(0.95f, 0.85f, 0.70f)
                    : d <= r * 0.42f ? new Color(0.55f, 0.05f, 0.05f)
                    : new Color(0.85f, 0.12f, 0.12f);
                px[y * w + x] = col;
            }
        tex.SetPixels32(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f));
    }
}
