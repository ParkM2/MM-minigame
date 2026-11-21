using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class StartSceneBuilder
{
    [MenuItem("Tools/Build Start Screen UI")]
    public static void BuildUI()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

        // Remove old Canvas/EventSystem
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Canvas" || root.name == "EventSystem")
                Object.DestroyImmediate(root);
        }

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

        // EventSystem
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();

        // Backgrounds
        MakeImg(ct, "Sky", 0f, 0f, 1f, 1f, 0.5f, 0.5f, 0, 0, 0, 0, 0.53f, 0.81f, 0.92f, 1f);
        MakeImg(ct, "Ground", 0f, 0f, 1f, 0.42f, 0.5f, 0f, 0, 0, 0, 0, 0.25f, 0.55f, 0.2f, 1f);

        // NEXON logo
        MakeTxt(ct, "Nexon", "NEXON", 0f, 1f, 0f, 1f, 0f, 1f, 140, 48, 80, -28, 1f, 1f, 1f, 1f, 28, true);

        // Title BG
        var tbg = MakeImg(ct, "TitleBg", 0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 600, 82, 0, -58, 0.48f, 0.62f, 0.82f, 0.93f);
        AddOutline(tbg, 0.3f, 0.5f, 0.8f, 1f, 3f);

        // Title text
        MakeTxt(ct, "TitleTxt", "메이플스토리M 미니게임 기획", 0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 600, 82, 0, -58, 1f, 0.95f, 0f, 1f, 34, true);

        // Game1 card
        var g1 = MakeImg(ct, "Game1Card", 0.27f, 0.5f, 0.27f, 0.5f, 0.5f, 0.5f, 330, 285, 0, 18, 0.4f, 0.16f, 0.66f, 1f);
        AddOutline(g1, 0.72f, 0.42f, 1f, 1f, 5f);
        MakeImg(g1.transform, "Img1", 0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 295, 210, 0, -20, 0.08f, 0.02f, 0.28f, 1f);
        MakeTxt(g1.transform, "Stars1", "★  ★  ★", 0.5f, 0f, 0.5f, 0f, 0.5f, 0f, 310, 32, 0, 58, 1f, 0.82f, 0.1f, 1f, 16, false);
        MakeTxt(g1.transform, "Lbl1", "시아와 틀린그림찾기", 0.5f, 0f, 0.5f, 0f, 0.5f, 0f, 320, 52, 0, 20, 1f, 1f, 1f, 1f, 24, true);
        AddButton(g1, 0.55f, 0.28f, 0.85f, 1f, 0.28f, 0.08f, 0.48f, 1f);

        // Game2 card
        var g2 = MakeImg(ct, "Game2Card", 0.73f, 0.5f, 0.73f, 0.5f, 0.5f, 0.5f, 330, 285, 0, 18, 0.4f, 0.16f, 0.66f, 1f);
        AddOutline(g2, 0.72f, 0.42f, 1f, 1f, 5f);
        MakeImg(g2.transform, "Img2", 0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 295, 210, 0, -20, 0.08f, 0.02f, 0.28f, 1f);
        MakeTxt(g2.transform, "Stars2", "★  ★  ★", 0.5f, 0f, 0.5f, 0f, 0.5f, 0f, 310, 32, 0, 58, 1f, 0.82f, 0.1f, 1f, 16, false);
        MakeTxt(g2.transform, "Lbl2", "루시드의 퀴즈퀴즈", 0.5f, 0f, 0.5f, 0f, 0.5f, 0f, 320, 52, 0, 20, 1f, 1f, 1f, 1f, 24, true);
        AddButton(g2, 0.55f, 0.28f, 0.85f, 1f, 0.28f, 0.08f, 0.48f, 1f);

        // Exit button
        var ex = MakeImg(ct, "ExitBtn", 1f, 0f, 1f, 0f, 1f, 0f, 130, 62, -73, 38, 0.92f, 0.92f, 0.88f, 1f);
        AddOutline(ex, 0.5f, 0.5f, 0.5f, 1f, 2f);
        MakeTxt(ex.transform, "ExLbl", "종료", 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 130, 62, 0, 0, 0.1f, 0.1f, 0.1f, 1f, 28, true);
        AddButton(ex, 0.75f, 0.75f, 0.75f, 1f, 0.6f, 0.6f, 0.6f, 1f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("StartScreen UI built! Canvas children: " + ct.childCount);
    }

    static GameObject MakeImg(Transform par, string nm, float ax0, float ay0, float ax1, float ay1, float px, float py, float sw, float sh, float ox, float oy, float r, float g, float b, float a)
    {
        var go = new GameObject(nm);
        go.transform.SetParent(par, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax0, ay0);
        rt.anchorMax = new Vector2(ax1, ay1);
        rt.pivot = new Vector2(px, py);
        rt.sizeDelta = new Vector2(sw, sh);
        rt.anchoredPosition = new Vector2(ox, oy);
        go.AddComponent<Image>().color = new Color(r, g, b, a);
        return go;
    }

    static GameObject MakeTxt(Transform par, string nm, string text, float ax0, float ay0, float ax1, float ay1, float px, float py, float sw, float sh, float ox, float oy, float r, float g, float b, float a, int fs, bool bold)
    {
        var go = new GameObject(nm);
        go.transform.SetParent(par, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax0, ay0);
        rt.anchorMax = new Vector2(ax1, ay1);
        rt.pivot = new Vector2(px, py);
        rt.sizeDelta = new Vector2(sw, sh);
        rt.anchoredPosition = new Vector2(ox, oy);
        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fs;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.color = new Color(r, g, b, a);
        t.alignment = TextAnchor.MiddleCenter;
        var sh2 = go.AddComponent<Shadow>();
        sh2.effectColor = new Color(0, 0, 0, 0.7f);
        sh2.effectDistance = new Vector2(2, -2);
        return go;
    }

    static void AddOutline(GameObject go, float r, float g, float b, float a, float dist)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor = new Color(r, g, b, a);
        o.effectDistance = new Vector2(dist, dist);
    }

    static void AddButton(GameObject go, float hr, float hg, float hb, float ha, float pr, float pg, float pb, float pa)
    {
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.highlightedColor = new Color(hr, hg, hb, ha);
        cb.pressedColor = new Color(pr, pg, pb, pa);
        btn.colors = cb;
    }
}
