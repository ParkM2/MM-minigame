---
name: unity-code-simplifier
description: Unity C# 스크립트를 동작은 그대로 두고 더 읽기 쉽고 짧게 정리할 때 사용. "코드 간소화", "정리", "리팩터링" 요청에 사용.
---

# Unity 코드 간소화 (Anthropic 공식 code-simplifier 기반, Unity C#용으로 조정)

출처: anthropics/claude-plugins-official/plugins/code-simplifier

## 원칙
1. **동작 보존**: 무엇을 하는지는 절대 바꾸지 않는다. 화면 문구, 타이밍, 수치, Inspector에 보이는 [SerializeField]/public 필드 이름과 기본값, 씬·프리팹이 참조하는 이름(클래스, 메서드, 필드)은 그대로 둔다. 이름을 바꾸면 씬 연결이 끊긴다.
2. **명확성 우선**: 줄 수보다 읽기 쉬움. 중첩 삼항 연산자 금지, 지나치게 압축한 한 줄 코드 금지.
3. **줄일 것**
   - 중복 코드 → 작은 private 메서드로 묶기
   - 깊은 if 중첩 → early return
   - 쓰이지 않는 필드·메서드·using·변수, 주석 처리된 옛 코드
   - 코드를 그대로 읽어 주는 뻔한 주석 (이유를 설명하는 주석은 유지)
   - 매 프레임 GetComponent/Find 반복 → 캐시
4. **과한 간소화 금지**: 여러 책임을 한 메서드에 몰지 않는다. 도움이 되는 구조(ClickGuard, 상태 enum 등)는 유지.
5. **Unity 주의**: Update/Start/Awake 등 메시지 메서드 이름, [RuntimeInitializeOnLoadMethod], DefaultExecutionOrder, Coroutine 흐름, Time.timeScale/unscaled 시간 사용은 그대로.

## 절차
1. 대상 스크립트 목록과 각 파일 줄 수를 적는다.
2. 파일별로 위 원칙에 맞춰 정리한다.
3. 컴파일(에러 0) → 빌드 → 실제 플레이로 기능 확인.
4. 파일별 줄 수 전/후와 바꾼 내용의 요점을 보고한다.
