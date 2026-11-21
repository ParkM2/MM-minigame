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

    void WireButtons()
    {
        Wire("Game1Card", () => SceneManager.LoadScene("SiaSpotDifferenceScene"));
        Wire("Game2Card", () => SceneManager.LoadScene("GameScene2"));
        Wire("ExitBtn", QuitGame);
    }

    // 이전 씬에서 누른 채로 넘어온 클릭으로 버튼이 눌리지 않도록, 이 화면에서 새로 누른 클릭만 인정
    void Wire(string objectName, System.Action action)
    {
        var button = GameObject.Find(objectName)?.GetComponent<Button>();
        if (button == null) return;
        button.onClick.AddListener(() =>
        {
            if (ClickGuard.PressedAfter(sceneStartFrame)) action();
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
