using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    private int sceneStartFrame;

    void Start()
    {
        sceneStartFrame = Time.frameCount;
        WireButtons();
    }

    // 이전 씬에서 누른 채로 넘어온 클릭으로 카드가 눌리지 않도록, 이 화면에서 새로 누른 클릭만 인정
    void OnCard(string sceneName)
    {
        if (!ClickGuard.PressedAfter(sceneStartFrame)) return;
        SceneManager.LoadScene(sceneName);
    }

    void WireButtons()
    {
        var g1 = GameObject.Find("Game1Card");
        if (g1 != null)
            g1.GetComponent<Button>()?.onClick.AddListener(() => OnCard("SiaSpotDifferenceScene"));

        var g2 = GameObject.Find("Game2Card");
        if (g2 != null)
            g2.GetComponent<Button>()?.onClick.AddListener(() => OnCard("GameScene2"));

        var ex = GameObject.Find("ExitBtn");
        if (ex != null)
            ex.GetComponent<Button>()?.onClick.AddListener(() =>
            {
                if (ClickGuard.PressedAfter(sceneStartFrame)) QuitGame();
            });
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
