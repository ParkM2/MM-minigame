# 메이플스토리M 미니게임 기획·프로토타입

메이플스토리M 캐릭터(시아, 루시드)를 주제로 미니게임 2종을 기획하고 Unity로 만든 플레이 가능한 프로토타입입니다.

## 게임 소개

### 시아와 틀린그림찾기
- 모두 3라운드입니다. 라운드마다 왼쪽·오른쪽 그림을 비교해 왼쪽 그림에서 다른 곳을 클릭합니다.
- 찾아야 하는 곳은 1라운드 4곳, 2·3라운드는 3곳입니다.
- 라운드마다 제한 시간은 60초이고, 틀린 곳을 클릭하면 2초가 줄어듭니다(클릭 위치에 `-2초` 표시).
- 찾은 곳은 빨간 원으로 표시되고, 시간이 다 되면 못 찾은 곳을 노란 원으로 보여 줍니다.
- 마지막에 몇 라운드를 성공했는지 보여 줍니다.

### 루시드의 퀴즈퀴즈
- 화면을 클릭하면 주제 돌림판이 돕니다. 4개 주제가 같은 넓이의 칸으로 나뉘어 있고, 어느 주제가 나올 확률도 같습니다.
- 나온 주제에서 3문제를 무작위로 냅니다.
- 보기를 고르면 정답은 초록색, 내가 고른 오답은 빨간색으로 표시되고, 틀리면 정답을 바로 알려 줍니다.
- 결과에 따라 루시드의 표정(기본·기쁨·슬픔)이 바뀝니다.

### 공통 기능
- ESC 일시정지 팝업(계속하기 / 초기 화면으로)
- 앞 화면에서 누른 클릭이 다음 화면에서 잘못 처리되지 않도록 막는 클릭 가드(`ClickGuard`)
- 게임별 배경음악. 씬이 바뀌면 소리가 서서히 바뀌고, 일시정지 중에는 작게 들립니다.

## 실행 방법
- **바로 해보기:** [Releases](../../releases)에서 `mapleM_minigame_windows.zip`을 받아 압축을 풀고 `mapleM mini game.exe`를 실행합니다. (Windows)
- **프로젝트 열기:** Unity **6000.4.3f1**로 이 폴더를 엽니다. 시작 씬은 `Assets/Scenes/StartScene.unity`입니다.

## 퀴즈 문제 바꾸기 (CSV)
문제는 `Assets/StreamingAssets/quiz_data.csv`에서 읽습니다. 빌드한 뒤에도 `mapleM mini game_Data/StreamingAssets/quiz_data.csv`만 고치면 다시 빌드하지 않아도 반영됩니다.

```
주제,문제,보기1,보기2,보기3,보기4,정답,해설
보스,아카이럼 난이도 중 없는 난이도는?,이지,노말,하드,카오스,1,
```

- 파일은 UTF-8로 저장합니다(엑셀에서는 'CSV UTF-8').
- 정답은 1~4 중 하나의 숫자입니다. 해설은 비워 둘 수 있습니다.
- 칸 안에 쉼표가 있으면 큰따옴표로 감싸고, 문제 안에서 줄을 바꾸려면 `\n`을 적습니다.
- `#`으로 시작하는 줄과 빈 줄은 무시합니다.
- 돌림판에는 CSV에 처음 나온 순서대로 앞의 4개 주제가 들어갑니다.

## 폴더 구조
```
Assets/
  Scripts/
    StartScreenManager.cs        시작 화면 카드·종료 버튼
    SpotDifferenceGameManager.cs 틀린그림찾기 진행(라운드, 타이머, 정답 판정)
    QuizGameManager.cs           퀴즈 진행(돌림판 → 문제 → 결과)
    TopicWheel.cs                주제 돌림판(같은 넓이의 칸, 균등 확률)
    QuizData.cs                  quiz_data.csv 읽기와 검사
    PauseMenu.cs                 ESC 일시정지 팝업
    BgmPlayer.cs                 씬별 배경음악
    ClickGuard.cs                화면 전환 시 클릭이 넘어가는 문제 방지
  Editor/                        Windows 빌드 스크립트, 씬 생성 도구
  Scenes/                        StartScene, SiaSpotDifferenceScene, GameScene2(퀴즈)
  StreamingAssets/quiz_data.csv  퀴즈 문제
  Resources/Audio/               배경음악
Tools/synth_bgm.py               배경음악 생성 스크립트
```

## 기획 문서
- [시아와 틀린그림찾기 기획서](sia_spot_the_difference_plan.md)
- [루시드의 퀴즈퀴즈 기획서](lucid_quiz_game_plan.md)
- [퀴즈 초안](퀴즈.md)

## AI 활용
기획서를 바탕으로 Claude(AI)와 함께 구현 → 플레이테스트 → 피드백 반영을 반복하며 만들었습니다. 코드 정리는 Anthropic의 공식 code-simplifier를 Unity C#에 맞게 고친 스킬(`.claude/skills/unity-code-simplifier`)을 사용했습니다.

## 에셋 안내
- 메이플스토리 관련 이미지, 캐릭터, 폰트 등의 지식재산권은 NEXON에 있으며, 이 저장소는 비상업적 포트폴리오 용도로만 사용합니다.
- 배경음악은 `Tools/synth_bgm.py`로 직접 만든 오리지널 곡입니다.
