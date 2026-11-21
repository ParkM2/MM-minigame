using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[System.Serializable]
public class AnswerArea
{
    public Vector2 anchoredPosition;
    public Vector2 size = new Vector2(80f, 80f);
}

[System.Serializable]
public class StageData
{
    public AnswerArea[] areas = new AnswerArea[3];
    public int requiredPoints = 3; // 클리어에 필요한 정답 수
}

public class SpotDifferenceGameManager : MonoBehaviour
{
    [Header("Sia Sprites")]
    public Sprite normalSprite;
    public Sprite happySprite;
    public Sprite sadSprite;

    [Header("Stage Images")]
    public Sprite[] leftImages;   // 0~2: 1_l ~ 3_l
    public Sprite[] rightImages;  // 0~2: 1_r ~ 3_r

    [Header("UI References")]
    public Image siaImage;
    public Text dialogueText;
    public Text timerText;
    public Image leftImage;
    public Image rightImage;
    public GameObject gameBoard;
    public Button bgClickButton;      // 왼쪽 이미지 전체 커버 (오답 처리)
    public Button[] answerButtons;    // 정답 버튼 (최대 4개)

    [Header("Stage Answer Data")]
    public StageData[] stages = new StageData[3];

    // AnswerButton 앵커 (LeftImage 중심 기준 - 런타임 계산)
    private Vector2 _answerAnchor = new Vector2(0.1639f, 0.4597f);

    // Runtime state
    private enum GameState { Intro1, Intro2, Playing, RoundEnd, GameEnd }
    private GameState state = GameState.Intro1;
    private int stateEnterFrame;     // 현재 상태가 시작된 프레임 (이전 클릭이 넘어오지 않게 하는 기준)

    // 라운드 수 = 스테이지 데이터 수와 스테이지 이미지 수 중 작은 값 (데이터만 늘리면 라운드가 늘어남)
    private int TotalStages => Mathf.Min(stages.Length, leftImages.Length, rightImages.Length);

    [Header("Rule Values")]
    public float stageTime = 60f;       // 스테이지 제한 시간
    public float wrongPenalty = 2f;     // 오답 클릭 시 차감 시간
    public float warningTime = 10f;     // 이 시간 이하면 타이머 빨간색 경고

    // 피드백 색상
    private static readonly Color FoundColor = new Color(1f, 0f, 0f, 0.85f);     // 찾은 곳: 빨간 원
    private static readonly Color MissedColor = new Color(1f, 0.85f, 0f, 0.95f); // 못 찾은 곳: 노란 원 (시간 초과 시 공개)
    private static readonly Color WarningColor = new Color(1f, 0.3f, 0.3f, 1f);
    private Color timerDefaultColor;
    private Coroutine timerFlashRoutine;

    private int currentStage = 0;
    private int foundPoints = 0;
    private int successCount = 0;
    private float timer = 60f;
    private bool[] answerFound = new bool[4];
    private Sprite _circleSprite;

    void SetState(GameState next)
    {
        state = next;
        stateEnterFrame = Time.frameCount;
    }

    // 배경을 투명하게 하고 색 전환 없이 클릭만 받는 버튼으로 설정
    static void SetupInvisibleButton(Button button, UnityEngine.Events.UnityAction onClick)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
        button.transition = Selectable.Transition.None;
        SetButtonLook(button, null, Color.clear);
    }

    static void SetButtonLook(Button button, Sprite sprite, Color color)
    {
        var img = button.GetComponent<Image>();
        if (img == null) return;
        img.sprite = sprite;
        img.color = color;
    }

    void Start()
    {
        // 배경 클릭 버튼 (오답)
        SetupInvisibleButton(bgClickButton, OnWrongClick);

        // 정답 버튼
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int idx = i;
            SetupInvisibleButton(answerButtons[i], () => OnAnswerClick(idx));
        }

        // LeftImage 중심 앵커 런타임 계산
        Vector3[] corners = new Vector3[4];
        leftImage.GetComponent<RectTransform>().GetWorldCorners(corners);
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            Vector3[] cCorners = new Vector3[4];
            rootCanvas.GetComponent<RectTransform>().GetWorldCorners(cCorners);
            float cW = cCorners[2].x - cCorners[0].x;
            float cH = cCorners[2].y - cCorners[0].y;
            float cx = ((corners[0].x + corners[2].x) * 0.5f) / cW;
            float cy = ((corners[0].y + corners[2].y) * 0.5f) / cH;
            _answerAnchor = new Vector2(cx, cy);
        }

        _circleSprite = GetCircleSprite();
        timerDefaultColor = timerText.color;
        // 타이머 옆 "찾은 곳" 표시가 잘리지 않도록
        timerText.horizontalOverflow = HorizontalWrapMode.Overflow;
        timerText.verticalOverflow = VerticalWrapMode.Overflow;

        gameBoard.SetActive(false);
        timerText.gameObject.SetActive(false);

        siaImage.sprite = normalSprite;
        dialogueText.text = "시아와 틀린그림찾기에\n오신걸 환영합니다";
        SetState(GameState.Intro1);
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;

        if (state == GameState.Playing)
        {
            timer = Mathf.Max(0f, timer - Time.deltaTime);
            UpdateTimerUI();

            if (timer <= 0f)
                OnTimerExpired();
            return;
        }

        // 손을 뗀 순간 + 이 화면이 시작된 뒤에 누른 클릭만 화면 넘김으로 인정
        if (ClickGuard.ClickedAfter(stateEnterFrame))
            OnScreenClick();
    }

    void OnScreenClick()
    {
        switch (state)
        {
            case GameState.Intro1:
                dialogueText.text = "그럼 첫번째 문제입니다.";
                SetState(GameState.Intro2);
                break;

            case GameState.Intro2:
                dialogueText.text = "서로 다른 부분을 왼쪽 그림에서 찾아보세요.";
                StartStage(0);
                break;

            case GameState.RoundEnd:
                currentStage++;
                if (currentStage >= TotalStages)
                    ShowGameEnd();
                else
                    StartNextStage();
                break;

            case GameState.GameEnd:
                SceneManager.LoadScene("StartScene");
                break;
        }
    }

    void StartStage(int stageIndex)
    {
        currentStage = stageIndex;
        foundPoints = 0;
        timer = stageTime;
        answerFound = new bool[answerButtons.Length];

        leftImage.sprite = leftImages[stageIndex];
        rightImage.sprite = rightImages[stageIndex];

        // 정답 버튼 위치/크기 적용 (스테이지별 필요한 수만 활성화)
        StageData data = stages[stageIndex];
        for (int i = 0; i < answerButtons.Length; i++)
        {
            bool used = i < data.areas.Length; // 이번 스테이지에서 쓰지 않는 버튼은 숨김
            answerButtons[i].gameObject.SetActive(used);
            if (!used) continue;

            RectTransform rt = answerButtons[i].GetComponent<RectTransform>();
            rt.anchorMin = _answerAnchor;
            rt.anchorMax = _answerAnchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = data.areas[i].anchoredPosition;
            rt.sizeDelta = data.areas[i].size;
            answerButtons[i].interactable = true;
            SetButtonLook(answerButtons[i], null, Color.clear);
        }

        bgClickButton.interactable = true;
        gameBoard.SetActive(true);
        timerText.gameObject.SetActive(true);

        siaImage.sprite = normalSprite;
        SetState(GameState.Playing);   // 스테이지 시작 프레임 기록 → 이전 클릭은 정답/오답으로 처리 안 됨
        UpdateTimerUI();
    }

    void StartNextStage()
    {
        siaImage.sprite = normalSprite;
        dialogueText.text = $"{currentStage + 1}번째 문제입니다.";
        StartStage(currentStage);
    }

    // 스테이지 시작 후 새로 누른 클릭인지 (문제 넘김 클릭이 -2초로 이어지는 문제 방지)
    bool IsValidBoardClick()
    {
        return state == GameState.Playing && !PauseMenu.IsPaused && ClickGuard.PressedAfter(stateEnterFrame);
    }

    void OnAnswerClick(int index)
    {
        if (!IsValidBoardClick() || answerFound[index]) return;

        answerFound[index] = true;
        foundPoints++;
        answerButtons[index].interactable = false;

        SetButtonLook(answerButtons[index], _circleSprite, FoundColor);
        UpdateTimerUI();

        if (foundPoints >= stages[currentStage].requiredPoints)
            OnRoundSuccess();
    }

    // 빈 원형(테두리만) 스프라이트 생성
    Sprite GetCircleSprite()
    {
        int size = 128;
        int stroke = 10;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];

        float center = size / 2f;
        float outerR = center - 2f;
        float innerR = outerR - stroke;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                bool inRing = dist <= outerR && dist >= innerR;
                pixels[y * size + x] = inRing ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void OnWrongClick()
    {
        if (!IsValidBoardClick()) return;
        timer = Mathf.Max(0f, timer - wrongPenalty);
        UpdateTimerUI();
        ShowPenaltyPopup();
        if (timerFlashRoutine != null) StopCoroutine(timerFlashRoutine);
        timerFlashRoutine = StartCoroutine(FlashTimer());
        if (timer <= 0f)
            OnTimerExpired();
    }

    void OnRoundSuccess()
    {
        int remain = Mathf.CeilToInt(timer);
        timer = 0f;
        successCount++;
        EndRound(happySprite, $"축하합니다!\n{remain}초 남기고 다 찾았어요");
    }

    void OnTimerExpired()
    {
        foreach (var btn in answerButtons)
            btn.interactable = false;
        RevealMissedAnswers();
        EndRound(sadSprite, "힘내세요!\n노란 원이 못 찾은 곳이에요");
    }

    void EndRound(Sprite face, string message)
    {
        timerText.gameObject.SetActive(false);
        bgClickButton.interactable = false;
        siaImage.sprite = face;
        dialogueText.text = message;
        SetState(GameState.RoundEnd);
    }

    void ShowGameEnd()
    {
        gameBoard.SetActive(false);
        timerText.gameObject.SetActive(false);
        siaImage.sprite = successCount == TotalStages ? happySprite : normalSprite;
        dialogueText.text = $"수고하셨어요.\n{TotalStages}문제 중 {successCount}번 성공하셨습니다";
        SetState(GameState.GameEnd);
    }

    void UpdateTimerUI()
    {
        if (currentStage < 0 || currentStage >= stages.Length) return;
        int required = stages[currentStage].requiredPoints;
        timerText.text = $"남은 시간: {Mathf.CeilToInt(timer)}초   찾은 곳: {foundPoints}/{required}";
        // 플래시 중이 아니면 잔여 시간에 따라 경고 색 적용
        if (timerFlashRoutine == null)
            timerText.color = timer <= warningTime ? WarningColor : timerDefaultColor;
    }

    // 시간 초과 시 못 찾은 정답 위치를 노란 원으로 공개 (정답 학습 + 납득감)
    void RevealMissedAnswers()
    {
        int count = Mathf.Min(stages[currentStage].areas.Length, answerButtons.Length);
        for (int i = 0; i < count; i++)
        {
            if (!answerFound[i]) SetButtonLook(answerButtons[i], _circleSprite, MissedColor);
        }
    }

    // 오답 클릭 위치에 "-2초" 팝업을 띄우고 위로 떠오르며 사라짐
    void ShowPenaltyPopup()
    {
        Canvas canvas = dialogueText.canvas;
        if (canvas == null || Mouse.current == null) return;

        var go = new GameObject("PenaltyPopup", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160f, 50f);

        RectTransform canvasRt = canvas.GetComponent<RectTransform>();
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, Mouse.current.position.ReadValue(), cam, out Vector2 local);
        rt.anchoredPosition = local;

        var txt = go.AddComponent<Text>();
        txt.font = timerText.font;
        txt.fontSize = 32;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = WarningColor;
        txt.raycastTarget = false; // 팝업이 클릭을 막지 않도록
        txt.text = $"-{wrongPenalty:0}초";
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);

        StartCoroutine(FloatAndFade(rt, txt));
    }

    IEnumerator FloatAndFade(RectTransform rt, Text txt)
    {
        const float duration = 0.7f;
        Vector2 start = rt.anchoredPosition;
        Color c = txt.color;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            rt.anchoredPosition = start + new Vector2(0f, 60f * k);
            txt.color = new Color(c.r, c.g, c.b, 1f - k);
            yield return null;
        }
        Destroy(rt.gameObject);
    }

    // 오답 시 타이머 텍스트를 잠깐 빨갛게 깜빡이고 살짝 키움
    IEnumerator FlashTimer()
    {
        var rt = timerText.rectTransform;
        timerText.color = WarningColor;
        rt.localScale = Vector3.one * 1.15f;
        yield return new WaitForSeconds(0.25f);
        rt.localScale = Vector3.one;
        timerFlashRoutine = null;
        UpdateTimerUI();
    }
}
