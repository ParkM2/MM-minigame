using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class QuizSceneBuilder
{
    [MenuItem("Tools/Build Quiz Scene (GameScene2)")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "GameScene2")
        {
            Debug.LogError("GameScene2를 먼저 로드하세요!");
            return;
        }

        // Clear existing objects
        foreach (var r in scene.GetRootGameObjects())
            Object.DestroyImmediate(r);

        // Load assets
        var bgSprite    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/시작화면.png");
        var normalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/루시드카드.png"); // 임시 (노멀)
        var happySprite  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/기쁜루시드.png");
        var sadSprite    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/슬픈루시드.png");
        var mapleFont    = AssetDatabase.LoadAssetAtPath<Font>("Assets/font/Maplestory Light.ttf");

        // Camera
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.05f, 0.2f);

        // EventSystem
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();

        // Canvas
        var cvGO = new GameObject("Canvas");
        var canvas = cvGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = cvGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        cvGO.AddComponent<GraphicRaycaster>();
        var ct = cvGO.transform;

        // Background
        var bgGO = MakeImg(ct, "Background", 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);
        bgGO.GetComponent<Image>().sprite = bgSprite;
        bgGO.GetComponent<Image>().color = Color.white;
        bgGO.GetComponent<Image>().type = Image.Type.Simple;
        bgGO.GetComponent<Image>().preserveAspect = false;

        // Semi-transparent overlay for readability
        var overlay = MakeImg(ct, "Overlay", 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);
        overlay.GetComponent<Image>().color = new Color(0.05f, 0.0f, 0.15f, 0.55f);

        // Lucid character (left-center)
        var lucidGO = MakeImg(ct, "LucidImage", 0.22f, 0.08f, 0.22f, 0.08f, 0.5f, 0f, 340, 420, 0, 0);
        lucidGO.GetComponent<Image>().sprite = normalSprite;
        lucidGO.GetComponent<Image>().color = Color.white;
        lucidGO.GetComponent<Image>().preserveAspect = true;

        // Question bubble (right-center area)
        var bubbleBg = MakeImg(ct, "QuestionBubble", 0.55f, 0.45f, 0.55f, 0.45f, 0.5f, 0.5f, 560, 220, 0, 0);
        bubbleBg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);
        var bubbleOutline = bubbleBg.AddComponent<Outline>();
        bubbleOutline.effectColor = new Color(0.55f, 0.25f, 0.85f);
        bubbleOutline.effectDistance = new Vector2(4, 4);

        // Question text inside bubble
        var qTextGO = MakeTxt(bubbleBg.transform, "QuestionText", "",
            0, 0, 1, 1, 0.5f, 0.5f, -20, -20, 0, 0,
            new Color(0.1f, 0.05f, 0.25f), 28, true, mapleFont);
        qTextGO.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

        // Click to advance label
        var clickHint = MakeTxt(ct, "ClickHint", "▼ 화면을 클릭하세요",
            0.5f, 0.02f, 0.5f, 0.02f, 0.5f, 0f, 360, 36, 0, 0,
            new Color(1f, 1f, 0.6f, 0.85f), 20, false, mapleFont);

        // Option panel (bottom area)
        var optPanel = MakeImg(ct, "OptionPanel", 0.5f, 0f, 0.5f, 0f, 0.5f, 0f, 1100, 155, 0, 10);
        optPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0f);

        // 4 option buttons
        Button[] optBtns = new Button[4];
        Text[] optTxts = new Text[4];
        Color[] btnColors = {
            new Color(0.9f, 0.3f, 0.3f),
            new Color(0.25f, 0.7f, 0.3f),
            new Color(0.3f, 0.5f, 0.9f),
            new Color(0.9f, 0.8f, 0.2f)
        };
        string[] numLabels = { "①", "②", "③", "④" };
        float[] xPositions = { -412f, -137f, 137f, 412f };

        for (int i = 0; i < 4; i++)
        {
            var btnGO = MakeImg(optPanel.transform, "Option" + (i + 1),
                0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 255, 120, xPositions[i], 0);
            btnGO.GetComponent<Image>().color = new Color(0, 0, 0, 0);

            // Number label
            var numGO = MakeTxt(btnGO.transform, "Num", numLabels[i],
                0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 255, 35, 0, -4,
                Color.white, 22, true, mapleFont);

            // Option text
            var txtGO = MakeTxt(btnGO.transform, "OptionText", "",
                0, 0, 1, 0.72f, 0.5f, 0.5f, -14, -10, 0, 0,
                Color.white, 22, false, mapleFont);
            txtGO.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            var btn = btnGO.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = cb;
            btn.targetGraphic = btnGO.GetComponent<Image>();

            optBtns[i] = btn;
            optTxts[i] = txtGO.GetComponent<Text>();
        }

        // Click catcher (transparent full-screen button for advancing state)
        var clickCatcher = MakeImg(ct, "ClickCatcher", 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);
        clickCatcher.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        clickCatcher.AddComponent<Button>().transition = Selectable.Transition.None;

        // Manager object
        var mgrGO = new GameObject("QuizGameManager");
        var mgr = mgrGO.AddComponent<QuizGameManager>();
        mgr.normalSprite = normalSprite;
        mgr.happySprite  = happySprite;
        mgr.sadSprite    = sadSprite;
        mgr.lucidImage   = lucidGO.GetComponent<Image>();
        mgr.questionText = qTextGO.GetComponent<Text>();
        mgr.optionPanel  = optPanel;
        mgr.optionButtons = optBtns;
        mgr.optionTexts  = optTxts;

        // Wire button onClick events
        int captureIdx = 0;
        for (int i = 0; i < 4; i++)
        {
            int idx = i + 1; // 1-indexed
            optBtns[i].onClick.AddListener(() => mgr.OnOptionSelected(idx));
        }
        // Wire click catcher
        clickCatcher.GetComponent<Button>().onClick.AddListener(() => mgr.OnScreenClick());

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("QuizScene built successfully!");
    }

    static GameObject MakeImg(Transform par, string nm,
        float ax0, float ay0, float ax1, float ay1, float px, float py,
        float sw, float sh, float ox, float oy)
    {
        var go = new GameObject(nm);
        go.transform.SetParent(par, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax0, ay0);
        rt.anchorMax = new Vector2(ax1, ay1);
        rt.pivot     = new Vector2(px, py);
        rt.sizeDelta = new Vector2(sw, sh);
        rt.anchoredPosition = new Vector2(ox, oy);
        go.AddComponent<Image>().color = Color.white;
        return go;
    }

    static GameObject MakeTxt(Transform par, string nm, string text,
        float ax0, float ay0, float ax1, float ay1, float px, float py,
        float sw, float sh, float ox, float oy,
        Color col, int fs, bool bold, Font font)
    {
        var go = new GameObject(nm);
        go.transform.SetParent(par, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax0, ay0);
        rt.anchorMax = new Vector2(ax1, ay1);
        rt.pivot     = new Vector2(px, py);
        rt.sizeDelta = new Vector2(sw, sh);
        rt.anchoredPosition = new Vector2(ox, oy);
        var t = go.AddComponent<Text>();
        t.text      = text;
        t.font      = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize  = fs;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.color     = col;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var sh2 = go.AddComponent<Shadow>();
        sh2.effectColor    = new Color(0, 0, 0, 0.6f);
        sh2.effectDistance = new Vector2(2, -2);
        return go;
    }
}
