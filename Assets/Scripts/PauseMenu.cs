using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// ESC 일시정지 팝업 (계속하기 / 초기 화면으로). 게임 씬에서만 동작하며 씬 수정 없이 자동 생성된다.
public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    const string StartSceneName = "StartScene";
    static PauseMenu instance;
    GameObject root;

    static readonly Color DimColor = new Color(0f, 0f, 0f, 0.6f);
    static readonly Color PanelColor = new Color(0.12f, 0.08f, 0.24f, 0.96f);
    static readonly Color ButtonColor = new Color(0.44f, 0.19f, 0.63f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (instance != null) return;
        var go = new GameObject("[PauseMenu]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<PauseMenu>();
        SceneManager.sceneLoaded += (s, m) => instance.ForceResume();
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
        if (SceneManager.GetActiveScene().name == StartSceneName) return; // 시작 화면에서는 사용 안 함
        SetPaused(!IsPaused);
    }

    void ForceResume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (root != null) root.SetActive(false);
    }

    void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;     // 타이머·연출 정지
        ClickGuard.BlockCurrentPress();        // 팝업 버튼 클릭이 게임 화면 클릭으로 이어지지 않게
        if (paused)
        {
            if (root == null) Build();
            root.SetActive(true);
        }
        else if (root != null)
        {
            root.SetActive(false);
        }
    }

    void OnContinue() { SetPaused(false); }

    void OnGoHome()
    {
        SetPaused(false);
        SceneManager.LoadScene(StartSceneName);
    }

    // ---------- UI 생성 ----------
    void Build()
    {
        Font font = FindGameFont();

        root = new GameObject("PauseCanvas");
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        // 화면 전체 어둡게 + 뒤쪽 클릭 차단
        var dim = NewRect("Dim", root.transform);
        dim.anchorMin = Vector2.zero; dim.anchorMax = Vector2.one;
        dim.offsetMin = Vector2.zero; dim.offsetMax = Vector2.zero;
        dim.gameObject.AddComponent<Image>().color = DimColor;

        var panel = NewRect("Panel", root.transform);
        panel.sizeDelta = new Vector2(460, 300);
        panel.gameObject.AddComponent<Image>().color = PanelColor;
        var outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.35f);
        outline.effectDistance = new Vector2(2, -2);

        var title = NewText("Title", panel, font, "일시정지", 36);
        title.rectTransform.anchoredPosition = new Vector2(0, 95);
        title.rectTransform.sizeDelta = new Vector2(400, 60);

        NewButton("ContinueButton", panel, font, "계속하기", new Vector2(0, 5), OnContinue);
        NewButton("HomeButton", panel, font, "초기 화면으로", new Vector2(0, -85), OnGoHome);

        root.SetActive(false);
    }

    static Font FindGameFont()
    {
        var anyText = Object.FindAnyObjectByType<Text>();
        if (anyText != null && anyText.font != null) return anyText.font;
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static Text NewText(string name, Transform parent, Font font, string text, int size)
    {
        var rt = NewRect(name, parent);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.raycastTarget = false;
        return t;
    }

    static void NewButton(string name, Transform parent, Font font, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var rt = NewRect(name, parent);
        rt.sizeDelta = new Vector2(300, 66);
        rt.anchoredPosition = pos;
        var img = rt.gameObject.AddComponent<Image>();
        img.color = ButtonColor;
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        var t = NewText("Label", rt, font, label, 28);
        t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one;
        t.rectTransform.offsetMin = Vector2.zero; t.rectTransform.offsetMax = Vector2.zero;
    }
}
