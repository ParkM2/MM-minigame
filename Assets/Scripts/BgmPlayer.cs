using UnityEngine;
using UnityEngine.SceneManagement;

// 씬별 배경음악 재생 (자동 생성, 씬 전환 시 페이드). 음원은 Assets/Resources/Audio/ 에 있음.
public class BgmPlayer : MonoBehaviour
{
    const float Volume = 0.5f;          // 기본 볼륨
    const float PausedVolumeRate = 0.4f; // 일시정지 중에는 작게
    const float FadeSpeed = 0.6f;        // 초당 볼륨 변화량

    static BgmPlayer instance;
    AudioSource source;
    float target;

    static string ClipFor(string sceneName)
    {
        switch (sceneName)
        {
            case "SiaSpotDifferenceScene": return "Audio/bgm_sia";   // 시아의 틀린그림찾기: 잔잔한 곡
            case "GameScene2": return "Audio/bgm_lucid";             // 루시드의 퀴즈퀴즈: 몽환적인 곡
            default: return null;                                    // 시작 화면 등: 음악 없음
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (instance != null) return;
        var go = new GameObject("[BgmPlayer]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<BgmPlayer>();
        instance.source = go.AddComponent<AudioSource>();
        instance.source.loop = true;
        instance.source.playOnAwake = false;
        instance.source.ignoreListenerPause = true;
        SceneManager.sceneLoaded += (s, m) => instance.PlayFor(s.name);
        instance.PlayFor(SceneManager.GetActiveScene().name);
    }

    void PlayFor(string sceneName)
    {
        string path = ClipFor(sceneName);
        if (path == null) { target = 0f; return; }

        var clip = Resources.Load<AudioClip>(path);
        if (clip == null) { Debug.LogWarning("BGM not found: " + path); target = 0f; return; }

        if (source.clip != clip)
        {
            source.clip = clip;
            source.volume = 0f;
            source.Play();
        }
        else if (!source.isPlaying)
        {
            source.Play();
        }
        target = Volume;
    }

    void Update()
    {
        float want = target * (PauseMenu.IsPaused ? PausedVolumeRate : 1f);
        source.volume = Mathf.MoveTowards(source.volume, want, FadeSpeed * Time.unscaledDeltaTime);
        if (target <= 0f && source.volume <= 0f && source.isPlaying) source.Stop();
    }
}
