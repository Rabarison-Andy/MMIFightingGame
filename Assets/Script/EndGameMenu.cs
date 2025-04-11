using UnityEngine;
using UnityEngine.UI;

public class EndGameMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject selectionCanvas;

    [SerializeField]
    private SelectPlayer selectPlayerScript;

    [SerializeField]
    private Button restartButton;

    private Button quitButton;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (restartButton == null || quitButton == null || selectionCanvas == null || selectPlayerScript == null)
        {
            Debug.LogError("Toutes les références doivent être assignées dans l'inspecteur");
            return;
        }

        restartButton.onClick.AddListener(ReturnToSelection);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void ReturnToSelection()
    {
        ResetGame();
        selectionCanvas.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;

        if (selectPlayerScript != null)
        {
            selectPlayerScript.ResetSelection();
        }
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void SetMenuActive(bool state)
    {
        gameObject.SetActive(state);
        Time.timeScale = state ? 0f : 1f;
    }
}