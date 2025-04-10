using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère le menu de fin de jeu et le retour à la sélection des personnages
/// </summary>
public class EndGameMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    [Tooltip("Référence au Canvas de sélection des personnages")]
    private GameObject selectionCanvas;

    [Header("Player References")]
    [SerializeField]
    [Tooltip("Référence au script SelectPlayer pour le reset")]
    private SelectPlayer selectPlayerScript;

    [Header("UI Elements")]
    [SerializeField]
    [Tooltip("Bouton pour revenir à la sélection")]
    private Button restartButton;
    
    [SerializeField]
    [Tooltip("Bouton pour quitter le jeu")]
    private Button quitButton;

    /// <summary>
    /// Initialisation au démarrage
    /// </summary>
    private void Start()
    {
        // Configuration du curseur
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Vérification des références
        if (restartButton == null || quitButton == null || selectionCanvas == null || selectPlayerScript == null)
        {
            Debug.LogError("Toutes les références doivent être assignées dans l'inspecteur");
            return;
        }

        // Configuration des boutons
        restartButton.onClick.AddListener(ReturnToSelection);
        quitButton.onClick.AddListener(QuitGame);
    }

    /// <summary>
    /// Retourne au menu de sélection des personnages
    /// </summary>
    private void ReturnToSelection()
    {
        // Réinitialise le jeu
        ResetGame();

        // Active le canvas de sélection
        selectionCanvas.SetActive(true);

        // Désactive ce menu
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Réinitialise complètement l'état du jeu
    /// </summary>

    
    public void ResetGame()
    {
        // Réinitialise le temps
        Time.timeScale = 1f;

        // Réinitialise la sélection des joueurs
        if (selectPlayerScript != null)
        {
            selectPlayerScript.ResetSelection();
        }

        // Note: Ajoutez ici toute autre réinitialisation nécessaire
    }

    /// <summary>
    /// Quitte l'application
    /// </summary>
    private void QuitGame()
    {
        // Réinitialise le temps
        Time.timeScale = 1f;

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Active/désactive ce menu
    /// </summary>
    public void SetMenuActive(bool state)
    {
        gameObject.SetActive(state);
        Time.timeScale = state ? 0f : 1f;
    }
}