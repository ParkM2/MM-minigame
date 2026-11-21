using UnityEngine;
using UnityEngine.InputSystem;

// 화면/씬이 바뀌기 직전에 시작된 클릭이 다음 화면에서 처리되는 문제를 막는 공용 가드.
// - 화면 넘김은 "누른 순간"이 아니라 "손을 뗀 순간"에 처리하고,
// - 그 클릭이 현재 화면(상태)이 시작된 뒤에 눌린 것일 때만 인정한다.
public static class ClickGuard
{
    static int lastPressFrame = -1;
    static int blockUntilFrame = -1; // 이 프레임 이하에서 시작된 클릭은 무시 (일시정지 열고 닫을 때 사용)

    static bool PressedThisFrame => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

    // 매 프레임 호출해서 마지막으로 마우스를 누른 프레임을 기록
    public static void Track()
    {
        if (PressedThisFrame) lastPressFrame = Time.frameCount;
    }

    // 마지막 클릭(누름)이 sinceFrame 이후에 시작됐는지. 버튼 onClick(손 뗄 때 호출) 검사용
    public static bool PressedAfter(int sinceFrame)
    {
        int pf = PressedThisFrame ? Time.frameCount : lastPressFrame;
        return pf > sinceFrame && pf > blockUntilFrame;
    }

    // 화면 클릭 진행용: 이번 프레임에 손을 뗐고, 누름이 sinceFrame 이후에 시작됐는지
    public static bool ClickedAfter(int sinceFrame)
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasReleasedThisFrame) return false;
        return PressedAfter(sinceFrame);
    }

    // 지금까지 눌린 클릭을 모두 무효화
    public static void BlockCurrentPress()
    {
        blockUntilFrame = Time.frameCount;
    }
}

// 씬과 무관하게 매 프레임 클릭을 기록하는 보이지 않는 오브젝트 (자동 생성)
[DefaultExecutionOrder(-1000)]
public class ClickGuardRunner : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        var go = new GameObject("[ClickGuard]");
        DontDestroyOnLoad(go);
        go.AddComponent<ClickGuardRunner>();
    }

    void Update() { ClickGuard.Track(); }
}
