using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuizGameManager : MonoBehaviour
{
    [Header("Lucid Sprites")]
    public Sprite normalSprite;
    public Sprite happySprite;
    public Sprite sadSprite;

    [Header("UI References")]
    public Image lucidImage;
    public Text questionText;
    public GameObject optionPanel;
    public Button[] optionButtons;
    public Text[] optionTexts;

    [Header("Rule Values")]
    public int questionsPerGame = 3;    // 한 판에 출제할 문제 수 (주제 문제가 더 적으면 전부)
    public int wheelTopicCount = 4;     // 돌림판 칸 수 (CSV 주제 순서대로 앞에서부터)
    public Vector2 wheelPosition = new Vector2(-80f, -90f);
    public float wheelSize = 320f;

    // 문제 데이터는 StreamingAssets/quiz_data.csv 에서 읽는다 (QuizData.cs 참고)
    private List<QuizQuestion> questions = new List<QuizQuestion>();
    private List<string> wheelTopics;
    private string selectedTopic;
    private TopicWheel wheel;

    // State
    private enum GameState { Intro1, WheelReady, Spinning, WheelResult, Intro2, Question, Answered, Result, End, DataError }
    private GameState state = GameState.Intro1;
    private int stateEnterFrame;     // 현재 상태가 시작된 프레임 (이전 클릭이 넘어오지 않게 하는 기준)
    private int currentQuestion = 0;
    private int correctCount = 0;
    private bool isAnswered = false;

    // 보기 피드백 색상 (정답 공개 / 내가 고른 오답 / 나머지 흐리게)
    private static readonly Color CorrectColor = new Color(0.55f, 1f, 0.45f, 1f);
    private static readonly Color WrongColor = new Color(1f, 0.45f, 0.45f, 1f);
    private static readonly Color DimColor = new Color(1f, 1f, 1f, 0.35f);
    private Color[] defaultOptionColors;
    private FontStyle[] defaultOptionStyles;

    void SetState(GameState next)
    {
        state = next;
        stateEnterFrame = Time.frameCount;
    }

    void Start()
    {
        // 버튼 onClick 런타임 연결
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int idx = i + 1;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(idx));
        }

        // 버튼 배경 투명 (transition None으로 Button이 색상을 덮어쓰지 않도록)
        foreach (var btn in optionButtons)
        {
            btn.transition = Selectable.Transition.None;
            btn.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }

        // 보기 텍스트 기본 색/스타일 저장 (문제마다 복원)
        defaultOptionColors = new Color[optionTexts.Length];
        defaultOptionStyles = new FontStyle[optionTexts.Length];
        for (int i = 0; i < optionTexts.Length; i++)
        {
            defaultOptionColors[i] = optionTexts[i].color;
            defaultOptionStyles[i] = optionTexts[i].fontStyle;
        }

        optionPanel.SetActive(false);
        lucidImage.sprite = normalSprite;

        // 문제 데이터 읽기
        if (!QuizData.Load())
        {
            string err = QuizData.Errors.Count > 0 ? QuizData.Errors[0] : "문제가 하나도 없습니다";
            questionText.text = "\n퀴즈 데이터를 읽지 못했어요\n" + err;
            SetState(GameState.DataError);
            return;
        }
        wheelTopics = QuizData.Topics.GetRange(0, Mathf.Min(wheelTopicCount, QuizData.Topics.Count));

        questionText.text = "\n루시드의 퀴즈퀴즈에\n오신걸 환영합니다";
        SetState(GameState.Intro1);
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;
        if (state == GameState.Question || state == GameState.Spinning) return;

        // 손을 뗀 순간 + 이 화면이 시작된 뒤에 누른 클릭만 화면 넘김으로 인정
        if (ClickGuard.ClickedAfter(stateEnterFrame))
            OnScreenClick();
    }

    public void OnScreenClick()
    {
        switch (state)
        {
            case GameState.Intro1:
                ShowWheel();
                break;

            case GameState.WheelReady:
                questionText.text = "\n두근두근...";
                SetState(GameState.Spinning);
                wheel.Spin(OnWheelStopped);
                break;

            case GameState.WheelResult:
                Destroy(wheel.gameObject);
                questionText.text = "\n그럼 첫번째 문제입니다.";
                SetState(GameState.Intro2);
                break;

            case GameState.Intro2:
                ShowQuestion();
                break;

            case GameState.Answered:
                lucidImage.sprite = normalSprite;
                currentQuestion++;
                if (currentQuestion >= questions.Count)
                    ShowResult();
                else
                    ShowQuestion();
                break;

            case GameState.Result:
                questionText.text = "\n수고하셨습니다.";
                optionPanel.SetActive(false);
                SetState(GameState.End);
                break;

            case GameState.End:
            case GameState.DataError:
                SceneManager.LoadScene("StartScene");
                break;
        }
    }

    // ---------- 주제 돌림판 ----------
    void ShowWheel()
    {
        wheel = TopicWheel.Create(questionText.canvas, questionText.font, wheelTopics, wheelPosition, wheelSize);
        questionText.text = "\n화면을 클릭해서\n돌림판을 돌려주세요!";
        SetState(GameState.WheelReady);
    }

    void OnWheelStopped(int index)
    {
        selectedTopic = wheelTopics[index];
        questions = QuizData.PickQuestions(selectedTopic, questionsPerGame);
        currentQuestion = 0;
        correctCount = 0;
        lucidImage.sprite = happySprite;
        questionText.text = $"\n'{selectedTopic}' 주제가 나왔어요!";
        SetState(GameState.WheelResult);
    }

    // ---------- 문제 ----------
    void ShowQuestion()
    {
        isAnswered = false;
        var q = questions[currentQuestion];
        questionText.text = "\n" + q.question;
        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].text = (i + 1) + ". " + q.options[i];
            optionTexts[i].color = defaultOptionColors[i];
            optionTexts[i].fontStyle = defaultOptionStyles[i];
        }
        optionPanel.SetActive(true);
        foreach (var btn in optionButtons)
            btn.interactable = true;
        lucidImage.sprite = normalSprite;
        SetState(GameState.Question);
    }

    public void OnOptionSelected(int optionIndex) // 1-indexed
    {
        if (state != GameState.Question || isAnswered || PauseMenu.IsPaused) return;
        // 문제가 뜨기 전에 누른 클릭(이전 화면 넘김 클릭)으로 보기가 선택되지 않게
        if (!ClickGuard.PressedAfter(stateEnterFrame)) return;
        isAnswered = true;

        foreach (var btn in optionButtons)
            btn.interactable = false;

        var q = questions[currentQuestion];
        ShowOptionFeedback(optionIndex, q.answer);
        bool hasExp = !string.IsNullOrEmpty(q.explanation);

        if (optionIndex == q.answer)
        {
            correctCount++;
            lucidImage.sprite = happySprite;
            questionText.text = hasExp ? "\n맞혔습니다!\n" + q.explanation : "\n맞혔습니다!";
        }
        else
        {
            // 오답 시 정답을 바로 알려줘서 '왜 틀렸는지' 알 수 있게 함
            lucidImage.sprite = sadSprite;
            string ans = "정답: " + q.answer + "번 " + q.options[q.answer - 1];
            questionText.text = hasExp ? "\n틀렸습니다 (" + ans + ")\n" + q.explanation : "\n틀렸습니다\n" + ans;
        }
        SetState(GameState.Answered);
    }

    // 정답 보기는 초록, 내가 고른 오답은 빨강, 나머지는 흐리게
    void ShowOptionFeedback(int selected, int answer) // 1-indexed
    {
        for (int i = 0; i < optionTexts.Length; i++)
        {
            int idx = i + 1;
            if (idx == answer)
            {
                optionTexts[i].color = CorrectColor;
                optionTexts[i].fontStyle = FontStyle.Bold;
            }
            else if (idx == selected)
            {
                optionTexts[i].color = WrongColor;
                optionTexts[i].fontStyle = FontStyle.Bold;
            }
            else
            {
                optionTexts[i].color = DimColor;
            }
        }
    }

    void ShowResult()
    {
        optionPanel.SetActive(false);
        questionText.text = $"\n'{selectedTopic}' {questions.Count}문제 중\n{correctCount}개 맞히셨습니다";
        lucidImage.sprite = correctCount == questions.Count ? happySprite : normalSprite;
        SetState(GameState.Result);
    }
}
